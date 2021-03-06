﻿CREATE TABLE [dbo].[Note]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	UserId INT NOT NULL,
	GroupId INT DEFAULT NULL,
	IsShared BIT NOT NULL DEFAULT 0,
	Emphasized BIT NOT NULL DEFAULT 0,
	Title NVARCHAR(150) DEFAULT NULL,
	CreatedOn DATETIME2(7) NOT NULL DEFAULT (GETDATE()),
	DeletedOn DATETIME2(7) DEFAULT NULL,
	CONSTRAINT [PK_Note_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_Note_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([Id]), --ON DELETE CASCADE
	CONSTRAINT [FK_Note_ContentGroup_GroupId] FOREIGN KEY ([GroupId]) REFERENCES [dbo].[ContentGroup] ([Id])
)
