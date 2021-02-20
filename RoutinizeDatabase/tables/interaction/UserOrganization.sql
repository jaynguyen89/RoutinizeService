CREATE TABLE [dbo].[UserOrganization]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	UserId INT NOT NULL,
    DepartmentRoleId INT NOT NULL,
	OrganizationId INT NOT NULL,
    PositionId INT NOT NULL,
	EmployeeCode NVARCHAR(50) DEFAULT NULL,
	JointOn DATETIME2(7) DEFAULT NULL,
	IsActive BIT NOT NULL DEFAULT 0,
	LeftOn DATETIME2(7) DEFAULT NULL,
	CONSTRAINT [PK_UserOrganization_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_UserOrganization_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([Id]),
    CONSTRAINT [FK_UserOrganization_Organization_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [dbo].[Organization] ([Id]),
    CONSTRAINT [FK_UserOrganization_DepartmentRole_DepartmentRoleId] FOREIGN KEY ([DepartmentRoleId]) REFERENCES [dbo].[DepartmentRole] ([Id]),
    CONSTRAINT [FK_UserOrganization_PositionTitle_PositionId] FOREIGN KEY ([PositionId]) REFERENCES [dbo].[PositionTitle] ([Id])
)
