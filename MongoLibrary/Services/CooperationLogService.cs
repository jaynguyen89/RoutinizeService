using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoLibrary.Interfaces;
using MongoLibrary.Models;
using Newtonsoft.Json;

namespace MongoLibrary.Services {

    public sealed class CooperationLogService : ICooperationLogService {
        
        private readonly ILogger<RoutinizeAccountLogService> _logger;
        private readonly MongoDbContext _context;

        public CooperationLogService(
            ILogger<RoutinizeAccountLogService> logger,
            MongoDbContext context,
            IOptions<MongoDbOptions> options
        ) {
            _logger = logger;
            _context = context;
            _context.SetRoutinizeCooperationLogCollection(options.Value.CooperationLogCollection);
        }
        
        public async Task<bool> InsertCooperationParticipantLog<T>([NotNull] T data) {
            _logger.LogInformation("CooperationLogService.InsertCooperationParticipantLog - Service starts");

            try {
                var dataLog = new GenericLog {
                    DataType = nameof(T),
                    Data = JsonConvert.SerializeObject(data)
                };
                
                await _context.RoutinizeAccountLogCollection.InsertOneAsync(dataLog);
                return true;
            }
            catch (Exception e) {
                _logger.LogError("CooperationLogService.InsertCooperationParticipantLog - Error: " + e.Message);
                return false;
            }
        }
    }
}