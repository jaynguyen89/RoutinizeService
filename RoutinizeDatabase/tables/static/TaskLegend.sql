CREATE TABLE [dbo].[TaskLegend]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	CreatedById INT NOT NULL,
	ForOrganizationWide BIT NOT NULL DEFAULT 0,
	ForDepartmentIds NVARCHAR(4000) DEFAULT NULL, -- Applied to all Teams in the specified Department
	ForTeamIds NVARCHAR(4000) DEFAULT NULL, -- Applied to all Projects in the specified Team
	ForProjectIds NVARCHAR(4000) DEFAULT NULL, -- Applied only to the specified Projects
	LegendName NVARCHAR(50) NOT NULL,
	[Description] NVARCHAR(300) DEFAULT NULL,
	ColorId INT DEFAULT NULL,
	FillColor NVARCHAR(10) DEFAULT NULL,
	FillPattern TINYINT NOT NULL DEFAULT 0,
	CONSTRAINT [PK_TaskLegend_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_TaskLegend_User_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[User] ([Id]),
	CONSTRAINT [FK_TaskLegend_ColorPallete_ColorId] FOREIGN KEY ([ColorId]) REFERENCES [dbo].[ColorPallete] ([Id])
)
