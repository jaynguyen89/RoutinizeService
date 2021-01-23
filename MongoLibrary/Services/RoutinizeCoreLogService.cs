using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoLibrary.Interfaces;
using MongoLibrary.Models;

namespace MongoLibrary.Services {

    public sealed class RoutinizeCoreLogService : IRoutinizeCoreLogService {

        private readonly ILogger<RoutinizeCoreLogService> _logger;
        private readonly MongoDbContext _context;

        public RoutinizeCoreLogService(
            ILogger<RoutinizeCoreLogService> logger,
            IOptions<MongoDbOptions> options
        ) {
            _logger = logger;
            
            _context = new MongoDbContext(options);
            _context.SetRoutinizeCoreLogCollection(options.Value.CoreLogCollection);
        }

        public async Task InsertRoutinizeCoreLog(RoutinizeCoreLog log) {
            _logger.LogInformation("RoutinizeCoreLogService.InsertRoutinizeCoreLog - Service starts: Add log entry for RoutinizeCore");
            log.RecordedOn = DateTime.UtcNow;
            
            try {
                await _context.RoutinizeCoreLogCollection.InsertOneAsync(log);
                _logger.LogInformation("RoutinizeCoreLogService.InsertRoutinizeCoreLog - Service done");
            }
            catch (Exception e) {
                _logger.LogError("RoutinizeCoreLogService.InsertRoutinizeCoreLog - Error: " + e.Message);
            }
        }

        public async Task<List<RoutinizeCoreLog>> GetRoutinizeCoreLogInRange(int start = 0, int end = 100) {
            _logger.LogInformation("RoutinizeCoreLogService.GetRoutinizeCoreLogInRange - Service starts");

            List<RoutinizeCoreLog> logs;
            try {
                logs = await _context.RoutinizeCoreLogCollection.Find(Builders<RoutinizeCoreLog>.Filter.Empty).Skip(start).Limit(end).ToListAsync();
            }
            catch (Exception e) {
                _logger.LogInformation("RoutinizeCoreLogService.GetRoutinizeCoreLogInRange - Error: " + e.Message);
                return null;
            }
            
            _logger.LogInformation("RoutinizeCoreLogService.GetRoutinizeCoreLogInRange - Service done");
            return logs;
        }
    }
}