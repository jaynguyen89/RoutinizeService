-- Role only applies for members in Teams

CREATE TABLE [dbo].[Role]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	RoleName NVARCHAR(20) NOT NULL,
	[Description] NVARCHAR(150) NOT NULL,
	EnumValueClient TINYINT NOT NULL,
	CONSTRAINT [PK_Role_Id] PRIMARY KEY ([Id] ASC)
)
