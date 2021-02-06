using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using HelperLibrary;
using HelperLibrary.Shared;
using Microsoft.EntityFrameworkCore;
using MongoLibrary.Interfaces;
using MongoLibrary.Models;
using Newtonsoft.Json;
using RoutinizeCore.DbContexts;
using RoutinizeCore.Models;
using RoutinizeCore.Services.Interfaces;
using RoutinizeCore.ViewModels.Attachment;

namespace RoutinizeCore.Services.DatabaseServices {

    public sealed class AttachmentService : IAttachmentService {
        
        private readonly IRoutinizeCoreLogService _coreLogService;
        private readonly RoutinizeDbContext _dbContext;

        public AttachmentService(
            IRoutinizeCoreLogService coreLogService,
            RoutinizeDbContext dbContext
        ) {
            _coreLogService = coreLogService;
            _dbContext = dbContext;
        }

        public async Task<bool?> InsertNewAttachment([NotNull] Attachment attachment) {
            try {
                await _dbContext.Attachments.AddAsync(attachment);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AttachmentService) }.{ nameof(InsertNewAttachment) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while inserting entry to Attachments.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(attachment) } = { JsonConvert.SerializeObject(attachment) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<Attachment> GetAttachmentById([NotNull] int attachmentId) {
            try {
                return await _dbContext.Attachments.FindAsync(attachmentId);
            }
            catch (Exception e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AttachmentService) }.{ nameof(GetAttachmentById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(Exception),
                    DetailedInformation = $"Error while getting entry from Attachments.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(attachmentId) } = { attachmentId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> DeleteAttachment([NotNull] Attachment attachment) {
            try {
                _dbContext.Attachments.Remove(attachment);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AttachmentService) }.{ nameof(DeleteAttachment) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while deleting entry from Attachments.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(attachment) } = { JsonConvert.SerializeObject(attachment) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<Attachment[]> GetAllAttachmentsByItemId([NotNull] int itemId,[NotNull] string itemType) {
            try {
                return await _dbContext.Attachments
                                       .Where(
                                           attachment => attachment.ItemId == itemId &&
                                                         attachment.ItemType.ToLower().Equals(itemType.ToLower())
                                        )
                                       .ToArrayAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AttachmentService) }.{ nameof(GetAllAttachmentsByItemId) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while getting data in Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(itemId) }, { nameof(itemType) }) = ({ itemId }, { itemType })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<string[]> RemoveItemAttachmentsForFileName([NotNull] int itemId,[NotNull] string itemType) {
            try {
                var attachmentsToRemove = await GetAllAttachmentsByItemId(itemId, itemType);
                if (attachmentsToRemove == null) return null;
                
                _dbContext.Attachments.RemoveRange(attachmentsToRemove);
                var result = await _dbContext.SaveChangesAsync();
                if (result == 0) return null;

                var fileNames = attachmentsToRemove
                                .Where(
                                    attachment => attachment.AttachmentType <= (byte) SharedEnums.AttachmentTypes.File &&
                                                  attachment.IsHttp
                                )
                                .Select(attachment => attachment.AttachmentName)
                                .ToArray();

                return fileNames;
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AttachmentService) }.{ nameof(RemoveItemAttachmentsForFileName) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while getting data in Where-ToArray.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(itemId) }, { nameof(itemType) }) = ({ itemId }, { itemType })",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AttachmentService) }.{ nameof(RemoveItemAttachmentsForFileName) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while deleting multiple entries from Attachments.\n\n{ e.StackTrace }",
                    ParamData = $"({ nameof(itemId) }, { nameof(itemType) }) = ({ itemId }, { itemType })",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<int?> InsertPermission([NotNull] AttachmentPermission permission) {
            try {
                await _dbContext.AttachmentPermissions.AddAsync(permission);
                var result = await _dbContext.SaveChangesAsync();

                return result == 0 ? -1 : permission.Id;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AttachmentService) }.{ nameof(InsertPermission) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while inserting entry to AttachmentPermissions.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(permission) } = { JsonConvert.SerializeObject(permission) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> UpdateAttachment([NotNull] Attachment attachment) {
            try {
                _dbContext.Attachments.Update(attachment);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AttachmentService) }.{ nameof(UpdateAttachment) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating entry to Attachments.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(attachment) } = { JsonConvert.SerializeObject(attachment) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<GetAttachmentVM[]> Normalize([NotNull] Attachment[] attachments) {
            try {
                var vmAttachments = new List<GetAttachmentVM>();
                
                Array.ForEach(
                    attachments,
                    async attachment => {
                        var vmAttachment = new GetAttachmentVM {
                            Id = attachment.Id,
                            Type = attachment.AttachmentType
                        };

                        if (attachment.AttachmentType == (byte) SharedEnums.AttachmentTypes.Address)
                            vmAttachment.Place = new AtmPlace {
                                Address = await _dbContext.Addresses.FindAsync(attachment.AddressId),
                                Permission = attachment.PermissionId.HasValue
                                    ? await _dbContext.AttachmentPermissions.FindAsync(attachment.PermissionId)
                                    : null
                            };
                        else
                            vmAttachment.File = new AtmFile {
                                Name = attachment.AttachmentName,
                                Url = attachment.AttachmentUrl,
                                IsHttp = attachment.IsHttp,
                                Permission = attachment.PermissionId.HasValue
                                    ? await _dbContext.AttachmentPermissions.FindAsync(attachment.PermissionId)
                                    : null
                            };

                        var author = await _dbContext.Users.FindAsync(attachment.UploadedById);
                        vmAttachment.Audit = new AtmAudit {
                            UploadedOn = attachment.AttachedOn,
                            Author = new AtmAuthor {
                                Id = attachment.UploadedById,
                                FullName = Helpers.IsProperString(author.PreferredName)
                                    ? author.PreferredName
                                    : $"{ author.FirstName } { author.LastName }",
                                Avatar = author.AvatarName
                            }
                        };
                        
                        vmAttachments.Add(vmAttachment);
                    }
                );

                return vmAttachments.ToArray();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AttachmentService) }.{ nameof(Normalize) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while traversing array with ForEach.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(attachments) } = { JsonConvert.SerializeObject(attachments) }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
            catch (Exception e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AttachmentService) }.{ nameof(Normalize) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(Exception),
                    DetailedInformation = $"Error while searching entry by ID from Users, Permissions, Addresses.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(attachments) } = { JsonConvert.SerializeObject(attachments) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }
    }
}