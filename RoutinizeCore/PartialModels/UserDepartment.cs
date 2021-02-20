namespace RoutinizeCore.Models {

    public partial class UserDepartment {

        public static implicit operator UserDepartment(UserOrganization manager) {
            return new() {
                UserId = manager.UserId,
                DepartmentRoleId = manager.DepartmentRoleId,
                PositionId = manager.PositionId,
                EmployeeCode = manager.EmployeeCode,
                JointOn = manager.JointOn,
                IsActive = manager.IsActive,
                LeftOn = manager.LeftOn
            };
        }
    }
}