namespace RoutinizeCore.ViewModels.Account {

    public sealed class TwoFaUpdateVM {

        public string Password { get; set; }
        
        public string RecaptchaToken { get; set; }
    }
}