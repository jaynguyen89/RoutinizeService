CREATE TABLE [dbo].[CollaboratorTask]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	UserId INT NOT NULL,
	CollaborationId INT NOT NULL,
	TaskId INT NOT NULL,  --References to Todo, TodoGroup, Note or Note Segment
	TaskType NVARCHAR(30) NOT NULL,
	Permission TINYINT NOT NULL DEFAULT 0,
	CONSTRAINT [PK_CollaboratorTask_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_CollaboratorTask_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([Id]),
	CONSTRAINT [FK_CollaboratorTask_Collaboration_CollaborationId] FOREIGN KEY ([CollaborationId]) REFERENCES [dbo].[Collaboration] ([Id])
)
