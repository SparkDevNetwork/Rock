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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Sunsets the original Protect My Ministry (v1) background check component in Rock v20.
    /// Removes the PMM component's EntityType, its admin Page/PageRoute/Block/BlockType, its
    /// DVR Jurisdiction Codes DefinedType, its component-configuration Attributes and
    /// AttributeValues, and marks the original "Background Check" WorkflowType inactive
    /// (renamed to "Background Check (PMM Legacy)"). If the Rock instance still has PMM set
    /// as the default background check provider when this migration runs, an entry is written
    /// to the ExceptionLog so operators can see a breadcrumb of what was removed.
    ///
    /// The WorkflowType itself is intentionally NOT deleted because historical Workflow rows
    /// reference it; the deactivation-and-rename approach preserves audit history while making
    /// it obvious the workflow is legacy. Downstream code paths (BackgroundCheckFieldType,
    /// BackgroundCheckDocument) still recognize the PMM EntityType GUID so that any legacy
    /// background-check documents stored in the single-Guid format continue to render.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 307, "20.0" )]
    public class SunsetProtectMyMinistry : Migration
    {
        /// <summary>
        /// The full type name of the (now-removed) Protect My Ministry v1 component. This is
        /// hard-coded here because the class no longer exists in the assembly at the time this
        /// migration runs on an instance.
        /// </summary>
        private const string PmmComponentTypeName = "Rock.Security.BackgroundCheck.ProtectMyMinistry";

        /// <summary>
        /// Well-known Guids for the artifacts being removed.
        /// </summary>
        private const string PmmEntityTypeGuid = "C16856F4-3C6B-4AFB-A0B8-88A303508206";
        private const string PmmBlockGuid = "63AA839B-B6A1-4A57-A0DC-2F5B6DDA71BE";
        private const string PmmBlockTypeGuid = "AF36FA7E-BD2A-42A3-AF30-2FEBC1C46663";
        private const string PmmPageGuid = "E7F4B733-60FF-4FA3-AB17-0832E123F6F2";
        private const string PmmPageRouteGuid = "2BB14E39-6AEE-4379-8B92-ACB5EF3F700B";
        private const string PmmMvrJurisdictionDefinedTypeGuid = "2F8821E8-05B9-4CD5-9FA4-303662AAC85D";
        private const string PmmWorkflowTypeGuid = "16D12EF7-C546-4039-9036-B73D118EDC90";

        /// <summary>
        /// The system setting key that holds the currently-configured default background check provider.
        /// </summary>
        private const string DefaultBackgroundCheckProviderKey = "core_DefaultBackgroundCheckProvider";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            LogExceptionIfPmmIsStillTheDefaultProvider();
            ClearDefaultBackgroundCheckProviderIfPmm();
            DeactivateAndRenamePmmWorkflowType();
            DeletePmmComponentAttributesAndValues();
            DeletePmmAdminPageAndBlocks();
            DeletePmmMvrJurisdictionDefinedType();
            DeletePmmEntityType();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not supported in plug-in migrations.
        }

        /// <summary>
        /// Writes a row to <c>[ExceptionLog]</c> when the PMM v1 component is still configured
        /// as the default background check provider. This gives operators a breadcrumb that
        /// their PMM configuration became inoperable when this migration ran.
        /// </summary>
        private void LogExceptionIfPmmIsStillTheDefaultProvider()
        {
            Sql( $@"
IF EXISTS (
    SELECT 1
    FROM [Attribute]
    WHERE [EntityTypeId] IS NULL
      AND [EntityTypeQualifierColumn] = 'SystemSetting'
      AND [Key] = '{DefaultBackgroundCheckProviderKey}'
      AND [DefaultValue] = '{PmmComponentTypeName}'
)
BEGIN
    INSERT INTO [ExceptionLog]
        ( [HasInnerException], [ExceptionType], [Description], [Source],
          [Guid], [CreatedDateTime], [ModifiedDateTime] )
    VALUES
        ( 0,
          'System.Exception',
          'The legacy Protect My Ministry (v1) background check provider was removed by the Rock v20 sunset migration while it was still configured as the default background check provider. Configure a supported provider (Checkr, other, etc.) under Admin Tools > System Settings > Background Check.',
          'Rock.Plugin.HotFixes.SunsetProtectMyMinistry',
          NEWID(),
          SYSDATETIME(),
          SYSDATETIME() );
END
" );
        }

        /// <summary>
        /// Blanks the <c>core_DefaultBackgroundCheckProvider</c> SystemSetting if (and only if)
        /// it currently points at the removed PMM v1 type. Other provider selections are left
        /// alone.
        /// </summary>
        private void ClearDefaultBackgroundCheckProviderIfPmm()
        {
            Sql( $@"
UPDATE [Attribute]
SET [DefaultValue] = ''
WHERE [EntityTypeId] IS NULL
  AND [EntityTypeQualifierColumn] = 'SystemSetting'
  AND [Key] = '{DefaultBackgroundCheckProviderKey}'
  AND [DefaultValue] = '{PmmComponentTypeName}';
" );
        }

        /// <summary>
        /// The original "Background Check" WorkflowType was authored for PMM v1. Historical
        /// Workflow instances reference it, so it cannot be safely deleted. This renames it to
        /// "Background Check (PMM Legacy)" (matching Checkr's existing rename behavior) and
        /// deactivates it so it no longer surfaces as an active workflow type.
        /// </summary>
        private void DeactivateAndRenamePmmWorkflowType()
        {
            Sql( $@"
UPDATE [WorkflowType]
SET [Name] = 'Background Check (PMM Legacy)',
    [IsActive] = 0
WHERE [Guid] = '{PmmWorkflowTypeGuid}';
" );
        }

        /// <summary>
        /// Deletes the PMM component's configuration attributes (UserName, Password, Active,
        /// Order, TestMode, RequestURL, ReturnURL) and their values, plus the container-side
        /// componentized attributes qualified by the PMM EntityType Id. Uses joins on the
        /// EntityType Guid so it works regardless of Id churn between installs.
        /// </summary>
        private void DeletePmmComponentAttributesAndValues()
        {
            Sql( $@"
DECLARE @PmmEntityTypeId INT =
    ( SELECT [Id] FROM [EntityType] WHERE [Guid] = '{PmmEntityTypeGuid}' );

IF @PmmEntityTypeId IS NOT NULL
BEGIN
    -- Values for attributes owned by the PMM component EntityType.
    DELETE av
    FROM [AttributeValue] av
    INNER JOIN [Attribute] a ON a.[Id] = av.[AttributeId]
    WHERE a.[EntityTypeId] = @PmmEntityTypeId;

    DELETE FROM [Attribute]
    WHERE [EntityTypeId] = @PmmEntityTypeId;

    -- Container-side componentized attributes (""Active"", ""Order"") that live on the
    -- BackgroundCheckContainer service and are qualified by the PMM EntityType Id.
    DECLARE @PmmEntityTypeIdText NVARCHAR(50) = CAST( @PmmEntityTypeId AS NVARCHAR(50) );

    DELETE av
    FROM [AttributeValue] av
    INNER JOIN [Attribute] a ON a.[Id] = av.[AttributeId]
    WHERE a.[EntityTypeQualifierColumn] = 'EntityTypeId'
      AND a.[EntityTypeQualifierValue] = @PmmEntityTypeIdText;

    DELETE FROM [Attribute]
    WHERE [EntityTypeQualifierColumn] = 'EntityTypeId'
      AND [EntityTypeQualifierValue] = @PmmEntityTypeIdText;
END
" );
        }

        /// <summary>
        /// Deletes the placed PMM Settings block, the PMM Settings block type, the PMM admin
        /// page route, and the PMM admin page itself. Uses <see cref="RockMigrationHelper"/>
        /// helpers so any related Auth rows are cleaned up too.
        /// </summary>
        private void DeletePmmAdminPageAndBlocks()
        {
            // Block placed on the PMM admin page.
            RockMigrationHelper.DeleteBlock( PmmBlockGuid );

            // BlockType for ~/Blocks/Security/BackgroundCheck/ProtectMyMinistrySettings.ascx.
            RockMigrationHelper.DeleteBlockType( PmmBlockTypeGuid );

            // Route: /admin/system/protect-my-ministry.
            RockMigrationHelper.DeletePageRoute( PmmPageRouteGuid );

            // Page: Protect My Ministry (under Admin Tools > System Settings).
            RockMigrationHelper.DeletePage( PmmPageGuid );
        }

        /// <summary>
        /// The "Protect My Ministry DVR Jurisdiction Codes" DefinedType and its DefinedValues
        /// were only used by PMM v1's MVR search feature; no other provider references them.
        /// </summary>
        private void DeletePmmMvrJurisdictionDefinedType()
        {
            RockMigrationHelper.DeleteDefinedType( PmmMvrJurisdictionDefinedTypeGuid );
        }

        /// <summary>
        /// Removes the PMM component EntityType row itself. All attributes/values pointing at
        /// it were removed by <see cref="DeletePmmComponentAttributesAndValues"/> above.
        /// </summary>
        private void DeletePmmEntityType()
        {
            RockMigrationHelper.DeleteEntityType( PmmEntityTypeGuid );
        }
    }
}
