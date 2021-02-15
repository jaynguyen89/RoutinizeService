CREATE TABLE [dbo].[CooperationParticipant]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	CooperationId INT NOT NULL,
	ParticipantId INT NOT NULL, -- References to User or Organization
	ParticipantType NVARCHAR(30) NOT NULL,
	ParticipatedOn DATETIME2(7) NOT NULL DEFAULT (GETDATE()),
	IsActive BIT NOT NULL DEFAULT 0,
	CONSTRAINT [PK_CooperationParticipant_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_CooperationParticipant_Cooperation_CooperationId] FOREIGN KEY ([CooperationId]) REFERENCES [dbo].[Cooperation] ([Id])
)
