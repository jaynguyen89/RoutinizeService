CREATE TABLE [dbo].[TaskVaultItem]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	TaskVaultId INT NOT NULL,
	ItemId INT NOT NULL,
	ItemType NVARCHAR(30) NOT NULL,
	AddedOn DATETIME2(7) NOT NULL DEFAULT (GETDATE()),
	CONSTRAINT [PK_TaskVaultItem_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_TaskVaultItem_CooperationTaskVault_TaskVaultId] FOREIGN KEY ([TaskVaultId]) REFERENCES [dbo].[CooperationTaskVault] ([Id]) --ON DELETE CASCADE
)
