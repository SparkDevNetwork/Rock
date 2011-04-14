USE [IgniteChMS]
GO

/**
	Replace cms with your namespace
	Replace Authorization with your object name
*/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[cmsAuthorization](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Guid] [uniqueidentifier] NOT NULL,
	[System] [bit] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_cmsAuthorization] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[cmsAuthorization] ADD  CONSTRAINT [DF_cmsAuthorization_Guid]  DEFAULT (newid()) FOR [Guid]
GO

ALTER TABLE [dbo].[cmsAuthorization] ADD  CONSTRAINT [DF_cmsAuthorization_System]  DEFAULT ((0)) FOR [System]
GO


