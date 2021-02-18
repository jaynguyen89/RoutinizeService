namespace RoutinizeCore.ViewModels.Organization {

    public sealed class SearchOrganizationResultVM {

        public class OrganizationSearchVM {
            
            public int Id { get; set; }
            
            public string Name { get; set; }
        }
    
        public OrganizationSearchVM[] ClosestMatches { get; set; }
        
        public OrganizationSearchVM[] OtherMatches { get; set; }
    }
}