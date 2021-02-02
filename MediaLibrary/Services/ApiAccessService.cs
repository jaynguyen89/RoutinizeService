using System.Threading.Tasks;
using MediaLibrary.DbContexts;
using MediaLibrary.Interfaces;
using MediaLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediaLibrary.Services {

    public sealed class ApiAccessService : IApiAccessService {

        private readonly ILogger<ApiAccessService> _logger;
        private readonly MediaDbContext _dbContext;

        public ApiAccessService(
            ILogger<ApiAccessService> logger,
            MediaDbContext dbContext
        ) {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<int?> SaveApiToken(Token token) {
            try {
                await _dbContext.Tokens.AddAsync(token);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0 ? token.TokenId : -1;
            }
            catch (DbUpdateException e) {
                _logger.LogError("ApiAccessService.SaveApiToken - Error while inserting Token to MySQL DB: " + e.StackTrace);
                return null;
            }
        }
    }
}