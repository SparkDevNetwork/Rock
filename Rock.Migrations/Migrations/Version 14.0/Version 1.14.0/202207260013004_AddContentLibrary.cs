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
    /// Adds the tables and entity types for the content library.
    /// </summary>
    public partial class AddContentLibrary : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.ContentLibrary",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( nullable: false, maxLength: 100 ),
                    Description = c.String(),
                    LibraryKey = c.String( maxLength: 100 ),
                    TrendingEnabled = c.Boolean( nullable: false ),
                    TrendingWindowDay = c.Int( nullable: false ),
                    TrendingMaxItems = c.Int( nullable: false ),
                    TrendingGravity = c.Decimal( nullable: false, precision: 18, scale: 2 ),
                    EnableSegments = c.Boolean( nullable: false ),
                    EnableRequestFilters = c.Boolean( nullable: false ),
                    FilterSettings = c.String(),
                    LastIndexDateTime = c.DateTime(),
                    LastIndexItemCount = c.Int(),
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
                "dbo.ContentLibrarySource",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    EntityTypeId = c.Int( nullable: false ),
                    EntityId = c.Int( nullable: false ),
                    OccurrencesToShow = c.Int( nullable: false ),
                    AdditionalSettings = c.String(),
                    ContentLibraryId = c.Int( nullable: false ),
                    Order = c.Int( nullable: false ),
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
                .ForeignKey( "dbo.ContentLibrary", t => t.ContentLibraryId, cascadeDelete: true )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.EntityType", t => t.EntityTypeId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.EntityTypeId )
                .Index( t => t.ContentLibraryId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            RockMigrationHelper.UpdateEntityType( "Rock.Model.ContentLibrary", "Content Library", "Rock.Model.ContentLibrary, Rock, Version=1.14.0.12, Culture=neutral, PublicKeyToken=null", true, true, "AD7B9219-1B47-4164-9DD1-90F0AF588CB8" );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.ContentLibrarySource", "Content Library Source", "Rock.Model.ContentLibrarySource, Rock, Version=1.14.0.12, Culture=neutral, PublicKeyToken=null", true, true, "46BD0E73-14B3-499D-B8BE-C0EF6BDCD733" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.ContentLibrary", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.ContentLibrary", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.ContentLibrarySource", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.ContentLibrarySource", "EntityTypeId", "dbo.EntityType" );
            DropForeignKey( "dbo.ContentLibrarySource", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.ContentLibrarySource", "ContentLibraryId", "dbo.ContentLibrary" );
            DropIndex( "dbo.ContentLibrarySource", new[] { "Guid" } );
            DropIndex( "dbo.ContentLibrarySource", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.ContentLibrarySource", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.ContentLibrarySource", new[] { "ContentLibraryId" } );
            DropIndex( "dbo.ContentLibrarySource", new[] { "EntityTypeId" } );
            DropIndex( "dbo.ContentLibrary", new[] { "Guid" } );
            DropIndex( "dbo.ContentLibrary", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.ContentLibrary", new[] { "CreatedByPersonAliasId" } );
            DropTable( "dbo.ContentLibrarySource" );
            DropTable( "dbo.ContentLibrary" );
        }
    }
}
