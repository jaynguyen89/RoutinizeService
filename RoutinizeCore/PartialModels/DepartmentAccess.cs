using Newtonsoft.Json;
using RoutinizeCore.ViewModels.Cooperation;

namespace RoutinizeCore.Models {

    public partial class DepartmentAccess {

        public static implicit operator DepartmentAccess(DepartmentAccessVM access) {
            return new() {
                CooperationId = access.CooperationId,
                FromParticipantId = access.FromParticipantId,
                AccessGivenToParticipantId = access.AccessGivenToParticipantId,
                AccessibleDepartmentIds = JsonConvert.SerializeObject(access.AccessibleDepartmentIds)
            };
        }

        public void UpdateDataBy(DepartmentAccessVM access) {
            AccessibleDepartmentIds = JsonConvert.SerializeObject(access.AccessibleDepartmentIds);
        }
    }
}