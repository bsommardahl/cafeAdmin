SELECT _id, VendorName, Amout, Description, CreatedDate, TaxPaid
  FROM [Cafe].[dbo].[Debits]
  WHERE [CreatedDate] >= '2014/08/6'
	AND [CreatedDate] <= '2014/08/7'
	AND [OperationalExpense] = 0
	ORDER BY Amout

UPDATE [Cafe].[dbo].[Debits] SET [OperationalExpense] = 1
WHERE [_id] = '53e297e3b7fad727ab00005a'

DELETE FROM [Debits] WHERE _id='52f28a9e2eb1ab71d6000011'

UPDATE [Cafe].[dbo].[Debits] SET [CreatedDate] = '2014-01-15'
WHERE [_id] = '52f289c22eb1ab71d600000d'

UPDATE [Cafe].[dbo].[Debits] SET [TaxPaid] = 33.91
WHERE [_id] = '533710f6b7fad708ba000005'

UPDATE [Cafe].[dbo].[Debits] SET [Amout] = 240
WHERE [_id] = '5334aafa00e55731a5000086'

UPDATE [Cafe].[dbo].[Debits] SET [Description] = '2 botes de agua 5 gls, 6 aguacates'
WHERE [_id] = '53064c652eb1ab7a600000af'

UPDATE [Cafe].[dbo].[Debits] SET [CreatedDate] = '2014-03-19'
WHERE [_id] = '5329d29600e557768f000039'





