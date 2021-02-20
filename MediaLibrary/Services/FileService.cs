using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using HelperLibrary.Shared;
using MediaLibrary.DbContexts;
using MediaLibrary.Interfaces;
using MediaLibrary.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MediaLibrary.Services {
    
    public sealed class FileService : IFileService {
        
        private readonly ILogger<AvatarService> _logger;
        private readonly MediaDbContext _dbContext;
        private readonly HttpClient _httpClient = new();
        
        public FileService(
            ILogger<AvatarService> logger,
            MediaDbContext dbContext
        ) {
            _logger = logger;
            _dbContext = dbContext;

            _httpClient.BaseAddress = new Uri(@"https://routinize.jaydeveloper.com/");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
        }

        public async Task<AtmSaveResult> SendSaveFilesRequestToRoutinizeStorageApi(FilesUploadVM uploadedData) {
            try {
                var apiAccessToken = await _dbContext.Tokens.FindAsync(uploadedData.TokenId);
                if (apiAccessToken.AccountId != uploadedData.AccountId) return null;

                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(SharedConstants.ContentTypes["form"]));
                var formData = new MultipartFormDataContent {
                    { new StringContent(uploadedData.AccountId.ToString()), "accountId" },
                    { new StringContent(apiAccessToken.TokenString), "apiKey" },
                    { new StringContent(uploadedData.ItemId.ToString()), "container" }
                };

                foreach (var uploadedFile in uploadedData.UploadedFiles) {
                    var formFile = new StreamContent(uploadedFile.OpenReadStream());
                    formData.Add(formFile, "attachments", uploadedFile.Name);
                }
                
                var response = await _httpClient.PostAsync("attachment/save-attachments", formData);
                if (!response.IsSuccessStatusCode) return null;

                var result = JsonConvert.DeserializeObject<AtmSaveResult>(await response.Content.ReadAsStringAsync());
                return result;
            }
            catch (Exception e) {
                _logger.LogError("AvatarService.SendSaveAvatarRequestToWater - Error: " + e.StackTrace);
                return null;
            }
            finally {
                _httpClient.DefaultRequestHeaders.Accept.Clear();
            }
        }

        public async Task<AtmSaveResult> SendDeleteFilesRequestToRoutinizeStorageApi(
            int tokenId, int accountId, int itemId, string[] fileNames
        ) {
            try {
                var apiAccessToken = await _dbContext.Tokens.FindAsync(tokenId);
                if (apiAccessToken.AccountId != accountId) return null;
                
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(SharedConstants.ContentTypes["json"]));
                var response = await _httpClient.PostAsJsonAsync(
                    "attachment/remove-attachments",
                    JsonConvert.SerializeObject(new {
                        apiKey = apiAccessToken.TokenString,
                        container = itemId,
                        accountId,
                        removals = fileNames
                    })
                );
                
                if (!response.IsSuccessStatusCode) return null;
                var result = JsonConvert.DeserializeObject<AtmSaveResult>(await response.Content.ReadAsStringAsync());

                return result;
            }
            catch (Exception e) {
                _logger.LogError("AvatarService.SendSaveAvatarRequestToWater - Error: " + e.StackTrace);
                return null;
            }
            finally {
                _httpClient.DefaultRequestHeaders.Accept.Clear();
            }
        }
    }
}