// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    using Rock.Security;

    /// <summary>
    ///
    /// </summary>
    public partial class PersonMergeRequests : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // MP: Person Merge Request
            RockMigrationHelper.AddPage( "84FD84DF-F58B-4B9D-A407-96276C40AB7E", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Merge Requests", "Lists person merge requests", "5180AE8E-BF1C-444F-A154-14E5A8A4ACC9", "fa fa-files-o" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Person Merge Request List", "Lists Perge Merge Requests", "~/Blocks/Crm/PersonMergeRequestList.ascx", "CRM", "4CBFB5FC-0174-489A-8B95-90BB8FAA2144" );

            // Add Block to Page: Merge Requests, Site: Rock RMS
            RockMigrationHelper.AddBlock( "5180AE8E-BF1C-444F-A154-14E5A8A4ACC9", "", "4CBFB5FC-0174-489A-8B95-90BB8FAA2144", "Person Merge Request List", "Main", "", "", 0, "566A2CE5-1439-4BA5-8A3A-11BF457AD4ED" );


            // MP: fix [Layout] not having a unique constaint on [Guid]
            Sql( @"WITH cteDups
AS (
    SELECT [Guid]
        ,[FirstId]
    FROM (
        SELECT [Guid]
            ,min(Id) [FirstId]
            ,Count(*) dupcount
        FROM [Layout]
        GROUP BY [Guid]
        ) a
    WHERE dupcount > 1
    )
DELETE
FROM Layout
WHERE [Guid] IN (
        SELECT [Guid]
        FROM cteDups
        )
    AND [Id] NOT IN (
        SELECT [FirstID]
        FROM cteDups
        )

IF NOT EXISTS (
        SELECT *
        FROM sys.indexes
        WHERE NAME = N'IX_Guid'
            AND Object_ID = Object_ID(N'Layout')
        )
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Guid] ON [dbo].[Layout] ([Guid] ASC)
END" );

            /* MP: register blocktypes, etc that have been added by Rock but not in a migration yet   */
            RockMigrationHelper.AddLayout( "F3F82256-2D66-432B-9D67-3552CD2F4C2B", "Blank", "Blank", "", "55E19934-762D-48E5-BD07-ACB1249ACBDC" ); // Site:External Website
            RockMigrationHelper.AddLayout( "A5FA7C3C-A238-4E0B-95DE-B540144321EC", "Blank", "Blank", "", "01158948-1DC3-4A92-B213-9E26BE194F68" ); // Site:Rock Check-in Manager
            RockMigrationHelper.AddLayout( "C2D29296-6A87-47A9-A753-EE4E9159C4C4", "Error", "Error", "", "DAFB37C9-0D61-424C-8788-EE0ACF7EA5FD" ); // Site:Rock RMS
            RockMigrationHelper.AddLayout( "C2D29296-6A87-47A9-A753-EE4E9159C4C4", "Homepage", "Homepage", "", "F0A9626B-0D87-4057-AD60-6F978E9EDA87" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Calendar Item List Lava", "Renders calendar items using Lava.", "~/Blocks/Event/EventItemListLava.ascx", "Event", "6DF11547-8757-4305-BC9A-122B9D929342" );
            RockMigrationHelper.UpdateBlockType( "Calendar Item Occurrence List Lava", "Block that takes a calendar item and displays occurrences for it using Lava.", "~/Blocks/Event/EventItemOccurrenceListLava.ascx", "Event", "3ABC7007-CE3E-4092-900F-C907948CA8C2" );
            RockMigrationHelper.UpdateBlockType( "Developer Environment Info", "Shows Information about the Development environment", "~/Blocks/Examples/DevelopEnvironmentInfo.ascx", "Examples", "03BFBFCA-36C4-480D-A10B-3CF349F4A6EA" );
            RockMigrationHelper.UpdateBlockType( "Model Map", "Displays details about each model classes in Rock.Model.", "~/Blocks/Examples/ModelMap.ascx", "Examples", "DA2AAD13-209B-4885-8739-B7BE99F6510D" );

            // Attrib for BlockType: Change Password:Change Password Not Supported Caption
            RockMigrationHelper.AddBlockTypeAttribute( "3C12DE99-2D1B-40F2-A9B8-6FE7C2524B37", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Change Password Not Supported Caption", "ChangePasswordNotSupportedCaption", "", "", 2, @"Changing your password is not supported.", "A6915BB2-590A-450E-9B02-3C03B7462DA8" );

            // Attrib for BlockType: Group Registration:Register Button Alt Text
            RockMigrationHelper.AddBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Register Button Alt Text", "RegisterButtonAltText", "", "Alternate text to use for the Register button (default is 'Register').", 10, @"", "19D5EB3B-9921-440E-9075-6617203FEE39" );

            // Attrib for BlockType: Connection Opportunity Search:Display Campus Filter
            RockMigrationHelper.AddBlockTypeAttribute( "C0D58DEE-D266-4AA8-8750-414A3CC26C07", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Campus Filter", "DisplayCampusFilter", "", "Display the campus filter", 0, @"True", "085C5012-B8A1-4ABA-BC69-4ED2BEE03FA6" );

            // Attrib for BlockType: Group Finder:Show Description
            RockMigrationHelper.AddBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Description", "ShowDescription", "", "", 0, @"True", "25678A31-0618-49B0-9942-CEC629B1EDBB" );

            // Attrib for BlockType: Calendar Lava:Show Date Range Filter
            RockMigrationHelper.AddBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Date Range Filter", "ShowDateRangeFilter", "", "Determines whether the date range filters are shown", 6, @"False", "D6AC8CFA-A81C-4E70-A96E-5F1B8D529BC9" );

            // Attrib for BlockType: Calendar Lava:Enable Campus Context
            RockMigrationHelper.AddBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Campus Context", "EnableCampusContext", "", "If the page has a campus context it's value will be used as a filter", 11, @"False", "FE0FDF47-2045-41B8-B2D6-E455632CA7FD" );

            // Attrib for BlockType: Registration Entry:Display Progress Bar
            RockMigrationHelper.AddBlockTypeAttribute( "CABD2BFB-DFFF-42CD-BF1A-14F3BEE583DD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Progress Bar", "DisplayProgressBar", "", "Display a progress bar for the registration.", 4, @"True", "2661D3E0-97E3-4B3B-8B1B-4017C1310F93" );

            // Attrib for BlockType: Registration Entry:Enable Debug
            RockMigrationHelper.AddBlockTypeAttribute( "CABD2BFB-DFFF-42CD-BF1A-14F3BEE583DD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Display the merge fields that are available for lava ( Success Page ).", 5, @"False", "C29EA587-75D0-451E-9FFE-63E03A4D79C8" );

            // Attrib for BlockType: Exception List:Show Legend
            RockMigrationHelper.AddBlockTypeAttribute( "6302B319-9830-4BE3-A402-17801C88F7E4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Legend", "ShowLegend", "", "", 3, @"True", "43F96F2B-9E30-49F7-8739-A2AC5C994DF4" );

            // Attrib for BlockType: Group Registration:Auto Fill Form
            RockMigrationHelper.AddBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Auto Fill Form", "AutoFillForm", "", "If set to FALSE then the form will not load the context of the logged in user (default: 'True'.)", 9, @"true", "D9D915AB-D013-4285-9727-645A054C6832" );

            // Attrib for BlockType: Exception List:Legend Position
            RockMigrationHelper.AddBlockTypeAttribute( "6302B319-9830-4BE3-A402-17801C88F7E4", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Legend Position", "LegendPosition", "", "Select the position of the Legend (corner)", 4, @"ne", "973210C8-04E3-46A7-8D15-63BE0D4A84CC" );

            // Attrib for BlockType: Prayer Request Entry:Expires After (Days)
            RockMigrationHelper.AddBlockTypeAttribute( "4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Expires After (Days)", "ExpireDays", "", "Number of days until the request will expire (only applies when auto-approved is enabled).", 4, @"14", "6F704B38-C5BD-4C47-B14F-4A36488BDC29" );

            // Attrib for BlockType: Login:Redirect Page
            RockMigrationHelper.AddBlockTypeAttribute( "7B83D513-1178-429E-93FF-E76430E038E4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Redirect Page", "RedirectPage", "", "Page to redirect user to upon successful login. The 'returnurl' query string will always override this setting for database authenticated logins. Redirect Page Setting will override third-party authentication 'returnurl'.", 9, @"", "2E55DFD3-8544-4B97-B488-4CCA20363844" );

            // Attrib for BlockType: Account Entry:Record Status
            RockMigrationHelper.AddBlockTypeAttribute( "99362B60-71A5-44C6-BCFE-DDA9B00CC7F3", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "", "The record status to use for new individuals (default: 'Pending'.)", 0, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "5010D78F-8764-4EDB-ABFC-8DBED07A9C21" );

            // Attrib for BlockType: Account Entry:Connection Status
            RockMigrationHelper.AddBlockTypeAttribute( "99362B60-71A5-44C6-BCFE-DDA9B00CC7F3", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "", "The connection status to use for new individuals (default: 'Web Prospect'.)", 0, @"368DD475-242C-49C4-A42C-7278BE690CC2", "91960813-EB6A-499C-A140-A54FAC180056" );

            // Attrib for BlockType: Transaction Entry:Connection Status
            RockMigrationHelper.AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "", "The connection status to use for new individuals (default: 'Web Prospect'.)", 25, @"368DD475-242C-49C4-A42C-7278BE690CC2", "743F636C-0EBF-421F-A1DD-4C4848638F6F" );

            // Attrib for BlockType: Transaction Entry:Record Status
            RockMigrationHelper.AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "", "The record status to use for new individuals (default: 'Pending'.)", 26, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "36299C3A-53F5-4DC1-AB2C-89482554A305" );

            // Attrib for BlockType: Calendar Lava:Filter Categories
            RockMigrationHelper.AddBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Filter Categories", "FilterCategories", "", "Determines which categories should be displayed in the filter.", 5, @"", "F01D4B22-4319-4FD1-9B4D-443D29813DBF" );

            // Attrib for BlockType: Prayer Request Entry:Default Category
            RockMigrationHelper.AddBlockTypeAttribute( "4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Default Category", "DefaultCategory", "", "If categories are not being shown, choose a default category to use for all new prayer requests.", 2, @"4B2D88F5-6E45-4B4B-8776-11118C8E8269", "EC71B180-9DCC-4F36-AB0F-AF1881A7D9B0" );

            // Attrib for BlockType: Confirm Account:Password Reset Unavailable Caption
            RockMigrationHelper.AddBlockTypeAttribute( "734DFF21-7465-4E02-BFC3-D40F7A65FB60", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Password Reset Unavailable Caption", "PasswordResetUnavailableCaption", "", "", 6, @"This type of account does not allow passwords to be changed.  Please contact your system administrator for assistance changing your password.", "C27D13AE-0F51-4F94-B7A9-891D067ED73C" );

            // JE: Update following suggestion email to fix hard coded page id
            Sql( MigrationSQL._201509032145388_PersonMergeRequests );

            // NA: Add ADMINISTRATE action with the 'RSR - Connection Administration' role for the My Connection Opportunities block instance 
            RockMigrationHelper.AddSecurityAuthForBlock( "80710A2C-9B90-40AE-B887-B885AAA43538", 0, Authorization.ADMINISTRATE, true, Rock.SystemGuid.Group.GROUP_CONNECTION_ADMINISTRATORS, Model.SpecialRole.None, "B12DE04A-F5AA-4EFA-95A5-35FFE1C62CCB" );

            // DT: Add transaction detail page settings to Registration Instance & Registration blocks
            // Attrib for BlockType: Registration Instance Detail:Transaction Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "22B67EDB-6D13-4D29-B722-DF45367AA3CB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Transaction Detail Page", "TransactionDetailPage", "", "The page for viewing details about a payment", 6, @"", "A4754BCF-7259-45AC-834E-2E6EBCA9CCC0" );
            // Attrib for BlockType: Registration Detail:Transaction Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "A1C967B2-EEDA-416F-A53C-7BE46D6DA4E1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Transaction Detail Page", "TransactionDetailPage", "", "The page for viewing details about a payment", 6, @"", "9E53BDB7-BD53-4335-A862-20A3454C0CCF" );
            // Attrib Value for Block:Registration Instance Detail, Attribute:Transaction Detail Page Page: Registration Instance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5F44A3A8-500B-4C89-95CA-8C4246B53C3F", "A4754BCF-7259-45AC-834E-2E6EBCA9CCC0", @"b67e38cb-2ef1-43ea-863a-37daa1c7340f" );
            // Attrib Value for Block:Registration Detail, Attribute:Transaction Detail Page Page: Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "0E0AE4CB-E348-435E-A0CA-9E3B2FC6BA19", "9E53BDB7-BD53-4335-A862-20A3454C0CCF", @"b67e38cb-2ef1-43ea-863a-37daa1c7340f" );

            // DT: Add history category for registration
            RockMigrationHelper.UpdateCategory( "546D5F43-1184-47C9-8265-2D7BF4E1BCA5", "Event", "", "", "035CDEDA-7BB9-4E42-B7FD-E0B7487108E5", 0 );
            RockMigrationHelper.UpdateCategory( "546D5F43-1184-47C9-8265-2D7BF4E1BCA5", "Registration", "fa fa-group", "", "813DF1A5-ADBD-481C-AC1D-884F0FA7AE77", 0, "035CDEDA-7BB9-4E42-B7FD-E0B7487108E5" );

            Sql( @"
    DECLARE @UrlMaskAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '0C405062-72BB-4362-9738-90C9ED5ACDDE' )
    DECLARE @RegistrationCategoryId int = ( SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = '813DF1A5-ADBD-481C-AC1D-884F0FA7AE77' )
    DECLARE @RegistrationDetailPageId int = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = 'FC81099A-2F98-4EBA-AC5A-8300B2FE46C4' )
    IF @UrlMaskAttributeId IS NOT NULL AND @RegistrationCategoryId IS NOT NULL AND @RegistrationDetailPageId IS NOT NULL
    BEGIN

        DELETE [AttributeValue]
        WHERE [AttributeId] = @UrlMaskAttributeId
        AND [EntityId] = @RegistrationCategoryId

        INSERT INTO [AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Value],[Guid] )
        VALUES( 0, @UrlMaskAttributeId, @RegistrationCategoryId, '~/page/' + CAST(@RegistrationDetailPageId AS varchar) + '?RegistrationId={0}', NEWID())
    END
" );
            // DT: Registration Audit Page
            RockMigrationHelper.AddPage( "FC81099A-2F98-4EBA-AC5A-8300B2FE46C4", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Registration Audit Log", "", "747C1DAA-1E77-45CB-99C5-7F4D030F824E", "" ); // Site:Rock RMS
            // Add Block to Page: Registration Audit Log, Site: Rock RMS
            RockMigrationHelper.AddBlock( "747C1DAA-1E77-45CB-99C5-7F4D030F824E", "", "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0", "History Log", "Main", "", "", 0, "BF085310-4BC6-4BD5-9338-7A0533F61E4E" );
            // Attrib for BlockType: Registration Entry:Enable Debug
            // Attrib for BlockType: Registration Detail:Audit Page
            RockMigrationHelper.AddBlockTypeAttribute( "A1C967B2-EEDA-416F-A53C-7BE46D6DA4E1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Audit Page", "AuditPage", "", "Page used to display the history of changes to a registration.", 5, @"", "DAE22517-2B39-4CB5-9687-5EB4F4104628" );
            // Attrib Value for Block:Registration Detail, Attribute:Audit Page Page: Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "0E0AE4CB-E348-435E-A0CA-9E3B2FC6BA19", "DAE22517-2B39-4CB5-9687-5EB4F4104628", @"747c1daa-1e77-45cb-99c5-7f4d030f824e" );
            // Attrib Value for Block:History Log, Attribute:Entity Type Page: Registration Audit Log, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BF085310-4BC6-4BD5-9338-7A0533F61E4E", "8FB690EC-5299-46C5-8695-AAD23168E6E1", @"40e5f218-69c0-495d-9ff3-d81528f35c46" );
            // Add/Update PageContext for Page:Registration Audit Log, Entity: Rock.Model.Registration, Parameter: RegistrationId
            RockMigrationHelper.UpdatePageContext( "747C1DAA-1E77-45CB-99C5-7F4D030F824E", "Rock.Model.Registration", "RegistrationId", "2DC21259-C186-4DEF-A19A-7E763F7066E6" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // DT: Registration Audit Page
            // Attrib for BlockType: Registration Detail:Audit Page
            RockMigrationHelper.DeleteAttribute( "DAE22517-2B39-4CB5-9687-5EB4F4104628" );
            // Remove Block: History Log, from Page: Registration Audit Log, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "BF085310-4BC6-4BD5-9338-7A0533F61E4E" );
            RockMigrationHelper.DeletePage( "747C1DAA-1E77-45CB-99C5-7F4D030F824E" ); //  Page: Registration Audit Log, Layout: Full Width, Site: Rock RMS
            // Delete PageContext for Page:Registration Audit Log, Entity: Rock.Model.Registration, Parameter: RegistrationId
            RockMigrationHelper.DeletePageContext( "2DC21259-C186-4DEF-A19A-7E763F7066E6" );

            // DT: Add transaction detail page settings to Registration Instance & Registration blocks
            // Attrib for BlockType: Registration Detail:Transaction Detail Page
            RockMigrationHelper.DeleteAttribute( "9E53BDB7-BD53-4335-A862-20A3454C0CCF" );
            // Attrib for BlockType: Registration Instance Detail:Transaction Detail Page
            RockMigrationHelper.DeleteAttribute( "A4754BCF-7259-45AC-834E-2E6EBCA9CCC0" );

            // NA: Add ADMINISTRATE action with the 'RSR - Connection Administration' role for the My Connection Opportunities block instance
            RockMigrationHelper.DeleteSecurityAuth( "B12DE04A-F5AA-4EFA-95A5-35FFE1C62CCB" );

            // MP: Person Merge Request
            // Remove Block: Person Merge Request List, from Page: Merge Requests, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "566A2CE5-1439-4BA5-8A3A-11BF457AD4ED" );

            RockMigrationHelper.DeleteBlockType( "4CBFB5FC-0174-489A-8B95-90BB8FAA2144" ); // Person Merge Request List

            RockMigrationHelper.DeletePage( "5180AE8E-BF1C-444F-A154-14E5A8A4ACC9" ); //  Page: Merge Requests, Layout: Full Width, Site: Rock RMS

            /* un-register blocktypes, etc that have been added by Rock but not in a migration yet   */

            // Attrib for BlockType: Exception List:Show Legend
            RockMigrationHelper.DeleteAttribute( "43F96F2B-9E30-49F7-8739-A2AC5C994DF4" );
            // Attrib for BlockType: Exception List:Legend Position
            RockMigrationHelper.DeleteAttribute( "973210C8-04E3-46A7-8D15-63BE0D4A84CC" );
            // Attrib for BlockType: Registration Entry:Enable Debug
            RockMigrationHelper.DeleteAttribute( "C29EA587-75D0-451E-9FFE-63E03A4D79C8" );
            // Attrib for BlockType: Registration Entry:Display Progress Bar
            RockMigrationHelper.DeleteAttribute( "2661D3E0-97E3-4B3B-8B1B-4017C1310F93" );
            // Attrib for BlockType: Calendar Lava:Enable Campus Context
            RockMigrationHelper.DeleteAttribute( "FE0FDF47-2045-41B8-B2D6-E455632CA7FD" );
            // Attrib for BlockType: Calendar Lava:Show Date Range Filter
            RockMigrationHelper.DeleteAttribute( "D6AC8CFA-A81C-4E70-A96E-5F1B8D529BC9" );
            // Attrib for BlockType: Calendar Lava:Filter Categories
            RockMigrationHelper.DeleteAttribute( "F01D4B22-4319-4FD1-9B4D-443D29813DBF" );
            // Attrib for BlockType: Group Registration:Auto Fill Form
            RockMigrationHelper.DeleteAttribute( "D9D915AB-D013-4285-9727-645A054C6832" );
            // Attrib for BlockType: Group Registration:Register Button Alt Text
            RockMigrationHelper.DeleteAttribute( "19D5EB3B-9921-440E-9075-6617203FEE39" );
            // Attrib for BlockType: Group Finder:Show Description
            RockMigrationHelper.DeleteAttribute( "25678A31-0618-49B0-9942-CEC629B1EDBB" );
            // Attrib for BlockType: Connection Opportunity Search:Display Campus Filter
            RockMigrationHelper.DeleteAttribute( "085C5012-B8A1-4ABA-BC69-4ED2BEE03FA6" );
            // Attrib for BlockType: Prayer Request Entry:Expires After (Days)
            RockMigrationHelper.DeleteAttribute( "6F704B38-C5BD-4C47-B14F-4A36488BDC29" );
            // Attrib for BlockType: Prayer Request Entry:Default Category
            RockMigrationHelper.DeleteAttribute( "EC71B180-9DCC-4F36-AB0F-AF1881A7D9B0" );
            // Attrib for BlockType: Change Password:Change Password Not Supported Caption
            RockMigrationHelper.DeleteAttribute( "A6915BB2-590A-450E-9B02-3C03B7462DA8" );
            // Attrib for BlockType: Transaction Entry:Record Status
            RockMigrationHelper.DeleteAttribute( "36299C3A-53F5-4DC1-AB2C-89482554A305" );
            // Attrib for BlockType: Transaction Entry:Connection Status
            RockMigrationHelper.DeleteAttribute( "743F636C-0EBF-421F-A1DD-4C4848638F6F" );
            // Attrib for BlockType: Confirm Account:Password Reset Unavailable Caption
            RockMigrationHelper.DeleteAttribute( "C27D13AE-0F51-4F94-B7A9-891D067ED73C" );
            // Attrib for BlockType: Account Entry:Connection Status
            RockMigrationHelper.DeleteAttribute( "91960813-EB6A-499C-A140-A54FAC180056" );
            // Attrib for BlockType: Account Entry:Record Status
            RockMigrationHelper.DeleteAttribute( "5010D78F-8764-4EDB-ABFC-8DBED07A9C21" );
            // Attrib for BlockType: Login:Redirect Page
            RockMigrationHelper.DeleteAttribute( "2E55DFD3-8544-4B97-B488-4CCA20363844" );

            RockMigrationHelper.DeleteBlockType( "DA2AAD13-209B-4885-8739-B7BE99F6510D" ); // Model Map
            RockMigrationHelper.DeleteBlockType( "03BFBFCA-36C4-480D-A10B-3CF349F4A6EA" ); // Developer Environment Info
            RockMigrationHelper.DeleteBlockType( "3ABC7007-CE3E-4092-900F-C907948CA8C2" ); // Calendar Item Occurrence List Lava
            RockMigrationHelper.DeleteBlockType( "6DF11547-8757-4305-BC9A-122B9D929342" ); // Calendar Item List Lava
            RockMigrationHelper.DeleteLayout( "F0A9626B-0D87-4057-AD60-6F978E9EDA87" ); //  Layout: Homepage, Site: Rock RMS
            RockMigrationHelper.DeleteLayout( "DAFB37C9-0D61-424C-8788-EE0ACF7EA5FD" ); //  Layout: Error, Site: Rock RMS
            RockMigrationHelper.DeleteLayout( "01158948-1DC3-4A92-B213-9E26BE194F68" ); //  Layout: Blank, Site: Rock Check-in Manager
            RockMigrationHelper.DeleteLayout( "55E19934-762D-48E5-BD07-ACB1249ACBDC" ); //  Layout: Blank, Site: External Website
        }
    }
}
