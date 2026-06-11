// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20251222 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            SwapBlocksForV19Up();
            UpdateFileManagerPage();
            JPH_AddSystemSenderPerson_Up();
            AddEventRegistrationAdminAuthToTopLevelCategories();
            UpdateMobileCommunicationView();
            AddEventRegistrationAdminAuthToTopLevelCategoriesSecondRound();
            UpdateFileEditorPage();
            UpdateCommunicationTemplatesAndShortcodeForV18_2();
            NA_RemoveDuplicateDefinedTypeCategories();
            NA_SetFacilitatorAndStudentRolesToCanViewUp();
            NA_ConvertCampusColorAttributeUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            JPH_AddSystemSenderPerson_Down();
            UpdateCommunicationTemplatesAndShortcodeForV18_2_Down();
            NA_SetFacilitatorAndStudentRolesToCanViewDown();
            NA_ConvertCampusColorAttributeDown();
        }

        #region 263_SwapFileManager Plugin Migration

        private void UpdateFileManagerPage()
        {
            Sql( @"
UPDATE dbo.[Page]
SET [LayoutId] = (
    SELECT [Id]
    FROM dbo.[Layout]
    WHERE [Guid] = 'C2467799-BB45-4251-8EE6-F0BF27201535'
)
WHERE [Guid] = '6F074DAA-BDCC-44C5-BB89-B899C1AAC6C1';
" );
        }

        /// <summary>
        /// Ensure the Entity, BlockType and Block Setting Attribute records exist
        /// before the swap job runs. Any missing attributes would cause the job to fail.
        /// </summary>
        private void RegisterBlockAttributesForSwap()
        {

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.FileAssetManager
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.FileAssetManager", "File Asset Manager", "Rock.Blocks.Cms.FileAssetManager, Rock.Blocks, Version=19.0.0.0, Culture=neutral, PublicKeyToken=null", false, false, "E357AD54-1725-48B8-997C-23C2587800FB" );

            // Add/Update Obsidian Block Type
            //   Name:File Asset Manager
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.FileAssetManager
            RockMigrationHelper.AddOrUpdateEntityBlockType( "File Asset Manager", "Browse and manage files on the web server or stored on a remote server or 3rd party cloud storage", "Rock.Blocks.Cms.FileAssetManager", "CMS", "535500A7-967F-4DA3-8FCA-CB844203CB3D" );

            // Remove any existing attributes that are assigned to the wrong BlockType (Cleanup from prior swap)
            Sql( @"
-- BlockType Guid
DECLARE @BlockTypeGuid UNIQUEIDENTIFIER = '535500A7-967F-4DA3-8FCA-CB844203CB3D';
DECLARE @BlockTypeId INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = @BlockTypeGuid);

-- Table of Attribute GUIDs
DECLARE @AttributeGuids TABLE (Guid UNIQUEIDENTIFIER);

INSERT INTO @AttributeGuids (Guid)
VALUES
('C872B6A7-36F6-4771-807A-7B4A7E8BAD2C'), -- Browse Mode
('7750F7BB-DC53-41C6-987B-5FD2B02674C2'), -- Enable Asset Storage Providers
('FCBB90A6-965F-4237-9B0F-4384E3FFC991'), -- Enable File Manager
('BD031ADA-5D23-4237-A332-468FAC7282E9'), -- Enable Zip Upload
('80D4544B-563A-4109-9F61-F4E019580B3A'), -- File Editor Page
('67ECB409-F5C5-4487-A60B-FD572B99D95B'), -- Height Mode
('48C3DFAD-2168-4F6C-8DEC-167E49C379B7'), -- Height
('14684245-5768-442D-9BFB-C80E1383775A'); -- Root Folder

DECLARE @AttributeGuid UNIQUEIDENTIFIER;
DECLARE @AttributeId INT;

DECLARE AttributeCursor CURSOR FOR
SELECT Guid FROM @AttributeGuids;

OPEN AttributeCursor;
FETCH NEXT FROM AttributeCursor INTO @AttributeGuid;

WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @AttributeId = [Id]
	FROM [Attribute]
	WHERE [Guid] = @AttributeGuid
		AND [EntityTypeQualifierColumn] = 'BlockTypeId'
		AND [EntityTypeQualifierValue] <> CAST(@BlockTypeId AS NVARCHAR(200));

    IF @AttributeId IS NOT NULL
    BEGIN
        DELETE FROM [Attribute] WHERE [Id] = @AttributeId;
    END

    FETCH NEXT FROM AttributeCursor INTO @AttributeGuid;
END

CLOSE AttributeCursor;
DEALLOCATE AttributeCursor;
" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Browse Mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Browse Mode", "BrowseMode", "Browse Mode", @"Select 'image' to show only image files. Select 'doc' to show all files.", 5, @"doc", "C872B6A7-36F6-4771-807A-7B4A7E8BAD2C" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Enable Asset Storage Providers
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Asset Storage Providers", "EnableAssetProviders", "Enable Asset Storage Providers", @"Set this to true to enable showing folders and files from your configured asset storage providers.", 0, @"False", "7750F7BB-DC53-41C6-987B-5FD2B02674C2" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Enable File Manager
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable File Manager", "EnableFileManager", "Enable File Manager", @"Set this to true to enable showing folders and files your server's local file system.", 1, @"True", "FCBB90A6-965F-4237-9B0F-4384E3FFC991" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Enable Zip Upload
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Zip Upload", "ZipUploaderEnabled", "Enable Zip Upload", @"Set this to true to enable the Zip File uploader.", 7, @"False", "BD031ADA-5D23-4237-A332-468FAC7282E9" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: File Editor Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "File Editor Page", "FileEditorPage", "File Editor Page", @"Page used to edit the contents of a file.", 6, @"", "80D4544B-563A-4109-9F61-F4E019580B3A" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Height Mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Height Mode", "HeightMode", "Height Mode", @"Static lets you set a CSS height below to determine the height of the block. Flexible will grow with the content. Full Worksurface is designed to fill up a full worksurface page layout.", 2, @"full", "67ECB409-F5C5-4487-A60B-FD572B99D95B" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Height
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Height", "Height", "Height", @"If you've selected Yes for ""Use Static Height"", this will be the CSS length value that dictates how tall the block will be.", 3, @"400px", "48C3DFAD-2168-4F6C-8DEC-167E49C379B7" );

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Root Folder
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535500A7-967F-4DA3-8FCA-CB844203CB3D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Root Folder", "RootFolder", "Root Folder", @"The root file manager folder to browse", 4, @"~/Content", "14684245-5768-442D-9BFB-C80E1383775A" );
        }

        private void SwapBlockTypesv19_0()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop Block Types 19.0 (19.0.1)",
                blockTypeReplacements: new Dictionary<string, string> {
                // blocks chopped in v19.0.1
{ "BA327D25-BD8A-4B67-B04C-17B499DDA4B6", "535500a7-967f-4da3-8fca-cb844203cb3d" }, // File Manager -> File Asset Manager
                },
                migrationStrategy: "Swap",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_190_SWAP_OBSIDIAN_BLOCKS,
                blockAttributeKeysToIgnore: null );
        }

        private void SwapBlocksForV19Up()
        {
            RegisterBlockAttributesForSwap();
            SwapBlockTypesv19_0();
        }

        #endregion

        #region 264_AddSystemSenderPerson Plugin Migration

        /// <summary>
        /// JPH: Add 'System Sender' person - up.
        /// </summary>
        private void JPH_AddSystemSenderPerson_Up()
        {
            Sql( $@"
IF NOT EXISTS (
    SELECT 1
    FROM [Person]
    WHERE [Guid] = '{SystemGuid.Person.SYSTEM_SENDER}'
)
BEGIN
    DECLARE @PersonRecordTypePersonValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '{SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON}')
        , @PersonRecordStatusActiveValueId INT = (SELECT TOP 1 [Id] from [DefinedValue] WHERE [Guid] = '{SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE}')
        , @PersonConnectionStatusParticipantValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '{SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PARTICIPANT}')
        , @FamilyGroupTypeId INT = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '{SystemGuid.GroupType.GROUPTYPE_FAMILY}')
        , @AdultGroupTypeRoleId INT = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = '{SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT}')
        , @SystemPersonGuid UNIQUEIDENTIFIER = '{SystemGuid.Person.SYSTEM_SENDER}'
        , @SystemPersonId INT
        , @SystemFamilyGroupId INT
        , @Now DATETIME = (SELECT GETDATE());

    -- Create person.
    INSERT INTO [Person]
    (
        [IsSystem]
        , [RecordTypeValueId]
        , [RecordStatusValueId]
        , [ConnectionStatusValueId]
        , [IsDeceased]
        , [FirstName]
        , [NickName]
        , [LastName]
        , [Gender]
        , [IsEmailActive]
        , [Guid]
        , [CreatedDateTime]
        , [EmailPreference]
        , [CommunicationPreference]
        , [AgeClassification]
        , [IsLockedAsChild]
        , [GivingLeaderId]
        , [AccountProtectionProfile]
        , [AgeBracket]
    )
    VALUES
    (
        1                                               --IsSystem
        , @PersonRecordTypePersonValueId                --RecordTypeValueId
        , @PersonRecordStatusActiveValueId              --RecordStatusValueId
        , @PersonConnectionStatusParticipantValueId     --ConnectionStatusValueId
        , 0                                             --IsDeceased
        , 'System'                                      --FirstName
        , 'System'                                      --NickName
        , 'Sender'                                      --LastName
        , 0                                             --Gender (Unknown)
        , 0                                             --IsEmailActive
        , @SystemPersonGuid                             --Guid
        , @Now                                          --CreatedDateTime
        , 0                                             --EmailPreference (EmailAllowed)
        , 0                                             --CommunicationPreference (None)
        , 1                                             --AgeClassification (Adult)
        , 0                                             --IsLockedAsChild
        , 0                                             --GivingLeaderId
        , 0                                             --AccountProtectionProfile (Low)
        , 0                                             --AgeBracket (Unknown)
    );

    SET @SystemPersonId = SCOPE_IDENTITY();

    -- Create primary alias.
    INSERT INTO [PersonAlias]
    (
        [PersonId]
        , [AliasPersonId]
        , [AliasPersonGuid]
        , [Guid]
    )
    VALUES
    (
        @SystemPersonId
        , @SystemPersonId
        , @SystemPersonGuid
        , NEWID()
    );

    -- Create family.
    INSERT INTO [Group]
    (
        [IsSystem]
        , [GroupTypeId]
        , [Name]
        , [IsSecurityRole]
        , [IsActive]
        , [Order]
        , [Guid]
        , [CreatedDateTime]
        , [IsPublic]
        , [IsArchived]
        , [SchedulingMustMeetRequirements]
        , [AttendanceRecordRequiredForCheckIn]
        , [DisableScheduleToolboxAccess]
        , [DisableScheduling]
        , [ElevatedSecurityLevel]
        , [IsSpecialNeeds]
    )
    VALUES
    (
        1                                               --IsSystem
        , @FamilyGroupTypeId                            --GroupTypeId
        , 'Sender Family'                               --Name
        , 0                                             --IsSecurityRole
        , 1                                             --IsActive
        , 0                                             --Order
        , NEWID()                                       --Guid
        , @Now                                          --CreatedDateTime
        , 1                                             --IsPublic
        , 0                                             --IsArchived
        , 0                                             --SchedulingMustMeetRequirements
        , 0                                             --AttendanceRecordRequiredForCheckIn
        , 0                                             --DisableScheduleToolboxAccess
        , 0                                             --DisableScheduling
        , 0                                             --ElevatedSecurityLevel (None)
        , 0                                             --IsSpecialNeeds
    );

    SET @SystemFamilyGroupId = SCOPE_IDENTITY();

    -- Create family member.
    INSERT INTO [GroupMember] (
        [IsSystem]
        , [GroupId]
        , [PersonId]
        , [GroupRoleId]
        , [GroupMemberStatus]
        , [Guid]
        , [CreatedDateTime]
        , [DateTimeAdded]
        , [IsNotified]
        , [IsArchived]
        , [CommunicationPreference]
        , [GroupTypeId]
        , [IsChatMuted]
        , [IsChatBanned]
       )
    VALUES (
        1                                               --IsSystem
        , @SystemFamilyGroupId                          --GroupId
        , @SystemPersonId                               --PersonId
        , @AdultGroupTypeRoleId                         --GroupRoleId
        , 1                                             --GroupMemberStatus (Active)
        , NEWID()                                       --Guid
        , @Now                                          --CreatedDateTime
        , @Now                                          --DateTimeAdded
        , 0                                             --IsNotified
        , 0                                             --IsArchived
        , 0                                             --CommunicationPreference (None)
        , @FamilyGroupTypeId                            --GroupTypeId
        , 0                                             --IsChatMuted
        , 0                                             --IsChatBanned
    );
