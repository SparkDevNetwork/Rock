DECLARE @FullWidthLayout int = ( SELECT TOP 1 [Id] FROM [Layout] WHERE [Guid] = 'D65F783D-87A9-4CC9-8110-E83466A0EADB' )
DECLARE @PersonProfilePageId int = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '08DBD8A5-2C35-4146-B4A8-0F7652348B25' )
DECLARE @PersonalDevicesPageId int = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = 'B2786294-99DC-477E-871D-2E28FCE00A98' )

IF @FullWidthLayout IS NOT NULL AND @PersonProfilePageId IS NOT NULL AND @PersonalDevicesPageId IS NOT NULL 
BEGIN

	-- Move personal devices page under person profile (not as a tab on person profile)
	UPDATE [Page]
	SET 
		[LayoutId] = @FullWidthLayout
		,[ParentPageId] = @PersonProfilePageId
		,[InternalName] = 'Personal Devices'
		,[PageTitle] = 'Personal Devices'
		,[BrowserTitle] = 'Personal Devices'
	WHERE 
		[Id] = @PersonalDevicesPageId

	-- Delete the person context for the page
	DELETE [PageContext]
	WHERE 
		[PageId] = @PersonalDevicesPageId

	-- Update the badge's page attribute to point to the personal device page
	DECLARE @BadgeId int = ( SELECT TOP 1 [Id] FROM [PersonBadge] WHERE [Guid] = '307CB56D-140C-4CC9-8B54-DD551CC40174' )
	DECLARE @BadgeEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.PersonBadge' )
	DECLARE @PersonalDeviceEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.PersonProfile.Badge.PersonalDevice' )
	DECLARE @BadgeAttributeId int = ( 
		SELECT TOP 1 
			[Id] 
		FROM 
			[Attribute] 
		WHERE 
			[EntityTypeId] = @BadgeEntityTypeId 
		AND 
			[EntityTypeQualifierColumn] = 'EntityTypeId'
		AND 
			[EntityTypeQualifierValue] = CAST( @PersonalDeviceEntityTypeId as varchar )
		AND 
			[Key] = 'PersonalDevicesDetail' )

	IF @BadgeId IS NOT NULL AND @BadgeAttributeId IS NOT NULL
	BEGIN
		DELETE [AttributeValue]
		WHERE
			[AttributeID] = @BadgeAttributeId

		INSERT INTO [AttributeValue] 
			( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
		VALUES 
			( 0, @BadgeAttributeId, @BadgeId, 'b2786294-99dc-477e-871d-2e28fce00a98', NEWID() )
	END

END

-- Correct Page titles
UPDATE [Page]
SET [InternalName] = 'Channel Details', [PageTitle] = 'Channel Details', [BrowserTitle] = 'Channel Details'
WHERE [Guid] = 'AF2FBEB8-1E47-4F51-A503-3D73C0D66B4E'

UPDATE [Page]
SET [InternalName] = 'Component Detail', [PageTitle] = 'Component Detail', [BrowserTitle] = 'Component Detail'
WHERE [Guid] = '9043D8F9-F9FD-4BE6-A50B-ABF9821EC0CD'

UPDATE [Page]
SET [InternalName] = 'Interaction Detail', [PageTitle] = 'Interaction Detail', [BrowserTitle] = 'Interaction Detail'
WHERE [Guid] = 'B6F6AB6F-A572-45FE-A143-2E4B8F192C8D'

-- Correct casing for interaction.Id query string param
UPDATE [Attribute]
SET DefaultValue = REPLACE( [DefaultValue], '<a href = ''{{ InteractionDetailPage }}?interactionId={{ interaction.Id }}''>', '<a href = ''{{ InteractionDetailPage }}?InteractionId={{ interaction.Id }}''>')
WHERE [Guid] = '2507A83C-CC50-49B9-8F46-E7844D44E371'

UPDATE [InteractionChannel]
SET [InteractionListTemplate] = REPLACE( [InteractionListTemplate], '<a href = ''{{ InteractionDetailPage }}?interactionId={{ interaction.Id }}''>', '<a href = ''{{ InteractionDetailPage }}?InteractionId={{ interaction.Id }}''>')
where [Guid] = 'AEFF9B52-AE61-8EBB-4F43-37C152342076'



UPDATE [Block] 
SET [PreHtml] = ''
WHERE [Guid] = 'CD99F432-DFB4-4AA2-8B79-83B469448F98'
