using System.Collections.Generic;
using System.IO;

namespace HelperLibrary.Shared {

    public static class SharedConstants {

        public const int PreferedRsaKeyLength = 1024;

        public const int CacheAbsoluteExpiration = 3600; //seconds
        
        public const int TwoFaSecretKeyLength = 32;
        public const string ProjectName = "RoutinizeCore_JQBTM";
        public const int TwoFaQrImageSize = 300;
        public const int TwoFaTolerance = 3; //minutes

        public const int DefaultUniqueIdLength = 16;
        public const char AccountUniqueIdDelimiter = '-';
        public const int AccountUniqueIdGroupLength = 4;

        public const int AccountActivationTokenLength = 30;

        public static readonly string EmailTemplatesDirectory =
            Path.GetDirectoryName(Directory.GetCurrentDirectory()) + @"/RoutinizeCore/EmailTemplates/";

        public const int AccountActivationEmailValidityDuration = 24; //hours
        public const int UntrustedAuthenticationExpiryDuration = 3; //hours
        public const int TrustedAuthenticationExpiryDuration = 720; //~30days

        public const int DaysPerYear = 365;
        public const int QuartersPerYear = 4;
        public const int DaysPerMonth = 30;
        public const int DaysPerWeek = 7;
        public const int MonthsPerYear = 12;
        public const int MonthsPerQuarter = 3;
        public const int WeeksPerYear = 52;
        public const int WeeksPerMonth = 4;
        public const int HoursPerDay = 24;
        public const int MinutesPerHour = 60;
        public const int SecondsPerMinute = 60;
        public const int TicksPerSecond = 100;
        public const int MillisPerSecond = 1000;

        public static readonly List<string> InvalidEmailTokens = new() {
            "--", "_@", "-@", ".-", "-.", "._", "_.", " ", "@_", "@-", "__", "..", "_-", "-_"
        };

        public const int ImageFileMaxSize = 2000000; //2MB
        public static readonly List<string> ImageTypes = new() {
            "image/gif", "image/png", "image/jpg", "image/jpeg"
        };

        public const string Na = "N/A";
        public const string AllSpaces = @"\s+";
        public const string DoubleSpace = "  ";
        public const string MonoSpace = " ";
        public const string Fslash = "/";
        public const string Bslash = "\\";
        
        public static readonly Dictionary<string, string> ApiTokenTargets = new() {
            { "save_avatar", "Avatar/saveAvatar" },
            { "replace_avatar", "Avatar/replaceAvatar" },
            { "remove_avatar", "Avatar/removeAvatar" },
            { "save-cover", "Photo/saveCover" },
            { "replace-cover", "Photo/replaceCover" },
            { "remove-cover", "Photo/removeCover" },
            { "save-attachments", "Attachment/saveAttachments" },
            { "remove-attachments", "Attachment/removeAttachments" }
        };

        public static readonly Dictionary<string, string> ContentTypes = new() {
            { "json", "application/json" },
            { "form", "multipart/form-data" },
            { "xml", "application/xml" },
            { "mixed", "multipart/mixed" },
            { "alt", "multipart/alternative" },
            { "base64", "application/base64" }
        };

        public const string TaskInsert = "INSERT";
        public const string TaskUpdate = "UPDATE";
        public const string TaskDelete = "DELETE";

        public const string DefaultDepartmentRole = "Owner (default)";
        public const string DefaultPositionTitle = "CEO (default)";

        public const int NameLengthForCloseMatch = 10;
        public const int RegistrationNoLengthForCloseMatch = 8;
        public const int UniqueIdLengthForCloseMatch = 6;

        public const int OwnerHierarchyIndex = 0;

        public const string ResponseGettingDataIssue = "An issue happened while getting data.";
        public const string ResponseSavingDataIssue = "An issue happened while saving data.";
        public const string ResponseUpdatingDataIssue = "An issue happened while updating data.";
        public const string ResponseRemovingDataIssue = "An issue happened while removing data.";
        public const string ResponseAuthorizationIssue = "You are not authorized for this action.";
    }
}