CREATE TABLE [dbo].[TaskFolder]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	CreatedById INT NOT NULL,
	[Name] NVARCHAR(30) NOT NULL,
	[Description] NVARCHAR(150) DEFAULT NULL,
	ColorId INT DEFAULT NULL,
	FillColor NVARCHAR(10) DEFAULT NULL,
	CreatedOn DATETIME2(7) NOT NULL DEFAULT (GETDATE()),
	CONSTRAINT [PK_TaskFolder_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_TaskFolder_User_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[User] ([Id]), --ON DELETE CASCADE
	CONSTRAINT [FK_TaskFolder_ColorPallete_ColorId] FOREIGN KEY ([ColorId]) REFERENCES [dbo].[ColorPallete] ([Id])
)
