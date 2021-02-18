CREATE TABLE [dbo].[ContentGroup]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	GroupName NVARCHAR(100) NOT NULL,
	GroupOfType NVARCHAR(30) NOT NULL, --References to Todo or Note
	[Description] NVARCHAR(300) DEFAULT NULL,
	IsShared BIT NOT NULL DEFAULT 0,
	CreatedById INT NOT NULL,
	CreatedOn DATETIME2(7) NOT NULL DEFAULT (GETDATE()),
	DeletedOn DATETIME2(7) DEFAULT NULL,
	CONSTRAINT [PK_ContentGroup_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_ContentGroup_User_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[User] ([Id])
)
