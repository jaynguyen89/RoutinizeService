using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using RoutinizeCore.Models;
using RoutinizeCore.ViewModels.Organization;

namespace RoutinizeCore.Services.Interfaces {

    public interface IOrganizationService : IDbServiceBase {

        Task<IndustryVM[]> GetIndustryList();
        
        Task<SearchOrganizationVM> SearchForMotherOrganizations(OrganizationSearchDataVM searchData);
        
        Task<bool> IsOrganizationUniqueIdAvailable([NotNull] string uniqueId);
        
        Task<int?> InsertNewOrganization(Organization organization);
        
        Task<int?> InsertNewUserOrganization(UserOrganization userOrganization);
        
        Task<Organization> GetOrganizationById(int organizationId);
        
        Task<bool?> UpdateOrganization(Organization organization);
        
        Task<OrganizationVM[]> GetAllOrganizationsOwnedByUserId(int userId);
        
        Task<OrganizationDetailVM> GetDetailsForOrganizationById(int organizationId);
        
        Task<int?> InsertNewDepartment(Department department);
        
        Task<Department> GetDepartmentById(int departmentId);
        
        Task<bool?> UpdateDepartment(Department department);
        
        Task<DepartmentVM[]> GetAllDepartmentsByOrganizationId(int organizationId);
        
        Task<int?> InsertNewPositionTitle(PositionTitle userPosition);
        
        Task<int?> InsertNewUserDepartment(UserDepartment userDepartment);
        
        Task<UserOrganization> GetUserOrganizationById(int id);
        
        Task<UserOrganization> GetUserOrganizationByUserId(int userId, int organizationId);
        
        Task<UserDepartment> GetUserDepartmentById(int id);
        
        Task<UserDepartment> GetUserDepartmentByUserId(int userId, int organizationId);
        
        Task<bool?> UpdateUserOrganization(UserOrganization userOrganization);
        
        Task<bool?> UpdateUserDepartment(UserDepartment userDepartment);
        
        Task<UserOrganizationVM[]> GetAllOrganizationManagers(int organizationId, bool isActive = true);
        
        Task<UserDepartmentVM[]> GetAllDepartmentEmployees(int organizationId, bool isActive = true);
        
        Task<AllPersonelsVM> GetAllOrganizationPersonels(int organizationId, bool isActive);

        Task<PositionTitle[]> GetAllPositionTitlesFor(int organizationId);
        
        Task<bool?> DeletePositionTitleById(int positionId);
        
        Task<bool?> UpdatePositionTitle(PositionTitle positionTitle);
        
        Task<bool?> HasUserBeenAddedTo(string destination, int userId, int organizationId);
        
        Task<GenericRoleVM[]> GetRolesAccordingToDepartmentId(int departmentId, int organizationId);
        
        Task<bool?> IsUserBelongedToOrganizationAndAllowedToManageCooperation(int userId, int organizationId);
        
        Task<Organization> GetOrganizationByUniqueId(string uniqueId);
        
        Task<bool?> IsThisDepartmentExistedAndForCooperation(int departmentId, int organizationId);
    }

}