using System;
using Newtonsoft.Json;
using RoutinizeCore.ViewModels.Cooperation;

namespace RoutinizeCore.Models {

    public partial class CooperationRequest {

        public static implicit operator CooperationRequest(CooperationRequestVM request) {
            return new() {
                RequestedById = request.RequestedById,
                RequestedByType = request.RequestedByType,
                RequestedToId = request.RequestedToId,
                RequestedToType = request.RequestedToType,
                Message = request.Message,
                RequestedOn = DateTime.UtcNow,
                ResponderSignatures = JsonConvert.SerializeObject(new SignaturePoolVM())
            };
        }
    }
}