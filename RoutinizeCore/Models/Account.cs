using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class Account
    {
        public Account()
        {
            AuthRecords = new HashSet<AuthRecord>();
            ChallengeRecords = new HashSet<ChallengeRecord>();
            Users = new HashSet<User>();
        }

        public int Id { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string Username { get; set; }
        public string UniqueId { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public int AccessFailedCount { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public string TwoFaSecretKey { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? DeactivatedOn { get; set; }
        public string RecoveryToken { get; set; }
        public DateTime? TokenSetOn { get; set; }
        public string FcmToken { get; set; }

        public virtual ICollection<AuthRecord> AuthRecords { get; set; }
        public virtual ICollection<ChallengeRecord> ChallengeRecords { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
