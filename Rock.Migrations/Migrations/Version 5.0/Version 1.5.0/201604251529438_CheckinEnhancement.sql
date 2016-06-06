DECLARE @GroupTypeEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.GroupType' )
DECLARE @CheckInTemplatePurposeId int = ( SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '4A406CB0-495B-4795-B788-52BDFDE00B01' )
IF @GroupTypeEntityTypeId IS NOT NULL AND @CheckInTemplatePurposeId IS NOT NULL
BEGIN

    UPDATE [Attribute] SET [EntityTypeQualifierValue] = CAST( @CheckInTemplatePurposeId AS varchar) 
    WHERE [EntityTypeId] = @GroupTypeEntityTypeId
    AND [EntityTypeQualifierColumn] = 'GroupTypePurposeValueId'
    AND [Key] LIKE 'core_checkin_%'

	DECLARE @DefinedTypeId int = ( SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = '1EBCDB30-A89A-4C14-8580-8289EC2C7742' )
	IF @DefinedTypeId IS NOT NULL
	BEGIN
		UPDATE [AttributeQualifier] SET [Value] = CAST( @DefinedTypeId AS varchar) WHERE [Guid] = '504A6A25-40D1-4D6C-AECD-F27445DEA07D' 
	END

	;MERGE INTO [AttributeValue] AS RV
	USING (

		-- Check-in Type
		SELECT 
				( SELECT [Id] FROM [Attribute] WHERE [Guid] = '90C34D24-7CFB-4A52-B39C-DFF05A40997C' ) AS [AttributeId]
			,[Id] AS [EntityId]
			,'0' AS [Value]
		FROM [GroupType]
		WHERE [GroupTypePurposeValueId] = @CheckInTemplatePurposeId

		UNION ALL
	
		-- Refresh Interval
		SELECT 
				( SELECT [Id] FROM [Attribute] WHERE [Guid] = 'C99D34BF-711B-4865-84B4-B0929C92D16C' ) AS [AttributeId]
			,[Id] AS [EntityId]
			,( SELECT TOP 1 V.[Value] FROM [Attribute] A INNER JOIN [AttributeValue] V ON V.[AttributeId] = A.[Id] WHERE A.[Guid] = '5FD00163-5EDC-4E36-93D3-D917EFDEF63B' ) AS [Value]
		FROM [GroupType]
		WHERE [GroupTypePurposeValueId] = @CheckInTemplatePurposeId

		UNION ALL

		-- Enable Override
		SELECT 
				( SELECT [Id] FROM [Attribute] WHERE [Guid] = '745154D6-E108-41C2-9001-7AD543CFC75D' ) AS [AttributeId]
			,[Id] AS [EntityId]
			,( SELECT TOP 1 V.[Value] FROM [Attribute] A INNER JOIN [AttributeValue] V ON V.[AttributeId] = A.[Id] WHERE A.[Guid] = 'B4CF0964-F2AD-482F-A50C-570159FD1FFC' ) AS [Value]
		FROM [GroupType]
		WHERE [GroupTypePurposeValueId] = @CheckInTemplatePurposeId

		UNION ALL

		-- Enable Manager
		SELECT 
				( SELECT [Id] FROM [Attribute] WHERE [Guid] = '5BF4C3CD-052F-4A21-B677-21811C5ABEDD' ) AS [AttributeId]
			,[Id] AS [EntityId]
			,( SELECT TOP 1 V.[Value] FROM [Attribute] A INNER JOIN [AttributeValue] V ON V.[AttributeId] = A.[Id] WHERE A.[Guid] = '4C3B1C57-AF71-48A7-A8FE-36702BD67E78' ) AS [Value]
		FROM [GroupType]
		WHERE [GroupTypePurposeValueId] = @CheckInTemplatePurposeId

		UNION ALL

		-- Min Phone Len
		SELECT 
				( SELECT [Id] FROM [Attribute] WHERE [Guid] = 'DA3417AC-7138-4219-9363-7AB37D614350' ) AS [AttributeId]
			,[Id] AS [EntityId]
			,( SELECT TOP 1 V.[Value] FROM [Attribute] A INNER JOIN [AttributeValue] V ON V.[AttributeId] = A.[Id] WHERE A.[Guid] = 'BC0D6B30-C82C-4FB8-B9DC-1C44184812B1' ) AS [Value]
		FROM [GroupType]
		WHERE [GroupTypePurposeValueId] = @CheckInTemplatePurposeId

		UNION ALL

		-- Max Phone Len
		SELECT 
				( SELECT [Id] FROM [Attribute] WHERE [Guid] = '93CA081A-6128-4BBE-BF2B-DF55B7AA817C' ) AS [AttributeId]
			,[Id] AS [EntityId]
			,( SELECT TOP 1 V.[Value] FROM [Attribute] A INNER JOIN [AttributeValue] V ON V.[AttributeId] = A.[Id] WHERE A.[Guid] = '7182EBDD-8128-46E7-8635-D6C664E15F63' ) AS [Value]
		FROM [GroupType]
		WHERE [GroupTypePurposeValueId] = @CheckInTemplatePurposeId

		UNION ALL

		-- Search Regex
		SELECT 
				( SELECT [Id] FROM [Attribute] WHERE [Guid] = 'DE32D84F-5653-41F9-9B34-D04BA9024670' ) AS [AttributeId]
			,[Id] AS [EntityId]
			,( SELECT TOP 1 V.[Value] FROM [Attribute] A INNER JOIN [AttributeValue] V ON V.[AttributeId] = A.[Id] WHERE A.[Guid] = '5DCF5D08-2367-4CB9-9684-27631B054F97' ) AS [Value]
		FROM [GroupType]
		WHERE [GroupTypePurposeValueId] = @CheckInTemplatePurposeId

		UNION ALL

		-- Search Type
		SELECT 
				( SELECT [Id] FROM [Attribute] WHERE [Guid] = 'BBF88ADA-3C2E-4A0B-A9AE-5B07D4557129' ) AS [AttributeId]
			,[Id] AS [EntityId]
			,( SELECT TOP 1 V.[Value] FROM [Attribute] A INNER JOIN [AttributeValue] V ON V.[AttributeId] = A.[Id] WHERE A.[Guid] = 'E5BD71C5-1D30-40F4-8E62-D3A4E68A7F86' ) AS [Value]
		FROM [GroupType]
		WHERE [GroupTypePurposeValueId] = @CheckInTemplatePurposeId

		UNION ALL

		-- Age Required
		SELECT 
				( SELECT [Id] FROM [Attribute] WHERE [Guid] = '46C8DC94-D57E-4B9A-8FB9-1A797DD3D525' ) AS [AttributeId]
			,[Id] AS [EntityId]
			,( SELECT TOP 1 V.[Value] FROM [Attribute] A INNER JOIN [AttributeValue] V ON V.[AttributeId] = A.[Id] WHERE A.[Guid] = '70CA83F0-D8E6-46D2-824E-34C50D16E6F6' ) AS [Value]
		FROM [GroupType]
		WHERE [GroupTypePurposeValueId] = @CheckInTemplatePurposeId

		UNION ALL

		-- Phone Search Type
		SELECT 
				( SELECT [Id] FROM [Attribute] WHERE [Guid] = '34D0971A-53AB-4D43-94EA-E251081D7F93' ) AS [AttributeId]
			,[Id] AS [EntityId]
			,( SELECT TOP 1 V.[Value] FROM [Attribute] A INNER JOIN [AttributeValue] V ON V.[AttributeId] = A.[Id] WHERE A.[Guid] = '81D0D72D-C9EA-48A4-A0DF-EB2ABC042B94' ) AS [Value]
		FROM [GroupType]
		WHERE [GroupTypePurposeValueId] = @CheckInTemplatePurposeId

		UNION ALL

		-- Max Results
		SELECT 
				( SELECT [Id] FROM [Attribute] WHERE [Guid] = '24A8A4B0-F54D-490A-89F6-476B044CD114' ) AS [AttributeId]
			,[Id] AS [EntityId]
			,( SELECT TOP 1 V.[Value] FROM [Attribute] A INNER JOIN [AttributeValue] V ON V.[AttributeId] = A.[Id] WHERE A.[Guid] = 'BC30C3AA-B249-4CC7-A5A3-89A933DE689D' ) AS [Value]
		FROM [GroupType]
		WHERE [GroupTypePurposeValueId] = @CheckInTemplatePurposeId

		UNION ALL

		-- Display Location Count
		SELECT 
				( SELECT [Id] FROM [Attribute] WHERE [Guid] = '17DA47FF-EC64-4A97-B46B-394626C25100' ) AS [AttributeId]
			,[Id] AS [EntityId]
			,'True' AS [Value]
		FROM [GroupType]
		WHERE [GroupTypePurposeValueId] = @CheckInTemplatePurposeId

		UNION ALL

		-- Security Code Length
		SELECT 
				( SELECT [Id] FROM [Attribute] WHERE [Guid] = '712CFC8A-7B67-4793-A71E-E2EEB2D1048D' ) AS [AttributeId]
			,[Id] AS [EntityId]
			,( SELECT TOP 1 V.[Value] FROM [Attribute] A INNER JOIN [AttributeValue] V ON V.[AttributeId] = A.[Id] WHERE A.[Guid] = 'D57F42C9-E497-4FEE-8231-4FE2D13DC191' ) AS [Value]
		FROM [GroupType]
		WHERE [GroupTypePurposeValueId] = @CheckInTemplatePurposeId

		UNION ALL

		-- Reuse Code
		SELECT 
				( SELECT [Id] FROM [Attribute] WHERE [Guid] = '2B1E044B-6BD7-4F91-86A1-2D854A3BBF2D' ) AS [AttributeId]
			,[Id] AS [EntityId]
			,( SELECT TOP 1 V.[Value] FROM [Attribute] A INNER JOIN [AttributeValue] V ON V.[AttributeId] = A.[Id] WHERE A.[Guid] = 'BB7591B6-B264-46D9-8A5F-F2E13811AA5E' ) AS [Value]
		FROM [GroupType]
		WHERE [GroupTypePurposeValueId] = @CheckInTemplatePurposeId

		UNION ALL

		-- One Parent Label
		SELECT 
				( SELECT [Id] FROM [Attribute] WHERE [Guid] = 'EC7FA927-95D0-44A8-8AB3-2D74A9FA2F26' ) AS [AttributeId]
			,[Id] AS [EntityId]
			,'False' AS [Value]
		FROM [GroupType]
		WHERE [GroupTypePurposeValueId] = @CheckInTemplatePurposeId

		UNION ALL

		-- Auto Select Days Back
		SELECT 
				( SELECT [Id] FROM [Attribute] WHERE [Guid] = '5BA86237-B327-4A2E-8992-6AE784B2A41C' ) AS [AttributeId]
			,[Id] AS [EntityId]
			,'10' AS [Value]
		FROM [GroupType]
		WHERE [GroupTypePurposeValueId] = @CheckInTemplatePurposeId

    ) AS AV 
		ON RV.[AttributeId] = AV.[AttributeId]
		AND RV.[EntityId] = AV.[EntityId]
	WHEN MATCHED THEN
		UPDATE SET [Value] = AV.[Value]
	WHEN NOT MATCHED BY TARGET THEN
		INSERT ( [IsSystem],[AttributeId],[EntityId],[Value],[Guid]	)
		VALUES (0 ,AV.[AttributeId] ,AV.[EntityId] ,AV.[Value] ,NEWID() )
	;

	UPDATE [GroupType] SET [IconCssClass] = 'fa fa-clock-o' WHERE [Guid]  = '92435F1D-E525-4FD2-BEC7-4956DC056A2B' AND ( [IconCssClass] IS NULL OR [IconCssClass] = '' )
	UPDATE [GroupType] SET [IconCssClass] = 'fa fa-child' WHERE [Guid]  = 'FEDD389A-616F-4A53-906C-63D8255631C5' AND ( [IconCssClass] IS NULL OR [IconCssClass] = '' )

END
