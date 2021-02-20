using System;
using Newtonsoft.Json;
using RoutinizeCore.Models;
using RoutinizeCore.ViewModels.User;

namespace RoutinizeCore.ViewModels.Organization {

    public class OrganizationVM {
        
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public string Logo { get; set; }

        public static implicit operator OrganizationVM(Models.Organization organization) {
            return new() {
                Id = organization.Id,
                Name = organization.FullName,
                Logo = organization.LogoName
            };
        }
    }

    public sealed class OrganizationDetailVM : OrganizationVM {

        public MotherVM Mother { get; set; }
        
        public AddressVM Address { get; set; }
        
        public IndustryVM Industry { get; set; }
        
        public string UniqueId { get; set; }
        
        public string ShortName { get; set; }
        
        public string RegistrationNumber { get; set; }
        
        public string RegistrationCode { get; set; }
        
        public DateTime? FoundedOn { get; set; }
        
        public WebsiteVM Websites { get; set; }
        
        public PhoneNumberVM PhoneNumbers { get; set; }
        
        public string Introduction { get; set; }
        
        public string Details { get; set; }
        
        public sealed class MotherVM {
            
            public int Id { get; set; }
            
            public string Name { get; set; }

            public static implicit operator MotherVM(Models.Organization organization) {
                return new() {
                    Id = organization.Id,
                    Name = organization.FullName
                };
            }
        }
        
        public sealed class IndustryVM {
            
            public int Id { get; set; }
            
            public string Name { get; set; }

            public static implicit operator IndustryVM(Industry industry) {
                return new() {
                    Id = industry.Id,
                    Name = industry.Name
                };
            }
        }

        public static implicit operator OrganizationDetailVM(Models.Organization organization) {
            return new() {
                Id = organization.Id,
                Name = organization.FullName,
                Logo = organization.LogoName,
                Mother = organization.Mother,
                Address = organization.Address,
                Industry = organization.Industry,
                UniqueId = organization.UniqueId,
                ShortName = organization.ShortName,
                RegistrationNumber = organization.RegistrationNumber,
                RegistrationCode = organization.RegistrationCode,
                FoundedOn = organization.FoundedOn,
                Websites = JsonConvert.DeserializeObject<WebsiteVM>(organization.Websites),
                PhoneNumbers = JsonConvert.DeserializeObject<PhoneNumberVM>(organization.PhoneNumbers),
                Introduction = organization.BriefDescription,
                Details = organization.FullDetails
            };
        }
    }
}