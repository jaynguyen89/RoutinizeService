using HelperLibrary.Shared;

namespace RoutinizeCore.ViewModels {

    public sealed class JsonResponse {
        
        public SharedEnums.RequestResults Result { get; set; }

        public string Message { get; set; } = null;
        
        public object Data { get; set; }
        
        public SharedEnums.HttpStatusCodes? Error { get; set; }
    }
}