namespace RoutinizeCore.ViewModels.Account {

    public sealed class EmailUpdateLog {
        
        public int AccountId { get; set; }
        
        public string Activity { get; set; }
        
        public string EmailBeingReplaced { get; set; }

        public bool IsCompleted { get; set; } = false;
    }
}