﻿CREATE TABLE [dbo].[User]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	AccountId INT NOT NULL,
	AddressId INT DEFAULT NULL,
	AvatarName NVARCHAR(50) DEFAULT NULL,
	FirstName NVARCHAR(50) DEFAULT NULL,
	LastName NVARCHAR(50) DEFAULT NULL,
	PreferredName NVARCHAR(50) DEFAULT NULL,
	Gender BIT NOT NULL DEFAULT 0,
	CONSTRAINT [PK_User_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_User_Account_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[Account] ([Id]),
	CONSTRAINT [FK_User_Address_AddressId] FOREIGN KEY ([AddressId]) REFERENCES [dbo].[Address] ([Id])
)
