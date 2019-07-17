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
    public partial class BadgeTypes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RenameTable( name: "dbo.PersonBadge", newName: "BadgeType" );
            RockMigrationHelper.RenameEntityType( SystemGuid.EntityType.BADGE_TYPE, "Rock.Model.BadgeType", "Badge Type", "Rock.Model.BadgeType, Rock, Version = 0.1.5.0, Culture = neutral, PublicKeyToken = null", true, true );

            RenameColumn( table: "dbo.BadgeType", name: "EntityTypeId", newName: "BadgeComponentEntityTypeId" );
            RenameIndex( table: "dbo.BadgeType", name: "IX_EntityTypeId", newName: "IX_BadgeComponentEntityTypeId" );

            Sql( $@"
UPDATE a
SET a.EntityTypeQualifierColumn = 'BadgeComponentEntityTypeId' 
FROM 
    Attribute a
    JOIN EntityType et ON a.EntityTypeId = et.Id 
WHERE 
    a.EntityTypeQualifierColumn = 'EntityTypeId' 
    AND et.Guid = '{SystemGuid.EntityType.BADGE_TYPE}';" );

            AddColumn( "dbo.BadgeType", "IsActive", c => c.Boolean( nullable: false ) );
            Sql( "UPDATE BadgeType SET IsActive = 1;" );

            AddColumn( "dbo.BadgeType", "EntityTypeId", c => c.Int( nullable: false ) );
            Sql( "UPDATE BadgeType SET EntityTypeId = (SELECT Id FROM EntityType WHERE Name = 'Rock.Model.Person');" );

            AddColumn( "dbo.BadgeType", "EntityTypeQualifierColumn", c => c.String( maxLength: 50 ) );
            AddColumn( "dbo.BadgeType", "EntityTypeQualifierValue", c => c.String( maxLength: 200 ) );
            CreateIndex( "dbo.BadgeType", "EntityTypeId" );
            AddForeignKey( "dbo.BadgeType", "EntityTypeId", "dbo.EntityType", "Id" );

            RockMigrationHelper.RenamePage( SystemGuid.Page.BADGE_TYPES, "Badge Types" );
            RockMigrationHelper.RenamePage( SystemGuid.Page.BADGE_TYPE_DETAIL, "Badge Type" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.RenamePage( SystemGuid.Page.BADGE_TYPES, "Person Profile Badges" );
            RockMigrationHelper.RenamePage( SystemGuid.Page.BADGE_TYPE_DETAIL, "Person Profile Badge Detail" );

            DropForeignKey( "dbo.BadgeType", "EntityTypeId", "dbo.EntityType" );
            DropIndex( "dbo.BadgeType", new[] { "EntityTypeId" } );
            DropColumn( "dbo.BadgeType", "EntityTypeQualifierValue" );
            DropColumn( "dbo.BadgeType", "EntityTypeQualifierColumn" );

            DropColumn( "dbo.BadgeType", "EntityTypeId" );

            DropColumn( "dbo.BadgeType", "IsActive" );

            Sql( $@"
UPDATE a
SET a.EntityTypeQualifierColumn = 'EntityTypeId' 
FROM 
    Attribute a
    JOIN EntityType et ON a.EntityTypeId = et.Id 
WHERE 
    a.EntityTypeQualifierColumn = 'BadgeComponentEntityTypeId' 
    AND et.Guid = '{SystemGuid.EntityType.BADGE_TYPE}'" );

            RenameIndex( table: "dbo.BadgeType", name: "IX_BadgeComponentEntityTypeId", newName: "IX_EntityTypeId" );
            RenameColumn( table: "dbo.BadgeType", name: "BadgeComponentEntityTypeId", newName: "EntityTypeId" );

            RockMigrationHelper.RenameEntityType( SystemGuid.EntityType.BADGE_TYPE, "Rock.Model.PersonBadge", "Person Badge", "Rock.Model.PersonBadge, Rock, Version = 0.1.5.0, Culture = neutral, PublicKeyToken = null", true, true );
            RenameTable( name: "dbo.BadgeType", newName: "PersonBadge" );
        }
    }
}
