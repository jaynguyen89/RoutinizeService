using Newtonsoft.Json;
using RoutinizeCore.ViewModels.Cooperation;

namespace RoutinizeCore.Models {

    public partial class Cooperation {

        public static Cooperation GetDefaultInstance() {
            return new() {
                AllowAnyoneToRespondRequest = true,
                RequestAcceptancePolicy = JsonConvert.SerializeObject(
                    new RequestAcceptancePolicyVM {
                        AcceptIfAllParticipantsAccept = true,
                        RejectIfOneParticipantReject = true,
                        // AcceptBasingOnMajority = true,
                        // EarlyAutoAccept = true,
                        // PercentageOfMajority = 0.5,
                        // RangeForTurningToBeDetermined = 0.1
                    })
            };
        }
    }
}