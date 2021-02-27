CREATE TABLE [dbo].[Department]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	OrganizationId INT NOT NULL,
	ParentId INT DEFAULT NULL,
	Avatar NVARCHAR(100) DEFAULT NULL,
	[Name] NVARCHAR(100) NOT NULL,
	[Description] NVARCHAR(4000) DEFAULT NULL,
	ContactDetails NVARCHAR(MAX) DEFAULT NULL, -- Save in JSON format { url, phoneNumber, extension }
	ForCooperation BIT NOT NULL DEFAULT 0,
	CONSTRAINT [PK_Department_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_Department_Organization_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [dbo].[Organization] ([Id]),
	CONSTRAINT [FK_Department_Department_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[Department] ([Id])
)
