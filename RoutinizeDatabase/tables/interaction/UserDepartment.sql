CREATE TABLE [dbo].[UserDepartment]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	UserId INT NOT NULL,
    DepartmentRoleId INT NOT NULL,
	DepartmentId INT NOT NULL,
    PositionName NVARCHAR(50) DEFAULT NULL,
	EmployeeCode NVARCHAR(50) DEFAULT NULL,
	CONSTRAINT [PK_UserDepartment_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_UserDepartment_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([Id]),
    CONSTRAINT [FK_UserDepartment_Department_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [dbo].[Department] ([Id]),
    CONSTRAINT [FK_UserDepartment_DepartmentRole_DepartmentRoleId] FOREIGN KEY ([DepartmentRoleId]) REFERENCES [dbo].[DepartmentRole] ([Id])
)
