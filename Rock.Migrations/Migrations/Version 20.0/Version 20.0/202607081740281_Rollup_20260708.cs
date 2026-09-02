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
    using System.IO;
    using System.Web.Hosting;

    using Rock.Migrations.Migrations;
    using Rock.Plugin.HotFixes;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20260708 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // ----------------------------------------------------------------
            // HotFix data-migrations moved to this EF migration (v20/develop):
            // ----------------------------------------------------------------

            // v19.1; 292_AddDefaultBenevolenceBlockAuth.cs
            KH_AddDefaultBenevolenceBlockAuth_Up();

            // v20.0; 293_AddConnectionTypeSettingsFieldType.cs
            JH_AddConnectionTypeSettingsFieldType_Up();

            // v20.0; 294_AddSmartyStreetsInternational.cs
            NA_AddSmartyStreetsInternational_Up();

            // v19.3; 295_FixGoupPlacementPersonFilter.cs
            KH_FixGoupPlacementPersonFilter_Up();

            // v20.0; 296_FinalizeCoreAzureBlobStorageProviderConversion.cs
            NA_FinalizeCoreAzureBlobStorageProviderConversion_Up();

            // v20.0; 297_MigrationRollupForV20_0_4.cs
            NA_Update_PersonalizationSegmentsPageIcon_Up();

            // v19.3; 298_FixGroupPlacementWaitList.cs
            KH_FixGroupPlacementWaitList_Up();

            // v19.3; 299_FixAnalyticsETLFamilyOperatorPrecedence.cs
            NA_FixAnalyticsETLFamilyOperatorPrecedence_Up();

            // v20.0; 300_AddCategoryTreeViewToFormBuilderPage.cs
            JH_AddCategoryTreeViewToFormBuilderPage_Up();

            // v19.3; 301_UpdateBrokenPageIconsForV19_2.cs
            NA_Update_PageIcons_V19_3_Up(); // was moved to v19.3

            //v18.4; 302_MigrationRollupsForV18_4_0.cs
            ChangeConnectionStatusColorAttributeToColorPickerPart2_Up();

            // v19.3; 303_MigrationRollupsForV19_3_0.cs
            JE_TablerIconReplaceInAttributeDefaultValues_Up();

            // ----------------------------------------------------------------
            // Rollup Migrations for v20.0.5
            // ----------------------------------------------------------------
            JPH_UpdateLocationSelectionStrategyHelpText_Up();
            NA_DeleteLegacyContributionStatementLavaBlock_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // v20.0.5
            JPH_UpdateLocationSelectionStrategyHelpText_Down();
        }

        /// <summary>
        /// JPH: Updates the location selection strategy help text - up.
        /// </summary>
        private void JPH_UpdateLocationSelectionStrategyHelpText_Up()
        {
            Sql( $@"
DECLARE @GroupTypeEntityId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '{Rock.SystemGuid.EntityType.GROUP_TYPE}');
DECLARE @GroupTypeId INT = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '0572A5FE-20A4-4BF1-95CD-C71DB5281392');

UPDATE [Attribute]
SET [Description] = 'Determines how a location is selected for the group. Ask displays all available and open scheduled locations. Load Balance evenly distributes check-ins across available rooms. Fill In Order fills locations in the order they are configured until each is full. Note: Location balancing is designed for Family check-in, where service time is selected before room selection. Using it with Individual check-in may lead to unexpected results.'
WHERE [EntityTypeId] = @GroupTypeEntityId
    AND [EntityTypeQualifierColumn] = 'Id'
    AND [EntityTypeQualifierValue] = @GroupTypeId
    AND [Key] = '{Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_LOCATION_SELECTION_STRATEGY}';" );
        }

        /// <summary>
        /// JPH: Updates the location selection strategy help text - down.
        /// </summary>
        private void JPH_UpdateLocationSelectionStrategyHelpText_Down()
        {
            Sql( $@"
DECLARE @GroupTypeEntityId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '{Rock.SystemGuid.EntityType.GROUP_TYPE}');
DECLARE @GroupTypeId INT = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '0572A5FE-20A4-4BF1-95CD-C71DB5281392');

