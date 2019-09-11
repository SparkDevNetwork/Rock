IF NOT EXISTS (
		SELECT *
		FROM [ContentChannelType]
		WHERE [Guid] = 'BCFE276D-96A3-46E5-BF9F-7B68CC44DA33'
		)
	INSERT INTO [ContentChannelType] (
		[IsSystem]
		,[Name]
		,[DateRangeType]
		,[DisablePriority]
        ,[ShowInChannelList]
		,[Guid]
		)
	VALUES (
		1
		,'Content Component'
		,3
		,0
        ,0
		,'BCFE276D-96A3-46E5-BF9F-7B68CC44DA33'
		)

DECLARE @ContentComponentContentChannelTypeId INT = (
		SELECT TOP 1 [Id]
		FROM [ContentChannelType]
		WHERE Guid = 'BCFE276D-96A3-46E5-BF9F-7B68CC44DA33'
		)
	,@ContentChannelItemEntityTypeId INT = (
		SELECT TOP 1 [Id]
		FROM [EntityType]
		WHERE [Name] = 'Rock.Model.ContentChannelItem'
		)
	,@ContentChannelEntityTypeId INT = (
		SELECT TOP 1 [Id]
		FROM [EntityType]
		WHERE [Name] = 'Rock.Model.ContentChannel'
		)
	,@SINGLE_SELECTFieldTypeId INT = (
		SELECT TOP 1 [Id]
		FROM [FieldType]
		WHERE [Guid] = '7525C4CB-EE6B-41D4-9B64-A08048D5A5C0'
		)
	,@COLORFieldTypeId INT = (
		SELECT TOP 1 [Id]
		FROM [FieldType]
		WHERE [Guid] = 'D747E6AE-C383-4E22-8846-71518E3DD06F'
		)
	,@IMAGEFieldTypeId INT = (
		SELECT TOP 1 [Id]
		FROM [FieldType]
		WHERE [Guid] = '97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D'
		)

-- Image ContentChannelItem Attribute on Content Component
IF NOT EXISTS (
		SELECT *
		FROM [Attribute]
		WHERE [Guid] = '4AAF1BB1-901F-4234-9F60-401DBBCEC3F3'
		)
BEGIN
	INSERT INTO [Attribute] (
		[IsSystem]
		,[FieldTypeId]
		,[EntityTypeId]
		,[EntityTypeQualifierColumn]
		,[EntityTypeQualifierValue]
		,[Key]
		,[Name]
		,[Description]
		,[Order]
		,[IsGridColumn]
		,[DefaultValue]
		,[IsMultiValue]
		,[IsRequired]
		,[Guid]
		)
	VALUES (
		0
		,@IMAGEFieldTypeId
		,@ContentChannelItemEntityTypeId
		,'ContentChannelTypeId'
		,CAST(@ContentComponentContentChannelTypeId AS VARCHAR)
		,'Image'
		,'Image'
		,''
		,0
		,0
		,''
		,0
		,0
		,'4AAF1BB1-901F-4234-9F60-401DBBCEC3F3'
		)

	DECLARE @ItemAttributeId INT = SCOPE_IDENTITY();

	INSERT INTO [AttributeQualifier] (
		[IsSystem]
		,[AttributeId]
		,[Key]
		,[Value]
		,[Guid]
		)
	VALUES (
		0
		,@ItemAttributeId
		,'binaryFileType'
		,'8dbf874c-f3c2-4848-8137-c963c431eb0b'
		,'01487182-6B2B-4820-B548-ED75F3E20F45'
		)
		,(
		0
		,@ItemAttributeId
		,'formatAsLink'
		,'False'
		,'C63B1236-C49F-4255-8C58-B620A837507D'
		)
		,(
		0
		,@ItemAttributeId
		,'img_tag_template'
		,''
		,'1700F932-B036-4107-B57B-379D2087A5D3'
		)
END

-- TitleSize ContentChannel Attribute on Content Component 
IF NOT EXISTS (
		SELECT *
		FROM [Attribute]
		WHERE [Guid] = 'F89C6A73-341E-445E-8529-9C44BBF0ED79'
		)
BEGIN
	INSERT INTO [Attribute] (
		[IsSystem]
		,[FieldTypeId]
		,[EntityTypeId]
		,[EntityTypeQualifierColumn]
		,[EntityTypeQualifierValue]
		,[Key]
		,[Name]
		,[Description]
		,[Order]
		,[IsGridColumn]
		,[DefaultValue]
		,[IsMultiValue]
		,[IsRequired]
		,[Guid]
		)
	VALUES (
		0
		,@SINGLE_SELECTFieldTypeId
		,@ContentChannelEntityTypeId
		,'ContentChannelTypeId'
		,CAST(@ContentComponentContentChannelTypeId AS VARCHAR)
		,'TitleSize'
		,'Title Size'
		,''
		,0
		,0
		,''
		,0
		,0
		,'F89C6A73-341E-445E-8529-9C44BBF0ED79'
		)

	DECLARE @TitleSizeAttributeId INT = SCOPE_IDENTITY();

	INSERT INTO [AttributeQualifier] (
		[IsSystem]
		,[AttributeId]
		,[Key]
		,[Value]
		,[Guid]
		)
	VALUES (
		0
		,@TitleSizeAttributeId
		,'fieldtype'
		,'rb'
		,'9575FC56-68E8-403B-8BE6-BC05ED0080EC'
		)
		,(
		0
		,@TitleSizeAttributeId
		,'values'
		,'h1^H1,h2^H2,h3^H3,h4^H4,h5^H5'
		,'A6B49F25-F61A-4B40-8239-73D09E4575D3'
		)
