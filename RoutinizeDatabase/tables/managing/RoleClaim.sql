CREATE TABLE [dbo].[RoleClaim]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	RoleId INT NOT NULL,
	ClaimerId INT NOT NULL,
	AllowAddMember BIT NOT NULL DEFAULT 0,
	AllowApproveMember BIT NOT NULL DEFAULT 0,
	AllowRemoveMember BIT NOT NULL DEFAULT 0,
	AllowEditTeamInfo BIT NOT NULL DEFAULT 0,
	CONSTRAINT [PK_RoleClaim_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_RoleClaim_Role_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Role] ([Id]), --ON DELETE CASCADE
	CONSTRAINT [FK_RoleClaim_TeamMember_ClaimerId] FOREIGN KEY ([ClaimerId]) REFERENCES [dbo].[TeamMember] ([Id]) --ON DELETE CASCADE
);