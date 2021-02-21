using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class UserOrganization
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int DepartmentRoleId { get; set; }
        public int OrganizationId { get; set; }
        public int PositionId { get; set; }
        public string EmployeeCode { get; set; }
        public DateTime? JointOn { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LeftOn { get; set; }

        public virtual DepartmentRole DepartmentRole { get; set; }
        public virtual Organization Organization { get; set; }
        public virtual PositionTitle Position { get; set; }
        public virtual User User { get; set; }
    }
}