END

-- BackgroundColor ContentChannel Attribute on Content Component 
IF NOT EXISTS (
		SELECT *
		FROM [Attribute]
		WHERE [Guid] = '5A7A161B-5126-41CF-BF2E-313BB4A7FCA3'
		)
BEGIN
	INSERT INTO [Attribute] (
		[IsSystem]
		,[FieldTypeId]
		,[EntityTypeId]
		,[EntityTypeQualifierColumn]
		,[EntityTypeQualifierValue]
		,[Key]
		,[Name]
		,[Description]
		,[Order]
		,[IsGridColumn]
		,[DefaultValue]
		,[IsMultiValue]
		,[IsRequired]
		,[Guid]
		)
	VALUES (
		0
		,@COLORFieldTypeId
		,@ContentChannelEntityTypeId
		,'ContentChannelTypeId'
		,CAST(@ContentComponentContentChannelTypeId AS VARCHAR)
		,'BackgroundColor'
		,'Background Color'
		,''
		,3
		,0
		,''
		,0
		,0
		,'5A7A161B-5126-41CF-BF2E-313BB4A7FCA3'
		)

	DECLARE @BackgroundColorAttributeId INT = SCOPE_IDENTITY();

	INSERT INTO [AttributeQualifier] (
		[IsSystem]
		,[AttributeId]
		,[Key]
		,[Value]
		,[Guid]
		)
	VALUES (
		0
		,@BackgroundColorAttributeId
		,'selectiontype'
		,'Color Picker'
		,'9BF6D564-00EC-4117-AEDF-3670224440FA'
		)
END


-- ForegroundColor ContentChannel Attribute on Content Component 
IF NOT EXISTS (
		SELECT *
		FROM [Attribute]
		WHERE [Guid] = 'E2784B1D-E04B-4CCE-ADF0-808E4B0D3C28'
		)
BEGIN
	INSERT INTO [Attribute] (
		[IsSystem]
		,[FieldTypeId]
		,[EntityTypeId]
		,[EntityTypeQualifierColumn]
		,[EntityTypeQualifierValue]
		,[Key]
		,[Name]
		,[Description]
		,[Order]
		,[IsGridColumn]
		,[DefaultValue]
		,[IsMultiValue]
		,[IsRequired]
		,[Guid]
		)
	VALUES (
		0
		,@COLORFieldTypeId
		,@ContentChannelEntityTypeId
		,'ContentChannelTypeId'
		,CAST(@ContentComponentContentChannelTypeId AS VARCHAR)
		,'ForegroundColor'
		,'Foreground Color'
		,''
		,2
		,0
		,''
		,0
		,0
		,'E2784B1D-E04B-4CCE-ADF0-808E4B0D3C28'
		)

	DECLARE @ForegroundColorAttributeId INT = SCOPE_IDENTITY();

	INSERT INTO [AttributeQualifier] (
		[IsSystem]
		,[AttributeId]
		,[Key]
		,[Value]
		,[Guid]
		)
	VALUES (
		0
		,@ForegroundColorAttributeId
		,'selectiontype'
		,'Color Picker'
		,'CC809A2E-020B-4916-84E4-D4D7C4651D58'
		)
END

-- ContentAlignment ContentChannel Attribute on Content Component 
IF NOT EXISTS (
		SELECT *
		FROM [Attribute]
		WHERE [Guid] = '8145A715-9095-4B2D-8E3E-A33BAA15BED1'
		)
BEGIN
	INSERT INTO [Attribute] (
		[IsSystem]
		,[FieldTypeId]
		,[EntityTypeId]
		,[EntityTypeQualifierColumn]
		,[EntityTypeQualifierValue]
		,[Key]
		,[Name]
		,[Description]
		,[Order]
		,[IsGridColumn]
		,[DefaultValue]
		,[IsMultiValue]
		,[IsRequired]
		,[Guid]
		)
	VALUES (
		0
		,@SINGLE_SELECTFieldTypeId
		,@ContentChannelEntityTypeId
		,'ContentChannelTypeId'
		,CAST(@ContentComponentContentChannelTypeId AS VARCHAR)
		,'ContentAlignment'
		,'Content Alignment'
		,''
		,1
		,0
		,''
		,0
		,0
		,'8145A715-9095-4B2D-8E3E-A33BAA15BED1'
		)

	DECLARE @ContentAlignmentAttributeId INT = SCOPE_IDENTITY();

	INSERT INTO [AttributeQualifier] (
		[IsSystem]
		,[AttributeId]
		,[Key]
		,[Value]
		,[Guid]
		)
	VALUES (
		0
		,@ContentAlignmentAttributeId
		,'fieldtype'
		,'rb'
		,'7F6C5713-8202-45FC-BB2D-70CD37FE47CF'
		)
		,(
		0
		,@ContentAlignmentAttributeId
		,'values'
		,'Center,Left,Right'
		,'F85D3889-1FFF-4AD2-947F-B0BA45D19D77'
		)
END