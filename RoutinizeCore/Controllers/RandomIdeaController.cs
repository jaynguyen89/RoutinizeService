using System.Threading.Tasks;
using HelperLibrary.Shared;
using Microsoft.AspNetCore.Mvc;
using RoutinizeCore.Attributes;
using RoutinizeCore.Models;
using RoutinizeCore.Services.Interfaces;
using RoutinizeCore.ViewModels;

namespace RoutinizeCore.Controllers {

    [ApiController]
    [RoutinizeActionFilter]
    [Route("random-idea")]
    public sealed class RandomIdeaController {

        private readonly IRandomIdeaService _randomIdeaService;
        private readonly IUserService _userService;

        public RandomIdeaController(
            IRandomIdeaService randomIdeaService,
            IUserService userService
        ) {
            _randomIdeaService = randomIdeaService;
        }

        [HttpPost("get")]
        public async Task<JsonResult> GetAllRandomIdeasByUser([FromHeader] int userId) {
            var randomIdeas = await _randomIdeaService.GetRandomIdeasByUserId(userId);
            return randomIdeas == null ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                                       : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = randomIdeas });
        }

        [HttpPost("add")]
        public async Task<JsonResult> AddNewRandomIdea([FromHeader] int userId,[FromBody] RandomIdea randomIdea) {
            var error = randomIdea.VerifyContent();
            if (error.Length != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = error });

            randomIdea.UserId = userId;
            
            var saveResult = await _randomIdeaService.InsertNewRandomIdea(randomIdea);
            return !saveResult.HasValue || saveResult.Value < 1
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = saveResult.Value });
        }
        
        [HttpPut("update")]
        public async Task<JsonResult> UpdateRandomIdea(RandomIdea randomIdea) {
            var error = randomIdea.VerifyContent();
            if (error.Length != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = error });
            
            var saveResult = await _randomIdeaService.UpdateRandomIdea(randomIdea);
            return !saveResult.HasValue || !saveResult.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
        
        [HttpDelete("delete/{randomIdeaId}")]
        public async Task<JsonResult> RemoveRandomIdea([FromHeader] int userId,[FromRoute] int randomIdeaId) {
            var shouldDeleteRandomIdea = await _randomIdeaService.DoesRandomIdeaBelongToThisUser(randomIdeaId, userId);
            if (!shouldDeleteRandomIdea.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!shouldDeleteRandomIdea.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            var hasPremiumSubscription = await _userService.DoesUserHasPremiumForAnything(userId);
            if (!hasPremiumSubscription.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            bool? result;
            if (!hasPremiumSubscription.Value) result = await _randomIdeaService.DeleteRandomIdeaById(randomIdeaId);
            else result = await _randomIdeaService.ArchiveRandomIdeaById(randomIdeaId);
            
            return !result.HasValue || !result.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = $"An issue happened while { (hasPremiumSubscription.Value ? "archiving" : "deleting") } data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
        
        [HttpPut("revive/{randomIdeaId}")]
        public async Task<JsonResult> ReviveRandomIdea([FromHeader] int userId,[FromRoute] int randomIdeaId) {
            var shouldReviveRandomIdea = await _randomIdeaService.DoesRandomIdeaBelongToThisUser(randomIdeaId, userId);
            if (!shouldReviveRandomIdea.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!shouldReviveRandomIdea.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "You are not authorized for this action." });

            var hasPremiumSubscription = await _userService.DoesUserHasPremiumForAnything(userId);
            if (!hasPremiumSubscription.HasValue) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            if (!hasPremiumSubscription.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Please subscribe Premium to use this feature." });
            
            var reviveResult = await _randomIdeaService.ReviveRandomIdeaById(randomIdeaId);
            
            return !reviveResult.HasValue || !reviveResult.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while deleting data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
    }
}