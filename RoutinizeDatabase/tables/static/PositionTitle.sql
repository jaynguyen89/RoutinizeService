CREATE TABLE [dbo].[PositionTitle]
(
    Id INT IDENTITY (1, 1) NOT NULL,
    OrganizationId INT NOT NULL,
    Title NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(300) DEFAULT NULL,
    AddedOn DATETIME2(7) NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_PositionTitle_Id] PRIMARY KEY ([Id] ASC),
    CONSTRAINT [FK_PositionTitle_Organization_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [dbo].[Organization] ([Id])
);