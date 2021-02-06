using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using HelperLibrary.Shared;
using MediaLibrary.DbContexts;
using MediaLibrary.Interfaces;
using MediaLibrary.ViewModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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

        public async Task<ImgSaveResult> SendSavePhotosRequestToRoutinizeStorageApi(ImgUploadVM uploadedData) {
            try {
                var apiAccessToken = await _dbContext.Tokens.FindAsync(uploadedData.TokenId);
                if (apiAccessToken.AccountId != uploadedData.AccountId) return null;

                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(SharedConstants.CONTENT_TYPES["form"]));

                var formFile = new StreamContent(uploadedData.UploadedFile.OpenReadStream());
                formFile.Headers.ContentType = MediaTypeHeaderValue.Parse(uploadedData.UploadedFile.ContentType);

                var formData = new MultipartFormDataContent {
                    { formFile, "image", uploadedData.UploadedFile.FileName },
                    { new StringContent(uploadedData.AccountId.ToString()), "accountId" },
                    { new StringContent(apiAccessToken.TokenString), "apiKey" }
                };

                var response = await _httpClient.PostAsync("photo/save-cover", formData);
                if (!response.IsSuccessStatusCode) return null;

                var result = JsonConvert.DeserializeObject<ImgSaveResult>(await response.Content.ReadAsStringAsync());
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
        
        public async Task<ImgSaveResult> SendReplacePhotosRequestToRoutinizeStorageApi(ImgReplaceVM uploadedData) {
            try {
                var apiAccessToken = await _dbContext.Tokens.FindAsync(uploadedData.TokenId);
                if (apiAccessToken.AccountId != uploadedData.AccountId) return null;
                
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(SharedConstants.CONTENT_TYPES["form"]));
                
                var formFile = new StreamContent(uploadedData.FileToSave.OpenReadStream());
                formFile.Headers.ContentType = MediaTypeHeaderValue.Parse(uploadedData.FileToSave.ContentType);

                var formData = new MultipartFormDataContent {
                    { formFile, "replaceBy", uploadedData.FileToSave.Name },
                    { new StringContent(uploadedData.CurrentImage), "current" },
                    { new StringContent(uploadedData.AccountId.ToString()), "accountId" },
                    { new StringContent(apiAccessToken.TokenString), "apiKey" }
                };

                var response = await _httpClient.PostAsync("photo/replace-cover", formData);
                if (!response.IsSuccessStatusCode) return null;

                var result = JsonConvert.DeserializeObject<ImgSaveResult>(await response.Content.ReadAsStringAsync());
                return result;
            }
            catch (Exception e) {
                _logger.LogError("AvatarService.SendReplaceAvatarRequestToWater - Error: " + e.StackTrace);
                return null;
            }
            finally {
                _httpClient.DefaultRequestHeaders.Accept.Clear();
            }
        }

        public async Task<ImgSaveResult> SendDeletePhotosRequestToRoutinizeStorageApi(int tokenId, int accountId, string photoName) {
            try {
                var apiAccessToken = await _dbContext.Tokens.FindAsync(tokenId);
                if (apiAccessToken.AccountId != accountId) return null;
                
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(SharedConstants.CONTENT_TYPES["json"]));
                var response = await _httpClient.PostAsJsonAsync(
                    "photo/remove-cover",
                    JsonConvert.SerializeObject(new {
                        apiKey = apiAccessToken.TokenString,
                        image = photoName
                    })
                );
                
                if (!response.IsSuccessStatusCode) return null;
                var result = JsonConvert.DeserializeObject<ImgSaveResult>(await response.Content.ReadAsStringAsync());
                
                return result;
            }
            catch (Exception e) {
                _logger.LogError("AvatarService.SendDeleteAvatarRequestToWater - Error: " + e.StackTrace);
                return null;
            }
            finally {
                _httpClient.DefaultRequestHeaders.Accept.Clear();
            }
        }
    }
}