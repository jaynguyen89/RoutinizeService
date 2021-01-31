-- CommenterReferenceType == 'TeamMember', CommenterId references a TeamMember and CommentOnId references one of TeamTask, IterationTask, ProjectIteration
-- CommenterReferenceType == 'Collaboration' (collaborator), CommenterId references a Collaboration (collaborator) and CommentOnId references a CollaboratorTask

-- More work:
-- # Adjust this table to support mentioning @Member, or @Collaborator, or @TaskName, or @Note, or @Project, or @Iteration
-- # Mentioning can be injected at anywhere in the comment content

CREATE TABLE [dbo].[TaskComment]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	CommentedOnId INT NOT NULL, --References to teamTasks, iterationTasks, collaboratorTasks, projectIterations
	CommentedOnType NVARCHAR(30) NOT NULL,
	CommenterId INT NOT NULL,
	CommenterReferenceId INT NOT NULL,
	CommenterReferenceType NVARCHAR(30) NOT NULL,
	RepliedToId INT DEFAULT NULL,
	Content NVARCHAR(1000) NOT NULL,
	CommentOn DATETIME2(7) NOT NULL DEFAULT(GETDATE()),
	LastEdittedOn DATETIME2(7) DEFAULT NULL,
	EdittedCount TINYINT NOT NULL DEFAULT 0,
	CONSTRAINT [PK_TaskComment_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_TaskComment_TaskComment_QuotedCommentId] FOREIGN KEY ([RepliedToId]) REFERENCES [dbo].[TaskComment] ([Id])
)
