using System;
using System.Net.Http;
using System.Threading.Tasks;
using MediaLibrary.DbContexts;
using MediaLibrary.Interfaces;
using MediaLibrary.ViewModels;
using Microsoft.Extensions.Logging;

namespace MediaLibrary.Services {
    
    public sealed class PhotoService : IPhotoService {
        
        private readonly ILogger<AvatarService> _logger;
        private readonly MediaDbContext _dbContext;
        private readonly HttpClient _httpClient = new();
        
        public PhotoService(
            ILogger<AvatarService> logger,
            MediaDbContext dbContext
        ) {
            _logger = logger;
            _dbContext = dbContext;

            _httpClient.BaseAddress = new Uri(@"https://routinize.jaydeveloper.com/");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
        }

        public async Task<ApiRequestResult> SendSavePhotosRequestToRoutinizeStorageApi(FilesUploadVM uploadedData) {
            throw new System.NotImplementedException();
        }

        public async Task<ApiRequestResult> SendDeletePhotosRequestToRoutinizeStorageApi(int tokenId, int accountId, string photoName) {
            throw new System.NotImplementedException();
        }
    }
}