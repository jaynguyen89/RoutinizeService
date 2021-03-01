using RoutinizeCore.ViewModels.Organization;

namespace RoutinizeCore.ViewModels.Cooperation {

    public sealed class DepartmentAccessVM {
        
        public int Id { get; set; }
        
        public int CooperationId { get; set; }
        
        public int FromParticipantId { get; set; }
        
        public int AccessGivenToParticipantId { get; set; }
        
        public int[] AccessibleDepartmentIds { get; set; }
        
        public GenericDepartmentVM[] AccessibleDepartments { get; set; }
    }

    public sealed class AccessibleDepartmentVM {
        
        public OrganizationVM Organization { get; set; }
        
        public GenericDepartmentVM[] Departments { get; set; }
    }
}