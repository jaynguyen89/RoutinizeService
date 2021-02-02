using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RoutinizeCore.Attributes;
using RoutinizeCore.Services.Interfaces;

namespace RoutinizeCore.Controllers {

    [ApiController]
    [Route("attachment")]
    [RoutinizeActionFilter]
    public sealed class AttachmentController {

        private readonly ITodoService _todoService;
        private readonly IAttachmentService _attachmentService;

        public AttachmentController(
            ITodoService todoService,
            IAttachmentService attachmentService
        ) {
            _todoService = todoService;
            _attachmentService = attachmentService;
        }
    }
    
    [HttpPost("")]
    public async Task<JsonResult> AddAttachments() { }
    
    public async Task<JsonResult> SetAttachmentPermissions() { }
        
    public async Task<JsonResult> RemoveAttachments() { }
}