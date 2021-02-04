using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class Todo
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? GroupId { get; set; }
        public bool IsShared { get; set; }
        public bool Emphasized { get; set; }
        public string CoverImage { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Details { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? DueDate { get; set; }
        public int? DoneById { get; set; }
        public DateTime? ActuallyDoneOn { get; set; }
        public DateTime? DeletedOn { get; set; }

        public virtual User DoneBy { get; set; }
        public virtual ContentGroup Group { get; set; }
        public virtual User User { get; set; }
    }
}
