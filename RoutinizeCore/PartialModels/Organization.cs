using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HelperLibrary;
using HelperLibrary.Shared;
using Newtonsoft.Json;
using RoutinizeCore.ViewModels.Organization;

namespace RoutinizeCore.Models {

    public partial class Organization {

        public List<string> VerifyOrganizationData() {
            var errors = VerifyOrganizationFullName();
            errors.AddRange(VerifyOrganizationShortName());
            errors.AddRange(VerifyRegistrationNumber());
            errors.AddRange(VerifyRegistrationCode());
            errors.AddRange(VerifyWebsiteData());
            errors.AddRange(VerifyPhoneNumberData());
            errors.AddRange(VerifyBriefDescription());
            CheckOrganizationDetails();

            if (Address == null) return errors;
            
            var addressErrors = Address.VerifyAddressData();
            errors.AddRange(Address.GenerateErrorMessages(addressErrors));

            return errors;
        }

        private List<string> VerifyOrganizationFullName() {
            if (!Helpers.IsProperString(FullName)) return new List<string>() { "Organization name is missing." };

            FullName = Helpers.CapitalizeFirstLetterOfEachWord(FullName.Trim());
            var errors = new List<string>();

            var lenTest = new Regex(@".{3,100}");
            if (!lenTest.IsMatch(FullName))
                errors.Add("Organization name is too " + (FullName.Length > 60 ? "long. Max 100 characters." : "short. Min 3 characters."));

            var spTest = new Regex(@"^[\w_\-.'() ]*$");
            if (!spTest.IsMatch(FullName))
                errors.Add("Organization name can only contain these special characters: _-.(')");

            return errors;
        }

        private List<string> VerifyOrganizationShortName() {
            if (!Helpers.IsProperString(ShortName)) {
                ShortName = null;
                return default;
            }

            ShortName = Helpers.CapitalizeFirstLetterOfEachWord(ShortName.Trim());
            var errors = new List<string>();

            var lenTest = new Regex(@".{3,60}");
            if (!lenTest.IsMatch(ShortName))
                errors.Add("Organization alias is too " + (ShortName.Length > 30 ? "long. Max 60 characters." : "short. Min 3 characters."));

            var spTest = new Regex(@"^[\w_\-.'() ]*$");
            if (!spTest.IsMatch(ShortName))
                errors.Add("Organization alias can only contain these special characters: _-.(')");

            return errors;
        }

        private List<string> VerifyRegistrationNumber() {
            if (!Helpers.IsProperString(RegistrationNumber)) {
                RegistrationNumber = null;
                return default;
            }

            RegistrationNumber = RegistrationNumber.Trim().Replace(SharedConstants.ALL_SPACES, SharedConstants.MONO_SPACE);
            var errors = new List<string>();

            var lenTest = new Regex(@".{3,30}");
            if (!lenTest.IsMatch(RegistrationNumber))
                errors.Add("Registration number is too " + (RegistrationNumber.Length > 30 ? "long. Max 30 characters." : "short. Min 3 characters."));

            var spTest = new Regex(@"^[\w_\-. ]*$");
            if (!spTest.IsMatch(RegistrationNumber))
                errors.Add("Registration number can only contain these special characters: _-.");

            return errors;
        }

        private List<string> VerifyRegistrationCode() {
            if (!Helpers.IsProperString(RegistrationCode)) {
                RegistrationCode = null;
                return default;
            }

            RegistrationCode = RegistrationCode.Trim().Replace(SharedConstants.ALL_SPACES, SharedConstants.MONO_SPACE);
            var errors = new List<string>();

            var lenTest = new Regex(@".{3,30}");
            if (!lenTest.IsMatch(RegistrationCode))
                errors.Add("Registration code is too " + (RegistrationNumber.Length > 20 ? "long. Max 30 characters." : "short. Min 3 characters."));

            var spTest = new Regex(@"^[\w_\-. ]*$");
            if (!spTest.IsMatch(RegistrationNumber))
                errors.Add("Registration code can only contain these special characters: _-.");

            return errors;
        }

        private List<string> VerifyWebsiteData() {
            if (!Helpers.IsProperString(Websites)) {
                Websites = null;
                return default;
            }

            List<WebsiteVM> websiteData;
            try {
                websiteData = JsonConvert.DeserializeObject<List<WebsiteVM>>(Websites);
            } catch (Exception) {
                return new List<string>() { "Websites data are improperly formatted." };
            }

            return websiteData.Select(website => website.VerifyWebsite())
                              .FirstOrDefault(errors => errors.Count != 0);
        }

        private List<string> VerifyPhoneNumberData() {
            if (!Helpers.IsProperString(PhoneNumbers)) {
                PhoneNumbers = null;
                return default;
            }

            List<PhoneNumberVM> phoneNumberData;
            try {
                phoneNumberData = JsonConvert.DeserializeObject<List<PhoneNumberVM>>(PhoneNumbers);
            } catch (Exception) {
                return new List<string>() { "Phone number data are improperly formatted." };
            }
            
            return phoneNumberData.Select(number => number.VerifyPhoneNumberData())
                                  .FirstOrDefault(errors => errors.Count != 0);
        }

        private List<string> VerifyBriefDescription() {
            if (!Helpers.IsProperString(BriefDescription)) {
                BriefDescription = null;
                return default;
            }

            BriefDescription = BriefDescription.Trim();
            return BriefDescription.Length <= 1000 ? default : new List<string>() { "Brief description is too long. Max 1000 characters." };
        }

        private void CheckOrganizationDetails() {
            FullDetails = FullDetails?.Trim();
        }

        public void UpdateDataBy(Organization organization) {
            MotherId = organization.MotherId;
            AddressId = organization.AddressId;
            IndustryId = organization.IndustryId;
            FullName = organization.FullName;
            ShortName = organization.ShortName;
            RegistrationNumber = organization.RegistrationNumber;
            RegistrationCode = organization.RegistrationCode;
            FoundedOn = organization.FoundedOn;
            Websites = organization.Websites;
            PhoneNumbers = organization.PhoneNumbers;
            BriefDescription = organization.BriefDescription;
            FullDetails = organization.FullDetails;
        }
    }
}