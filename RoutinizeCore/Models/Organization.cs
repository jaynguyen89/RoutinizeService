using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class Organization
    {
        public Organization()
        {
            DepartmentRoles = new HashSet<DepartmentRole>();
            Departments = new HashSet<Department>();
            InverseMother = new HashSet<Organization>();
            PositionTitles = new HashSet<PositionTitle>();
            TeamDepartments = new HashSet<TeamDepartment>();
            UserOrganizations = new HashSet<UserOrganization>();
        }

        public int Id { get; set; }
        public int? MotherId { get; set; }
        public int? AddressId { get; set; }
        public int? IndustryId { get; set; }
        public string UniqueId { get; set; }
        public string LogoName { get; set; }
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public string RegistrationNumber { get; set; }
        public string RegistrationCode { get; set; }
        public DateTime? FoundedOn { get; set; }
        public string Websites { get; set; }
        public string PhoneNumbers { get; set; }
        public string BriefDescription { get; set; }
        public string FullDetails { get; set; }

        public virtual Address Address { get; set; }
        public virtual Industry Industry { get; set; }
        public virtual Organization Mother { get; set; }
        public virtual ICollection<DepartmentRole> DepartmentRoles { get; set; }
        public virtual ICollection<Department> Departments { get; set; }
        public virtual ICollection<Organization> InverseMother { get; set; }
        public virtual ICollection<PositionTitle> PositionTitles { get; set; }
        public virtual ICollection<TeamDepartment> TeamDepartments { get; set; }
        public virtual ICollection<UserOrganization> UserOrganizations { get; set; }
    }
}
