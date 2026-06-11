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
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class RemainingV19DataChanges : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // v18.3 hotfix data migration #281
            NA_Fix_ConnectValidExistingSignatureDocumentsToRegistrants6737_Up();

            // v18.3 hotfix data migration #282
            NA_SetDefaultImageForTemplateDefinedTypeIconAttribute_Up();

            JMH_UpdateColorPickerSwatchesDefinedType_Up();
            DH_EnableObsidianComponentsInDataViewRelatedBlocks_Up();
            JMH_AddCommunicationEntryWizardCustomizeTextBlockSettings_Up();
            NA_RemoveLegacyCloudflareTurnstileSystemSettings_Up();
            CleanupMigrationHistory();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            JMH_UpdateColorPickerSwatchesDefinedType_Down();

            JMH_AddCommunicationEntryWizardCustomizeTextBlockSettings_Down();
        }


        /// <summary>
        /// Updates the Category EntityType to add "ViewList" that goes along with the fix to issue 6712.
        /// https://github.com/SparkDevNetwork/Rock/issues/6712
        /// </summary>
        private void NA_Fix_ConnectValidExistingSignatureDocumentsToRegistrants6737_Up()
        {
            Sql( @"
/* 
    Migration: Backfill RegistrationRegistrant.SignatureDocumentId when missing, using an existing valid
    signature document from another registrant for the same Person (across any PersonAlias),
    AND only when both registrants' RegistrationTemplates require the same SignatureDocumentTemplateId
    AND only when signed document was created before the registrant record.
*/

DECLARE @Today date = CONVERT( date, GETDATE() );

;WITH Targets AS
(
    SELECT
        rr.Id              AS RegistrationRegistrantId,
        rr.CreatedDateTime AS RegistrantCreatedDateTime,
        paTarget.PersonId  AS PersonId,
        sdt.Id             AS SignatureDocumentTemplateId,
        DATEADD( day, 1 - sdt.ValidityDurationInDays, @Today ) AS EarliestSignatureDate
    FROM dbo.RegistrationRegistrant rr
    INNER JOIN dbo.PersonAlias paTarget ON paTarget.Id = rr.PersonAliasId
    INNER JOIN dbo.RegistrationTemplate rt ON rt.Id = rr.RegistrationTemplateId
    INNER JOIN dbo.SignatureDocumentTemplate sdt ON sdt.Id = rt.RequiredSignatureDocumentTemplateId
    WHERE
        rr.SignatureDocumentId IS NULL
        AND rt.RequiredSignatureDocumentTemplateId IS NOT NULL
        AND sdt.IsActive = 1
        AND sdt.ProviderEntityTypeId IS NULL
        AND sdt.IsValidInFuture = 1
        AND sdt.ValidityDurationInDays IS NOT NULL
)
UPDATE rr
    SET rr.SignatureDocumentId = v.SignatureDocumentId
FROM dbo.RegistrationRegistrant rr
INNER JOIN Targets t ON t.RegistrationRegistrantId = rr.Id
CROSS APPLY
(
    SELECT TOP (1)
        sd.Id AS SignatureDocumentId
    FROM dbo.RegistrationRegistrant rr2
    INNER JOIN dbo.RegistrationTemplate rt2 ON rt2.Id = rr2.RegistrationTemplateId
    INNER JOIN dbo.SignatureDocument sd ON sd.Id = rr2.SignatureDocumentId
    INNER JOIN dbo.PersonAlias paAppliesTo ON paAppliesTo.Id = sd.AppliesToPersonAliasId
    WHERE
        rr2.SignatureDocumentId IS NOT NULL
        AND paAppliesTo.PersonId = t.PersonId
        AND sd.SignatureDocumentTemplateId = t.SignatureDocumentTemplateId
        AND sd.SignedDateTime >= t.EarliestSignatureDate
        AND rt2.RequiredSignatureDocumentTemplateId = t.SignatureDocumentTemplateId
        AND t.RegistrantCreatedDateTime IS NOT NULL
        AND sd.CreatedDateTime IS NOT NULL
        AND sd.CreatedDateTime < t.RegistrantCreatedDateTime
    ORDER BY
        sd.SignedDateTime DESC,
        sd.Id DESC
) v
WHERE
    rr.SignatureDocumentId IS NULL
    AND v.SignatureDocumentId IS NOT NULL;
" );
        }

        /// <summary>
        /// We need to add a default image to this attribute "Icon" attribute which is
        /// defined on DefinedType called "Template".  The Icon attribute was originally
        /// via 202001081542541_Rollup_01081.cs but no default image was set at that time.
        /// See https://app.asana.com/1/20866866924293/project/1208321217019996/task/1211303414636849?focus=true
        /// </summary>
        private void NA_SetDefaultImageForTemplateDefinedTypeIconAttribute_Up()
        {
            string newBinaryFileGuid = "3FC8FF25-13FC-4CED-81F1-E7C73205C18A";
            string standardIconSvg = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";
            RockMigrationHelper.AddOrUpdateBinaryFileForDatabaseStorage( Rock.SystemGuid.BinaryFiletype.DEFAULT, standardIconSvg, "standard-template.svg", "image/svg+xml", "", newBinaryFileGuid );

            // Also, add this other possible missing BinaryFile for Guid 8B53F981-6FF6-4657-9CD5-01E36EB0DF51
            RockMigrationHelper.AddOrUpdateBinaryFileForDatabaseStorage( Rock.SystemGuid.BinaryFiletype.DEFAULT, standardIconSvg, "standard-template.svg", "image/svg+xml", "", "8B53F981-6FF6-4657-9CD5-01E36EB0DF51" );

            // Now we need to set the default value of the Icon attribute for the Template DefinedType to be the image we just added.
            Sql( $@"
DECLARE @IconAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '831403eb-262e-4bc5-8b5e-f16153493bf5')
IF @IconAttributeId IS NOT NULL
BEGIN
    UPDATE [Attribute] SET [DefaultValue] = '{newBinaryFileGuid}', IsDefaultPersistedValueDirty = 1 WHERE Id = @IconAttributeId
END
            " );

            // We also need to do some other minor data maintenance on certain defined values.
            Sql( @"
UPDATE [DefinedValue]
    SET [Value] = 'Legacy 2'
    , [Order] = 3
    WHERE [Guid] = '74760472-3516-480d-b96b-1f77aaef0862'
    
UPDATE [DefinedValue]
    SET [Order] = 2
    WHERE [Guid] = '74760472-3516-480d-b96b-1f77aaef0862'
    
UPDATE [DefinedValue]
    SET [Order] = 1
    WHERE [Guid] = '5114db6e-40cc-4455-99b8-c109b7bb52d1'
" );
        }

        /// <summary>
        /// Applies the following updates to the "Color Picker Swatches" Defined Type.
        /// <list type="bullet">
        ///     <item>
        ///         <description>Adds Display Location attribute with options: "Custom" and "Palette Color".</description>
        ///     </item>
        ///     <item>
        ///         <description>Adds new "Palette Color" Defined Values: "Primary", "Secondary", "White", and "Black".</description>
        ///     </item>
        ///     <item>
        ///         <description>Updates default "Custom" Defines Values to use Tailwind v4 colors.</description>
        ///     </item>
        /// </list>
        /// </summary>
        private void JMH_UpdateColorPickerSwatchesDefinedType_Up()
        {
            // Add Display Location attribute to Color Picker Swatches defined type
            RockMigrationHelper.AddDefinedTypeAttribute( "CC1400B3-E161-45E3-BF49-49825D3D6467", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Display Location", "DisplayLocation", "", 2031, "Custom", "2B3F5FD0-52C1-48BD-B633-FA565E886A74" );
            Sql( $@"
    DECLARE @DefinedTypeId INT = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = 'CC1400B3-E161-45E3-BF49-49825D3D6467')
    DECLARE @FieldTypeId INT = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '7525C4CB-EE6B-41D4-9B64-A08048D5A5C0')
    DECLARE @EntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.DefinedValue')

    IF EXISTS (
        SELECT [Id]
          FROM [Attribute]
         WHERE [EntityTypeId] = @EntityTypeId
               AND [EntityTypeQualifierColumn] = 'DefinedTypeId'
               AND [EntityTypeQualifierValue] = CAST(@DefinedTypeId as varchar)
               AND [Guid] = '2B3F5FD0-52C1-48BD-B633-FA565E886A74' )
    BEGIN

        UPDATE [Attribute] 
           SET [IsGridColumn] = 1
         WHERE [EntityTypeId] = @EntityTypeId
               AND [EntityTypeQualifierColumn] = 'DefinedTypeId'
               AND [EntityTypeQualifierValue] = CAST(@DefinedTypeId as varchar)
               AND [Guid] = '2B3F5FD0-52C1-48BD-B633-FA565E886A74'

    END" );
            RockMigrationHelper.AddAttributeQualifier( "2B3F5FD0-52C1-48BD-B633-FA565E886A74", "fieldtype", "ddl", "99A4010F-ACFB-4056-A451-07F21EAFC687" );
            RockMigrationHelper.AddAttributeQualifier( "2B3F5FD0-52C1-48BD-B633-FA565E886A74", "repeatColumns", "", "A1C073F4-5EE5-4549-BFEC-BE17FCDF62FF" );
            RockMigrationHelper.AddAttributeQualifier( "2B3F5FD0-52C1-48BD-B633-FA565E886A74", "values", "Custom,Palette Color", "AF44DCE1-5D44-4EE6-B73D-4EDA4CFABC7E" );

            // Add Palette Color defined values
            RockMigrationHelper.UpdateDefinedValue( "CC1400B3-E161-45E3-BF49-49825D3D6467", "#ff791d", "Primary", "5D73F5EE-3257-40D7-92F9-6BCFB3BEB2A8", true );
            RockMigrationHelper.UpdateDefinedValue( "CC1400B3-E161-45E3-BF49-49825D3D6467", "#83758f", "Secondary", "05FF6690-7113-479F-967E-05ED5A07418E", true );
            RockMigrationHelper.UpdateDefinedValue( "CC1400B3-E161-45E3-BF49-49825D3D6467", "#fafafa", "White", "DAE5084D-1A9C-4D8C-B4FD-E1DE80D77314", true );
            RockMigrationHelper.UpdateDefinedValue( "CC1400B3-E161-45E3-BF49-49825D3D6467", "#09090b", "Black", "64BEB38D-294C-4524-A8D9-A09D07185390", true );

            // Associate Display Location attribute to Palette Color defined values
            RockMigrationHelper.AddDefinedValueAttributeValue( "5D73F5EE-3257-40D7-92F9-6BCFB3BEB2A8", "2B3F5FD0-52C1-48BD-B633-FA565E886A74", @"Palette Color" ); // Primary
            RockMigrationHelper.AddDefinedValueAttributeValue( "05FF6690-7113-479F-967E-05ED5A07418E", "2B3F5FD0-52C1-48BD-B633-FA565E886A74", @"Palette Color" ); // Secondary
            RockMigrationHelper.AddDefinedValueAttributeValue( "DAE5084D-1A9C-4D8C-B4FD-E1DE80D77314", "2B3F5FD0-52C1-48BD-B633-FA565E886A74", @"Palette Color" ); // White
            RockMigrationHelper.AddDefinedValueAttributeValue( "64BEB38D-294C-4524-A8D9-A09D07185390", "2B3F5FD0-52C1-48BD-B633-FA565E886A74", @"Palette Color" ); // Black

            // Update Custom defined values to new colors
            Sql( @"
UPDATE [DefinedValue] 
   SET [Value] = '#fb2c36'
 WHERE [Guid] = '69E71798-DF68-4ED7-93AC-CFE9915D664D' /* Red */
       AND [Value] = '#f44336'

UPDATE [DefinedValue] 
   SET [Value] = '#ad46ff'
 WHERE [Guid] = 'EDFF48D7-DE7C-455D-9B89-86614D3FA14D' /* Purple */
       AND [Value] = '#9c27b0'

UPDATE [DefinedValue] 
   SET [Value] = '#615fff'
 WHERE [Guid] = '0DA03527-0DBB-41D2-A3D1-DE60D10E4193' /* Indigo */
       AND [Value] = '#3f51b5'

UPDATE [DefinedValue] 
   SET [Value] = '#2b7fff'
 WHERE [Guid] = 'D64BD9F7-5A0E-4709-9A0E-B8F88ED360F9' /* Blue */
       AND [Value] = '#2196f3'

UPDATE [DefinedValue] 
   SET [Value] = '#00c950' 
 WHERE [Guid] = 'B18A4333-A5BA-418A-9559-0CEB59770D33' /* Green */
       AND [Value] = '#4caf50'

UPDATE [DefinedValue] 
   SET [Value] = '#f0b100' 
 WHERE [Guid] = '80051D45-6D75-4316-BD12-2EBCA226F0BC' /* Yellow */
       AND [Value] = '#ffeb3b'" );
        }

        private void DH_EnableObsidianComponentsInDataViewRelatedBlocks_Up()
        {
            // Enable Obsidian components in Data View related blocks
            Sql( @"
UPDATE av
SET
    av.[Value] = 'True',
    av.[PersistedTextValue] = 'Yes',
    av.[PersistedCondensedTextValue] = 'Y',
    av.[PersistedCondensedHtmlValue] = 'Y',
    av.[IsPersistedValueDirty] = 1,
    av.ValueAsBoolean = 1
FROM dbo.[AttributeValue] AS av
INNER JOIN dbo.[Attribute] AS a ON a.[Id] = av.[AttributeId]
INNER JOIN dbo.[BlockType] AS bt ON bt.[Id] = TRY_CAST(a.[EntityTypeQualifierValue] AS INT)
WHERE a.[Key] = 'UseObsidianComponents'
  AND a.[EntityTypeId] = 9
  AND a.[EntityTypeQualifierColumn] = 'BlockTypeId'
  AND bt.[Guid] IN (
    'EB279DF9-D817-4905-B6AC-D9883F0DA2E4', -- Data View Detail
    'E431DBDF-5C65-45DC-ADC5-157A02045CCD', -- Report Detail
    'C7C069DB-9EEE-4245-9DF2-34E3A1FF4CCB' -- Dynamic Report
  )
  AND av.[Value] = 'False';

INSERT INTO [dbo].[AttributeValue]
(
    [IsSystem], [AttributeId], [EntityId],
    [Value], [Guid],
    [PersistedTextValue], [PersistedHtmlValue], [PersistedCondensedTextValue], [PersistedCondensedHtmlValue],
    [IsPersistedValueDirty], [ValueAsBoolean]
)
SELECT
    0, [a].[Id], [b].[Id],
    'True', NEWID(),
    'Y', 'Y', 'Y', 'Y',
    1, 1
FROM [dbo].[Block] AS [b]
INNER JOIN [dbo].[BlockType] AS [bt] ON [bt].[Id] = [b].[BlockTypeId]
INNER JOIN [dbo].[Attribute] AS [a] ON TRY_CAST(a.[EntityTypeQualifierValue] AS INT) = [bt].[Id]
WHERE a.[Key] = 'UseObsidianComponents'
  AND a.[EntityTypeId] = 9
  AND a.[EntityTypeQualifierColumn] = 'BlockTypeId'
  AND bt.[Guid] IN (
    'EB279DF9-D817-4905-B6AC-D9883F0DA2E4', -- Data View Detail
    'E431DBDF-5C65-45DC-ADC5-157A02045CCD', -- Report Detail
    'C7C069DB-9EEE-4245-9DF2-34E3A1FF4CCB' -- Dynamic Report
  )
  AND [b].[Id] NOT IN (SELECT [EntityId] FROM [dbo].[AttributeValue] AS [av] WHERE [av].[AttributeId] = [a].[Id])
" );
        }

        /// <summary>
        /// Down migration for "Color Picker Swatches" Defined Type updates.
        /// </summary>
        private void JMH_UpdateColorPickerSwatchesDefinedType_Down()
        {
            // Revert Custom defined values
            Sql( @"
UPDATE [DefinedValue] 
   SET [Value] = '#f44336'
 WHERE [Guid] = '69E71798-DF68-4ED7-93AC-CFE9915D664D' /* Red */
       AND [Value] = '#fb2c36'

UPDATE [DefinedValue] 
   SET [Value] = '#9c27b0'
 WHERE [Guid] = 'EDFF48D7-DE7C-455D-9B89-86614D3FA14D' /* Purple */
       AND [Value] = '#ad46ff'

UPDATE [DefinedValue] 
   SET [Value] = '#3f51b5'
 WHERE [Guid] = '0DA03527-0DBB-41D2-A3D1-DE60D10E4193' /* Indigo */
       AND [Value] = '#615fff'

UPDATE [DefinedValue] 
   SET [Value] = '#2196f3'
 WHERE [Guid] = 'D64BD9F7-5A0E-4709-9A0E-B8F88ED360F9' /* Blue */
       AND [Value] = '#2b7fff'

UPDATE [DefinedValue] 
   SET [Value] = '#4caf50' 
 WHERE [Guid] = 'B18A4333-A5BA-418A-9559-0CEB59770D33' /* Green */
       AND [Value] = '#00c950'

UPDATE [DefinedValue] 
   SET [Value] = '#ffeb3b' 
 WHERE [Guid] = '80051D45-6D75-4316-BD12-2EBCA226F0BC' /* Yellow */
       AND [Value] = '#f0b100'" );

            // Delete Palette Color defined values
            RockMigrationHelper.DeleteDefinedValue( "64BEB38D-294C-4524-A8D9-A09D07185390" ); // Black
            RockMigrationHelper.DeleteDefinedValue( "DAE5084D-1A9C-4D8C-B4FD-E1DE80D77314" ); // White
            RockMigrationHelper.DeleteDefinedValue( "05FF6690-7113-479F-967E-05ED5A07418E" ); // Secondary
            RockMigrationHelper.DeleteDefinedValue( "5D73F5EE-3257-40D7-92F9-6BCFB3BEB2A8" ); // Primary

            // Delete Display Location attribute
            RockMigrationHelper.DeleteAttribute( "2B3F5FD0-52C1-48BD-B633-FA565E886A74" );
        }

        /// <summary>
        /// JMH - Adds the Communication Entry Wizard customizable text block settings attributes and values.
        /// </summary>
        private void JMH_AddCommunicationEntryWizardCustomizeTextBlockSettings_Up()
        {

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Personal / Need-to-Know Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Personal / Need-to-Know Title", "IsNotBulkOptionTitle", "Personal / Need-to-Know Title", @"The title text to display for the Personal / Need-to-Know option.", 0, @"Personal / Need-to-Know", "1CF7EC34-1634-4ECF-8657-23EA216F8C85" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Personal / Need-to-Know Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Personal / Need-to-Know Description", "IsNotBulkOptionDescription", "Personal / Need-to-Know Description", @"The description text to display for the Personal / Need-to-Know option.", 1, @"Direct messages an individual expects or considers important or timely.", "3B90997A-494C-49B0-8D84-7B22458430B6" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Bulk / Marketing Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Bulk / Marketing Title", "IsBulkOptionTitle", "Bulk / Marketing Title", @"The title text to display for the Bulk / Marketing option.", 2, @"Bulk / Marketing", "B03C4051-9D90-4623-9009-FA9F6702EF02" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Bulk / Marketing Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Bulk / Marketing Description", "IsBulkOptionDescription", "Bulk / Marketing Description", @"The description text to display for the Bulk / Marketing option.", 3, @"Marketing messages sent to large groups; regulated by law, and misuse can cause fines and reputational harm.", "10B76346-8615-47B3-BBF6-C1EF8CC41CE6" );

            // Add Block Attribute Value
            //   Block: Communication Entry Wizard
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Block Location: Page=New Communication, Site=Rock RMS
            //   Attribute: Personal / Need-to-Know Title
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "EF53AAFB-EEA4-4354-BA1A-D430D465A913", "1CF7EC34-1634-4ECF-8657-23EA216F8C85", @"" );

            // Add Block Attribute Value
            //   Block: Communication Entry Wizard
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Block Location: Page=New Communication, Site=Rock RMS
            //   Attribute: Personal / Need-to-Know Description
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "EF53AAFB-EEA4-4354-BA1A-D430D465A913", "3B90997A-494C-49B0-8D84-7B22458430B6", @"" );

            // Add Block Attribute Value
            //   Block: Communication Entry Wizard
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Block Location: Page=New Communication, Site=Rock RMS
            //   Attribute: Bulk / Marketing Title
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "EF53AAFB-EEA4-4354-BA1A-D430D465A913", "B03C4051-9D90-4623-9009-FA9F6702EF02", @"" );

            // Add Block Attribute Value
            //   Block: Communication Entry Wizard
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Block Location: Page=New Communication, Site=Rock RMS
            //   Attribute: Bulk / Marketing Description
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "EF53AAFB-EEA4-4354-BA1A-D430D465A913", "10B76346-8615-47B3-BBF6-C1EF8CC41CE6", @"" );
        }

        /// <summary>
        /// JMH - Removes the Communication Entry Wizard customizable text block settings attributes and values.
        /// </summary>
        private void JMH_AddCommunicationEntryWizardCustomizeTextBlockSettings_Down()
        {
            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Bulk / Marketing Description
            RockMigrationHelper.DeleteAttribute( "10B76346-8615-47B3-BBF6-C1EF8CC41CE6" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Bulk / Marketing Title
            RockMigrationHelper.DeleteAttribute( "B03C4051-9D90-4623-9009-FA9F6702EF02" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Personal / Need-to-Know Description
            RockMigrationHelper.DeleteAttribute( "3B90997A-494C-49B0-8D84-7B22458430B6" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Personal / Need-to-Know Title
            RockMigrationHelper.DeleteAttribute( "1CF7EC34-1634-4ECF-8657-23EA216F8C85" );
        }

        /// <summary>
        /// Removes legacy Cloudflare Turnstile system setting attributes from the database.
        /// </summary>
        /// <remarks>This method deletes attributes related to the legacy Cloudflare Turnstile
        /// integration, specifically those with keys 'core_CaptchaSecretKey' and 'core_CaptchaSiteKey'. Use this method
        /// as part of a migration to clean up obsolete system settings.</remarks>
        private void NA_RemoveLegacyCloudflareTurnstileSystemSettings_Up()
        {
            Sql( $@"
                DELETE FROM [dbo].[Attribute]
                WHERE [FieldTypeId] = 1
                    AND [EntityTypeId] IS NULL
                    AND [EntityTypeQualifierColumn] = 'SystemSetting'
                    AND [Key] IN ('core_CaptchaSecretKey', 'core_CaptchaSiteKey');
            " );
        }

        /// <summary>
        /// Cleanups the migration history records except the last one.
        /// </summary>
        private void CleanupMigrationHistory()
        {
            Sql( @"
UPDATE [dbo].[__MigrationHistory]
SET [Model] = 0x
WHERE MigrationId < (SELECT TOP 1 MigrationId FROM __MigrationHistory ORDER BY MigrationId DESC)" );
        }
    }
}
