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
            
            // Add Block to Page: Ability Levels, Site: Rock RMS
            RockMigrationHelper.AddBlock("9DD78A23-BE4B-474E-BCBC-F06AAABB67FA","","08C35F15-9AF7-468F-9D50-CDFD3D21220C","Defined Type Detail","Main","","",0,"AF457FEF-E26E-409D-A413-0508355FB4E2"); 

            // Add Block to Page: Ability Levels, Site: Rock RMS
            RockMigrationHelper.AddBlock("9DD78A23-BE4B-474E-BCBC-F06AAABB67FA","","0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE","Defined Value List","Main","","",1,"69EE5E71-4AD1-4B5D-99C7-175177AA7A3E"); 

            // Add Block to Page: Label Merge Fields, Site: Rock RMS
            RockMigrationHelper.AddBlock("1DED4B72-1784-4781-A836-83D705B153FC","","08C35F15-9AF7-468F-9D50-CDFD3D21220C","Defined Type Detail","Main","","",0,"24551954-8068-4DE5-8369-ACD06B6BD6EC"); 

            // Add Block to Page: Label Merge Fields, Site: Rock RMS
            RockMigrationHelper.AddBlock("1DED4B72-1784-4781-A836-83D705B153FC","","0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE","Defined Value List","Main","","",1,"B481F00A-5588-4F40-B605-490EDF30C66E"); 

            // Add Block to Page: Search Type, Site: Rock RMS
            RockMigrationHelper.AddBlock("3E0327B1-EE0E-41DC-87DB-C4C14922A7CA","","08C35F15-9AF7-468F-9D50-CDFD3D21220C","Defined Type Detail","Main","","",0,"66060BF3-C15D-4971-A9DF-E6A1CF54D6F6"); 

            // Add Block to Page: Search Type, Site: Rock RMS
            RockMigrationHelper.AddBlock("3E0327B1-EE0E-41DC-87DB-C4C14922A7CA","","0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE","Defined Value List","Main","","",1,"146FF626-4D12-468D-B86E-8261F47B9A19"); 

            // Attrib for BlockType: Dynamic Data:Merge Fields
            RockMigrationHelper.AddBlockTypeAttribute("E31E02E9-73F6-4B3E-98BA-E0E4F86CA126","9C204CD0-1233-41C5-818A-C5DA439445AA","Merge Fields","MergeFields","","Any fields to make available as merge fields for any new communications",9,@"","8EB882CE-5BB1-4844-9C28-10190903EECD");

            // Attrib for BlockType: Dynamic Data:Formatted Output
            RockMigrationHelper.AddBlockTypeAttribute("E31E02E9-73F6-4B3E-98BA-E0E4F86CA126","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Formatted Output","FormattedOutput","","Optional formatting to apply to the returned results.  If left blank, a grid will be displayed. Example: {% for row in rows %} {{ row.FirstName }}<br/> {% endfor %}",7,@"","6A233402-446C-47E9-94A5-6A247C29BC21");

            // Attrib for BlockType: Dynamic Data:Person Report
            RockMigrationHelper.AddBlockTypeAttribute("E31E02E9-73F6-4B3E-98BA-E0E4F86CA126","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Person Report","PersonReport","","Is this report a list of people.?",8,@"False","8104CE53-FDB3-4E9F-B8E7-FD9E06E7551C");

            // Attrib Value for Block:Defined Type Detail, Attribute:Defined Type Page: Ability Levels, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("AF457FEF-E26E-409D-A413-0508355FB4E2","0305EF98-C791-4626-9996-F189B9BB674C",@"7beef4d4-0860-4913-9a3d-857634d1bf7c");

            // Attrib Value for Block:Defined Value List, Attribute:Defined Type Page: Ability Levels, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("69EE5E71-4AD1-4B5D-99C7-175177AA7A3E","9280D61F-C4F3-4A3E-A9BB-BCD67FF78637",@"7beef4d4-0860-4913-9a3d-857634d1bf7c");

            // Attrib Value for Block:Defined Type Detail, Attribute:Defined Type Page: Label Merge Fields, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("24551954-8068-4DE5-8369-ACD06B6BD6EC","0305EF98-C791-4626-9996-F189B9BB674C",@"e4d289a9-70fa-4381-913e-2a757ad11147");

            // Attrib Value for Block:Defined Value List, Attribute:Defined Type Page: Label Merge Fields, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("B481F00A-5588-4F40-B605-490EDF30C66E","9280D61F-C4F3-4A3E-A9BB-BCD67FF78637",@"e4d289a9-70fa-4381-913e-2a757ad11147");

            // Attrib Value for Block:Defined Type Detail, Attribute:Defined Type Page: Search Type, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("66060BF3-C15D-4971-A9DF-E6A1CF54D6F6","0305EF98-C791-4626-9996-F189B9BB674C",@"1ebcdb30-a89a-4c14-8580-8289ec2c7742");

            // Attrib Value for Block:Defined Value List, Attribute:Defined Type Page: Search Type, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("146FF626-4D12-468D-B86E-8261F47B9A19","9280D61F-C4F3-4A3E-A9BB-BCD67FF78637",@"1ebcdb30-a89a-4c14-8580-8289ec2c7742");

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Defined Value List, from Page: Search Type, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("146FF626-4D12-468D-B86E-8261F47B9A19");
            // Remove Block: Defined Type Detail, from Page: Search Type, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("66060BF3-C15D-4971-A9DF-E6A1CF54D6F6");
            // Remove Block: Defined Value List, from Page: Label Merge Fields, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("B481F00A-5588-4F40-B605-490EDF30C66E");
            // Remove Block: Defined Type Detail, from Page: Label Merge Fields, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("24551954-8068-4DE5-8369-ACD06B6BD6EC");
            // Remove Block: Defined Value List, from Page: Ability Levels, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("69EE5E71-4AD1-4B5D-99C7-175177AA7A3E");
            // Remove Block: Defined Type Detail, from Page: Ability Levels, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("AF457FEF-E26E-409D-A413-0508355FB4E2");


            RockMigrationHelper.DeletePage("3E0327B1-EE0E-41DC-87DB-C4C14922A7CA"); //  Page: Search Type, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage("1DED4B72-1784-4781-A836-83D705B153FC"); //  Page: Label Merge Fields, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage("9DD78A23-BE4B-474E-BCBC-F06AAABB67FA"); //  Page: Ability Levels, Layout: Full Width, Site: Rock RMS

        }
    }
}
