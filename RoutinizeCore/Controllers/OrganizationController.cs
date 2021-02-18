using System;
using System.Threading.Tasks;
using HelperLibrary;
using HelperLibrary.Shared;
using Microsoft.AspNetCore.Mvc;
using RoutinizeCore.Attributes;
using RoutinizeCore.Models;
using RoutinizeCore.Services.Interfaces;
using RoutinizeCore.ViewModels;
using RoutinizeCore.ViewModels.Organization;

namespace RoutinizeCore.Controllers {

    [ApiController]
    [RoutinizeActionFilter]
    [Route("organization")]
    public sealed class OrganizationController {

        private readonly IOrganizationService _organizationService;
        private readonly IAddressService _addressService;
        private readonly IDepartmentRoleService _departmentRoleService;

        public OrganizationController(
            IOrganizationService organizationService,
            IAddressService addressService,
            IDepartmentRoleService departmentRoleService
        ) {
            _organizationService = organizationService;
            _addressService = addressService;
            _departmentRoleService = departmentRoleService;
        }

        [HttpGet("get-industries")]
        public async Task<JsonResult> GetIndustryList() {
            var industryList = await _organizationService.GetIndustryList();
            return industryList == null ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                                        : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = industryList });
        }

        [HttpGet("search/{organizationId}/{keyword}")]
        public async Task<JsonResult> SearchOrganizationsByFullName([FromRoute] int organizationId,[FromRoute] string keyword) {
            if (!Helpers.IsProperString(keyword)) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed });

            var searchResult = await _organizationService.SearchMotherOrganizationsOnFullNameByKeywordFor(organizationId, keyword);
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = searchResult });
        }

        [HttpPost("create")]
        public async Task<JsonResult> CreateOrganization([FromHeader] int userId,[FromBody] Organization organization) {
            var errors = organization.VerifyOrganizationData();
            if (errors.Count != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errors });

            await _organizationService.StartTransaction();

            var saveAddressResult = await _addressService.SaveNewAddress(organization.Address);
            if (!saveAddressResult.HasValue || saveAddressResult.Value < 1) {
                await _organizationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." });
            }

            organization.AddressId = saveAddressResult.Value;
            var organizationUniqueId = string.Empty;
            var isOrganizationUniqueIdValid = false;
            while (!isOrganizationUniqueIdValid) {
                organizationUniqueId = Helpers.GenerateRandomString(SharedConstants.DEFAULT_UNIQUE_ID_LENGTH);
                isOrganizationUniqueIdValid = await _organizationService.IsOrganizationUniqueIdAvailable(organizationUniqueId);

                if (isOrganizationUniqueIdValid) organizationUniqueId = organizationUniqueId.ToUpper();
            }

            organization.UniqueId = organizationUniqueId;
            var saveOrganizationResult = await _organizationService.InsertNewOrganization(organization);
            if (!saveOrganizationResult.HasValue || saveOrganizationResult.Value < 1) {
                await _organizationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." });
            }

            var defaultUserRole = DepartmentRole.GetDefaultManagerialInstance();
            defaultUserRole.OrganizationId = saveOrganizationResult.Value;
            defaultUserRole.AddedById = userId;

            var saveRoleResult = await _departmentRoleService.InsertNewDepartmentRole(defaultUserRole);
            if (!saveRoleResult.HasValue || saveRoleResult.Value < 1) {
                await _organizationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." });
            }

            var defaultUserPosition = PositionTitle.GetDefaultManagerialTitle();
            defaultUserPosition.OrganizationId = saveOrganizationResult.Value;

            var savePositionResult = await _organizationService.InsertNewPositionTitle(defaultUserPosition);
            if (!savePositionResult.HasValue || savePositionResult.Value < 1) {
                await _organizationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." });
            }
            
            var userOrganization = new UserOrganization {
                UserId = userId,
                DepartmentRoleId = saveRoleResult.Value,
                OrganizationId = saveOrganizationResult.Value,
                PositionId = savePositionResult.Value,
                IsActive = true,
                JointOn = DateTime.UtcNow
            };

            var saveUserOrgResult = await _organizationService.InsertNewUserOrganization(userOrganization);
            if (!saveUserOrgResult.HasValue || saveUserOrgResult.Value < 1) {
                await _organizationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." });
            }

            await _organizationService.CommitTransaction();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = saveOrganizationResult.Value });
        }
        
        [HttpPut("update")]
        public async Task<JsonResult> UpdateOrganization(Organization organization) {
            var errors = organization.VerifyOrganizationData();
            if (errors.Count != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errors });

            var dbOrganization = await _organizationService.GetOrganizationById(organization.Id);
            if (dbOrganization == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            dbOrganization.UpdateDataBy(organization);
            var updateResult = await _organizationService.UpdateOrganization(dbOrganization);

            return !updateResult.HasValue || !updateResult.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
        
        [HttpGet("get")]
        public async Task<JsonResult> GetOrganizationsByUser([FromHeader] int userId) {
            var organizations = await _organizationService.GetAllOrganizationByUserId(userId);
            return organizations == null ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                                         : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = organizations });
        }
        
        [HttpGet("details/{organizationId}")]
        public async Task<JsonResult> GetOrganizationDetails([FromRoute] int organizationId) {
            var organizationDetail = await _organizationService.GetDetailsForOrganizationById(organizationId);
            return organizationDetail == null ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                                              : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = organizationDetail });
        }
        
        [HttpPost("add-department")]
        public async Task<JsonResult> AddDepartmentToOrganization(Department department) {
            var errors = department.VerifyDepartmentData();
            if (errors.Count != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errors });

            var saveResult = await _organizationService.InsertNewDepartment(department);
            return !saveResult.HasValue || saveResult.Value < 1
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = saveResult.Value });
        }
        
        [HttpPut("update-department")]
        public async Task<JsonResult> UpdateDepartment(Department department) {
            var errors = department.VerifyDepartmentData();
            if (errors.Count != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errors });

            var dbDepartment = await _organizationService.GetDepartmentById(department.Id);
            if (dbDepartment == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            dbDepartment.UpdateDataBy(department);
            var saveResult = await _organizationService.UpdateDepartment(department);
            return !saveResult.HasValue || !saveResult.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
        
        [HttpGet("get-departments/{organizationId}")]
        public async Task<JsonResult> GetAllDepartmentsForOrganization([FromRoute] int organizationId) {
            var departments = await _organizationService.GetAllDepartmentsByOrganizationId(organizationId);
            return departments == null ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                                       : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = departments });
        }
        
        [HttpPost("add-user")]
        public async Task<JsonResult> AddOrganizationOrDepartmentLevelUser(EmployeeVM employeeData) {
            var errors = employeeData.VerifyEmployeeData();
            if (errors.Length != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errors });
            
            int? saveEmployeeResult;
            if (employeeData.EmployeeType.Equals(nameof(UserOrganization))) {
                var newManager = employeeData.GetManagerUser();
                saveEmployeeResult = await _organizationService.InsertNewUserOrganization(newManager);
            }
            else {
                var newEmployee = employeeData.GetEmployeeUser();
                saveEmployeeResult = await _organizationService.InsertNewUserDepartment(newEmployee);
            }

            return !saveEmployeeResult.HasValue || saveEmployeeResult.Value < 1
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = saveEmployeeResult.Value });
        }
        
        [HttpPut("update-user")]
        public async Task<JsonResult> UpdateOrganizationOrDepartmentLevelUser(EmployeeVM employeeData) {
            var errors = employeeData.VerifyEmployeeData();
            if (errors.Length != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errors });

            bool? updateEmployeeResult;
            if (employeeData.EmployeeType.Equals(nameof(UserOrganization))) {
                var manager = await _organizationService.GetUserOrganizationById(employeeData.Id);
                employeeData.UpdateManagerUserFor(ref manager);

                updateEmployeeResult = await _organizationService.UpdateUserOrganization(manager);
            }
            else {
                var employee = await _organizationService.GetUserDepartmentById(employeeData.Id);
                employeeData.UpdateEmployeeUserFor(ref employee);

                updateEmployeeResult = await _organizationService.UpdateUserDepartment(employee);
            }

            return !updateEmployeeResult.HasValue || !updateEmployeeResult.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
        
        [HttpGet("get-organization-level-users/{organizationId}")]
        public async Task<JsonResult> GetOrganizationLevelUsers([FromRoute] int organizationId) {
            var managers = await _organizationService.GetAllOrganizationManagers(organizationId);
            return managers == null ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                                    : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = managers });
        }
        
        [HttpGet("get-department-level-users/{departmentId}")]
        public async Task<JsonResult> GetDepartmentLevelUsers([FromRoute] int departmentId) {
            var employees = await _organizationService.GetAllDepartmentEmployees(departmentId);
            return employees == null ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                                     : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = employees });
        }
        
        [HttpGet("get-all-users/{organizationId}")]
        public async Task<JsonResult> GetAllUsersWithinAnOrganization([FromRoute] int organizationId) {
            var personels = await _organizationService.GetAllOrganizationPersonels(organizationId);
            return personels == null ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                                     : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = personels });
        }
        
        [HttpPut("update-role")]
        public async Task<JsonResult> UpdateDepartmentRoleForUser(EmployeeVM employeeData) {
            bool? updateResult;
            if (employeeData.EmployeeType.Equals(nameof(UserOrganization))) {
                var manager = await _organizationService.GetUserOrganizationById(employeeData.Id);
                if (manager == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

                manager.DepartmentRoleId = employeeData.RoleId;
                updateResult = await _organizationService.UpdateUserOrganization(manager);
            }
            else {
                var employee = await _organizationService.GetUserDepartmentById(employeeData.Id);
                if (employee == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

                employee.DepartmentRoleId = employeeData.RoleId;
                updateResult = await _organizationService.UpdateUserDepartment(employee);
            }

            return !updateResult.HasValue || !updateResult.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }

        [HttpPut("transfer-user")]
        public async Task<JsonResult> TransferBetweenUserOrganizationAndUserDepartment(EmployeeVM employeeData) {
            await _organizationService.StartTransaction();

            int? transferResult;
            if (employeeData.EmployeeType.Equals(nameof(UserOrganization))) {
                var manager = await _organizationService.GetUserOrganizationById(employeeData.Id);
                if (manager == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

                var deleteManagerResult = await _organizationService.DeleteUserOrganization(manager);
                if (!deleteManagerResult.HasValue || !deleteManagerResult.Value) {
                    await _organizationService.RevertTransaction();
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while removing data." });
                }

                UserDepartment employee = manager;
                employee.DepartmentId = employeeData.DepartmentId;
                transferResult = await _organizationService.InsertNewUserDepartment(employee);
            }
            else {
                var employee = await _organizationService.GetUserDepartmentById(employeeData.Id);
                if (employee == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

                var deleteEmployeeResult = await _organizationService.DeleteUserDepartment(employee);
                if (!deleteEmployeeResult.HasValue || !deleteEmployeeResult.Value) {
                    await _organizationService.RevertTransaction();
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while removing data." });
                }

                UserOrganization manager = employee;
                manager.OrganizationId = employeeData.OrganizationId;
                transferResult = await _organizationService.InsertNewUserOrganization(manager);
            }

            if (!transferResult.HasValue || transferResult.Value < 1) {
                await _organizationService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." });
            }

            await _organizationService.CommitTransaction();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = transferResult.Value });
        }

        [HttpPost("add-role")]
        public async Task<JsonResult> AddNewRoleForOrganization(DepartmentRole newRole) {
            var errors = newRole.VerifyRoleData();
            if (errors.Length != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errors });

            var saveResult = await _departmentRoleService.InsertNewDepartmentRole(newRole);
            return !saveResult.HasValue || saveResult.Value < 1
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = saveResult.Value });
        }
        
        [HttpPut("update-role")]
        public async Task<JsonResult> UpdateRoleForOrganization(DepartmentRole departmentRole) {
            var errors = departmentRole.VerifyRoleData();
            if (errors.Length != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errors });

            var dbDepartmentRole = await _departmentRoleService.GetDepartmentRoleById(departmentRole.Id);
            if (dbDepartmentRole == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            dbDepartmentRole.UpdateDataBy(departmentRole);

            var saveResult = await _departmentRoleService.UpdateDepartmentRole(departmentRole);
            return !saveResult.HasValue || !saveResult.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
        
        [HttpDelete("remove-role/{roleId}")]
        public async Task<JsonResult> DeleteRoleForOrganization([FromRoute] int roleId) {
            var shouldDeleteRole = await _departmentRoleService.IsThisRoleAssignedToAnyone(roleId);
            if (!shouldDeleteRole.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!shouldDeleteRole.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Role is assigned to someone." });

            var deleteResult = await _departmentRoleService.DeleteRoleById(roleId);
            return !deleteResult.HasValue || !deleteResult.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while removing data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
        
        [HttpGet("get-roles")]
        public async Task<JsonResult> GetAllOrganizationRoles() {
            var departmentRoles = await _departmentRoleService.GetAllDepartmentRoles();
            return departmentRoles == null ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                                           : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = departmentRoles });
        }

        [HttpGet("get-employees-by-role/{roleId}")]
        public async Task<JsonResult> GetEmployeesByRole([FromRoute] int roleId) {
            var personels = await _departmentRoleService.GetAnyoneHavingThisRoleById(roleId);
            return personels == null ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                                     : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = personels });
        }

        [HttpPost("add-position-title")]
        public async Task<JsonResult> AddPositionTitle([FromHeader] int userId,[FromBody] PositionTitle positionTitle) {
            var errors = positionTitle.VerifyPositionTitleData();
            if (errors.Length != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errors }); 
            
            var organization = await _organizationService.GetOrganizationByUserId(userId);
            if (organization == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            positionTitle.OrganizationId = organization.Id;
            positionTitle.AddedOn = DateTime.UtcNow;

            var saveResult = await _organizationService.InsertNewPositionTitle(positionTitle);
            return !saveResult.HasValue || saveResult.Value < 1
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while removing data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = saveResult.Value });
        }
        
        [HttpPut("update-position-title")]
        public async Task<JsonResult> UpdatePositionTitle(PositionTitle positionTitle) {
            var updateResult = await _organizationService.UpdatePositionTitle(positionTitle);
            return !updateResult.HasValue || !updateResult.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while removing data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
        
        [HttpDelete("remove-position-title/{positionId}")]
        public async Task<JsonResult> RemovePositionTitle([FromRoute] int positionId) {
            var deleteResult = await _organizationService.DeletePositionTitleById(positionId);
            return !deleteResult.HasValue || !deleteResult.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while removing data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
        
        [HttpGet("get-position-titles")]
        public async Task<JsonResult> GetAllPositionTitles() {
            var positionTitles = await _organizationService.GetAllPositionTitles();
            return positionTitles == null ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                                          : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = positionTitles });
        }

        [HttpPut("swap-department-parent/{departmentId}/{newParentId}")]
        public async Task<JsonResult> SwapParentForDepartment([FromRoute] int departmentId,[FromRoute] int newParentId) {
            //Consider updating ParentId for a department, deleting data associated with current parent if necessary
            throw new NotImplementedException();
        }

        [HttpDelete("remove-department/{departmentId}")]
        public async Task<JsonResult> RemoveDepartment() {
            //Consider delete/archive department, if delete, remove all associated data
            throw new NotImplementedException();
        }
        
        [HttpPut("deactivate-user/{employeeId}")]
        public async Task<JsonResult> DeactivateOrganizationOrDepartmentLevelUser([FromRoute] int employeeId) {
            //Consider deactivating a user, allowing for that user to later join the organization again, keep audits of join/leave
            throw new NotImplementedException();
        }
        
        [HttpDelete("delete/{organizationId}")]
        public async Task<JsonResult> DeleteOrganization() {
            //Consider delete/archive organization, if delete, remove all associated data
            throw new NotImplementedException();
        }
    }
}