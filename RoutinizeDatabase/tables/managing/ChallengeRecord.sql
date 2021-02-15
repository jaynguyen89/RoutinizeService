CREATE TABLE [dbo].[ChallengeRecord]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	ChallengeQuestionId INT NOT NULL,
	AccountId INT NOT NULL,
	Response NVARCHAR(50) NOT NULL,
	RecordedOn DATETIME2(7) NOT NULL DEFAULT(GETDATE()),
	CONSTRAINT [PK_ChallengeRecord_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_ChallengeRecord_ChallengeQuestion_QuestionId] FOREIGN KEY ([ChallengeQuestionId]) REFERENCES [dbo].[ChallengeQuestion] ([Id]),
	CONSTRAINT [FK_ChallengeRecord_Account_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[Account] ([Id]) --ON DELETE CASCADE
)
