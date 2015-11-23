DECLARE @OrganizationWebSiteId int = (SELECT Id from [Attribute] WHERE [Key] = 'OrganizationWebSite')
IF EXISTS ( SELECT [Id] FROM [AttributeValue]  WHERE [AttributeId] = @OrganizationWebSiteId )
BEGIN
	UPDATE [AttributeValue]
	SET
		[Value] = 'http://localhost:6229/'
	WHERE [AttributeId] = @OrganizationWebSiteId
END
ELSE
BEGIN
	INSERT INTO [AttributeValue]
           ([IsSystem]
           ,[AttributeId]
           ,[EntityId]
           ,[Value]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId])
     VALUES
           (1
           ,@OrganizationWebSiteId
           ,null
           ,'http://localhost:6229/'
           ,'E257083E-B0C2-479B-880B-E8702A6E25A3'
           ,GetDate()
           ,GetDate()
           ,1
           ,1)
END

DECLARE @PublicApplicationRootId int = (select Id from [Attribute] WHERE [Key] = 'PublicApplicationRoot')

IF EXISTS ( SELECT [Id] FROM [AttributeValue]  WHERE [AttributeId] = @PublicApplicationRootId )
BEGIN
	UPDATE [AttributeValue]
	SET
		[Value] = 'http://localhost:6229/'
	WHERE [AttributeId] = @PublicApplicationRootId
END
ELSE
BEGIN
	INSERT INTO [AttributeValue]
           ([IsSystem]
           ,[AttributeId]
           ,[EntityId]
           ,[Value]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId])
     VALUES
           (1
           ,@PublicApplicationRootId
           ,null
           ,'http://localhost:6229/'
           ,'A416AD46-028C-4EEC-A23D-FB2093AEB1A8'
           ,GetDate()
           ,GetDate()
           ,null
           ,null)
END

DECLARE @InternalApplicationRootId int = (select Id from [Attribute] WHERE [Key] = 'InternalApplicationRoot')

IF EXISTS ( SELECT [Id] FROM [AttributeValue]  WHERE [AttributeId] = @InternalApplicationRootId )
BEGIN
	UPDATE [AttributeValue]
	SET
		[Value] = 'http://localhost:6229/'
	WHERE [AttributeId] = @InternalApplicationRootId
END
ELSE
BEGIN
	INSERT INTO [AttributeValue]
           ([IsSystem]
           ,[AttributeId]
           ,[EntityId]
           ,[Value]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId])
     VALUES
           (1
           ,@InternalApplicationRootId
           ,null
           ,'http://localhost:6229/'
           ,'06E0E3FC-9A1C-43AF-8B3B-C760F9951012'
           ,GetDate()
           ,GetDate()
           ,null
           ,null)
END

DECLARE @CommChannelServerId int = (SELECT Id from [Attribute] WHERE [Guid] = '6CFFDF99-E93A-49B8-B440-0EF93878A51F')
IF EXISTS ( SELECT [Id] FROM [AttributeValue]  WHERE [AttributeId] = @CommChannelServerId )
BEGIN
	UPDATE [AttributeValue]
	SET
		[Value] = 'localhost'
	WHERE [AttributeId] = @CommChannelServerId
END
ELSE
BEGIN
	INSERT INTO [AttributeValue]
           ([IsSystem]
           ,[AttributeId]
           ,[EntityId]
           ,[Value]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId])
     VALUES
           (1
           ,@CommChannelServerId
           ,0
           ,'localhost'
           ,'BD18DBE8-E8C0-4A5E-823D-BECBCDABF419'
           ,GetDate()
           ,GetDate()
           ,null
           ,null)
END