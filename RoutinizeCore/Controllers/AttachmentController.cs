using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelperLibrary;
using HelperLibrary.Shared;
using MediaLibrary.Interfaces;
using MediaLibrary.ViewModels;
using Microsoft.AspNetCore.Mvc;
using RoutinizeCore.Attributes;
using RoutinizeCore.Models;
using RoutinizeCore.Services.Interfaces;
using RoutinizeCore.ViewModels;
using RoutinizeCore.ViewModels.Attachment;

namespace RoutinizeCore.Controllers {

    [ApiController]
    [Route("attachment")]
    [RoutinizeActionFilter]
    public sealed class AttachmentController {

        private readonly ITodoService _todoService;
        private readonly IOrganizationService _organizationService;
        private readonly IAttachmentService _attachmentService;
        private readonly IPhotoService _photoService;
        private readonly IFileService _fileService;

        public AttachmentController(
            ITodoService todoService,
            IOrganizationService organizationService,
            IAttachmentService attachmentService,
            IPhotoService photoService,
            IFileService fileService
        ) {
            _todoService = todoService;
            _organizationService = organizationService;
            _attachmentService = attachmentService;
            _photoService = photoService;
            _fileService = fileService;
        }

        [HttpPost("edit-cover-image")]
        public async Task<JsonResult> SetCoverImage([FromHeader] int accountId,[FromBody] CoverImageVM coverData) {
            var imageUpdateExpression = coverData.ItemType switch {
                nameof(Todo) when coverData.TaskType.Equals(SharedConstants.TaskUpdate) => (Func<Task<bool>>)(async () => await SetTodoCoverImage(accountId, coverData)),
                nameof(Todo) when coverData.TaskType.Equals(SharedConstants.TaskDelete) => async () => await RemoveTodoCoverImage(accountId, coverData),
                nameof(Organization) when coverData.TaskType.Equals(SharedConstants.TaskUpdate) => async () => await SetOrganizationLogoImage(accountId, coverData),
                nameof(Organization) when coverData.TaskType.Equals(SharedConstants.TaskDelete) => async () => await RemoveOrganizationLogoImage(accountId, coverData),
                nameof(Department) when coverData.TaskType.Equals(SharedConstants.TaskUpdate) => async () => await SetDepartmentAvatarImage(accountId, coverData),
                nameof(Department) when coverData.TaskType.Equals(SharedConstants.TaskDelete) => async () => await RemoveDepartmentAvatarImage(accountId, coverData),
            };

            var isSavingDataSuccess = imageUpdateExpression.Invoke().Result;
            return isSavingDataSuccess ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success })
                                       : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
        }

        private async Task<bool> SetTodoCoverImage(int accountId, CoverImageVM coverData) {
            var todo = await _todoService.GetTodoById(coverData.ItemId);
            if (todo == null) return false;

            ImgSaveResult httpResult;
            if (!Helpers.IsProperString(todo.CoverImage))
                httpResult = await _photoService.SendSavePhotosRequestToRoutinizeStorageApi(
                    new ImgUploadVM {
                        AccountId = accountId,
                        TokenId = coverData.TokenId,
                        UploadedFile = coverData.UploadedFile
                    }
                );
            else
                httpResult = await _photoService.SendReplacePhotosRequestToRoutinizeStorageApi(
                    new ImgReplaceVM {
                        AccountId = accountId,
                        TokenId = coverData.TokenId,
                        FileToSave = coverData.UploadedFile,
                        CurrentImage = Helpers.ExtractImageNameFromPath(todo.CoverImage)
                    }
                );

            if (httpResult.Error) return false;
            var savedImageName = $"{ httpResult.Result.Location }{ httpResult.Result.Name }";

            todo.CoverImage = savedImageName;
            if (await _todoService.UpdateTodo(todo)) return true;
            
            await _photoService.SendDeletePhotosRequestToRoutinizeStorageApi(coverData.TokenId, accountId, httpResult.Result.Name);
            return false;
        }

        private async Task<bool> RemoveTodoCoverImage(int accountId, CoverImageVM coverData) {
            var todo = await _todoService.GetTodoById(coverData.ItemId);
            if (todo == null) return false;

            todo.CoverImage = null;
            var updateResult = await _todoService.UpdateTodo(todo);
            if (!updateResult) return false;
            
            var coverImgName = Helpers.ExtractImageNameFromPath(todo.CoverImage);
            var httpResult = await _photoService.SendDeletePhotosRequestToRoutinizeStorageApi(coverData.TokenId, accountId, coverImgName);

            return httpResult.Error;
        }
        
        private async Task<bool> SetOrganizationLogoImage(int accountId, CoverImageVM coverData) {
            var organization = await _organizationService.GetOrganizationById(coverData.ItemId);
            if (organization == null) return false;

            ImgSaveResult httpResult;
            if (!Helpers.IsProperString(organization.LogoName))
                httpResult = await _photoService.SendSavePhotosRequestToRoutinizeStorageApi(
                    new ImgUploadVM {
                        AccountId = accountId,
                        TokenId = coverData.TokenId,
                        UploadedFile = coverData.UploadedFile
                    }
                );
            else
                httpResult = await _photoService.SendReplacePhotosRequestToRoutinizeStorageApi(
                    new ImgReplaceVM {
                        AccountId = accountId,
                        TokenId = coverData.TokenId,
                        FileToSave = coverData.UploadedFile,
                        CurrentImage = Helpers.ExtractImageNameFromPath(organization.LogoName)
                    }
                );

            if (httpResult.Error) return false;
            var savedImageName = $"{ httpResult.Result.Location }{ httpResult.Result.Name }";

            organization.LogoName = savedImageName;
            var updateResult = await _organizationService.UpdateOrganization(organization);
            if (updateResult.HasValue && updateResult.Value) return true;
            
            await _photoService.SendDeletePhotosRequestToRoutinizeStorageApi(coverData.TokenId, accountId, httpResult.Result.Name);
            return false;
        }

        private async Task<bool> RemoveOrganizationLogoImage(int accountId, CoverImageVM coverData) {
            var organization = await _organizationService.GetOrganizationById(coverData.ItemId);
            if (organization == null) return false;

            organization.LogoName = null;
            var updateResult = await _organizationService.UpdateOrganization(organization);
            if (!updateResult.HasValue || !updateResult.Value) return false;
            
            var coverImgName = Helpers.ExtractImageNameFromPath(organization.LogoName);
            var httpResult = await _photoService.SendDeletePhotosRequestToRoutinizeStorageApi(coverData.TokenId, accountId, coverImgName);

            return httpResult.Error;
        }
        
        private async Task<bool> SetDepartmentAvatarImage(int accountId, CoverImageVM coverData) {
            var department = await _organizationService.GetDepartmentById(coverData.ItemId);
            if (department == null) return false;

            ImgSaveResult httpResult;
            if (!Helpers.IsProperString(department.Avatar))
                httpResult = await _photoService.SendSavePhotosRequestToRoutinizeStorageApi(
                    new ImgUploadVM {
                        AccountId = accountId,
                        TokenId = coverData.TokenId,
                        UploadedFile = coverData.UploadedFile
                    }
                );
            else
                httpResult = await _photoService.SendReplacePhotosRequestToRoutinizeStorageApi(
                    new ImgReplaceVM {
                        AccountId = accountId,
                        TokenId = coverData.TokenId,
                        FileToSave = coverData.UploadedFile,
                        CurrentImage = Helpers.ExtractImageNameFromPath(department.Avatar)
                    }
                );

            if (httpResult.Error) return false;
            var savedImageName = $"{ httpResult.Result.Location }{ httpResult.Result.Name }";

            department.Avatar = savedImageName;
            var updateResult = await _organizationService.UpdateDepartment(department);
            if (updateResult.HasValue && updateResult.Value) return true;
            
            await _photoService.SendDeletePhotosRequestToRoutinizeStorageApi(coverData.TokenId, accountId, httpResult.Result.Name);
            return false;
        }

        private async Task<bool> RemoveDepartmentAvatarImage(int accountId, CoverImageVM coverData) {
            var department = await _organizationService.GetDepartmentById(coverData.ItemId);
            if (department == null) return false;

            department.Avatar = null;
            var updateResult = await _organizationService.UpdateDepartment(department);
            if (!updateResult.HasValue || !updateResult.Value) return false;
            
            var coverImgName = Helpers.ExtractImageNameFromPath(department.Avatar);
            var httpResult = await _photoService.SendDeletePhotosRequestToRoutinizeStorageApi(coverData.TokenId, accountId, coverImgName);

            return httpResult.Error;
        }

        [HttpPost("add")]
        public async Task<JsonResult> AddAttachments([FromHeader] int accountId,[FromBody] ItemAttachmentVM attachmentData) {
            var attachmentsFailedToSave = new List<string>();
            
            Array.ForEach(
                attachmentData.Attachments,
                async attachment => {
                    var saveResult = await _attachmentService.InsertNewAttachment(
                        new Attachment {
                            AddressId = attachment.AddressId,
                            ItemId = attachmentData.ItemId,
                            ItemType = Helpers.CapitalizeFirstLetterOfEachWord(attachmentData.ItemType.Trim()),
                            UploadedById = attachmentData.UploadedById,
                            AttachmentType = (byte) attachment.FileType,
                            AttachmentName = attachment.FileName,
                            AttachmentUrl = attachment.FileUrl,
                            IsHttp = Helpers.IsAttachmentAHttpLink(attachment.FileUrl),
                            AttachedOn = DateTime.UtcNow
                        });
                    
                    if (!saveResult.HasValue || !saveResult.Value) attachmentsFailedToSave.Add(attachment.FileName);
                }
            );

            var error = attachmentsFailedToSave.Count != 0;
            if (attachmentData.UploadedFiles.Count == 0)
                return error ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success })
                             : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Partial, Data = attachmentsFailedToSave });
            
            var httpResult = await _fileService.SendSaveFilesRequestToRoutinizeStorageApi(
                new FilesUploadVM {
                    AccountId = accountId,
                    ItemId = attachmentData.ItemId,
                    TokenId = attachmentData.TokenId,
                    UploadedFiles = attachmentData.UploadedFiles
                }
            );

            error = httpResult.Error;
            
            if (httpResult.Error && httpResult.Result != null) {
                attachmentsFailedToSave.AddRange(httpResult.Result.Fails);
                attachmentsFailedToSave.AddRange(httpResult.Result.Oversizes);
            }

            if (httpResult.Result != null)
                Array.ForEach(
                    httpResult.Result.Saves,
                    async savedFile => {
                        var saveResult = await _attachmentService.InsertNewAttachment(
                            new Attachment {
                                ItemId = attachmentData.ItemId,
                                ItemType = Helpers.CapitalizeFirstLetterOfEachWord(attachmentData.ItemType.Trim()),
                                UploadedById = attachmentData.UploadedById,
                                AttachmentType = Helpers.FindAttachmentType(savedFile.Type),
                                AttachmentName = savedFile.Original,
                                AttachmentUrl = $"{ savedFile.Location }{ savedFile.Name }"
                            }
                        );

                        if (saveResult.HasValue && saveResult.Value) return;
                        
                        await _fileService.SendDeleteFilesRequestToRoutinizeStorageApi(attachmentData.TokenId, accountId, attachmentData.ItemId, new [] { savedFile.Name });
                        attachmentsFailedToSave.Add(savedFile.Original);
                    }
                );

            return error ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success })
                         : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Partial, Data = attachmentsFailedToSave });
        }

        [HttpDelete("remove/{attachmentId}/{tokenId}")]
        public async Task<JsonResult> RemoveAttachmentSingle([FromHeader] int accountId,[FromRoute] int attachmentId,[FromRoute] int tokenId) {
            var attachment = await _attachmentService.GetAttachmentById(attachmentId);
            if (attachment == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            var removeResult = await _attachmentService.DeleteAttachment(attachment);
            if (!removeResult.HasValue || !removeResult.Value)
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while removing data." });

            if (attachment.AttachmentType == (byte) SharedEnums.AttachmentTypes.Address || attachment.IsHttp)
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
            
            var httpResult = await _fileService.SendDeleteFilesRequestToRoutinizeStorageApi(
                tokenId, accountId, attachment.ItemId, new[] { Helpers.ExtractImageNameFromPath(attachment.AttachmentUrl) }
            );

            return httpResult.Error ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Partial })
                                    : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
        
        [HttpDelete("remove-all/{itemType}/{itemId}/{tokenId}")]
        public async Task<JsonResult> RemoveAllAttachments([FromHeader] int accountId,[FromRoute] string itemType,[FromRoute] int itemId,[FromRoute] int tokenId) {
            var attachmentsToRemove = await _attachmentService.RemoveItemAttachmentsForFileName(itemId, itemType);
            if (attachmentsToRemove == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            
            var httpResult = await _fileService.SendDeleteFilesRequestToRoutinizeStorageApi(tokenId, accountId, itemId, attachmentsToRemove);
            return httpResult.Error switch {
                true when httpResult.Result == null => new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while processing request." }),
                true when httpResult.Result != null => new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = httpResult.Result.Fails.Concat(httpResult.Result.Unknowns) }),
                _ => new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success })
            };
        }
        
        [HttpPost("set-permission")]
        public async Task<JsonResult> SetAttachmentPermissions(AttachmentPermissionVM permissions) {
            AttachmentPermission permission = permissions;
            var pResult = await _attachmentService.InsertPermission(permission);
            if (!pResult.HasValue || pResult.Value < 1) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            
            var attachment = await _attachmentService.GetAttachmentById(permissions.AttachmentId);
            if (attachment == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            attachment.PermissionId = pResult.Value;
            var saveResult = await _attachmentService.UpdateAttachment(attachment);

            return !saveResult.HasValue || !saveResult.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = pResult.Value });
        }

        [HttpGet("get/{itemType}/{itemId}")]
        public async Task<JsonResult> GetAttachmentsForItem([FromRoute] string itemType,[FromRoute] int itemId) {
            var attachments = await _attachmentService.GetAllAttachmentsByItemId(itemId, itemType);
            if (attachments == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
            
            var normalizedAttachments = await _attachmentService.Normalize(attachments);
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = normalizedAttachments });
        }
    }
}