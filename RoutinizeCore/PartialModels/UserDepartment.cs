namespace RoutinizeCore.Models {

    public partial class UserDepartment {

        public static implicit operator UserDepartment(UserOrganization manager) {
            return new UserDepartment {
                UserId = manager.UserId,
                DepartmentRoleId = manager.DepartmentRoleId,
                PositionId = manager.PositionId,
                EmployeeCode = manager.EmployeeCode,
                IsActive = manager.IsActive,
                JointOn = manager.JointOn,
                LeftOn = manager.LeftOn
            };
        }
    }
}