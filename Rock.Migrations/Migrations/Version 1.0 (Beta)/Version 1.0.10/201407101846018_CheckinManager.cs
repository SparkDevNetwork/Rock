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
    public partial class CheckinManager : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddSite( "Rock Check-in Manager", "Rock site to manage check-in", "Rock", "A5FA7C3C-A238-4E0B-95DE-B540144321EC" );

            RockMigrationHelper.AddLayout( "A5FA7C3C-A238-4E0B-95DE-B540144321EC", "Dialog", "Dialog", "", "39C06290-BD9D-4982-A89C-8D22A0E91D22" ); // Site:Rock Check-in Manager
            RockMigrationHelper.AddLayout( "A5FA7C3C-A238-4E0B-95DE-B540144321EC", "Error", "Error", "", "85D80B81-498B-4647-8DD5-7981D8C74F81" ); // Site:Rock Check-in Manager
            RockMigrationHelper.AddLayout( "A5FA7C3C-A238-4E0B-95DE-B540144321EC", "FullWidth", "Full Width", "", "8305704F-928D-4379-967A-253E576E0923" ); // Site:Rock Check-in Manager
            RockMigrationHelper.AddLayout( "A5FA7C3C-A238-4E0B-95DE-B540144321EC", "Homepage", "Homepage", "", "EFDA3DF0-6242-4332-BFF7-8C85CC6CC20A" ); // Site:Rock Check-in Manager
            RockMigrationHelper.AddLayout( "A5FA7C3C-A238-4E0B-95DE-B540144321EC", "LeftSidebar", "Left Sidebar", "", "2669A579-48A5-4160-88EA-C3A10024E1E1" ); // Site:Rock Check-in Manager
            RockMigrationHelper.AddLayout( "A5FA7C3C-A238-4E0B-95DE-B540144321EC", "PersonDetail", "Person Detail", "", "E67CD583-4DD6-4523-A5C1-204DB71FF6B8" ); // Site:Rock Check-in Manager
            RockMigrationHelper.AddLayout( "A5FA7C3C-A238-4E0B-95DE-B540144321EC", "RightSidebar", "Right Sidebar", "", "25605CF3-9D3D-48B2-ACA5-2CFE7050C0F3" ); // Site:Rock Check-in Manager
            RockMigrationHelper.AddLayout( "A5FA7C3C-A238-4E0B-95DE-B540144321EC", "Splash", "Splash", "", "07AADC2E-F986-42C5-8406-A9CBCECEA144" ); // Site:Rock Check-in Manager
            RockMigrationHelper.AddLayout( "A5FA7C3C-A238-4E0B-95DE-B540144321EC", "ThreeColumn", "Three Column", "", "BEA61CC2-F7BB-488F-9E82-6E047AC4A722" ); // Site:Rock Check-in Manager

            RockMigrationHelper.AddPage( "", "8305704F-928D-4379-967A-253E576E0923", "Check-in Type", "", "62C70118-0A6F-432A-9D84-A5296655CB9E", "" ); // Site:Rock Check-in Manager
            RockMigrationHelper.AddPageRoute( "62C70118-0A6F-432A-9D84-A5296655CB9E", "ManageCheckin" );
            RockMigrationHelper.AddPage( "62C70118-0A6F-432A-9D84-A5296655CB9E", "8305704F-928D-4379-967A-253E576E0923", "Check-in Manager", "", "A4DCE339-9C11-40CA-9A02-D2FE64EA164B", "" ); // Site:Rock Check-in Manager
            RockMigrationHelper.AddPage( "A4DCE339-9C11-40CA-9A02-D2FE64EA164B", "8305704F-928D-4379-967A-253E576E0923", "Person Profile", "", "F3062622-C6AD-48F3-ADD7-7F58E4BD4EF3", "" ); // Site:Rock Check-in Manager

            Sql( @"
    DECLARE @PageId int = (SELECT [Id] FROM [Page] WHERE [Guid] = '62C70118-0A6F-432A-9D84-A5296655CB9E')
    UPDATE [Site] SET [DefaultPageId] = @PageId WHERE [Guid] = 'A5FA7C3C-A238-4E0B-95DE-B540144321EC'
" );

            RockMigrationHelper.UpdateBlockType( "Select Check-In Area", "Block used to select a type of check-in area before managing locations.", "~/Blocks/CheckIn/Manager/SelectArea.ascx", "Check-in > Manager", "17E8F764-562A-4E94-980D-FF1B15640670" );
            RockMigrationHelper.UpdateBlockType( "Locations", "Block used to view current check-in counts and locations.", "~/Blocks/CheckIn/Manager/Locations.ascx", "Check-in > Manager", "00FC1DEA-FE34-41E3-BC0A-2EE9138091EC" );
            RockMigrationHelper.UpdateBlockType( "Person Profile", "Displays person and details about recent check-ins.", "~/Blocks/CheckIn/Manager/Person.ascx", "Check-in > Manager", "48BBB7A7-1E1D-461E-9B64-E9CAD815E9E1" );

            // Add Block to Layout: Full Width, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlock( "", "8305704F-928D-4379-967A-253E576E0923", "4A5AAFFC-B1C7-4EFD-A9E4-84363242EA85", "Campus Context Setter", "Login", "", "", 0, "8B940F43-C38A-4086-80D8-7C33961518E3" );

            // Add Block to Page: Check-in Type, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlock( "62C70118-0A6F-432A-9D84-A5296655CB9E", "", "17E8F764-562A-4E94-980D-FF1B15640670", "Select Check-In Area", "Main", "", "", 0, "F3D99F2C-417F-45C2-B518-E07BEC6E58D9" );
            // Add Block to Page: Check-in Manager, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlock( "A4DCE339-9C11-40CA-9A02-D2FE64EA164B", "", "00FC1DEA-FE34-41E3-BC0A-2EE9138091EC", "Check-in Manager", "Main", "", "", 0, "D38C2DA2-4F76-4BA5-9B26-ADA39D98DEDC" );
            // Add Block to Page: Person Profile, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlock( "F3062622-C6AD-48F3-ADD7-7F58E4BD4EF3", "", "48BBB7A7-1E1D-461E-9B64-E9CAD815E9E1", "Person Profile", "Main", "", "", 0, "1D33D2F9-D19C-495B-BBC8-4379AEF416FE" );

            // Attrib for BlockType: Select Check-In Area:Location Page
            RockMigrationHelper.AddBlockTypeAttribute( "17E8F764-562A-4E94-980D-FF1B15640670", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Location Page", "LocationPage", "", "Page used to display locations", 2, @"", "64A3829D-66C2-43ED-8A34-67A27B21B9E3" );

            // Attrib for BlockType: Locations:Navigation Mode
            RockMigrationHelper.AddBlockTypeAttribute( "00FC1DEA-FE34-41E3-BC0A-2EE9138091EC", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Navigation Mode", "Mode", "", "Navigation and attendance counts can be grouped and displayed either by 'Group Type > Group Type (etc) > Group > Location' or by 'location > location (etc).'  Select the navigation heirarchy that is most appropriate for your organization.", 0, @"T", "3C54B604-47B3-43B4-8B9E-63D8494519E8" );
            // Attrib for BlockType: Locations:Check-in Type
            RockMigrationHelper.AddBlockTypeAttribute( "00FC1DEA-FE34-41E3-BC0A-2EE9138091EC", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Check-in Type", "GroupTypeTemplate", "", "The Check-in Area to display.  This value can also be overridden through the URL query string key (e.g. when navigated to from the Check-in Type selection block).", 1, @"", "B67238FA-5ED7-448A-B3C7-FA07F47DE854" );
            // Attrib for BlockType: Locations:Chart Style
            RockMigrationHelper.AddBlockTypeAttribute( "00FC1DEA-FE34-41E3-BC0A-2EE9138091EC", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Chart Style", "ChartStyle", "", "", 3, @"2ABB2EA0-B551-476C-8F6B-478CD08C2227", "4708D29F-0D15-4175-91C6-A7AFEA37BF1D" );
            // Attrib for BlockType: Locations:Person Page
            RockMigrationHelper.AddBlockTypeAttribute( "00FC1DEA-FE34-41E3-BC0A-2EE9138091EC", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Page", "PersonPage", "", "The page used to display a selected person's details.", 2, @"", "FDC448AC-A552-46A4-A4C4-1700FB150859" );
            
            // Attrib for BlockType: Person Profile:Manager Page
            RockMigrationHelper.AddBlockTypeAttribute( "48BBB7A7-1E1D-461E-9B64-E9CAD815E9E1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Manager Page", "ManagerPage", "", "Page used to manage check-in locations", 0, @"", "DB666717-A429-4F44-B713-3F57702F3BD6" );

            // Attrib Value for Block:Select Check-In Area, Attribute:Location Page Page: Check-in Type, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue( "F3D99F2C-417F-45C2-B518-E07BEC6E58D9", "64A3829D-66C2-43ED-8A34-67A27B21B9E3", @"a4dce339-9c11-40ca-9a02-d2fe64ea164b" );
            // Attrib Value for Block:Select Check-In Area, Attribute:Navigation Mode Page: Check-in Manager, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue( "D38C2DA2-4F76-4BA5-9B26-ADA39D98DEDC", "3C54B604-47B3-43B4-8B9E-63D8494519E8", @"T" );
            // Attrib Value for Block:Select Check-In Area, Attribute:Check-in Type Page: Check-in Manager, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue( "D38C2DA2-4F76-4BA5-9B26-ADA39D98DEDC", "B67238FA-5ED7-448A-B3C7-FA07F47DE854", @"fedd389a-616f-4a53-906c-63d8255631c5" );
            // Attrib Value for Block:Select Check-In Area, Attribute:Chart Style Page: Check-in Manager, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue( "D38C2DA2-4F76-4BA5-9B26-ADA39D98DEDC", "4708D29F-0D15-4175-91C6-A7AFEA37BF1D", @"b45da8e1-b9a6-46fd-9a2b-e8440d7d6aac" );
            // Attrib Value for Block:Select Check-In Area, Attribute:Person Page Page: Check-in Manager, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue( "D38C2DA2-4F76-4BA5-9B26-ADA39D98DEDC", "FDC448AC-A552-46A4-A4C4-1700FB150859", @"f3062622-c6ad-48f3-add7-7f58e4bd4ef3" );

            // Attrib Value for Block:Person Profile, Attribute:Manager Page Page: Person Profile, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue( "1D33D2F9-D19C-495B-BBC8-4379AEF416FE", "DB666717-A429-4F44-B713-3F57702F3BD6", @"a4dce339-9c11-40ca-9a02-d2fe64ea164b" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Person Profile:Manager Page
            RockMigrationHelper.DeleteAttribute( "DB666717-A429-4F44-B713-3F57702F3BD6" );

            // Attrib for BlockType: Locations:Person Page
            RockMigrationHelper.DeleteAttribute( "FDC448AC-A552-46A4-A4C4-1700FB150859" );
            // Attrib for BlockType: Locations:Chart Style
            RockMigrationHelper.DeleteAttribute( "4708D29F-0D15-4175-91C6-A7AFEA37BF1D" );
            // Attrib for BlockType: Locations:Check-in Type
            RockMigrationHelper.DeleteAttribute( "B67238FA-5ED7-448A-B3C7-FA07F47DE854" );
            // Attrib for BlockType: Locations:Navigation Mode
            RockMigrationHelper.DeleteAttribute( "3C54B604-47B3-43B4-8B9E-63D8494519E8" );

            // Attrib for BlockType: Select Check-In Area:Location Page
            RockMigrationHelper.DeleteAttribute( "64A3829D-66C2-43ED-8A34-67A27B21B9E3" );
            
            // Remove Block: Campus Context Setter, from Layout: Full Width, Site: Rock Check-in Manager
            RockMigrationHelper.DeleteBlock( "8B940F43-C38A-4086-80D8-7C33961518E3" );
            // Remove Block: Person Profile, from Page: Person Profile, Site: Rock Check-in Manager
            RockMigrationHelper.DeleteBlock( "1D33D2F9-D19C-495B-BBC8-4379AEF416FE" );
            // Remove Block: Select Check-In Area, from Page: Check-in Manager, Site: Rock Check-in Manager
            RockMigrationHelper.DeleteBlock( "D38C2DA2-4F76-4BA5-9B26-ADA39D98DEDC" );
            // Remove Block: Select Check-In Area, from Page: Check-in Type, Site: Rock Check-in Manager
            RockMigrationHelper.DeleteBlock( "F3D99F2C-417F-45C2-B518-E07BEC6E58D9" );
            
            RockMigrationHelper.DeleteBlockType( "17E8F764-562A-4E94-980D-FF1B15640670" ); // Select Check-In Area
            RockMigrationHelper.DeleteBlockType( "48BBB7A7-1E1D-461E-9B64-E9CAD815E9E1" ); // Person Profile
            RockMigrationHelper.DeleteBlockType( "00FC1DEA-FE34-41E3-BC0A-2EE9138091EC" ); // Locations

            RockMigrationHelper.DeleteLayout( "BEA61CC2-F7BB-488F-9E82-6E047AC4A722" ); // Layout: Three Column, Site: Rock Check-in Manager
            RockMigrationHelper.DeleteLayout( "07AADC2E-F986-42C5-8406-A9CBCECEA144" ); // Layout: Splash, Site: Rock Check-in Manager
            RockMigrationHelper.DeleteLayout( "25605CF3-9D3D-48B2-ACA5-2CFE7050C0F3" ); // Layout: Right Sidebar, Site: Rock Check-in Manager
            RockMigrationHelper.DeleteLayout( "E67CD583-4DD6-4523-A5C1-204DB71FF6B8" ); // Layout: Person Detail, Site: Rock Check-in Manager
            RockMigrationHelper.DeleteLayout( "2669A579-48A5-4160-88EA-C3A10024E1E1" ); // Layout: Left Sidebar, Site: Rock Check-in Manager
            RockMigrationHelper.DeleteLayout( "EFDA3DF0-6242-4332-BFF7-8C85CC6CC20A" ); // Layout: Homepage, Site: Rock Check-in Manager
            RockMigrationHelper.DeleteLayout( "8305704F-928D-4379-967A-253E576E0923" ); // Layout: Full Width, Site: Rock Check-in Manager
            RockMigrationHelper.DeleteLayout( "85D80B81-498B-4647-8DD5-7981D8C74F81" ); // Layout: Error, Site: Rock Check-in Manager
            RockMigrationHelper.DeleteLayout( "39C06290-BD9D-4982-A89C-8D22A0E91D22" ); // Layout: Dialog, Site: Rock Check-in Manager

            RockMigrationHelper.DeleteSite( "A5FA7C3C-A238-4E0B-95DE-B540144321EC" );

            RockMigrationHelper.DeletePage( "F3062622-C6AD-48F3-ADD7-7F58E4BD4EF3" ); // Page: Person ProfileLayout: Full Width, Site: Rock Check-in Manager
            RockMigrationHelper.DeletePage( "A4DCE339-9C11-40CA-9A02-D2FE64EA164B" ); // Page: Check-in ManagerLayout: Full Width, Site: Rock Check-in Manager
            RockMigrationHelper.DeletePage( "62C70118-0A6F-432A-9D84-A5296655CB9E" ); // Page: Check-in TypeLayout: Full Width, Site: Rock Check-in Manager
            
        }
    }
}
