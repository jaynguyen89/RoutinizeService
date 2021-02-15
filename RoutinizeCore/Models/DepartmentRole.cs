using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class DepartmentRole
    {
        public DepartmentRole()
        {
            UserDepartments = new HashSet<UserDepartment>();
            UserOrganizations = new HashSet<UserOrganization>();
        }

        public int Id { get; set; }
        public int AddedById { get; set; }
        public string RoleName { get; set; }
        public bool IsManagerialRole { get; set; }
        public string Description { get; set; }
        public byte EnumValue { get; set; }
        public string ForDepartmentIds { get; set; }
        public DateTime AddedOn { get; set; }
        public bool AllowCreateDepartment { get; set; }
        public bool AllowEditDepartment { get; set; }
        public bool AllowDeleteDepartment { get; set; }
        public bool AllowCreateDepartmentRole { get; set; }
        public bool AllowEditDepartmentRole { get; set; }
        public bool AllowDeleteDepartmentRole { get; set; }
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

        public virtual User AddedBy { get; set; }
        public virtual ICollection<UserDepartment> UserDepartments { get; set; }
        public virtual ICollection<UserOrganization> UserOrganizations { get; set; }
    }
}
