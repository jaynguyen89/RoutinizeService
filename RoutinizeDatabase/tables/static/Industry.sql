CREATE TABLE [dbo].[Industry]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	ParentId INT DEFAULT NULL,
	[Name] NVARCHAR(100) DEFAULT NULL,
	[Description] NVARCHAR(300) DEFAULT NULL,
	CONSTRAINT [PK_Industry_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_Industry_Industry_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[Industry] ([Id])
)
