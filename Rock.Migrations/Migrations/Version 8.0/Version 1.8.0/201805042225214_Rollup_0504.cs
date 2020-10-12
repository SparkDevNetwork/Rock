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
    public partial class Rollup_0504 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateBlockType( "Fundraising Donation List", "Lists donations in a grid for the current fundraising opportunity or participant.", "~/Blocks/Fundraising/FundraisingDonationList.ascx", "Fundraising", "82E5E387-6171-4E0E-AEDB-B50BF0B7A52D" );
            // Attrib for BlockType: Fundraising Donation List:Entity Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "82E5E387-6171-4E0E-AEDB-B50BF0B7A52D", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", @"The type of entity that will provide context for this block", 0, @"", "10418A26-CC57-4514-A11B-9C038C1D81FA" );
            // Attrib for BlockType: Fundraising Donation List:Participant Column
            RockMigrationHelper.UpdateBlockTypeAttribute( "82E5E387-6171-4E0E-AEDB-B50BF0B7A52D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Participant Column", "ParticipantColumn", "", @"The value that should be displayed for the Participant column. <span class='tip tip-lava'></span>", 3, @"<a href=""/Person/{{ Participant.PersonId }}"" class=""pull-right margin-l-sm btn btn-sm btn-default"">
    <i class=""fa fa-user""></i>
</a>
<a href=""/GroupMember/{{ Participant.Id }}"">{{ Participant.Person.FullName }}</a>", "4A397564-63D5-4E3C-9008-9140C9F263E2" );
            // Attrib for BlockType: Fundraising Donation List:Hide Grid Columns
            RockMigrationHelper.UpdateBlockTypeAttribute( "82E5E387-6171-4E0E-AEDB-B50BF0B7A52D", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Hide Grid Columns", "HideGridColumns", "", @"The grid columns that should be hidden from the user.", 0, @"", "13C277EC-1B63-47CC-B78C-8009CB3D8B03" );
            // Attrib for BlockType: Fundraising Donation List:Hide Grid Actions
            RockMigrationHelper.UpdateBlockTypeAttribute( "82E5E387-6171-4E0E-AEDB-B50BF0B7A52D", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Hide Grid Actions", "HideGridActions", "", @"The grid actions that should be hidden from the user.", 1, @"", "955C28AD-94DE-46C9-9E8B-51EFE7109FB4" );
            // Attrib for BlockType: Fundraising Donation List:Donor Column
            RockMigrationHelper.UpdateBlockTypeAttribute( "82E5E387-6171-4E0E-AEDB-B50BF0B7A52D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Donor Column", "DonorColumn", "", @"The value that should be displayed for the Donor column. <span class='tip tip-lava'></span>", 2, @"<a href=""/Person/{{ Donor.Id }}"">{{ Donor.FullName }}</a>", "0E416F63-DC15-4BAD-AE0A-29A1C090950F" );
            // Attrib for BlockType: Communication Detail:Enable Personal Templates
            RockMigrationHelper.UpdateBlockTypeAttribute( "CEDC742C-0AB3-487D-ABC2-77A0A443AEBF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Personal Templates", "EnablePersonalTemplates", "", @"Should support for personal templates be enabled? These are templates that a user can create and are personal to them. If enabled, they will be able to create a new template based on the current communication.", 0, @"False", "3E9A6243-2A06-489F-9422-A305468839A7" );
            // Attrib for BlockType: Communication Entry:Allowed SMS Numbers
            RockMigrationHelper.UpdateBlockTypeAttribute( "D9834641-7F39-4CFA-8CB2-E64068127565", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Allowed SMS Numbers", "AllowedSMSNumbers", "", @"Set the allowed FROM numbers to appear when in SMS mode (if none are selected all numbers will be included). ", 9, @"", "EC088A6A-8CC0-41BC-B074-2A9788EC710F" );
            // Attrib for BlockType: Communication Entry Wizard:Allowed SMS Numbers
            RockMigrationHelper.UpdateBlockTypeAttribute( "F7D464E2-5F7C-47BA-84DB-7CC7B0B623C0", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Allowed SMS Numbers", "AllowedSMSNumbers", "", @"Set the allowed FROM numbers to appear when in SMS mode (if none are selected all numbers will be included). ", 9, @"", "EE0A9EFA-81DD-4BA0-A6F9-2675114AE592" );
            // Attrib for BlockType: Edit Person:Search Key Types
            RockMigrationHelper.UpdateBlockTypeAttribute( "0A15F28C-4828-4B38-AF66-58AC5BDE48E0", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Search Key Types", "SearchKeyTypes", "", @"Optional list of search key types to limit the display in search keys grid. No selection will show all.", 2, @"", "D94509D6-03BB-4CA0-A7DC-EF27F8E4B076" );
            // Attrib for BlockType: Group Detail Lava:Alternate Communication Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "218B057F-B214-4317-8E84-7A95CF88067E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Alternate Communication Page", "AlternateCommunicationPage", "", @"The communication page to use for sending an alternate communication to the group members.", 5, @"", "27FFD2AE-AD69-4489-88CA-188C5F85E301" );
            // Attrib for BlockType: Group List Personalized Lava:Inactive Parameter Name
            RockMigrationHelper.UpdateBlockTypeAttribute( "1B172C33-8672-4C98-A995-8E123FF316BD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Inactive Parameter Name", "InactiveParameterName", "", @"The page parameter name to toggle inactive groups", 0, @"showinactivegroups", "97206743-85EF-4DB5-B19C-21D48F9FEB07" );
            // Attrib for BlockType: Group List Personalized Lava:Display Inactive Groups
            RockMigrationHelper.UpdateBlockTypeAttribute( "1B172C33-8672-4C98-A995-8E123FF316BD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Inactive Groups", "DisplayInactiveGroups", "", @"Include inactive groups in the lava results", 0, @"False", "16BD94BD-C7E5-4366-97C4-D12FE9BBA641" );
            // Attrib for BlockType: Group List Personalized Lava:Initial Active Setting
            RockMigrationHelper.UpdateBlockTypeAttribute( "1B172C33-8672-4C98-A995-8E123FF316BD", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Initial Active Setting", "InitialActiveSetting", "", @"Select whether to initially show all or just active groups in the lava", 8, @"1", "3C97D277-3B19-44BD-A38A-EEF3047E2B50" );
            // Attrib for BlockType: Group Search:Group URL Format
            RockMigrationHelper.UpdateBlockTypeAttribute( "F1E188A5-2F9D-4BA6-BCA1-82B2450DAC1C", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Group URL Format", "GroupURLFormat", "", @"The URL to use for linking to a group. <span class='tip tip-lava'></span>", 0, @"~/Group/{{ Group.Id }}", "A95943BF-6818-421E-992B-8A529E8B07A6" );
            // Attrib for BlockType: Interaction Channel List:Interaction Channels
            RockMigrationHelper.UpdateBlockTypeAttribute( "FBC2066B-8E7C-43CB-AFD2-FA9408F6699D", "D5781EB0-3A2A-4FBB-AF8E-E14664147003", "Interaction Channels", "InteractionChannels", "", @"Select interaction channel to limit the display. No selection will show all.", 3, @"", "439C2ABE-795D-4433-826E-FD6D9BCE3F7B" );
            // Attrib for BlockType: Report Detail:Report
            RockMigrationHelper.UpdateBlockTypeAttribute( "E431DBDF-5C65-45DC-ADC5-157A02045CCD", "B7FA826C-3367-4BF2-90E5-8C6730079D82", "Report", "Report", "", @"Select the report to present to the user.", 2, @"", "452352A3-4DBB-45FD-B4A7-CEA8A6DD2D47" );
            RockMigrationHelper.UpdateFieldType( "Content Channel Type", "", "Rock", "Rock.Field.Types.ContentChannelTypeFieldType", "2B58514E-47F8-4740-A72C-B862B030855B" );
            RockMigrationHelper.UpdateFieldType( "Registration Instance", "", "Rock", "Rock.Field.Types.RegistrationInstanceFieldType", "5F0F6D6A-DEB7-47AD-93C7-4CCC88EF932D" );
            RockMigrationHelper.UpdateFieldType( "Registration Template", "", "Rock", "Rock.Field.Types.RegistrationTemplateFieldType", "E1EBAEE8-AF7E-426D-9A1B-02CBD785E620" );
            RockMigrationHelper.UpdateFieldType( "Report", "", "Rock", "Rock.Field.Types.ReportFieldType", "B7FA826C-3367-4BF2-90E5-8C6730079D82" );

            UpdateGradeTransitionDateFieldType();
            FundraisingDonationListBlocks();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Report Detail:Report
            RockMigrationHelper.DeleteAttribute( "452352A3-4DBB-45FD-B4A7-CEA8A6DD2D47" );
            // Attrib for BlockType: Interaction Channel List:Interaction Channels
            RockMigrationHelper.DeleteAttribute( "439C2ABE-795D-4433-826E-FD6D9BCE3F7B" );
            // Attrib for BlockType: Group Search:Group URL Format
            RockMigrationHelper.DeleteAttribute( "A95943BF-6818-421E-992B-8A529E8B07A6" );
            // Attrib for BlockType: Group List Personalized Lava:Initial Active Setting
            RockMigrationHelper.DeleteAttribute( "3C97D277-3B19-44BD-A38A-EEF3047E2B50" );
            // Attrib for BlockType: Group List Personalized Lava:Display Inactive Groups
            RockMigrationHelper.DeleteAttribute( "16BD94BD-C7E5-4366-97C4-D12FE9BBA641" );
            // Attrib for BlockType: Group List Personalized Lava:Inactive Parameter Name
            RockMigrationHelper.DeleteAttribute( "97206743-85EF-4DB5-B19C-21D48F9FEB07" );
            // Attrib for BlockType: Group Detail Lava:Alternate Communication Page
            RockMigrationHelper.DeleteAttribute( "27FFD2AE-AD69-4489-88CA-188C5F85E301" );
            // Attrib for BlockType: Edit Person:Search Key Types
            RockMigrationHelper.DeleteAttribute( "D94509D6-03BB-4CA0-A7DC-EF27F8E4B076" );
            // Attrib for BlockType: Communication Entry Wizard:Allowed SMS Numbers
            RockMigrationHelper.DeleteAttribute( "EE0A9EFA-81DD-4BA0-A6F9-2675114AE592" );
            // Attrib for BlockType: Communication Entry:Allowed SMS Numbers
            RockMigrationHelper.DeleteAttribute( "EC088A6A-8CC0-41BC-B074-2A9788EC710F" );
            // Attrib for BlockType: Communication Detail:Enable Personal Templates
            RockMigrationHelper.DeleteAttribute( "3E9A6243-2A06-489F-9422-A305468839A7" );
            // Attrib for BlockType: Fundraising Donation List:Donor Column
            RockMigrationHelper.DeleteAttribute( "0E416F63-DC15-4BAD-AE0A-29A1C090950F" );
            // Attrib for BlockType: Fundraising Donation List:Hide Grid Actions
            RockMigrationHelper.DeleteAttribute( "955C28AD-94DE-46C9-9E8B-51EFE7109FB4" );
            // Attrib for BlockType: Fundraising Donation List:Hide Grid Columns
            RockMigrationHelper.DeleteAttribute( "13C277EC-1B63-47CC-B78C-8009CB3D8B03" );
            // Attrib for BlockType: Fundraising Donation List:Participant Column
            RockMigrationHelper.DeleteAttribute( "4A397564-63D5-4E3C-9008-9140C9F263E2" );
            // Attrib for BlockType: Fundraising Donation List:Entity Type
            RockMigrationHelper.DeleteAttribute( "10418A26-CC57-4514-A11B-9C038C1D81FA" );
            // Fundraising Donation List
            RockMigrationHelper.DeleteBlockType( "82E5E387-6171-4E0E-AEDB-B50BF0B7A52D" );

            // FundraisingDonationListBlocks
            RockMigrationHelper.DeleteAttribute( "2949635A-F117-4C45-9FB9-7F1101DAF6CD" );
            RockMigrationHelper.DeleteAttribute( "98BF6D7E-F511-4913-947F-66E39D1378BB" );
            RockMigrationHelper.DeleteAttribute( "E0FA4EEE-5210-4CB4-838A-7D390576BEE7" );
            RockMigrationHelper.DeleteAttribute( "EEBF3C2A-1D33-4C0F-A8F5-D62B0B71552F" );
            RockMigrationHelper.DeleteAttribute( "F23A364F-8F48-495D-8D63-9EAA8B64773A" );
            RockMigrationHelper.DeleteBlock( "3002742B-7F17-4CB5-AF27-4C3ABB09172A" );
            RockMigrationHelper.DeleteBlock( "5E373C14-91DA-486A-87B2-5EB124B852D2" );
            RockMigrationHelper.DeleteBlockType( "4AAE3DB5-C9F8-4985-B6DC-9037B2F91100" );
        }

        private void UpdateGradeTransitionDateFieldType()
        {
            RockMigrationHelper.UpdateFieldType( "Month Day", "", "Rock", "Rock.Field.Types.MonthDayFieldType", Rock.SystemGuid.FieldType.MONTH_DAY );
            Sql( $@"
DECLARE @FieldTypeIdMonthDay INT = (
 SELECT TOP 1 Id
 FROM FieldType
 WHERE [Guid] = '{Rock.SystemGuid.FieldType.MONTH_DAY}'
 )
UPDATE Attribute
SET FieldTypeId = @FieldTypeIdMonthDay
 ,[Description] = 'The date when kids are moved to the next grade level.'
WHERE [Key] = 'GradeTransitionDate'
 AND [EntityTypeId] IS NULL
 AND (
 FieldTypeId != @FieldTypeIdMonthDay
 OR [Description] != 'The date when kids are moved to the next grade level.'
 )
" );
        }

        private void FundraisingDonationListBlocks()
        {
            #region SQL Statements

            const string addAttributeToCategory = @"INSERT INTO [AttributeCategory] ([AttributeId], [CategoryId])
SELECT a.[Id], c.[Id]
FROM [Attribute] AS a, [Category] AS c
WHERE a.[Guid] = '{0}' AND c.[Guid] = '{1}'
  AND NOT EXISTS (SELECT [AttributeId], [CategoryId] FROM [AttributeCategory] AS ac WHERE ac.[AttributeId] = a.[Id] AND ac.[CategoryId] = c.[Id])
";

            #endregion

            #region Add Fundraising Donation List Block Type

            // BlockType: Fundraising Donation List
            RockMigrationHelper.UpdateBlockType( "Fundraising Donation List",
                "Lists donations in a grid for the current fundraising opportunity or participant.",
                "~/Blocks/Fundraising/FundraisingDonationList.ascx",
                "Fundraising",
                "4AAE3DB5-C9F8-4985-B6DC-9037B2F91100" );

            // Attrib for BlockType: Fundraising Donation List:Entity Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "4AAE3DB5-C9F8-4985-B6DC-9037B2F91100",
                SystemGuid.FieldType.ENTITYTYPE,
                "Entity Type",
                "ContextEntityType",
                "",
                "The type of entity that will provide context for this block",
                0,
                "",
                "F23A364F-8F48-495D-8D63-9EAA8B64773A" );

            // Attrib for BlockType: Fundraising Donation List:Hide Grid Columns
            RockMigrationHelper.UpdateBlockTypeAttribute( "4AAE3DB5-C9F8-4985-B6DC-9037B2F91100",
                SystemGuid.FieldType.MULTI_SELECT,
                "Hide Grid Columns",
                "HideGridColumns",
                "",
                "The grid columns that should be hidden from the user.",
                0,
                "",
                "EEBF3C2A-1D33-4C0F-A8F5-D62B0B71552F" );

            // Attrib for BlockType: Fundraising Donation List:Hide Grid Actions
            RockMigrationHelper.UpdateBlockTypeAttribute( "4AAE3DB5-C9F8-4985-B6DC-9037B2F91100",
                SystemGuid.FieldType.MULTI_SELECT,
                "Hide Grid Actions",
                "HideGridActions",
                "",
                "The grid actions that should be hidden from the user.",
                1,
                "",
                "E0FA4EEE-5210-4CB4-838A-7D390576BEE7" );

            // Attrib for BlockType: Fundraising Donation List:Donor Column
            RockMigrationHelper.UpdateBlockTypeAttribute( "4AAE3DB5-C9F8-4985-B6DC-9037B2F91100",
                SystemGuid.FieldType.CODE_EDITOR,
                "Donor Column",
                "DonorColumn",
                "",
                "The value that should be displayed for the Donor column. <span class='tip tip-lava'></span>",
                2,
                @"<a href=""/Person/{{ Donor.Id }}"">{{ Donor.FullName }}</a>",
                "98BF6D7E-F511-4913-947F-66E39D1378BB" );
            RockMigrationHelper.UpdateAttributeQualifier( "98BF6D7E-F511-4913-947F-66E39D1378BB",
                "editorHeight", "100", "DB01253E-BCB6-460E-B91E-1D7D43ED0031" );
            RockMigrationHelper.UpdateAttributeQualifier( "98BF6D7E-F511-4913-947F-66E39D1378BB",
                "editorMode", "Lava", "F566D66E-A794-4C0D-ABED-B0468B1C8DE3" );
            RockMigrationHelper.UpdateAttributeQualifier( "98BF6D7E-F511-4913-947F-66E39D1378BB",
                "editorTheme", "Rock", "06A12788-5207-4C33-A559-62E62AD35B3E" );

            // Attrib for BlockType: Fundraising Donations List:Participant Column
            RockMigrationHelper.UpdateBlockTypeAttribute( "4AAE3DB5-C9F8-4985-B6DC-9037B2F91100",
                "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5",
                "Participant Column",
                "ParticipantColumn",
                "",
                "The value that should be displayed for the Participant column. <span class='tip tip-lava'></span>",
                3,
                @"<a href=""/Person/{{ Participant.PersonId }}"" class=""pull-right margin-l-sm btn btn-sm btn-default"">
    <i class=""fa fa-user""></i>
</a>
<a href=""/GroupMember/{{ Participant.Id }}"">{{ Participant.Person.FullName }}</a>",
                "2949635A-F117-4C45-9FB9-7F1101DAF6CD" );
            RockMigrationHelper.UpdateAttributeQualifier( "2949635A-F117-4C45-9FB9-7F1101DAF6CD",
                "editorHeight", "100", "3130FF24-E6D8-40B7-BB6E-5EE0662F45F2" );
            RockMigrationHelper.UpdateAttributeQualifier( "2949635A-F117-4C45-9FB9-7F1101DAF6CD",
                "editorMode", "Lava", "C01A4FA7-2BC3-4991-A2C1-6C4672C6D517" );
            RockMigrationHelper.UpdateAttributeQualifier( "2949635A-F117-4C45-9FB9-7F1101DAF6CD",
                "editorTheme", "Rock", "8FFD837E-5CEB-4A39-90E6-3201296EED94" );

            // Update category setting for attributes.
            Sql( string.Format( addAttributeToCategory, "EEBF3C2A-1D33-4C0F-A8F5-D62B0B71552F", "171E45E4-74EC-4962-9AEA-56D899217AFB" ) );
            Sql( string.Format( addAttributeToCategory, "E0FA4EEE-5210-4CB4-838A-7D390576BEE7", "171E45E4-74EC-4962-9AEA-56D899217AFB" ) );
            Sql( string.Format( addAttributeToCategory, "98BF6D7E-F511-4913-947F-66E39D1378BB", "171E45E4-74EC-4962-9AEA-56D899217AFB" ) );
            Sql( string.Format( addAttributeToCategory, "2949635A-F117-4C45-9FB9-7F1101DAF6CD", "171E45E4-74EC-4962-9AEA-56D899217AFB" ) );

            #endregion

            // Add Block to Page: Group View, Site: Rock RMS
            RockMigrationHelper.AddBlock( true,
                SystemGuid.Page.GROUP_VIEWER,
                "",
                "4AAE3DB5-C9F8-4985-B6DC-9037B2F91100",
                "Fundraising Donation List",
                "Main",
                "",
                "",
                2,
                "5E373C14-91DA-486A-87B2-5EB124B852D2" );

            // Add Block to Page: Group Member Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true,
                SystemGuid.Page.GROUP_MEMBER_DETAIL_GROUP_VIEWER,
                "",
                "4AAE3DB5-C9F8-4985-B6DC-9037B2F91100",
                "Fundraising Donation List",
                "Main",
                "",
                "",
                2,
                "3002742B-7F17-4CB5-AF27-4C3ABB09172A" );

            // Set Block Context on Group Viewer page
            RockMigrationHelper.AddBlockAttributeValue( "5E373C14-91DA-486A-87B2-5EB124B852D2",
                "F23A364F-8F48-495D-8D63-9EAA8B64773A",
                SystemGuid.EntityType.GROUP );

            // Set Block Context on Group Member Detail page
            RockMigrationHelper.AddBlockAttributeValue( "3002742B-7F17-4CB5-AF27-4C3ABB09172A",
                "F23A364F-8F48-495D-8D63-9EAA8B64773A",
                SystemGuid.EntityType.GROUP_MEMBER );

            // Set Group context parameter for Group Viewer page
            RockMigrationHelper.UpdatePageContext( SystemGuid.Page.GROUP_VIEWER,
                "Rock.Model.Group",
                "GroupId",
                "B63C0346-28CA-444A-8A30-E668FA3E541E" );
        }
    }
}
