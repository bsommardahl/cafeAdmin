USE [Cafe]
GO

/****** Object:  Table [dbo].[Products]    Script Date: 2/18/2014 12:20:42 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Products](
	[_id] [varchar](50) NOT NULL,
	[LocationId] [varchar](50) NOT NULL,
	[Priority] [int] NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[Tag] [varchar](50) NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[TaxRate] [decimal](18, 2) NOT NULL,
	[Cost] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED 
(
	[_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_TaxRate]  DEFAULT ((0.15)) FOR [TaxRate]
GO

ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_Cost]  DEFAULT ((0)) FOR [Cost]
GO

