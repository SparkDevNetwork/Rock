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
    public partial class RssFeedBlockTypes : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateBlockType( "RSS Feed", "Gets and consumes and RSS Feed. The feed is rendered based on a provided liquid template. ", "~/Blocks/Cms/RSSFeed.ascx", "CMS", "2760F435-3E89-4016-85D9-13C019D0C58F" );
            UpdateBlockType( "RSS Feed Item", "Gets an item from a RSS feed and displays the content of that item based on a provided liquid template.", "~/Blocks/Cms/RSSFeedItem.ascx", "CMS", "F7898E47-8496-4D70-9594-4D1F616928F5" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
