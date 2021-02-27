using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HelperLibrary;
using Newtonsoft.Json;
using RoutinizeCore.ViewModels.Organization;

namespace RoutinizeCore.Models {

    public partial class Department {

        public List<string> VerifyDepartmentData() {
            var errors = VerifyDepartmentName();
            errors.AddRange(VerifyContactDetails());
            CheckDescription();

            return errors;
        }

        private List<string> VerifyDepartmentName() {
            if (!Helpers.IsProperString(Name)) return new List<string>() { "Department name is missing." };

            Name = Helpers.CapitalizeFirstLetterOfEachWord(Name.Trim());
            var errors = new List<string>();

            var lenTest = new Regex(@".{3,60}");
            if (!lenTest.IsMatch(Name))
                errors.Add("Department name is too " + (Name.Length > 100 ? "long. Max 100 characters." : "short. Min 3 characters."));

            var spTest = new Regex(@"^[\w_\-.'() ]*$");
            if (!spTest.IsMatch(Name))
                errors.Add("Department name can only contain these special characters: _-.(')");

            return errors;
        }

        private List<string> VerifyContactDetails() {
            if (!Helpers.IsProperString(ContactDetails)) {
                ContactDetails = null;
                return default;
            }

            DepartmentContactVM contactDetails;
            try {
                contactDetails = JsonConvert.DeserializeObject<DepartmentContactVM>(ContactDetails);
            } catch (Exception) {
                return new List<string>() { "Contact Details data are improperly formatted." };
            }

            return contactDetails.VerifyContactDetails();
        }

        private void CheckDescription() {
            Description = Description?.Trim();
        }

        public void UpdateDataBy(Department department) {
            Name = department.Name;
            Description = department.Description;
            ContactDetails = department.ContactDetails;
            ForCooperation = department.ForCooperation;
        }
    }
}