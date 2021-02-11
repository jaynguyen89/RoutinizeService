using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using RoutinizeCore.Models;
using RoutinizeCore.ViewModels.Attachment;

namespace RoutinizeCore.Services.Interfaces {

    public interface IAttachmentService : IDbServiceBase {
        
        Task<bool?> InsertNewAttachment([NotNull] Attachment attachment);
        
        Task<Attachment> GetAttachmentById([NotNull] int attachmentId);
        
        Task<bool?> DeleteAttachment([NotNull] Attachment attachment);
        
        Task<Attachment[]> GetAllAttachmentsByItemId([NotNull] int itemId,[NotNull] string itemType);
        
        Task<string[]> RemoveItemAttachmentsForFileName([NotNull] int itemId,[NotNull] string itemType);
        
        Task<int?> InsertPermission([NotNull] AttachmentPermission permission);
        
        Task<bool?> UpdateAttachment([NotNull] Attachment attachment);
        
        Task<GetAttachmentVM[]> Normalize([NotNull] Attachment[] attachments);
    }
}