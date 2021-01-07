CREATE TABLE [dbo].[Attachment]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	ItemId INT NOT NULL, --References to Todo, Note, Note Segment, Project, Team, Task or Comment
	ItemType NVARCHAR(30) NOT NULL,
	AttachmentType TINYINT NOT NULL,
	AttachmentName NVARCHAR(100) DEFAULT NULL,
	AttachmentUrl NVARCHAR(250) DEFAULT NULL,
	CONSTRAINT [PK_Attachment_Id] PRIMARY KEY ([Id] ASC),
)
