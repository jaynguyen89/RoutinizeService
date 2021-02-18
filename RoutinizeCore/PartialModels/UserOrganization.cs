namespace RoutinizeCore.Models {

    public partial class UserOrganization {

        public static implicit operator UserOrganization(UserDepartment employee) {
            return new UserOrganization {
                UserId = employee.UserId,
                DepartmentRoleId = employee.DepartmentRoleId,
                PositionId = employee.PositionId,
                EmployeeCode = employee.EmployeeCode,
                IsActive = employee.IsActive,
                JointOn = employee.JointOn,
                LeftOn = employee.LeftOn
            };
        }
    }
}