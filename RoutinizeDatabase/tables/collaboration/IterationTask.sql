CREATE TABLE [dbo].[IterationTask]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	IterationId INT NOT NULL,
	TaskId INT NOT NULL, --References to Todo, TodoGroup, Note or Note Segment
	TaskType NVARCHAR(30) NOT NULL,
	AssignedOn DATETIME2(7) NOT NULL DEFAULT (GETDATE()),
	[Message] NVARCHAR(150) DEFAULT NULL,
	CONSTRAINT [PK_IterationTask_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_IterationTask_ProjectIteration_IterationId] FOREIGN KEY ([IterationId]) REFERENCES [dbo].[ProjectIteration] ([Id]) --ON DELETE CASCADE
)
