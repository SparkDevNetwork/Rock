/*
----------------  Set Rock Instance Id  ----------------------
*/
UPDATE [Attribute] SET [Guid] = NEWID() WHERE [Key] = 'RockInstanceId'

/*
----------------  Set Admin Passwd  ----------------------
*/

UPDATE [UserLogin]
SET [Password] = '{AdminPassword}',
    [UserName] = '{AdminUsername}'
WHERE [Guid] = '7E10A764-EF6B-431F-87C7-861053C84131'

/*
----------------  Application Roots  ----------------------
*/

/* public application root */
DECLARE @attribute_id int
SET @attribute_id = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '49AD7AD6-9BAC-4743-B1E8-B917F6271924')

IF EXISTS(SELECT * FROM  [AttributeValue]  WHERE [AttributeId] =  @attribute_id)
	BEGIN
		UPDATE [AttributeValue]
			SET [Value] = '{PublicAppRoot}'
			WHERE [AttributeId] = @attribute_id
	END
ELSE
	BEGIN
		INSERT INTO [AttributeValue]
			(
				[IsSystem]
				, [AttributeId]
				, [Value]
				, [Guid]
			)
			VALUES (
				0
				, @attribute_id
				, '{PublicAppRoot}'
				, '2643FFBF-3EB1-4CB1-9424-87EDCE897566'
			)
	END

INSERT INTO [SiteDomain]
	(
		[IsSystem]
		, [SiteId]
		, [Domain]
		, [Guid])
	VALUES (
		0,
		(SELECT [Id] FROM [Site] WHERE [Guid] = 'F3F82256-2D66-432B-9D67-3552CD2F4C2B')
		, '{PublicAppSite}'
		, '46C6ED56-A45F-4D09-8995-C7A57A3B4DC6'
	)

/* internal application root */
SET @attribute_id = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '06E0E3FC-9A1C-43AF-8B3B-C760F9951012')
IF EXISTS(SELECT * FROM  [AttributeValue]  WHERE [AttributeId] =  @attribute_id)
	BEGIN
		UPDATE [AttributeValue]
			SET [Value] = '{InternalAppRoot}'
			WHERE [AttributeId] = @attribute_id
	END
ELSE
	BEGIN
		INSERT INTO [AttributeValue]
			(
				[IsSystem]
				, [AttributeId]
				, [Value]
				, [Guid]
			)
			VALUES (
				0
				, @attribute_id
				, '{InternalAppRoot}'
				, '652705CA-2A46-4742-9BFB-4C3CFEFA7A27'
			)
	END

INSERT INTO [SiteDomain]
	(
		[IsSystem]
		, [SiteId]
		, [Domain]
		, [Guid])
	VALUES (
		0,
		(SELECT [Id] FROM [Site] WHERE [Guid] = 'C2D29296-6A87-47A9-A753-EE4E9159C4C4')
		, '{InternalAppSite}'
		, '53249DAE-3AAA-44B7-B825-9072115848A7'
	)

/*
----------------  Org Info  ----------------------
*/

/* org name */
SET @attribute_id = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '410BF494-0714-4E60-AFBD-AD65899A12BE')
IF EXISTS(SELECT * FROM  [AttributeValue]  WHERE [AttributeId] =  @attribute_id)
	BEGIN
		UPDATE [AttributeValue]
			SET [Value] = '{OrgName}'
			WHERE [AttributeId] = @attribute_id
	END
ELSE
	BEGIN
		INSERT INTO [AttributeValue]
			(
				[IsSystem]
				, [AttributeId]
				, [Value]
				, [Guid]
			)
			VALUES (
				0
				, @attribute_id
				, '{OrgName}'
				, 'DC864AF7-52F0-44C9-80E7-49B7C524A479'
			)
	END
	
/* org abbrev */
SET @attribute_id = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '555306F1-6117-48B9-B184-D48DC1EC445F')
IF EXISTS(SELECT * FROM  [AttributeValue]  WHERE [AttributeId] =  @attribute_id)
	BEGIN
		UPDATE [AttributeValue]
			SET [Value] = '{OrgName}'
			WHERE [AttributeId] = @attribute_id
	END
