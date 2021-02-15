using System.Threading.Tasks;
using HelperLibrary.Shared;
using Microsoft.AspNetCore.Mvc;
using RoutinizeCore.Attributes;
using RoutinizeCore.Models;
using RoutinizeCore.Services.Interfaces;
using RoutinizeCore.ViewModels;
using RoutinizeCore.ViewModels.TaskRelationship;

namespace RoutinizeCore.Controllers {

    [ApiController]
    [RoutinizeActionFilter]
    [Route("relationship")]
    public class TaskRelationController {

        private readonly ITaskRelationService _relationService;
        private readonly ICollaborationService _collaborationService;

        public TaskRelationController(
            ITaskRelationService relationService,
            ICollaborationService collaborationService
        ) {
            _relationService = relationService;
            _collaborationService = collaborationService;
        }

        // [HttpGet("get")]
        // public async Task<JsonResult> GetRelationships() {
        //     var relationships = await _relationService.GetAllRelationships();
        //     return relationships == null
        //         ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
        //         : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = relationships });
        // }
        //
        // [HttpPost("add")]
        // public async Task<JsonResult> CreateTasksRelationship(TaskRelationshipVM relationshipData) {
        //     var errors = relationshipData.VerifyTaskRelationshipData();
        //     if (errors.Length != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errors });
        //
        //     bool? isRelatable;
        //     if (relationshipData.FromTask.TaskType.Equals(nameof(CollaboratorTask)))
        //         isRelatable = await _collaborationService.AreTheseTasksInTheSameCollaboration(relationshipData.FromTask, relationshipData.ToTask);
        //     else {
        //         isRelatable = await _relationService.AreTheseTasksOfTheSameTeam();
        //         isRelatable = isRelatable || await _relationService.AreTheseTasksOfTheSameProject();
        //     }
        // }
        //
        // [HttpPut("update")]
        // public async Task<JsonResult> UpdateTasksRelationship() {
        //     
        // }
        //
        // [HttpDelete("remove")]
        // public async Task<JsonResult> DeleteTasksRelationship() {
        //     
        // }
        //
        // [HttpGet("get")]
        // public async Task<JsonResult> GetRelatedTasksForTask() {
        //     
        // }
        //
        // [HttpGet("get-relatables")]
        // public async Task<JsonResult> GetRelatablesForTask() {
        //     
        // }
    }
}