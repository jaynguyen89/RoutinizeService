﻿CREATE TABLE [dbo].[TeamRequest]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	TeamId INT NOT NULL,
	RequestedById INT NOT NULL,
	RequestedOn DATETIME2(7) NOT NULL DEFAULT(GETDATE()),
	ValidUntil DATETIME2(7) DEFAULT NULL,
	[Message] NVARCHAR(100) DEFAULT NULL,
	IsAccepted BIT NOT NULL DEFAULT 0,
	AcceptedOn DATETIME2(7) DEFAULT NULL,
	CONSTRAINT [PK_TeamRequest_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_TeamRequest_User_RequestedById] FOREIGN KEY ([RequestedById]) REFERENCES [dbo].[User] ([Id]),
	CONSTRAINT [FK_TeamRequest_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Team] ([Id])
)
