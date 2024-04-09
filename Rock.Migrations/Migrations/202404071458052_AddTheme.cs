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
    public partial class AddTheme : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddDefinedType( "CMS Settings", "Theme Purpose", "", "99FF0317-9B21-4E56-9F83-EA89A3C8C789", @"" );
            RockMigrationHelper.UpdateDefinedValue( "99FF0317-9B21-4E56-9F83-EA89A3C8C789", "Check-in", "", "2BBB1A44-708E-4469-80DE-4AAE6227BEF8", false );
            RockMigrationHelper.UpdateDefinedValue( "99FF0317-9B21-4E56-9F83-EA89A3C8C789", "Website Legacy", "", "4E1477FD-B105-4E4B-99BB-E5F1B964DC94", false );
            RockMigrationHelper.UpdateDefinedValue( "99FF0317-9B21-4E56-9F83-EA89A3C8C789", "Website NextGen", "", "B177E07F-7E07-4D7B-AFA7-9DE163797659", false );

            CreateTable(
                "dbo.Theme",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        RootPath = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        IsSystem = c.Boolean(nullable: false),
                        PurposeValueId = c.Int(),
                        AdditionalSettingsJson = c.String(),
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
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.DefinedValue", t => t.PurposeValueId)
                .Index(t => t.PurposeValueId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteDefinedValue( "2BBB1A44-708E-4469-80DE-4AAE6227BEF8" ); // Check-in
            RockMigrationHelper.DeleteDefinedValue( "4E1477FD-B105-4E4B-99BB-E5F1B964DC94" ); // Website Legacy
            RockMigrationHelper.DeleteDefinedValue( "B177E07F-7E07-4D7B-AFA7-9DE163797659" ); // Website NextGen
            RockMigrationHelper.DeleteDefinedType( "99FF0317-9B21-4E56-9F83-EA89A3C8C789" ); // Theme Purpose

            DropForeignKey( "dbo.Theme", "PurposeValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.Theme", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Theme", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.Theme", new[] { "Guid" });
            DropIndex("dbo.Theme", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.Theme", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.Theme", new[] { "PurposeValueId" });
            DropTable("dbo.Theme");
        }
    }
}
