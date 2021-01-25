﻿using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class ChallengeRecord
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public int AccountId { get; set; }
        public string Response { get; set; }
        public DateTime RecordedOn { get; set; }

        public virtual Account Account { get; set; }
        public virtual ChallengeQuestion Question { get; set; }
    }
}
