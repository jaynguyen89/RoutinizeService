using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class ChallengeQuestion
    {
        public ChallengeQuestion()
        {
            ChallengeRecords = new HashSet<ChallengeRecord>();
        }

        public int Id { get; set; }
        public string Question { get; set; }
        public DateTime AddedOn { get; set; }

        public virtual ICollection<ChallengeRecord> ChallengeRecords { get; set; }
    }
}
