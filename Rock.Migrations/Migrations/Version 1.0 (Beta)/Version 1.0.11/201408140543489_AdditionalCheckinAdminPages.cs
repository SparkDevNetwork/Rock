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
    public partial class AdditionalCheckinAdminPages : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddPage( "66C5DD58-094C-4FF9-9AFB-44801FCFCC2D", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Ability Levels", "", "9DD78A23-BE4B-474E-BCBC-F06AAABB67FA", "fa fa-child" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "66C5DD58-094C-4FF9-9AFB-44801FCFCC2D", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Label Merge Fields", "", "1DED4B72-1784-4781-A836-83D705B153FC", "fa fa-tag" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "66C5DD58-094C-4FF9-9AFB-44801FCFCC2D", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Search Type", "", "3E0327B1-EE0E-41DC-87DB-C4C14922A7CA", "fa fa-search" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "199DC522-F4D6-4D82-AF44-3C16EE9D2CDA", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "SMS From Values", "", "3F1EA6E5-6C61-444A-A80E-5B66F96F521B", "fa fa-mobile-phone" ); // Site:Rock RMS

            // Add Block to Page: Ability Levels, Site: Rock RMS
            RockMigrationHelper.AddBlock( "9DD78A23-BE4B-474E-BCBC-F06AAABB67FA", "", "08C35F15-9AF7-468F-9D50-CDFD3D21220C", "Defined Type Detail", "Main", "", "", 0, "AF457FEF-E26E-409D-A413-0508355FB4E2" );

            // Add Block to Page: Ability Levels, Site: Rock RMS
            RockMigrationHelper.AddBlock( "9DD78A23-BE4B-474E-BCBC-F06AAABB67FA", "", "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "Defined Value List", "Main", "", "", 1, "69EE5E71-4AD1-4B5D-99C7-175177AA7A3E" );

            // Add Block to Page: Label Merge Fields, Site: Rock RMS
            RockMigrationHelper.AddBlock( "1DED4B72-1784-4781-A836-83D705B153FC", "", "08C35F15-9AF7-468F-9D50-CDFD3D21220C", "Defined Type Detail", "Main", "", "", 0, "24551954-8068-4DE5-8369-ACD06B6BD6EC" );

            // Add Block to Page: Label Merge Fields, Site: Rock RMS
            RockMigrationHelper.AddBlock( "1DED4B72-1784-4781-A836-83D705B153FC", "", "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "Defined Value List", "Main", "", "", 1, "B481F00A-5588-4F40-B605-490EDF30C66E" );

            // Add Block to Page: Search Type, Site: Rock RMS
            RockMigrationHelper.AddBlock( "3E0327B1-EE0E-41DC-87DB-C4C14922A7CA", "", "08C35F15-9AF7-468F-9D50-CDFD3D21220C", "Defined Type Detail", "Main", "", "", 0, "66060BF3-C15D-4971-A9DF-E6A1CF54D6F6" );

            // Add Block to Page: Search Type, Site: Rock RMS
            RockMigrationHelper.AddBlock( "3E0327B1-EE0E-41DC-87DB-C4C14922A7CA", "", "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "Defined Value List", "Main", "", "", 1, "146FF626-4D12-468D-B86E-8261F47B9A19" );

            // Add Block to Page: SMS From Values, Site: Rock RMS
            RockMigrationHelper.AddBlock( "3F1EA6E5-6C61-444A-A80E-5B66F96F521B", "", "08C35F15-9AF7-468F-9D50-CDFD3D21220C", "Defined Type Detail", "Main", "", "", 0, "BD89E08E-97E3-4EFC-A299-8F0956A1B5EF" );

            // Add Block to Page: SMS From Values, Site: Rock RMS
            RockMigrationHelper.AddBlock( "3F1EA6E5-6C61-444A-A80E-5B66F96F521B", "", "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "Defined Value List", "Main", "", "", 1, "6DF11D72-96ED-415B-BACA-1A4390CAA4D7" );

            // Attrib for BlockType: Dynamic Data:Merge Fields
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Merge Fields", "MergeFields", "", "Any fields to make available as merge fields for any new communications", 9, @"", "8EB882CE-5BB1-4844-9C28-10190903EECD" );

            // Attrib for BlockType: Dynamic Data:Formatted Output
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Formatted Output", "FormattedOutput", "", "Optional formatting to apply to the returned results.  If left blank, a grid will be displayed. Example: {% for row in rows %} {{ row.FirstName }}<br/> {% endfor %}", 7, @"", "6A233402-446C-47E9-94A5-6A247C29BC21" );

            // Attrib for BlockType: Dynamic Data:Person Report
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Person Report", "PersonReport", "", "Is this report a list of people.?", 8, @"False", "8104CE53-FDB3-4E9F-B8E7-FD9E06E7551C" );

            // Attrib Value for Block:Defined Type Detail, Attribute:Defined Type Page: Ability Levels, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "AF457FEF-E26E-409D-A413-0508355FB4E2", "0305EF98-C791-4626-9996-F189B9BB674C", @"7beef4d4-0860-4913-9a3d-857634d1bf7c" );

            // Attrib Value for Block:Defined Value List, Attribute:Defined Type Page: Ability Levels, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "69EE5E71-4AD1-4B5D-99C7-175177AA7A3E", "9280D61F-C4F3-4A3E-A9BB-BCD67FF78637", @"7beef4d4-0860-4913-9a3d-857634d1bf7c" );

            // Attrib Value for Block:Defined Type Detail, Attribute:Defined Type Page: Label Merge Fields, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "24551954-8068-4DE5-8369-ACD06B6BD6EC", "0305EF98-C791-4626-9996-F189B9BB674C", @"e4d289a9-70fa-4381-913e-2a757ad11147" );

            // Attrib Value for Block:Defined Value List, Attribute:Defined Type Page: Label Merge Fields, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B481F00A-5588-4F40-B605-490EDF30C66E", "9280D61F-C4F3-4A3E-A9BB-BCD67FF78637", @"e4d289a9-70fa-4381-913e-2a757ad11147" );

            // Attrib Value for Block:Defined Type Detail, Attribute:Defined Type Page: Search Type, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "66060BF3-C15D-4971-A9DF-E6A1CF54D6F6", "0305EF98-C791-4626-9996-F189B9BB674C", @"1ebcdb30-a89a-4c14-8580-8289ec2c7742" );

            // Attrib Value for Block:Defined Value List, Attribute:Defined Type Page: Search Type, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "146FF626-4D12-468D-B86E-8261F47B9A19", "9280D61F-C4F3-4A3E-A9BB-BCD67FF78637", @"1ebcdb30-a89a-4c14-8580-8289ec2c7742" );

            // Attrib Value for Block:Defined Type Detail, Attribute:Defined Type Page: SMS From Values, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BD89E08E-97E3-4EFC-A299-8F0956A1B5EF", "0305EF98-C791-4626-9996-F189B9BB674C", @"611bde1f-7405-4d16-8626-ccfedb0e62be" );

            // Attrib Value for Block:Defined Value List, Attribute:Defined Type Page: SMS From Values, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6DF11D72-96ED-415B-BACA-1A4390CAA4D7", "9280D61F-C4F3-4A3E-A9BB-BCD67FF78637", @"611bde1f-7405-4d16-8626-ccfedb0e62be" );

            // Fix up default value of Chart Style for Attendance Analysis block
            RockMigrationHelper.AddBlockAttributeValue( "3EF007F1-6B46-4BCD-A435-345C03EBCA17", "AD789FB4-2DBF-43E8-9B3F-F85AFCB06979", @"2ABB2EA0-B551-476C-8F6B-478CD08C2227" );

            Sql( @"
    -- Fix search routes
    UPDATE [PageRoute] SET [Route] = 'Person/Search/{SearchType}' WHERE [Route] = 'Person/Search/{SearchType}/{SearchTerm}' 
    UPDATE [PageRoute] SET [Route] = 'Group/Search/{SearchType}' WHERE [Route] = 'Group/Search/{SearchType}/{SearchTerm}' 
    UPDATE [AttributeValue] SET [Value] = 'Person/Search/name/?SearchTerm={0}' WHERE [Guid] = 'EC970033-6AFE-4D19-AF66-417B970308A2'
    UPDATE [AttributeValue] SET [Value] = 'Person/Search/phone/?SearchTerm={0}' WHERE [Guid] = 'B60BB5E5-612D-4B74-9562-64AFCEF94B4F'
    UPDATE [AttributeValue] SET [Value] = 'Person/Search/email/?SearchTerm={0}' WHERE [Guid] = '343FBFEF-BB95-4F59-A901-E0DAD3D8EC2F'
    UPDATE [AttributeValue] SET [Value] = 'Person/Search/address/?SearchTerm={0}' WHERE [Guid] = '93ADF726-4E0A-448A-A84C-614A02C9B354'
    UPDATE [AttributeValue] SET [Value] = 'Group/Search/name/?SearchTerm={0}' WHERE [Guid] = 'E114EA45-75FD-47EF-BEC2-E87CEB9E5119'

    -- rename 'Display Text' to 'Liquid Template' for liquid dashboard widget
    UPDATE [Attribute]
    SET [Key] = 'LiquidTemplate'
        ,[Name] = 'Liquid Template'
    WHERE [Guid] = 'EE736CAE-5BAA-4FA4-B190-70F4F7DE92AB'
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Defined Value List, from Page: SMS From Values, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "6DF11D72-96ED-415B-BACA-1A4390CAA4D7" );
            // Remove Block: Defined Type Detail, from Page: SMS From Values, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "BD89E08E-97E3-4EFC-A299-8F0956A1B5EF" );
            // Remove Block: Defined Value List, from Page: Search Type, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "146FF626-4D12-468D-B86E-8261F47B9A19" );
            // Remove Block: Defined Type Detail, from Page: Search Type, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "66060BF3-C15D-4971-A9DF-E6A1CF54D6F6" );
            // Remove Block: Defined Value List, from Page: Label Merge Fields, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B481F00A-5588-4F40-B605-490EDF30C66E" );
            // Remove Block: Defined Type Detail, from Page: Label Merge Fields, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "24551954-8068-4DE5-8369-ACD06B6BD6EC" );
            // Remove Block: Defined Value List, from Page: Ability Levels, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "69EE5E71-4AD1-4B5D-99C7-175177AA7A3E" );
            // Remove Block: Defined Type Detail, from Page: Ability Levels, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "AF457FEF-E26E-409D-A413-0508355FB4E2" );

            RockMigrationHelper.DeletePage( "3F1EA6E5-6C61-444A-A80E-5B66F96F521B" ); //  Page: SMS From Values, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "3E0327B1-EE0E-41DC-87DB-C4C14922A7CA" ); //  Page: Search Type, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "1DED4B72-1784-4781-A836-83D705B153FC" ); //  Page: Label Merge Fields, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "9DD78A23-BE4B-474E-BCBC-F06AAABB67FA" ); //  Page: Ability Levels, Layout: Full Width, Site: Rock RMS

        }
    }
}
