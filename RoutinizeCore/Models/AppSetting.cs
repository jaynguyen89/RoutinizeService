using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class AppSetting
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public byte Theme { get; set; }
        public bool IsPremium { get; set; }
        public bool TodoUnlocked { get; set; }
        public bool NoteUnlocked { get; set; }
        public bool CollabUnlocked { get; set; }
        public bool ShouldHideAds { get; set; }
        public DateTime? PremiumUntil { get; set; }
        public DateTime? UnlockedUntil { get; set; }
        public string DateTimeFormat { get; set; }
        public byte UnitSystem { get; set; }
        public byte AddressFormat { get; set; }

        public virtual User User { get; set; }
    }
}
