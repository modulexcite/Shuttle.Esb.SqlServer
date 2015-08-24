SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Idempotence](
	[MessageId] [uniqueidentifier] NOT NULL,
	[InboxWorkQueueUri] [varchar](265) NOT NULL,
	[DateStarted] [datetime] NOT NULL,
	[AssignedThreadId] [int] NULL,
	[DateThreadIdAssigned] [datetime] NULL,
	[MessageHandled] [int],
 CONSTRAINT [PK_Idempotence] PRIMARY KEY CLUSTERED 
(
	[MessageId] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[IdempotenceHistory](
	[MessageId] [uniqueidentifier] NOT NULL,
	[InboxWorkQueueUri] [varchar](265) NOT NULL,
	[DateStarted] [datetime] NOT NULL,
	[DateCompleted] [datetime] NOT NULL,
 CONSTRAINT [PK_IdempotenceHistory] PRIMARY KEY CLUSTERED 
(
	[MessageId] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[IdempotenceDeferredMessage](
	[MessageId] [uniqueidentifier] NOT NULL,
	[MessageIdReceived] [uniqueidentifier] NOT NULL,
	[MessageBody] [image] NOT NULL,
 CONSTRAINT [PK_IdempotenceDeferredMessage] PRIMARY KEY NONCLUSTERED 
(
	[MessageId] ASC
) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
CREATE CLUSTERED INDEX [IX_IdempotenceDeferredMessage] ON [dbo].[IdempotenceDeferredMessage] 
(
	[MessageIdReceived] ASC
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Idempotence] ADD  CONSTRAINT [DF_Idempotence_DateStarted]  DEFAULT (getdate()) FOR [DateStarted]
GO
ALTER TABLE [dbo].[Idempotence] ADD  CONSTRAINT [DF_Idempotence_MessageHandled]  DEFAULT ((0)) FOR [MessageHandled]
GO
ALTER TABLE [dbo].[IdempotenceHistory] ADD  CONSTRAINT [DF_IdempotenceHistory_DateStarted]  DEFAULT (getdate()) FOR [DateCompleted]
GO