UPDATE [Attribute]
SET [Description] = 'Determines how the location for the group will be selected. Ask will offer all available scheduled locations that are open. Load balance will fill rooms in an even manner. Fill In Order will fill locations in their configured order until they are full. The location balancing feature is intended for use with Family check-in because it asks for service time selection before room selection. If you attempt to use this feature with Individual check-in, you are likely to experience unexpected results.'
WHERE [EntityTypeId] = @GroupTypeEntityId
    AND [EntityTypeQualifierColumn] = 'Id'
    AND [EntityTypeQualifierValue] = @GroupTypeId
    AND [Key] = '{Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_LOCATION_SELECTION_STRATEGY}';" );
        }

        /// <summary>
        /// As follow-up to the data migration (Rollup_20260520.cs) in ✓ Planned Block Removals and Chops v20 (b51ba27b2f) that
        /// removed any instances of the legacy block, this data migration removes the BlockType. (Which should have been done
        /// in that migration, but there was a early out RETURN that skipped the final step if there were no instances.)
        /// </summary>
        private void NA_DeleteLegacyContributionStatementLavaBlock_Up()
        {
            Sql( @"
DECLARE @BlockEntityTypeId INT = ( SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D89555CA-9AE4-4D62-8AF1-E5E463C1EF65' );
DECLARE @ContributionStatementLavaBlockTypeId INT = ( SELECT [Id] FROM [BlockType] WHERE [Guid] = 'AF986B72-ADD9-4E05-971F-1DE4EBED8667' );

-- Check if already cleaned up by the prior data migration.  If so, nothing to do now.
IF @ContributionStatementLavaBlockTypeId IS NULL
BEGIN
    RETURN;
END;

DECLARE @QualifierValue NVARCHAR(200) = CAST( @ContributionStatementLavaBlockTypeId AS NVARCHAR(200) );

-- Clean up the orphaned legacy Attribute definitions (AllowPersonQuerystring,
-- DisplayPledges, Accounts, etc.) and any stray values/qualifiers tied to them.
DELETE av
FROM [AttributeValue] av
INNER JOIN [Attribute] a ON a.[Id] = av.[AttributeId]
WHERE a.[EntityTypeId] = @BlockEntityTypeId
    AND a.[EntityTypeQualifierColumn] = 'BlockTypeId'
    AND a.[EntityTypeQualifierValue] = @QualifierValue;

DELETE aq
FROM [AttributeQualifier] aq
INNER JOIN [Attribute] a ON a.[Id] = aq.[AttributeId]
WHERE a.[EntityTypeId] = @BlockEntityTypeId
    AND a.[EntityTypeQualifierColumn] = 'BlockTypeId'
    AND a.[EntityTypeQualifierValue] = @QualifierValue;

DELETE FROM [Attribute]
WHERE [EntityTypeId] = @BlockEntityTypeId
    AND [EntityTypeQualifierColumn] = 'BlockTypeId'
    AND [EntityTypeQualifierValue] = @QualifierValue;

DELETE FROM [BlockType]
WHERE [Id] = @ContributionStatementLavaBlockTypeId;
" );
        }

        private void KH_AddDefaultBenevolenceBlockAuth_Up()
        {
            /*
                6/3/26 - KH

                Grant the Benevolence security role 'Edit' (Allow) on every existing
                Benevolence Request Detail block instance. The NOT EXISTS guard makes
                this re-runnable and leaves any identical rule an admin already added
                untouched.
            */
            Sql( @"
DECLARE @BlockEntityTypeId  INT = ( SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D89555CA-9AE4-4D62-8AF1-E5E463C1EF65' ); -- Rock.Model.Block
DECLARE @BenevolenceGroupId INT = ( SELECT [Id] FROM [Group] WHERE [Guid] = '02FA0881-3552-42B8-A519-D021139B800F' );      -- Benevolence role
DECLARE @BlockTypeId        INT = ( SELECT [Id] FROM [BlockType] WHERE [Guid] = '34275D0E-BC7E-4A9C-913E-623D086159A1' ); -- Benevolence Request Detail

IF @BlockEntityTypeId IS NOT NULL AND @BenevolenceGroupId IS NOT NULL AND @BlockTypeId IS NOT NULL
BEGIN
    DECLARE @Now DATETIME = GETDATE();

    INSERT INTO [Auth] (
        [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny],
        [SpecialRole], [GroupId], [PersonAliasId], [Guid],
        [CreatedDateTime], [ModifiedDateTime]
    )
    SELECT
        @BlockEntityTypeId,
        [b].[Id],
        0,
        'Edit',
        'A',
        0,
        @BenevolenceGroupId,
        NULL,
        NEWID(),
        @Now,
        @Now
    FROM [Block] AS [b]
    WHERE [b].[BlockTypeId] = @BlockTypeId
      AND NOT EXISTS (
          SELECT 1
          FROM [Auth] AS [a]
          WHERE [a].[EntityTypeId] = @BlockEntityTypeId
            AND [a].[EntityId]     = [b].[Id]
            AND [a].[Action]       = 'Edit'
            AND [a].[GroupId]      = @BenevolenceGroupId
            AND [a].[SpecialRole]  = 0
      );
END
" );
        }

        private void JH_AddConnectionTypeSettingsFieldType_Up()
        {
            RockMigrationHelper.UpdateFieldType(
                "Connection Type Settings",
                "Captures a Connection Type plus optional Opportunity, Status, and Type Source selections as a single composite attribute value.",
                "Rock",
                "Rock.Field.Types.ConnectionTypeSettingsFieldType",
                Rock.SystemGuid.FieldType.CONNECTION_TYPE_SETTINGS );
        }

        private void NA_AddSmartyStreetsInternational_Up()
        {
            // Register the new VerificationComponent's EntityType so the row exists
            // immediately after this migration runs. The class is also decorated with
            // [Rock.SystemGuid.EntityTypeGuid] which would register it at startup, but
            // doing it explicitly here keeps the migration intent obvious.
            // GUID matches [Rock.SystemGuid.EntityTypeGuid] on
            // Rock.Address.SmartyStreetsInternational. The existing US SmartyStreets
            // component is registered the same way (inline GUID literal, no SystemGuid
            // constant), so this follows the established pattern.
            RockMigrationHelper.UpdateEntityType(
                "Rock.Address.SmartyStreetsInternational",
                "Smarty Streets International",
                "Rock.Address.SmartyStreetsInternational, Rock, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                false,
                true,
                "2F47652E-9C13-4407-9094-A14FADE5C51F" );

            // Add the "ISO 3166 Alpha-3" attribute to the Countries DefinedType so each
            // country DefinedValue can carry its alpha-3 code alongside the existing
            // alpha-2 stored in DefinedValue.Value.
            RockMigrationHelper.AddDefinedTypeAttribute(
                Rock.SystemGuid.DefinedType.LOCATION_COUNTRIES,
                Rock.SystemGuid.FieldType.TEXT,
                "ISO 3166 Alpha-3",
                Rock.SystemKey.CountryAttributeKey.ISO3166Alpha3,
                "The ISO 3166-1 alpha-3 country code (e.g., \"CAN\" for Canada). Used to match responses from international address verification services.",
                0,
                true,
                string.Empty,
                false,
                false,
                Rock.SystemGuid.Attribute.COUNTRY_ISO3166_ALPHA3 );

            // Seed alpha-3 values for every Country DefinedValue currently in Rock by
            // joining a static (alpha2, alpha3) VALUES table against [DefinedValue] on
            // [Value] (alpha-2). The WHERE NOT EXISTS guard makes the migration safe
            // to re-run and respects any partner who has manually added an alpha-3
            // value before this migration shipped.
            //
            // The 252 entries below cover every Country DefinedValue seeded into Rock
            // by the original release-1.7.0 InternationalAddress migration. AN, CS,
            // and XK are not strictly in ISO 3166-1 (Netherlands Antilles and Serbia
            // and Montenegro are ISO 3166-3 transitional codes; Kosovo is a
            // user-assigned code), but their commonly-used alpha-3 forms are seeded
            // here so the SmartyStreets International component does not return a
            // blank Country for those edge cases.
            Sql( @"
DECLARE @DefinedTypeId INT = ( SELECT [Id] FROM [DefinedType] WHERE [Guid] = 'D7979EA1-44E9-46E2-BF37-DDAF7F741378' )
DECLARE @AttributeId  INT = ( SELECT [Id] FROM [Attribute]  WHERE [Guid] = '65776D88-0C89-4B9C-B705-683F028948E3' )

;WITH [IsoMapping] ( [Alpha2], [Alpha3] ) AS (
    SELECT [Alpha2], [Alpha3] FROM ( VALUES
        ( 'AF', 'AFG' ), ( 'AX', 'ALA' ), ( 'AL', 'ALB' ), ( 'DZ', 'DZA' ), ( 'AS', 'ASM' ),
        ( 'AD', 'AND' ), ( 'AO', 'AGO' ), ( 'AI', 'AIA' ), ( 'AQ', 'ATA' ), ( 'AG', 'ATG' ),
        ( 'AR', 'ARG' ), ( 'AM', 'ARM' ), ( 'AW', 'ABW' ), ( 'AU', 'AUS' ), ( 'AT', 'AUT' ),
        ( 'AZ', 'AZE' ), ( 'BS', 'BHS' ), ( 'BH', 'BHR' ), ( 'BD', 'BGD' ), ( 'BB', 'BRB' ),
        ( 'BY', 'BLR' ), ( 'BE', 'BEL' ), ( 'BZ', 'BLZ' ), ( 'BJ', 'BEN' ), ( 'BM', 'BMU' ),
        ( 'BT', 'BTN' ), ( 'BO', 'BOL' ), ( 'BQ', 'BES' ), ( 'BA', 'BIH' ), ( 'BW', 'BWA' ),
        ( 'BV', 'BVT' ), ( 'BR', 'BRA' ), ( 'IO', 'IOT' ), ( 'VG', 'VGB' ), ( 'BN', 'BRN' ),
        ( 'BG', 'BGR' ), ( 'BF', 'BFA' ), ( 'BI', 'BDI' ), ( 'KH', 'KHM' ), ( 'CM', 'CMR' ),
        ( 'CA', 'CAN' ), ( 'CV', 'CPV' ), ( 'KY', 'CYM' ), ( 'CF', 'CAF' ), ( 'TD', 'TCD' ),
        ( 'CL', 'CHL' ), ( 'CN', 'CHN' ), ( 'CX', 'CXR' ), ( 'CC', 'CCK' ), ( 'CO', 'COL' ),
        ( 'KM', 'COM' ), ( 'CK', 'COK' ), ( 'CR', 'CRI' ), ( 'HR', 'HRV' ), ( 'CU', 'CUB' ),
        ( 'CW', 'CUW' ), ( 'CY', 'CYP' ), ( 'CZ', 'CZE' ), ( 'CD', 'COD' ), ( 'DK', 'DNK' ),
        ( 'DJ', 'DJI' ), ( 'DM', 'DMA' ), ( 'DO', 'DOM' ), ( 'TL', 'TLS' ), ( 'EC', 'ECU' ),
        ( 'EG', 'EGY' ), ( 'SV', 'SLV' ), ( 'GQ', 'GNQ' ), ( 'ER', 'ERI' ), ( 'EE', 'EST' ),
        ( 'ET', 'ETH' ), ( 'FK', 'FLK' ), ( 'FO', 'FRO' ), ( 'FJ', 'FJI' ), ( 'FI', 'FIN' ),
        ( 'FR', 'FRA' ), ( 'GF', 'GUF' ), ( 'PF', 'PYF' ), ( 'TF', 'ATF' ), ( 'GA', 'GAB' ),
        ( 'GM', 'GMB' ), ( 'GE', 'GEO' ), ( 'DE', 'DEU' ), ( 'GH', 'GHA' ), ( 'GI', 'GIB' ),
        ( 'GR', 'GRC' ), ( 'GL', 'GRL' ), ( 'GD', 'GRD' ), ( 'GP', 'GLP' ), ( 'GU', 'GUM' ),
        ( 'GT', 'GTM' ), ( 'GG', 'GGY' ), ( 'GN', 'GIN' ), ( 'GW', 'GNB' ), ( 'GY', 'GUY' ),
        ( 'HT', 'HTI' ), ( 'HM', 'HMD' ), ( 'HN', 'HND' ), ( 'HK', 'HKG' ), ( 'HU', 'HUN' ),
        ( 'IS', 'ISL' ), ( 'IN', 'IND' ), ( 'ID', 'IDN' ), ( 'IR', 'IRN' ), ( 'IQ', 'IRQ' ),
        ( 'IE', 'IRL' ), ( 'IM', 'IMN' ), ( 'IL', 'ISR' ), ( 'IT', 'ITA' ), ( 'CI', 'CIV' ),
        ( 'JM', 'JAM' ), ( 'JP', 'JPN' ), ( 'JE', 'JEY' ), ( 'JO', 'JOR' ), ( 'KZ', 'KAZ' ),
        ( 'KE', 'KEN' ), ( 'KI', 'KIR' ), ( 'XK', 'XKX' ), ( 'KW', 'KWT' ), ( 'KG', 'KGZ' ),
        ( 'LA', 'LAO' ), ( 'LV', 'LVA' ), ( 'LB', 'LBN' ), ( 'LS', 'LSO' ), ( 'LR', 'LBR' ),
        ( 'LY', 'LBY' ), ( 'LI', 'LIE' ), ( 'LT', 'LTU' ), ( 'LU', 'LUX' ), ( 'MO', 'MAC' ),
        ( 'MK', 'MKD' ), ( 'MG', 'MDG' ), ( 'MW', 'MWI' ), ( 'MY', 'MYS' ), ( 'MV', 'MDV' ),
        ( 'ML', 'MLI' ), ( 'MT', 'MLT' ), ( 'MH', 'MHL' ), ( 'MQ', 'MTQ' ), ( 'MR', 'MRT' ),
        ( 'MU', 'MUS' ), ( 'YT', 'MYT' ), ( 'MX', 'MEX' ), ( 'FM', 'FSM' ), ( 'MD', 'MDA' ),
        ( 'MC', 'MCO' ), ( 'MN', 'MNG' ), ( 'ME', 'MNE' ), ( 'MS', 'MSR' ), ( 'MA', 'MAR' ),
        ( 'MZ', 'MOZ' ), ( 'MM', 'MMR' ), ( 'NA', 'NAM' ), ( 'NR', 'NRU' ), ( 'NP', 'NPL' ),
        ( 'NL', 'NLD' ), ( 'AN', 'ANT' ), ( 'NC', 'NCL' ), ( 'NZ', 'NZL' ), ( 'NI', 'NIC' ),
        ( 'NE', 'NER' ), ( 'NG', 'NGA' ), ( 'NU', 'NIU' ), ( 'NF', 'NFK' ), ( 'KP', 'PRK' ),
        ( 'MP', 'MNP' ), ( 'NO', 'NOR' ), ( 'OM', 'OMN' ), ( 'PK', 'PAK' ), ( 'PW', 'PLW' ),
        ( 'PS', 'PSE' ), ( 'PA', 'PAN' ), ( 'PG', 'PNG' ), ( 'PY', 'PRY' ), ( 'PE', 'PER' ),
        ( 'PH', 'PHL' ), ( 'PN', 'PCN' ), ( 'PL', 'POL' ), ( 'PT', 'PRT' ), ( 'PR', 'PRI' ),
        ( 'QA', 'QAT' ), ( 'CG', 'COG' ), ( 'RE', 'REU' ), ( 'RO', 'ROU' ), ( 'RU', 'RUS' ),
        ( 'RW', 'RWA' ), ( 'BL', 'BLM' ), ( 'SH', 'SHN' ), ( 'KN', 'KNA' ), ( 'LC', 'LCA' ),
        ( 'MF', 'MAF' ), ( 'PM', 'SPM' ), ( 'VC', 'VCT' ), ( 'WS', 'WSM' ), ( 'SM', 'SMR' ),
        ( 'ST', 'STP' ), ( 'SA', 'SAU' ), ( 'SN', 'SEN' ), ( 'RS', 'SRB' ), ( 'CS', 'SCG' ),
        ( 'SC', 'SYC' ), ( 'SL', 'SLE' ), ( 'SG', 'SGP' ), ( 'SX', 'SXM' ), ( 'SK', 'SVK' ),
        ( 'SI', 'SVN' ), ( 'SB', 'SLB' ), ( 'SO', 'SOM' ), ( 'ZA', 'ZAF' ), ( 'GS', 'SGS' ),
        ( 'KR', 'KOR' ), ( 'SS', 'SSD' ), ( 'ES', 'ESP' ), ( 'LK', 'LKA' ), ( 'SD', 'SDN' ),
        ( 'SR', 'SUR' ), ( 'SJ', 'SJM' ), ( 'SZ', 'SWZ' ), ( 'SE', 'SWE' ), ( 'CH', 'CHE' ),
        ( 'SY', 'SYR' ), ( 'TW', 'TWN' ), ( 'TJ', 'TJK' ), ( 'TZ', 'TZA' ), ( 'TH', 'THA' ),
        ( 'TG', 'TGO' ), ( 'TK', 'TKL' ), ( 'TO', 'TON' ), ( 'TT', 'TTO' ), ( 'TN', 'TUN' ),
        ( 'TR', 'TUR' ), ( 'TM', 'TKM' ), ( 'TC', 'TCA' ), ( 'TV', 'TUV' ), ( 'VI', 'VIR' ),
        ( 'UG', 'UGA' ), ( 'UA', 'UKR' ), ( 'AE', 'ARE' ), ( 'GB', 'GBR' ), ( 'US', 'USA' ),
        ( 'UM', 'UMI' ), ( 'UY', 'URY' ), ( 'UZ', 'UZB' ), ( 'VU', 'VUT' ), ( 'VA', 'VAT' ),
        ( 'VE', 'VEN' ), ( 'VN', 'VNM' ), ( 'WF', 'WLF' ), ( 'EH', 'ESH' ), ( 'YE', 'YEM' ),
        ( 'ZM', 'ZMB' ), ( 'ZW', 'ZWE' )
    ) AS [Mapping] ( [Alpha2], [Alpha3] )
)
INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
SELECT 1, @AttributeId, [dv].[Id], [iso].[Alpha3], NEWID()
FROM [IsoMapping] AS [iso]
INNER JOIN [DefinedValue] AS [dv]
    ON [dv].[Value] = [iso].[Alpha2]
    AND [dv].[DefinedTypeId] = @DefinedTypeId
WHERE NOT EXISTS (
    SELECT 1 FROM [AttributeValue] AS [av]
    WHERE [av].[AttributeId] = @AttributeId
    AND [av].[EntityId] = [dv].[Id]
);
" );
        }

        private void KH_FixGoupPlacementPersonFilter_Up()
        {
            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Add [spGetGroupPlacementPeople] (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spGetGroupPlacementPeople]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spGetGroupPlacementPeople];" );

            Sql( RockMigrationSQL._202607081740281_Rollup_20260708_295_FixGoupPlacementPersonFilter_spGetGroupPlacementPeople );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );
        }


        private void NA_FinalizeCoreAzureBlobStorageProviderConversion_Up()
        {
            Sql( @"
/*
    Finalizes the migration from the legacy Azure Blob Storage provider
    (rocks.pillars.AzureStorageProvider.AzureBlobStorage) to the Core
    Rock.Storage.Provider.AzureBlobStorage provider.

    Re-runs every step of v19 (UpdateExistingToCoreAzureBlobStorageProvider)
    to catch installations where the legacy provider was used again,
    PLUS three additional steps:

        STEP 4: Re-point existing BinaryFile rows from the legacy provider to
                the Core provider, renaming the per-BinaryFileType setting
                keys inside the StorageEntitySettings JSON snapshot.

        STEP 6: Mark the legacy provider Inactive (the 'STEP 5' v19 deferred).

        STEP 7: Delete the legacy provider EntityType.

    Each step uses UPSERT / WHERE NOT EXISTS guards, so a re-run after success
    is a no-op.
*/

DECLARE @LegacyEntityTypeName    NVARCHAR(200)    = N'rocks.pillars.AzureStorageProvider.AzureBlobStorage';
DECLARE @CoreProviderGuid        UNIQUEIDENTIFIER = '9925a20a-7262-4fc7-b86e-856f6d98be17';

DECLARE @LegacyAzureBlobStorageEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = @LegacyEntityTypeName);
DECLARE @CoreAzureBlobStorageEntityTypeId   INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = @CoreProviderGuid);

IF @LegacyAzureBlobStorageEntityTypeId IS NOT NULL
   AND @CoreAzureBlobStorageEntityTypeId IS NOT NULL
BEGIN

    -- Tag the legacy EntityType for the UI (idempotent).
    UPDATE [EntityType]
    SET [FriendlyName] = 'Azure Blob Storage (legacy)'
    WHERE [Id] = @LegacyAzureBlobStorageEntityTypeId
      AND ISNULL( [FriendlyName], '' ) <> 'Azure Blob Storage (legacy)';

    -- 'Active' attribute key (defined by the Component base class) for each provider EntityType.
    DECLARE @LegacyActiveAttrId INT = (SELECT TOP 1 a.[Id] FROM [Attribute] a WHERE a.[EntityTypeId] = @LegacyAzureBlobStorageEntityTypeId AND a.[Key] = N'Active');
    DECLARE @CoreActiveAttrId   INT = (SELECT TOP 1 a.[Id] FROM [Attribute] a WHERE a.[EntityTypeId] = @CoreAzureBlobStorageEntityTypeId   AND a.[Key] = N'Active');

    /* -----------------------------------------------------------------------
        STEP 1: Copy component-level AttributeValues (EntityId = 0) from the
        legacy provider to the Core provider, matched by Attribute.[Key].
        Covers: Active, Order, AccountName, AccountKey, CustomDomain,
        DefaultContainerName (and any other keys that happen to match).
       ----------------------------------------------------------------------- */
    UPDATE tgtAV
    SET tgtAV.[Value]                  = srcAV.[Value]
      , tgtAV.[ModifiedDateTime]       = GETDATE()
      , tgtAV.[IsPersistedValueDirty]  = 1
    FROM [AttributeValue] tgtAV
    INNER JOIN [Attribute]      tgtA  ON tgtA.[Id] = tgtAV.[AttributeId]
    INNER JOIN [Attribute]      srcA  ON srcA.[Key] = tgtA.[Key] AND srcA.[EntityTypeId] = @LegacyAzureBlobStorageEntityTypeId
    INNER JOIN [AttributeValue] srcAV ON srcAV.[AttributeId] = srcA.[Id] AND srcAV.[EntityId] = 0
    WHERE tgtA.[EntityTypeId] = @CoreAzureBlobStorageEntityTypeId
      AND tgtAV.[EntityId] = 0
      AND ISNULL( tgtAV.[Value], '' ) <> ISNULL( srcAV.[Value], '' );

    INSERT INTO [AttributeValue]
        ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid], [CreatedDateTime], [ModifiedDateTime], [IsPersistedValueDirty] )
    SELECT
        0, tgtA.[Id], 0, srcAV.[Value], NEWID(), GETDATE(), GETDATE(), 1
    FROM [Attribute] tgtA
    INNER JOIN [Attribute]      srcA  ON srcA.[Key] = tgtA.[Key] AND srcA.[EntityTypeId] = @LegacyAzureBlobStorageEntityTypeId
    INNER JOIN [AttributeValue] srcAV ON srcAV.[AttributeId] = srcA.[Id] AND srcAV.[EntityId] = 0
    WHERE tgtA.[EntityTypeId] = @CoreAzureBlobStorageEntityTypeId
      AND NOT EXISTS (
          SELECT 1 FROM [AttributeValue] existing
          WHERE existing.[AttributeId] = tgtA.[Id] AND existing.[EntityId] = 0
      );

    /* -----------------------------------------------------------------------
        STEP 2: Re-point any BinaryFileType rows from Legacy to Core. v19
        already did this; v20 catches any created against Legacy after v19.
       ----------------------------------------------------------------------- */
    UPDATE [BinaryFileType]
    SET [StorageEntityTypeId] = @CoreAzureBlobStorageEntityTypeId
    WHERE [StorageEntityTypeId] = @LegacyAzureBlobStorageEntityTypeId;

    /* -----------------------------------------------------------------------
        STEP 3: Copy per-BinaryFileType qualified AttributeValues from the
        legacy provider's attributes to the Core provider's attributes.
        Keys differ between providers, so we map them by Attribute.[Guid]:

            Legacy 'ContainerName'        (5AA80B71-825C-4757-8B04-AA4C233DC862)
              --> Core 'AzureBlobContainerName'       (5D921DDE-623A-4079-B987-25C74B4CDB7B)

            Legacy 'ContainerFolderPath'  (6465A83B-7A1F-4B90-BA73-862726ED8B41)
              --> Core 'AzureBlobContainerFolderPath' (BA7C28E6-B45E-4983-8A8D-96985E2C4EF4)

        AttributeValue.EntityId = BinaryFileType.Id and is preserved.
       ----------------------------------------------------------------------- */

    DECLARE @LegacyContainerNameAttrId       INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '5AA80B71-825C-4757-8B04-AA4C233DC862');
    DECLARE @LegacyContainerFolderPathAttrId INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '6465A83B-7A1F-4B90-BA73-862726ED8B41');
    DECLARE @CoreContainerNameAttrId         INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '5D921DDE-623A-4079-B987-25C74B4CDB7B');
    DECLARE @CoreContainerFolderPathAttrId   INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'BA7C28E6-B45E-4983-8A8D-96985E2C4EF4');

    -- Copy ContainerName --> AzureBlobContainerName.
    IF @LegacyContainerNameAttrId IS NOT NULL AND @CoreContainerNameAttrId IS NOT NULL
    BEGIN
        UPDATE tgtAV
        SET tgtAV.[Value]                  = srcAV.[Value]
          , tgtAV.[ModifiedDateTime]       = GETDATE()
          , tgtAV.[IsPersistedValueDirty]  = 1
        FROM [AttributeValue] tgtAV
        INNER JOIN [AttributeValue] srcAV ON srcAV.[AttributeId] = @LegacyContainerNameAttrId
                                         AND srcAV.[EntityId]    = tgtAV.[EntityId]
        WHERE tgtAV.[AttributeId] = @CoreContainerNameAttrId
          AND ISNULL( tgtAV.[Value], '' ) <> ISNULL( srcAV.[Value], '' );

        INSERT INTO [AttributeValue]
            ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid], [CreatedDateTime], [ModifiedDateTime], [IsPersistedValueDirty] )
        SELECT
            0, @CoreContainerNameAttrId, srcAV.[EntityId], srcAV.[Value], NEWID(), GETDATE(), GETDATE(), 1
        FROM [AttributeValue] srcAV
        WHERE srcAV.[AttributeId] = @LegacyContainerNameAttrId
          AND NOT EXISTS (
              SELECT 1 FROM [AttributeValue] existing
              WHERE existing.[AttributeId] = @CoreContainerNameAttrId
                AND existing.[EntityId]    = srcAV.[EntityId]
          );
    END

    -- Copy ContainerFolderPath --> AzureBlobContainerFolderPath.
    IF @LegacyContainerFolderPathAttrId IS NOT NULL AND @CoreContainerFolderPathAttrId IS NOT NULL
    BEGIN
        UPDATE tgtAV
        SET tgtAV.[Value]                  = srcAV.[Value]
          , tgtAV.[ModifiedDateTime]       = GETDATE()
          , tgtAV.[IsPersistedValueDirty]  = 1
        FROM [AttributeValue] tgtAV
        INNER JOIN [AttributeValue] srcAV ON srcAV.[AttributeId] = @LegacyContainerFolderPathAttrId
                                         AND srcAV.[EntityId]    = tgtAV.[EntityId]
        WHERE tgtAV.[AttributeId] = @CoreContainerFolderPathAttrId
          AND ISNULL( tgtAV.[Value], '' ) <> ISNULL( srcAV.[Value], '' );

        INSERT INTO [AttributeValue]
            ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid], [CreatedDateTime], [ModifiedDateTime], [IsPersistedValueDirty] )
        SELECT
            0, @CoreContainerFolderPathAttrId, srcAV.[EntityId], srcAV.[Value], NEWID(), GETDATE(), GETDATE(), 1
        FROM [AttributeValue] srcAV
        WHERE srcAV.[AttributeId] = @LegacyContainerFolderPathAttrId
          AND NOT EXISTS (
              SELECT 1 FROM [AttributeValue] existing
              WHERE existing.[AttributeId] = @CoreContainerFolderPathAttrId
                AND existing.[EntityId]    = srcAV.[EntityId]
          );
    END

    /* -----------------------------------------------------------------------
        STEP 4: Re-point existing BinaryFile rows from Legacy to Core, and
        rewrite the per-BinaryFileType setting keys inside the
        StorageEntitySettings JSON:

            ""ContainerName""        --> ""AzureBlobContainerName""
            ""ContainerFolderPath""  --> ""AzureBlobContainerFolderPath""

       ----------------------------------------------------------------------- */
    UPDATE [BinaryFile]
    SET
        [StorageEntityTypeId]    = @CoreAzureBlobStorageEntityTypeId,
        [StorageEntitySettings]  = REPLACE(
            REPLACE( [StorageEntitySettings], '""ContainerName""', '""AzureBlobContainerName""' ),
            '""ContainerFolderPath""',
            '""AzureBlobContainerFolderPath""'
        )
    WHERE [StorageEntityTypeId] = @LegacyAzureBlobStorageEntityTypeId;

    /* -----------------------------------------------------------------------
        STEP 5: Ensure the Core provider's Active = 'True'.
       ----------------------------------------------------------------------- */
    IF @CoreActiveAttrId IS NOT NULL
    BEGIN
        IF EXISTS ( SELECT 1 FROM [AttributeValue] WHERE [AttributeId] = @CoreActiveAttrId AND [EntityId] = 0 )
        BEGIN
            UPDATE [AttributeValue]
            SET [Value]                  = N'True'
              , [ModifiedDateTime]       = GETDATE()
              , [IsPersistedValueDirty]  = 1
            WHERE [AttributeId] = @CoreActiveAttrId
              AND [EntityId] = 0
              AND [Value] <> N'True';
        END
        ELSE
        BEGIN
            INSERT INTO [AttributeValue]
                ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid], [CreatedDateTime], [ModifiedDateTime], [IsPersistedValueDirty] )
            VALUES
                ( 0, @CoreActiveAttrId, 0, N'True', NEWID(), GETDATE(), GETDATE(), 1 );
        END
    END

    /* -----------------------------------------------------------------------
        STEP 6: Mark the legacy provider Inactive. Safe now because every
        BinaryFile and BinaryFileType has been re-pointed to Core above.
       ----------------------------------------------------------------------- */
    IF @LegacyActiveAttrId IS NOT NULL
    BEGIN
        IF EXISTS ( SELECT 1 FROM [AttributeValue] WHERE [AttributeId] = @LegacyActiveAttrId AND [EntityId] = 0 )
        BEGIN
            UPDATE [AttributeValue]
            SET [Value]                  = N'False'
              , [ModifiedDateTime]       = GETDATE()
              , [IsPersistedValueDirty]  = 1
            WHERE [AttributeId] = @LegacyActiveAttrId
              AND [EntityId] = 0
              AND [Value] <> N'False';
        END
        ELSE
        BEGIN
            INSERT INTO [AttributeValue]
                ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid], [CreatedDateTime], [ModifiedDateTime], [IsPersistedValueDirty] )
            VALUES
                ( 0, @LegacyActiveAttrId, 0, N'False', NEWID(), GETDATE(), GETDATE(), 1 );
        END
    END

    /* -----------------------------------------------------------------------
        STEP 7: Delete the legacy EntityType row and its associated metadata
        (component-level AttributeValues, per-BinaryFileType AttributeValues,
        and the Attribute definitions themselves).

        Safe at this point because:
          - All BinaryFileType.StorageEntityTypeId rows were re-pointed in STEP 2.
          - All BinaryFile.StorageEntityTypeId rows were re-pointed in STEP 4.
          - The legacy plugin DLL is removed from ~/Bin in this same migration
            (post-SQL, below), so no startup code can re-register the
            EntityType by name.

        Wrapped in TRY/CATCH so an unforeseen FK reference does not fail the
        whole migration. The component is already Inactive after STEP 6, so
        leaving the EntityType row in place is an acceptable fallback.
       ----------------------------------------------------------------------- */
    BEGIN TRY

        -- Remove AttributeValues that belong to the legacy EntityType's Attributes.
        DELETE av
        FROM [AttributeValue] av
        INNER JOIN [Attribute] a ON a.[Id] = av.[AttributeId]
        WHERE a.[EntityTypeId] = @LegacyAzureBlobStorageEntityTypeId;

        -- Remove the Attribute definitions for the legacy EntityType.
        DELETE FROM [Attribute]
        WHERE [EntityTypeId] = @LegacyAzureBlobStorageEntityTypeId;

        -- Remove the EntityType row itself.
        DELETE FROM [EntityType]
        WHERE [Id] = @LegacyAzureBlobStorageEntityTypeId;

    END TRY
    BEGIN CATCH
        -- Intentionally swallowed: an unexpected FK reference will leave the
        -- legacy EntityType in place. STEP 6 already marked it Inactive, so
        -- the system remains in a fully migrated state either way.
    END CATCH

