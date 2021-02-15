CREATE TABLE [dbo].[TeamRoleClaim]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	TeamRoleId INT NOT NULL,
	ClaimerId INT NOT NULL,
	AllowAddMember BIT NOT NULL DEFAULT 0,
	AllowApproveMember BIT NOT NULL DEFAULT 0,
	AllowRemoveMember BIT NOT NULL DEFAULT 0,
	AllowEditTeamInfo BIT NOT NULL DEFAULT 0,
	CONSTRAINT [PK_TeamRoleClaim_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_TeamRoleClaim_TeamRole_TeamRoleId] FOREIGN KEY ([TeamRoleId]) REFERENCES [dbo].[TeamRole] ([Id]), --ON DELETE CASCADE
	CONSTRAINT [FK_TeamRoleClaim_TeamMember_ClaimerId] FOREIGN KEY ([ClaimerId]) REFERENCES [dbo].[TeamMember] ([Id]) --ON DELETE CASCADE
);