CREATE TABLE [dbo].[ParticipantReturnRequest]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	CooperationParticipantId INT NOT NULL,
	[Message] NVARCHAR(300) DEFAULT NULL,
	RequestedOn DATETIME2(7) NOT NULL DEFAULT (GETDATE()),
	IsAccepted BIT DEFAULT NULL,
	RespondedById INT NOT NULL, -- is a UserId that must be a User participant or someone in an Organization participant in the cooperation
	RespondedOn DATETIME2(7) DEFAULT NULL,
	RespondNote NVARCHAR(300) DEFAULT NULL,
	CONSTRAINT [PK_ParticipantReturnRequest_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_ParticipantReturnRequest_CooperationParticipant_CooperationParticipantId] FOREIGN KEY ([CooperationParticipantId]) REFERENCES [dbo].[CooperationParticipant] ([Id]), -- ON DELETE CASCADE
	CONSTRAINT [FK_ParticipantReturnRequest_User_RespondedById] FOREIGN KEY ([RespondedById]) REFERENCES [dbo].[User] ([Id])
)