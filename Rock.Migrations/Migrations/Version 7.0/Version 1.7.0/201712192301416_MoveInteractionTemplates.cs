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
