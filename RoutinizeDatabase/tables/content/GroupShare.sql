CREATE TABLE [dbo].[GroupShare]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	GroupId INT NOT NULL,
	SharedToType NVARCHAR(20) NOT NULL,
	SharedToId INT NOT NULL,
	SharedById INT NOT NULL,
	SharedOn DATETIME2 NOT NULL DEFAULT (GETDATE()),
	SideNotes NVARCHAR(150) DEFAULT NULL,
	CONSTRAINT [PK_GroupShare_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_GroupShare_ContentGroup_GroupId] FOREIGN KEY ([GroupId]) REFERENCES [dbo].[ContentGroup] ([Id]),
	CONSTRAINT [FK_GroupShare_User_SharedById] FOREIGN KEY ([SharedById]) REFERENCES [dbo].[User] ([Id])
)
