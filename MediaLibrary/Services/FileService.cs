using System.Threading.Tasks;
using MediaLibrary.Interfaces;
using MediaLibrary.ViewModels;

namespace MediaLibrary.Services {
    
    public sealed class FileService : IFileService {

        public async Task<ApiRequestResult> SendSaveFilesRequestToRoutinizeStorageApi(FilesUploadVM uploadedData) {
            throw new System.NotImplementedException();
        }

        public async Task<ApiRequestResult> SendDeleteFilesRequestToRoutinizeStorageApi(int tokenId, int accountId, string fileName) {
            throw new System.NotImplementedException();
        }
    }
}