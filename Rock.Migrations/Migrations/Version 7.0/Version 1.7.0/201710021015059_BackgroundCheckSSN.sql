
DECLARE @WorkflowTypeId int = ( SELECT TOP 1 [Id] FROM [WorkflowType] WHERE [Guid] = '16D12EF7-C546-4039-9036-B73D118EDC90' )
DECLARE @WFEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '3540E9A7-FE30-43A9-8B0A-A372B63DFC93' )
DECLARE @EncryptedTextFieldTypeId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Guid] = '36167F3E-8CB2-44F9-9022-102F171FBC9A' )
DECLARE @SSNFieldTypeId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Guid] = '4722C99A-C078-464A-968F-13AB5E8E318F' )
IF @WorkflowTypeId IS NOT NULL AND @WFEntityTypeId IS NOT NULL AND @SSNFieldTypeId IS NOT NULL 
BEGIN

	DECLARE @SSNAttributeId int = ( 
		SELECT TOP 1 [Id] 
		FROM [Attribute]
		WHERE [FieldTypeId] = @EncryptedTextFieldTypeId
		AND [EntityTypeId] = @WFEntityTypeId
		AND [EntityTypeQualifierColumn] = 'WorkflowTypeId'
		AND [EntityTypeQualifierValue] = CAST( @WorkflowTypeId as varchar )
	)

	IF @SSNAttributeId IS NOT NULL
	BEGIN

		DELETE [AttributeQualifier] WHERE [AttributeId] = @SSNAttributeId

		UPDATE [Attribute] SET [FieldTypeId] = @SSNFieldTypeId
		WHERE [Id] = @SSNAttributeId

		DECLARE @TextValueAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'E5272B11-A2B8-49DC-860D-8D574E2BC15C' )
		DECLARE @ClearSSNActionTypeId int = ( SELECT TOP 1 [Id] FROM [WorkflowActionType] WHERE [Guid] = 'EE581993-40BB-4D67-82AE-0CC152FE9620' )
		UPDATE [AttributeValue] SET [Value] = '' 
		WHERE [AttributeId] = @TextValueAttributeId
		AND [EntityId] = @ClearSSNActionTypeId

	END

END