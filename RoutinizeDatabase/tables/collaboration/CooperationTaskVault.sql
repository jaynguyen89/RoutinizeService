CREATE TABLE [dbo].[CooperationTaskVault]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	CooperationId INT NOT NULL,
	PossessedByCooperatorId INT NOT NULL,
	AssignedToCooperatorId INT NOT NULL,
	AssociatedWithDepartmentId INT NOT NULL,
	TaskVaultName NVARCHAR(100) DEFAULT NULL,
	[Description] NVARCHAR(1000) DEFAULT NULL,
	CreatedOn DATETIME2(7) NOT NULL DEFAULT (GETDATE()),
	CONSTRAINT [PK_CooperationTaskVault_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_CooperationTaskVault_Cooperation_CooperationId] FOREIGN KEY ([CooperationId]) REFERENCES [dbo].[Cooperation] ([Id]),
	CONSTRAINT [FK_CooperationTaskVault_CooperationParticipant_PossessedByCooperatorId] FOREIGN KEY ([PossessedByCooperatorId]) REFERENCES [dbo].[CooperationParticipant] ([Id]),
	CONSTRAINT [FK_CooperationTaskVault_CooperationParticipant_AssignedToCooperatorId] FOREIGN KEY ([AssignedToCooperatorId]) REFERENCES [dbo].[CooperationParticipant] ([Id]),
	CONSTRAINT [FK_CooperationTaskVault_Department_AssociatedWithDepartmentId] FOREIGN KEY ([AssociatedWithDepartmentId]) REFERENCES [dbo].[Department] ([Id])
)
