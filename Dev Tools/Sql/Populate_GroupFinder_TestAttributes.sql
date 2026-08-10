SET NOCOUNT ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON

-- =============================================
-- Description: Seeds a few Small Group (GroupTypeId 25) attributes of varied field types and sets
--              values on several map-located sample groups, to exercise the Group Finder card display,
--              More Filters, and Featured pills. Values are deliberately left blank on some groups so the
--              "hide empty card attribute" and "exclude groups missing a filter value" behaviors can be tested.
--              Idempotent: re-running updates in place rather than duplicating.
-- Date: 2026-08-05
-- =============================================

DECLARE @GroupTypeId INT = 25
DECLARE @GroupEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Group')
DECLARE @QualifierValue NVARCHAR(50) = CAST(@GroupTypeId AS NVARCHAR(50))

DECLARE @BooleanFieldTypeId INT = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '1EDAFDED-DFE6-4334-B019-6EECBA89E05A')
DECLARE @SingleSelectFieldTypeId INT = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '7525C4CB-EE6B-41D4-9B64-A08048D5A5C0')
DECLARE @TextFieldTypeId INT = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA')

-- Stable attribute guids so re-runs are idempotent.
DECLARE @ChildcareGuid UNIQUEIDENTIFIER = '7F1A2B3C-1111-4A5B-9C0D-A0B1C2D3E401'
DECLARE @FrequencyGuid UNIQUEIDENTIFIER = '7F1A2B3C-1111-4A5B-9C0D-A0B1C2D3E402'
DECLARE @FocusGuid     UNIQUEIDENTIFIER = '7F1A2B3C-1111-4A5B-9C0D-A0B1C2D3E403'
DECLARE @LeaderGuid    UNIQUEIDENTIFIER = '7F1A2B3C-1111-4A5B-9C0D-A0B1C2D3E404'

DECLARE @Order INT = (SELECT ISNULL(MAX([Order]), 0) FROM [Attribute]
    WHERE [EntityTypeId] = @GroupEntityTypeId AND [EntityTypeQualifierColumn] = 'GroupTypeId' AND [EntityTypeQualifierValue] = @QualifierValue)

BEGIN TRANSACTION

-- ---------------------------------------------
-- Attributes
-- ---------------------------------------------
IF NOT EXISTS (SELECT 1 FROM [Attribute] WHERE [Guid] = @ChildcareGuid)
BEGIN
    INSERT INTO [Attribute] ([IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],[Key],[Name],[Description],[Order],[IsGridColumn],[IsMultiValue],[IsRequired],[AllowSearch],[IsIndexEnabled],[IsAnalytic],[IsAnalyticHistory],[IsActive],[IsPublic],[ShowOnBulk],[EnableHistory],[IsSuppressHistoryLogging],[IconCssClass],[Guid],[CreatedDateTime],[ModifiedDateTime])
    VALUES (0,@BooleanFieldTypeId,@GroupEntityTypeId,'GroupTypeId',@QualifierValue,'ChildcareProvided','Childcare Provided','Whether childcare is provided for this group.',@Order + 1,0,0,0,0,0,0,0,1,0,0,0,0,'ti ti-baby-carriage',@ChildcareGuid,GETDATE(),GETDATE())
END

IF NOT EXISTS (SELECT 1 FROM [Attribute] WHERE [Guid] = @FrequencyGuid)
BEGIN
    INSERT INTO [Attribute] ([IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],[Key],[Name],[Description],[Order],[IsGridColumn],[IsMultiValue],[IsRequired],[AllowSearch],[IsIndexEnabled],[IsAnalytic],[IsAnalyticHistory],[IsActive],[IsPublic],[ShowOnBulk],[EnableHistory],[IsSuppressHistoryLogging],[IconCssClass],[Guid],[CreatedDateTime],[ModifiedDateTime])
    VALUES (0,@SingleSelectFieldTypeId,@GroupEntityTypeId,'GroupTypeId',@QualifierValue,'MeetingFrequency','Meeting Frequency','How often the group meets.',@Order + 2,0,0,0,0,0,0,0,1,0,0,0,0,'ti ti-calendar-repeat',@FrequencyGuid,GETDATE(),GETDATE())
END