END" );
        }

        /// <summary>
        /// JPH: Add 'System Sender' person - down.
        /// </summary>
        private void JPH_AddSystemSenderPerson_Down()
        {
            Sql( $@"
UPDATE [Person]
SET [PrimaryFamilyId] = NULL
WHERE [Guid] = '{SystemGuid.Person.SYSTEM_SENDER}';

DELETE
FROM [Group]
WHERE [Id] IN (
    SELECT [GroupId]
    FROM [GroupMember]
    WHERE [PersonId] IN (
        SELECT [Id]
        FROM [Person]
        WHERE [Guid] = '{SystemGuid.Person.SYSTEM_SENDER}'
    )
);

DELETE
FROM [PersonAlias]
WHERE [PersonId] IN (
    SELECT [Id]
    FROM [Person]
    WHERE [Guid] = '{SystemGuid.Person.SYSTEM_SENDER}'
);

DELETE
FROM [Person]
WHERE [Guid] = '{SystemGuid.Person.SYSTEM_SENDER}';" );
        }

        #endregion

        #region 266_MigrationRollupsForV19_0_0 Plugin Migration

        #region NA: Migration to Add EDIT/ADMINISTRATE Auth to the "RSR - Event Registration Administration" group on all top-level RegistrationTemplate categories

        private void AddEventRegistrationAdminAuthToTopLevelCategories()
        {
            Sql( @"
-- Add Edit and Administrate auth to the RSR Event Registration Administration group
-- on all top-level RegistrationTemplate categories, without duplicates.

DECLARE @Now DATETIME = GETDATE();
DECLARE @RSREventRegistrationAdministrationRoleId INT =
(
    SELECT [Id]
    FROM [Group]
    WHERE [Guid] = '2A92086B-DFF0-4B9C-46CB-4DAD805615AF'
);

DECLARE @RegistrationTemplateEntityTypeId INT =
(
    SELECT [Id]
    FROM [EntityType]
    WHERE [Name] = 'Rock.Model.RegistrationTemplate'
);

DECLARE @CategoryEntityTypeId INT =
(
    SELECT [Id]
    FROM [EntityType]
    WHERE [Name] = 'Rock.Model.Category'
);

;WITH [TopLevelRegistrationTemplateCategories] AS
(
    SELECT
        c.[Id] AS [CategoryId]
    FROM [Category] AS c
    WHERE
        c.[ParentCategoryId] IS NULL
        AND c.[EntityTypeId] = @RegistrationTemplateEntityTypeId
),
[DesiredAuth] AS
(
    SELECT
          tl.[CategoryId]
        , a.[Action]
        , a.[Order]
    FROM [TopLevelRegistrationTemplateCategories] AS tl
    CROSS APPLY (VALUES
        ('Edit', 0),
        ('Administrate', 0)
    ) AS a([Action], [Order])
)
INSERT INTO [Auth]
(
      [EntityTypeId]
    , [EntityId]
    , [Order]
    , [Action]
    , [AllowOrDeny]
    , [SpecialRole]
    , [GroupId]
    , [Guid]
    , [CreatedDateTime]
    , [ModifiedDateTime]
)
SELECT
      @CategoryEntityTypeId                       -- [EntityTypeId]
    , d.[CategoryId]                              -- [EntityId]
    , d.[Order]                                   -- [Order]
    , d.[Action]                                  -- [Action]
    , 'A'                                         -- [AllowOrDeny]
    , 0                                           -- [SpecialRole]
    , @RSREventRegistrationAdministrationRoleId   -- [GroupId]
    , NEWID()                                     -- [Guid]
    , @Now                                        -- [CreatedDateTime]
    , @Now                                        -- [ModifiedDateTime]
FROM [DesiredAuth] AS d
WHERE NOT EXISTS
(
    SELECT 1
    FROM [Auth] AS a
    WHERE
        a.[EntityTypeId] = @CategoryEntityTypeId
        AND a.[EntityId] = d.[CategoryId]
        AND a.[Action] = d.[Action]
        AND a.[SpecialRole] = 0
        AND a.[GroupId] = @RSREventRegistrationAdministrationRoleId
);
" );
        }

        #endregion

        #endregion

        #region 271_MigrationRollupsForV19_0_1 Plugin Migration

        #region NA: Rename original Default to "Legacy (Original)"

        private void UpdateMobileCommunicationView()
        {
            // Update original "Default" Template for the Mobile Communication View to be called "Legacy (Original)"
            // because there is already a newer Template called "Legacy" and the newest "Default".
            Sql( "UPDATE [DefinedValue] SET [Value] = 'Legacy (Original)' WHERE [Guid] = '39B8B16D-D213-46FD-9B8F-710453806193'" );
        }

        #endregion

        #region NA: Migration to Add EDIT/ADMINISTRATE Auth to the "RSR - Event Registration Administration" group on all top-level RegistrationTemplate categories

        private void AddEventRegistrationAdminAuthToTopLevelCategoriesSecondRound()
        {
            Sql( @"-- Add Edit and Administrate auth to the RSR Event Registration Administration group
-- on all top-level RegistrationTemplate categories, without duplicates.

DECLARE @Now DATETIME = GETDATE();
DECLARE @RSREventRegistrationAdministrationRoleId INT =
(
    SELECT [Id]
    FROM [Group]
    WHERE [Guid] = '2A92086B-DFF0-4B9C-46CB-4DAD805615AF'
);

DECLARE @RegistrationTemplateEntityTypeId INT =
(
    SELECT [Id]
    FROM [EntityType]
    WHERE [Name] = 'Rock.Model.RegistrationTemplate'
);

DECLARE @CategoryEntityTypeId INT =
(
    SELECT [Id]
    FROM [EntityType]
    WHERE [Name] = 'Rock.Model.Category'
);

;WITH [TopLevelRegistrationTemplateCategories] AS
(
    SELECT
        c.[Id] AS [CategoryId]
    FROM [Category] AS c
    WHERE
        c.[ParentCategoryId] IS NULL
        AND c.[EntityTypeId] = @RegistrationTemplateEntityTypeId
),
[DesiredAuth] AS
(
    SELECT
          tl.[CategoryId]
        , a.[Action]
        , a.[Order]
    FROM [TopLevelRegistrationTemplateCategories] AS tl
    CROSS APPLY (VALUES
        ('Edit', 0),
        ('Administrate', 0)
    ) AS a([Action], [Order])
)
INSERT INTO [Auth]
(
      [EntityTypeId]
    , [EntityId]
    , [Order]
    , [Action]
    , [AllowOrDeny]
    , [SpecialRole]
    , [GroupId]
    , [Guid]
    , [CreatedDateTime]
    , [ModifiedDateTime]
)
SELECT
      @CategoryEntityTypeId                       -- [EntityTypeId]
    , d.[CategoryId]                              -- [EntityId]
    , d.[Order]                                   -- [Order]
    , d.[Action]                                  -- [Action]
    , 'A'                                         -- [AllowOrDeny]
    , 0                                           -- [SpecialRole]
    , @RSREventRegistrationAdministrationRoleId   -- [GroupId]
    , NEWID()                                     -- [Guid]
    , @Now                                        -- [CreatedDateTime]
    , @Now                                        -- [ModifiedDateTime]
FROM [DesiredAuth] AS d
WHERE NOT EXISTS
(
    SELECT 1
    FROM [Auth] AS a
    WHERE
        a.[EntityTypeId] = @CategoryEntityTypeId
        AND a.[EntityId] = d.[CategoryId]
        AND a.[Action] = d.[Action]
        AND a.[SpecialRole] = 0
        AND a.[GroupId] = @RSREventRegistrationAdministrationRoleId
);" );
        }

        #endregion

        #endregion

        #region 272_UpdateFileEditorPageLayout Plugin Migration

        private void UpdateFileEditorPage()
        {
            Sql( @"
                UPDATE dbo.[Page]
                SET [LayoutId] = (
                    SELECT [Id]
                    FROM dbo.[Layout]
                    WHERE [Guid] = 'C2467799-BB45-4251-8EE6-F0BF27201535'
                )
                WHERE [Guid] = '053C3F1D-8BF2-48B2-A8E6-55184F8A87F4';
            " );
        }

        #endregion

        #region 273_UpdateCommunicationTemplatesAndShortcodeForV18_2 Plugin Migration

        private void UpdateCommunicationTemplatesAndShortcodeForV18_2()
        {
            Sql( @"
                -- Fix typo in TextBox LavaShortcode
                UPDATE [LavaShortcode]
                SET [Documentation] = REPLACE([Documentation], 'preeaddon', 'preaddon')
                WHERE [Guid] = 'BE829889-A775-4170-9A88-591BB82C86DD'

                -- Fix isrequire issue in the Checkbox list LavaShortcode
                UPDATE [LavaShortcode]
                SET [Markup] =
                    REPLACE(
                        REPLACE(
                            [Markup],
                            '{% assign sc-values =  value | Split:'','',true %}',
                            '{% assign isrequired = isrequired | AsBoolean %}' + CHAR(13) + CHAR(10) +
                            '{% assign sc-values =  value | Split:'','',true %}'
                        ),
                        '{% if isrequired %}',
                        '{% if isrequired == true %}'
                    )
                WHERE [Guid] = 'D052824F-E514-47D9-953E-2C9B55FF72D0'
                  AND [Markup] NOT LIKE '% assign isrequired = isrequired | AsBoolean %'


                -- Rename the 'Blank' template to Legacy
                UPDATE [CommunicationTemplate]
                SET [Name] = 'Blank (Legacy)'
                WHERE [Guid] = 'A3C7F623-7F6F-4C48-B66F-CBEE2DF30B6A'
                AND [Name] = 'Blank'

                -- Rename the preview template to just 'Blank'
                UPDATE [CommunicationTemplate]
                SET [Name] = 'Blank'
                WHERE [Guid] = '6280214C-404E-4F4E-BC33-7A5D4CDF8DBC'
                AND [Name] = 'Blank (Preview)'
            " );
        }

        private void UpdateCommunicationTemplatesAndShortcodeForV18_2_Down()
        {
            Sql( @"
                -- Don't undo typo in TextBox LavaShortcode
                -- no op

                -- Undo the fix for the isrequire issue in the Checkbox list LavaShortcode
                UPDATE [LavaShortcode]
                SET [Markup] =
                    REPLACE(
                        REPLACE(
                            [Markup],
                            '{% if isrequired == true %}',
                            '{% if isrequired %}'
                        ),
                        '{% assign isrequired = isrequired | AsBoolean %}' + CHAR(13) + CHAR(10),
                        ''
                    )
                WHERE [Guid] = 'D052824F-E514-47D9-953E-2C9B55FF72D0'
                  AND  [Markup] LIKE '% assign isrequired = isrequired | AsBoolean %'

                -- Rename the 'Blank' template back to Legacy
                UPDATE [CommunicationTemplate]
                SET [Name] = 'Blank'
                WHERE [Guid] = 'A3C7F623-7F6F-4C48-B66F-CBEE2DF30B6A'
                AND [Name] = 'Blank (Legacy)'

                -- Rename the preview template back to preview
                UPDATE [CommunicationTemplate]
                SET [Name] = 'Blank (Preview)'
                WHERE [Guid] = '6280214C-404E-4F4E-BC33-7A5D4CDF8DBC'
                AND [Name] = 'Blank'
            " );
        }

        #endregion

        #region 274_MigrationRollupsForV18_2_0 Plugin Migration

        #region NA: Data Migration to Remove Duplicate Categories for DefinedType

        /// <summary>
        /// Remap DefinedType.CategoryId from duplicate Category rows to the keeper per name,
        /// then delete duplicates for 'Global', 'Group', 'Person' under the DefinedType
        /// EntityType.
        /// </summary>
        private void NA_RemoveDuplicateDefinedTypeCategories()
        {
            Sql( @"
-- Clean up the duplicate Categories ('Global', 'Group', and 'Person') 
-- that are for the DefinedType entity:

DECLARE @Global_KeepCategoryGuid UNIQUEIDENTIFIER = 'CB71E9CD-F11D-4EA0-A154-EDF3CECF6F77';
DECLARE @Group_KeepCategoryGuid UNIQUEIDENTIFIER = '8437CC1A-A799-4A49-B336-19CE03A06EF0';
DECLARE @Person_KeepCategoryGuid UNIQUEIDENTIFIER = '994992BC-BE9E-4555-9E76-F1D6219AAB13';

DECLARE @DefinedTypeEntityTypeGuid UNIQUEIDENTIFIER = '6028D502-79F4-4A74-9323-525E90F900C7';
DECLARE @DefinedTypeEntityTypeId INT;

SELECT @DefinedTypeEntityTypeId = et.[Id]
FROM [EntityType] et
WHERE et.[Guid] = @DefinedTypeEntityTypeGuid;

-- Global: remap then delete
UPDATE dt
SET dt.[CategoryId] = keep.[Id]
FROM [DefinedType] dt
JOIN [Category] dup
  ON dup.[Id] = dt.[CategoryId]
 AND dup.[EntityTypeId] = @DefinedTypeEntityTypeId
 AND dup.[Name] = 'Global'
JOIN [Category] keep
  ON keep.[EntityTypeId] = @DefinedTypeEntityTypeId
 AND keep.[Name] = 'Global'
 AND keep.[Guid] = @Global_KeepCategoryGuid
-- NULL-safe equality across the qualifiers and parent
 AND ISNULL(dup.[ParentCategoryId], -2147483648) = ISNULL(keep.[ParentCategoryId], -2147483648)
 AND ISNULL(dup.[EntityTypeQualifierColumn], '') = ISNULL(keep.[EntityTypeQualifierColumn], '')
 AND ISNULL(dup.[EntityTypeQualifierValue],  '') = ISNULL(keep.[EntityTypeQualifierValue],  '')
WHERE dup.[Guid] <> keep.[Guid];

DELETE [Category]
WHERE [EntityTypeId] = @DefinedTypeEntityTypeId
  AND [Name] = 'Global'
  AND [Guid] <> @Global_KeepCategoryGuid
  AND EXISTS (
        SELECT 1
        FROM [Category] keep
        WHERE keep.[EntityTypeId] = @DefinedTypeEntityTypeId
          AND keep.[Name] = 'Global'
          AND keep.[Guid] = @Global_KeepCategoryGuid
          AND ISNULL(keep.[ParentCategoryId], -2147483648) = ISNULL([Category].[ParentCategoryId], -2147483648)
          AND ISNULL(keep.[EntityTypeQualifierColumn], '') = ISNULL([Category].[EntityTypeQualifierColumn], '')
          AND ISNULL(keep.[EntityTypeQualifierValue],  '') = ISNULL([Category].[EntityTypeQualifierValue],  '')
  );

-- Group: remap then delete
UPDATE dt
SET dt.[CategoryId] = keep.[Id]
FROM [DefinedType] dt
JOIN [Category] dup
  ON dup.[Id] = dt.[CategoryId]
 AND dup.[EntityTypeId] = @DefinedTypeEntityTypeId
 AND dup.[Name] = 'Group'
JOIN [Category] keep
  ON keep.[EntityTypeId] = @DefinedTypeEntityTypeId
 AND keep.[Name] = 'Group'
 AND keep.[Guid] = @Group_KeepCategoryGuid
 AND ISNULL(dup.[ParentCategoryId], -2147483648) = ISNULL(keep.[ParentCategoryId], -2147483648)
 AND ISNULL(dup.[EntityTypeQualifierColumn], '') = ISNULL(keep.[EntityTypeQualifierColumn], '')
 AND ISNULL(dup.[EntityTypeQualifierValue],  '') = ISNULL(keep.[EntityTypeQualifierValue],  '')
WHERE dup.[Guid] <> keep.[Guid];

DELETE [Category]
WHERE [EntityTypeId] = @DefinedTypeEntityTypeId
  AND [Name] = 'Group'
  AND [Guid] <> @Group_KeepCategoryGuid
  AND EXISTS (
        SELECT 1
        FROM [Category] keep
        WHERE keep.[EntityTypeId] = @DefinedTypeEntityTypeId
          AND keep.[Name] = 'Group'
          AND keep.[Guid] = @Group_KeepCategoryGuid
          AND ISNULL(keep.[ParentCategoryId], -2147483648) = ISNULL([Category].[ParentCategoryId], -2147483648)
          AND ISNULL(keep.[EntityTypeQualifierColumn], '') = ISNULL([Category].[EntityTypeQualifierColumn], '')
          AND ISNULL(keep.[EntityTypeQualifierValue],  '') = ISNULL([Category].[EntityTypeQualifierValue],  '')
  );

-- Person: remap then delete
UPDATE dt
SET dt.[CategoryId] = keep.[Id]
FROM [DefinedType] dt
JOIN [Category] dup
  ON dup.[Id] = dt.[CategoryId]
 AND dup.[EntityTypeId] = @DefinedTypeEntityTypeId
 AND dup.[Name] = 'Person'
JOIN [Category] keep
  ON keep.[EntityTypeId] = @DefinedTypeEntityTypeId
 AND keep.[Name] = 'Person'
 AND keep.[Guid] = @Person_KeepCategoryGuid
 AND ISNULL(dup.[ParentCategoryId], -2147483648) = ISNULL(keep.[ParentCategoryId], -2147483648)
 AND ISNULL(dup.[EntityTypeQualifierColumn], '') = ISNULL(keep.[EntityTypeQualifierColumn], '')
 AND ISNULL(dup.[EntityTypeQualifierValue],  '') = ISNULL(keep.[EntityTypeQualifierValue],  '')
WHERE dup.[Guid] <> keep.[Guid];

DELETE [Category]
WHERE [EntityTypeId] = @DefinedTypeEntityTypeId
  AND [Name] = 'Person'
  AND [Guid] <> @Person_KeepCategoryGuid
  AND EXISTS (
        SELECT 1
        FROM [Category] keep
        WHERE keep.[EntityTypeId] = @DefinedTypeEntityTypeId
          AND keep.[Name] = 'Person'
          AND keep.[Guid] = @Person_KeepCategoryGuid
          AND ISNULL(keep.[ParentCategoryId], -2147483648) = ISNULL([Category].[ParentCategoryId], -2147483648)
          AND ISNULL(keep.[EntityTypeQualifierColumn], '') = ISNULL([Category].[EntityTypeQualifierColumn], '')
          AND ISNULL(keep.[EntityTypeQualifierValue],  '') = ISNULL([Category].[EntityTypeQualifierValue],  '')
  );
          " );
        }

        #endregion

        #region NA: Migration to Fix Student and Facilitator LMS Roles

        /// <summary>
        /// Fix issue #6601 by setting CanView for both Faciliator and Student roles.
        /// </summary>
        private void NA_SetFacilitatorAndStudentRolesToCanViewUp()
        {
            Sql( @"
-- Fix issue #6601 by setting CanView for both Faciliator and Student roles 
DECLARE @Facilitator_RoleGuid UNIQUEIDENTIFIER = '80F802CE-2F59-4AB1-ABD8-CFD7A009A00A';
DECLARE @Student_RoleGuid UNIQUEIDENTIFIER = 'FA3ACAC2-0377-484C-B888-974CA3BF2FF2';

UPDATE [GroupTypeRole]
SET [CanView] = 1
WHERE [Guid] IN ( @Facilitator_RoleGuid, @Student_RoleGuid );
          " );
        }

        /// <summary>
        /// Remove the fix for issue #6601
        /// </summary>
        private void NA_SetFacilitatorAndStudentRolesToCanViewDown()
        {
            Sql( @"
-- Remove fix for issue #6601 by setting CanView for both Faciliator and Student roles 
DECLARE @Facilitator_RoleGuid UNIQUEIDENTIFIER = '80F802CE-2F59-4AB1-ABD8-CFD7A009A00A';
DECLARE @Student_RoleGuid UNIQUEIDENTIFIER = 'FA3ACAC2-0377-484C-B888-974CA3BF2FF2';

UPDATE [GroupTypeRole]
SET [CanView] = 0
WHERE [Guid] IN ( @Facilitator_RoleGuid, @Student_RoleGuid );
          " );
        }

        #endregion

        #endregion

        #region NA: v19 Data Migration to Update core_CampusColor description/tooltip

        /// <summary>
        /// Change the existing core Campus Color attribute's tooltip. 
        /// </summary>
        public void NA_ConvertCampusColorAttributeUp()
        {
            Sql( @"
UPDATE [Attribute] 
SET [Description] = 'Color used to represent this campus when displayed alongside others.' 
WHERE [Key] = 'core_CampusColor' AND [Guid] = 'B63C8C1A-58DC-4DDF-AA5D-C71F1E2D74B6'
" );
        }

        /// <summary>
        /// Revert the existing core Campus Color attribute's tooltip. 
        /// </summary>
        public void NA_ConvertCampusColorAttributeDown()
        {
            Sql( @"
UPDATE [Attribute] 
SET [Description] = 'This color will be shown in certain places where multiple campuses are being visualized.' 
WHERE [Key] = 'core_CampusColor' AND [Guid] = 'B63C8C1A-58DC-4DDF-AA5D-C71F1E2D74B6'
" );
        }

        #endregion
    }
}
