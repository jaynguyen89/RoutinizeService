using System.Threading.Tasks;
using MediaLibrary.ViewModels;

namespace MediaLibrary.Interfaces {
    
    public interface IPhotoService {

        Task<ImgSaveResult> SendSavePhotosRequestToRoutinizeStorageApi(ImgUploadVM uploadedData);

        Task<ImgSaveResult> SendReplacePhotosRequestToRoutinizeStorageApi(ImgReplaceVM uploadedData);

        Task<ImgSaveResult> SendDeletePhotosRequestToRoutinizeStorageApi(int tokenId, int accountId, string photoName);
    }
}