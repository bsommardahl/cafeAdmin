USE [Cafe]
GO

/****** Object:  Table [dbo].[Times]    Script Date: 2/18/2014 12:21:15 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Times](
	[_id] [varchar](50) NOT NULL,
	[EmployeeName] [varchar](50) NOT NULL,
	[EmployeeId] [varchar](50) NOT NULL,
	[Date] [datetime] NOT NULL,
	[TimeIn] [datetime] NOT NULL,
	[TimeOut] [datetime] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[LocationId] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Times] PRIMARY KEY CLUSTERED 
(
	[_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

