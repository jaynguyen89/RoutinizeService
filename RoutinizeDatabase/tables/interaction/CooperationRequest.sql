﻿CREATE TABLE [dbo].[CooperationRequest]
(
	Id INT IDENTITY(1, 1) NOT NULL,
	RequestedById INT NOT NULL, -- References to User or Organization
	RequestedByType NVARCHAR(30) NOT NULL,
	RequestedToId INT NOT NULL, -- References to Cooperation or Organization
	RequestedToType NVARCHAR(30) NOT NULL,
	[Message] NVARCHAR(2000) DEFAULT NULL,
	RequestedOn DATETIME2(7) NOT NULL DEFAULT (GETDATE()),
	IsAccepted BIT NOT NULL DEFAULT 0, -- Use directly if RequestedToId references an Organization, else determined by RequestAcceptance
	RequestAcceptance NVARCHAR(MAX) DEFAULT NULL, -- Save in JSON format { participantId, isAccepted, respondedOn, note }
	AcceptedOn DATETIME2(7) DEFAULT NULL,
	AcceptedById INT DEFAULT NULL,
	RejectedOn DATETIME2(7) DEFAULT NULL,
	RejectedById INT DEFAULT NULL,
	ResponderNote NVARCHAR(1000) DEFAULT NULL, -- Save in JSON format { participantId, message }
	ResponderSignatures NVARCHAR(MAX) DEFAULT NULL, --Save in JSON format { participantId, signature, timestamp }
	CONSTRAINT [PK_CooperationRequest_Id] PRIMARY KEY ([Id] ASC),
)
