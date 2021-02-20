using System;
using RoutinizeCore.Models;

namespace RoutinizeCore.ViewModels.Organization {

    public sealed class AllPersonelsVM {
        
        public UserOrganizationVM[] Managers { get; set; }
        
        public UserDepartmentVM[] Employees { get; set; }
    }

    public sealed class UserOrganizationVM : EmployeeDetailVM {
        
        public ManagerRoleVM Role { get; set; }

        public static implicit operator UserOrganizationVM(UserOrganization userOrganization) {
            return new() {
                Id = userOrganization.Id,
                UserId = userOrganization.UserId,
                Position = userOrganization.Position,
                FullName = userOrganization.User.PreferredName ?? $"{ userOrganization.User.FirstName } { userOrganization.User.LastName }",
                Avatar = userOrganization.User.AvatarName,
                EmployeeCode = userOrganization.EmployeeCode,
                JointOn = userOrganization.JointOn,
                LeftOn = userOrganization.LeftOn,
                Role = userOrganization.DepartmentRole
            };
        }
    }

    public sealed class UserDepartmentVM : EmployeeDetailVM {
        
        public EmployeeRoleVM Role { get; set; }
        
        public static implicit operator UserDepartmentVM(UserDepartment userDepartment) {
            return new() {
                Id = userDepartment.Id,
                UserId = userDepartment.UserId,
                Position = userDepartment.Position,
                FullName = userDepartment.User.PreferredName ?? $"{ userDepartment.User.FirstName } { userDepartment.User.LastName }",
                Avatar = userDepartment.User.AvatarName,
                EmployeeCode = userDepartment.EmployeeCode,
                JointOn = userDepartment.JointOn,
                LeftOn = userDepartment.LeftOn,
                Role = userDepartment.DepartmentRole
            };
        }
    }

    public class EmployeeDetailVM {
        
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        public PositionTitle Position { get; set; }
        
        public string FullName { get; set; }
        
        public string Avatar { get; set; }
        
        public string EmployeeCode { get; set; }
        
        public DateTime? JointOn { get; set; }
        
        public DateTime? LeftOn { get; set; }
    }
}