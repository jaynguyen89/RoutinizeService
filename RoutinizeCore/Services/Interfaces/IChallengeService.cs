using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using RoutinizeCore.Models;
using RoutinizeCore.ViewModels.Challenge;

namespace RoutinizeCore.Services.Interfaces {

    public interface IChallengeService {

        Task<List<ChallengeQuestion>> GetChallengeQuestions();

        Task<List<ChallengeRecord>> GetChallengeRecordsForAccount([NotNull] int accountId);

        Task<bool?> SaveChallengeRecordsForAccount([NotNull] AccountChallengeVM challengeRecord);

        Task<bool?> VerifyChallengeProofFor([NotNull] int accountId,[NotNull] ChallengeResponseVM challengeResponse);

        Task<ChallengeQuestion> GetRandomChallengeQuestionForAccount([NotNull] int accountId);
    }
}