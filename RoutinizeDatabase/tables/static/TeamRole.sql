-- Role only applies for members in Teams

CREATE TABLE [dbo].[TeamRole]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	UserId INT NOT NULL,
	RoleName NVARCHAR(50) NOT NULL,
	[Description] NVARCHAR(300) NOT NULL,
	EnumValue TINYINT NOT NULL,
	ForTeamIds NVARCHAR(4000) DEFAULT NULL, -- Save in JSON format int[]
	AddedOn DATETIME2(7) NOT NULL DEFAULT (GETDATE()),
	AllowAddProjectIteration BIT NOT NULL DEFAULT 0,
	AllowEditProjectIteration BIT NOT NULL DEFAULT 0,
	AllowDeleteProjectIteration BIT NOT NULL DEFAULT 0,
	AllowAddTask BIT NOT NULL DEFAULT 0,
	AllowEditTask BIT NOT NULL DEFAULT 0,
	AllowDeleteTask BIT NOT NULL DEFAULT 0,
	AllowAssignTask BIT NOT NULL DEFAULT 0,
	AllowConstraintTask BIT NOT NULL DEFAULT 0,
	CONSTRAINT [PK_Role_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_TeamRole_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([Id])
)
