using System.Threading.Tasks;
using RoutinizeCore.Models;
using RoutinizeCore.ViewModels.Organization;

namespace RoutinizeCore.Services.Interfaces {

    public interface IDepartmentRoleService : IDbServiceBase {

        Task<int?> InsertNewDepartmentRole(DepartmentRole departmentRole);
        
        Task<DepartmentRole> GetDepartmentRoleById(int departmentRoleId);
        
        Task<bool?> UpdateDepartmentRole(DepartmentRole departmentRole);
        
        Task<bool?> IsThisRoleAssignedToAnyone(int roleId);
        
        Task<bool?> DeleteRoleById(int roleId);
        
        Task<GetRolesVM> GetAllDepartmentRoles(int organizationId);
        
        Task<AllPersonelsVM> GetAnyoneHavingThisRoleForOrganization(int roleId, int organizationId);
    }
}