IF NOT EXISTS (SELECT 1 FROM [Attribute] WHERE [Guid] = @FocusGuid)
BEGIN
    INSERT INTO [Attribute] ([IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],[Key],[Name],[Description],[Order],[IsGridColumn],[IsMultiValue],[IsRequired],[AllowSearch],[IsIndexEnabled],[IsAnalytic],[IsAnalyticHistory],[IsActive],[IsPublic],[ShowOnBulk],[EnableHistory],[IsSuppressHistoryLogging],[IconCssClass],[Guid],[CreatedDateTime],[ModifiedDateTime])
    VALUES (0,@SingleSelectFieldTypeId,@GroupEntityTypeId,'GroupTypeId',@QualifierValue,'GroupFocus','Group Focus','Who the group is intended for.',@Order + 3,0,0,0,0,0,0,0,1,0,0,0,0,'ti ti-users-group',@FocusGuid,GETDATE(),GETDATE())
END

IF NOT EXISTS (SELECT 1 FROM [Attribute] WHERE [Guid] = @LeaderGuid)
BEGIN
    INSERT INTO [Attribute] ([IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],[Key],[Name],[Description],[Order],[IsGridColumn],[IsMultiValue],[IsRequired],[AllowSearch],[IsIndexEnabled],[IsAnalytic],[IsAnalyticHistory],[IsActive],[IsPublic],[ShowOnBulk],[EnableHistory],[IsSuppressHistoryLogging],[IconCssClass],[Guid],[CreatedDateTime],[ModifiedDateTime])
    VALUES (0,@TextFieldTypeId,@GroupEntityTypeId,'GroupTypeId',@QualifierValue,'LeaderName','Leader Name','The group leader''s name.',@Order + 4,0,0,0,0,0,0,0,1,0,0,0,0,'ti ti-user',@LeaderGuid,GETDATE(),GETDATE())
END

-- Resolve attribute ids now that they exist.
DECLARE @ChildcareId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = @ChildcareGuid)
DECLARE @FrequencyId  INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = @FrequencyGuid)
DECLARE @FocusId      INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = @FocusGuid)
DECLARE @LeaderId     INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = @LeaderGuid)

-- ---------------------------------------------
-- Attribute qualifiers (Single-Select option lists; Boolean Yes/No labels)
-- ---------------------------------------------
IF NOT EXISTS (SELECT 1 FROM [AttributeQualifier] WHERE [AttributeId] = @ChildcareId AND [Key] = 'truetext')
    INSERT INTO [AttributeQualifier] ([IsSystem],[AttributeId],[Key],[Value],[Guid]) VALUES (0,@ChildcareId,'truetext','Yes',NEWID())
IF NOT EXISTS (SELECT 1 FROM [AttributeQualifier] WHERE [AttributeId] = @ChildcareId AND [Key] = 'falsetext')
    INSERT INTO [AttributeQualifier] ([IsSystem],[AttributeId],[Key],[Value],[Guid]) VALUES (0,@ChildcareId,'falsetext','No',NEWID())

IF NOT EXISTS (SELECT 1 FROM [AttributeQualifier] WHERE [AttributeId] = @FrequencyId AND [Key] = 'values')
    INSERT INTO [AttributeQualifier] ([IsSystem],[AttributeId],[Key],[Value],[Guid]) VALUES (0,@FrequencyId,'values','Weekly,Every Other Week,Monthly',NEWID())
IF NOT EXISTS (SELECT 1 FROM [AttributeQualifier] WHERE [AttributeId] = @FrequencyId AND [Key] = 'fieldtype')
    INSERT INTO [AttributeQualifier] ([IsSystem],[AttributeId],[Key],[Value],[Guid]) VALUES (0,@FrequencyId,'fieldtype','ddl',NEWID())

IF NOT EXISTS (SELECT 1 FROM [AttributeQualifier] WHERE [AttributeId] = @FocusId AND [Key] = 'values')
    INSERT INTO [AttributeQualifier] ([IsSystem],[AttributeId],[Key],[Value],[Guid]) VALUES (0,@FocusId,'values','Men,Women,Coed,Families',NEWID())
