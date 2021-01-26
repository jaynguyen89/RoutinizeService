namespace RoutinizeCore.ViewModels.Account {

    public sealed class TwoFaUpdateVM {
        
        public int AccountId { get; set; }
        
        public string Password { get; set; }
        
        public string RecaptchaToken { get; set; }
    }
}