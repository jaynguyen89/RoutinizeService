CREATE TABLE [dbo].[Collaboration]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	UserId INT NOT NULL,
	CollaboratorId INT NOT NULL,
	[Message] NVARCHAR(150) DEFAULT NULL,
	InvitedOn DATETIME2(7) NOT NULL DEFAULT (GETDATE()),
	IsAccepted BIT NOT NULL DEFAULT 0,
	AcceptedOn DATETIME2(7) NOT NULL,
	RejectedOn DATETIME2(7) NOT NULL,
	CONSTRAINT [PK_Collaboration_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_Collaboration_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([Id]), --ON DELETE CASCADE
	CONSTRAINT [FK_Collaboration_User_CollaboratorId] FOREIGN KEY ([CollaboratorId]) REFERENCES [dbo].[User] ([Id]) --ON DELETE CASCADE
)
