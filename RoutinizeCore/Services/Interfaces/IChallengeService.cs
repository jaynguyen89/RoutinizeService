using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using RoutinizeCore.Models;
using RoutinizeCore.ViewModels.Challenge;

namespace RoutinizeCore.Services.Interfaces {

    public interface IChallengeService {

        Task<ChallengeQuestionVM[]> GetChallengeQuestions();

        Task<ChallengeRecordVM[]> GetChallengeResponsesForAccount([NotNull] int accountId);

        Task<bool?> SaveChallengeRecordsForAccount([NotNull] int accountId, [NotNull] ChallengeResponseVM[] challengeResponses);

        Task<bool?> VerifyChallengeProofFor([NotNull] int accountId,[NotNull] ChallengeResponseVM challengeResponse);

        Task<ChallengeQuestion> GetRandomChallengeQuestionForAccount([NotNull] int accountId);

        Task<bool?> UpdateChallengeRecords([NotNull] ChallengeRecordVM[] newResponses);

        Task<bool?> DeleteChallengeRecords([NotNull] ChallengeRecordVM[] removedChallengeResponses);
    }
}