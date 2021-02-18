using System.Threading.Tasks;
using HelperLibrary.Shared;
using MongoLibrary.Interfaces;
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
        
        public new async Task SetChangesToDbContext(object any, string task = SharedConstants.TASK_INSERT) {
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
            throw new System.NotImplementedException();
        }

        public async Task<DepartmentRole> GetDepartmentRoleById(int employeeDataRoleId) {
            throw new System.NotImplementedException();
        }

        public async Task<bool?> UpdateDepartmentRole(DepartmentRole departmentRole) {
            throw new System.NotImplementedException();
        }

        public async Task<bool?> IsThisRoleAssignedToAnyone(int roleId) {
            throw new System.NotImplementedException();
        }

        public async Task<bool?> DeleteRoleById(int roleId) {
            throw new System.NotImplementedException();
        }

        public async Task<GetRolesVM> GetAllDepartmentRoles() {
            throw new System.NotImplementedException();
        }

        public async Task<AllPersonelsVM> GetAnyoneHavingThisRoleById(int roleId) {
            throw new System.NotImplementedException();
        }
    }
}