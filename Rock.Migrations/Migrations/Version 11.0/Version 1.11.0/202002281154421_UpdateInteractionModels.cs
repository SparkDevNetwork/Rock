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
    public partial class UpdateInteractionModels : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.InteractionChannel", "ChannelCustom1Label", c => c.String(maxLength: 100));
            AddColumn("dbo.InteractionChannel", "ChannelCustom2Label", c => c.String(maxLength: 100));
            AddColumn("dbo.InteractionChannel", "ChannelCustomIndexed1Label", c => c.String(maxLength: 100));
            AddColumn("dbo.Interaction", "ChannelCustom1", c => c.String(maxLength: 500));
            AddColumn("dbo.Interaction", "ChannelCustom2", c => c.String(maxLength: 2000));
            AddColumn("dbo.Interaction", "ChannelCustomIndexed1", c => c.String(maxLength: 500));
            AddColumn("dbo.Interaction", "InteractionLength", c => c.Double());
            AddColumn("dbo.Interaction", "InteractionTimeToServe", c => c.Double());
            CreateIndex("dbo.Interaction", "ChannelCustomIndexed1");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.Interaction", new[] { "ChannelCustomIndexed1" });
            DropColumn("dbo.Interaction", "InteractionTimeToServe");
            DropColumn("dbo.Interaction", "InteractionLength");
            DropColumn("dbo.Interaction", "ChannelCustomIndexed1");
            DropColumn("dbo.Interaction", "ChannelCustom2");
            DropColumn("dbo.Interaction", "ChannelCustom1");
            DropColumn("dbo.InteractionChannel", "ChannelCustomIndexed1Label");
            DropColumn("dbo.InteractionChannel", "ChannelCustom2Label");
            DropColumn("dbo.InteractionChannel", "ChannelCustom1Label");
        }
    }
}
