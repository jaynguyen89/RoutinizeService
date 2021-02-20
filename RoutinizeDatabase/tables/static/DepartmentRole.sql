-- Role only applies for departments within an organization

CREATE TABLE [dbo].[DepartmentRole]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	AddedById INT NOT NULL,
	OrganizationId INT NOT NULL,
	RoleName NVARCHAR(100) NOT NULL,
	IsManagerialRole BIT NOT NULL DEFAULT 0,
	[Description] NVARCHAR(300) NOT NULL,
	HierarchyIndex TINYINT NOT NULL,
	ForDepartmentIds NVARCHAR(4000) DEFAULT NULL, -- Save in JSON format int[]
	AddedOn DATETIME2(7) NOT NULL DEFAULT(GETDATE()),
	-- Permissions for organization-level users
	AllowCreateDepartment BIT NOT NULL DEFAULT 0,
	AllowEditDepartment BIT NOT NULL DEFAULT 0,
	AllowDeleteDepartment BIT NOT NULL DEFAULT 0,
	AllowCreateDepartmentRole BIT NOT NULL DEFAULT 0,
	AllowEditDepartmentRole BIT NOT NULL DEFAULT 0,
	AllowDeleteDepartmentRole BIT NOT NULL DEFAULT 0,
	AllowAddUserOrganization BIT NOT NULL DEFAULT 0,
	AllowEditUserOrganization BIT NOT NULL DEFAULT 0,
	AllowDeleteUserOrganization BIT NOT NULL DEFAULT 0,
	-- Permissions for either-level users
	AllowAddUserDepartment BIT NOT NULL DEFAULT 0,
	AllowEditUserDepartment BIT NOT NULL DEFAULT 0,
	AllowDeleteUserDepartment BIT NOT NULL DEFAULT 0,
	AllowAddTeamDepartment BIT NOT NULL DEFAULT 0,
	AllowEditTeamDepartment BIT NOT NULL DEFAULT 0,
	AllowDeleteTeamDepartment BIT NOT NULL DEFAULT 0,
	AllowAddProject BIT NOT NULL DEFAULT 0,
	AllowEditProject BIT NOT NULL DEFAULT 0,
	AllowDeleteProject BIT NOT NULL DEFAULT 0,
	AllowConstraintProject BIT NOT NULL DEFAULT 0,
	CONSTRAINT [PK_DepartmentRole_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_DepartmentRole_User_AddedById] FOREIGN KEY ([AddedById]) REFERENCES [dbo].[User] ([Id]),
    CONSTRAINT [FK_DepartmentRole_Organization_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [dbo].[Organization] ([Id])
)
