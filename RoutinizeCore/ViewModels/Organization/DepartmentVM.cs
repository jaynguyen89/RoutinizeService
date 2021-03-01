using RoutinizeCore.Models;

namespace RoutinizeCore.ViewModels.Organization {
    
    public class GenericDepartmentVM {
            
        public int Id { get; set; }
        
        public string Name { get; set; }

        public string Avatar { get; set; }
        
        public static implicit operator GenericDepartmentVM(Department department) {
            return new() {
                Id = department.Id,
                Name = department.Name,
                Avatar = department.Avatar
            };
        }
    }
    
    public sealed class DepartmentVM : GenericDepartmentVM {
        
        public OrganizationVM Organization { get; set; }
        
        public GenericDepartmentVM Parent { get; set; }
        
        public string Description { get; set; }
        
        public DepartmentContactVM ContactDetail { get; set; }
        
        public static implicit operator DepartmentVM(Department department) {
            return new() {
                Id = department.Id,
                Name = department.Name,
                Avatar = department.Avatar,
                Organization = department.Organization,
                Parent = department.Parent,
                Description = department.Description,
                ContactDetail = department.ContactDetails
            };
        }
    }
}