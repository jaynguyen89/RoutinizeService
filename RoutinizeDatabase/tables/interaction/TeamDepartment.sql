CREATE TABLE [dbo].[TeamDepartment]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	TeamId INT NOT NULL,
	OrganizationId INT NOT NULL,
	DepartmentId INT DEFAULT NULL,
	ForCooperation BIT NOT NULL DEFAULT 0,
	CONSTRAINT [PK_TeamDepartment_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_TeamDepartment_Team_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Team] ([Id]),
	CONSTRAINT [FK_TeamDepartment_Department_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [dbo].[Department] ([Id]),
	CONSTRAINT [FK_TeamDepartment_Organization_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [dbo].[Organization] ([Id])
);
