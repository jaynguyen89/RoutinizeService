using System;
using HelperLibrary;
using RoutinizeCore.Models;

namespace RoutinizeCore.ViewModels.TaskRelationship {

    public sealed class TaskVM {
        
        public int TaskId { get; set; }
        
        public string TaskType { get; set; }

        public string[] VerifyTask() {
            if (TaskId == 0 || !Helpers.IsProperString(TaskType))
                return new[] { "Task data missing." };

            if (!TaskType.Equals(nameof(TeamTask)) || !TaskType.Equals(nameof(IterationTask)) || !TaskType.Equals(nameof(CollaboratorTask)))
                return new[] { "Task Type is invalid." };

            return Array.Empty<string>();
        }
    }
}