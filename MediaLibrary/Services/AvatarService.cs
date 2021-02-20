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
    
    public sealed class AvatarService : IAvatarService {

        private readonly ILogger<AvatarService> _logger;
        private readonly MediaDbContext _dbContext;
        private readonly HttpClient _httpClient = new();

        public AvatarService(
            ILogger<AvatarService> logger,
            MediaDbContext dbContext
        ) {
            _logger = logger;
            _dbContext = dbContext;

            _httpClient.BaseAddress = new Uri(@"https://routinize.jaydeveloper.com/");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
        }


        public async Task<ImgSaveResult> SendSaveAvatarRequestToRoutinizeStorageApi(ImgUploadVM uploadedData) {
            try {
                var apiAccessToken = await _dbContext.Tokens.FindAsync(uploadedData.TokenId);
                if (apiAccessToken.AccountId != uploadedData.AccountId) return null;

                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(SharedConstants.ContentTypes["form"]));

                var formFile = new StreamContent(uploadedData.UploadedFile.OpenReadStream());
                formFile.Headers.ContentType = MediaTypeHeaderValue.Parse(uploadedData.UploadedFile.ContentType);

                var formData = new MultipartFormDataContent {
                    { formFile, "image", uploadedData.UploadedFile.FileName },
                    { new StringContent(uploadedData.AccountId.ToString()), "accountId" },
                    { new StringContent(apiAccessToken.TokenString), "apiKey" }
                };

                var response = await _httpClient.PostAsync("avatar/save-avatar", formData);
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

        public async Task<ImgSaveResult> SendDeleteAvatarRequestToRoutinizeStorageApi(int tokenId, int accountId, string photoName) {
            try {
                var apiAccessToken = await _dbContext.Tokens.FindAsync(tokenId);
                if (apiAccessToken.AccountId != accountId) return null;
                
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(SharedConstants.ContentTypes["json"]));
                var response = await _httpClient.PostAsJsonAsync(
                    "avatar/remove-avatar",
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

        public async Task<ImgSaveResult> SendReplaceAvatarRequestToRoutinizeStorageApi(ImgReplaceVM uploadedData) {
            try {
                var apiAccessToken = await _dbContext.Tokens.FindAsync(uploadedData.TokenId);
                if (apiAccessToken.AccountId != uploadedData.AccountId) return null;
                
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(SharedConstants.ContentTypes["form"]));
                
                var formFile = new StreamContent(uploadedData.FileToSave.OpenReadStream());
                formFile.Headers.ContentType = MediaTypeHeaderValue.Parse(uploadedData.FileToSave.ContentType);

                var formData = new MultipartFormDataContent {
                    { formFile, "replaceBy", uploadedData.FileToSave.Name },
                    { new StringContent(uploadedData.CurrentImage), "current" },
                    { new StringContent(uploadedData.AccountId.ToString()), "accountId" },
                    { new StringContent(apiAccessToken.TokenString), "apiKey" }
                };

                var response = await _httpClient.PostAsync("avatar/replace-avatar", formData);
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
    }
}