using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HelperLibrary;
using HelperLibrary.Shared;
using Microsoft.EntityFrameworkCore;
using MongoLibrary.Interfaces;
using MongoLibrary.Models;
using Newtonsoft.Json;
using RoutinizeCore.DbContexts;
using RoutinizeCore.Models;
using RoutinizeCore.Services.Interfaces;
using RoutinizeCore.ViewModels.Organization;

namespace RoutinizeCore.Services.DatabaseServices {

    public class OrganizationService : DbServiceBase, IOrganizationService {

        public OrganizationService(
            IRoutinizeCoreLogService coreLogService,
            RoutinizeDbContext dbContext
        ) : base(coreLogService, dbContext) { }

        public new async Task SetChangesToDbContext(object any, string task = SharedConstants.TaskInsert) {
            await base.SetChangesToDbContext(any, task);
        }

        public new async Task<bool?> CommitChanges() {
            return await base.CommitChanges();
        }

        public new void ToggleTransactionAuto(bool auto = true) {
            base.ToggleTransactionAuto(auto);
        }

        public new async Task StartTransaction() {
            await base.StartTransaction();
        }

        public new async Task CommitTransaction() {
            await base.CommitTransaction();
        }

        public new async Task RevertTransaction() {
            await base.RevertTransaction();
        }

        public new async Task ExecuteRawOn<T>(string query) {
            await base.ExecuteRawOn<T>(query);
        }

