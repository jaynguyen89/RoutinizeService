using System;

namespace HelperLibrary.Shared {

    public static class SharedEnums {

        public enum SessionKeys {
            [StringValue("IsUserAuthenticated")]
            IsUserAuthenticated,
            [StringValue("AuthenticatedUser")]
            AuthenticatedUser
        }

        public enum ActionFilterResults {
            RequestProcessingError,
            UnauthenticatedRequest
        }

        public enum RequestResults {
            Failed,
            Success,
            Denied,
            Partial
        }

        public enum LogSeverity {
            [StringValue("Verbose")]
            Verbose,
            [StringValue("Information")]
            Information,
            [StringValue("Low")]
            Low,
            [StringValue("Caution")]
            Caution,
            [StringValue("High")]
            High,
            [StringValue("Fatal")]
            Fatal
        }

        public enum DateTimeFormats {
            [StringValue("dd-MM-yyyy HH:mm")]
            COMPACT_H_DMY,
            [StringValue("dd-MM-yyyy hh:mm tt")]
            COMPACT_T_DMY
        }
        
        public enum RedisCacheKeys {
            [StringValue("Challenge_Questions_List")]
            ChallengeQuestions
        }

        public enum Permissions {
            Read,
            Edit,
            Delete
        }

        public enum Relationships {
            RelatesTo,
            Blocks,
            Causes,
            DependsOn,
            Verifies,
            Resolves
        }

        public enum AttachmentTypes {
            [StringValue("image")]
            Photo = 0,
            [StringValue("video")]
            Video = 1,
            [StringValue("audio")]
            Audio = 2,
            [StringValue("file")]
            File = 11,
            [StringValue("address")]
            Address = 12
        }

        public enum HttpStatusCodes {
            Continue = 100,
            SwitchingProtocol = 101,
            Processing = 102,
            EarlyHints = 103,
            Ok = 200,
            Created = 201,
            Accepted = 202,
            NonauthoritativeInformation = 203,
            NoContent = 204,
            ResetContent = 205,
            PartialContent = 206,
            MultiStatus = 207,
            AlreadyReported = 208,
            ImUsed = 226,
            MultipleChoices = 300,
            MovedPermanently = 301,
            Found = 302, //previously Moved Temporarily
            SeeOther = 303,
            NotModified = 304,
            UseProxy = 305,
            SwitchProxy = 306,
            TemporaryRedirect = 307,
            PermanentRedirect = 308,
            BadRequest = 400,
            Unauthorized = 401,
            PaymentRequired = 402,
            Forbidden = 403,
            NotFound = 404,
            MethodNotAllowed = 405,
            NotAcceptable = 406,
            ProxyAuthenticationRequired = 407,
            RequestTimeout = 408,
            Conflict = 409,
            Gone = 410,
            LengthRequired = 411,
            PreconditionFailed = 412,
            PayloadTooLarge = 413,
            UriTooLong = 414,
            UnsupportedMediaType = 415,
            RangeNotSatisfiable = 416,
            ExpectationFailed = 417,
            ImATeapot = 418,
            MisredirectedRequest = 421,
            UnprocessableEntity = 422,
            Locked = 423,
            FailedDependency = 424,
            TooEarly = 425,
            UpgradeRequired = 426,
            PreconditionRequired = 428,
            TooManyRequests = 429,
            RequestHeaderFieldsTooLarge = 431,
            UnavailableForLegalReasons = 451,
            InternalServerError = 500,
            NotImplemented = 501,
            BadGateway = 502,
            ServiceUnavailable = 503,
            GatewayTimeout = 504,
            HttpVersionNotSupported = 505,
            VariantAlsoNegotiates = 506,
            InsufficientStorage = 507,
            LoopDetected = 508,
            NotExtended = 510,
            NetworkAuthenticationRequired = 511,
            InvalidSslCertificate = 526
        }
    }

    public sealed class StringValueAttribute : Attribute {
        
        public string Value { get; set; }

        public StringValueAttribute(string name) {
            Value = name;
        }
    }
}