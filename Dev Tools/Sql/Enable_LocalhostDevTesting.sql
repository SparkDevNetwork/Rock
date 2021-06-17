/* Run this script to set all the Organization, Public, and Internal website globalattributes to localhost */
/* This will also update the SMTP Transport to use localhost as the SMTP Server.  You can then use a email tool such as https://github.com/ChangemakerStudios/Papercut/releases to test emails */
/* If this script returns 'O rows affected', then you are already set for localhost testing */

/*  Update Organization Website Global Attribute to the localhost url that RockWeb uses when developing locally */

/* set @localDevRootUrl to be the base url you are using for your dev environment (usually 'http://localhost:6229/') */
DECLARE @localDevRootUrl nvarchar(100) = 'http://localhost:6229/';

DECLARE @OrganizationWebSiteId int = (SELECT TOP 1 Id FROM [Attribute] WHERE [Key] = 'OrganizationWebSite' AND [IsActive] = 1)
IF EXISTS ( SELECT [Id] FROM [AttributeValue]  WHERE [AttributeId] = @OrganizationWebSiteId )
BEGIN
	UPDATE [AttributeValue]
	SET
		[Value] = @localDevRootUrl
	WHERE [AttributeId] = @OrganizationWebSiteId
	AND [Value] != @localDevRootUrl
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
           ,@localDevRootUrl
           ,'E257083E-B0C2-479B-880B-E8702A6E25A3'
           ,GetDate()
           ,GetDate()
           ,1
           ,1)
END

/*  Update PublicApplicationRoot Global Attribute to the localhost url that RockWeb uses when developing locally. */
DECLARE @PublicApplicationRootId int = (SELECT TOP 1 Id FROM [Attribute] WHERE [Key] = 'PublicApplicationRoot' AND [IsActive] = 1)

IF EXISTS ( SELECT [Id] FROM [AttributeValue]  WHERE [AttributeId] = @PublicApplicationRootId )
BEGIN
	UPDATE [AttributeValue]
	SET
		[Value] = @localDevRootUrl
	WHERE [AttributeId] = @PublicApplicationRootId
	AND [Value] != @localDevRootUrl
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
           ,@localDevRootUrl
           ,'A416AD46-028C-4EEC-A23D-FB2093AEB1A8'
           ,GetDate()
           ,GetDate()
           ,null
           ,null)
END

/*  Update InternalApplicationRoot Global Attribute to the localhost url that RockWeb uses when developing locally. */
DECLARE @InternalApplicationRootId int = (SELECT TOP 1 Id FROM [Attribute] WHERE [Key] = 'InternalApplicationRoot' AND [IsActive] = 1)

IF EXISTS ( SELECT [Id] FROM [AttributeValue]  WHERE [AttributeId] = @InternalApplicationRootId )
BEGIN
	UPDATE [AttributeValue]
	SET
		[Value] = @localDevRootUrl
	WHERE [AttributeId] = @InternalApplicationRootId
	AND [Value] != @localDevRootUrl
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
           ,@localDevRootUrl
           ,'06E0E3FC-9A1C-43AF-8B3B-C760F9951012'
           ,GetDate()
           ,GetDate()
           ,null
           ,null)
END

/* Update SMTP Transport to use localhost as the SMTP Server.  Use https://github.com/ChangemakerStudios/Papercut/releases or similar email server tool to test email system */
DECLARE @CommChannelServerId int = (SELECT Id FROM [Attribute] WHERE [Guid] = '6CFFDF99-E93A-49B8-B440-0EF93878A51F')
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


-- Remove the Printer to  for 'Main Campus: Central Kiosk' so that Checkin doesn't spend a bunch of time trying to print to a printer that doesn't exist
Update [Device] set PrinterDeviceId = null where [Guid] = '61111232-01D7-427D-9C1F-D45CF4D3F7CB' and PrinterDeviceId is not null

/*

--
-- If you're really paranoid, you can also run this to .test`ify everyone's emails.
--
UPDATE [Person] 
SET [Email] = CONCAT([Email], '.test' )
WHERE [Email] != '' AND IsSystem != 1

*/
