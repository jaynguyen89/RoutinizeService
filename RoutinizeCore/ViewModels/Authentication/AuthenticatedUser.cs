namespace RoutinizeCore.ViewModels.Authentication {

    public class AuthenticatedUser {
        
        public int AccountId { get; set; }
        
        public string AuthToken { get; set; }

        private bool TrustedAuth;

        public void SetTrustedAuth(bool trustedAuth) {
            TrustedAuth = trustedAuth;
        }

        public bool GetTrustedAuth() {
            return TrustedAuth;
        }
    }
}