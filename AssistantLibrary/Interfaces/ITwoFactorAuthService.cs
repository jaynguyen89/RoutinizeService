using System.Diagnostics.CodeAnalysis;
using AssistantLibrary.Models;
using HelperLibrary.Shared;

namespace AssistantLibrary.Interfaces {

    public interface ITwoFactorAuthService {

        TwoFactorAuth GetTwoFactorAuthSetup(
            [NotNull] string secretKey,
            [NotNull] string email,
            [NotNull] string projectName = SharedConstants.PROJECT_NAME,
            [NotNull] int imageSize = SharedConstants.TWO_FA_QR_IMAGE_SIZE
        );

        bool VerifyTwoFactorAuth([NotNull] string secretKey,[NotNull] string pin);
    }
}