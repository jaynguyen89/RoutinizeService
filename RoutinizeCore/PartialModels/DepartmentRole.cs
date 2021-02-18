using System;
using System.Collections.Generic;
using HelperLibrary;
using HelperLibrary.Shared;
using RoutinizeCore.ViewModels.Organization;

namespace RoutinizeCore.Models {

    public partial class DepartmentRole {

        public string[] VerifyRoleData() {
            var errors = VerifyRoleName();
            errors.AddRange(VerifyDescription());

            return errors.ToArray();
        }

        private List<string> VerifyRoleName() {
            if (!Helpers.IsProperString(RoleName)) return new List<string> { "Role name is missing." };

            RoleName = RoleName.Trim().Replace(SharedConstants.ALL_SPACES, SharedConstants.MONO_SPACE);
            return RoleName.Length > 100 ? new List<string> { "Role name is too long. Max 100 characters." } : default;
        }

        private List<string> VerifyDescription() {
            if (!Helpers.IsProperString(Description)) {
                Description = null;
                return default;
            }

            Description = Description.Trim().Replace(SharedConstants.ALL_SPACES, SharedConstants.MONO_SPACE);
            Description = Helpers.CapitalizeFirstLetterOfSentence(Description);
            return Description.Length > 300 ? new List<string> { "Role description is too long. Max 300 characters." } : default;
        }

        public static DepartmentRole GetDefaultManagerialInstance() {
            return new DepartmentRole {
                RoleName = SharedConstants.DEFAULT_DEPARTMENT_ROLE,
                IsManagerialRole = true,
                Description = "Default Owner role is created when user creates an organization. This role is assigned, and grants all permissions, to that user by default.",
                //EnumValue = 0,
                AddedOn = DateTime.UtcNow,
                AllowCreateDepartment = true,
                AllowEditDepartment = true,
                AllowDeleteDepartment = true,
                AllowCreateDepartmentRole = true,
                AllowEditDepartmentRole = true,
                AllowDeleteDepartmentRole = true,
                AllowAddUserOrganization = true,
                AllowEditUserOrganization = true,
                AllowDeleteUserOrganization = true,
                AllowAddUserDepartment = true,
                AllowEditUserDepartment = true,
                AllowDeleteUserDepartment = true,
                AllowAddTeamDepartment = true,
                AllowEditTeamDepartment = true,
                AllowDeleteTeamDepartment = true,
                AllowAddProject = true,
                AllowEditProject = true,
                AllowDeleteProject = true,
                AllowConstraintProject = true
            };
        }

        public static implicit operator DepartmentRole(ManagerRoleVM role) {
            return new DepartmentRole {
                RoleName = role.RoleName,
                IsManagerialRole = true,
                AddedOn = DateTime.UtcNow,
                AllowCreateDepartment = role.Permissions.AllowCreateDepartment,
                AllowEditDepartment = role.Permissions.AllowEditDepartment,
                AllowDeleteDepartment = role.Permissions.AllowDeleteDepartment,
                AllowCreateDepartmentRole = role.Permissions.AllowCreateDepartmentRole,
                AllowEditDepartmentRole = role.Permissions.AllowEditDepartmentRole,
                AllowDeleteDepartmentRole = role.Permissions.AllowDeleteDepartmentRole,
                AllowAddUserOrganization = role.Permissions.AllowAddUserOrganization,
                AllowEditUserOrganization = role.Permissions.AllowEditUserOrganization,
                AllowDeleteUserOrganization = role.Permissions.AllowDeleteUserOrganization,
                AllowAddUserDepartment = role.Permissions.AllowAddUserDepartment,
                AllowEditUserDepartment = role.Permissions.AllowEditUserDepartment,
                AllowDeleteUserDepartment = role.Permissions.AllowDeleteUserDepartment,
                AllowAddTeamDepartment = role.Permissions.AllowAddTeamDepartment,
                AllowEditTeamDepartment = role.Permissions.AllowEditTeamDepartment,
                AllowDeleteTeamDepartment = role.Permissions.AllowDeleteTeamDepartment,
                AllowAddProject = role.Permissions.AllowAddProject,
                AllowEditProject = role.Permissions.AllowEditProject,
                AllowDeleteProject = role.Permissions.AllowDeleteProject,
                AllowConstraintProject = role.Permissions.AllowConstraintProject
            };
        }
        
        public static implicit operator DepartmentRole(EmployeeRoleVM role) {
            return new DepartmentRole {
                RoleName = role.RoleName,
                IsManagerialRole = true,
                AddedOn = DateTime.UtcNow,
                AllowAddUserDepartment = role.Permissions.AllowAddUserDepartment,
                AllowEditUserDepartment = role.Permissions.AllowEditUserDepartment,
                AllowDeleteUserDepartment = role.Permissions.AllowDeleteUserDepartment,
                AllowAddTeamDepartment = role.Permissions.AllowAddTeamDepartment,
                AllowEditTeamDepartment = role.Permissions.AllowEditTeamDepartment,
                AllowDeleteTeamDepartment = role.Permissions.AllowDeleteTeamDepartment,
                AllowAddProject = role.Permissions.AllowAddProject,
                AllowEditProject = role.Permissions.AllowEditProject,
                AllowDeleteProject = role.Permissions.AllowDeleteProject,
                AllowConstraintProject = role.Permissions.AllowConstraintProject
            };
        }

        public void UpdateDataBy(DepartmentRole newRoleData) {
            RoleName = newRoleData.RoleName;
            IsManagerialRole = newRoleData.IsManagerialRole;
            Description = newRoleData.Description;
            ForDepartmentIds = newRoleData.ForDepartmentIds;

            AllowCreateDepartment = newRoleData.AllowCreateDepartment;
            AllowEditDepartment = newRoleData.AllowEditDepartment;
            AllowDeleteDepartment = newRoleData.AllowDeleteDepartment;
            AllowCreateDepartmentRole = newRoleData.AllowCreateDepartmentRole;
            AllowEditDepartmentRole = newRoleData.AllowEditDepartmentRole;
            AllowDeleteDepartmentRole = newRoleData.AllowDeleteDepartmentRole;
            AllowAddUserOrganization = newRoleData.AllowAddUserOrganization;
            AllowEditUserOrganization = newRoleData.AllowEditUserOrganization;
            AllowDeleteUserOrganization = newRoleData.AllowDeleteUserOrganization;
            AllowAddUserDepartment = newRoleData.AllowAddUserDepartment;
            AllowEditUserDepartment = newRoleData.AllowEditUserDepartment;
            AllowDeleteUserDepartment = newRoleData.AllowDeleteUserDepartment;
            AllowAddTeamDepartment = newRoleData.AllowAddTeamDepartment;
            AllowEditTeamDepartment = newRoleData.AllowEditTeamDepartment;
            AllowDeleteTeamDepartment = newRoleData.AllowDeleteTeamDepartment;
            AllowAddProject = newRoleData.AllowAddProject;
            AllowEditProject = newRoleData.AllowEditProject;
            AllowDeleteProject = newRoleData.AllowDeleteProject;
            AllowConstraintProject = newRoleData.AllowConstraintProject;
        }
    }
}