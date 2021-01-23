namespace RoutinizeCore.ViewModels.Authentication {

    public sealed class AccountActivationVM {
        
        public string Email { get; set; }

        public string ActivationToken { get; set; }

        public string RecaptchaToken { get; set; }
    }
}