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
    public partial class MetricPartitions : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.Metric", "EntityTypeId", "dbo.EntityType");
            DropIndex("dbo.Metric", new[] { "EntityTypeId" });
            DropIndex("dbo.MetricValue", new[] { "EntityId" });
            CreateTable(
                "dbo.MetricPartition",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MetricId = c.Int(nullable: false),
                        Label = c.String(maxLength: 100),
                        EntityTypeId = c.Int(),
                        IsRequired = c.Boolean(nullable: false),
                        Order = c.Int(nullable: false),
                        EntityTypeQualifierColumn = c.String(maxLength: 50),
                        EntityTypeQualifierValue = c.String(maxLength: 200),
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
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.Metric", t => t.MetricId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.MetricId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId)
                .Index(t => t.ForeignGuid)
                .Index(t => t.ForeignKey);
            
            CreateTable(
                "dbo.MetricValuePartition",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MetricPartitionId = c.Int(nullable: false),
                        MetricValueId = c.Int(nullable: false),
                        EntityId = c.Int(),
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
                .ForeignKey("dbo.MetricPartition", t => t.MetricPartitionId, cascadeDelete: true)
                .ForeignKey("dbo.MetricValue", t => t.MetricValueId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.MetricPartitionId)
                .Index(t => t.MetricValueId)
                .Index(t => t.EntityId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId)
                .Index(t => t.ForeignGuid)
                .Index(t => t.ForeignKey);

            // upgrade the old Metric data into a Single Partition (note EntityTypeId can be null but we'll still need the partition)
            Sql( @"
INSERT INTO [dbo].[MetricPartition] (
    [MetricId]
    ,[Label]
    ,[EntityTypeId]
    ,[IsRequired]
    ,[Order]
    ,[Guid]
    )
SELECT m.Id
    ,et.FriendlyName [Label]
    ,EntityTypeId
    ,CASE 
        WHEN EntityTypeId IS NULL
            THEN 0
        ELSE 1
        END
    ,0
    ,newid()
FROM Metric m
LEFT JOIN EntityType et ON et.Id = m.EntityTypeId
WHERE m.Id NOT IN (
        SELECT MetricId
        FROM MetricPartition
        )" );

            Sql( @"
INSERT INTO MetricValuePartition (
    MetricPartitionId
    ,MetricValueId
    ,EntityId
    ,[Guid]
    )
SELECT mp.Id
    ,mv.Id
    ,mv.EntityId
    ,newid()
FROM MetricValue mv
JOIN MetricPartition mp ON mp.MetricId = mv.MetricId
WHERE mv.Id NOT IN (
        SELECT MetricValueId
        FROM MetricValuePartition
        )
" );
            
            DropColumn("dbo.Metric", "EntityTypeId");
            DropColumn("dbo.MetricValue", "Order");
            DropColumn("dbo.MetricValue", "EntityId");

            RockMigrationHelper.UpdateFieldType( "Schedule", "", "Rock", "Rock.Field.Types.ScheduleFieldType", "E9C12C59-98EA-4977-8318-647435BE9A9C" );

            RockMigrationHelper.UpdateEntityTypeSingleValueFieldType( "Rock.Model.DefinedValue", Rock.SystemGuid.FieldType.DEFINED_VALUE );
            RockMigrationHelper.UpdateEntityTypeSingleValueFieldType( "Rock.Model.Category", Rock.SystemGuid.FieldType.CATEGORY );
            RockMigrationHelper.UpdateEntityTypeSingleValueFieldType( "Rock.Model.Schedule", Rock.SystemGuid.FieldType.SCHEDULE );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.MetricValue", "EntityId", c => c.Int());
            AddColumn("dbo.MetricValue", "Order", c => c.Int(nullable: false));
            AddColumn("dbo.Metric", "EntityTypeId", c => c.Int());
            DropForeignKey("dbo.MetricValuePartition", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MetricValuePartition", "MetricValueId", "dbo.MetricValue");
            DropForeignKey("dbo.MetricValuePartition", "MetricPartitionId", "dbo.MetricPartition");
            DropForeignKey("dbo.MetricValuePartition", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MetricPartition", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MetricPartition", "MetricId", "dbo.Metric");
            DropForeignKey("dbo.MetricPartition", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.MetricPartition", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.MetricValuePartition", new[] { "ForeignKey" });
            DropIndex("dbo.MetricValuePartition", new[] { "ForeignGuid" });
            DropIndex("dbo.MetricValuePartition", new[] { "ForeignId" });
            DropIndex("dbo.MetricValuePartition", new[] { "Guid" });
            DropIndex("dbo.MetricValuePartition", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.MetricValuePartition", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.MetricValuePartition", new[] { "EntityId" });
            DropIndex("dbo.MetricValuePartition", new[] { "MetricValueId" });
            DropIndex("dbo.MetricValuePartition", new[] { "MetricPartitionId" });
            DropIndex("dbo.MetricPartition", new[] { "ForeignKey" });
            DropIndex("dbo.MetricPartition", new[] { "ForeignGuid" });
            DropIndex("dbo.MetricPartition", new[] { "ForeignId" });
            DropIndex("dbo.MetricPartition", new[] { "Guid" });
            DropIndex("dbo.MetricPartition", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.MetricPartition", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.MetricPartition", new[] { "EntityTypeId" });
            DropIndex("dbo.MetricPartition", new[] { "MetricId" });
            DropTable("dbo.MetricValuePartition");
            DropTable("dbo.MetricPartition");
            CreateIndex("dbo.MetricValue", "EntityId");
            CreateIndex("dbo.Metric", "EntityTypeId");
            AddForeignKey("dbo.Metric", "EntityTypeId", "dbo.EntityType", "Id");
        }
    }
}
