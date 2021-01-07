CREATE TABLE [dbo].[AuthRecord]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	AccountId INT NOT NULL,
	SessionId NVARCHAR(250) NOT NULL,
	AuthToken NVARCHAR(250) NOT NULL,
	AuthTimestamp INT NOT NULL,
	DeviceInformation NVARCHAR(250) DEFAULT NULL, --Save JSON data
	CONSTRAINT [PK_AuthRecord_Id] PRIMARY KEY ([Id] ASC),
	CONSTRAINT [FK_AuthRecord_Account_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[Account] ([Id]) ON DELETE CASCADE
)
