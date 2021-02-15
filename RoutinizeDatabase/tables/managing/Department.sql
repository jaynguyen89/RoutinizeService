CREATE TABLE [dbo].[Department]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	OrganizationId INT NOT NULL,
	ParentId INT DEFAULT NULL,
	[Name] NVARCHAR(60) NOT NULL,
	[Description] NVARCHAR(4000) DEFAULT NULL,
	ContactDetails NVARCHAR(500) DEFAULT NULL, -- Save in JSON format { url, phoneNumber, extension }
	CONSTRAINT [PK_Department_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_Department_Organization_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [dbo].[Organization] ([Id]),
	CONSTRAINT [FK_Department_Department_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[Department] ([Id])
)
