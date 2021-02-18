using System;
using RoutinizeCore.Models;

namespace RoutinizeCore.ViewModels.Organization {

    public sealed class AllPersonelsVM {
        
        public UserDepartmentVM[] Managers { get; set; }
        
        public UserDepartmentVM[] Employees { get; set; }
    }

    public sealed class UserOrganizationVM : EmployeeDetailVM {
        
        public ManagerRoleVM Role { get; set; }
    }

    public sealed class UserDepartmentVM : EmployeeDetailVM {
        
        public EmployeeRoleVM Role { get; set; }
    }

    public class EmployeeDetailVM {
        
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        public PositionTitle Position { get; set; }
        
        public string EmployeeCode { get; set; }
        
        public bool IsActive { get; set; }
        
        public DateTime? JointOn { get; set; }
        
        public DateTime? LeftOn { get; set; }
    }
}