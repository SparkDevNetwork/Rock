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

    using Rock.Model;

    /// <summary>
    ///
    /// </summary>
    public partial class _202501231536362_AddCommunicationEntryWizardV2 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.EmailSection",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        UsageSummary = c.String(nullable: false, maxLength: 500),
                        IsSystem = c.Boolean(nullable: false),
                        SourceMarkup = c.String(nullable: false),
                        CategoryId = c.Int(),
                        ThumbnailBinaryFileId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Category", t => t.CategoryId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.BinaryFile", t => t.ThumbnailBinaryFileId)
                .Index(t => t.CategoryId)
                .Index(t => t.ThumbnailBinaryFileId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            AddColumn("dbo.Communication", "CommunicationTopicValueId", c => c.Int());
            AddColumn("dbo.CommunicationTemplate", "IsStarter", c => c.Boolean(nullable: false));
            CreateIndex("dbo.Communication", "CommunicationTopicValueId");
            AddForeignKey("dbo.Communication", "CommunicationTopicValueId", "dbo.DefinedValue", "Id");
            
            Sql( $"UPDATE [CommunicationTemplate] SET [IsStarter] = 1 WHERE [Guid] = '{SystemGuid.Communication.COMMUNICATION_TEMPLATE_BLANK}'" );
            
            this.RockMigrationHelper.AddDefinedType( 
                "Communication", 
                "Communication Topic",
                "A categorized label used to group and track communications within a specific topic or campaign, ensuring better organization and reporting of outreach efforts.", 
                SystemGuid.DefinedType.COMMUNICATION_TOPIC,
                null,
                true );

            this.RockMigrationHelper.AddOrUpdateEntityType( "Rock.Model.EmailSection", SystemGuid.EntityType.EMAIL_SECTION, true, false );

            this.RockMigrationHelper.UpdateCategory( 
                SystemGuid.EntityType.EMAIL_SECTION, 
                "Starter Sections",
                string.Empty,
                string.Empty, 
                SystemGuid.Category.EMAIL_SECTION_STARTER_SECTIONS );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Communication", "CommunicationTopicValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.EmailSection", "ThumbnailBinaryFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.EmailSection", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.EmailSection", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.EmailSection", "CategoryId", "dbo.Category");
            DropIndex("dbo.Communication", new[] { "CommunicationTopicValueId" });
            DropIndex("dbo.EmailSection", new[] { "Guid" });
            DropIndex("dbo.EmailSection", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.EmailSection", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.EmailSection", new[] { "ThumbnailBinaryFileId" });
            DropIndex("dbo.EmailSection", new[] { "CategoryId" });
            DropColumn("dbo.CommunicationTemplate", "IsStarter");
            DropColumn("dbo.Communication", "CommunicationTopicValueId");
            DropTable("dbo.EmailSection");
            
            this.RockMigrationHelper.DeleteCategory( SystemGuid.Category.EMAIL_SECTION_STARTER_SECTIONS );
            this.RockMigrationHelper.DeleteDefinedType( SystemGuid.DefinedType.COMMUNICATION_TOPIC );
            this.RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.EMAIL_SECTION );
        }
    }
}
