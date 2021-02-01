using System;
using System.Collections.Generic;

#nullable disable

namespace MediaLibrary.Models
{
    public partial class Photo
    {
        public Photo()
        {
            Userphotos = new HashSet<Userphoto>();
        }

        public int Id { get; set; }
        public string PhotoName { get; set; }
        public string Location { get; set; }

        public virtual ICollection<Userphoto> Userphotos { get; set; }
    }
}
