DECLARE @GmailUserId nvarchar(max)
DECLARE @GmailPassword nvarchar(max)

-- ##NOTE##:  Fill in these values
SET @GmailUserId = '' 
SET @GmailPassword = ''

-- Set Email Settings
DELETE AV
FROM [AttributeValue] AV
INNER JOIN [Attribute] A
                ON A.[Id] = AV.[AttributeId]
WHERE A.[Guid] IN (
                '6CFFDF99-E93A-49B8-B440-0EF93878A51F',
                'C6B13F15-9D6F-45B2-BDB9-E77D29A32EBF',
                '2CE8D3AC-F851-462C-93D5-DB82F48DDBFD',
                'D3641DA0-9E50-4C98-A994-978AF308E745',
                'B3B2308B-6CD2-4853-8220-C80D861F5D3C'
)
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
