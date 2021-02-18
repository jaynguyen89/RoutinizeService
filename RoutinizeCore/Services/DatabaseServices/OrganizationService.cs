using System.Threading.Tasks;
using HelperLibrary.Shared;
using MongoLibrary.Interfaces;
using RoutinizeCore.DbContexts;
using RoutinizeCore.Models;
using RoutinizeCore.Services.Interfaces;
using RoutinizeCore.ViewModels.Organization;

namespace RoutinizeCore.Services.DatabaseServices {

    public class OrganizationService : DbServiceBase, IOrganizationService {

        private readonly IRoutinizeCoreLogService _coreLogService;
        private readonly RoutinizeDbContext _dbContext;

        public OrganizationService(
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

        public async Task<string[]> GetIndustryList() {
            throw new System.NotImplementedException();
        }

        public async Task<SearchOrganizationResultVM> SearchMotherOrganizationsOnFullNameByKeywordFor(int organizationId, string keyword) {
            throw new System.NotImplementedException();
        }

        public async Task<bool> IsOrganizationUniqueIdAvailable(string uniqueId) {
            throw new System.NotImplementedException();
        }

        public async Task<int?> InsertNewOrganization(Organization organization) {
            throw new System.NotImplementedException();
        }

        public async Task<int?> InsertNewUserOrganization(UserOrganization userOrganization) {
            throw new System.NotImplementedException();
        }

        public async Task<Organization> GetOrganizationById(int organizationId) {
            throw new System.NotImplementedException();
        }

        public async Task<bool?> UpdateOrganization(Organization organization) {
            throw new System.NotImplementedException();
        }

        public async Task<OrganizationVM[]> GetAllOrganizationByUserId(int userId) {
            throw new System.NotImplementedException();
        }

        public async Task<OrganizationDetailVM> GetDetailsForOrganizationById(int organizationId) {
            throw new System.NotImplementedException();
        }

        public async Task<int?> InsertNewDepartment(Department department) {
            throw new System.NotImplementedException();
        }

        public async Task<Department> GetDepartmentById(int departmentId) {
            throw new System.NotImplementedException();
        }

        public async Task<bool?> UpdateDepartment(Department department) {
            throw new System.NotImplementedException();
        }

        public async Task<DepartmentVM[]> GetAllDepartmentsByOrganizationId(int organizationId) {
            throw new System.NotImplementedException();
        }

        public async Task<int?> InsertNewPositionTitle(PositionTitle userPosition) {
            throw new System.NotImplementedException();
        }

        public async Task<int?> InsertNewUserDepartment(UserDepartment userDepartment) {
            throw new System.NotImplementedException();
        }

        public async Task<UserOrganization> GetUserOrganizationById(int id) {
            throw new System.NotImplementedException();
        }

        public async Task<UserDepartment> GetUserDepartmentById(int id) {
            throw new System.NotImplementedException();
        }

        public async Task<bool?> UpdateUserOrganization(UserOrganization userOrganization) {
            throw new System.NotImplementedException();
        }

        public async Task<bool?> UpdateUserDepartment(UserDepartment userDepartment) {
            throw new System.NotImplementedException();
        }

        public async Task<UserOrganizationVM[]> GetAllOrganizationManagers(int organizationId) {
            throw new System.NotImplementedException();
        }

        public async Task<UserDepartmentVM[]> GetAllDepartmentEmployees(int organizationId) {
            throw new System.NotImplementedException();
        }

        public async Task<AllPersonelsVM> GetAllOrganizationPersonels(int organizationId) {
            throw new System.NotImplementedException();
        }

        public async Task<bool?> DeleteUserOrganization(UserOrganization userOrganization) {
            throw new System.NotImplementedException();
        }

        public async Task<bool?> DeleteUserDepartment(UserDepartment userDepartment) {
            throw new System.NotImplementedException();
        }

        public async Task<PositionTitle> GetAllPositionTitles() {
            throw new System.NotImplementedException();
        }

        public async Task<bool?> DeletePositionTitleById(int positionId) {
            throw new System.NotImplementedException();
        }

        public async Task<bool?> UpdatePositionTitle(PositionTitle positionTitle) {
            throw new System.NotImplementedException();
        }

        public async Task<Organization> GetOrganizationByUserId(int userId) {
            throw new System.NotImplementedException();
        }
    }
}