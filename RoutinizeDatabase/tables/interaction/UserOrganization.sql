CREATE TABLE [dbo].[UserOrganization]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	UserId INT NOT NULL,
    DepartmentRoleId INT DEFAULT NULL,
	OrganizationId INT NOT NULL,
    PositionName NVARCHAR(50) DEFAULT NULL,
	EmployeeCode NVARCHAR(50) DEFAULT NULL,
	CONSTRAINT [PK_UserOrganization_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_UserOrganization_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([Id]),
    CONSTRAINT [FK_UserOrganization_Organization_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [dbo].[Organization] ([Id]),
    CONSTRAINT [FK_UserOrganization_DepartmentRole_DepartmentRoleId] FOREIGN KEY ([DepartmentRoleId]) REFERENCES [dbo].[DepartmentRole] ([Id])
)
