using System;

namespace RoutinizeCore.ViewModels.Challenge {

    public sealed class ChallengeQuestionVM {
        
        public int Id { get; set; }
        
        public string Question { get; set; }
        
        public DateTime AddedOn { get; set; }
    }
}