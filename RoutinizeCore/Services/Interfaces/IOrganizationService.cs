using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using RoutinizeCore.Models;
using RoutinizeCore.ViewModels.Organization;

namespace RoutinizeCore.Services.Interfaces {

    public interface IOrganizationService : IDbServiceBase {

        Task<string[]> GetIndustryList();
        
        Task<SearchOrganizationResultVM> SearchMotherOrganizationsOnFullNameByKeywordFor(int organizationId, string keyword);
        
        Task<bool> IsOrganizationUniqueIdAvailable([NotNull] string uniqueId);
        
        Task<int?> InsertNewOrganization(Organization organization);
        
        Task<int?> InsertNewUserOrganization(UserOrganization userOrganization);
        
        Task<Organization> GetOrganizationById(int organizationId);
        
        Task<bool?> UpdateOrganization(Organization organization);
        
        Task<OrganizationVM[]> GetAllOrganizationByUserId(int userId);
        
        Task<OrganizationDetailVM> GetDetailsForOrganizationById(int organizationId);
        
        Task<int?> InsertNewDepartment(Department department);
        
        Task<Department> GetDepartmentById(int departmentId);
        
        Task<bool?> UpdateDepartment(Department department);
        
        Task<DepartmentVM[]> GetAllDepartmentsByOrganizationId(int organizationId);
        
        Task<int?> InsertNewPositionTitle(PositionTitle userPosition);
        
        Task<int?> InsertNewUserDepartment(UserDepartment userDepartment);
        
        Task<UserOrganization> GetUserOrganizationById(int id);
        
        Task<UserDepartment> GetUserDepartmentById(int id);
        
        Task<bool?> UpdateUserOrganization(UserOrganization userOrganization);
        
        Task<bool?> UpdateUserDepartment(UserDepartment userDepartment);
        
        Task<UserOrganizationVM[]> GetAllOrganizationManagers(int organizationId);
        
        Task<UserDepartmentVM[]> GetAllDepartmentEmployees(int organizationId);
        
        Task<AllPersonelsVM> GetAllOrganizationPersonels(int organizationId);
        
        Task<bool?> DeleteUserOrganization(UserOrganization userOrganization);
        
        Task<bool?> DeleteUserDepartment(UserDepartment userDepartment);
        
        Task<PositionTitle> GetAllPositionTitles();
        
        Task<bool?> DeletePositionTitleById(int positionId);
        
        Task<bool?> UpdatePositionTitle(PositionTitle positionTitle);
        
        Task<Organization> GetOrganizationByUserId(int userId);
    }

}