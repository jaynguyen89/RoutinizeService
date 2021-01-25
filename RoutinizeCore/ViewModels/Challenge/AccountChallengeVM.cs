using System.Collections.Generic;

namespace RoutinizeCore.ViewModels.Challenge {

    public sealed class AccountChallengeVM {
        
        public int AccountId { get; set; }
        
        public List<ChallengeResponseVM> ChallengeResponses { get; set; }
    }
}