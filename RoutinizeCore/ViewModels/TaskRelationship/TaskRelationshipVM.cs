using System;
using System.Linq;
using RoutinizeCore.Models;

namespace RoutinizeCore.ViewModels.TaskRelationship {

    public class TaskRelationshipVM {
        
        public int Id { get; set; }
        
        public Relationship Relationship { get; set; }
        
        public TaskVM FromTask { get; set; }
        
        public TaskVM ToTask { get; set; }

        public string[] VerifyTaskRelationshipData() {
            var errors = FromTask.VerifyTask();
            errors = errors.Union(ToTask.VerifyTask()).ToArray();

            if (errors.Length != 0) return errors;
            if (Relationship.EnumValue > 16) return new[] { "Relationship not existed." };

            if (FromTask.TaskId == ToTask.TaskId) return new[] { "Task can not relate to itself." };
            
            if ((FromTask.TaskType.Equals(nameof(CollaboratorTask)) && ToTask.TaskType.Equals(nameof(CollaboratorTask))) ||
                (!FromTask.TaskType.Equals(nameof(CollaboratorTask)) && !ToTask.TaskType.Equals(nameof(CollaboratorTask)))
            )
                return Array.Empty<string>();

            return new[] { "Invalid relationship." };
        }
    }
}