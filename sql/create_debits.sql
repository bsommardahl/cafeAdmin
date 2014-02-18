USE [Cafe]
GO

/****** Object:  Table [dbo].[Debits]    Script Date: 2/18/2014 12:20:05 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Debits](
	[_id] [varchar](50) NOT NULL,
	[VendorId] [varchar](50) NOT NULL,
	[LocationId] [varchar](50) NOT NULL,
	[VendorName] [varchar](50) NOT NULL,
	[Amout] [decimal](18, 0) NOT NULL,
	[Description] [varchar](500) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[TaxPaid] [decimal](18, 0) NOT NULL,
	[OperationalExpense] [bit] NOT NULL,
 CONSTRAINT [PK_Debits] PRIMARY KEY CLUSTERED 
(
	[_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[Debits] ADD  CONSTRAINT [DF_Debits_TaxPaid]  DEFAULT ((0)) FOR [TaxPaid]
GO

ALTER TABLE [dbo].[Debits] ADD  CONSTRAINT [DF_Debits_OperationalExpense]  DEFAULT ((0)) FOR [OperationalExpense]
GO

