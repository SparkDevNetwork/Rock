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
    public partial class Badges : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RenameTable( name: "dbo.PersonBadge", newName: "Badge" );
            RockMigrationHelper.RenameEntityType( SystemGuid.EntityType.BADGE, "Rock.Model.Badge", "Badge", "Rock.Model.Badge, Rock, Version = 0.1.5.0, Culture = neutral, PublicKeyToken = null", true, true );

            RenameColumn( table: "dbo.Badge", name: "EntityTypeId", newName: "BadgeComponentEntityTypeId" );
            RenameIndex( table: "dbo.Badge", name: "IX_EntityTypeId", newName: "IX_BadgeComponentEntityTypeId" );

            Sql( $@"
UPDATE a
SET a.EntityTypeQualifierColumn = 'BadgeComponentEntityTypeId' 
FROM 
    Attribute a
    JOIN EntityType et ON a.EntityTypeId = et.Id 
WHERE 
    a.EntityTypeQualifierColumn = 'EntityTypeId' 
    AND et.Guid = '{SystemGuid.EntityType.BADGE}';" );

            AddColumn( "dbo.Badge", "IsActive", c => c.Boolean( nullable: false, defaultValue: true ) );
            AddColumn( "dbo.Badge", "EntityTypeId", c => c.Int( nullable: false ) );
            Sql( "UPDATE Badge SET EntityTypeId = (SELECT Id FROM EntityType WHERE Name = 'Rock.Model.Person');" );

            AddColumn( "dbo.Badge", "EntityTypeQualifierColumn", c => c.String( maxLength: 50 ) );
            AddColumn( "dbo.Badge", "EntityTypeQualifierValue", c => c.String( maxLength: 200 ) );
            CreateIndex( "dbo.Badge", "EntityTypeId" );
            AddForeignKey( "dbo.Badge", "EntityTypeId", "dbo.EntityType", "Id" );

            RockMigrationHelper.RenamePage( SystemGuid.Page.BADGES, "Badges" );
            RockMigrationHelper.RenamePage( SystemGuid.Page.BADGE_DETAIL, "Badge" );

            RockMigrationHelper.UpdateBlockTypeByGuid( "Badge Detail", "Shows the details of a particular badge.", "~/Blocks/Crm/BadgeDetail.ascx", "CRM", "A79336CD-2265-4E36-B915-CF49956FD689" );
            RockMigrationHelper.UpdateBlockTypeByGuid( "Badge List", "Shows a list of all entity badges.", "~/Blocks/Crm/BadgeList.ascx", "CRM", "D8CCD577-2200-44C5-9073-FD16F174D364" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.UpdateBlockTypeByGuid( "Person Badge Detail", "Shows the details of a particular person badge.", "~/Blocks/Crm/PersonBadgeDetail.ascx", "CRM", "A79336CD-2265-4E36-B915-CF49956FD689" );
            RockMigrationHelper.UpdateBlockTypeByGuid( "Person Badge List", "Shows a list of all person badges.", "~/Blocks/Crm/PersonBadgeList.ascx", "CRM", "D8CCD577-2200-44C5-9073-FD16F174D364" );

            RockMigrationHelper.RenamePage( SystemGuid.Page.BADGES, "Person Profile Badges" );
            RockMigrationHelper.RenamePage( SystemGuid.Page.BADGE_DETAIL, "Person Profile Badge Detail" );

            DropForeignKey( "dbo.Badge", "EntityTypeId", "dbo.EntityType" );
            DropIndex( "dbo.Badge", new[] { "EntityTypeId" } );
            DropColumn( "dbo.Badge", "EntityTypeQualifierValue" );
            DropColumn( "dbo.Badge", "EntityTypeQualifierColumn" );

            DropColumn( "dbo.Badge", "EntityTypeId" );

            DropColumn( "dbo.Badge", "IsActive" );

            Sql( $@"
UPDATE a
SET a.EntityTypeQualifierColumn = 'EntityTypeId' 
FROM 
    Attribute a
    JOIN EntityType et ON a.EntityTypeId = et.Id 
WHERE 
    a.EntityTypeQualifierColumn = 'BadgeComponentEntityTypeId' 
    AND et.Guid = '{SystemGuid.EntityType.BADGE}'" );

            RenameIndex( table: "dbo.Badge", name: "IX_BadgeComponentEntityTypeId", newName: "IX_EntityTypeId" );
            RenameColumn( table: "dbo.Badge", name: "BadgeComponentEntityTypeId", newName: "EntityTypeId" );

            RockMigrationHelper.RenameEntityType( SystemGuid.EntityType.BADGE, "Rock.Model.PersonBadge", "Person Badge", "Rock.Model.PersonBadge, Rock, Version = 0.1.5.0, Culture = neutral, PublicKeyToken = null", true, true );
            RenameTable( name: "dbo.Badge", newName: "PersonBadge" );
        }
    }
}
