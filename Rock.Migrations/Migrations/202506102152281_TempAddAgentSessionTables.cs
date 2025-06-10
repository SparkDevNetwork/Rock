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
    public partial class TempAddAgentSessionTables : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.AIAgentSessionAnchor",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    AIAgentSessionId = c.Int( nullable: false ),
                    AddedDateTime = c.DateTime( nullable: false ),
                    RemovedDateTime = c.DateTime(),
                    EntityTypeId = c.Int(),
                    EntityId = c.Int(),
                    IsActive = c.Boolean( nullable: false ),
                    PayloadLastRefreshedDateTime = c.DateTime( nullable: false ),
                    PayloadJson = c.String(),
                    AdditionalSettingsJson = c.String(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.AIAgentSession", t => t.AIAgentSessionId, cascadeDelete: true )
                .ForeignKey( "dbo.EntityType", t => t.EntityTypeId )
                .Index( t => t.AIAgentSessionId )
                .Index( t => t.EntityTypeId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.AIAgentSession",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    AIAgentId = c.Int( nullable: false ),
                    PersonAliasId = c.Int( nullable: false ),
                    RelatedEntityTypeId = c.Int(),
                    RelatedEntityId = c.Int(),
                    LastMessageDateTime = c.DateTime( nullable: false ),
                    AdditionalSettingsJson = c.String(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.AIAgent", t => t.AIAgentId, cascadeDelete: true )
                .ForeignKey( "dbo.PersonAlias", t => t.PersonAliasId )
                .ForeignKey( "dbo.EntityType", t => t.RelatedEntityTypeId, cascadeDelete: true )
                .Index( t => t.AIAgentId )
                .Index( t => t.PersonAliasId )
                .Index( t => t.RelatedEntityTypeId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.AIAgentSessionHistory",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    AIAgentSessionId = c.Int( nullable: false ),
                    MessageRole = c.Int( nullable: false ),
                    MessageDateTime = c.DateTime( nullable: false ),
                    Message = c.String(),
                    IsCurrentlyInContext = c.Boolean( nullable: false ),
                    IsSummary = c.Boolean( nullable: false ),
                    InputTokenCount = c.Int( nullable: false ),
                    OutputTokenCount = c.Int( nullable: false ),
                    AdditionalSettingsJson = c.String(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.AIAgentSession", t => t.AIAgentSessionId, cascadeDelete: true )
                .Index( t => t.AIAgentSessionId )
                .Index( t => t.Guid, unique: true );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.AIAgentSessionAnchor", "EntityTypeId", "dbo.EntityType" );
            DropForeignKey( "dbo.AIAgentSessionAnchor", "AIAgentSessionId", "dbo.AIAgentSession" );
            DropForeignKey( "dbo.AIAgentSession", "RelatedEntityTypeId", "dbo.EntityType" );
            DropForeignKey( "dbo.AIAgentSession", "PersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AIAgentSessionHistory", "AIAgentSessionId", "dbo.AIAgentSession" );
            DropForeignKey( "dbo.AIAgentSession", "AIAgentId", "dbo.AIAgent" );
            DropIndex( "dbo.AIAgentSessionHistory", new[] { "Guid" } );
            DropIndex( "dbo.AIAgentSessionHistory", new[] { "AIAgentSessionId" } );
            DropIndex( "dbo.AIAgentSession", new[] { "Guid" } );
            DropIndex( "dbo.AIAgentSession", new[] { "RelatedEntityTypeId" } );
            DropIndex( "dbo.AIAgentSession", new[] { "PersonAliasId" } );
            DropIndex( "dbo.AIAgentSession", new[] { "AIAgentId" } );
            DropIndex( "dbo.AIAgentSessionAnchor", new[] { "Guid" } );
            DropIndex( "dbo.AIAgentSessionAnchor", new[] { "EntityTypeId" } );
            DropIndex( "dbo.AIAgentSessionAnchor", new[] { "AIAgentSessionId" } );
            DropTable( "dbo.AIAgentSessionHistory" );
            DropTable( "dbo.AIAgentSession" );
            DropTable( "dbo.AIAgentSessionAnchor" );
        }
    }
}
