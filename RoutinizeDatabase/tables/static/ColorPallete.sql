CREATE TABLE [dbo].[ColorPallete]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	SupplementColorIds NVARCHAR(200) DEFAULT NULL, -- Save JSON format int[]
	ColorName NVARCHAR(50) NOT NULL,
	ColorCode NVARCHAR(10) NOT NULL,
	CONSTRAINT [PK_ColorPallete_Id] PRIMARY KEY ([Id] ASC)
)
