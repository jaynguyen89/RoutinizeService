CREATE TABLE [dbo].[Cooperation]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	Name NVARCHAR(100) DEFAULT NULL,
	TermsAndConditions NVARCHAR(MAX) DEFAULT NULL,
	IsInEffect BIT NOT NULL DEFAULT 0,
	StartedOn DATETIME2(7) DEFAULT NULL,
	EndedOn DATETIME2(7) DEFAULT NULL,
    AllowAnyoneToRespondRequest BIT NOT NULL DEFAULT 0,
    ConfidedRequestResponderIds NVARCHAR(1000) DEFAULT NULL, -- Save in string format { userIds[], orgIds[] }
    RequestAcceptancePolicy NVARCHAR(1000) DEFAULT NULL, -- Save in JSON format RequestAcceptancePolicyVM
    AgreementSigners NVARCHAR(MAX) DEFAULT NULL, -- Save in JSON format { participantId, signature, timestamp }
 	CONSTRAINT [PK_Cooperation_Id] PRIMARY KEY ([Id] ASC)
)
