SELECT *
  FROM [Cafe].[dbo].[Debits]
  WHERE [CreatedDate] >= '2014/01/15'
	AND [CreatedDate] <= '2014/01/16'
	AND [OperationalExpense] = 0

DELETE FROM [Debits] WHERE _id='52f28a9e2eb1ab71d6000011'

UPDATE [Cafe].[dbo].[Debits] SET [CreatedDate] = '2014-01-15'
WHERE [_id] = '52f289c22eb1ab71d600000d'

UPDATE [Cafe].[dbo].[Debits] SET [TaxPaid] = 120
WHERE [_id] = '52d723c9372011363500002c'

UPDATE [Cafe].[dbo].[Debits] SET [OperationalExpense] = 1
WHERE [_id] = '52f289c22eb1ab71d600000d'

UPDATE [Cafe].[dbo].[Debits] SET [Description] = 'Cafe de San Rafael'
WHERE [_id] = '52f2886b2eb1ab71d6000003'

