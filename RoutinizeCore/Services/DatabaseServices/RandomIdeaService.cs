using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HelperLibrary;
using HelperLibrary.Shared;
using Microsoft.EntityFrameworkCore;
using MongoLibrary.Interfaces;
using MongoLibrary.Models;
using Newtonsoft.Json;
using RoutinizeCore.DbContexts;
using RoutinizeCore.Models;
using RoutinizeCore.Services.Interfaces;

namespace RoutinizeCore.Services.DatabaseServices {

    public sealed class RandomIdeaService : IRandomIdeaService {

        private readonly IRoutinizeCoreLogService _coreLogService;
        private readonly RoutinizeDbContext _dbContext;

        public RandomIdeaService(
            IRoutinizeCoreLogService coreLogService,
            RoutinizeDbContext dbContext
        ) {
            _coreLogService = coreLogService;
            _dbContext = _dbContext;
        }

        public async Task<int?> InsertNewRandomIdea(RandomIdea randomIdea) {
            try {
                await _dbContext.RandomIdeas.AddAsync(randomIdea);
                var result = await _dbContext.SaveChangesAsync();
                
                return result == 0 ? -1 : randomIdea.Id;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(RandomIdeaService) }.{ nameof(InsertNewRandomIdea) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while inserting entry to RandomIdeas.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(randomIdea) } = { JsonConvert.SerializeObject(randomIdea) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });
            
                return null;
            }
        }

        public async Task<bool?> UpdateRandomIdea(RandomIdea randomIdea) {
            try {
                _dbContext.RandomIdeas.Update(randomIdea);
                var result = await _dbContext.SaveChangesAsync();
                
                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(RandomIdeaService) }.{ nameof(UpdateRandomIdea) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating entry to RandomIdeas.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(randomIdea) } = { JsonConvert.SerializeObject(randomIdea) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });
            
                return null;
            }
        }

        public async Task<bool?> DoesRandomIdeaBelongToThisUser(int randomIdeaId, int userId) {
            try {
                return await _dbContext.RandomIdeas
                                       .AnyAsync(idea => idea.UserId == userId && idea.Id == randomIdeaId);
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(RandomIdeaService) }.{ nameof(DoesRandomIdeaBelongToThisUser) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching RandomIdeas with AnyAsync.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, { nameof(randomIdeaId) }) = ({ userId }, { randomIdeaId })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
        }

        public async Task<bool?> DeleteRandomIdeaById(int randomIdeaId) {
            try {
                var randomIdea = await _dbContext.RandomIdeas.FindAsync(randomIdeaId);
                if (randomIdea == null) return default;
                
                _dbContext.RandomIdeas.Remove(randomIdea);
                var result = await _dbContext.SaveChangesAsync();
                
                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(RandomIdeaService) }.{ nameof(DeleteRandomIdeaById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while deleting entry from RandomIdeas.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(randomIdeaId) } = { randomIdeaId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });
            
                return null;
            }
        }

        public async Task<bool?> ArchiveRandomIdeaById(int randomIdeaId) {
            try {
                var randomIdea = await _dbContext.RandomIdeas.FindAsync(randomIdeaId);
                if (randomIdea == null) return default;

                randomIdea.DeletedOn = DateTime.UtcNow;
                _dbContext.RandomIdeas.Update(randomIdea);

                var result = await _dbContext.SaveChangesAsync();
                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(RandomIdeaService) }.{ nameof(ArchiveRandomIdeaById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while archiving RandomIdea by updating.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(randomIdeaId) } = { randomIdeaId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });
            
                return null;
            }
        }

        public async Task<bool?> ReviveRandomIdeaById(int randomIdeaId) {
            try {
                var randomIdea = await _dbContext.RandomIdeas.FindAsync(randomIdeaId);
                if (randomIdea == null) return default;

                randomIdea.DeletedOn = null;
                _dbContext.RandomIdeas.Update(randomIdea);

                var result = await _dbContext.SaveChangesAsync();
                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(RandomIdeaService) }.{ nameof(ReviveRandomIdeaById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while reviving RandomIdea by updating.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(randomIdeaId) } = { randomIdeaId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });
            
                return null;
            }
        }

        public async Task<bool?> DoAllIdeasBelongToThisUser(int[] ideaIds, int userId) {
            try {
                foreach (var todoId in ideaIds) {
                    var isBelongedToUser = await _dbContext.Notes
                                                           .AnyAsync(
                                                               idea => idea.Id == todoId &&
                                                                       idea.UserId == userId &&
                                                                       !idea.DeletedOn.HasValue
                                                           );

                    if (!isBelongedToUser) return false;
                }

                return true;
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(RandomIdeaService) }.{ nameof(DoAllIdeasBelongToThisUser) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while searching RandomIdeas with AnyAsync.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(userId) }, { nameof(ideaIds) }) = ({ userId }, { JsonConvert.SerializeObject(ideaIds) })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
        }

        public async Task<RandomIdea[]> GetRandomIdeasByUserId(int userId) {
            try {
                return await _dbContext.RandomIdeas
                                       .Where(
                                           idea => idea.UserId == userId &&
                                                   !idea.DeletedOn.HasValue
                                       )
                                       .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(RandomIdeaService) }.{ nameof(GetRandomIdeasByUserId) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while getting RandomIdeas with Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userId) } = { userId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return default;
            }
        }
    }
}