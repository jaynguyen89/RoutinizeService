CREATE TABLE [dbo].[FolderItem]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	FolderId INT NOT NULL,
	ItemId INT NOT NULL, -- References CollaboratorTask, IterationTask, TeamTask
	ItemType NVARCHAR(30) NOT NULL,
	AddedOn DATETIME2(7) NOT NULL DEFAULT (GETDATE()),
	CONSTRAINT [PK_FolderItem_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_FolderItem_ContentFolder_FolderId] FOREIGN KEY ([FolderId]) REFERENCES [dbo].[ContentFolder] ([Id]) --ON DELETE CASCADE
)
