﻿CREATE TABLE [dbo].[ChallengeQuestion]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	Question NVARCHAR(100) NOT NULL,
	AddedOn DATETIME2(7) NOT NULL DEFAULT(GETDATE()),
	CONSTRAINT [PK_ChallengeQuestion_Id] PRIMARY KEY ([Id] ASC)
)
