CREATE TABLE [dbo].[UserDepartment]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	UserId INT NOT NULL,
    DepartmentRoleId INT NOT NULL,
	DepartmentId INT NOT NULL,
    PositionId INT NOT NULL,
	EmployeeCode NVARCHAR(50) DEFAULT NULL,
	JointOn DATETIME2(7) DEFAULT NULL,
	IsActive BIT NOT NULL DEFAULT 0,
	LeftOn DATETIME2(7) DEFAULT NULL,
	CONSTRAINT [PK_UserDepartment_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_UserDepartment_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([Id]),
    CONSTRAINT [FK_UserDepartment_Department_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [dbo].[Department] ([Id]),
    CONSTRAINT [FK_UserDepartment_DepartmentRole_DepartmentRoleId] FOREIGN KEY ([DepartmentRoleId]) REFERENCES [dbo].[DepartmentRole] ([Id]),
    CONSTRAINT [FK_UserDepartment_PositionTitle_PositionId] FOREIGN KEY ([PositionId]) REFERENCES [dbo].[PositionTitle] ([Id])
)
