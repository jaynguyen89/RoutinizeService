CREATE TABLE [dbo].[CollaboratorTask]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	CollaborationId INT NOT NULL,
	TaskId INT NOT NULL,  --References to Todo, TodoGroup, Note or Note Segment
	TaskType NVARCHAR(30) NOT NULL,
	Permission TINYINT NOT NULL DEFAULT 0,
	AssignedOn DATETIME2(7) NOT NULL DEFAULT (GETDATE()),
	[Message] NVARCHAR(300) DEFAULT NULL,
	CONSTRAINT [PK_CollaboratorTask_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_CollaboratorTask_Collaboration_CollaborationId] FOREIGN KEY ([CollaborationId]) REFERENCES [dbo].[Collaboration] ([Id])
)
