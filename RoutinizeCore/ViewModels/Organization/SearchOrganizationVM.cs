using HelperLibrary;
using HelperLibrary.Shared;

namespace RoutinizeCore.ViewModels.Organization {
    
    public sealed class OrganizationSearchDataVM {
            
        public string Keyword { get; set; } // In client, only send search request when Keyword is longer than 3 characters
            
        public bool ByOrganizationName { get; set; }
            
        public bool ByRegistrationNumber { get; set; }
            
        public bool ByUniqueId { get; set; }

        public string[] VerifyKeyword() {
            if (!Helpers.IsProperString(Keyword)) return new[] { "No keyword to search." };
                
            Keyword = Keyword.ToLower().Trim().Replace(SharedConstants.AllSpaces, SharedConstants.MonoSpace);
            return default;
        }
    }

    public sealed class SearchOrganizationVM {

        public class OrganizationSearchResultVM {
            
            public int Id { get; set; }
            
            public string Name { get; set; }
            
            public string Logo { get; set; }

            public static implicit operator OrganizationSearchResultVM(Models.Organization organization) {
                return new() {
                    Id = organization.Id,
                    Name = $"{ organization.FullName } - { organization.ShortName }",
                    Logo = organization.LogoName
                };
            }
        }
    
        public OrganizationSearchResultVM[] ClosestMatches { get; set; }
        
        public OrganizationSearchResultVM[] OtherMatches { get; set; }
    }
}