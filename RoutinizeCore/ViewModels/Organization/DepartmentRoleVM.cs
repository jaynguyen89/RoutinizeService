using System;
using RoutinizeCore.ViewModels.User;

namespace RoutinizeCore.ViewModels.Organization {

    public sealed class GetRolesVM {
        
        public ManagerRoleVM ManagerialRoles { get; set; }
        
        public EmployeeRoleVM EmployeeRoles { get; set; }
    }

    public sealed class EmployeeRoleVM : GenericRoleVM {
        
        public CommonPermissionVM Permissions { get; set; }
    }

    public sealed class ManagerRoleVM : GenericRoleVM {
        
        public AllPermissionsVM Permissions { get; set; }
    }

    public class GenericRoleVM {
        
        public int Id { get; set; }
        
        public UserVM AddedByUser { get; set; }
        
        public string RoleName { get; set; }
        
        public string Description { get; set; }
        
        public DateTime? AddedOn { get; set; }
    }

    public sealed class AllPermissionsVM : CommonPermissionVM {
        
        public bool AllowCreateDepartment { get; set; }
        
        public bool AllowEditDepartment { get; set; }
        
        public bool AllowDeleteDepartment { get; set; }
        
        public bool AllowCreateDepartmentRole { get; set; }
        
        public bool AllowEditDepartmentRole { get; set; }
        
        public bool AllowDeleteDepartmentRole { get; set; }
        
        public bool AllowAddUserOrganization { get; set; }
        
        public bool AllowEditUserOrganization { get; set; }
        
        public bool AllowDeleteUserOrganization { get; set; }
    }

    public class CommonPermissionVM {
        
        public bool AllowAddUserDepartment { get; set; }
        
        public bool AllowEditUserDepartment { get; set; }
        
        public bool AllowDeleteUserDepartment { get; set; }
        
        public bool AllowAddTeamDepartment { get; set; }
        
        public bool AllowEditTeamDepartment { get; set; }
        
        public bool AllowDeleteTeamDepartment { get; set; }
        
        public bool AllowAddProject { get; set; }
        
        public bool AllowEditProject { get; set; }
        
        public bool AllowDeleteProject { get; set; }
        
        public bool AllowConstraintProject { get; set; }
    }
}