using System;

namespace RoutinizeCore.ViewModels.Cooperation {

    public sealed class CooperationLog {
        
        public int CooperationId { get; set; }
        
        public string Activity { get; set; }
        
        public DateTime Timestamp { get; set; }
    }

    public sealed class CooperationParticipantLog {
        
        public int ParticipantId { get; set; }
        
        public string Activity { get; set; }
        
        public DateTime Timestamp { get; set; }
    }
}