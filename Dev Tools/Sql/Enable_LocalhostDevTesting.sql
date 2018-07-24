/* Run this script to set all the Organization, Public, and Internal website globalattributes to localhost */
/* This will also update the SMTP Transport to use localhost as the SMTP Server.  You can then use a email tool such as https://github.com/ChangemakerStudios/Papercut/releases to test emails */
/* If this script returns 'O rows affected', then you are already set for localhost testing */

/*  Update Organization Website Global Attribute to the localhost url that RockWeb uses when developing locally */
DECLARE @OrganizationWebSiteId int = (SELECT Id from [Attribute] WHERE [Key] = 'OrganizationWebSite')
IF EXISTS ( SELECT [Id] FROM [AttributeValue]  WHERE [AttributeId] = @OrganizationWebSiteId )
BEGIN
	UPDATE [AttributeValue]
	SET
		[Value] = 'http://localhost:6229/'
	WHERE [AttributeId] = @OrganizationWebSiteId
	AND [Value] != 'http://localhost:6229/'
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

/*  Update PublicApplicationRoot Global Attribute to the localhost url that RockWeb uses when developing locally. */
DECLARE @PublicApplicationRootId int = (select Id from [Attribute] WHERE [Key] = 'PublicApplicationRoot')

IF EXISTS ( SELECT [Id] FROM [AttributeValue]  WHERE [AttributeId] = @PublicApplicationRootId )
BEGIN
	UPDATE [AttributeValue]
	SET
		[Value] = 'http://localhost:6229/'
	WHERE [AttributeId] = @PublicApplicationRootId
	AND [Value] != 'http://localhost:6229/'
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

/*  Update InternalApplicationRoot Global Attribute to the localhost url that RockWeb uses when developing locally. */
DECLARE @InternalApplicationRootId int = (select Id from [Attribute] WHERE [Key] = 'InternalApplicationRoot')

IF EXISTS ( SELECT [Id] FROM [AttributeValue]  WHERE [AttributeId] = @InternalApplicationRootId )
BEGIN
	UPDATE [AttributeValue]
	SET
		[Value] = 'http://localhost:6229/'
	WHERE [AttributeId] = @InternalApplicationRootId
	AND [Value] != 'http://localhost:6229/'
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

/* Update SMTP Transport to use localhost as the SMTP Server.  Use https://github.com/ChangemakerStudios/Papercut/releases or similar email server tool to test email system */
DECLARE @CommChannelServerId int = (SELECT Id from [Attribute] WHERE [Guid] = '6CFFDF99-E93A-49B8-B440-0EF93878A51F')
IF EXISTS ( SELECT [Id] FROM [AttributeValue]  WHERE [AttributeId] = @CommChannelServerId )
BEGIN
	UPDATE [AttributeValue]
	SET
		[Value] = 'localhost'
	WHERE [AttributeId] = @CommChannelServerId
	AND [Value] != 'localhost'
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

/*

--
-- If you're really paranoid, you can also run this to .test`ify everyone's emails.
--
UPDATE [Person] 
SET [Email] = CONCAT([Email], '.test' )
WHERE [Email] != '' AND IsSystem != 1

*/
