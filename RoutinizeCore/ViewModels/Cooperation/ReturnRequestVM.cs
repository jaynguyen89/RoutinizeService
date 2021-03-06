﻿using System;
using HelperLibrary;
using Newtonsoft.Json;
using RoutinizeCore.Models;
using RoutinizeCore.ViewModels.User;

namespace RoutinizeCore.ViewModels.Cooperation {

    public sealed class ReturnRequestVM {
        
        public int CooperationParticipantId { get; set; }
        
        public string Message { get; set; }

        public string[] VerifyMessage() {
            if (Helpers.IsProperString(Message))
                return Message.Length > 300
                    ? new[] { "Message is too long. Max 300 characters." }
                    : Array.Empty<string>();
            
            Message = null;
            return Array.Empty<string>();
        }
    }

    public sealed class ReturnRequestResponseVM {
        
        public int RequestId { get; set; }
        
        public bool IsAccepted { get; set; }
        
        public string Note { get; set; }

        public string[] VerifyNote() {
            if (Helpers.IsProperString(Note))
                return Note.Length > 300
                    ? new[] {"Message is too long. Max 300 characters."}
                    : Array.Empty<string>();
            
            Note = null;
            return Array.Empty<string>();
        }
    }

    public sealed class ReturnRequestDetailVM {
        
        public int Id { get; set; }
        
        public CooperationParticipantVM Participant { get; set; }
        
        public string Message { get; set; }
        
        public DateTime RequestedOn { get; set; }
        
        public bool AllowRespond { get; set; }
        
        public ResponseVM Response { get; set; }

        public class ResponseVM {
            
            public UserVM RespondedByUser { get; set; }

            public bool IsAccepted { get; set; }
            
            public DateTime? RespondedOn { get; set; }
            
            public DbSignatureRecordVM Signature { get; set; }

            public static implicit operator ResponseVM(ParticipantReturnRequest request) {
                return new() {
                    IsAccepted = request.IsAccepted.Value,
                    RespondedOn = request.RespondedOn,
                    Signature = JsonConvert.DeserializeObject<DbSignatureRecordVM>(request.RespondNote)
                };
            }
        }

        public static implicit operator ReturnRequestDetailVM(ParticipantReturnRequest request) {
            return new() {
                Id = request.Id,
                Message = request.Message,
                RequestedOn = request.RequestedOn,
                Response = request.IsAccepted.HasValue ? request : null
            };
        }
    }
}