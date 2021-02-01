﻿using System.Collections.Generic;
using System.IO;

namespace HelperLibrary.Shared {

    public static class SharedConstants {

        public const int CACHE_ABSOLUTE_EXPIRATION = 3600; //seconds
        
        public const int TWO_FA_SECRET_KEY_LENGTH = 32;
        public const string PROJECT_NAME = "RoutinizeCore_JQBTM";
        public const int TWO_FA_QR_IMAGE_SIZE = 300;
        public const int TWO_FA_TOLERANCE = 3; //minutes

        public const int ACCOUNT_UNIQUE_ID_LENGTH = 16;
        public const char ACCOUNT_UNIQUE_ID_DELIMITER = '-';
        public const int ACCOUNT_UNIQUE_ID_GROUP_LENGTH = 4;

        public const int ACCOUNT_ACTIVATION_TOKEN_LENGTH = 30;

        public static readonly string EMAIL_TEMPLATES_DIRECTORY =
            Path.GetDirectoryName(Directory.GetCurrentDirectory()) + @"/RoutinizeCore/EmailTemplates/";

        public const int ACCOUNT_ACTIVATION_EMAIL_VALIDITY_DURATION = 24; //hours
        public const int UNTRUSTED_AUTHENTICATION_EXPIRY_DURATION = 3; //hours
        public const int TRUSTED_AUTHENTICATION_EXPIRY_DURATION = 720; //~30days

        public const int DAYS_PER_YEAR = 365;
        public const int QUARTERS_PER_YEAR = 4;
        public const int DAYS_PER_MONTH = 30;
        public const int DAYS_PER_WEEK = 7;
        public const int MONTHS_PER_YEAR = 12;
        public const int MONTHS_PER_QUARTER = 3;
        public const int WEEKS_PER_YEAR = 52;
        public const int WEEKS_PER_MONTH = 4;
        public const int HOURS_PER_DAY = 24;
        public const int MINUTES_PER_HOUR = 60;
        public const int SECONDS_PER_MINUTE = 60;
        public const int TICKS_PER_SECOND = 100;
        public const int MILLIS_PER_SECOND = 1000;

        public static readonly List<string> INVALID_EMAIL_TOKENS = new List<string> {
            "--", "_@", "-@", ".-", "-.", "._", "_.", " ", "@_", "@-", "__", "..", "_-", "-_"
        };

        public const int IMAGE_FILE_MAX_SIZE = 2000000; //2MB
        public static readonly List<string> IMAGE_TYPES = new List<string> {
            "image/gif", "image/png", "image/jpg", "image/jpeg"
        };

        public const string ALL_SPACES = @"\s+";
        public const string DOUBLE_SPACE = "  ";
        public const string MONO_SPACE = " ";
    }
}