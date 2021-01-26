using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using HelperLibrary;
using HelperLibrary.Shared;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using MongoLibrary.Interfaces;
using MongoLibrary.Models;
using RoutinizeCore.DbContexts;
using RoutinizeCore.Models;
using RoutinizeCore.Services.ApplicationServices.CacheService;
using RoutinizeCore.Services.Interfaces;
using RoutinizeCore.ViewModels.Challenge;

namespace RoutinizeCore.Services.DatabaseServices {

    public sealed class ChallengeService : IChallengeService {

        private readonly IRoutinizeCoreLogService _coreLogService;
        private readonly RoutinizeDbContext _dbContext;
        private readonly IRoutinizeRedisCache _redisCache;
        

        public ChallengeService(
            IRoutinizeCoreLogService coreLogService,
            RoutinizeDbContext dbContext,
            IRoutinizeRedisCache redisCache
        ) {
            _coreLogService = coreLogService;
            _dbContext = dbContext;
            _redisCache = redisCache;
        }

        public async Task<List<ChallengeQuestion>> GetChallengeQuestions() {
            var challengeQuestions = await _redisCache.GetRedisCacheEntry<List<ChallengeQuestion>>(SharedEnums.RedisCacheKeys.ChallengeQuestions.GetEnumValue());
            if (challengeQuestions != null) return challengeQuestions;

            try {
                challengeQuestions = await _dbContext.ChallengeQuestions.ToListAsync();
                if (challengeQuestions != null)
                    await _redisCache.InsertRedisCacheEntry<ChallengeQuestion>(new CacheEntry {
                        Data = challengeQuestions,
                        EntryKey = SharedEnums.RedisCacheKeys.ChallengeQuestions.GetEnumValue()
                    });

                return challengeQuestions;
            }
            catch (ArgumentNullException e) {
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();
                
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(ChallengeService) }.{ nameof(GetChallengeQuestions) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Unable to get all ChallengeQuestions.\n\n{ e.StackTrace }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<List<ChallengeRecord>> GetChallengeRecordsForAccount([NotNull] int accountId) {
            try {
                var challengeRecords = await _dbContext.ChallengeRecords.Where(record => record.AccountId == accountId).ToListAsync();
                return challengeRecords ?? new List<ChallengeRecord>();
            }
            catch (ArgumentNullException e) {
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();
                
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(ChallengeService) }.{ nameof(GetChallengeRecordsForAccount) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Unable to get challenge records.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(accountId) } = { accountId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> SaveChallengeRecordsForAccount([NotNull] AccountChallengeVM challengeRecord) {
            try {
                challengeRecord.ChallengeResponses.ForEach(async response => {
                    var record = new ChallengeRecord {
                        AccountId = challengeRecord.AccountId,
                        QuestionId = response.QuestionId,
                        Response = response.Response,
                        RecordedOn = DateTime.UtcNow
                    };

                    await _dbContext.ChallengeRecords.AddAsync(record);
                });

                var result = await _dbContext.SaveChangesAsync();
                return result != 0;
            }
            catch (DbUpdateException e) {
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();
                
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(ChallengeService) }.{ nameof(SaveChallengeRecordsForAccount) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while saving challenge records.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(challengeRecord) } = { JsonConvert.SerializeObject(challengeRecord) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> VerifyChallengeProofFor([NotNull] int accountId, [NotNull] ChallengeResponseVM challengeResponse) {
            try {
                var challengeRecordByAccount = await _dbContext.ChallengeRecords.SingleOrDefaultAsync(
                    record => record.QuestionId == challengeResponse.QuestionId &&
                              record.AccountId == accountId
                );

                return challengeRecordByAccount?.Response.Equals(challengeResponse.Response.Trim());
            }
            catch (ArgumentNullException e) {
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();
                
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(ChallengeService) }.{ nameof(VerifyChallengeProofFor) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while getting ChallengeRecord caused by null argument.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(accountId) } = { accountId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
            catch (InvalidOperationException e) {
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();
                
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(ChallengeService) }.{ nameof(VerifyChallengeProofFor) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while getting ChallengeRecords as >1 entry matches predicate.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(accountId) } = { accountId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<ChallengeQuestion> GetRandomChallengeQuestionForAccount([NotNull] int accountId) {
            var challengeQuestionsRespondedByAccount = 
                await _dbContext.ChallengeRecords
                    .Where(challengeRecord => challengeRecord.AccountId == accountId)
                    .Select(challengeRecord => challengeRecord.ChallengeQuestion)
                    .Distinct()
                    .OrderBy(question => question.Question)
                    .ToArrayAsync();

            return challengeQuestionsRespondedByAccount[Helpers.GetRandomNumber(challengeQuestionsRespondedByAccount.Length - 1)];
        }
    }
}