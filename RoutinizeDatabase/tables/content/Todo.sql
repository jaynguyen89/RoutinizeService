﻿CREATE TABLE [dbo].[Todo]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	UserId INT NOT NULL,
	IsShared BIT NOT NULL DEFAULT 0,
	Emphasized BIT NOT NULL DEFAULT 0,
	CoverImage NVARCHAR(100) DEFAULT NULL,
	Title NVARCHAR(100) DEFAULT NULL,
	[Description] NVARCHAR(250) DEFAULT NULL,
	Details NVARCHAR(4000) DEFAULT NULL,
	CreatedOn DATETIME2(7) NOT NULL DEFAULT (GETDATE()),
	DueDate DATETIME2(7) DEFAULT NULL,
	DoneById INT DEFAULT NULL,
	DeletedOn NVARCHAR(7) DEFAULT NULL,
	CONSTRAINT [PK_Todo_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_Todo_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([Id]),
	CONSTRAINT [FK_Todo_User_DoneById] FOREIGN KEY ([DoneById]) REFERENCES [dbo].[User] ([Id])
)