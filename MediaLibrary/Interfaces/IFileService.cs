using System.Threading.Tasks;
using MediaLibrary.ViewModels;

namespace MediaLibrary.Interfaces {
    
    public interface IFileService {

        Task<ApiRequestResult> SendSaveFilesRequestToRoutinizeStorageApi(FilesUploadVM uploadedData);

        Task<ApiRequestResult> SendDeleteFilesRequestToRoutinizeStorageApi(int tokenId, int accountId, string fileName);
    }
}