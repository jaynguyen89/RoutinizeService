namespace RoutinizeCore.ViewModels.Organization {

    public sealed class DepartmentVM {
        
        public int Id { get; set; }
        
        public string Avatar { get; set; }
        
        public OrganizationVM Organization { get; set; }
        
        public ParentVM Parent { get; set; }
        
        public string Description { get; set; }
        
        public DepartmentContactVM ContactDetail { get; set; }
        
        public sealed class ParentVM {
            
            public int Id { get; set; }
            
            public string Name { get; set; }
        }
    }
}