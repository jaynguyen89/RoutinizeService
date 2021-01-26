using System.Collections.Generic;

namespace RoutinizeCore.ViewModels.Challenge {

    public sealed class AccountChallengeVM {
        
        public int AccountId { get; set; }
        
        public ChallengeResponseVM[] ChallengeResponses { get; set; }
    }
}