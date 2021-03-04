using System;
using System.Diagnostics;
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
            
        }

        public async Task<bool?> UpdateRandomIdea(RandomIdea randomIdea) {
            
        }

        public async Task<bool?> DoesRandomIdeaBelongToThisUser(int randomIdeaId, int userId) {
            
        }

        public async Task<bool?> DeleteRandomIdeaById(int randomIdeaId) {
            
        }

        public async Task<bool?> ArchiveRandomIdeaById(int randomIdeaId) {
            
        }

        public async Task<bool?> ReviveRandomIdeaById(int randomIdeaId) {
            
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
                    Location = $"private { nameof(RandomIdeaService) }.{ nameof(DoAllIdeasBelongToThisUser) }",
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
            
        }
    }
}