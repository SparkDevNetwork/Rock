DECLARE @simpleCommunicationDetailBlockId INT = (
 SELECT TOP 1 Id
 FROM [Block]
 WHERE [Guid] = 'A02F7695-4C6E-44E9-84CB-42E6F51F285F'
 )

 DECLARE @wizardCommunicationDetailBlockId INT = (
 SELECT TOP 1 Id
 FROM [Block]
 WHERE [Guid] = '25D890B9-9609-4B63-AD25-4AE427205563'
 )

DECLARE @entityTypeIdBlock INT = (
 SELECT TOP 1 Id
 FROM EntityType
 WHERE [Guid] = 'D89555CA-9AE4-4D62-8AF1-E5E463C1EF65'
 ) /* Rock.Model.Block */


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
 ,@wizardCommunicationDetailBlockId
 ,[Order]
 ,[Action]
 ,AllowOrDeny
 ,SpecialRole
 ,GroupId
 ,PersonAliasId
 ,newid()
FROM [Auth]
WHERE EntityTypeId = @entityTypeIdBlock
 AND EntityId = @simpleCommunicationDetailBlockId
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
 AND EntityId = @wizardCommunicationDetailBlockId
 AND [Action] = 'Approve'
 )