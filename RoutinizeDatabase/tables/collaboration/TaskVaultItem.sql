CREATE TABLE [dbo].[TaskVaultItem]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	TaskVaultId INT NOT NULL,
	ItemId INT NOT NULL, -- References to Todo, Note, NoteSegment, ContentGroup, RandomIdea
	ItemType NVARCHAR(30) NOT NULL,
	AddedOn DATETIME2(7) NOT NULL DEFAULT (GETDATE()),
	AddedByUserId INT NOT NULL,
	CONSTRAINT [PK_TaskVaultItem_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_TaskVaultItem_CooperationTaskVault_TaskVaultId] FOREIGN KEY ([TaskVaultId]) REFERENCES [dbo].[CooperationTaskVault] ([Id]), --ON DELETE CASCADE
	CONSTRAINT [FK_TaskVaultItem_User_AddedByUserId] FOREIGN KEY ([AddedByUserId]) REFERENCES [dbo].[User] ([Id])
)
