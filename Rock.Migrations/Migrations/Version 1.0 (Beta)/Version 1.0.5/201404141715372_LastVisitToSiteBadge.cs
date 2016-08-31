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
    public partial class LastVisitToSiteBadge : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // run the down() incase the new pages, etc already exist
            Down();
            
            // Add Person Page Views page
            AddPage( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "195BCD57-1C10-4969-886F-7324B6287B75", "Person Page Views", "", "82E9CDDB-A60E-4C0E-9306-C07BEAAD5F70", "" ); // Site:Rock RMS

            // add blocktype
            UpdateBlockType( "Person Web Session List", "Lists a persons web sessions with details.", "~/Blocks/Crm/PersonPageViews.ascx", "CRM", "877156AE-8D61-4BD9-8E77-0A7FAD9AEACD" );

            // Attrib for BlockType: Person Web Session List:Session Count
            AddBlockTypeAttribute( "877156AE-8D61-4BD9-8E77-0A7FAD9AEACD", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Session Count", "SessionCount", "", "The number of sessions to show per page.", 1, @"20", "F0E8D219-496F-44EC-AEE6-C58A8491046C" );

            // Attrib for BlockType: Person Web Session List:Show Header
            AddBlockTypeAttribute( "877156AE-8D61-4BD9-8E77-0A7FAD9AEACD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Header", "ShowHeader", "", "Determines whether the header showing the person's name and date filter should be displayed", 2, @"True", "A14365FB-60E2-4E2B-98C5-528C8AA3549E" );

            // Add Block to Page: Person Page Views, Site: Rock RMS
            AddBlock( "82E9CDDB-A60E-4C0E-9306-C07BEAAD5F70", "", "877156AE-8D61-4BD9-8E77-0A7FAD9AEACD", "Person Web Session List", "Main", "", "", 0, "932D56E9-1944-48BA-BBE2-AD49454A609D" ); 
            
            // Make sure Rock.PersonProfile.Badge.LastVisitOnSite is registered an EntityType
            UpdateEntityType( "Rock.PersonProfile.Badge.LastVisitOnSite", "Last Visit On Site", "Rock.PersonProfile.Badge.LastVisitOnSite, Rock, Version=1.0.4.0, Culture=neutral, PublicKeyToken=null", false, true, "A8619A37-5DB6-4CD1-AC5A-B2FD9AC80F67" );

            // Ensure the PersonBadge for Rock.PersonProfile.Badge.LastVisitOnSite is added
            UpdatePersonBadge( "Last Visit on External Site", "Shows the number of days since the last visit to the external site and links to the details of the person's page views.",
                "Rock.PersonProfile.Badge.LastVisitOnSite", 0, "8A9AD88E-359F-46FD-9BA1-8B0603644F17" );

            // Add/Update the Attribute
            AddPersonBadgeAttribute( "8A9AD88E-359F-46FD-9BA1-8B0603644F17", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108",
                "Page View Details", "PageViewDetails", "Page to show the details of the page views. If blank no link is created.",
                2, string.Empty, "47D7442A-72CD-41D6-98B7-EB911F11F109" );

            // PersonBadge Page View Details page guid
            AddPersonBadgeAttributeValue( "8A9AD88E-359F-46FD-9BA1-8B0603644F17", "47D7442A-72CD-41D6-98B7-EB911F11F109", "82e9cddb-a60e-4c0e-9306-c07beaad5f70" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // delete PersonBadgeAttribute
            DeleteAttribute( "47D7442A-72CD-41D6-98B7-EB911F11F109" );
            
            // Attrib for BlockType: Person Web Session List:Show Header
            DeleteAttribute( "A14365FB-60E2-4E2B-98C5-528C8AA3549E" );
            
            // Attrib for BlockType: Person Web Session List:Session Count
            DeleteAttribute( "F0E8D219-496F-44EC-AEE6-C58A8491046C" );
            
            // Remove Block: Person Web Session List, from Page: Person Page Views, Site: Rock RMS
            DeleteBlock( "932D56E9-1944-48BA-BBE2-AD49454A609D" );
            
            DeleteBlockType( "877156AE-8D61-4BD9-8E77-0A7FAD9AEACD" ); // Person Web Session List
            
            DeletePage( "82E9CDDB-A60E-4C0E-9306-C07BEAAD5F70" ); // Page: Person Page ViewsLayout: Full Width Panel, Site: Rock RMS
        }
    }
}
