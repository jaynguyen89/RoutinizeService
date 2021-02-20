using System.Diagnostics.CodeAnalysis;
using AssistantLibrary.Models;
using HelperLibrary.Shared;

namespace AssistantLibrary.Interfaces {

    public interface ITwoFactorAuthService {

        TwoFactorAuth GetTwoFactorAuthSetup(
            [NotNull] string secretKey,
            [NotNull] string email,
            [NotNull] string projectName = SharedConstants.ProjectName,
            [NotNull] int imageSize = SharedConstants.TwoFaQrImageSize
        );

        bool VerifyTwoFactorAuth([NotNull] string secretKey,[NotNull] string pin);
    }
}