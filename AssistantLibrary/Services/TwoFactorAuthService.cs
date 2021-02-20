using System;
using System.Diagnostics.CodeAnalysis;
using AssistantLibrary.Interfaces;
using AssistantLibrary.Models;
using Google.Authenticator;
using HelperLibrary;
using HelperLibrary.Shared;

namespace AssistantLibrary.Services {

    public sealed class TwoFactorAuthService : ITwoFactorAuthService {

        private readonly TwoFactorAuthenticator _authenticator;

        public TwoFactorAuthService() {
            _authenticator = new TwoFactorAuthenticator();
        }

        public TwoFactorAuth GetTwoFactorAuthSetup(
            [NotNull] string secretKey,
            [NotNull] string email,
            [NotNull] string projectName = SharedConstants.ProjectName,
            [NotNull] int imageSize = SharedConstants.TwoFaQrImageSize
        ) {
            var authenticator = _authenticator.GenerateSetupCode(
                projectName, email, secretKey, true, imageSize
            );

            return new TwoFactorAuth {
                QrCodeImageUrl = authenticator.QrCodeSetupImageUrl,
                ManualEntryKey = authenticator.ManualEntryKey
            };
        }

        public bool VerifyTwoFactorAuth([NotNull] string secretKey, [NotNull] string pin) {
            return _authenticator.ValidateTwoFactorPIN(secretKey, pin, TimeSpan.FromMinutes(SharedConstants.TwoFaTolerance));
        }
    }
}