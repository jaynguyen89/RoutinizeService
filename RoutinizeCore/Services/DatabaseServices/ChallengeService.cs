using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using HelperLibrary;
using HelperLibrary.Shared;
using Microsoft.EntityFrameworkCore;
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

        public async Task<ChallengeQuestionVM[]> GetChallengeQuestions() {
            var challengeQuestions = await _redisCache.GetRedisCacheEntry<ChallengeQuestionVM[]>(SharedEnums.RedisCacheKeys.ChallengeQuestions.GetEnumValue());
            if (challengeQuestions != null) return challengeQuestions;

            try {
                challengeQuestions = await _dbContext.ChallengeQuestions
                                                     .Select(question => new ChallengeQuestionVM {
                                                         Id = question.Id,
                                                         Question = question.Question,
                                                         AddedOn = question.AddedOn
                                                     })
                                                     .ToArrayAsync();
                
                if (challengeQuestions != null)
                    await _redisCache.InsertRedisCacheEntry(new CacheEntry {
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

        public async Task<ChallengeRecordVM[]> GetChallengeResponsesForAccount([NotNull] int accountId) {
            try {
                var challengeRecords =
                    await _dbContext.ChallengeRecords
                                    .Where(record => record.AccountId == accountId)
                                    .Join(
                                        _dbContext.ChallengeQuestions,
                                        challengeRecord => challengeRecord.QuestionId,
                                        challengeQuestion => challengeQuestion.Id, 
                                        (challengeRecord, challengeQuestion) => new { challengeRecord, challengeQuestion }
                                    )
                                    .Where(challengeResponse => 
                                        challengeResponse.challengeRecord.AccountId == accountId
                                    )
                                    .Select(challengeResponse =>
                                        new ChallengeRecordVM {
                                            QuestionId = challengeResponse.challengeQuestion.Id,
                                            Question = challengeResponse.challengeQuestion.Question,
                                            RecordId = challengeResponse.challengeRecord.Id,
                                            Response = challengeResponse.challengeRecord.Response
                                        }
                                    )
                                    .ToArrayAsync();
                
                return challengeRecords;
            }
            catch (ArgumentNullException e) {
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();
                
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(ChallengeService) }.{ nameof(GetChallengeResponsesForAccount) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Unable to get challenge records.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(accountId) } = { accountId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> SaveChallengeRecordsForAccount([NotNull] int accountId, [NotNull] ChallengeResponseVM[] challengeResponses) {
            try {
                Array.ForEach(
                    challengeResponses,
                    async response => {
                    var record = new ChallengeRecord {
                        AccountId = accountId,
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
                    DetailedInformation = $"Error while saving challenge records for account.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(accountId) } = { accountId }",
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
            try {
                var challengeQuestionsRespondedByAccount =
                    await _dbContext.ChallengeRecords
                                    .Where(challengeRecord => challengeRecord.AccountId == accountId)
                                    .Select(challengeRecord => challengeRecord.ChallengeQuestion)
                                    .Distinct()
                                    .OrderBy(question => question.Question)
                                    .ToArrayAsync();

                return challengeQuestionsRespondedByAccount[Helpers.GetRandomNumber(challengeQuestionsRespondedByAccount.Length - 1)];
            }
            catch (ArgumentNullException e) {
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();
                
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(ChallengeService) }.{ nameof(GetRandomChallengeQuestionForAccount) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Unable to get random challenge record.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(accountId) } = { accountId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> UpdateChallengeRecords([NotNull] ChallengeRecordVM[] newResponses) {
            try {
                var error = false;
                Array.ForEach(
                    newResponses,
                    async response => {
                        if (error) return;
                        
                        var updateResult = await UpdateChallengeRecordSingle(response);
                        if (!updateResult.HasValue || !updateResult.Value) error = true;
                    }
                );

                return !error;
            }
            catch (ArgumentNullException e) {
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();
                
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(ChallengeService) }.{ nameof(UpdateChallengeRecords) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating challenge records.\n\n{ e.StackTrace }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        private async Task<bool?> UpdateChallengeRecordSingle(ChallengeRecordVM newResponse) {
            try {
                var dbChallengeRecord = await _dbContext.ChallengeRecords.FindAsync(newResponse.RecordId);
                if (dbChallengeRecord == null) return false;

                dbChallengeRecord.Response = newResponse.Response;
                dbChallengeRecord.RecordedOn = DateTime.UtcNow;

                _dbContext.Update(dbChallengeRecord);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();
                
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(ChallengeService) }.{ nameof(UpdateChallengeRecordSingle) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while saving challenge records.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(newResponse) }.RecordId = { newResponse.RecordId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> DeleteChallengeRecords(ChallengeRecordVM[] removedChallengeResponses) {
            try {
                var error = false;
                var challengeRecordsToRemove = new List<ChallengeRecord>();
                Array.ForEach(
                    removedChallengeResponses,
                    async responseToRemove => {
                        if (error) return;
                        var dbChallengeRecord = await _dbContext.ChallengeRecords.FindAsync(responseToRemove.RecordId);
                        
                        error = dbChallengeRecord == null;
                        if (!error) challengeRecordsToRemove.Add(dbChallengeRecord);
                    }
                );

                if (error) return false;
                _dbContext.RemoveRange(challengeRecordsToRemove);

                return await _dbContext.SaveChangesAsync() != 0;
            }
            catch (DbUpdateException e) {
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();
                
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(ChallengeService) }.{ nameof(DeleteChallengeRecords) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while saving db changes.\n\n{ e.StackTrace }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
            catch (ArgumentNullException e) {
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();
                
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(ChallengeService) }.{ nameof(DeleteChallengeRecords) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while iterating null array.\n\n{ e.StackTrace }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }
    }
}