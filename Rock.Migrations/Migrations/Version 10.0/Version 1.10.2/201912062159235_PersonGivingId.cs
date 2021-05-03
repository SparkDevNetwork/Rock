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
    public partial class PersonGivingId : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpUpdatePersonGivingId();

            //SK: Add "Highlight Color" attribute to Audience Type defined type
            UpAddHighlightColorToAudienceType();
        }

        /// <summary>
        /// Ups the type of the add highlight color to audience.
        /// </summary>
        private void UpAddHighlightColorToAudienceType()
        {
            //SK: Add "Highlight Color" attribute to Audience Type defined type
            RockMigrationHelper.AddDefinedTypeAttribute( "799301A3-2026-4977-994E-45DC68502559", "D747E6AE-C383-4E22-8846-71518E3DD06F", "Highlight Color", "HighlightColor", "", 1039, "#c4c4c4", "7CE41BAD-58A3-4A1B-B06D-3277F2C816A5" );
            RockMigrationHelper.AddAttributeQualifier( "7CE41BAD-58A3-4A1B-B06D-3277F2C816A5", "selectiontype", "Color Picker", "2EF74C40-7316-432D-82AE-A89E2DD08D0F" );
        }

        /// <summary>
        /// Ups the update person giving identifier.
        /// </summary>
        private void UpUpdatePersonGivingId()
        {
            DropIndex( "dbo.Person", new[] { "GivingId" } );
            DropColumn( "dbo.Person", "GivingId" );
            AddColumn( "dbo.Person", "GivingId", c => c.String( maxLength: 50 ) );
            CreateIndex( "dbo.Person", "GivingId" );

            Sql( @"
UPDATE Person
SET GivingId = (
		CASE 
			WHEN [GivingGroupId] IS NOT NULL
				THEN 'G' + CONVERT([varchar], [GivingGroupId])
			ELSE 'P' + CONVERT([varchar], [Id])
			END
		)
WHERE GivingId IS NULL OR GivingId != (
		CASE 
			WHEN [GivingGroupId] IS NOT NULL
				THEN 'G' + CONVERT([varchar], [GivingGroupId])
			ELSE 'P' + CONVERT([varchar], [Id])
			END
		)" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // can't really down this, but it doesn't matter since we'll have a GivingID column either way
        }
    }
}
