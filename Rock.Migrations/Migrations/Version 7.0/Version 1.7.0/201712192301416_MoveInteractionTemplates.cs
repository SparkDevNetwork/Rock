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
    public partial class MoveInteractionTemplates : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.InteractionChannel", "ChannelListTemplate", c => c.String());
            AddColumn("dbo.InteractionChannel", "ChannelDetailTemplate", c => c.String());
            AddColumn("dbo.InteractionChannel", "ComponentListTemplate", c => c.String());
            AddColumn("dbo.InteractionChannel", "ComponentDetailTemplate", c => c.String());
            AddColumn("dbo.InteractionChannel", "SessionListTemplate", c => c.String());
            AddColumn("dbo.InteractionChannel", "SessionDetailTemplate", c => c.String());
            AddColumn("dbo.InteractionChannel", "InteractionListTemplate", c => c.String());
            AddColumn("dbo.InteractionChannel", "InteractionDetailTemplate", c => c.String());
            DropColumn("dbo.InteractionComponent", "ListTemplate");
            DropColumn("dbo.InteractionComponent", "DetailTemplate");
            DropColumn("dbo.InteractionChannel", "ListTemplate");
            DropColumn("dbo.InteractionChannel", "DetailTemplate");
            DropColumn("dbo.Interaction", "ListTemplate");
            DropColumn("dbo.Interaction", "DetailTemplate");
            DropColumn("dbo.InteractionSession", "ListTemplate");
            DropColumn("dbo.InteractionSession", "DetailTemplate");

            // MP: Add new 'Communication Page' BlockSetting to Bio and set bio block on the internal person profile page to use the Simple Communication page.
            // Attrib for BlockType: Person Bio:Communication Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Communication Page", "CommunicationPage", "", @"The communication page to use for when the person's email address is clicked. Leave this blank to use the default.", 15, @"", "18B917AE-6395-4B70-95F4-AA5E6EA1F799" );
            // Attrib Value for Block:Bio, Attribute:Communication Page , Layout: PersonDetail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A", "18B917AE-6395-4B70-95F4-AA5E6EA1F799", @"7e8408b2-354c-4a5a-8707-36754ae80b9a" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.InteractionSession", "DetailTemplate", c => c.String());
            AddColumn("dbo.InteractionSession", "ListTemplate", c => c.String());
            AddColumn("dbo.Interaction", "DetailTemplate", c => c.String());
            AddColumn("dbo.Interaction", "ListTemplate", c => c.String());
            AddColumn("dbo.InteractionChannel", "DetailTemplate", c => c.String());
            AddColumn("dbo.InteractionChannel", "ListTemplate", c => c.String());
            AddColumn("dbo.InteractionComponent", "DetailTemplate", c => c.String());
            AddColumn("dbo.InteractionComponent", "ListTemplate", c => c.String());
            DropColumn("dbo.InteractionChannel", "InteractionDetailTemplate");
            DropColumn("dbo.InteractionChannel", "InteractionListTemplate");
            DropColumn("dbo.InteractionChannel", "SessionDetailTemplate");
            DropColumn("dbo.InteractionChannel", "SessionListTemplate");
            DropColumn("dbo.InteractionChannel", "ComponentDetailTemplate");
            DropColumn("dbo.InteractionChannel", "ComponentListTemplate");
            DropColumn("dbo.InteractionChannel", "ChannelDetailTemplate");
            DropColumn("dbo.InteractionChannel", "ChannelListTemplate");
        }
    }
}
