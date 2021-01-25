namespace RoutinizeCore.ViewModels.Authentication {

    public class PasswordRecoveryVM {
        
        public string Email { get; set; }
        
        public string Username { get; set; }
        
        public string RecaptchaToken { get; set; }
    }
}