        public async Task<IndustryVM[]> GetIndustryList() {
            try {
                var parentIndustries = await _dbContext.Industries
                                                       .Where(industry => !industry.ParentId.HasValue)
                                                       .OrderBy(industry => industry.Name)
                                                       .Select(industry => (IndustryVM) industry)
                                                       .ToArrayAsync();

                var childIndustries = _dbContext.Industries.Where(industry => industry.ParentId.HasValue);
                await childIndustries.ForEachAsync(industry => {
                    Array.ForEach(parentIndustries, parentIndustry => {
                        if (parentIndustry.Id == industry.ParentId)
                            parentIndustry.SubIndustries.Add(industry);
                    });
                });

                return parentIndustries;
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(GetIndustryList) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while getting Industries with Where-ToArray.\n\n{ e.StackTrace }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<SearchOrganizationVM> SearchForMotherOrganizations(OrganizationSearchDataVM searchData) {
            try {
                var organizationsWithMatchLevel = await PerformSearchVariations(searchData);

                var keywordTokens = searchData.Keyword.Split(SharedConstants.MonoSpace);
                Array.ForEach(keywordTokens, async token => {
                    searchData.Keyword = token;
                    var tokenSearchResultsWithMatchLevel = await PerformSearchVariations(searchData);
                    if (tokenSearchResultsWithMatchLevel == null) return;
                    
                    organizationsWithMatchLevel = organizationsWithMatchLevel.Union(tokenSearchResultsWithMatchLevel).ToList();
                });

                var closeMatches = organizationsWithMatchLevel
                                   .Where(pair => pair.Value)
                                   .Select(pair => (SearchOrganizationVM.OrganizationSearchResultVM) pair.Key)
                                   .ToArray();
                
                var otherMatches = organizationsWithMatchLevel
                                   .Where(pair => !pair.Value)
                                   .Select(pair => (SearchOrganizationVM.OrganizationSearchResultVM) pair.Key)
                                   .ToArray();

                return new SearchOrganizationVM {
                    ClosestMatches = closeMatches,
                    OtherMatches = otherMatches
                };
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(OrganizationService) }.{ nameof(SearchForMotherOrganizations) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while merging Organization search results with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(searchData) } = { JsonConvert.SerializeObject(searchData) }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
        }

        private async Task<List<KeyValuePair<Organization, bool>>> PerformSearchVariations(OrganizationSearchDataVM searchData) {
            try {
                var organizationsWithMatchLevel = new List<KeyValuePair<Organization, bool>>();
            
                void DuplicateFilter(Organization organization, bool isCloseMatch) {
                    var duplicatedOrganization = organizationsWithMatchLevel.SingleOrDefault(pair => pair.Key.Id == organization.Id);
                    if (duplicatedOrganization.Value) return;

                    if (duplicatedOrganization.Key == null) {
                        organizationsWithMatchLevel.Add(new KeyValuePair<Organization, bool>(organization, isCloseMatch));
                        return;
                    }

                    if (!isCloseMatch) return;
                    organizationsWithMatchLevel.Remove(duplicatedOrganization);
                    organizationsWithMatchLevel.Add(new KeyValuePair<Organization, bool>(organization, true));
                }
                
                if (searchData.ByOrganizationName) {
                    var byNameResults = await SearchOrganizationByNameWith(searchData.Keyword);
                    await byNameResults.ForEachAsync(result =>
                        DuplicateFilter(result, searchData.Keyword.Length >= SharedConstants.NameLengthForCloseMatch)
                    );
                }

                if (searchData.ByRegistrationNumber) {
                    var byRegistrationNoResults = await SearchOrganizationByRegistrationNoWith(searchData.Keyword);
                    await byRegistrationNoResults.ForEachAsync(result =>
                        DuplicateFilter(result, searchData.Keyword.Length >= SharedConstants.RegistrationNoLengthForCloseMatch)
                    );
                }

                if (searchData.ByUniqueId) {
                    var byUniqueIdResults = await SearchOrganizationByUniqueIdWith(searchData.Keyword);
                    await byUniqueIdResults.ForEachAsync(result =>
                        DuplicateFilter(result, searchData.Keyword.Length >= SharedConstants.UniqueIdLengthForCloseMatch)
                    );
                }

                return organizationsWithMatchLevel;
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(OrganizationService) }.{ nameof(PerformSearchVariations) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while merging Organizations search result with ForEach.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(searchData) } = { JsonConvert.SerializeObject(searchData) }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
        }

        private async Task<IQueryable<Organization>> SearchOrganizationByNameWith(string keyword) {
            try {
                return _dbContext.Organizations
                                 .Where(
                                     organization => organization.FullName.ToLower().Contains(keyword) ||
                                                     organization.ShortName.ToLower().Contains(keyword)
                                 );
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(OrganizationService) }.{ nameof(SearchOrganizationByNameWith) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Organization with Where-Except-AsEnumerable.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(keyword) } = { keyword }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return Enumerable.Empty<Organization>().AsQueryable();
            }
        }

        private async Task<IQueryable<Organization>> SearchOrganizationByRegistrationNoWith(string keyword) {
            try {
                return _dbContext.Organizations
                                 .Where(organization => organization.RegistrationNumber.ToLower().Contains(keyword));
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(OrganizationService) }.{ nameof(SearchOrganizationByRegistrationNoWith) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Organization with Where-Except-AsEnumerable.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(keyword) } = { keyword }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return Enumerable.Empty<Organization>().AsQueryable();
            }
        }
        
        private async Task<IQueryable<Organization>> SearchOrganizationByUniqueIdWith(string keyword) {
            try {
                return _dbContext.Organizations
                                 .Where(organization => organization.UniqueId.ToLower().Contains(keyword));
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(OrganizationService) }.{ nameof(SearchOrganizationByUniqueIdWith) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Organization with Where-Except-AsEnumerable.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(keyword) } = { keyword }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return Enumerable.Empty<Organization>().AsQueryable();
            }
        }

