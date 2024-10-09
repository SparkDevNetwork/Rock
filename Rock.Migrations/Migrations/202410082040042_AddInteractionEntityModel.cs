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
    /// <summary>
    ///
    /// </summary>
    public partial class AddInteractionEntityModel : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.InteractionEntity",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EntityTypeId = c.Int(nullable: false),
                        EntityId = c.Int(nullable: false),
                        InteractionGuid = c.Guid(nullable: false),
                        InteractionId = c.Int(),
                        CreatedDateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId, cascadeDelete: true)
                // We don't need a foreign key constraint for this as that
                // will cause queries to Interaction table and it just isn't
                // important.
                //.ForeignKey("dbo.Interaction", t => t.InteractionId)
                // Created manually below.
                //.Index(t => new { t.EntityTypeId, t.EntityId })
                .Index(t => new { t.InteractionId, t.CreatedDateTime });

            // Manually create the above index with an included column.
            Sql( "CREATE INDEX [IX_EntityTypeId_EntityId] ON [dbo].[InteractionEntity] ([EntityTypeId], [EntityId]) INCLUDE([InteractionId])" );

            AddColumn("dbo.EntityType", "IsRelatedToInteractionTrackedOnCreate", c => c.Boolean(nullable: false));
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // We skipped creating this above since we don't want it.
            //DropForeignKey("dbo.InteractionEntity", "InteractionId", "dbo.Interaction");

            DropForeignKey("dbo.InteractionEntity", "EntityTypeId", "dbo.EntityType");
            DropIndex("dbo.InteractionEntity", new[] { "InteractionId", "CreatedDateTime" });
            DropIndex("dbo.InteractionEntity", new[] { "EntityTypeId", "EntityId" });
            DropColumn("dbo.EntityType", "IsRelatedToInteractionTrackedOnCreate");
            DropTable("dbo.InteractionEntity");
        }
    }
}
