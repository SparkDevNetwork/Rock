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
    public partial class Rollup_20231101 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateNmiGatewayName();
            CleanStaleCodeGenEntries();
            UpdateSQLFunctionGetSpousePersonIdFromPersonIdUp();
            MarkTransactionEntryBlockAsLegacyUp();
            UpdateReminderNotificationSystemCommunicationBody();
            FixHomepageFeaturedLink();
            UpdateParentEntityForSignatureDocumentsBinaryFiles();
            SwapFinancialBatchListUp();
            ChopSecurityBlocksUp();
            UpdatePasswordlessLogin();
            JPH_AddObsidianGroupScheduleToolboxBlockTypeAndAttributesUp();
            JPH_SwapAndChopGroupScheduleToolboxBlocksUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateSQLFunctionGetSpousePersonIdFromPersonIdDown();
            MarkTransactionEntryBlockAsLegacyDown();
            SwapFinancialBatchListDown();
            ChopSecurityBlocksDown();
            JPH_SwapAndChopGroupScheduleToolboxBlocksDown();
            JPH_AddObsidianGroupScheduleToolboxBlockTypeAndAttributesDown();
        }

        /// <summary>
        /// SC: Migration to update NMI gateway name.
        /// </summary>
        private void UpdateNmiGatewayName()
        {
            Sql(
                @"DECLARE @EntityTypeId INT =
                (
	                SELECT	[Id]
	                FROM	[EntityType] 
	                WHERE
			                [Guid] = 'B8282486-7866-4ED5-9F24-093D25FF0820'
		                AND	[Name] = 'Rock.NMI.Gateway'
		                AND	[FriendlyName] = 'NMI Gateway'
                );

                IF (@EntityTypeId IS NOT NULL)
                BEGIN
	                UPDATE [EntityType] SET [FriendlyName] = 'Celero/TransNational NMI Gateway' WHERE [Id] = @EntityTypeId;
	                UPDATE [FinancialGateway] SET [Name] = 'Celero/TransNational NMI Gateway' WHERE [EntityTypeId] = @EntityTypeId;
                END" );
            ;
        }

        /// <summary>
        /// PA: Clean Stale Code Gen Entries
        /// </summary>
        private void CleanStaleCodeGenEntries()
        {
            Sql(
                @"-- SQL to remove stale block attributes of the above block (TO BE EXECUTED FIRST)
                DELETE [AttributeValue] FROM [AttributeValue]
                JOIN [Attribute] ON [Attribute].[Id] = [AttributeValue].[AttributeId]
                JOIN [Block] ON [Block].[Id] = [AttributeValue].[EntityId]
                JOIN [BlockType] ON [Block].[BlockTypeId] = [BlockType].[Id]
                    AND [Block].[Zone] = 'SectionB1' AND [Block].[Name] = 'Membership'
                    AND [Block].[IsSystem] = 1
                    AND [Attribute].[Guid] = 'EC43CF32-3BDF-4544-8B6A-CE9208DD7C81'
                WHERE [BlockType].[Guid] = 'D70A59DC-16BE-43BE-9880-59598FA7A94C'


                -- SQL to remove stale Membership blocks from Page: Extended Attributes V1.
                DELETE [dbo].[Block]
                FROM [dbo].[Block]
                JOIN [dbo].[BlockType] ON [dbo].[Block].[BlockTypeId] = [dbo].[BlockType].[Id]
                WHERE [dbo].[BlockType].[Guid] = 'D70A59DC-16BE-43BE-9880-59598FA7A94C'
                    AND [dbo].[Block].[Zone] = 'SectionB1' AND [dbo].[Block].[Name] = 'Membership'
                    AND [dbo].[Block].[PageId] IS NULL" );
        }

        /// <summary>
        /// PA: Update SQL Function GetSpousePersonIdFromPersonId to include Non Bible Strict Spouse
        /// </summary>
        private void UpdateSQLFunctionGetSpousePersonIdFromPersonIdUp() {
    Sql( @"/*
<doc>
    <summary>
        This function returns the most likely spouse for the person [Id] provided
    </summary>

    <returns>
        Person [Id] of the most likely spouse; otherwise returns NULL
    </returns>
    <remarks>


    </remarks>
    <code>
        SELECT [dbo].[ufnCrm_GetSpousePersonIdFromPersonId](5) -- Ted Decker (married)
        SELECT [dbo].[ufnCrm_GetSpousePersonIdFromPersonId](8) -- Ben Jones (single)
    </code>
</doc>
*/


ALTER FUNCTION [dbo].[ufnCrm_GetSpousePersonIdFromPersonId] (
    @PersonId INT
)
RETURNS INT
AS
BEGIN
    DECLARE @BibleStrictSpouse INT = (SELECT TOP (1) [Id] FROM [Attribute] WHERE [Key]= 'core_BibleStrictSpouse')
    IF (@BibleStrictSpouse = NULL)
        SET @BibleStrictSpouse = 0
    RETURN (
        SELECT
            TOP 1 S.[Id]
        FROM
            [Group] F
            INNER JOIN [GroupType] GT ON F.[GroupTypeId] = GT.[Id]
            INNER JOIN [GroupMember] FM ON FM.[GroupId] = F.[Id]
            INNER JOIN [Person] P ON P.[Id] = FM.[PersonId]
            INNER JOIN [GroupTypeRole] R ON R.[Id] = FM.[GroupRoleId]
            INNER JOIN [GroupMember] FM2 ON FM2.[GroupID] = F.[Id]
            INNER JOIN [Person] S ON S.[Id] = FM2.[PersonId]
            INNER JOIN [GroupTypeRole] R2 ON R2.[Id] = FM2.[GroupRoleId]
            CROSS APPLY (SELECT top 1 GroupOrder FROM GroupMember where GroupId = f.Id and PersonId = @PersonId) pgm
        WHERE
            GT.[Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E' -- Family
            AND P.[Id] = @PersonID
            AND R.[Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' -- Person must be an Adult
            AND R2.[Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' -- Potential spouse must be an Adult
            AND P.[MaritalStatusValueId] = 143 -- Person must be Married
            AND S.[MaritalStatusValueId] = 143 -- Potential spouse must be Married
            AND FM.[PersonId] != FM2.[PersonId] -- Cannot be married to yourself
            -- In the future, we may need to implement and check a GLOBAL Attribute ""BibleStrict"" with this logic:

            AND( @BibleStrictSpouse = 0 OR P.[Gender] != S.[Gender] OR P.[Gender] = 0 OR S.[Gender] = 0 )-- Genders cannot match if both are known

        ORDER BY
            isnull(pgm.GroupOrder, 9999),
            ABS( DATEDIFF( DAY, ISNULL( P.[BirthDate], '1/1/0001' ), ISNULL( S.[BirthDate], '1/1/0001' ) ) )-- If multiple results, choose nearest in age
            , S.[Id]-- Sort by Id so that the same result is always returned
    )

END" );
        }

        /// <summary>
        /// PA: Update SQL Function GetSpousePersonIdFromPersonId to include Non Bible Strict Spouse
        /// </summary>
        private void UpdateSQLFunctionGetSpousePersonIdFromPersonIdDown() {
            Sql( @"/*
<doc>
    <summary>
        This function returns the most likely spouse for the person [Id] provided
    </summary>

    <returns>
        Person [Id] of the most likely spouse; otherwise returns NULL
    </returns>
    <remarks>
    

    </remarks>
    <code>
        SELECT [dbo].[ufnCrm_GetSpousePersonIdFromPersonId](3) -- Ted Decker (married) 
        SELECT [dbo].[ufnCrm_GetSpousePersonIdFromPersonId](7) -- Ben Jones (single)
    </code>
</doc>
*/

ALTER FUNCTION [dbo].[ufnCrm_GetSpousePersonIdFromPersonId] ( 
    @PersonId INT 
) 
RETURNS INT 
AS
BEGIN

    RETURN (
        SELECT 
            TOP 1 S.[Id]
        FROM 
            [Group] F
            INNER JOIN [GroupType] GT ON F.[GroupTypeId] = GT.[Id]
            INNER JOIN [GroupMember] FM ON FM.[GroupId] = F.[Id]
            INNER JOIN [Person] P ON P.[Id] = FM.[PersonId]
            INNER JOIN [GroupTypeRole] R ON R.[Id] = FM.[GroupRoleId]
            INNER JOIN [GroupMember] FM2 ON FM2.[GroupID] = F.[Id]
            INNER JOIN [Person] S ON S.[Id] = FM2.[PersonId]
            INNER JOIN [GroupTypeRole] R2 ON R2.[Id] = FM2.[GroupRoleId]
            CROSS APPLY (SELECT top 1 GroupOrder FROM GroupMember where GroupId = f.Id and PersonId = @PersonId) pgm    
        WHERE 
            GT.[Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E' -- Family
            AND P.[Id] = @PersonID
            AND R.[Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' -- Person must be an Adult
            AND R2.[Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' -- Potential spouse must be an Adult
            AND P.[MaritalStatusValueId] = 143 -- Person must be Married
            AND S.[MaritalStatusValueId] = 143 -- Potential spouse must be Married
            AND FM.[PersonId] != FM2.[PersonId] -- Cannot be married to yourself
            -- In the future, we may need to implement and check a GLOBAL Attribute ""BibleStrict"" with this logic: 

            AND( P.[Gender] != S.[Gender] OR P.[Gender] = 0 OR S.[Gender] = 0 )-- Genders cannot match if both are known

        ORDER BY
            isnull(pgm.GroupOrder, 9999),
            ABS( DATEDIFF( DAY, ISNULL( P.[BirthDate], '1/1/0001' ), ISNULL( S.[BirthDate], '1/1/0001' ) ) )-- If multiple results, choose nearest in age
            , S.[Id]-- Sort by Id so that the same result is always returned
    )

END" );
        }

        /// <summary>
        /// KA: Migration to mark TransactionEntry as legacy
        /// </summary>
        private void MarkTransactionEntryBlockAsLegacyUp()
        {
            Sql( @"
                UPDATE BlockType SET [Name] = 'Transaction Entry (Legacy)',
                [Description] = 'Creates a new financial transaction or scheduled transaction. This block has been replaced with the Utility Payment Entry block.'
                WHERE [Guid] = '74EE3481-3E5A-4971-A02E-D463ABB45591'" );
        }
        
        /// <summary>
        /// KA: Migration to mark TransactionEntry as legacy
        /// </summary>
        private void MarkTransactionEntryBlockAsLegacyDown()
        {
            Sql( @"
                UPDATE BlockType SET [Name] = 'Transaction Entry',
                [Description] = 'Creates a new financial transaction or scheduled transaction.'
                WHERE [Guid] = '74EE3481-3E5A-4971-A02E-D463ABB45591'" );
        }

        /// <summary>
        /// PA: Update the Body of Reminder Notification System Communication
        /// </summary>
        private void UpdateReminderNotificationSystemCommunicationBody()
        {
            Sql( $@"
                UPDATE [dbo].[SystemCommunication]
                SET [Body] = REPLACE([Body], 'Below are {{ MaxRemindersPerEntityType }} of the most recent reminders', 'Below are the most recent reminders')
                WHERE [Guid] = '7899958C-BC2F-499E-A5CC-11DE1EF8DF20'" );
        }

        /// <summary>
        /// GJ: Fix Homepage Featured Link
        /// </summary>
        private void FixHomepageFeaturedLink()
        {
            Sql( @"
                UPDATE [AttributeValue]
                SET [Value] = REPLACE(REPLACE([Value], '{% assign featureLink = Item | Attribute:''FeatureLink'' -%}', '{% assign featureLink = Item | Attribute:''FeatureLink'',''RawValue'' %}'), '<a class=""btn btn-xs btn-link"" href=""{{ featureLink }}"">More Info</a>', '<a class=""btn btn-xs btn-link p-0"" href=""{{ featureLink | Remove:''https://'' | Remove:''http://'' | Prepend:''https://'' }}"">More Info</a>')
                WHERE [Guid] = '1E2F7914-FD32-4B86-AE08-987408F09DCD'" );
        }

        /// <summary>
        /// PA: Update Parent Entity for Signature Documents Binary Files
        /// </summary>
        private void UpdateParentEntityForSignatureDocumentsBinaryFiles()
        {
            Sql( @"
                DECLARE @SignatureDocumentTemplateEntityTypeId INT = (SELECT [Id] FROM [dbo].[EntityType] WHERE [Guid] = '3F9828CC-8224-4AB0-98A5-6D60001EBE32');

                UPDATE 
	                [dbo].[BinaryFile] 
                SET 
	                [ParentEntityId] = [dbo].[SignatureDocument].[SignatureDocumentTemplateId],
	                [ParentEntityTypeId] = @SignatureDocumentTemplateEntityTypeId 
                FROM 
	                [dbo].[BinaryFile] 
                JOIN 
	                [dbo].[SignatureDocument] 
                ON 
	                [dbo].[BinaryFile].[Id] = [dbo].[SignatureDocument].[BinaryFileId]" );
        }

        /// <summary>
        /// PA: Swap Financial Batch List webforms block with obsidian Block
        /// </summary>
        private void SwapFinancialBatchListUp()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
#pragma warning restore CS0618 // Type or member is obsolete
                "Swap Financial Batch List",
                blockTypeReplacements: new Dictionary<string, string> {
                    { "AB345CE7-5DC6-41AF-BBDC-8D23D52AFE25", "F1950524-E959-440F-9CF6-1A8B9B7527D8" }, // Financial Batch List
                },
                migrationStrategy: "Swap",
                jobGuid: "7750ECFD-26E3-49DE-8E90-1B1A6DCCC3FE" );

            Sql( $"UPDATE [BlockType] SET [Name] = 'Batch List (Legacy)' WHERE [Guid] = 'AB345CE7-5DC6-41AF-BBDC-8D23D52AFE25'" );
        }

        /// <summary>
        /// PA: Swap Financial Batch List webforms block with obsidian Block
        /// </summary>
        private void SwapFinancialBatchListDown()
        {
            RockMigrationHelper.DeleteByGuid( "7750ECFD-26E3-49DE-8E90-1B1A6DCCC3FE", "ServiceJob" );
            Sql( $"UPDATE [BlockType] SET [Name] = 'Batch List' WHERE [Guid] = 'AB345CE7-5DC6-41AF-BBDC-8D23D52AFE25'" );
        }
    
        /// <summary>
        /// JMH: Chop Block Type
        /// </summary>
        private void ChopSecurityBlocksUp()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
#pragma warning restore CS0618 // Type or member is obsolete
                "Chop AccountEntry and Login",
                blockTypeReplacements: new Dictionary<string, string> {
                    { "99362B60-71A5-44C6-BCFE-DDA9B00CC7F3", "E5C34503-DDAD-4881-8463-0E1E20B1675D" }, // Account Entry
                    { "7B83D513-1178-429E-93FF-E76430E038E4", "5437C991-536D-4D9C-BE58-CBDB59D1BBB3" }, // Login
                },
                migrationStrategy: "Chop",
                jobGuid: "A65D26C1-229E-4198-B388-E269C3534BC0" );
        }

        /// <summary>
        /// JMH: Chop Block Types
        /// </summary>
        private void ChopSecurityBlocksDown()
        {
            // Delete the Service Job Entity
            RockMigrationHelper.DeleteByGuid( "A65D26C1-229E-4198-B388-E269C3534BC0", "ServiceJob" );
        }

        /// <summary>
        /// GJ: System Communication Update
        /// </summary>
        private void UpdatePasswordlessLogin()
        {
            Sql( @"
                UPDATE
                    [SystemCommunication]
                SET
                    [Body] = N'{{ ''Global'' | Attribute:''EmailHeader'' }}

<h1>Verify Your Email to Complete Your Sign-In</h1>

<p>We have received a {% if IsNewPerson %}account creation request{% else %}sign-in attempt{% endif %} for a {{ ''Global'' | Attribute:''OrganizationName'' }} digital platform. Click the button below to confirm your sign-in. This link will expire in {{ LinkExpiration }}.</p>

<p><!--[if mso]>
<v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ Link }}"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""11%"" strokecolor=""#269abc"" fillcolor=""#31b0d5"">
<w:anchorlock/>
<center style=""color:#ffffff;font-family:sans-serif;font-size:14px;font-weight:normal;"">Complete Sign-In</center>
</v:roundrect>
<![endif]-->
<a href=""{{ Link }}"" style=""background-color:#31b0d5;border:1px solid #269abc;border-radius:4px;color:#ffffff !important;display:inline-block;font-family:sans-serif;font-size:14px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"">Complete Sign-In</a>
</p>

<p>If you have trouble with the button above please use the link below:</p>
<p>
<a href=""{{ Link }}"">{{ Link }}</a>
</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}'
                WHERE
                    [Guid] = 'A7AD9FD5-A343-4ADA-868D-A3528D650143'
                    AND [ModifiedDateTime] IS NULL" );
        }

        /// <summary>
        /// JPH: Add Obsidian Group Schedule Toolbox block type and attributes up.
        /// </summary>
        private void JPH_AddObsidianGroupScheduleToolboxBlockTypeAndAttributesUp()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Group.Scheduling.GroupScheduleToolbox
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Group.Scheduling.GroupScheduleToolbox", "Group Schedule Toolbox", "Rock.Blocks.Group.Scheduling.GroupScheduleToolbox, Rock.Blocks, Version=1.16.1.11, Culture=neutral, PublicKeyToken=null", false, false, "FDADA51C-C7E6-4ECA-A984-646B42FBFC40" );

            // Add/Update Obsidian Block Type
            //   Name:Group Schedule Toolbox
            //   Category:Group Scheduling
            //   EntityType:Rock.Blocks.Group.Scheduling.GroupScheduleToolbox
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Group Schedule Toolbox", "Allows management of group scheduling for a specific person (worker).", "Rock.Blocks.Group.Scheduling.GroupScheduleToolbox", "Group Scheduling", "6554ADE3-2FC8-482B-BA63-2C3EABC11D32" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Enable Additional Time Sign-Up
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Additional Time Sign-Up", "EnableAdditionalTimeSignUp", "Enable Additional Time Sign-Up", @"When enabled, a button will allow the individual to sign up for upcoming schedules for their group.", 0, @"True", "2F24C3AC-461A-4AF2-A733-A609E0AE9C73" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Additional Time Sign-Up Button Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Additional Time Sign-Up Button Text", "AdditionalTimeSignUpButtonText", "Additional Time Sign-Up Button Text", @"The text to display for the Additional Time Sign-Up button.", 1, @"Sign Up for Additional Times", "EDDD5B92-E5A2-4CCA-AE81-DF3229EF609C" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Additional Time Sign-Up Header
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Additional Time Sign-Up Header", "SignupforAdditionalTimesHeader", "Additional Time Sign-Up Header", @"Header content to show above the Additional Time Sign-Up panel. <span class='tip tip-lava'></span>", 2, @"", "5C6CAAAA-5CA9-4DEB-80E5-92D09E07B5E8" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Date Range
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Date Range", "FutureWeekDateRange", "Date Range", @"The date range to allow individuals to sign up for a schedule. Please note that only current and future dates will be accepted. Schedules that have already started will never be displayed.", 3, @"Next|6|Week||", "AE77BD8D-147D-46C7-8370-9DD147CD57F4" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Cutoff Time (Hours)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cutoff Time (Hours)", "AdditionalTimeSignUpCutoffTime", "Cutoff Time (Hours)", @"Set the cutoff time in hours for hiding schedules that are too close to their start time. Schedules within this cutoff window will not be displayed for sign-up. Schedules that have already started will never be displayed.", 4, @"12", "F4A7DECE-F034-4F2B-91C4-C1AA03F7E382" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Require Location for Additional Time Sign-Up
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Location for Additional Time Sign-Up", "RequireLocationForAdditionalSignups", "Require Location for Additional Time Sign-Up", @"When enabled, a location will be required when signing up for additional times.", 5, @"False", "7E67EAA5-641A-4D55-9436-7841DB84CB5C" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Additional Time Sign-Up Schedule Exclusions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "EC6A5CAF-F6A2-47A4-9CBA-6E1C53D7E59B", "Additional Time Sign-Up Schedule Exclusions", "AdditionalTimeSignUpScheduleExclusions", "Additional Time Sign-Up Schedule Exclusions", @"Select named schedules that you would like to exclude from all groups on the Additional Time Sign-Up panel.", 6, @"", "A6BB5229-63F9-4940-B9A8-E8C1A8DBF54F" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Enable Immediate Needs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Immediate Needs", "EnableImmediateNeeds", "Enable Immediate Needs", @"When enabled, upcoming opportunities that still need individuals will be highlighted.", 7, @"False", "A24D633D-A93F-4811-B0ED-601F257E8C4D" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Immediate Need Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Immediate Need Title", "ImmediateNeedTitle", "Immediate Need Title", @"The title to use for the Immediate Need panel.", 8, @"Immediate Needs", "9419F50C-B027-49B7-A959-966B4B176065" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Immediate Need Introduction
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Immediate Need Introduction", "ImmediateNeedIntroduction", "Immediate Need Introduction", @"The introductory text to show above the Immediate Need panel.", 9, @"This group has an immediate need for volunteers. If you're able to assist we would greatly appreciate your help.", "4F00EFD1-B49F-4D1E-B1A5-90EA7E548B43" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Immediate Need Window (Hours)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Immediate Need Window (Hours)", "ImmediateNeedWindow", "Immediate Need Window (Hours)", @"The hour range to determine which schedules are in the immediate window. This works with the cutoff setting so ensure that you reduce the cutoff setting to include schedules you will want shown in the Immediate Need panel.", 10, @"0", "5971D51D-DD6D-46D3-8B7D-1C5745AA66E1" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Current Schedule Button Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Current Schedule Button Text", "CurrentScheduleButtonText", "Current Schedule Button Text", @"The text to display for the Current Schedule button.", 0, @"Current Schedule", "6374FED9-0021-4C92-86C1-1AF8963723E3" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Current Schedule Header
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Current Schedule Header", "CurrentScheduleHeader", "Current Schedule Header", @"Header content to show above the Current Schedule panel. <span class='tip tip-lava'></span>", 1, @"", "607953EC-9A9B-4FC0-8119-7851BCF35C27" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Scheduler Receive Confirmation Emails
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Scheduler Receive Confirmation Emails", "SchedulerReceiveConfirmationEmails", "Scheduler Receive Confirmation Emails", @"When enabled, the scheduler will receive an email for each confirmation or decline. Note that if a Group's ""Schedule Cancellation Person to Notify"" is defined, that person will automatically receive an email for schedules that are declined or cancelled, regardless of this setting.", 2, @"False", "BF2C31AA-A97B-43C4-886D-67654B3A979A" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Scheduling Response Email
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "Scheduling Response Email", "SchedulingResponseEmail", "Scheduling Response Email", @"The system communication that will be used for sending emails to the scheduler for each confirmation or decline. If a Group's ""Schedule Cancellation Person to Notify"" is defined, this system communication will also be used to send those emails for schedules that are declined or cancelled.", 3, @"D095F78D-A5CF-4EF6-A038-C7B07E250611", "DA96A6CD-D853-48DA-9863-30EF68B75401" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Decline Reason Note
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Decline Reason Note", "DeclineReasonNote", "Decline Reason Note", @"Controls whether a note will be shown for the person to elaborate on why they cannot attend. A schedule's Group Type must also require a decline reason for this setting to have any effect.", 4, @"hide", "EFEA542E-6103-44BE-AFB8-17F1BA3DD3AE" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Enable Update Schedule Preferences
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Update Schedule Preferences", "EnableUpdateSchedulePreferences", "Enable Update Schedule Preferences", @"When enabled, a button will allow the individual to set their group reminder preferences and preferred schedule.", 0, @"True", "B9D046B3-71E2-4683-A209-8AC6E587F592" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Update Schedule Preferences Button Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Update Schedule Preferences Button Text", "UpdateSchedulePreferencesButtonText", "Update Schedule Preferences Button Text", @"The text to display for the Update Schedule Preferences button.", 1, @"Update Schedule Preferences", "BF2736D3-A158-4071-9313-C61F4CD8D645" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Update Schedule Preferences Header
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Update Schedule Preferences Header", "UpdateSchedulePreferencesHeader", "Update Schedule Preferences Header", @"Header content to show above the Update Schedule Preferences panel. <span class='tip tip-lava'></span>", 2, @"", "B8727222-F857-4AED-874D-43C6932206BD" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Enable Schedule Unavailability
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Schedule Unavailability", "EnableScheduleUnavailability", "Enable Schedule Unavailability", @"When enabled, a button will allow the individual to specify dates or date ranges when they will be unavailable to serve.", 0, @"True", "35B5BF8B-307E-4D2B-A4AF-DBE51DBD3A26" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Schedule Unavailability Button Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Schedule Unavailability Button Text", "ScheduleUnavailabilityButtonText", "Schedule Unavailability Button Text", @"The text to display for the Schedule Unavailability button.", 1, @"Schedule Unavailability", "F985115C-E0C4-422A-89F9-1F850963D199" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Schedule Unavailability Header
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Schedule Unavailability Header", "ScheduleUnavailabilityHeader", "Schedule Unavailability Header", @"Header content to show above the Schedule Unavailability panel. <span class='tip tip-lava'></span>", 2, @"", "794DC298-719A-4003-8FCE-FCAE0EE2C4B5" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Action Header Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Action Header Lava Template", "ActionHeaderLavaTemplate", "Action Header Lava Template", @"Header content to show above the action buttons. <span class='tip tip-lava'></span>", 0, @"<h4>Actions</h4>", "E49211AE-9520-4108-9476-68E91D27FBB8" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Override Hide from Toolbox
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Override Hide from Toolbox", "OverrideHideFromToolbox", "Override Hide from Toolbox", @"When enabled this setting will show all schedule enabled groups no matter what their ""Disable Schedule Toolbox Access"" setting is set to.", 1, @"False", "880373F4-BD70-44CC-9821-B8FC9E69FD71" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Include Group Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Include Group Types", "IncludeGroupTypes", "Include Group Types", @"The group types to display in the list. If none are selected, all group types will be included.", 2, @"", "0666A1D5-0ED4-4899-88E2-9F7A7B029C09" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Exclude Group Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Exclude Group Types", "ExcludeGroupTypes", "Exclude Group Types", @"The group types to exclude from the list (only valid if including all groups).", 3, @"", "C159F464-03EE-47C7-9796-DF7C7B8DCD56" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Show Campus on Tabs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Show Campus on Tabs", "ShowCampusOnTabs", "Show Campus on Tabs", @"Optionally shows the group's campus on the tabs.", 4, @"never", "56A31EA7-2F98-4637-8A16-178706F7EB1E" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Schedule List Format
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Schedule List Format", "ScheduleListFormat", "Schedule List Format", @"The format to be used when displaying schedules for schedule preferences and additional time sign-ups.", 5, @"1", "AE327AC7-65A4-470A-94CC-9DB2DAAEB89A" );
        }

        /// <summary>
        /// JPH: Swap Group Schedule Toolbox (v1) and Chop Group Schedule Toolbox v2 Up.
        /// </summary>
        private void JPH_SwapAndChopGroupScheduleToolboxBlocksUp()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
#pragma warning restore CS0618 // Type or member is obsolete
                "Group Schedule Toolbox",
                blockTypeReplacements: new Dictionary<string, string>
                {
                    { "7F9CEA6F-DCE5-4F60-A551-924965289F1D", "6554ADE3-2FC8-482B-BA63-2C3EABC11D32" } // Group Schedule Toolbox (v1)
                },
                migrationStrategy: "Swap",
                jobGuid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_161_SWAP_BLOCK_GROUP_SCHEDULE_TOOLBOX_V1 );

            Sql( "UPDATE [BlockType] SET [Name] = 'Group Schedule Toolbox (Legacy)' WHERE [Guid] = '7F9CEA6F-DCE5-4F60-A551-924965289F1D';" );

#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
#pragma warning restore CS0618 // Type or member is obsolete
                "Group Schedule Toolbox v2",
                blockTypeReplacements: new Dictionary<string, string>
                {
                    { "18A6DCE3-376C-4A62-B1DD-5E5177C11595", "6554ADE3-2FC8-482B-BA63-2C3EABC11D32" } // Group Schedule Toolbox v2
                },
                migrationStrategy: "Chop",
                jobGuid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_161_CHOP_BLOCK_GROUP_SCHEDULE_TOOLBOX_V2 );

            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v16.1 - Remove Obsidian Group Schedule Toolbox Back Buttons",
                description: "This job removes the Back buttons from 3 Lava block settings within the Obsidian Group Schedule Toolbox block.",
                jobType: "Rock.Jobs.PostUpdateJobs.PostV161RemoveObsidianGroupScheduleToolboxBackButtons",
                cronExpression: "0 0 21 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_161_REMOVE_OBSIDIAN_GROUP_SCHEDULE_TOOLBOX_BACK_BUTTONS );
        }

        /// <summary>
        /// JPH: Swap Group Schedule Toolbox (V1) and Chop Group Schedule Toolbox v2 Down.
        /// </summary>
        private void JPH_SwapAndChopGroupScheduleToolboxBlocksDown()
        {
            // Reset the v1 toolbox name.
            Sql( "UPDATE [BlockType] SET [Name] = 'Group Schedule Toolbox' WHERE [Guid] = '7F9CEA6F-DCE5-4F60-A551-924965289F1D';" );

            // Delete the Service Job records.
            RockMigrationHelper.DeleteByGuid( Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_161_SWAP_BLOCK_GROUP_SCHEDULE_TOOLBOX_V1, "ServiceJob" );
            RockMigrationHelper.DeleteByGuid( Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_161_CHOP_BLOCK_GROUP_SCHEDULE_TOOLBOX_V2, "ServiceJob" );
            RockMigrationHelper.DeleteByGuid( Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_161_REMOVE_OBSIDIAN_GROUP_SCHEDULE_TOOLBOX_BACK_BUTTONS, "ServiceJob" );
        }

        /// <summary>
        /// JPH: Add Obsidian Group Schedule Toolbox block type and attributes down.
        /// </summary>
        private void JPH_AddObsidianGroupScheduleToolboxBlockTypeAndAttributesDown()
        {
            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Schedule List Format
            RockMigrationHelper.DeleteAttribute( "AE327AC7-65A4-470A-94CC-9DB2DAAEB89A" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Show Campus on Tabs
            RockMigrationHelper.DeleteAttribute( "56A31EA7-2F98-4637-8A16-178706F7EB1E" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Exclude Group Types
            RockMigrationHelper.DeleteAttribute( "C159F464-03EE-47C7-9796-DF7C7B8DCD56" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Include Group Types
            RockMigrationHelper.DeleteAttribute( "0666A1D5-0ED4-4899-88E2-9F7A7B029C09" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Override Hide from Toolbox
            RockMigrationHelper.DeleteAttribute( "880373F4-BD70-44CC-9821-B8FC9E69FD71" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Action Header Lava Template
            RockMigrationHelper.DeleteAttribute( "E49211AE-9520-4108-9476-68E91D27FBB8" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Schedule Unavailability Header
            RockMigrationHelper.DeleteAttribute( "794DC298-719A-4003-8FCE-FCAE0EE2C4B5" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Schedule Unavailability Button Text
            RockMigrationHelper.DeleteAttribute( "F985115C-E0C4-422A-89F9-1F850963D199" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Enable Schedule Unavailability
            RockMigrationHelper.DeleteAttribute( "35B5BF8B-307E-4D2B-A4AF-DBE51DBD3A26" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Update Schedule Preferences Header
            RockMigrationHelper.DeleteAttribute( "B8727222-F857-4AED-874D-43C6932206BD" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Update Schedule Preferences Button Text
            RockMigrationHelper.DeleteAttribute( "BF2736D3-A158-4071-9313-C61F4CD8D645" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Enable Update Schedule Preferences
            RockMigrationHelper.DeleteAttribute( "B9D046B3-71E2-4683-A209-8AC6E587F592" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Decline Reason Note
            RockMigrationHelper.DeleteAttribute( "EFEA542E-6103-44BE-AFB8-17F1BA3DD3AE" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Scheduling Response Email
            RockMigrationHelper.DeleteAttribute( "DA96A6CD-D853-48DA-9863-30EF68B75401" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Scheduler Receive Confirmation Emails
            RockMigrationHelper.DeleteAttribute( "BF2C31AA-A97B-43C4-886D-67654B3A979A" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Current Schedule Header
            RockMigrationHelper.DeleteAttribute( "607953EC-9A9B-4FC0-8119-7851BCF35C27" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Current Schedule Button Text
            RockMigrationHelper.DeleteAttribute( "6374FED9-0021-4C92-86C1-1AF8963723E3" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Immediate Need Window (Hours)
            RockMigrationHelper.DeleteAttribute( "5971D51D-DD6D-46D3-8B7D-1C5745AA66E1" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Immediate Need Introduction
            RockMigrationHelper.DeleteAttribute( "4F00EFD1-B49F-4D1E-B1A5-90EA7E548B43" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Immediate Need Title
            RockMigrationHelper.DeleteAttribute( "9419F50C-B027-49B7-A959-966B4B176065" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Enable Immediate Needs
            RockMigrationHelper.DeleteAttribute( "A24D633D-A93F-4811-B0ED-601F257E8C4D" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Additional Time Sign-Up Schedule Exclusions
            RockMigrationHelper.DeleteAttribute( "A6BB5229-63F9-4940-B9A8-E8C1A8DBF54F" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Require Location for Additional Time Sign-Up
            RockMigrationHelper.DeleteAttribute( "7E67EAA5-641A-4D55-9436-7841DB84CB5C" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Cutoff Time (Hours)
            RockMigrationHelper.DeleteAttribute( "F4A7DECE-F034-4F2B-91C4-C1AA03F7E382" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Date Range
            RockMigrationHelper.DeleteAttribute( "AE77BD8D-147D-46C7-8370-9DD147CD57F4" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Additional Time Sign-Up Header
            RockMigrationHelper.DeleteAttribute( "5C6CAAAA-5CA9-4DEB-80E5-92D09E07B5E8" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Additional Time Sign-Up Button Text
            RockMigrationHelper.DeleteAttribute( "EDDD5B92-E5A2-4CCA-AE81-DF3229EF609C" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Enable Additional Time Sign-Up
            RockMigrationHelper.DeleteAttribute( "2F24C3AC-461A-4AF2-A733-A609E0AE9C73" );

            // Delete BlockType
            //   Name: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Path: -
            //   EntityType: Group Schedule Toolbox
            RockMigrationHelper.DeleteBlockType( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32" );

            // Delete Entity Type
            //   EntityType:Rock.Blocks.Group.Scheduling.GroupScheduleToolbox
            RockMigrationHelper.DeleteEntityType( "FDADA51C-C7E6-4ECA-A984-646B42FBFC40" );
        }
    }
}
