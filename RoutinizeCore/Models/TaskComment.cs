using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class TaskComment
    {
        public TaskComment()
        {
            InverseRepliedTo = new HashSet<TaskComment>();
        }

        public int Id { get; set; }
        public int CommentedOnId { get; set; }
        public string CommentedOnType { get; set; }
        public int CommenterId { get; set; }
        public int CommenterReferenceId { get; set; }
        public string CommenterReferenceType { get; set; }
        public int? RepliedToId { get; set; }
        public string Content { get; set; }
        public DateTime CommentOn { get; set; }
        public DateTime? LastEdittedOn { get; set; }
        public byte EdittedCount { get; set; }

        public virtual TaskComment RepliedTo { get; set; }
        public virtual ICollection<TaskComment> InverseRepliedTo { get; set; }
    }
}
