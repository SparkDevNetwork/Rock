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
    public partial class TemporaryAddAgentTables : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.AIAgentSkill",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    AIAgentId = c.Int( nullable: false ),
                    AISkillId = c.Int( nullable: false ),
                    AdditionalSettingsJson = c.String(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.AIAgent", t => t.AIAgentId, cascadeDelete: true )
                .ForeignKey( "dbo.AISkill", t => t.AISkillId, cascadeDelete: true )
                .Index( t => t.AIAgentId )
                .Index( t => t.AISkillId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.AIAgent",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( nullable: false, maxLength: 100 ),
                    Description = c.String(),
                    AvatarBinaryFileId = c.Int( nullable: true ),
                    Persona = c.String(),
                    AdditionalSettingsJson = c.String(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.BinaryFile", t => t.AvatarBinaryFileId )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.AvatarBinaryFileId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.AISkill",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( nullable: false, maxLength: 100 ),
                    Description = c.String(),
                    UsageHint = c.String(),
                    CodeEntityTypeId = c.Int( nullable: true ),
                    AdditionalSettingsJson = c.String(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.EntityType", t => t.CodeEntityTypeId )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.CodeEntityTypeId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.AISkillFunction",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    AISkillId = c.Int( nullable: false ),
                    Name = c.String( nullable: false, maxLength: 100 ),
                    Description = c.String(),
                    UsageHint = c.String(),
                    FunctionType = c.Int( nullable: false ),
                    AdditionalSettingsJson = c.String(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.AISkill", t => t.AISkillId, cascadeDelete: true )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.AISkillId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.AISkillFunction", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AISkillFunction", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AISkillFunction", "AISkillId", "dbo.AISkill" );
            DropForeignKey( "dbo.AIAgentSkill", "AISkillId", "dbo.AISkill" );
            DropForeignKey( "dbo.AISkill", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AISkill", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AISkill", "CodeEntityTypeId", "dbo.EntityType" );
            DropForeignKey( "dbo.AIAgentSkill", "AIAgentId", "dbo.AIAgent" );
            DropForeignKey( "dbo.AIAgent", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AIAgent", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AIAgent", "AvatarBinaryFileId", "dbo.BinaryFile" );
            DropIndex( "dbo.AISkillFunction", new[] { "Guid" } );
            DropIndex( "dbo.AISkillFunction", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.AISkillFunction", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.AISkillFunction", new[] { "AISkillId" } );
            DropIndex( "dbo.AISkill", new[] { "Guid" } );
            DropIndex( "dbo.AISkill", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.AISkill", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.AISkill", new[] { "CodeEntityTypeId" } );
            DropIndex( "dbo.AIAgent", new[] { "Guid" } );
            DropIndex( "dbo.AIAgent", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.AIAgent", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.AIAgent", new[] { "AvatarBinaryFileId" } );
            DropIndex( "dbo.AIAgentSkill", new[] { "Guid" } );
            DropIndex( "dbo.AIAgentSkill", new[] { "AISkillId" } );
            DropIndex( "dbo.AIAgentSkill", new[] { "AIAgentId" } );
            DropTable( "dbo.AISkillFunction" );
            DropTable( "dbo.AISkill" );
            DropTable( "dbo.AIAgent" );
            DropTable( "dbo.AIAgentSkill" );
        }
    }
}
