namespace RoutinizeCore.Models {

    public partial class AppSetting {

        public AppSetting() {
            Theme = 3;
            IsPremium = false;
            TodoUnlocked = false;
            NoteUnlocked = false;
            CollabUnlocked = false;
            ShouldHideAds = false;
            PremiumUntil = null;
            UnlockedUntil = null;
            DateTimeFormat = "FRIENDLY_DMY";
            UnitSystem = 0;
            AddressFormat = 0;
        }
    }
}