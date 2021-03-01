CREATE TABLE [dbo].[SigningChecker]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	CooperationParticipantId INT NOT NULL,
	ForActivity NVARCHAR(50) DEFAULT NULL,
	CreatedOn DATETIME2(7) NOT NULL DEFAULT (GETDATE()),
	IsValid BIT NOT NULL DEFAULT 0,
	InvalidOn DATETIME2(7) DEFAULT NULL,
	CONSTRAINT [PK_SigningChecker_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_SigningChecker_CooperationParticipant_CooperationParticipantId] FOREIGN KEY ([CooperationParticipantId]) REFERENCES [dbo].[CooperationParticipant] ([Id]), -- ON DELETE CASCADE
)