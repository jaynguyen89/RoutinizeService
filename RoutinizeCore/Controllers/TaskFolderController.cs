using Microsoft.AspNetCore.Mvc;
using RoutinizeCore.Attributes;
using RoutinizeCore.Services.Interfaces;

namespace RoutinizeCore.Controllers {

    [ApiController]
    [RoutinizeActionFilter]
    [Route("task-folder")]
    public sealed class TaskFolderController {

        private readonly ITaskFolderService _taskFolderService;
        private readonly ITaskVaultService _taskVaultService;

        public TaskFolderController(
            ITaskFolderService taskFolderService,
            ITaskVaultService taskVaultService
        ) {
            _taskFolderService = taskFolderService;
            _taskVaultService = taskVaultService;
        }
    }
}