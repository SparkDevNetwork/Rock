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
    public partial class AddInteractionSessionLocation : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.InteractionSessionLocation",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IpAddress = c.String(maxLength: 45),
                        LookupDateTime = c.DateTime(nullable: false),
                        PostalCode = c.String(maxLength: 50),
                        Location = c.String(maxLength: 250),
                        ISP = c.String(maxLength: 100),
                        CountryCode = c.String(maxLength: 2),
                        CountryValueId = c.Int(),
                        RegionCode = c.String(maxLength: 2),
                        RegionValueId = c.Int(),
                        GeoPoint = c.Geography(),
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
                .ForeignKey("dbo.DefinedValue", t => t.CountryValueId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.DefinedValue", t => t.RegionValueId)
                .Index(t => t.CountryValueId)
                .Index(t => t.RegionValueId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            AddColumn("dbo.Site", "EnablePageViewGeoTracking", c => c.Boolean(nullable: false));
            AddColumn("dbo.InteractionSession", "SessionStartDateKey", c => c.Int());
            AddColumn("dbo.InteractionSession", "DurationSeconds", c => c.Int());
            AddColumn("dbo.InteractionSession", "DurationLastCalculatedDateTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.InteractionSession", "InteractionSessionLocationId", c => c.Int());
            CreateIndex("dbo.InteractionSession", "InteractionSessionLocationId");
            AddForeignKey("dbo.InteractionSession", "InteractionSessionLocationId", "dbo.InteractionSessionLocation", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.InteractionSession", "InteractionSessionLocationId", "dbo.InteractionSessionLocation");
            DropForeignKey("dbo.InteractionSessionLocation", "RegionValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.InteractionSessionLocation", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractionSessionLocation", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractionSessionLocation", "CountryValueId", "dbo.DefinedValue");
            DropIndex("dbo.InteractionSessionLocation", new[] { "Guid" });
            DropIndex("dbo.InteractionSessionLocation", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.InteractionSessionLocation", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.InteractionSessionLocation", new[] { "RegionValueId" });
            DropIndex("dbo.InteractionSessionLocation", new[] { "CountryValueId" });
            DropIndex("dbo.InteractionSession", new[] { "InteractionSessionLocationId" });
            DropColumn("dbo.InteractionSession", "InteractionSessionLocationId");
            DropColumn("dbo.InteractionSession", "DurationLastCalculatedDateTime");
            DropColumn("dbo.InteractionSession", "DurationSeconds");
            DropColumn("dbo.InteractionSession", "SessionStartDateKey");
            DropColumn("dbo.Site", "EnablePageViewGeoTracking");
            DropTable("dbo.InteractionSessionLocation");
        }
    }
}
