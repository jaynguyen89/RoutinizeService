CREATE TABLE [dbo].[TeamInvitation]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	InvitationSentById INT NOT NULL, --This UserId must be a TeamMember
	InvitationSentToId INT NOT NULL,
	IssuedOn DATETIME2(7) NOT NULL DEFAULT(GETDATE()),
	ValidUntil DATETIME2(7) DEFAULT NULL,
	[Message] NVARCHAR(100) DEFAULT NULL,
	IsAccepted BIT NOT NULL DEFAULT 0,
	AcceptedOn DATETIME2(7) DEFAULT NULL,
	CONSTRAINT [PK_TeamInvitation_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_TeamInvitation_User_InvitationSentById] FOREIGN KEY ([InvitationSentById]) REFERENCES [dbo].[User] ([Id]),
	CONSTRAINT [FK_TeamInvitation_User_InvitationSentToId] FOREIGN KEY ([InvitationSentToId]) REFERENCES [dbo].[User] ([Id])
)
