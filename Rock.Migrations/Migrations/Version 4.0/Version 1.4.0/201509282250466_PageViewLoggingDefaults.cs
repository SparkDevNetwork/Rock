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
    public partial class PageViewLoggingDefaults : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // MP: Set Page View Logging Defaults
            Sql( @"/*
Don't track page views for:
Check-in
Check-in Manager
Self Service Kiosk
*/
update [Site] set EnablePageViews=0 where [Guid] in ( '15AEFC01-ACB3-4F5D-B83E-AB3AB7F2A54A','A5FA7C3C-A238-4E0B-95DE-B540144321EC','05E96F7B-B75E-4987-825A-B6F51F8D9CAA')

/* Internal site set the 'Page View Retention Period' to 60 days */
update [Site] set PageViewRetentionPeriodDays = 60 where [Guid] = 'C2D29296-6A87-47A9-A753-EE4E9159C4C4'" );

            // JE: Add following list to My Dashbaord
            // Attrib for BlockType: Person Suggestion Notice:List Page
            RockMigrationHelper.AddBlockTypeAttribute( "983B9EBE-BDD9-49A6-87FF-7E1A585E97E4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Followers Page", "FollowersPage", "", "", 1, @"", "28441CF6-08ED-0DA0-45A1-EE05860AB392" );

            // Attrib Value for Block:Event List, Attribute:Detail Page Page: Following Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D54B70C3-B964-459D-B5B6-39BB49AE4E7A", "28441CF6-08ED-0DA0-45A1-EE05860AB392", @"A6AE67F7-0B46-4F9A-9C96-054E1E82F784" );

            // MP: Update HelpText on File Storage Provider Path 
            Sql( @"UPDATE [Attribute]
    SET [Description] = 'The relative path where files should be stored on the file system ( Default: ''~/App_Data/Files'' ).'
    WHERE [Guid] = '3CAFA34D-9208-439B-A046-CB727FB729DE'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
