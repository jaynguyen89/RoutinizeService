namespace RoutinizeCore.ViewModels.Authentication {

    public sealed class AuthenticatedUser {
        
        public string AuthenticationToken { get; set; }
        
        public int AuthenticatedUserId { get; set; }
        
        public string UserEmail { get; set; }
        
        public string ValidityDuration { get; set; }
        
        public string AuthenticationTimestamp { get; set; }
    }
}