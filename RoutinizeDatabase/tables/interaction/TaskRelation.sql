-- CollaboratorTask can only relate to other CollaboratorTask of the same Collaboration
-- TeamTask and IterationTask can be inter-related, as long as they come from the same Team or Project

CREATE TABLE [dbo].[TaskRelation]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	RelationshipId INT NOT NULL,
	TaskId INTEGER NOT NULL,
	TaskType NVARCHAR(30) NOT NULL, --References to TeamTask, IterationTask, CollaboratorTask
	RelatedToId INT NOT NULL,
	RelatedToType NVARCHAR(30) NOT NULL,
	CreatedOn DATETIME2(7) NOT NULL DEFAULT(GETDATE()),
	CONSTRAINT [PK_TaskRelation_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_TaskRelation_Relationship_RelationshipId] FOREIGN KEY ([RelationshipId]) REFERENCES [dbo].[Relationship] ([Id])
)
