using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class PositionTitle
    {
        public PositionTitle()
        {
            UserDepartments = new HashSet<UserDepartment>();
            UserOrganizations = new HashSet<UserOrganization>();
        }

        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime AddedOn { get; set; }

        public virtual Organization Organization { get; set; }
        public virtual ICollection<UserDepartment> UserDepartments { get; set; }
        public virtual ICollection<UserOrganization> UserOrganizations { get; set; }
    }
}
