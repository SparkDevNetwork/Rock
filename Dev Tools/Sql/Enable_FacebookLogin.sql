DECLARE @FacebookAppId nvarchar(max)
DECLARE @FacebookAppSecret nvarchar(max)

-- ##NOTE##:  Fill in these values (get these values from whoever set up your facebook app_id)
SET @FacebookAppId = '' 
SET @FacebookAppSecret = ''


-- Activate Facebook
INSERT INTO [AttributeValue] ( [IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid]) 
                VALUES ( 0,(SELECT [Id] FROM [Attribute] WHERE [Guid] = 'AD8F8ED6-698B-47E7-950A-7CADCED70226'),0,0,'0',NEWID())
INSERT INTO [AttributeValue] ( [IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid]) 
                VALUES ( 0,(SELECT [Id] FROM [Attribute] WHERE [Guid] = 'BAE112EE-40D4-4F86-AED8-81C3942FF87D'),0,0,'True',NEWID())
INSERT INTO [AttributeValue] ( [IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid]) 
                VALUES ( 0,(SELECT [Id] FROM [Attribute] WHERE [Guid] = '73D53921-4AF9-4EBF-B84B-107D2A40D073'),0,0,@FacebookAppId,NEWID())
INSERT INTO [AttributeValue] ( [IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid]) 
                VALUES ( 0,(SELECT [Id] FROM [Attribute] WHERE [Guid] = '12211DBC-A51D-4FD8-B89A-A45189A94C6F'),0,0,@FacebookAppSecret,NEWID())
