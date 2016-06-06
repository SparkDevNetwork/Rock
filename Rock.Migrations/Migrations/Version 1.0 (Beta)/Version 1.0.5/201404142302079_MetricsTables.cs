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
    public partial class MetricsTables : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // drop the old Metrics tables
            DropForeignKey( "dbo.Metric", "CollectionFrequencyValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.Metric", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.MetricValue", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.MetricValue", "MetricId", "dbo.Metric" );
            DropForeignKey( "dbo.MetricValue", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Metric", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropIndex( "dbo.Metric", new[] { "CollectionFrequencyValueId" } );
            DropIndex( "dbo.Metric", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.Metric", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.Metric", new[] { "Guid" } );
            DropIndex( "dbo.MetricValue", new[] { "MetricId" } );
            DropIndex( "dbo.MetricValue", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.MetricValue", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.MetricValue", new[] { "Guid" } );
            DropTable( "dbo.Metric" );
            DropTable( "dbo.MetricValue" );

            // create the new metrics tables
            CreateTable(
                "dbo.Metric",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    IsSystem = c.Boolean( nullable: false ),
                    Title = c.String( nullable: false, maxLength: 100 ),
                    Subtitle = c.String( maxLength: 100 ),
                    Description = c.String(),
                    IconCssClass = c.String(),
                    IsCumulative = c.Boolean( nullable: false ),
                    SourceValueTypeId = c.Int(),
                    SourceSql = c.String(),
                    DataViewId = c.Int(),
                    XAxisLabel = c.String(),
                    YAxisLabel = c.String(),
                    StewardPersonAliasId = c.Int(),
                    AdminPersonAliasId = c.Int(),
                    ScheduleId = c.Int(),
                    LastRunDateTime = c.DateTime(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.String( maxLength: 50 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.AdminPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.DataView", t => t.DataViewId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.Schedule", t => t.ScheduleId )
                .ForeignKey( "dbo.DefinedValue", t => t.SourceValueTypeId )
                .ForeignKey( "dbo.PersonAlias", t => t.StewardPersonAliasId )
                .Index( t => t.SourceValueTypeId )
                .Index( t => t.DataViewId )
                .Index( t => t.StewardPersonAliasId )
                .Index( t => t.AdminPersonAliasId )
                .Index( t => t.ScheduleId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.MetricValue",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    MetricValueType = c.Int( nullable: false ),
                    XValue = c.String( nullable: false ),
                    YValue = c.Decimal( precision: 18, scale: 2 ),
                    Order = c.Int( nullable: false ),
                    MetricId = c.Int( nullable: false ),
                    Note = c.String(),
                    MetricValueDateTime = c.DateTime(),
                    CampusId = c.Int(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.String( maxLength: 50 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.Campus", t => t.CampusId )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.Metric", t => t.MetricId, cascadeDelete: true )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.MetricId )
                .Index( t => t.CampusId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.MetricCategory",
                c => new
                {
                    MetricId = c.Int( nullable: false ),
                    CategoryId = c.Int( nullable: false ),
                    Order = c.Int( nullable: false ),
                } )
                .PrimaryKey( t => new { t.MetricId, t.CategoryId } )
                .ForeignKey( "dbo.Metric", t => t.MetricId, cascadeDelete: true )
                .ForeignKey( "dbo.Category", t => t.CategoryId, cascadeDelete: true )
                .Index( t => t.MetricId )
                .Index( t => t.CategoryId );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // drop the new Metrics tables
            DropForeignKey( "dbo.MetricCategory", "CategoryId", "dbo.Category" );
            DropForeignKey( "dbo.MetricCategory", "MetricId", "dbo.Metric" );
            DropIndex( "dbo.MetricCategory", new[] { "CategoryId" } );
            DropIndex( "dbo.MetricCategory", new[] { "MetricId" } );
            DropTable( "dbo.MetricCategory" );
            DropForeignKey( "dbo.Metric", "StewardPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Metric", "SourceValueTypeId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.Metric", "ScheduleId", "dbo.Schedule" );
            DropForeignKey( "dbo.Metric", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.MetricValue", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.MetricValue", "MetricId", "dbo.Metric" );
            DropForeignKey( "dbo.MetricValue", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.MetricValue", "CampusId", "dbo.Campus" );
            DropForeignKey( "dbo.Metric", "DataViewId", "dbo.DataView" );
            DropForeignKey( "dbo.Metric", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Metric", "AdminPersonAliasId", "dbo.PersonAlias" );
            DropIndex( "dbo.MetricValue", new[] { "Guid" } );
            DropIndex( "dbo.MetricValue", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.MetricValue", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.MetricValue", new[] { "CampusId" } );
            DropIndex( "dbo.MetricValue", new[] { "MetricId" } );
            DropIndex( "dbo.Metric", new[] { "Guid" } );
            DropIndex( "dbo.Metric", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.Metric", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.Metric", new[] { "ScheduleId" } );
            DropIndex( "dbo.Metric", new[] { "AdminPersonAliasId" } );
            DropIndex( "dbo.Metric", new[] { "StewardPersonAliasId" } );
            DropIndex( "dbo.Metric", new[] { "DataViewId" } );
            DropIndex( "dbo.Metric", new[] { "SourceValueTypeId" } );
            DropTable( "dbo.MetricValue" );
            DropTable( "dbo.Metric" );

            // re-create the old metrics tables
            CreateTable(
                "dbo.MetricValue",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    IsSystem = c.Boolean( nullable: false ),
                    MetricId = c.Int( nullable: false ),
                    Value = c.String( nullable: false, maxLength: 100 ),
                    Description = c.String(),
                    xValue = c.String( nullable: false ),
                    isDateBased = c.Boolean( nullable: false ),
                    Label = c.String(),
                    Order = c.Int( nullable: false ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.String( maxLength: 50 ),
                } )
                .PrimaryKey( t => t.Id );

            CreateTable(
                "dbo.Metric",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    IsSystem = c.Boolean( nullable: false ),
                    Type = c.Boolean( nullable: false ),
                    Category = c.String( maxLength: 100 ),
                    Title = c.String( nullable: false, maxLength: 100 ),
                    Subtitle = c.String( maxLength: 100 ),
                    Description = c.String(),
                    MinValue = c.Int(),
                    MaxValue = c.Int(),
                    CollectionFrequencyValueId = c.Int(),
                    LastCollectedDateTime = c.DateTime(),
                    Source = c.String( maxLength: 100 ),
                    SourceSQL = c.String(),
                    Order = c.Int( nullable: false ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.String( maxLength: 50 ),
                } )
                .PrimaryKey( t => t.Id );

            CreateIndex( "dbo.MetricValue", "Guid", unique: true );
            CreateIndex( "dbo.MetricValue", "ModifiedByPersonAliasId" );
            CreateIndex( "dbo.MetricValue", "CreatedByPersonAliasId" );
            CreateIndex( "dbo.MetricValue", "MetricId" );
            CreateIndex( "dbo.Metric", "Guid", unique: true );
            CreateIndex( "dbo.Metric", "ModifiedByPersonAliasId" );
            CreateIndex( "dbo.Metric", "CreatedByPersonAliasId" );
            CreateIndex( "dbo.Metric", "CollectionFrequencyValueId" );
            AddForeignKey( "dbo.Metric", "ModifiedByPersonAliasId", "dbo.PersonAlias", "Id" );
            AddForeignKey( "dbo.MetricValue", "ModifiedByPersonAliasId", "dbo.PersonAlias", "Id" );
            AddForeignKey( "dbo.MetricValue", "MetricId", "dbo.Metric", "Id", cascadeDelete: true );
            AddForeignKey( "dbo.MetricValue", "CreatedByPersonAliasId", "dbo.PersonAlias", "Id" );
            AddForeignKey( "dbo.Metric", "CreatedByPersonAliasId", "dbo.PersonAlias", "Id" );
            AddForeignKey( "dbo.Metric", "CollectionFrequencyValueId", "dbo.DefinedValue", "Id" );
        }
    }
}
