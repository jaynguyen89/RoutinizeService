﻿CREATE TABLE [dbo].[RandomIdea]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	UserId INT NOT NULL,
	GroupId INT DEFAULT NULL,
	Content NVARCHAR(4000) NOT NULL,
	AddedOn DATETIME2(7) NOT NULL DEFAULT (GETDATE()),
	DeletedOn DATETIME2(7) DEFAULT NULL,
	CONSTRAINT [PK_RandomIdea_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_RandomIdea_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([Id]), --ON DELETE CASCADE
	CONSTRAINT [FK_RandomIdea_ContentGroup_GroupId] FOREIGN KEY ([GroupId]) REFERENCES [dbo].[ContentGroup] ([Id])
)
