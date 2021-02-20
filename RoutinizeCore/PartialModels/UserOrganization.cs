namespace RoutinizeCore.Models {

    public partial class UserOrganization {
        
        public static implicit operator UserOrganization(UserDepartment employee) {
            return new() {
                UserId = employee.UserId,
                DepartmentRoleId = employee.DepartmentRoleId,
                PositionId = employee.PositionId,
                EmployeeCode = employee.EmployeeCode,
                JointOn = employee.JointOn,
                IsActive = employee.IsActive,
                LeftOn = employee.LeftOn
            };
        }
    }
}