        public async Task<bool> IsOrganizationUniqueIdAvailable(string uniqueId) {
            try {
                var uniqueIdFound = await _dbContext.Organizations.AnyAsync(organization => organization.UniqueId.Equals(uniqueId.ToUpper()));
                return !uniqueIdFound;
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(IsOrganizationUniqueIdAvailable) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Organization by UniqueId with AnyAsync.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(uniqueId) } = { uniqueId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
        }

        public async Task<int?> InsertNewOrganization(Organization organization) {
            try {
                await _dbContext.Organizations.AddAsync(organization);
                var result = await _dbContext.SaveChangesAsync();

                return result == 0 ? -1 : organization.Id;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(InsertNewOrganization) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while inserting entry to Organizations.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(organization) } = { JsonConvert.SerializeObject(organization) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<int?> InsertNewUserOrganization(UserOrganization userOrganization) {
            try {
                await _dbContext.UserOrganizations.AddAsync(userOrganization);
                var result = await _dbContext.SaveChangesAsync();

                return result == 0 ? -1 : userOrganization.Id;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(InsertNewUserOrganization) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while inserting entry to UserOrganizations.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userOrganization) } = { JsonConvert.SerializeObject(userOrganization) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<Organization> GetOrganizationById(int organizationId) {
            try {
                return await _dbContext.Organizations.FindAsync(organizationId);
            }
            catch (Exception e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(GetOrganizationById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(Exception),
                    DetailedInformation = $"Error while getting Organization using Find.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(organizationId) } = { organizationId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> UpdateOrganization(Organization organization) {
            try {
                _dbContext.Organizations.Update(organization);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(UpdateOrganization) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating entry to Organizations.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(organization) } = { JsonConvert.SerializeObject(organization) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<OrganizationVM[]> GetAllOrganizationsOwnedByUserId(int userId) {
            try {
                var userOrganizationTies = _dbContext.UserOrganizations
                                                     .Where(
                                                         userOrganization => userOrganization.UserId == userId &&
                                                                             userOrganization.IsActive &&
                                                                             userOrganization.DepartmentRole.IsManagerialRole &&
                                                                             userOrganization.DepartmentRole.HierarchyIndex == SharedConstants.OwnerHierarchyIndex
                                                     );
                
                return await userOrganizationTies
                             .Select(userOrganization => (OrganizationVM) userOrganization.Organization)
                             .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(GetAllOrganizationsOwnedByUserId) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching Organizations owned by User with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userId) } = { userId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<OrganizationDetailVM> GetDetailsForOrganizationById(int organizationId) {
            try {
                return await _dbContext.Organizations
                                       .Where(organization => organization.Id == organizationId)
                                       .Select(organization => (OrganizationDetailVM) organization)
                                       .FirstOrDefaultAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(GetDetailsForOrganizationById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while getting Organization details with Where-FirstOrDefault.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(organizationId) } = { organizationId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<int?> InsertNewDepartment(Department department) {
            try {
                await _dbContext.Departments.AddAsync(department);
                var result = await _dbContext.SaveChangesAsync();

                return result == 0 ? -1 : department.Id;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(InsertNewDepartment) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while inserting entry to Departments.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(department) } = { JsonConvert.SerializeObject(department) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<Department> GetDepartmentById(int departmentId) {
            try {
                return await _dbContext.Departments.FindAsync(departmentId);
            }
            catch (Exception e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(GetDepartmentById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(Exception),
                    DetailedInformation = $"Error while getting Department using Find.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(departmentId) } = { departmentId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> UpdateDepartment(Department department) {
            try {
                _dbContext.Departments.Update(department);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(UpdateDepartment) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating entry to Departments.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(department) } = { JsonConvert.SerializeObject(department) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<DepartmentVM[]> GetAllDepartmentsByOrganizationId(int organizationId) {
            try {
                return await _dbContext.Departments
                                       .Where(department => department.OrganizationId == organizationId)
                                       .Select(department => (DepartmentVM) department)
                                       .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(GetAllDepartmentsByOrganizationId) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while getting Departments within Organization with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(organizationId) } = { organizationId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<int?> InsertNewPositionTitle(PositionTitle userPosition) {
            try {
                await _dbContext.PositionTitles.AddAsync(userPosition);
                var result = await _dbContext.SaveChangesAsync();

                return result == 0 ? -1 : userPosition.Id;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(InsertNewPositionTitle) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while saving entry to PositionTitles.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userPosition) } = { JsonConvert.SerializeObject(userPosition) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<int?> InsertNewUserDepartment(UserDepartment userDepartment) {
            try {
                await _dbContext.UserDepartments.AddAsync(userDepartment);
                var result = await _dbContext.SaveChangesAsync();

                return result == 0 ? -1 : userDepartment.Id;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(InsertNewUserDepartment) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while saving entry to UserDepartments.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userDepartment) } = { JsonConvert.SerializeObject(userDepartment) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<UserOrganization> GetUserOrganizationById(int id) {
            try {
                return await _dbContext.UserOrganizations.FindAsync(id);
            }
            catch (Exception e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(GetUserOrganizationById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(Exception),
                    DetailedInformation = $"Error while getting UserOrganization using Find.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(id) } = { id }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<UserOrganization> GetUserOrganizationByUserId(int userId, int organizationId) {
            try {
                return await _dbContext.UserOrganizations
                                       .SingleOrDefaultAsync(
                                           userOrganization => userOrganization.UserId == userId &&
                                                               userOrganization.OrganizationId == organizationId &&
                                                               userOrganization.IsActive
                                       );
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(GetUserOrganizationByUserId) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while getting UserOrganization with SingleOrDefault, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, { nameof(organizationId) }) = ({ userId }, { organizationId })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(GetUserOrganizationByUserId) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while getting UserOrganization SingleOrDefault, >1 entry matching predicate.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, { nameof(organizationId) }) = ({ userId }, { organizationId })",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<UserDepartment> GetUserDepartmentById(int id) {
            try {
                return await _dbContext.UserDepartments.FindAsync(id);
            }
            catch (Exception e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(GetUserDepartmentById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(Exception),
                    DetailedInformation = $"Error while getting UserDepartment using Find.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(id) } = { id }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<UserDepartment> GetUserDepartmentByUserId(int userId, int organizationId) {
            try {
                return await _dbContext.UserDepartments
                                       .SingleOrDefaultAsync(
                                           userDepartment => userDepartment.UserId == userId &&
                                                             userDepartment.Department.OrganizationId == organizationId &&
                                                             userDepartment.IsActive
                                       );
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(GetUserDepartmentByUserId) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while getting UserDepartment with SingleOrDefault, null argument.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, { nameof(organizationId) }) = ({ userId }, { organizationId })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(GetUserDepartmentByUserId) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while getting UserDepartment SingleOrDefault, >1 entry matching predicate.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, { nameof(organizationId) }) = ({ userId }, { organizationId })",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> UpdateUserOrganization(UserOrganization userOrganization) {
            try {
                _dbContext.UserOrganizations.Update(userOrganization);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(UpdateUserOrganization) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating entry to UserOrganizations.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userOrganization) } = { JsonConvert.SerializeObject(userOrganization) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> UpdateUserDepartment(UserDepartment userDepartment) {
            try {
                _dbContext.UserDepartments.Update(userDepartment);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(UpdateUserDepartment) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating entry to UserDepartments.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userDepartment) } = { JsonConvert.SerializeObject(userDepartment) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<UserOrganizationVM[]> GetAllOrganizationManagers(int organizationId, bool isActive = true) {
            try {
                var userOrganizations = _dbContext.UserOrganizations
                                                  .Where(
                                                      userOrganization => userOrganization.OrganizationId == organizationId &&
                                                                          userOrganization.DepartmentRole.IsManagerialRole &&
                                                                          userOrganization.IsActive == isActive
                                                  );

                return await userOrganizations.Select(userOrganization => (UserOrganizationVM) userOrganization).ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(GetAllOrganizationManagers) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while getting UserOrganizations with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(organizationId) }, { nameof(isActive) }) = ({ organizationId }, { isActive })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<UserDepartmentVM[]> GetAllDepartmentEmployees(int organizationId, bool isActive = true) {
            try {
                var userOrganizations = _dbContext.UserDepartments
                                                  .Where(
                                                      userDepartment => userDepartment.Department.OrganizationId == organizationId &&
                                                                          userDepartment.DepartmentRole.IsManagerialRole &&
                                                                          userDepartment.IsActive == isActive
                                                  );

                return await userOrganizations.Select(userOrganization => (UserDepartmentVM) userOrganization).ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(GetAllDepartmentEmployees) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while getting UserDepartments with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(organizationId) }, { nameof(isActive) }) = ({ organizationId }, { isActive })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<AllPersonelsVM> GetAllOrganizationPersonels(int organizationId, bool isActive) {
            var managers = await GetAllOrganizationManagers(organizationId, isActive);
            if (managers == null) return null;

            var employees = await GetAllDepartmentEmployees(organizationId, isActive);
            if (employees == null) return null;

            return new AllPersonelsVM {
                Managers = managers,
                Employees = employees
            };
        }

        public async Task<PositionTitle[]> GetAllPositionTitlesFor(int organizationId) {
            try {
                return await _dbContext.PositionTitles.Where(title => title.OrganizationId == organizationId).ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(GetAllPositionTitlesFor) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while getting PositionTitles for Organization with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(organizationId) } = { organizationId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> DeletePositionTitleById(int positionId) {
            try {
                var positionTitle = await _dbContext.PositionTitles.FindAsync(positionId);
                if (positionTitle == null) return false;
                
                _dbContext.PositionTitles.Remove(positionTitle);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(DeletePositionTitleById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while deleting entry from OrganizationTitles.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(positionId) } = { positionId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> UpdatePositionTitle(PositionTitle positionTitle) {
            try {
                _dbContext.PositionTitles.Update(positionTitle);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(UpdatePositionTitle) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating entry to PositionTitles.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(positionTitle) } = { JsonConvert.SerializeObject(positionTitle) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> HasUserBeenAddedTo(string destination, int userId, int organizationId) {
            try {
                return destination switch {
                    nameof(UserOrganization) => await _dbContext.UserOrganizations
                                                                .AnyAsync(
                                                                    userOrganization => userOrganization.UserId == userId &&
                                                                                        userOrganization.IsActive &&
                                                                                        userOrganization.OrganizationId == organizationId
                                                                ),
                    nameof(UserDepartment) => await _dbContext.UserDepartments
                                                              .AnyAsync(
                                                                  userDepartment => userDepartment.UserId == userId &&
                                                                                    userDepartment.IsActive &&
                                                                                    userDepartment.Department.OrganizationId == organizationId
                                                              ),
                    _ => throw new ArgumentNullException($"Invalid param: { nameof(destination) }={ destination }")
                };
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(HasUserBeenAddedTo) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching UserDepartments or UserOrganization with AnyAsync.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(destination) }, { userId }, { nameof(organizationId) }) = ({destination}, { userId }, { organizationId })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<GenericRoleVM[]> GetRolesAccordingToDepartmentId(int departmentId, int organizationId) {
            try {
                var organizationRoles = _dbContext.DepartmentRoles.Where(role => role.OrganizationId == organizationId);

                return await organizationRoles
                    .Where(
                        role => !Helpers.IsProperString(role.ForDepartmentIds) ||
                                JsonConvert.DeserializeObject<int[]>(role.ForDepartmentIds).Contains(departmentId)
                    )
                    .Select(role => (GenericRoleVM) role)
                    .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(OrganizationService) }.{ nameof(GetRolesAccordingToDepartmentId) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while getting DepartmentRoles with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(departmentId) }, { nameof(organizationId) }) = ({departmentId}, { organizationId })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }
    }
}