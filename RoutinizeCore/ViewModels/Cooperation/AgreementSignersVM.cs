using System;
using System.Collections.Generic;

namespace RoutinizeCore.ViewModels.Cooperation {

    public sealed class AgreementSignersVM {
        
        public ExpectedSignerVM ExpectedSigner { get; set; }

        public List<SignerVM> Signers { get; set; } = new();
    }

    public sealed class SignerVM {
        
        public int UserId { get; set; }
        
        public int RsaKeyId { get; set; }
        
        public long Timestamp { get; set; }
        
        public string Signature { get; set; }
    }

    public sealed class ExpectedSignerVM {
        
        //Key: the number of signers. Value: expected total
        public KeyValuePair<int, int> SignersAsUser { get; set; }
        
        //Key: organizationId. Value: has someone in it signed or not
        public Dictionary<int, bool> SignerAsOrganization { get; set; }
    }
}