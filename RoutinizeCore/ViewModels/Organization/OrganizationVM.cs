using System;
using RoutinizeCore.ViewModels.User;

namespace RoutinizeCore.ViewModels.Organization {

    public class OrganizationVM {
        
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public string Logo { get; set; }
    }

    public sealed class OrganizationDetailVM : OrganizationVM {

        public MotherVM Mother { get; set; }
        
        public AddressVM Address { get; set; }
        
        public IndustryVM Industry { get; set; }
        
        public string UniqueId { get; set; }
        
        public string ShortName { get; set; }
        
        public string RegistrationNumber { get; set; }
        
        public string RegistrationCode { get; set; }
        
        public DateTime FoundedOn { get; set; }
        
        public WebsiteVM Websites { get; set; }
        
        public PhoneNumberVM PhoneNumbers { get; set; }
        
        public string Introduction { get; set; }
        
        public string Details { get; set; }
        
        public sealed class MotherVM {
            
            public int Id { get; set; }
            
            public string Name { get; set; }
        }
        
        public sealed class IndustryVM {
            
            public int Id { get; set; }
            
            public string Name { get; set; }
        }
    }
}