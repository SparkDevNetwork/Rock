IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[AIAgentSkill]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[AIAgentSkill]', RESEED, 1 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[AIAgentSkill]', RESEED, 2 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[AIAgent]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[AIAgent]', RESEED, 1 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[AIAgent]', RESEED, 2 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[AIProvider]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[AIProvider]', RESEED, 1 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[AIProvider]', RESEED, 2 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[AISkillTool]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[AISkillTool]', RESEED, 113 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[AISkillTool]', RESEED, 114 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[AISkill]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[AISkill]', RESEED, 21 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[AISkill]', RESEED, 22 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[AchievementType]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[AchievementType]', RESEED, 2 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[AchievementType]', RESEED, 3 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[AssessmentType]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[AssessmentType]', RESEED, 5 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[AssessmentType]', RESEED, 6 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[AssetStorageProvider]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[AssetStorageProvider]', RESEED, 1 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[AssetStorageProvider]', RESEED, 2 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[AttributeMatrixItem]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[AttributeMatrixItem]', RESEED, 3 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[AttributeMatrixItem]', RESEED, 4 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[AttributeMatrixTemplate]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[AttributeMatrixTemplate]', RESEED, 1 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[AttributeMatrixTemplate]', RESEED, 2 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[AttributeMatrix]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[AttributeMatrix]', RESEED, 1 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[AttributeMatrix]', RESEED, 2 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[AttributeQualifier]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[AttributeQualifier]', RESEED, 3507 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[AttributeQualifier]', RESEED, 3508 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[AttributeValue]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[AttributeValue]', RESEED, 8024 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[AttributeValue]', RESEED, 8025 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[Attribute]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[Attribute]', RESEED, 7716 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[Attribute]', RESEED, 7717 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[AuthClaim]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[AuthClaim]', RESEED, 12 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[AuthClaim]', RESEED, 13 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[AuthScope]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[AuthScope]', RESEED, 6 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[AuthScope]', RESEED, 7 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[Auth]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[Auth]', RESEED, 2475 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[Auth]', RESEED, 2476 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[Badge]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[Badge]', RESEED, 19 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[Badge]', RESEED, 20 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[BenevolenceType]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[BenevolenceType]', RESEED, 1 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[BenevolenceType]', RESEED, 2 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[BinaryFileType]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[BinaryFileType]', RESEED, 18 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[BinaryFileType]', RESEED, 19 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[BinaryFile]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[BinaryFile]', RESEED, 133 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[BinaryFile]', RESEED, 134 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[BlockType]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[BlockType]', RESEED, 1136 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[BlockType]', RESEED, 1137 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[Block]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[Block]', RESEED, 1594 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[Block]', RESEED, 1595 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[Campus]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[Campus]', RESEED, 1 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[Campus]', RESEED, 2 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[Category]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[Category]', RESEED, 314 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[Category]', RESEED, 315 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[CheckInLabel]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[CheckInLabel]', RESEED, 4 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[CheckInLabel]', RESEED, 5 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[CommunicationTemplate]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[CommunicationTemplate]', RESEED, 8 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[CommunicationTemplate]', RESEED, 9 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[ConnectionActivityType]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[ConnectionActivityType]', RESEED, 9 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[ConnectionActivityType]', RESEED, 10 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[ConnectionOpportunityCampus]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[ConnectionOpportunityCampus]', RESEED, 3 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[ConnectionOpportunityCampus]', RESEED, 4 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[ConnectionOpportunityConnectorGroup]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[ConnectionOpportunityConnectorGroup]', RESEED, 3 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[ConnectionOpportunityConnectorGroup]', RESEED, 4 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[ConnectionOpportunityGroupConfig]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[ConnectionOpportunityGroupConfig]', RESEED, 3 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[ConnectionOpportunityGroupConfig]', RESEED, 4 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[ConnectionOpportunityGroup]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[ConnectionOpportunityGroup]', RESEED, 3 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[ConnectionOpportunityGroup]', RESEED, 4 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[ConnectionOpportunity]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[ConnectionOpportunity]', RESEED, 3 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[ConnectionOpportunity]', RESEED, 4 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[ConnectionStatus]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[ConnectionStatus]', RESEED, 2 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[ConnectionStatus]', RESEED, 3 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[ConnectionType]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[ConnectionType]', RESEED, 1 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[ConnectionType]', RESEED, 2 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[ContentChannelItemAssociation]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[ContentChannelItemAssociation]', RESEED, 19 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[ContentChannelItemAssociation]', RESEED, 20 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[ContentChannelItemSlug]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[ContentChannelItemSlug]', RESEED, 52 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[ContentChannelItemSlug]', RESEED, 53 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[ContentChannelItem]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[ContentChannelItem]', RESEED, 53 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[ContentChannelItem]', RESEED, 54 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[ContentChannelType]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[ContentChannelType]', RESEED, 12 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[ContentChannelType]', RESEED, 13 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[ContentChannel]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[ContentChannel]', RESEED, 8 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[ContentChannel]', RESEED, 9 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[ContentTopicDomain]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[ContentTopicDomain]', RESEED, 10 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[ContentTopicDomain]', RESEED, 11 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[ContentTopic]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[ContentTopic]', RESEED, 89 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[ContentTopic]', RESEED, 90 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[DataViewFilter]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[DataViewFilter]', RESEED, 68 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[DataViewFilter]', RESEED, 69 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[DataView]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[DataView]', RESEED, 14 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[DataView]', RESEED, 15 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[DefinedType]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[DefinedType]', RESEED, 121 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[DefinedType]', RESEED, 122 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[DefinedValue]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[DefinedValue]', RESEED, 1415 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[DefinedValue]', RESEED, 1416 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[Device]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[Device]', RESEED, 11 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[Device]', RESEED, 12 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[DocumentType]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[DocumentType]', RESEED, 2 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[DocumentType]', RESEED, 3 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[EntityType]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[EntityType]', RESEED, 1402 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[EntityType]', RESEED, 1403 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[EventCalendarContentChannel]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[EventCalendarContentChannel]', RESEED, 2 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[EventCalendarContentChannel]', RESEED, 3 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[EventCalendarItem]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[EventCalendarItem]', RESEED, 7 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[EventCalendarItem]', RESEED, 8 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[EventCalendar]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[EventCalendar]', RESEED, 2 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[EventCalendar]', RESEED, 3 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[EventItemAudience]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[EventItemAudience]', RESEED, 10 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[EventItemAudience]', RESEED, 11 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[EventItemOccurrence]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[EventItemOccurrence]', RESEED, 4 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[EventItemOccurrence]', RESEED, 5 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[EventItem]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[EventItem]', RESEED, 4 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[EventItem]', RESEED, 5 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[ExceptionLog]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[ExceptionLog]', RESEED, 596 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[ExceptionLog]', RESEED, 597 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[FieldType]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[FieldType]', RESEED, 182 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[FieldType]', RESEED, 183 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[FinancialAccount]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[FinancialAccount]', RESEED, 4 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[FinancialAccount]', RESEED, 5 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[FinancialGateway]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[FinancialGateway]', RESEED, 3 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[FinancialGateway]', RESEED, 4 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[FinancialStatementTemplate]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[FinancialStatementTemplate]', RESEED, 1 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[FinancialStatementTemplate]', RESEED, 2 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[FollowingEventType]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[FollowingEventType]', RESEED, 8 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[FollowingEventType]', RESEED, 9 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[FollowingSuggestionType]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[FollowingSuggestionType]', RESEED, 5 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[FollowingSuggestionType]', RESEED, 6 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[GroupLocation]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[GroupLocation]', RESEED, 17 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[GroupLocation]', RESEED, 18 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[GroupMemberScheduleTemplate]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[GroupMemberScheduleTemplate]', RESEED, 4 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[GroupMemberScheduleTemplate]', RESEED, 5 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[GroupMember]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[GroupMember]', RESEED, 13 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[GroupMember]', RESEED, 14 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[GroupRequirementType]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[GroupRequirementType]', RESEED, 1 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[GroupRequirementType]', RESEED, 2 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[GroupTypeRole]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[GroupTypeRole]', RESEED, 63 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[GroupTypeRole]', RESEED, 64 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[GroupType]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[GroupType]', RESEED, 41 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[GroupType]', RESEED, 42 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[Group]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[Group]', RESEED, 80 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[Group]', RESEED, 81 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[HistoryLogin]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[HistoryLogin]', RESEED, 1 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[HistoryLogin]', RESEED, 2 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[HtmlContent]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[HtmlContent]', RESEED, 139 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[HtmlContent]', RESEED, 140 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[IdentityVerificationCode]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[IdentityVerificationCode]', RESEED, 963090 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[IdentityVerificationCode]', RESEED, 963091 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[InteractionChannel]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[InteractionChannel]', RESEED, 17 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[InteractionChannel]', RESEED, 18 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[InteractionComponent]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[InteractionComponent]', RESEED, 3 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[InteractionComponent]', RESEED, 4 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[InteractionDeviceType]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[InteractionDeviceType]', RESEED, 2 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[InteractionDeviceType]', RESEED, 3 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[InteractionSessionLocation]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[InteractionSessionLocation]', RESEED, 1 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[InteractionSessionLocation]', RESEED, 2 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[InteractionSession]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[InteractionSession]', RESEED, 2 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[InteractionSession]', RESEED, 3 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[Interaction]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[Interaction]', RESEED, 4 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[Interaction]', RESEED, 5 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[LavaShortcode]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[LavaShortcode]', RESEED, 28 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[LavaShortcode]', RESEED, 29 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[Layout]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[Layout]', RESEED, 64 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[Layout]', RESEED, 65 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[LearningGradingSystemScale]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[LearningGradingSystemScale]', RESEED, 8 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[LearningGradingSystemScale]', RESEED, 9 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[LearningGradingSystem]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[LearningGradingSystem]', RESEED, 3 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[LearningGradingSystem]', RESEED, 4 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[Location]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[Location]', RESEED, 13 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[Location]', RESEED, 14 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[MergeTemplate]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[MergeTemplate]', RESEED, 6 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[MergeTemplate]', RESEED, 7 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[MetaFirstNameGenderLookup]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[MetaFirstNameGenderLookup]', RESEED, 18278 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[MetaFirstNameGenderLookup]', RESEED, 18279 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[MetaLastNameLookup]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[MetaLastNameLookup]', RESEED, 151671 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[MetaLastNameLookup]', RESEED, 151672 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[MetaNickNameLookup]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[MetaNickNameLookup]', RESEED, 443 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[MetaNickNameLookup]', RESEED, 444 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[MetricCategory]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[MetricCategory]', RESEED, 48 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[MetricCategory]', RESEED, 49 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[MetricPartition]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[MetricPartition]', RESEED, 59 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[MetricPartition]', RESEED, 60 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[MetricValuePartition]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[MetricValuePartition]', RESEED, 3 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[MetricValuePartition]', RESEED, 4 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[MetricValue]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[MetricValue]', RESEED, 3 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[MetricValue]', RESEED, 4 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[Metric]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[Metric]', RESEED, 48 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[Metric]', RESEED, 49 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[NoteType]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[NoteType]', RESEED, 28 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[NoteType]', RESEED, 29 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[PageContext]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[PageContext]', RESEED, 53 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[PageContext]', RESEED, 54 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[PageRoute]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[PageRoute]', RESEED, 765 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[PageRoute]', RESEED, 766 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[Page]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[Page]', RESEED, 861 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[Page]', RESEED, 862 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[PersistedDataset]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[PersistedDataset]', RESEED, 1 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[PersistedDataset]', RESEED, 2 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[PersonAlias]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[PersonAlias]', RESEED, 14 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[PersonAlias]', RESEED, 15 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[PersonPreference]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[PersonPreference]', RESEED, 10 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[PersonPreference]', RESEED, 11 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[Person]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[Person]', RESEED, 5 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[Person]', RESEED, 6 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[PluginMigration]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[PluginMigration]', RESEED, 310 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[PluginMigration]', RESEED, 311 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[RegistrationTemplateFormField]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[RegistrationTemplateFormField]', RESEED, 6 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[RegistrationTemplateFormField]', RESEED, 7 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[RegistrationTemplateForm]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[RegistrationTemplateForm]', RESEED, 2 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[RegistrationTemplateForm]', RESEED, 3 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[RegistrationTemplate]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[RegistrationTemplate]', RESEED, 2 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[RegistrationTemplate]', RESEED, 3 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[ReminderType]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[ReminderType]', RESEED, 2 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[ReminderType]', RESEED, 3 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[ReportField]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[ReportField]', RESEED, 17 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[ReportField]', RESEED, 18 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[Report]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[Report]', RESEED, 4 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[Report]', RESEED, 5 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[RestAction]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[RestAction]', RESEED, 8541 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[RestAction]', RESEED, 8542 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[RestController]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[RestController]', RESEED, 644 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[RestController]', RESEED, 645 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[Schedule]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[Schedule]', RESEED, 25 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[Schedule]', RESEED, 26 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[ServiceJobHistory]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[ServiceJobHistory]', RESEED, 86 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[ServiceJobHistory]', RESEED, 87 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[ServiceJob]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[ServiceJob]', RESEED, 173 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[ServiceJob]', RESEED, 174 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[SignatureDocumentTemplate]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[SignatureDocumentTemplate]', RESEED, 2 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[SignatureDocumentTemplate]', RESEED, 3 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[SiteDomain]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[SiteDomain]', RESEED, 1 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[SiteDomain]', RESEED, 2 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[Site]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[Site]', RESEED, 8 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[Site]', RESEED, 9 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[SmsAction]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[SmsAction]', RESEED, 1 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[SmsAction]', RESEED, 2 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[SmsPipeline]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[SmsPipeline]', RESEED, 1 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[SmsPipeline]', RESEED, 2 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[SnippetType]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[SnippetType]', RESEED, 1 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[SnippetType]', RESEED, 2 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[StepProgram]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[StepProgram]', RESEED, 2 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[StepProgram]', RESEED, 3 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[StepStatus]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[StepStatus]', RESEED, 4 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[StepStatus]', RESEED, 5 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[StepTypePrerequisite]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[StepTypePrerequisite]', RESEED, 1 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[StepTypePrerequisite]', RESEED, 2 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[StepType]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[StepType]', RESEED, 5 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[StepType]', RESEED, 6 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[StreakType]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[StreakType]', RESEED, 1 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[StreakType]', RESEED, 2 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[SystemCommunication]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[SystemCommunication]', RESEED, 51 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[SystemCommunication]', RESEED, 52 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[SystemEmail]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[SystemEmail]', RESEED, 34 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[SystemEmail]', RESEED, 35 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[Tag]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[Tag]', RESEED, 1 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[Tag]', RESEED, 2 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[Theme]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[Theme]', RESEED, 14 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[Theme]', RESEED, 15 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[UserLogin]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[UserLogin]', RESEED, 2 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[UserLogin]', RESEED, 3 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[WorkflowActionFormAttribute]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[WorkflowActionFormAttribute]', RESEED, 351 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[WorkflowActionFormAttribute]', RESEED, 352 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[WorkflowActionForm]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[WorkflowActionForm]', RESEED, 33 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[WorkflowActionForm]', RESEED, 34 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[WorkflowActionType]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[WorkflowActionType]', RESEED, 283 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[WorkflowActionType]', RESEED, 284 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[WorkflowActivityType]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[WorkflowActivityType]', RESEED, 70 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[WorkflowActivityType]', RESEED, 71 ) WITH NO_INFOMSGS;
IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'[dbo].[WorkflowType]' ) AND [last_value] IS NOT NULL )
    DBCC CHECKIDENT ( N'[dbo].[WorkflowType]', RESEED, 23 ) WITH NO_INFOMSGS;
ELSE
    DBCC CHECKIDENT ( N'[dbo].[WorkflowType]', RESEED, 24 ) WITH NO_INFOMSGS;
