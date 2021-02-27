CREATE TABLE [dbo].[DepartmentAccess]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	CooperationId INT NOT NULL,
	FromParticipantId INT NOT NULL, -- Must be the Id of an organization participant
	AccessGivenToParticipantId INT NOT NULL, -- Can be the Id of user of organization participant
	AccessibleDepartmentIds NVARCHAR(500) NOT NULL, --Save JSON format data int[]
	CONSTRAINT [PK_DepartmentAccess_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_DepartmentAccess_Cooperation_CooerationId] FOREIGN KEY ([CooperationId]) REFERENCES [dbo].[Cooperation] ([Id])
)
