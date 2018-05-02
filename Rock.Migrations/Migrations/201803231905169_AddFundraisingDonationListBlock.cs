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
    public partial class AddFundraisingDonationListBlock : Rock.Migrations.RockMigration
    {
        #region SQL Statements

        const string AddAttributeToCategory = @"INSERT INTO [AttributeCategory] ([AttributeId], [CategoryId])
SELECT a.[Id], c.[Id]
FROM [Attribute] AS a, [Category] AS c
WHERE a.[Guid] = '{0}' AND c.[Guid] = '{1}'
  AND NOT EXISTS (SELECT [AttributeId], [CategoryId] FROM [AttributeCategory] AS ac WHERE ac.[AttributeId] = a.[Id] AND ac.[CategoryId] = c.[Id])
";

        #endregion

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
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
            Sql( string.Format( AddAttributeToCategory, "EEBF3C2A-1D33-4C0F-A8F5-D62B0B71552F", "171E45E4-74EC-4962-9AEA-56D899217AFB" ) );
            Sql( string.Format( AddAttributeToCategory, "E0FA4EEE-5210-4CB4-838A-7D390576BEE7", "171E45E4-74EC-4962-9AEA-56D899217AFB" ) );
            Sql( string.Format( AddAttributeToCategory, "98BF6D7E-F511-4913-947F-66E39D1378BB", "171E45E4-74EC-4962-9AEA-56D899217AFB" ) );
            Sql( string.Format( AddAttributeToCategory, "2949635A-F117-4C45-9FB9-7F1101DAF6CD", "171E45E4-74EC-4962-9AEA-56D899217AFB" ) );

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

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "2949635A-F117-4C45-9FB9-7F1101DAF6CD" );
            RockMigrationHelper.DeleteAttribute( "98BF6D7E-F511-4913-947F-66E39D1378BB" );
            RockMigrationHelper.DeleteAttribute( "E0FA4EEE-5210-4CB4-838A-7D390576BEE7" );
            RockMigrationHelper.DeleteAttribute( "EEBF3C2A-1D33-4C0F-A8F5-D62B0B71552F" );
            RockMigrationHelper.DeleteAttribute( "F23A364F-8F48-495D-8D63-9EAA8B64773A" );

            RockMigrationHelper.DeleteBlock( "3002742B-7F17-4CB5-AF27-4C3ABB09172A" );
            RockMigrationHelper.DeleteBlock( "5E373C14-91DA-486A-87B2-5EB124B852D2" );
            RockMigrationHelper.DeleteBlockType( "4AAE3DB5-C9F8-4985-B6DC-9037B2F91100" );
        }
    }
}
