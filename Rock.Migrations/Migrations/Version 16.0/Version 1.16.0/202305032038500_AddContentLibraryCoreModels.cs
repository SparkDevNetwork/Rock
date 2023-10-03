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
    public partial class AddContentLibraryCoreModels : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddContentLibraryCoreTables();
            AddContentLibraryDefinedTypesAndValues();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RemoveContentLibraryCoreTables();
            RemoveContentLibraryDefinedTypesAndValues();
        }

        private void AddContentLibraryCoreTables()
        {
            CreateTable(
                "dbo.ContentTopicDomain",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( nullable: false, maxLength: 200 ),
                    Description = c.String(),
                    Order = c.Int( nullable: false ),
                    IsSystem = c.Boolean( nullable: false ),
                    IsActive = c.Boolean( nullable: false ),
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
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.ContentTopic",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( nullable: false, maxLength: 200 ),
                    Description = c.String(),
                    Order = c.Int( nullable: false ),
                    IsSystem = c.Boolean( nullable: false ),
                    IsActive = c.Boolean( nullable: false ),
                    ContentTopicDomainId = c.Int( nullable: false ),
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
                .ForeignKey( "dbo.ContentTopicDomain", t => t.ContentTopicDomainId, cascadeDelete: true )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.ContentTopicDomainId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );
        }

        private void AddContentLibraryDefinedTypesAndValues()
        {
            // Library License Type
            RockMigrationHelper.AddDefinedType( "Content Channel", "Library License Type", @"<p class=""alert alert-warning"">Please note that the list of licenses provided in the Content Library should not be modified.</p><p>The details of the license which will include what can and can’t be done with it.</p>", "83FB89B4-205A-41D6-A798-A81F12E6CDB0", "List of licenses that are available for use with the Content Library feature." );
            RockMigrationHelper.UpdateDefinedValue( "83FB89B4-205A-41D6-A798-A81F12E6CDB0", "Open", "The Open content license is a type of license that allows an organization to use, modify, and distribute the licensed content without the need for attribution to the original creator. However, this license does not permit the commercial use of the content, meaning that the content cannot be sold or used for commercial purposes.", "54D8921D-A9E9-46DA-8B7C-433C163FD41A" );
            RockMigrationHelper.UpdateDefinedValue( "83FB89B4-205A-41D6-A798-A81F12E6CDB0", "Author Attribution (No Derivatives)", "This license allows the licensed content to be downloaded and shared with others, as long as the text of content is not modified in any way, and the organization does not use the content for commercial purposes. Additionally, the organization must provide attribution to the individual author when displaying the content.", "9AED8DEE-F74D-4F38-AD45-2423170D31D2" );
            RockMigrationHelper.UpdateDefinedValue( "83FB89B4-205A-41D6-A798-A81F12E6CDB0", "Organization Attribution (No Derivatives)", "This license allows the licensed content to be downloaded and shared with others, as long as the content is not modified in any way, and the content is not used for commercial purposes. Additionally, the user must provide attribution to the organization that published the content when displaying the content.", "577F2BD5-BFDF-41B7-96A8-32C0F1E44905" );

            // External Link Type
            RockMigrationHelper.AddDefinedType( "Content Channel", "External Link Type", @"<p class=""alert alert-warning"">Please note that the list of external link types provided in the Content Library should not be modified.</p>", "D42C1A05-6504-4BDE-BBB7-0EC99A7FB632", "List of external link types that can be used for content channel items." );
            RockMigrationHelper.UpdateDefinedValue( "D42C1A05-6504-4BDE-BBB7-0EC99A7FB632", "Amazon Link", "Link to a specific book or product on Amazon.", "BB3E2119-3CF1-4B20-958E-304BFFF120B1" );
            RockMigrationHelper.UpdateDefinedValue( "D42C1A05-6504-4BDE-BBB7-0EC99A7FB632", "Barnes & Noble Link", "Link to a specific book or product from Barnes and Noble.", "ACAC304A-A760-4313-BD7D-F272C2A8BC5B" );
            RockMigrationHelper.UpdateDefinedValue( "D42C1A05-6504-4BDE-BBB7-0EC99A7FB632", "ChristianBook Link", "Link to a specific book or product on the ChristianBook.com website.", "EE74B3D2-1B5A-4436-9908-ABC63FF59309" );
            RockMigrationHelper.UpdateDefinedValue( "D42C1A05-6504-4BDE-BBB7-0EC99A7FB632", "YouTube Video", "YouTube ID to a specific video. This link should be in the format of 'dQw4w9WgXcQ'.", "FE7A6DE2-206C-420F-B67E-7139BB9B8B6D" );

            // Library Content Type
            RockMigrationHelper.AddDefinedType( "Content Channel", "Library Content Type", @"<p class=""alert alert-warning"">Please note that the list of content types provided in the Content Library should not be modified.</p>", "C23B34D6-91D7-4FC5-AA80-E68A62288A05", "Types of content that can be utilized for the Content Library feature." );
            RockMigrationHelper.UpdateDefinedValue( "C23B34D6-91D7-4FC5-AA80-E68A62288A05", "Article", "General content about a specific topic.", "8B66EBAA-9BE4-42C8-A106-655A2EFD6109" );
        }

        private void RemoveContentLibraryDefinedTypesAndValues()
        {
            // Library License Type
            RockMigrationHelper.DeleteDefinedValue( "54D8921D-A9E9-46DA-8B7C-433C163FD41A" ); // Open
            RockMigrationHelper.DeleteDefinedValue( "9AED8DEE-F74D-4F38-AD45-2423170D31D2" ); // Author Attribution (No Derivatives)
            RockMigrationHelper.DeleteDefinedValue( "577F2BD5-BFDF-41B7-96A8-32C0F1E44905" ); // Organization Attribution (No Derivatives)
            RockMigrationHelper.DeleteDefinedType( "83FB89B4-205A-41D6-A798-A81F12E6CDB0" ); // Library License Type

            // External Link Type
            RockMigrationHelper.DeleteDefinedValue( "BB3E2119-3CF1-4B20-958E-304BFFF120B1" ); // Amazon Link
            RockMigrationHelper.DeleteDefinedValue( "ACAC304A-A760-4313-BD7D-F272C2A8BC5B" ); // Barnes & Noble Link
            RockMigrationHelper.DeleteDefinedValue( "EE74B3D2-1B5A-4436-9908-ABC63FF59309" ); // ChristianBook Link
            RockMigrationHelper.DeleteDefinedValue( "FE7A6DE2-206C-420F-B67E-7139BB9B8B6D" ); // YouTube Video
            RockMigrationHelper.DeleteDefinedType( "D42C1A05-6504-4BDE-BBB7-0EC99A7FB632" ); // External Link Type

            // Library Content Type
            RockMigrationHelper.DeleteDefinedValue( "8B66EBAA-9BE4-42C8-A106-655A2EFD6109" ); // Article
            RockMigrationHelper.DeleteDefinedType( "C23B34D6-91D7-4FC5-AA80-E68A62288A05" ); // Library Content Type
        }

        private void RemoveContentLibraryCoreTables()
        {
            DropForeignKey( "dbo.ContentTopicDomain", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.ContentTopicDomain", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.ContentTopic", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.ContentTopic", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.ContentTopic", "ContentTopicDomainId", "dbo.ContentTopicDomain" );
            DropIndex( "dbo.ContentTopic", new[] { "Guid" } );
            DropIndex( "dbo.ContentTopic", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.ContentTopic", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.ContentTopic", new[] { "ContentTopicDomainId" } );
            DropIndex( "dbo.ContentTopicDomain", new[] { "Guid" } );
            DropIndex( "dbo.ContentTopicDomain", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.ContentTopicDomain", new[] { "CreatedByPersonAliasId" } );
            DropTable( "dbo.ContentTopic" );
            DropTable( "dbo.ContentTopicDomain" );
        }
    }
}
