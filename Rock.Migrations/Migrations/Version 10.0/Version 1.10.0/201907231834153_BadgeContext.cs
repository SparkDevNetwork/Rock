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
    public partial class BadgeContext : Rock.Migrations.RockMigration
    {
        private static string ContextAttributeGuid = "AE2F0B6A-05ED-49AC-BD28-E65AD0679E67";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AlterColumn( "dbo.Badge", "EntityTypeId", c => c.Int( nullable: true ) );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                SystemGuid.BlockType.BADGES,
                SystemGuid.FieldType.ENTITYTYPE,
                "Entity Type",
                "ContextEntityType",
                "Entity Type",
                "The type of entity that will provide context for this block",
                0,
                SystemGuid.EntityType.PERSON,
                ContextAttributeGuid );

            RockMigrationHelper.AddBlockAttributeValue( "98A30DD7-8665-4C6D-B1BB-A8380E862A04", ContextAttributeGuid, SystemGuid.EntityType.PERSON );
            RockMigrationHelper.AddBlockAttributeValue( "AA588E23-D34C-433A-BA3D-B0B82797A22F", ContextAttributeGuid, SystemGuid.EntityType.PERSON );
            RockMigrationHelper.AddBlockAttributeValue( "F3E6CC14-C540-4FFC-A5A9-48AD9CC0A61B", ContextAttributeGuid, SystemGuid.EntityType.PERSON );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // There is no code here intentionally. The context attribute might have been created by the migration or
            // by the user adding a context entity, so we don't want to delete it. The EntityTypeId model property 
            // is nullable and always should have been nullable.
        }
    }
}
