using System.Threading.Tasks;
using AssistantLibrary.Interfaces;
using HelperLibrary;
using HelperLibrary.Shared;
using MediaLibrary.Interfaces;
using MediaLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using RoutinizeCore.Attributes;
using RoutinizeCore.ViewModels;

namespace RoutinizeCore.Controllers {

    [ApiController]
    [RoutinizeActionFilter]
    [Route("media-api")]
    public sealed class MediaApiController {

        private readonly IApiAccessService _apiAccessService;

        public MediaApiController(IApiAccessService apiAccessService) {
            _apiAccessService = apiAccessService;
        }

        [HttpGet("get-access-token/{task}")]
        public async Task<JsonResult> GetMediaApiAccessToken([FromRoute] string task, [FromHeader] int accountId) {
            var tokenLength = Helpers.GetRandomNumber(100, 30);

            var token = new Token {
                AccountId = accountId,
                TokenString = Helpers.GenerateRandomString(tokenLength, true),
                Target = SharedConstants.ApiTokenTargets[task]
            };

            var result = await _apiAccessService.SaveApiToken(token);
            return !result.HasValue || result.Value < 1
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An error happened while getting data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = token.TokenId });
        }
    }
}