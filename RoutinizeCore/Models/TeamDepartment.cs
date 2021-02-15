using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class TeamDepartment
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public int OrganizationId { get; set; }
        public int? DepartmentId { get; set; }
        public bool ForCooperation { get; set; }

        public virtual Department Department { get; set; }
        public virtual Organization Organization { get; set; }
        public virtual Team Team { get; set; }
    }
}
