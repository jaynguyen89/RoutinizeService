CREATE TABLE [dbo].[Organization]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	MotherId INT DEFAULT NULL,
	AddressId INT DEFAULT NULL,
	IndustryId INT DEFAULT NULL,
	UniqueId NVARCHAR(35) NOT NULL,
	LogoName NVARCHAR(100) DEFAULT NULL,
	FullName NVARCHAR(100) DEFAULT NULL,
	ShortName NVARCHAR(60) DEFAULT NULL,
	RegistrationNumber NVARCHAR(30) DEFAULT NULL,
	RegistrationCode NVARCHAR(30) DEFAULT NULL,
	FoundedOn DATETIME2(7) DEFAULT NULL,
	Websites NVARCHAR(MAX) DEFAULT NULL, -- Saved in JSON format [{ name, url }]
	PhoneNumbers NVARCHAR(MAX) DEFAULT NULL, --Saved in Json format [{ name, number, extension }]
	BriefDescription NVARCHAR(1000) DEFAULT NULL,
	FullDetails NVARCHAR(MAX) DEFAULT NULL,
	CONSTRAINT [PK_Organization_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_Organization_Organization_MotherId] FOREIGN KEY ([MotherId]) REFERENCES [dbo].[Organization] ([Id]),
	CONSTRAINT [FK_Organization_Address_AddressId] FOREIGN KEY ([AddressId]) REFERENCES [dbo].[Address] ([Id]),
	CONSTRAINT [FK_Organization_Industry_IndustryId] FOREIGN KEY ([IndustryId]) REFERENCES [dbo].[Industry] ([Id])
)
