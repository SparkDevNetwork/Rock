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
    public partial class AddPersonalizationSegmentCategories : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey( "dbo.PersonalizationSegment", "CategoryId", "dbo.Category" );
            DropIndex( "dbo.PersonalizationSegment", new[] { "CategoryId" } );
            DropColumn( "dbo.PersonalizationSegment", "CategoryId" );

            CreateTable(
                "dbo.PersonalizationSegmentCategory",
                c => new
                {
                    PersonalizationSegmentId = c.Int( nullable: false ),
                    CategoryId = c.Int( nullable: false ),
                } )
                .PrimaryKey( t => new { t.PersonalizationSegmentId, t.CategoryId } )
                .ForeignKey( "dbo.PersonalizationSegment", t => t.PersonalizationSegmentId, cascadeDelete: true )
                .ForeignKey( "dbo.Category", t => t.CategoryId, cascadeDelete: true )
                .Index( t => t.PersonalizationSegmentId )
                .Index( t => t.CategoryId );

            RockMigrationHelper.UpdateEntityType( "Rock.Model.PersonalizationSegmentCategory", SystemGuid.EntityType.PERSONALIZATION_SEGMENT_CATEGORY, false, true );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.PersonalizationSegmentCategory", "CategoryId", "dbo.Category" );
            DropForeignKey( "dbo.PersonalizationSegmentCategory", "PersonalizationSegmentId", "dbo.PersonalizationSegment" );
            DropIndex( "dbo.PersonalizationSegmentCategory", new[] { "CategoryId" } );
            DropIndex( "dbo.PersonalizationSegmentCategory", new[] { "PersonalizationSegmentId" } );
            DropTable( "dbo.PersonalizationSegmentCategory" );
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.PERSONALIZATION_SEGMENT_CATEGORY );

            AddColumn( "dbo.PersonalizationSegment", "CategoryId", c => c.Int() );
            CreateIndex( "dbo.PersonalizationSegment", "CategoryId" );
            AddForeignKey( "dbo.PersonalizationSegment", "CategoryId", "dbo.Category", "Id" );
        }
    }
}
