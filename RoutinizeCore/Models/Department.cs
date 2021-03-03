using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class Department
    {
        public Department()
        {
            CooperationTaskVaults = new HashSet<CooperationTaskVault>();
            InverseParent = new HashSet<Department>();
            TeamDepartments = new HashSet<TeamDepartment>();
            UserDepartments = new HashSet<UserDepartment>();
        }

        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public int? ParentId { get; set; }
        public string Avatar { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ContactDetails { get; set; }
        public bool ForCooperation { get; set; }

        public virtual Organization Organization { get; set; }
        public virtual Department Parent { get; set; }
        public virtual ICollection<CooperationTaskVault> CooperationTaskVaults { get; set; }
        public virtual ICollection<Department> InverseParent { get; set; }
        public virtual ICollection<TeamDepartment> TeamDepartments { get; set; }
        public virtual ICollection<UserDepartment> UserDepartments { get; set; }
    }
}