IF NOT EXISTS (SELECT 1 FROM [AttributeQualifier] WHERE [AttributeId] = @FocusId AND [Key] = 'fieldtype')
    INSERT INTO [AttributeQualifier] ([IsSystem],[AttributeId],[Key],[Value],[Guid]) VALUES (0,@FocusId,'fieldtype','ddl',NEWID())

-- ---------------------------------------------
-- Values across every map-located Small Group, spread over the option lists with deliberate gaps so
-- empty-value hiding and exclude-missing filtering stay testable:
--   Childcare  ~ half Yes, a quarter No, a quarter blank
--   Frequency  ~ Weekly / Every Other Week / Monthly, about one in five blank
--   Focus      ~ Men / Women / Coed / Families, about one in five blank
--   Leader     ~ derived from the group name, about one in three blank
-- The spread is keyed off a stable row number so re-running lands the same values.
-- ---------------------------------------------
;WITH [geo] AS (
    SELECT g.[Id], g.[Name], ROW_NUMBER() OVER (ORDER BY g.[Id]) AS [rn]
    FROM [Group] g
    WHERE g.[GroupTypeId] = @GroupTypeId AND g.[IsActive] = 1
      AND EXISTS (SELECT 1 FROM [GroupLocation] gl JOIN [Location] l ON l.[Id] = gl.[LocationId] WHERE gl.[GroupId] = g.[Id] AND l.[GeoPoint] IS NOT NULL)
),
[vals] AS (
    SELECT [Id] AS [EntityId], @ChildcareId AS [AttributeId],
        CASE [rn] % 4 WHEN 1 THEN 'True' WHEN 2 THEN 'True' WHEN 3 THEN 'False' ELSE NULL END AS [Val] FROM [geo]
    UNION ALL
    SELECT [Id], @FrequencyId,
        CASE [rn] % 5 WHEN 1 THEN 'Weekly' WHEN 2 THEN 'Every Other Week' WHEN 3 THEN 'Monthly' WHEN 4 THEN 'Weekly' ELSE NULL END FROM [geo]
    UNION ALL
    SELECT [Id], @FocusId,
        CASE [rn] % 5 WHEN 1 THEN 'Men' WHEN 2 THEN 'Women' WHEN 3 THEN 'Coed' WHEN 4 THEN 'Families' ELSE NULL END FROM [geo]
    UNION ALL
    SELECT [Id], @LeaderId,
        CASE WHEN [rn] % 3 = 0 THEN NULL ELSE NULLIF(LTRIM(RTRIM(REPLACE(REPLACE([Name], '''s Group', ''), ' Group', ''))), '') END FROM [geo]
)
MERGE INTO [AttributeValue] AS tgt
USING (SELECT [EntityId], [AttributeId], [Val] FROM [vals] WHERE [Val] IS NOT NULL) AS src
ON tgt.[AttributeId] = src.[AttributeId] AND tgt.[EntityId] = src.[EntityId]
WHEN MATCHED THEN
    UPDATE SET tgt.[Value] = src.[Val], tgt.[ModifiedDateTime] = GETDATE(), tgt.[IsPersistedValueDirty] = 1
WHEN NOT MATCHED THEN
    INSERT ([IsSystem],[AttributeId],[EntityId],[Value],[Guid],[CreatedDateTime],[ModifiedDateTime],[IsPersistedValueDirty])
    VALUES (0, src.[AttributeId], src.[EntityId], src.[Val], NEWID(), GETDATE(), GETDATE(), 1);

COMMIT TRANSACTION

-- ---------------------------------------------
-- Verify: how many groups landed on each value (blanks are simply absent rows).
-- ---------------------------------------------
SELECT a.[Name] AS [Attribute], av.[Value], COUNT(*) AS [Groups]
FROM [AttributeValue] av
JOIN [Attribute] a ON a.[Id] = av.[AttributeId]
WHERE a.[Guid] IN ('7F1A2B3C-1111-4A5B-9C0D-A0B1C2D3E401','7F1A2B3C-1111-4A5B-9C0D-A0B1C2D3E402','7F1A2B3C-1111-4A5B-9C0D-A0B1C2D3E403','7F1A2B3C-1111-4A5B-9C0D-A0B1C2D3E404')
GROUP BY a.[Name], av.[Value]
ORDER BY a.[Name], av.[Value]
