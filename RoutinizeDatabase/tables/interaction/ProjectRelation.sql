CREATE TABLE [dbo].[ProjectRelation]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	RelationshipId INT NOT NULL,
	FromProjectId INT NOT NULL,
	ToProjectId INT NOT NULL,
	CreatedOn DATETIME2(7) NOT NULL DEFAULT (GETDATE()),
	CONSTRAINT [PK_ProjectRelation_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_ProjectRelation_Relationship_RelationshipId] FOREIGN KEY ([RelationshipId]) REFERENCES [dbo].[Relationship] ([Id]),
	CONSTRAINT [FK_ProjectRelation_Project_FromProjectId] FOREIGN KEY ([FromProjectId]) REFERENCES [dbo].[Project] ([Id]),
	CONSTRAINT [FK_ProjectRelation_Project_ToProjectId] FOREIGN KEY ([ToProjectId]) REFERENCES [dbo].[Project] ([Id])
)
