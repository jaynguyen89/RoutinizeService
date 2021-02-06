using System.Collections.Generic;
using System.Text.RegularExpressions;
using HelperLibrary;
using HelperLibrary.Shared;

namespace RoutinizeCore.Models {

    public partial class Address {

        public List<int> VerifyAddressData() {
            var errors = VerifyAddressName();
            errors.AddRange(VerifyBuilding());
            errors.AddRange(VerifyStreet());
            errors.AddRange(VerifySuburb());
            errors.AddRange(VerifyPostcode());
            errors.AddRange(VerifyState());
            errors.AddRange(VerifyCountry());

            return errors;
        }

        public List<string> GenerateErrorMessages(List<int> errors) {
            var messages = new List<string>();

            //For Building
            if (errors.Contains(0)) messages.Add("Building Name is too " + (Building.Length < 1 ? "short." : "long."));
            if (errors.Contains(1)) messages.Add("Building Name con only contain these special characters: -.,'()");
            
            //For StreetAddress
            if (errors.Contains(2)) messages.Add("Street Address is missing.");
            if (errors.Contains(3)) messages.Add("Street Address is too " + (Street.Length < 1 ? "short." : "long."));
            if (errors.Contains(4)) messages.Add("Street Address can only contain these special characters: -.,'()");
            
            //For Address Name
            if (errors.Contains(5)) messages.Add("Address Name is too " + (Name.Length < 1 ? "short." : "long."));
            if (errors.Contains(6)) messages.Add("Address Name can only contain these special characters: -.,'()");
            
            //For Suburb
            if (errors.Contains(7)) messages.Add("Suburb is missing. This field is required.");
            if (errors.Contains(8)) messages.Add("Suburb is too " + (Suburb.Length < 1 ? "short." : "long."));
            if (errors.Contains(9)) messages.Add("Suburb can only contains these special characters: '.-");

            //For Postcode
            if (errors.Contains(10)) messages.Add("Postcode is missing.");
            if (errors.Contains(11)) messages.Add("Postcode is too " + (Postcode.Length < 1 ? "short." : "long."));
            if (errors.Contains(12)) messages.Add("Postcode can only contain digits.");

            //For State
            if (errors.Contains(13)) messages.Add("State is missing.");
            if (errors.Contains(14)) messages.Add("State is too " + (State.Length < 1 ? "short." : "long."));
            if (errors.Contains(15)) messages.Add("State can only contain these special characters: '.-");
            
            //For Country
            if (errors.Contains(16)) messages.Add("Country is missing.");
            if (errors.Contains(17)) messages.Add("Country is too " + (Country.Length < 1 ? "short." : "long."));
            if (errors.Contains(18)) messages.Add("Country can only contain these special characters: '.-");

            return messages;
        }

        private List<int> VerifyBuilding() {
            var errors = new List<int>();
            if (Building == null) return errors;

            Building = Building.Trim().Replace(SharedConstants.DOUBLE_SPACE, SharedConstants.MONO_SPACE);
            if (!Helpers.IsProperString(Building)) {
                Building = null;
                return errors;
            }

            Building = Helpers.CapitalizeFirstLetterOfEachWord(Building);

            var lenTest = new Regex(@".{1,30}");
            if (!lenTest.IsMatch(Building))
                errors.Add(0);

            var rx = new Regex(@"^[A-Za-z\d\-.,'() ]*$");
            if (!rx.IsMatch(Building))
                errors.Add(1);

            return errors;
        }

        private List<int> VerifyStreet() {
            var errors = new List<int>();
            if (Street == null) return new List<int> { 2 };

            Street = Street.Trim().Replace(SharedConstants.DOUBLE_SPACE, SharedConstants.MONO_SPACE);
            if (!Helpers.IsProperString(Street))
                return new List<int> { 2 };

            Street = Helpers.CapitalizeFirstLetterOfEachWord(Street);

            var lenTest = new Regex(@".{1,50}");
            if (!lenTest.IsMatch(Street))
                errors.Add(3);

            var rx = new Regex(@"^[A-Za-z\d\-/.,'() ]*$");
            if (!rx.IsMatch(Street))
                errors.Add(4);

            return errors;
        }
        
