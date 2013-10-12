DECLARE @GmailUserId nvarchar(max)
DECLARE @GmailPassword nvarchar(max)

-- ##NOTE##:  Fill in these values
SET @GmailUserId = '' 
SET @GmailPassword = ''

-- Set Email Settings
DELETE AV
FROM [AttributeValue] AV
INNER JOIN [Attribute] A ON A.[Id] = AV.[AttributeId]
WHERE A.[Guid] IN (
	-- SMTP Transport attributes
    '6CFFDF99-E93A-49B8-B440-0EF93878A51F',
    'C6B13F15-9D6F-45B2-BDB9-E77D29A32EBF',
    '2CE8D3AC-F851-462C-93D5-DB82F48DDBFD',
    'D3641DA0-9E50-4C98-A994-978AF308E745',
    'B3B2308B-6CD2-4853-8220-C80D861F5D3C',

	-- Legacy Global Attributes (still used by email templates)
	'1C4E71DD-ED38-4586-93CF-A847003EC594',
	'996B04C9-45E5-4DC1-A84B-27D14B53DCC6',
	'10DD8248-DC68-4206-ABFD-DA4E8BB849E3',
	'40690F08-1433-4046-8F22-B4B16075F1CF',
	'3C5F2BF8-8D8A-46D4-9182-2A25D32851EA'
)

-- SMTP Transport attributes
INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) 
SELECT 0, [Id], 0, 0, 'smtp.gmail.com', NEWID() FROM [Attribute] WHERE [Guid] = '6CFFDF99-E93A-49B8-B440-0EF93878A51F'
INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) 
SELECT 0, [Id], 0, 0, @GmailPassword, NEWID() FROM [Attribute] WHERE [Guid] = 'D3641DA0-9E50-4C98-A994-978AF308E745'
INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) 
SELECT 0, [Id], 0, 0, 'True', NEWID() FROM [Attribute] WHERE [Guid] = 'B3B2308B-6CD2-4853-8220-C80D861F5D3C'
INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) 
SELECT 0, [Id], 0, 0, @GmailUserId, NEWID() FROM [Attribute] WHERE [Guid] = '2CE8D3AC-F851-462C-93D5-DB82F48DDBFD'
INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) 
SELECT 0, [Id], 0, 0, '587', NEWID() FROM [Attribute] WHERE [Guid] = 'C6B13F15-9D6F-45B2-BDB9-E77D29A32EBF'

-- Legacy Global Attributes (still used by email templates)
INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) 
SELECT 0, [Id], 0, 0, 'smtp.gmail.com', NEWID() FROM [Attribute] WHERE [Guid] = '1C4E71DD-ED38-4586-93CF-A847003EC594'
INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) 
SELECT 0, [Id], 0, 0, @GmailPassword, NEWID() FROM [Attribute] WHERE [Guid] = '996B04C9-45E5-4DC1-A84B-27D14B53DCC6'
INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) 
SELECT 0, [Id], 0, 0, 'True', NEWID() FROM [Attribute] WHERE [Guid] = '10DD8248-DC68-4206-ABFD-DA4E8BB849E3'
INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) 
SELECT 0, [Id], 0, 0, @GmailUserId, NEWID() FROM [Attribute] WHERE [Guid] = '40690F08-1433-4046-8F22-B4B16075F1CF'
INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) 
SELECT 0, [Id], 0, 0, '587', NEWID() FROM [Attribute] WHERE [Guid] = '3C5F2BF8-8D8A-46D4-9182-2A25D32851EA'

