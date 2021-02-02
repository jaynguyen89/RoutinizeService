using System.Threading.Tasks;
using MediaLibrary.ViewModels;

namespace MediaLibrary.Interfaces {
    
    public interface IPhotoService {

        Task<ApiRequestResult> SendSavePhotosRequestToRoutinizeStorageApi(FilesUploadVM uploadedData);

        Task<ApiRequestResult> SendDeletePhotosRequestToRoutinizeStorageApi(int tokenId, int accountId, string photoName);
    }
}