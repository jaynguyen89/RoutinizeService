using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NotifierLibrary.Interfaces;
using RoutinizeCore.Attributes;

namespace RoutinizeCore.Controllers {

    [ApiController]
    [RoutinizeActionFilter]
    [Route("task-vault")]
    public class TaskVaultController : AppController {

        public TaskVaultController(
            IUserNotifier userNotifier
        ) : base(userNotifier) {
            
        }
        
        [HttpPost("create")]
        public async Task<JsonResult> CreateCooperationTaskVault() {
            throw new NotImplementedException();
        }
        
        [HttpPut("update")]
        public async Task<JsonResult> UpdateCooperationTaskVault() {
            throw new NotImplementedException();
        }
    }
}