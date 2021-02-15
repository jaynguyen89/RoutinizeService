-- Role only applies for departments within an organization

CREATE TABLE [dbo].[DepartmentRole]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	AddedById INT NOT NULL,
	RoleName NVARCHAR(20) NOT NULL,
	IsManagerialRole BIT NOT NULL DEFAULT 0,
	[Description] NVARCHAR(150) NOT NULL,
	EnumValue TINYINT NOT NULL,
	ForDepartmentIds NVARCHAR(500) DEFAULT NULL, -- Save in JSON format int[]
	AddedOn DATETIME2(7) NOT NULL DEFAULT(GETDATE()),
	-- Permissions for superuser
	AllowCreateDepartment BIT NOT NULL DEFAULT 0,
	AllowEditDepartment BIT NOT NULL DEFAULT 0,
	AllowDeleteDepartment BIT NOT NULL DEFAULT 0,
	AllowCreateDepartmentRole BIT NOT NULL DEFAULT 0,
	AllowEditDepartmentRole BIT NOT NULL DEFAULT 0,
	AllowDeleteDepartmentRole BIT NOT NULL DEFAULT 0,
	-- Permissions for other users
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
	CONSTRAINT [FK_DepartmentRole_User_AddedById] FOREIGN KEY ([AddedById]) REFERENCES [dbo].[User] ([Id])
)
