﻿CREATE TABLE [dbo].[ProjectIteration]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	TeamId INT NOT NULL,
	IterationName NVARCHAR(50) DEFAULT NULL,
	[Description] NVARCHAR(1000) DEFAULT NULL,
	CreatedOn DATETIME2(7) NOT NULL DEFAULT (GETDATE()),
	DueDate DATETIME2(7) DEFAULT NULL,
	ActuallyFinishedOn DATETIME2(7) DEFAULT NULL, 
	CONSTRAINT [PK_ProjectIteration_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_ProjectIteration_Team_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Team] ([Id]) --ON DELETE CASCADE
)
