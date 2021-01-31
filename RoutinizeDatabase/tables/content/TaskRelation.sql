CREATE TABLE [dbo].[TaskRelation]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	TaskId INTEGER NOT NULL,
	TaskType NVARCHAR(30) NOT NULL, --References to Todo, TodoGroup, Note or Note Segment
	RelatedToId INT NOT NULL,
	RelatedToType NVARCHAR(30) NOT NULL,
	Relationship TINYINT NOT NULL DEFAULT 0,
	RelationshipName NVARCHAR(30) NOT NULL DEFAULT 'RELATES TO',
	CONSTRAINT [PK_TaskRelation_Id] PRIMARY KEY ([Id] ASC),
)
