CREATE TABLE [dbo].[Cooperation]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	TermsAndConditions NVARCHAR(MAX) DEFAULT NULL,
	StartedOn DATETIME2(7) DEFAULT NULL,
	EndedOn DATETIME2(7) DEFAULT NULL,
	IsLocked BIT NOT NULL DEFAULT 0, -- If locked, disallow new participants to join
	AllowAnyoneToUnlock BIT NOT NULL DEFAULT 0,
	ParticipantIdsToAllowUnlock NVARCHAR(1000) DEFAULT NULL, -- Save in JSON format { userIds[], orgIds[] }
    ConfidedRequestResponderIds NVARCHAR(1000) DEFAULT NULL, -- Save in string format { userIds[], orgIds[] }
    AllowAnyoneToRespondRequest BIT NOT NULL DEFAULT 0,
    ParticipantIdsToAllowRespondRequest NVARCHAR(1000) DEFAULT NULL, -- Save in string format { userIds[], orgIds[] }
    AgreementSigners NVARCHAR(MAX) DEFAULT NULL, -- Save in JSON format { participantId, signature, timestamp }
 	CONSTRAINT [PK_Cooperation_Id] PRIMARY KEY ([Id] ASC)
)
