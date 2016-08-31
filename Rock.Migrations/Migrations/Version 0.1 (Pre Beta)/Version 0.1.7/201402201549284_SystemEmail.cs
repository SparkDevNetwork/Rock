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
    public partial class SystemEmail : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DeleteBlock( "845100E9-30D6-45A8-9B80-052395735982" );
            DeleteBlockType( "10DC44E9-ECC1-4679-8A07-C098A0DCD82E" );
            
            AddPage( "89B7A631-EA6F-4DA3-9380-04EE67B63E9E", "195BCD57-1C10-4969-886F-7324B6287B75", "System Email Details", "", "588C72A8-7DEC-405F-BA4A-FE64F87CB817", "" ); // Site:Rock RMS
            Sql( @"
    UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = '588C72A8-7DEC-405F-BA4A-FE64F87CB817'
" );
            
            AddBlockType( "System Email Detail", "Allows the administration of a system email.", "~/Blocks/Communication/SystemEmailDetail.ascx", "82B00455-B8CF-4673-ACF5-641B961DF59F" );
            AddBlockType( "System Email List", "Lists the system emails that can be configured.", "~/Blocks/Communication/SystemEmailList.ascx", "2645A264-D5E5-43E8-8FE2-D351F3D5435B" );
            
            // Add Block to Page: System Emails, Site: Rock RMS
            AddBlock( "89B7A631-EA6F-4DA3-9380-04EE67B63E9E", "", "2645A264-D5E5-43E8-8FE2-D351F3D5435B", "System Email List", "Main", "", "", 0, "68F10E30-BD74-49F5-B63F-DA671E31DA90" );
            // Add Block to Page: System Email Details, Site: Rock RMS
            AddBlock( "588C72A8-7DEC-405F-BA4A-FE64F87CB817", "", "82B00455-B8CF-4673-ACF5-641B961DF59F", "System Email Detail", "Main", "", "", 0, "707A99EB-C24A-46BB-9230-8607E674246C" );

            // Attrib for BlockType: System Email List:Detail Page
            AddBlockTypeAttribute( "2645A264-D5E5-43E8-8FE2-D351F3D5435B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "F9C2FE14-53A2-4A0D-80BE-C0ED84BF5BC6" );
            // Attrib Value for Block:System Email List, Attribute:Detail Page Page: System Emails, Site: Rock RMS
            AddBlockAttributeValue( "68F10E30-BD74-49F5-B63F-DA671E31DA90", "F9C2FE14-53A2-4A0D-80BE-C0ED84BF5BC6", @"588c72a8-7dec-405f-ba4a-fe64f87cb817" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: System Email List:Detail Page
            DeleteAttribute( "F9C2FE14-53A2-4A0D-80BE-C0ED84BF5BC6" );
            // Remove Block: System Email Detail, from Page: System Email Details, Site: Rock RMS
            DeleteBlock( "707A99EB-C24A-46BB-9230-8607E674246C" );
            // Remove Block: System Email List, from Page: System Emails, Site: Rock RMS
            DeleteBlock( "68F10E30-BD74-49F5-B63F-DA671E31DA90" );

            DeleteBlockType( "2645A264-D5E5-43E8-8FE2-D351F3D5435B" ); // System Email List
            DeleteBlockType( "82B00455-B8CF-4673-ACF5-641B961DF59F" ); // System Email Detail
            DeletePage( "588C72A8-7DEC-405F-BA4A-FE64F87CB817" ); // Page: System Email DetailsLayout: Full Width Panel, Site: Rock RMS

            AddBlockType( "Email Templates", "Allows the administration of email templates.", "~/Blocks/Communication/EmailTemplates.ascx", "10DC44E9-ECC1-4679-8A07-C098A0DCD82E" );

            // Add Block to Page: System Emails, Site: Rock RMS
            AddBlock( "89B7A631-EA6F-4DA3-9380-04EE67B63E9E", "", "10DC44E9-ECC1-4679-8A07-C098A0DCD82E", "Email Templates", "Main", "", "", 0, "845100E9-30D6-45A8-9B80-052395735982" );

        }
    }
}
