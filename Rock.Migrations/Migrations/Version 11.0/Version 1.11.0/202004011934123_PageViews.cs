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
    public partial class PageViews : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Page", "MedianPageLoadTime", c => c.Double());

            RockMigrationHelper.AddPage( true, "EC7A06CD-AAB5-4455-962E-B4043EA2440E", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Page Views", "", "E556D6C5-E2DB-4041-81AB-4F582008155C", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Page Views", "Lists interactions with a particular page.", "~/Blocks/Administration/PageViews.ascx", "Administration", "38C775A7-5CDC-415E-9595-76221354A999" );
            // Add Block to Page: Page Views Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "E556D6C5-E2DB-4041-81AB-4F582008155C".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "38C775A7-5CDC-415E-9595-76221354A999".AsGuid(), "Page Views", "Main", @"", @"", 0, "ADB8978F-07BD-499E-B042-BC34B51B540B" );
            // Attrib for BlockType: Page Properties:Median Time to Serve Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C7988C3E-822D-4E73-882E-9B7684398BAA", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Median Time to Serve Detail Page", "MedianTimeDetailPage", "Median Time to Serve Detail Page", @"The page that shows details about about the median time to serve was calculated.", 2, @"E556D6C5-E2DB-4041-81AB-4F582008155C", "4A8EAB07-7493-4CF5-8CB6-75F4AFA890D5" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Page Properties:Median Time to Serve Detail Page
            RockMigrationHelper.DeleteAttribute( "4A8EAB07-7493-4CF5-8CB6-75F4AFA890D5" );
            // Remove Block: Page Views, from Page: Page Views, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "ADB8978F-07BD-499E-B042-BC34B51B540B" );
            RockMigrationHelper.DeleteBlockType( "38C775A7-5CDC-415E-9595-76221354A999" ); // Page Views
            RockMigrationHelper.DeletePage( "E556D6C5-E2DB-4041-81AB-4F582008155C" ); //  Page: Page Views, Layout: Full Width, Site: Rock RMS

            DropColumn("dbo.Page", "MedianPageLoadTime");
        }
    }
}
