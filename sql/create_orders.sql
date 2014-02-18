USE [Cafe]
GO

/****** Object:  Table [dbo].[Orders]    Script Date: 2/18/2014 12:20:34 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Orders](
	[AmountPaid] [decimal](18, 0) NOT NULL,
	[Paid] [datetime] NOT NULL,
	[Created] [datetime] NOT NULL,
	[_id] [varchar](50) NOT NULL,
	[CustomerName] [varchar](100) NULL,
 CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED 
(
	[_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

