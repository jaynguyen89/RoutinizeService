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

    public class DepartmentRoleService : DbServiceBase, IDepartmentRoleService {
        
        private readonly IRoutinizeCoreLogService _coreLogService;
        private readonly RoutinizeDbContext _dbContext;

        public DepartmentRoleService(
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

        public async Task<int?> InsertNewDepartmentRole(DepartmentRole departmentRole) {
            try {
                await _dbContext.DepartmentRoles.AddAsync(departmentRole);
                var result = await _dbContext.SaveChangesAsync();

                return result == 0 ? -1 : departmentRole.Id;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(DepartmentRoleService) }.{ nameof(InsertNewDepartmentRole) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while inserting entry to DepartmentRoles.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(departmentRole) } = { JsonConvert.SerializeObject(departmentRole) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<DepartmentRole> GetDepartmentRoleById(int departmentRoleId) {
            try {
                return await _dbContext.DepartmentRoles.FindAsync(departmentRoleId);
            }
            catch (Exception e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(DepartmentRoleService) }.{ nameof(GetDepartmentRoleById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(Exception),
                    DetailedInformation = $"Error while getting DepartmentRole using Find.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(departmentRoleId) } = { departmentRoleId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> UpdateDepartmentRole(DepartmentRole departmentRole) {
            try {
                _dbContext.DepartmentRoles.Update(departmentRole);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(DepartmentRoleService) }.{ nameof(UpdateDepartmentRole) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating entry to DepartmentRoles.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(departmentRole) } = { JsonConvert.SerializeObject(departmentRole) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> IsThisRoleAssignedToAnyone(int roleId) {
            try {
                var isFoundInUserOrganization = await _dbContext.UserOrganizations.AnyAsync(userOrganization => userOrganization.DepartmentRoleId == roleId && userOrganization.IsActive);
                var isFoundInUserDepartment = await _dbContext.UserDepartments.AnyAsync(userDepartment => userDepartment.DepartmentRoleId == roleId && userDepartment.IsActive);

                return isFoundInUserOrganization || isFoundInUserDepartment;
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(DepartmentRoleService) }.{ nameof(IsThisRoleAssignedToAnyone) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching UserOrganizations and UserDepartments with AnyAsync.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(roleId) } = { roleId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> DeleteRoleById(int roleId) {
            try {
                var departmentRole = await _dbContext.DepartmentRoles.FindAsync(roleId);
                _dbContext.DepartmentRoles.Remove(departmentRole);

                var result = await _dbContext.SaveChangesAsync();
                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(DepartmentRoleService) }.{ nameof(DeleteRoleById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while removing entry from DepartmentRoles.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(roleId) } = { roleId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<GetRolesVM> GetAllDepartmentRoles(int organizationId) {
            try {
                var dbManagerialRoles = await _dbContext.DepartmentRoles.Where(role => role.IsManagerialRole && role.OrganizationId == organizationId).ToArrayAsync();
                var dbEmployeeRoles = await _dbContext.DepartmentRoles.Where(role => !role.IsManagerialRole && role.OrganizationId == organizationId).ToArrayAsync();

                void MapDepartmentsToIds(ref List<GenericDepartmentVM> departments, string departmentIds) {
                    var departmentIdsList = JsonConvert.DeserializeObject<List<int>>(departmentIds);
                    var vms = departments;
                    departmentIdsList.ForEach(async id => {
                        var department = await _dbContext.Departments.FindAsync(id);
                        vms.Add(department);
                    });
                }

                var managerRoles = dbManagerialRoles
                                   .Select(role => {
                                       var roleVm = new ManagerRoleVM();
                        
                                       var departments = new List<GenericDepartmentVM>();
                                       MapDepartmentsToIds(ref departments, role.ForDepartmentIds);
                        
                                       roleVm.ForDepartments = departments;
                                       return roleVm;
                                   })
                                   .ToArray();

                var employeeRoles = dbEmployeeRoles
                    .Select(role => {
                        var roleVm = new EmployeeRoleVM();
                        
                        var departments = new List<GenericDepartmentVM>();
                        MapDepartmentsToIds(ref departments, role.ForDepartmentIds);
                        
                        roleVm.ForDepartments = departments;
                        return roleVm;
                    })
                    .ToArray();

                return new GetRolesVM {
                    ManagerialRoles = managerRoles,
                    EmployeeRoles = employeeRoles
                };
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(DepartmentRoleService) }.{ nameof(GetAllDepartmentRoles) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching DepartmentRoles with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(organizationId) } = { organizationId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<AllPersonelsVM> GetAnyoneHavingThisRoleForOrganization(int roleId, int organizationId) {
            try {
                var userOrganizations = _dbContext.UserOrganizations
                                                  .Where(
                                                      userOrganization => userOrganization.DepartmentRoleId == roleId &&
                                                                          userOrganization.OrganizationId == organizationId &&
                                                                          userOrganization.IsActive
                                                  );
                
                var userDepartments = _dbContext.UserDepartments
                                                .Where(
                                                    userDepartment => userDepartment.DepartmentRoleId == roleId &&
                                                                      userDepartment.Department.OrganizationId == organizationId &&
                                                                      userDepartment.IsActive
                                                );

                var managers = await userOrganizations
                                     .Select(userOrganization => new UserOrganizationVM {
                                         Id = userOrganization.Id,
                                         UserId = userOrganization.UserId,
                                         Position = userOrganization.Position,
                                         FullName = $"{ userOrganization.User.FirstName } { userOrganization.User.LastName }",
                                         Avatar = userOrganization.User.AvatarName,
                                         EmployeeCode = userOrganization.EmployeeCode,
                                         JointOn = userOrganization.JointOn,
                                         Role = (ManagerRoleVM) userOrganization.DepartmentRole
                                     })
                                     .ToArrayAsync();

                var employees = await userDepartments
                                      .Select(userDepartment => new UserDepartmentVM {
                                          Id = userDepartment.Id,
                                          UserId = userDepartment.UserId,
                                          Position = userDepartment.Position,
                                          FullName = $"{ userDepartment.User.FirstName } { userDepartment.User.LastName }",
                                          Avatar = userDepartment.User.AvatarName,
                                          EmployeeCode = userDepartment.EmployeeCode,
                                          JointOn = userDepartment.JointOn,
                                          Role = (EmployeeRoleVM) userDepartment.DepartmentRole
                                      })
                                      .ToArrayAsync();

                return new AllPersonelsVM {
                    Managers = managers,
                    Employees = employees
                };
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(DepartmentRoleService) }.{ nameof(GetAnyoneHavingThisRoleForOrganization) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching UserOrganizations and UserDepartments with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(roleId) }, { nameof(organizationId) }) = ({ roleId }, { organizationId })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }
    }
}