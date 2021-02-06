using System;
using RoutinizeCore.Models;

namespace RoutinizeCore.ViewModels.User {

    public class AddressVM {
        
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public string Building { get; set; }
        
        public string Street { get; set; }
        
        public string Suburb { get; set; }
        
        public string Postcode { get; set; }
        
        public string State { get; set; }
        
        public string Country { get; set; }
        
        public Coordination Location { get; set; }

        public string NormalizedAddress { get; set; }

        public static implicit operator AddressVM(Address address) {
            return address == null
                ? null
                : new AddressVM {
                    Id = address.Id,
                    Name = address.Name,
                    Building = address.Building,
                    Street = address.Street,
                    Suburb = address.Suburb,
                    Postcode = address.Postcode,
                    State = address.State,
                    Country = address.Country,
                    Location = new Coordination {
                        Latitude = Convert.ToDouble(address.Latitude),
                        Longitude = Convert.ToDouble(address.Longitude)
                    },
                    NormalizedAddress = address.Normalize()
                };
        }
    }
    
    public sealed class Coordination {
        
        public double Latitude { get; set; }
        
        public double Longitude { get; set; }
    }
}