END
" );

            /*
                6/9/26 - NA

                Delete the legacy AzureStorageProvider assembly files from /bin.
                The Rock Update process should already have removed these via
                its deletefile.lst manifest, so on a normal, Rock-Update-driven
                upgrade this is a no-op. It exists as a fallback for systems
                (or developer workstations) that bypass the Rock Update process
                and may still have the old legacy DLL/PDB.
                
                Reason: Fallback cleanup of the legacy plugin assembly for
                installs that do not use the standard Rock Update process.
            */
            try
            {
                var path = HostingEnvironment.MapPath( "~/Bin/rocks.pillars.AzureStorageProvider.dll" );
                if ( File.Exists( path ) )
                {
                    File.Delete( path );
                }

                path = HostingEnvironment.MapPath( "~/Bin/rocks.pillars.AzureStorageProvider.pdb" );
                if ( File.Exists( path ) )
                {
                    File.Delete( path );
                }
            }
            catch ( Exception ex )
            {
                System.Diagnostics.Debug.WriteLine( $"Error during Legacy AzureStorageProvider cleanup: {ex.Message}" );
            }
        }

        /// <summary>
        /// Updates the icon for the Personalization Segments page.
        /// </summary>
        /// <remarks>
        /// This migration locates the well-known <c>Page</c> record by its known <c>Guid</c>
        /// (905F6132-AE1C-4C85-9752-18D22E604C3A) and sets its <c>IconCssClass</c>
        /// to <c>ti ti-user-circle</c>.
        /// </remarks>
        private void NA_Update_PersonalizationSegmentsPageIcon_Up()
        {
            Sql( $@"UPDATE [dbo].[Page] SET [IconCssClass] = 'ti ti-user-circle' WHERE [Guid] = '905F6132-AE1C-4C85-9752-18D22E604C3A'" ); // Personalization Segments Page
        }

        private void KH_FixGroupPlacementWaitList_Up()
        {
            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Add [spGetGroupPlacementPeople] (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spGetGroupPlacementPeople]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spGetGroupPlacementPeople];" );

            Sql( RockMigrationSQL._202607081740281_Rollup_20260708_298_FixGroupPlacementWaitList_spGetGroupPlacementPeople );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );
        }

        private void NA_FixAnalyticsETLFamilyOperatorPrecedence_Up()
        {
            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Add [spAnalytics_ETL_Family] (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spAnalytics_ETL_Family]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spAnalytics_ETL_Family];" );

            Sql( RockMigrationSQL._202607081740281_Rollup_20260708_299_FixAnalyticsETLFamilyOperatorPrecedence_spAnalytics_ETL_Family );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );
        }

        private void JH_AddCategoryTreeViewToFormBuilderPage_Up()
        {
            // Switch the Form Builder page to the internal-site Left Sidebar layout so the tree and
            // the form list sit side by side. (Rock.SystemGuid.Layout.LEFT_SIDEBAR_INTERNAL_SITE.)
            Sql( @"
UPDATE dbo.[Page]
SET [LayoutId] = (
    SELECT [Id]
    FROM dbo.[Layout]
    WHERE [Guid] = '0CB60906-6B74-44FD-AB25-026050EF70EB'
)
WHERE [Guid] = '4F77819C-8F69-4418-933E-08F63E7FC4F9';
" );

            // Place the Category Tree View block in the page's Sidebar 1 zone. The block type
            // (ADE003C7) is the converted WebForms Category Tree View; the startup conversion rewrites
            // it to the Obsidian entity-based block, so no block-type registration is needed here.
            RockMigrationHelper.AddBlock( true, "4F77819C-8F69-4418-933E-08F63E7FC4F9".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "ADE003C7-649B-466A-872B-B8AC952E7841".AsGuid(), "Form Categories", "Sidebar1", @"", @"", 0, "3F8E1C5A-7B2D-4A9E-8C6F-1D4B7E2A9C53" );

            /*
                06/11/26 - JMH

                The Form Builder placement is configured by attribute Key rather than attribute Guid.
                The block adopts the long-standing WebForms block type, whose attribute Guids are not
                known here, and the order in which the converted block's attributes register at startup
                relative to this migration is not guaranteed. Resolving by Key (and creating the newer
                Boolean attributes when they are missing) keeps the configuration correct either way.

                Reason: Configure the converted block without depending on attribute Guids or startup order.
            */
            Sql( $@"
DECLARE @BlockId INT = ( SELECT [Id] FROM [Block] WHERE [Guid] = '3F8E1C5A-7B2D-4A9E-8C6F-1D4B7E2A9C53' );
DECLARE @BlockTypeId INT = ( SELECT [Id] FROM [BlockType] WHERE [Guid] = 'ADE003C7-649B-466A-872B-B8AC952E7841' );
DECLARE @BlockEntityTypeId INT = ( SELECT [Id] FROM [EntityType] WHERE [Guid] = '{SystemGuid.EntityType.BLOCK}' );
DECLARE @BooleanFieldTypeId INT = ( SELECT [Id] FROM [FieldType] WHERE [Guid] = '{SystemGuid.FieldType.BOOLEAN}' );
DECLARE @Now DATETIMEOFFSET = SYSDATETIMEOFFSET();

IF @BlockId IS NULL OR @BlockTypeId IS NULL
BEGIN
    RETURN;
END

DECLARE @QualifierValue NVARCHAR(40) = CONVERT( NVARCHAR(40), @BlockTypeId );

DECLARE @EntityTypeAttributeId INT = ( SELECT [Id] FROM [Attribute] WHERE [Key] = 'EntityType' AND [EntityTypeId] = @BlockEntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = @QualifierValue );
DECLARE @FriendlyNameAttributeId INT = ( SELECT [Id] FROM [Attribute] WHERE [Key] = 'EntityTypeFriendlyName' AND [EntityTypeId] = @BlockEntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = @QualifierValue );
DECLARE @ShowOnlyCategoriesAttributeId INT = ( SELECT [Id] FROM [Attribute] WHERE [Key] = 'ShowOnlyCategories' AND [EntityTypeId] = @BlockEntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = @QualifierValue );

-- Show Only Categories is a Boolean setting that may not be registered yet; create it if it is
-- missing so its value can be set below.
IF @ShowOnlyCategoriesAttributeId IS NULL AND @BooleanFieldTypeId IS NOT NULL
BEGIN
    INSERT [Attribute] ( [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Description], [Order], [IsGridColumn], [IsMultiValue], [IsRequired], [DefaultValue], [Guid] )
    VALUES ( 1, @BooleanFieldTypeId, @BlockEntityTypeId, 'BlockTypeId', @QualifierValue, 'ShowOnlyCategories', 'Show Only Categories', 'Set to true to show only the categories rather than the categorized entities for the configured entity type.', 0, 0, 0, 0, 'False', NEWID() );
    SET @ShowOnlyCategoriesAttributeId = SCOPE_IDENTITY();
END

-- Entity Type => Workflow Type.
IF @EntityTypeAttributeId IS NOT NULL
BEGIN
    IF EXISTS ( SELECT 1 FROM [AttributeValue] WHERE [AttributeId] = @EntityTypeAttributeId AND [EntityId] = @BlockId )
        UPDATE [AttributeValue] SET [Value] = 'C9F3C4A5-1526-474D-803F-D6C7A45CBBAE', [ModifiedDateTime] = @Now WHERE [AttributeId] = @EntityTypeAttributeId AND [EntityId] = @BlockId;
    ELSE
        INSERT [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid], [IsPersistedValueDirty], [CreatedDateTime], [ModifiedDateTime] )
        VALUES ( 0, @EntityTypeAttributeId, @BlockId, 'C9F3C4A5-1526-474D-803F-D6C7A45CBBAE', NEWID(), 1, @Now, @Now );
END

-- Entity Type Friendly Name => panel title 'Form Categories'.
IF @FriendlyNameAttributeId IS NOT NULL
BEGIN
    IF EXISTS ( SELECT 1 FROM [AttributeValue] WHERE [AttributeId] = @FriendlyNameAttributeId AND [EntityId] = @BlockId )
        UPDATE [AttributeValue] SET [Value] = 'Form Categories', [ModifiedDateTime] = @Now WHERE [AttributeId] = @FriendlyNameAttributeId AND [EntityId] = @BlockId;
    ELSE
        INSERT [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid], [IsPersistedValueDirty], [CreatedDateTime], [ModifiedDateTime] )
        VALUES ( 0, @FriendlyNameAttributeId, @BlockId, 'Form Categories', NEWID(), 1, @Now, @Now );
END

-- Show Only Categories => True (the tree shows categories; the Form List shows the forms).
IF @ShowOnlyCategoriesAttributeId IS NOT NULL
BEGIN
    IF EXISTS ( SELECT 1 FROM [AttributeValue] WHERE [AttributeId] = @ShowOnlyCategoriesAttributeId AND [EntityId] = @BlockId )
        UPDATE [AttributeValue] SET [Value] = 'True', [ModifiedDateTime] = @Now WHERE [AttributeId] = @ShowOnlyCategoriesAttributeId AND [EntityId] = @BlockId;
    ELSE
        INSERT [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid], [IsPersistedValueDirty], [CreatedDateTime], [ModifiedDateTime] )
        VALUES ( 0, @ShowOnlyCategoriesAttributeId, @BlockId, 'True', NEWID(), 1, @Now, @Now );