ELSE
	BEGIN
		INSERT INTO [AttributeValue]
			(
				[IsSystem]
				, [AttributeId]
				, [Value]
				, [Guid]
			)
			VALUES (
				0
				, @attribute_id
				, '{OrgName}'
				, 'E342C9D2-DCC5-4EEA-9C12-0E36D7C126B1'
			)
	END

/* org phone */
SET @attribute_id = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '85716596-6AEA-4887-830F-744D22E28A0D')
IF EXISTS(SELECT * FROM  [AttributeValue]  WHERE [AttributeId] =  @attribute_id)
	BEGIN
		UPDATE [AttributeValue]
			SET [Value] = '{OrgPhone}'
			WHERE [AttributeId] = @attribute_id
	END
ELSE
	BEGIN
		INSERT INTO [AttributeValue]
			(
				[IsSystem]
				, [AttributeId]
				, [Value]
				, [Guid]
			)
			VALUES (
				0
				, @attribute_id
				, '{OrgPhone}'
				, '4387A992-6ABB-49DF-A3F2-D4D80DCC68C4'
			)
	END

/* org email */
SET @attribute_id = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '6837554F-93B3-4D46-BA48-A4059FA1766F')
IF EXISTS(SELECT * FROM  [AttributeValue]  WHERE [AttributeId] =  @attribute_id)
	BEGIN
		UPDATE [AttributeValue]
			SET [Value] = '{OrgEmail}'
			WHERE [AttributeId] = @attribute_id
	END
ELSE
	BEGIN
		INSERT INTO [AttributeValue]
			(
				[IsSystem]
				, [AttributeId]
				, [Value]
				, [Guid]
			)
			VALUES (
				0
				, @attribute_id
				, '{OrgEmail}'
				, 'AE69B1C4-61C2-49AB-B4FF-4373A7EC322E'
			)
	END

/* org website */
SET @attribute_id = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '118A083B-3F28-4D17-8B19-CC6859F89F33')
IF EXISTS(SELECT * FROM  [AttributeValue]  WHERE [AttributeId] =  @attribute_id)
	BEGIN
		UPDATE [AttributeValue]
			SET [Value] = '{OrgWebsite}'
			WHERE [AttributeId] = @attribute_id
	END
ELSE
	BEGIN
		INSERT INTO [AttributeValue]
			(
				[IsSystem]
				, [AttributeId]
				, [Value]
				, [Guid]
			)
			VALUES (
				0
				, @attribute_id
				, '{OrgWebsite}'
				, '3E5DA38D-79DF-4FF5-A488-9B0EE71DAC5F'
			)
	END

/*
----------------  Misc Global Attributes  ----------------------
*/

/* safe sender domain */
DECLARE @defined_type_id int = (SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = 'DB91D0E9-DCA6-45A9-8276-AEF032BE8AED')
INSERT INTO [DefinedValue]
	(
		[IsSystem]
		, [DefinedTypeId]
		, [Value]
		, [Guid]
		, [Order]
	)
	VALUES (
		0
		, @defined_type_id
		, '{SafeSender}'
		, '0ED6742D-15B5-44FD-B5F0-98708F47DDB6'
		, 0
	)

/* email exceptions */
SET @attribute_id = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'F7D2FE87-537D-4452-B503-3991D15BD242')
IF EXISTS(SELECT * FROM  [AttributeValue]  WHERE [AttributeId] =  @attribute_id)
	BEGIN
		UPDATE [AttributeValue]
			SET [Value] = '{EmailException}'
			WHERE [AttributeId] = @attribute_id
	END
ELSE
	BEGIN
		INSERT INTO [AttributeValue]
			(
				[IsSystem]
				, [AttributeId]
				, [Value]
				, [Guid]
			)
			VALUES (
				0
				, @attribute_id
				, '{EmailException}'
				, 'A0EAE6B1-DF91-4E88-A48D-98A4145566E0'
			)
	END

