namespace AssistantLibrary.Models {

    public sealed class TwoFactorAuth {
        
        public string QrCodeImageUrl { get; set; }
        
        public string ManualEntryKey { get; set; }
    }
}