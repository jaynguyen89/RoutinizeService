using System.Collections.Generic;
using System.Threading.Tasks;
using RoutinizeCore.Models;
using RoutinizeCore.ViewModels.Challenge;

namespace RoutinizeCore.Services.Interfaces {

    public interface IChallengeService {

        Task<List<ChallengeQuestion>> GetChallengeQuestions();

        Task<List<ChallengeRecord>> GetChallengeRecordsForAccount(int accountId);

        Task<bool?> SaveChallengeRecordsForAccount(AccountChallengeVM challengeRecord);

        Task<bool?> VerifyChallengeProofFor(int accountId, ChallengeResponseVM challengeResponse);

        Task<ChallengeQuestion> GetRandomChallengeQuestionForAccount(int accountId);
    }
}