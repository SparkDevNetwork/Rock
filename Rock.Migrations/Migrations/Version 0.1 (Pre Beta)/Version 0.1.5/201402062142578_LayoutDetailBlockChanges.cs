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
    public partial class LayoutDetailBlockChanges : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddBlockType( "Layout Block List", "Lists blocks that are on a given site layout.", "~/Blocks/Cms/LayoutBlockList.ascx", "CD3C0C1D-2171-4FCC-B840-FC6E6F72EEEF" );
            
            // Add Block to Page: Layout Detail, Site: Rock RMS
            AddBlock( "E6217A2B-B16F-4E84-BF67-795CA7F5F9AA", "", "CD3C0C1D-2171-4FCC-B840-FC6E6F72EEEF", "Layout Block List", "Main", "", "", 1, "5FB1CC3B-4550-4099-8C83-044FF57CEAD8" );

            Sql( @"UPDATE [Page] SET [LayoutId] = 13 WHERE [Guid] = 'e6217a2b-b16f-4e84-bf67-795ca7f5f9aa'" );

            // add the System Configuration blocktype
            AddBlockType( "System Configuration", "Used for making configuration changes to configurable items in the web.config.", "~/Blocks/Administration/SystemConfiguration.ascx", "E2D423B8-10F0-49E2-B2A6-D62892379429" );
            
            // add the page with the block
            AddPage( "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "System Configuration", "", "7BFD28F2-6B90-4191-B7C1-2CDBFA23C3FA", "fa fa-file-text" ); // Site:Rock RMS

            // Add Block to Page: System Configuration, Site: Rock RMS
            AddBlock("7BFD28F2-6B90-4191-B7C1-2CDBFA23C3FA","","E2D423B8-10F0-49E2-B2A6-D62892379429","System Configuration","Main","","",0,"FF9F4A63-76EC-4D83-8A30-241F6A473CF9");   

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: System Configuration, from Page: System Configuration, Site: Rock RMS
            DeleteBlock("FF9F4A63-76EC-4D83-8A30-241F6A473CF9");

            // Page: System ConfigurationLayout: Full Width, Site: Rock RMS
            DeletePage( "7BFD28F2-6B90-4191-B7C1-2CDBFA23C3FA" );

            DeleteBlockType( "E2D423B8-10F0-49E2-B2A6-D62892379429" );

            Sql( @"UPDATE [Page] SET [LayoutId] = 12 WHERE [Guid] = 'e6217a2b-b16f-4e84-bf67-795ca7f5f9aa'" );

            DeleteBlock( "5FB1CC3B-4550-4099-8C83-044FF57CEAD8" );
            DeleteBlockType( "CD3C0C1D-2171-4FCC-B840-FC6E6F72EEEF" );
        }
    }
}