END

DECLARE @DetailPageAttributeId INT = ( SELECT [Id] FROM [Attribute] WHERE [Key] = 'DetailPage' AND [EntityTypeId] = @BlockEntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = @QualifierValue );
DECLARE @PageReferenceFieldTypeId INT = ( SELECT [Id] FROM [FieldType] WHERE [Guid] = '{SystemGuid.FieldType.PAGE_REFERENCE}' );

-- Detail Page is a Page Reference setting that may not be registered yet on a fresh install (the
-- converted block registers its attributes at startup, after this migration runs); create it if it
-- is missing so its value can be set below.
IF @DetailPageAttributeId IS NULL AND @PageReferenceFieldTypeId IS NOT NULL
BEGIN
    INSERT [Attribute] ( [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Description], [Order], [IsGridColumn], [IsMultiValue], [IsRequired], [DefaultValue], [Guid] )
    VALUES ( 1, @PageReferenceFieldTypeId, @BlockEntityTypeId, 'BlockTypeId', @QualifierValue, 'DetailPage', 'Detail Page', 'The page to navigate to when a category or item is selected.', 0, 0, 0, 0, '', NEWID() );
    SET @DetailPageAttributeId = SCOPE_IDENTITY();
END

-- Detail Page => the Form Builder page itself, paired with its 'admin/general/form-builder' route.
-- Without this the navigate URL has no configured page to resolve and falls back to the page/{{Id}}
-- form; setting the page and route makes a category click land on the friendly route with CategoryId.
IF @DetailPageAttributeId IS NOT NULL
BEGIN
    IF EXISTS ( SELECT 1 FROM [AttributeValue] WHERE [AttributeId] = @DetailPageAttributeId AND [EntityId] = @BlockId )
        UPDATE [AttributeValue] SET [Value] = '4f77819c-8f69-4418-933e-08f63e7fc4f9,335f2313-7fc1-42b4-ad8e-4c2a965f3380', [ModifiedDateTime] = @Now WHERE [AttributeId] = @DetailPageAttributeId AND [EntityId] = @BlockId;
    ELSE
        INSERT [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid], [IsPersistedValueDirty], [CreatedDateTime], [ModifiedDateTime] )
        VALUES ( 0, @DetailPageAttributeId, @BlockId, '4f77819c-8f69-4418-933e-08f63e7fc4f9,335f2313-7fc1-42b4-ad8e-4c2a965f3380', NEWID(), 1, @Now, @Now );
