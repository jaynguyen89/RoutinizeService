CREATE TABLE [dbo].[AttachmentPermission]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	AllowViewToEveryone BIT NOT NULL DEFAULT 0,
	MembersToAllowView NVARCHAR(250) DEFAULT NULL, --Save JSON string
	AllowEditToEveryone BIT NOT NULL DEFAULT 0,
	MembersToAllowEdit NVARCHAR(250) DEFAULT NULL, --Save JSON string
	AllowDeleteToEveryone BIT NOT NULL DEFAULT 0,
	MembersToAllowDelete NVARCHAR(250) DEFAULT NULL, --Save JSON string
	AllowDownloadToEveryone BIT NOT NULL DEFAULT 0,
	MembersToAllowDownload NVARCHAR(250) DEFAULT NULL, --Save JSON string
	CONSTRAINT [PK_AttachmentPermission_Id] PRIMARY KEY ([Id] ASC),
)
