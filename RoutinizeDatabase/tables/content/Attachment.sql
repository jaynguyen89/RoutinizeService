CREATE TABLE [dbo].[Attachment]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	AddressId INT DEFAULT NULL,
	ItemId INT NOT NULL, --References to Todo, TodoGroup, Note, Note Segment, Project, Team, Task or Comment
	ItemType NVARCHAR(30) NOT NULL,
	PermissionId INT DEFAULT NULL,
	UploadedById INT NOT NULL,
	AttachmentType TINYINT NOT NULL,
	AttachmentName NVARCHAR(100) DEFAULT NULL,
	AttachmentUrl NVARCHAR(250) DEFAULT NULL,
	AttachedOn DATETIME2(7) NOT NULL DEFAULT (GETDATE()),
	CONSTRAINT [PK_Attachment_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_Attachment_Address_AddressId] FOREIGN KEY ([AddressId]) REFERENCES [dbo].[Address] ([Id]),
	CONSTRAINT [FK_Attachment_AttachmentPermission_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [dbo].[AttachmentPermission] ([Id]),
	CONSTRAINT [FK_Attachment_User_UploadedById] FOREIGN KEY ([UploadedById]) REFERENCES [dbo].[User] ([Id])
);