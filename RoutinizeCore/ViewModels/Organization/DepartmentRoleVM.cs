using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RoutinizeCore.Models;
using RoutinizeCore.ViewModels.User;

namespace RoutinizeCore.ViewModels.Organization {

    public sealed class GetRolesVM {
        
        public ManagerRoleVM[] ManagerialRoles { get; set; }
        
        public EmployeeRoleVM[] EmployeeRoles { get; set; }
    }

    public sealed class EmployeeRoleVM : GenericRoleVM {
        
        public CommonPermissionVM Permissions { get; set; }

        public static implicit operator EmployeeRoleVM(DepartmentRole role) {
            return new() {
                Id = role.Id,
                RoleName = role.RoleName,
                AddedByUser = role.AddedBy,
                Description = role.Description,
                HierarchyIndex = role.HierarchyIndex,
                AddedOn = role.AddedOn,
                Permissions = role
            };
        }
    }

    public sealed class ManagerRoleVM : GenericRoleVM {
        
        public AllPermissionsVM Permissions { get; set; }

        public static implicit operator ManagerRoleVM(DepartmentRole role) {
            return new() {
                Id = role.Id,
                RoleName = role.RoleName,
                AddedByUser = role.AddedBy,
                Description = role.Description,
                HierarchyIndex = role.HierarchyIndex,
                AddedOn = role.AddedOn,
                Permissions = role
            };
        }
    }

    public class GenericRoleVM {
        
        public int Id { get; set; }
        
        public UserVM AddedByUser { get; set; }
        
        public string RoleName { get; set; }
        
        public string Description { get; set; }
        
        public byte HierarchyIndex { get; set; }
        
        public List<GenericDepartmentVM> ForDepartments { get; set; }
        
        public DateTime? AddedOn { get; set; }

        public static implicit operator GenericRoleVM(DepartmentRole role) {
            return new() {
                Id = role.Id,
                AddedByUser = role.AddedBy,
                RoleName = role.RoleName,
                Description = role.Description,
                HierarchyIndex = role.HierarchyIndex,
                AddedOn = role.AddedOn
            };
        }
    }

    public sealed class AllPermissionsVM : CommonPermissionVM {
        
        public bool AllowManageCooperation { get; set; }
        
        public bool AllowCreateDepartment { get; set; }
        
        public bool AllowEditDepartment { get; set; }
        
        public bool AllowDeleteDepartment { get; set; }
        
        public bool AllowCreateDepartmentRole { get; set; }
        
        public bool AllowEditDepartmentRole { get; set; }
        
        public bool AllowDeleteDepartmentRole { get; set; }
        
        public bool AllowAddUserOrganization { get; set; }
        
        public bool AllowEditUserOrganization { get; set; }
        
        public bool AllowDeleteUserOrganization { get; set; }
        
        public static implicit operator AllPermissionsVM(DepartmentRole role) {
            return new() {
                AllowManageCooperation = role.AllowManageCooperation,
                AllowCreateDepartment = role.AllowCreateDepartment,
                AllowEditDepartment = role.AllowEditDepartment,
                AllowDeleteDepartment = role.AllowDeleteDepartment,
                AllowCreateDepartmentRole = role.AllowCreateDepartmentRole,
                AllowEditDepartmentRole = role.AllowEditDepartmentRole,
                AllowDeleteDepartmentRole = role.AllowDeleteDepartmentRole,
                AllowAddUserOrganization = role.AllowAddUserOrganization,
                AllowEditUserOrganization = role.AllowEditUserOrganization,
                AllowDeleteUserOrganization = role.AllowDeleteUserOrganization,
                AllowAddUserDepartment = role.AllowAddUserDepartment,
                AllowEditUserDepartment = role.AllowEditUserDepartment,
                AllowDeleteUserDepartment = role.AllowDeleteUserDepartment,
                AllowAddTeamDepartment = role.AllowAddTeamDepartment,
                AllowEditTeamDepartment = role.AllowEditTeamDepartment,
                AllowDeleteTeamDepartment = role.AllowDeleteTeamDepartment,
                AllowAddProject = role.AllowAddProject,
                AllowEditProject = role.AllowEditProject,
                AllowDeleteProject = role.AllowDeleteProject,
                AllowConstraintProject = role.AllowConstraintProject
            };
        }
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

        public static implicit operator CommonPermissionVM(DepartmentRole role) {
            return new() {
                AllowAddUserDepartment = role.AllowAddUserDepartment,
                AllowEditUserDepartment = role.AllowEditUserDepartment,
                AllowDeleteUserDepartment = role.AllowDeleteUserDepartment,
                AllowAddTeamDepartment = role.AllowAddTeamDepartment,
                AllowEditTeamDepartment = role.AllowEditTeamDepartment,
                AllowDeleteTeamDepartment = role.AllowDeleteTeamDepartment,
                AllowAddProject = role.AllowAddProject,
                AllowEditProject = role.AllowEditProject,
                AllowDeleteProject = role.AllowDeleteProject,
                AllowConstraintProject = role.AllowConstraintProject
            };
        }
    }
}