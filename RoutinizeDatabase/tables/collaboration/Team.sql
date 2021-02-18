﻿CREATE TABLE [dbo].[Team]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	CoverImageId INT DEFAULT NULL,
	UniqueCode NVARCHAR(50) DEFAULT NULL,
	TeamName NVARCHAR(100) DEFAULT NULL,
	CreatedOn DATETIME2(7) NOT NULL DEFAULT (GETDATE()),
	CreatedById INT DEFAULT NULL,
	CONSTRAINT [PK_Team_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_Team_Attachment_CoverImageId] FOREIGN KEY ([CoverImageId]) REFERENCES [dbo].[Attachment] ([Id]),
	CONSTRAINT [FK_Team_User_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[User] ([Id])
)
