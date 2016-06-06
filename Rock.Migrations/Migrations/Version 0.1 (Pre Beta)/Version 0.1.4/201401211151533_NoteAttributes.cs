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
    public partial class NoteAttributes : Rock.Migrations.RockMigration2
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib for BlockType: Notes:Heading
            AddBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Heading", "Heading", "", "The text to display as the heading.  If left blank, the Note Type name will be used.", 1, @"", "3CB0A7DF-996B-4D6C-B3B6-9BBCC40BDC69" );
            // Attrib for BlockType: Notes:Heading Icon CSS Class
            AddBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Heading Icon CSS Class", "HeadingIcon", "", "The css class name to use for the heading icon. ", 2, @"fa fa-calendar", "B69937BE-000A-4B94-852F-16DE92344392" );
            // Attrib for BlockType: Notes:Note Term
            AddBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Note Term", "NoteTerm", "", "The term to use for note (i.e. 'Note', 'Comment').", 3, @"Note", "FD0727DC-92F4-4765-82CB-3A08B7D864F8" );
            // Attrib for BlockType: Notes:Display Type
            AddBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Display Type", "DisplayType", "", "The format to use for displaying notes.", 4, @"Full", "5232BFAE-4DC8-4270-B38F-D29E1B00AB5E" );
            // Attrib for BlockType: Notes:Use Person Icon
            AddBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Person Icon", "UsePersonIcon", "", "", 5, @"False", "C05757C0-E83E-4170-8CBF-C4E1ABEC36E1" );
            // Attrib for BlockType: Notes:Allow Anonymous
            AddBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Anonymous", "AllowAnonymous", "", "", 9, @"False", "EB9CBD02-2B0F-4BA3-9112-BC73D54159E7" );
            // Attrib for BlockType: Notes:Add Always Visible
            AddBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Add Always Visible", "AddAlwaysVisible", "", "Should the add entry screen always be visible (vs. having to click Add button to display the entry screen).", 10, @"False", "8E0BDD15-6B92-4BB0-9138-E9382B60F3A9" );
            // Attrib for BlockType: Notes:Display Order
            AddBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Display Order", "DisplayOrder", "", "Descending will render with entry field at top and most recent note at top.  Ascending will render with entry field at bottom and most recent note at the end.  Ascending will also disable the more option", 11, @"Descending", "C9FC2C09-1BF5-4711-8F97-0B96633C46B1" );

            // Attrib Value for Block:Prayer Comments, Attribute:Heading Page: Prayer Request Detail, Site: Rock Internal
            AddBlockAttributeValue( "FCFDFA6B-4E3D-40D8-86F7-F25F2F4833C7", "3CB0A7DF-996B-4D6C-B3B6-9BBCC40BDC69", @"Comments" );
            // Attrib Value for Block:Prayer Comments, Attribute:Heading Icon CSS Class Page: Prayer Request Detail, Site: Rock Internal
            AddBlockAttributeValue( "FCFDFA6B-4E3D-40D8-86F7-F25F2F4833C7", "B69937BE-000A-4B94-852F-16DE92344392", @"fa fa-comments" );
            // Attrib Value for Block:Prayer Comments, Attribute:Note Term Page: Prayer Request Detail, Site: Rock Internal
            AddBlockAttributeValue( "FCFDFA6B-4E3D-40D8-86F7-F25F2F4833C7", "FD0727DC-92F4-4765-82CB-3A08B7D864F8", @"Comment" );
            // Attrib Value for Block:Prayer Comments, Attribute:Display Type Page: Prayer Request Detail, Site: Rock Internal
            AddBlockAttributeValue( "FCFDFA6B-4E3D-40D8-86F7-F25F2F4833C7", "5232BFAE-4DC8-4270-B38F-D29E1B00AB5E", @"Full" );
            // Attrib Value for Block:Prayer Comments, Attribute:Use Person Icon Page: Prayer Request Detail, Site: Rock Internal
            AddBlockAttributeValue( "FCFDFA6B-4E3D-40D8-86F7-F25F2F4833C7", "C05757C0-E83E-4170-8CBF-C4E1ABEC36E1", @"True" );
            // Attrib Value for Block:Prayer Comments, Attribute:Allow Anonymous Page: Prayer Request Detail, Site: Rock Internal
            AddBlockAttributeValue( "FCFDFA6B-4E3D-40D8-86F7-F25F2F4833C7", "EB9CBD02-2B0F-4BA3-9112-BC73D54159E7", @"False" );
            // Attrib Value for Block:Prayer Comments, Attribute:Add Always Visible Page: Prayer Request Detail, Site: Rock Internal
            AddBlockAttributeValue( "FCFDFA6B-4E3D-40D8-86F7-F25F2F4833C7", "8E0BDD15-6B92-4BB0-9138-E9382B60F3A9", @"True" );
            // Attrib Value for Block:Prayer Comments, Attribute:Display Order Page: Prayer Request Detail, Site: Rock Internal
            AddBlockAttributeValue( "FCFDFA6B-4E3D-40D8-86F7-F25F2F4833C7", "C9FC2C09-1BF5-4711-8F97-0B96633C46B1", @"Ascending" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Notes:Display Order
            DeleteAttribute( "C9FC2C09-1BF5-4711-8F97-0B96633C46B1" );
            // Attrib for BlockType: Notes:Add Always Visible
            DeleteAttribute( "8E0BDD15-6B92-4BB0-9138-E9382B60F3A9" );
            // Attrib for BlockType: Notes:Allow Anonymous
            DeleteAttribute( "EB9CBD02-2B0F-4BA3-9112-BC73D54159E7" );
            // Attrib for BlockType: Notes:Use Person Icon
            DeleteAttribute( "C05757C0-E83E-4170-8CBF-C4E1ABEC36E1" );
            // Attrib for BlockType: Notes:Display Type
            DeleteAttribute( "5232BFAE-4DC8-4270-B38F-D29E1B00AB5E" );
            // Attrib for BlockType: Notes:Note Term
            DeleteAttribute( "FD0727DC-92F4-4765-82CB-3A08B7D864F8" );
            // Attrib for BlockType: Notes:Heading Icon CSS Class
            DeleteAttribute( "B69937BE-000A-4B94-852F-16DE92344392" );
            // Attrib for BlockType: Notes:Heading
            DeleteAttribute( "3CB0A7DF-996B-4D6C-B3B6-9BBCC40BDC69" );
        }
    }
}
