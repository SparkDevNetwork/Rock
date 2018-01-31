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
    public partial class AdditionalInteractionFields : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropIndex("dbo.Interaction", new[] { "PersonAliasId" });
            AddColumn("dbo.InteractionComponent", "Name", c => c.String());
            AddColumn("dbo.InteractionChannel", "Name", c => c.String());
            AddColumn("dbo.InteractionDeviceType", "Name", c => c.String());
            AlterColumn("dbo.Interaction", "EntityId", c => c.Int());
            AlterColumn("dbo.Interaction", "PersonAliasId", c => c.Int());
            CreateIndex("dbo.Interaction", "PersonAliasId");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.Interaction", new[] { "PersonAliasId" });
            AlterColumn("dbo.Interaction", "PersonAliasId", c => c.Int(nullable: false));
            AlterColumn("dbo.Interaction", "EntityId", c => c.Int(nullable: false));
            DropColumn("dbo.InteractionDeviceType", "Name");
            DropColumn("dbo.InteractionChannel", "Name");
            DropColumn("dbo.InteractionComponent", "Name");
            CreateIndex("dbo.Interaction", "PersonAliasId");
        }
    }
}
