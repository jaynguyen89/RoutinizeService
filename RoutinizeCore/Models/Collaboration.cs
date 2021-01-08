using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class Collaboration
    {
        public Collaboration()
        {
            CollaboratorTasks = new HashSet<CollaboratorTask>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public int CollaboratorId { get; set; }
        public DateTime InvitedOn { get; set; }
        public bool IsAccepted { get; set; }
        public DateTime AcceptedOn { get; set; }
        public DateTime RejectedOn { get; set; }

        public virtual User Collaborator { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<CollaboratorTask> CollaboratorTasks { get; set; }
    }
}
