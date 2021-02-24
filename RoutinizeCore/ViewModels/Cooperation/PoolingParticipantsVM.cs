using System.Collections.Generic;

namespace RoutinizeCore.ViewModels.Cooperation {

    public sealed class PoolingParticipantsVM {

        public List<int> UserIds { get; set; } = new();

        public List<int> OrganizationIds { get; set; } = new();
    }
}