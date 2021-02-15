CREATE TABLE [dbo].[TeamProject]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	TeamId INT NOT NULL,
	ProjectId INT NOT NULL,
	[Description] NVARCHAR(1000) DEFAULT NULL,
	AssignedOn DATETIME2(7) NOT NULL DEFAULT (GETDATE()),
	CONSTRAINT [PK_TeamProject_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_TeamProject_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[Project] ([Id]),
	CONSTRAINT [FK_TeamProject_Team_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Team] ([Id])
)
