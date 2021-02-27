using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoLibrary.Interfaces;
using MongoLibrary.Models;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace MongoLibrary.Services {

    public sealed class RoutinizeAccountLogService : IRoutinizeAccountLogService {

        private readonly ILogger<RoutinizeAccountLogService> _logger;
        private readonly MongoDbContext _context;

        public RoutinizeAccountLogService(
            ILogger<RoutinizeAccountLogService> logger,
            MongoDbContext context,
            IOptions<MongoDbOptions> options
        ) {
            _logger = logger;
            _context = context;
            _context.SetRoutinizeDataLogCollection(options.Value.AccountLogCollection);
        }

        public async Task<bool> InsertRoutinizeAccountLog<T>([NotNull] T data) {
            _logger.LogInformation("RoutinizeAccountLogService.InsertRoutinizeAccountLog - Service starts");

            try {
                var dataLog = new GenericLog {
                    DataType = nameof(T),
                    Data = JsonConvert.SerializeObject(data)
                };
                
                await _context.RoutinizeAccountLogCollection.InsertOneAsync(dataLog);
                return true;
            }
            catch (Exception e) {
                _logger.LogError("RoutinizeAccountLogService.InsertRoutinizeDataLog - Error: " + e.Message);
                return false;
            }
        }

        public async Task<List<object>> GetRoutinizeAccountLogInRange([NotNull] int start,[NotNull] int end) {
            _logger.LogInformation("RoutinizeAccountLogService.GetRoutinizeAccountLogInRange - Service starts");

            try {
                var dataLogs = await _context.RoutinizeAccountLogCollection.Find(Builders<GenericLog>.Filter.Empty).Skip(start).Limit(end).ToListAsync();

                var dataList = new List<object>();
                dataLogs.ForEach(log => dataList.Add(log.Data));

                return dataList;
            }
            catch (Exception e) {
                _logger.LogError("RoutinizeAccountLogService.GetRoutinizeDataLogInRange - Error: " + e.Message);
                return null;
            }
        }

        public async Task<T> GetRoutinizeAccountLogByAccountIdFor<T>([NotNull] int id,[NotNull] string activity,[NotNull] bool completed) {
            _logger.LogInformation("RoutinizeAccountLogService.GetRoutinizeAccountLogByAccountIdFor - Service starts for " + nameof(T));

            try {
                var logEntry = await _context.RoutinizeAccountLogCollection.Find(
                    Builders<GenericLog>.Filter.Eq("dataType", nameof(T)) &
                    Builders<GenericLog>.Filter.Eq("data.accountId", id) &
                    Builders<GenericLog>.Filter.Eq("data.activity", activity) &
                    Builders<GenericLog>.Filter.Eq("data.isCompleted", id)
                ).SingleAsync();

                return JsonConvert.DeserializeObject<T>(logEntry.Data);
            }
            catch (Exception e) {
                _logger.LogError("RoutinizeAccountLogService.GetRoutinizeDataLogByDataIdFor - Error: " + e.Message);
                return default(T);
            }
        }

        public async Task<bool> RemoveAccountLogEntry<T>([NotNull] T entry) {
            _logger.LogInformation("RoutinizeAccountLogService.RemoveAccountLogEntry - Service starts for " + nameof(T));

            try {
                var result = await _context.RoutinizeAccountLogCollection.DeleteOneAsync(
                    Builders<GenericLog>.Filter.Eq("dataType", nameof(T)) &
                    Builders<GenericLog>.Filter.Eq("data", JsonConvert.SerializeObject(entry))
                );

                return result.IsAcknowledged;
            }
            catch (Exception e) {
                _logger.LogError("RoutinizeAccountLogService.RemoveAccountLogEntry - Error: " + e.Message);
                return false;
            }
        }
    }
}