/*
----------------  Email SMTP Transport  ----------------------
*/

/* server */
SET @attribute_id = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '6CFFDF99-E93A-49B8-B440-0EF93878A51F')
IF EXISTS(SELECT * FROM  [AttributeValue]  WHERE [AttributeId] =  @attribute_id)
	BEGIN
		UPDATE [AttributeValue]
			SET [Value] = '{SmtpServer}'
			WHERE [AttributeId] = @attribute_id
	END
ELSE
	BEGIN
		INSERT INTO [AttributeValue]
			(
				[IsSystem]
				, [AttributeId]
				, [Value]
				, [Guid]
				, [EntityId]
			)
			VALUES (
				0
				, @attribute_id
				, '{SmtpServer}'
				, 'D0849B6F-56F8-45D8-8861-C7E56806B496'
				, 0
			)
	END

/* port */
SET @attribute_id = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'C6B13F15-9D6F-45B2-BDB9-E77D29A32EBF')
IF EXISTS(SELECT * FROM  [AttributeValue]  WHERE [AttributeId] =  @attribute_id)
	BEGIN
		UPDATE [AttributeValue]
			SET [Value] = '{SmtpPort}'
			WHERE [AttributeId] = @attribute_id
	END
ELSE
	BEGIN
		INSERT INTO [AttributeValue]
			(
				[IsSystem]
				, [AttributeId]
				, [Value]
				, [Guid]
				, [EntityId]
			)
			VALUES (
				0
				, @attribute_id
				, '{SmtpPort}'
				, 'F671DE32-F2FD-40B6-A7E4-84D42233972A'
				, 0
			)
	END

/* username */
SET @attribute_id = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '2CE8D3AC-F851-462C-93D5-DB82F48DDBFD')
IF EXISTS(SELECT * FROM  [AttributeValue]  WHERE [AttributeId] =  @attribute_id)
	BEGIN
		UPDATE [AttributeValue]
			SET [Value] = '{SmtpUser}'
			WHERE [AttributeId] = @attribute_id
	END
ELSE
	BEGIN
		INSERT INTO [AttributeValue]
			(
				[IsSystem]
				, [AttributeId]
				, [Value]
				, [Guid]
				, [EntityId]
			)
			VALUES (
				0
				, @attribute_id
				, '{SmtpUser}'
				, '1A7C18C0-1751-4F83-B3E3-7059990B5B24'
				, 0
			)
	END

/* password */
SET @attribute_id = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'D3641DA0-9E50-4C98-A994-978AF308E745')
IF EXISTS(SELECT * FROM  [AttributeValue]  WHERE [AttributeId] =  @attribute_id)
	BEGIN
		UPDATE [AttributeValue]
			SET [Value] = '{SmtpPassword}'
			WHERE [AttributeId] = @attribute_id
	END
ELSE
	BEGIN
		INSERT INTO [AttributeValue]
			(
				[IsSystem]
				, [AttributeId]
				, [Value]
				, [Guid]
				, [EntityId]
			)
			VALUES (
				0
				, @attribute_id
				, '{SmtpPassword}'
				, '9CC1623D-1A25-4554-AA8E-3593930F3739'
				, 0
			)
	END

/* use ssl */
SET @attribute_id = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'B3B2308B-6CD2-4853-8220-C80D861F5D3C')
IF EXISTS(SELECT * FROM  [AttributeValue]  WHERE [AttributeId] =  @attribute_id)
	BEGIN
		UPDATE [AttributeValue]
			SET [Value] = '{SmtpUseSsl}'
			WHERE [AttributeId] = @attribute_id
	END
ELSE
	BEGIN
		INSERT INTO [AttributeValue]
			(
				[IsSystem]
				, [AttributeId]
				, [Value]
				, [Guid]
				, [EntityId]
			)
			VALUES (
				0
				, @attribute_id
				, '{SmtpUseSsl}'
				, '210A1472-6AAD-43F0-B346-FB5EBE9D4928'
				, 0
			)
	END