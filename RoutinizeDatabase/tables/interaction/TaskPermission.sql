CREATE TABLE [dbo].[TaskPermission]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	TaskId INT NOT NULL, --References to teamTasks or iterationTasks
	[Role] TINYINT NOT NULL DEFAULT 2,
	Permission TINYINT NOT NULL DEFAULT 0,
	CONSTRAINT [PK_TaskPermission_Id] PRIMARY KEY ([Id] ASC),
)
