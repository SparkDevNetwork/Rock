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
    public partial class PersonalizationEntityId : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropPrimaryKey("dbo.PersonAliasPersonalization");
            DropPrimaryKey("dbo.PersonalizedEntity");
            AddColumn("dbo.PersonAliasPersonalization", "PersonalizationEntityId", c => c.Int(nullable: false));
            AddColumn("dbo.PersonalizedEntity", "PersonalizationEntityId", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.PersonAliasPersonalization", new[] { "PersonAliasId", "PersonalizationType", "PersonalizationEntityId" });
            AddPrimaryKey("dbo.PersonalizedEntity", new[] { "EntityTypeId", "EntityId", "PersonalizationType", "PersonalizationEntityId" });
            DropColumn("dbo.PersonAliasPersonalization", "PersonalizationTypeId");
            DropColumn("dbo.PersonalizedEntity", "PersonalizationTypeId");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.PersonalizedEntity", "PersonalizationTypeId", c => c.Int(nullable: false));
            AddColumn("dbo.PersonAliasPersonalization", "PersonalizationTypeId", c => c.Int(nullable: false));
            DropPrimaryKey("dbo.PersonalizedEntity");
            DropPrimaryKey("dbo.PersonAliasPersonalization");
            DropColumn("dbo.PersonalizedEntity", "PersonalizationEntityId");
            DropColumn("dbo.PersonAliasPersonalization", "PersonalizationEntityId");
            AddPrimaryKey("dbo.PersonalizedEntity", new[] { "EntityTypeId", "EntityId", "PersonalizationType", "PersonalizationTypeId" });
            AddPrimaryKey("dbo.PersonAliasPersonalization", new[] { "PersonAliasId", "PersonalizationType", "PersonalizationTypeId" });
        }
    }
}