        private List<int> VerifyAddressName() {
            if (!Helpers.IsProperString(Name)) {
                Name = null;
                return new List<int>();
            }
            
            Name = Helpers.CapitalizeFirstLetterOfEachWord(Name.Trim());
            var errors = new List<int>();

            var lenTest = new Regex(@".{1,50}");
            if (!lenTest.IsMatch(Name))
                errors.Add(5);

            var spTest = new Regex(@"^[A-Za-z0-9_\-.'() ]*$");
            if (!spTest.IsMatch(Name))
                errors.Add(6);

            return errors;
        }
        
        private List<int> VerifySuburb() {
            var errors = new List<int>();
            if (Suburb == null) return new List<int> { 7 };

            Suburb = Suburb.Trim().Replace(SharedConstants.DOUBLE_SPACE, SharedConstants.MONO_SPACE);
            if (!Helpers.IsProperString(Suburb))
                return new List<int> { 7 };

            Suburb = Helpers.CapitalizeFirstLetterOfEachWord(Suburb);

            var lenTest = new Regex(@".{1,30}");
            if (!lenTest.IsMatch(Suburb))
                errors.Add(8);

            var rx = new Regex(@"^[A-Za-z'.\- ]*$");
            if (!rx.IsMatch(Suburb))
                errors.Add(9);

            return errors;
        }

        private List<int> VerifyPostcode() {
            var errors = new List<int>();
            if (Postcode == null) return new List<int> { 10 };

            Postcode = Postcode.Trim().Replace(SharedConstants.DOUBLE_SPACE, SharedConstants.MONO_SPACE);
            if (!Helpers.IsProperString(Postcode))
                return new List<int> { 10 };

            var lenTest = new Regex(@".{1,10}");
            if (!lenTest.IsMatch(Postcode))
                errors.Add(11);

            var rx = new Regex(@"^[\d]*$");
            if (!rx.IsMatch(Postcode))
                errors.Add(12);

            return errors;
        }

        private List<int> VerifyState() {
            var errors = new List<int>();
            if (State == null) return new List<int> { 13 };

            State = State.Trim().Replace(SharedConstants.DOUBLE_SPACE, SharedConstants.MONO_SPACE);
            if (!Helpers.IsProperString(State))
                return new List<int> { 13 };

            State = Helpers.CapitalizeFirstLetterOfEachWord(State);

            var lenTest = new Regex(@".{1,30}");
            if (!lenTest.IsMatch(State))
                errors.Add(14);

            var rx = new Regex(@"^[A-Za-z'.\- ]*$");
            if (!rx.IsMatch(State))
                errors.Add(15);

            return errors;
        }
        
        private List<int> VerifyCountry() {
            var errors = new List<int>();
            if (Country == null) return new List<int> { 16 };

            Country = Country.Trim().Replace(SharedConstants.DOUBLE_SPACE, SharedConstants.MONO_SPACE);
            if (!Helpers.IsProperString(Country))
                return new List<int> { 16 };

            Country = Helpers.CapitalizeFirstLetterOfEachWord(Country);

            var lenTest = new Regex(@".{1,30}");
            if (!lenTest.IsMatch(Country))
                errors.Add(17);

            var rx = new Regex(@"^[A-Za-z'.\- ]*$");
            if (!rx.IsMatch(Country))
                errors.Add(18);

            return errors;
        }

        public string Normalize() {
            var building = string.IsNullOrEmpty(Building) ? string.Empty : Building + ", ";
            var street = string.IsNullOrEmpty(Street) ? string.Empty : Street + ", ";
            var suburb = string.IsNullOrEmpty(Suburb) ? string.Empty : Suburb + ", ";
            var state = string.IsNullOrEmpty(State) ? string.Empty : State + " ";
            var post = string.IsNullOrEmpty(Postcode) ? string.Empty : Postcode + ", ";
            var country = string.IsNullOrEmpty(Country) ? string.Empty : Country;

            return $"{ building }{ street }{ suburb }{ state }{ post }{ country }";
        }
    }
}