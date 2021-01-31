CREATE TABLE [dbo].[TeamTask]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	TeamId INT NOT NULL,
	TaskId INT NOT NULL, --References to Todo, TodoGroup, Note or Note Segment
	TaskType NVARCHAR(30) NOT NULL,
	AssignedOn DATETIME2(7) NOT NULL DEFAULT (GETDATE()),
	CONSTRAINT [PK_TeamTask_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_TeamTask_Team_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Team] ([Id]) --ON DELETE CASCADE
)