END
" );

            // Rename the 'Well' workflow form section style defined value to 'Card'. Both columns are
            // guarded with CASE so a site that already changed the value or description is left untouched.
            Sql( @"
UPDATE [DefinedValue]
SET [Value] = CASE WHEN [Value] = 'Well' THEN 'Card' ELSE [Value] END
    , [Description] = CASE WHEN [Description] = 'This will apply a ''well'' with a muted background color and some padding.' THEN 'This will apply a ''card'' with a muted background color and some padding.' ELSE [Description] END
WHERE [Guid] = '2D6369C1-3B39-4E94-8122-78C55A962C33';
" );
        }

        /// <summary>
        /// Updates the icon for the Personalization Segments page and Experience Manager page.
        /// </summary>
        private void NA_Update_PageIcons_V19_3_Up()
        {
            Sql( $@"
UPDATE [dbo].[Page] SET [IconCssClass] = 'ti ti-user-circle' WHERE [Guid] = '905F6132-AE1C-4C85-9752-18D22E604C3A' -- Personalization Segments Page
UPDATE [dbo].[Page] SET [IconCssClass] = 'ti ti-chalkboard-teacher' WHERE [Guid] = '1DA3B534-FB71-483B-BD64-9BFB92F59123' -- Experience Manager Page
" );
        }

        private void ChangeConnectionStatusColorAttributeToColorPickerPart2_Up()
        {
            // Fix to address migration https://github.com/SparkDevNetwork/Rock/blame/980777e51eb4e3343f3707e9d173cddcf6d3c666/Rock.Migrations/Migrations/Version%2018.0/Version%2018.0/202509301628253_Rollup_20250930.cs#L594
            // in the event that an old custom Rock instance did was built without the original 1121 attributeId from original v1.0 beta migration 201407091948108_GroupMap.cs
            Sql( @"DECLARE @ColorAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '23777A50-E000-4F29-994F-26635A357160' )

-- add a new [AttributeQualifier] of 'selectiontype' (Key) and 'Color Picker' (Value)
IF NOT EXISTS (
    SELECT 1
    FROM AttributeQualifier
    WHERE AttributeId = @ColorAttributeId
      AND [Key] = 'selectiontype'
)
BEGIN
    INSERT INTO AttributeQualifier ([IsSystem], [AttributeId], [Key], [Value], [Guid])
    VALUES (1, @ColorAttributeId, 'selectiontype', 'Color Picker', NEWID() );
END" );
        }
        private void JE_TablerIconReplaceInAttributeDefaultValues_Up()
        {
            Sql( @"
UPDATE [Attribute]
SET
    [DefaultValue] = REPLACE( [DefaultValue], 'fa fa-gear', 'ti ti-settings' ),
    [ModifiedDateTime] = GETDATE()
WHERE [DefaultValue] LIKE '%fa fa-gear%'

UPDATE [Attribute]
SET
    [DefaultValue] = REPLACE( [DefaultValue], 'fa fa-arrow-circle-right', 'ti ti-circle-arrow-right' ),
    [ModifiedDateTime] = GETDATE()
WHERE [DefaultValue] LIKE '%fa fa-arrow-circle-right%'

UPDATE [Attribute]
SET
    [DefaultValue] = REPLACE( [DefaultValue], 'fa fa-user-lock', 'ti ti-lock' ),
    [ModifiedDateTime] = GETDATE()
WHERE [DefaultValue] LIKE '%fa fa-user-lock%'

UPDATE [Attribute]
SET
    [DefaultValue] = REPLACE( [DefaultValue], 'fas fa-user-lock', 'ti ti-lock' ),
    [ModifiedDateTime] = GETDATE()
WHERE [DefaultValue] LIKE '%fas fa-user-lock%'

UPDATE [Attribute]
SET
    [DefaultValue] = REPLACE( [DefaultValue], 'fa fa-flag', 'ti ti-flag' ),
    [ModifiedDateTime] = GETDATE()
WHERE [DefaultValue] LIKE '%fa fa-flag%'

UPDATE [Attribute]
SET
    [DefaultValue] = REPLACE( [DefaultValue], 'fa fa-calendar-alt', 'ti ti-calendar' ),
    [ModifiedDateTime] = GETDATE()
WHERE [DefaultValue] LIKE '%fa fa-calendar-alt%'

UPDATE [Attribute]
SET
    [DefaultValue] = REPLACE( [DefaultValue], 'fa fa-shield-alt', 'ti ti-shield' ),
    [ModifiedDateTime] = GETDATE()
WHERE [DefaultValue] LIKE '%fa fa-shield-alt%'

UPDATE [Attribute]
SET
    [DefaultValue] = REPLACE( [DefaultValue], 'fas fa-shield-alt', 'ti ti-shield' ),
    [ModifiedDateTime] = GETDATE()
WHERE [DefaultValue] LIKE '%fas fa-shield-alt%'

UPDATE [Attribute]
SET
    [DefaultValue] = REPLACE( [DefaultValue], 'fa fa-print', 'ti ti-printer' ),
    [ModifiedDateTime] = GETDATE()
WHERE [DefaultValue] LIKE '%fa fa-print%'

UPDATE [Attribute]
SET
    [DefaultValue] = REPLACE( [DefaultValue], 'fa fa-calendar', 'ti ti-calendar' ),
    [ModifiedDateTime] = GETDATE()
WHERE [DefaultValue] LIKE '%fa fa-calendar%'

UPDATE [Attribute]
SET
    [DefaultValue] = REPLACE( [DefaultValue], 'fa fa-plus', 'ti ti-plus' ),
    [ModifiedDateTime] = GETDATE()
WHERE [DefaultValue] LIKE '%fa fa-plus%'

UPDATE [Attribute]
SET
    [DefaultValue] = REPLACE( [DefaultValue], 'fa-plus', 'ti-plus' ),
    [ModifiedDateTime] = GETDATE()
WHERE [DefaultValue] LIKE '%fa-plus%'

UPDATE [Attribute]
SET
    [DefaultValue] = REPLACE( [DefaultValue], 'fa-minus', 'ti-minus' ),
    [ModifiedDateTime] = GETDATE()
WHERE [DefaultValue] LIKE '%fa-minus%'
" );
        }

    }
}
