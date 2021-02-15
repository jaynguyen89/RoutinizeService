using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class Address
    {
        public Address()
        {
            Attachments = new HashSet<Attachment>();
            Organizations = new HashSet<Organization>();
            Users = new HashSet<User>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Building { get; set; }
        public string Street { get; set; }
        public string Suburb { get; set; }
        public string Postcode { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }

        public virtual ICollection<Attachment> Attachments { get; set; }
        public virtual ICollection<Organization> Organizations { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
