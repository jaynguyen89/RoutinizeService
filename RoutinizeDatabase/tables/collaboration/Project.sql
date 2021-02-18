CREATE TABLE [dbo].[Project]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	CoverImageId INT NOT NULL,
	ProjectName NVARCHAR(100) NOT NULL,
	ProjectCode NVARCHAR(20) DEFAULT NULL,
	[Description] NVARCHAR(2000) DEFAULT NULL,
	CreatedOn DATETIME2(7) NOT NULL DEFAULT (GETDATE()),
	CreatedById INT DEFAULT NULL,
	DueDate DATETIME2(7) DEFAULT NULL,
	ActuallyFinishedOn DATETIME2(7) DEFAULT NULL,
	CONSTRAINT [PK_Project_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_Project_Attachment_CoverImageId] FOREIGN KEY ([CoverImageId]) REFERENCES [dbo].[Attachment] ([Id]),
	CONSTRAINT [FK_Project_User_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[User] ([Id])
)
