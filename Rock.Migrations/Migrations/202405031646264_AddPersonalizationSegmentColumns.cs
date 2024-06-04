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
    public partial class AddPersonalizationSegmentColumns : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add TimeToUpdateDurationMilliseconds column
            AddColumn( "dbo.PersonalizationSegment", "TimeToUpdateDurationMilliseconds", c => c.Double( nullable: true ) );

            // Add Description column
            AddColumn( "dbo.PersonalizationSegment", "Description", c => c.String() );

            // Add CategoryId column
            AddColumn( "dbo.PersonalizationSegment", "CategoryId", c => c.Int( nullable: true ) );

            // Create a foreign key relationship with the Category table, if applicable
            CreateIndex( "dbo.PersonalizationSegment", "CategoryId" );
            AddForeignKey( "dbo.PersonalizationSegment", "CategoryId", "dbo.Category", "Id", cascadeDelete: false );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Drop the foreign key relationship with the Category table, if applicable
            DropForeignKey( "dbo.PersonalizationSegment", "CategoryId", "dbo.Category" );

            // Drop the index on CategoryId
            DropIndex( "dbo.PersonalizationSegment", new[] { "CategoryId" } );

            // Drop the CategoryId column
            DropColumn( "dbo.PersonalizationSegment", "CategoryId" );

            // Drop the Description column
            DropColumn( "dbo.PersonalizationSegment", "Description" );

            // Drop the TimeToUpdateDurationMilliseconds column
            DropColumn( "dbo.PersonalizationSegment", "TimeToUpdateDurationMilliseconds" );
        }
    }
}
