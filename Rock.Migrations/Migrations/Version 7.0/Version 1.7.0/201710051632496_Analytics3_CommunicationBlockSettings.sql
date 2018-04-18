DECLARE @simpleCommunicationEntryBlockId INT = (
 SELECT TOP 1 Id
 FROM [Block]
 WHERE [Guid] = 'BD9B2F32-AB18-4761-80C9-FDA4DBEEA9EC'
 )
DECLARE @wizardCommunicationEntryBlockId INT = (
 SELECT TOP 1 Id
 FROM [Block]
 WHERE [Guid] = '82D5B1A0-1C17-464E-9EC5-414549FB44C7'
 )
DECLARE @simpleCommunicationEntryBlockTypeId INT = (
 SELECT TOP 1 Id
 FROM [BlockType]
 WHERE [Guid] = 'D9834641-7F39-4CFA-8CB2-E64068127565'
 );
DECLARE @wizardCommunicationEntryBlockTypeId INT = (
 SELECT TOP 1 Id
 FROM [BlockType]
 WHERE [Guid] = 'F7D464E2-5F7C-47BA-84DB-7CC7B0B623C0'
 );
DECLARE @entityTypeIdBlock INT = (
 SELECT TOP 1 Id
 FROM EntityType
 WHERE [Guid] = 'D89555CA-9AE4-4D62-8AF1-E5E463C1EF65'
 ) /* Rock.Model.Block */
DECLARE @EnabledLavaCommands NVARCHAR(MAX) = (
 SELECT TOP 1 [Value]
 FROM AttributeValue
 WHERE EntityId = @simpleCommunicationEntryBlockId
 AND AttributeId IN (
 SELECT Id
 FROM Attribute
 WHERE EntityTypeId = @entityTypeIdBlock
 AND EntityTypeQualifierColumn = 'BlockTypeId'
 AND EntityTypeQualifierValue = @simpleCommunicationEntryBlockTypeId
 AND [Key] = 'EnabledLavaCommands'
 )
 )
DECLARE @MaximumRecipients NVARCHAR(MAX) = (
 SELECT TOP 1 [Value]
 FROM AttributeValue
 WHERE EntityId = @simpleCommunicationEntryBlockId
 AND AttributeId IN (
 SELECT Id
 FROM Attribute
 WHERE EntityTypeId = @entityTypeIdBlock
 AND EntityTypeQualifierColumn = 'BlockTypeId'
 AND EntityTypeQualifierValue = @simpleCommunicationEntryBlockTypeId
 AND [Key] = 'MaximumRecipients'
 )
 )
DECLARE @SendWhenApproved NVARCHAR(MAX) = (
 SELECT TOP 1 [Value]
 FROM AttributeValue
 WHERE EntityId = @simpleCommunicationEntryBlockId
 AND AttributeId IN (
 SELECT Id
 FROM Attribute
 WHERE EntityTypeId = @entityTypeIdBlock
 AND EntityTypeQualifierColumn = 'BlockTypeId'
 AND EntityTypeQualifierValue = @simpleCommunicationEntryBlockTypeId
 AND [Key] = 'SendWhenApproved'
 )
 )
UPDATE [AttributeValue]
SET [Value] = @EnabledLavaCommands
WHERE AttributeId IN (
 SELECT Id
 FROM Attribute
 WHERE [Guid] = 'ADEF85D0-F870-4883-9694-396EC5BF8F52'
 )
 AND EntityId = @wizardCommunicationEntryBlockId
UPDATE [AttributeValue]
SET [Value] = @MaximumRecipients
WHERE AttributeId IN (
 SELECT Id
 FROM Attribute
 WHERE [Guid] = 'C9468757-5DDB-448A-BDB3-DE1AFCB4CFB5'
 )
 AND EntityId = @wizardCommunicationEntryBlockId
UPDATE [AttributeValue]
SET [Value] = @SendWhenApproved
WHERE AttributeId IN (
 SELECT Id
 FROM Attribute
 WHERE [Guid] = 'A2D43F93-3200-41AC-A5DB-DAD7EB147873'
 )
 AND EntityId = @wizardCommunicationEntryBlockId
INSERT INTO [Auth] (
 EntityTypeid
 ,EntityId
 ,[Order]
 ,[Action]
 ,AllowOrDeny
 ,SpecialRole
 ,GroupId
 ,PersonAliasId
 ,[Guid]
 )
SELECT @entityTypeIdBlock
 ,@wizardCommunicationEntryBlockId
 ,[Order]
 ,[Action]
 ,AllowOrDeny
 ,SpecialRole
 ,GroupId
 ,PersonAliasId
 ,newid()
FROM [Auth]
WHERE EntityTypeId = @entityTypeIdBlock
 AND EntityId = @simpleCommunicationEntryBlockId
 AND [Action] = 'Approve'
 AND CONCAT (
 GroupId
 ,PersonAliasId
 ,SpecialRole
 ) NOT IN (
 SELECT CONCAT (
 GroupId
 ,PersonAliasId
 ,SpecialRole
 )
 FROM Auth
 WHERE EntityTypeId = @entityTypeIdBlock
 AND EntityId = @wizardCommunicationEntryBlockId
 AND [Action] = 'Approve'
 )
