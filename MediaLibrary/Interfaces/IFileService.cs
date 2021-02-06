using System.Threading.Tasks;
using MediaLibrary.ViewModels;

namespace MediaLibrary.Interfaces {
    
    public interface IFileService {

        Task<AtmSaveResult> SendSaveFilesRequestToRoutinizeStorageApi(FilesUploadVM uploadedData);

        Task<AtmSaveResult> SendDeleteFilesRequestToRoutinizeStorageApi(int tokenId, int accountId, int itemId, string[] fileNames);
    }
}