using System.Threading.Tasks;
using MediaLibrary.ViewModels;
using Microsoft.AspNetCore.Http;

namespace MediaLibrary.Interfaces  {
    
    public interface IAvatarService {
        
        /// <summary>
        /// Send HttpClient POST request to Routinize Storage API. Returns null on error, otherwise, an instance of ApiRequestResult.
        /// </summary>
        Task<ImgSaveResult> SendSaveAvatarRequestToRoutinizeStorageApi(ImgUploadVM uploadedData);
        
        /// <summary>
        /// Send HttpClient DELETE request to Routinize Storage API. Returns null on error, otherwise, an instance of ApiRequestResult.
        /// </summary>
        Task<ImgSaveResult> SendDeleteAvatarRequestToRoutinizeStorageApi(int tokenId, int accountId, string photoName);
        
        /// <summary>
        /// Send HttpClient POST request to Routinize Storage API. Returns null on error, otherwise, an instance of ApiRequestResult.
        /// </summary>
        Task<ImgSaveResult> SendReplaceAvatarRequestToRoutinizeStorageApi(ImgReplaceVM uploadedData);
    }
}