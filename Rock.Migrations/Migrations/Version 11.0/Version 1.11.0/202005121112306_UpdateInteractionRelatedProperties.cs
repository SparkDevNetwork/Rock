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
    public partial class UpdateInteractionRelatedProperties : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.InteractionComponent", "ChannelCustom1", c => c.String(maxLength: 100));
            AddColumn("dbo.InteractionComponent", "ChannelCustom2", c => c.String(maxLength: 100));
            AddColumn("dbo.InteractionComponent", "ChannelCustomIndexed1", c => c.String(maxLength: 100));
            RenameColumn( "dbo.InteractionChannel", "ChannelCustom1Label", "InteractionCustom1Label" );
            RenameColumn( "dbo.InteractionChannel", "ChannelCustom2Label", "InteractionCustom2Label" );
            RenameColumn( "dbo.InteractionChannel", "ChannelCustomIndexed1Label", "InteractionCustomIndexed1Label" );
            AddColumn("dbo.InteractionChannel", "ComponentCustom1Label", c => c.String(maxLength: 100));
            AddColumn("dbo.InteractionChannel", "ComponentCustom2Label", c => c.String(maxLength: 100));
            AddColumn("dbo.InteractionChannel", "ComponentCustomIndexed1Label", c => c.String(maxLength: 100));
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RenameColumn( "dbo.InteractionChannel", "InteractionCustom1Label", "ChannelCustom1Label" );
            RenameColumn( "dbo.InteractionChannel", "InteractionCustom2Label", "ChannelCustom2Label" );
            RenameColumn( "dbo.InteractionChannel", "InteractionCustomIndexed1Label", "ChannelCustomIndexed1Label" );
            DropColumn("dbo.InteractionChannel", "ComponentCustomIndexed1Label");
            DropColumn("dbo.InteractionChannel", "ComponentCustom2Label");
            DropColumn("dbo.InteractionChannel", "ComponentCustom1Label");
            DropColumn("dbo.InteractionComponent", "ChannelCustomIndexed1");
            DropColumn("dbo.InteractionComponent", "ChannelCustom2");
            DropColumn("dbo.InteractionComponent", "ChannelCustom1");
        }
    }
}
