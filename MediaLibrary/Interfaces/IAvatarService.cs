using System.Threading.Tasks;
using MediaLibrary.ViewModels;
using Microsoft.AspNetCore.Http;

namespace MediaLibrary.Interfaces  {
    
    public interface IAvatarService {
        
        /// <summary>
        /// Send HttpClient POST request to Routinize Storage API. Returns null on error, otherwise, an instance of ApiRequestResult.
        /// </summary>
        Task<ApiRequestResult> SendSaveAvatarRequestToRoutinizeStorageApi(AvatarUploadVM uploadedData);
        
        /// <summary>
        /// Send HttpClient DELETE request to Routinize Storage API. Returns null on error, otherwise, an instance of ApiRequestResult.
        /// </summary>
        Task<ApiRequestResult> SendDeleteAvatarRequestToRoutinizeStorageApi(int tokenId, int accountId, string photoName);
        
        /// <summary>
        /// Send HttpClient POST request to Routinize Storage API. Returns null on error, otherwise, an instance of ApiRequestResult.
        /// </summary>
        Task<ApiRequestResult> SendReplaceAvatarRequestToRoutinizeStorageApi(AvatarReplaceVM uploadedData);
    }
}