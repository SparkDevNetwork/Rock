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
    public partial class MetricEntityType : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.MetricValue", "CampusId", "dbo.Campus");
            DropIndex("dbo.MetricValue", new[] { "CampusId" });
            RenameColumn(table: "dbo.Metric", name: "StewardPersonAliasId", newName: "MetricChampionPersonAliasId");
            RenameIndex(table: "dbo.Metric", name: "IX_StewardPersonAliasId", newName: "IX_MetricChampionPersonAliasId");
            AddColumn("dbo.EntityType", "SingleValueFieldTypeId", c => c.Int());
            AddColumn("dbo.EntityType", "MultiValueFieldTypeId", c => c.Int());
            AddColumn("dbo.Metric", "EntityTypeId", c => c.Int());
            AddColumn("dbo.MetricValue", "EntityId", c => c.Int());
            CreateIndex("dbo.EntityType", "SingleValueFieldTypeId");
            CreateIndex("dbo.EntityType", "MultiValueFieldTypeId");
            CreateIndex("dbo.Metric", "EntityTypeId");
            CreateIndex("dbo.MetricValue", "MetricValueDateTime");
            CreateIndex("dbo.MetricValue", "EntityId");
            AddForeignKey("dbo.EntityType", "MultiValueFieldTypeId", "dbo.FieldType", "Id");
            AddForeignKey("dbo.EntityType", "SingleValueFieldTypeId", "dbo.FieldType", "Id");
            AddForeignKey("dbo.Metric", "EntityTypeId", "dbo.EntityType", "Id");
            DropColumn("dbo.MetricValue", "CampusId");

            UpdateEntityTypeSingleValueFieldType( "Rock.Model.Campus", Rock.SystemGuid.FieldType.CAMPUS );
            UpdateEntityTypeMultiValueFieldType( "Rock.Model.Campus", Rock.SystemGuid.FieldType.CAMPUSES );

            UpdateEntityTypeSingleValueFieldType( "Rock.Model.Category", Rock.SystemGuid.FieldType.CATEGORY );
            UpdateEntityTypeMultiValueFieldType( "Rock.Model.Category", Rock.SystemGuid.FieldType.CATEGORIES );

            UpdateEntityTypeSingleValueFieldType( "Rock.Model.EntityType", Rock.SystemGuid.FieldType.ENTITYTYPE );

            UpdateEntityTypeSingleValueFieldType( "Rock.Model.FinancialAccount", Rock.SystemGuid.FieldType.FINANCIAL_ACCOUNT );
            UpdateEntityTypeMultiValueFieldType( "Rock.Model.FinancialAccount", Rock.SystemGuid.FieldType.FINANCIAL_ACCOUNTS );

            UpdateEntityTypeSingleValueFieldType( "Rock.Model.Group", Rock.SystemGuid.FieldType.GROUP );

            UpdateEntityTypeSingleValueFieldType( "Rock.Model.GroupTypeRole", Rock.SystemGuid.FieldType.GROUP_ROLE );

            UpdateEntityTypeSingleValueFieldType( "Rock.Model.GroupType", Rock.SystemGuid.FieldType.GROUP_TYPE );
            UpdateEntityTypeMultiValueFieldType( "Rock.Model.GroupType", Rock.SystemGuid.FieldType.GROUP_TYPES );

            UpdateEntityTypeSingleValueFieldType( "Rock.Model.Location", Rock.SystemGuid.FieldType.LOCATION );

            UpdateEntityTypeSingleValueFieldType( "Rock.Model.Person", Rock.SystemGuid.FieldType.PERSON );

            UpdateEntityTypeSingleValueFieldType( "Rock.Model.Site", Rock.SystemGuid.FieldType.SITE );

            UpdateEntityTypeSingleValueFieldType( "Rock.Model.WorkflowType", Rock.SystemGuid.FieldType.WORKFLOW_TYPE );

            // update new location of jobscheduler installer
            Sql( "UPDATE [AttributeValue] set [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/jobscheduler/1.0.6/jobscheduler.exe' where [Guid] = '7FBC4397-6BFD-451D-A6B9-83D7B7265641'" );

            // TestLinqGrid (doesn't exist anymore)
            DeleteBlockType( "584dc3c4-5b58-4467-bf58-3e49fda05655" );

            // Old Metric Admin BlockTypes (don't exist anymore)
            DeleteBlockType( "ae70e6c4-a34c-4d05-a13c-ce0abe2a6b5b" );
            DeleteBlockType( "ce769f0a-722f-4745-a6a8-7f00548cd1d2" );
            DeleteBlockType( "819c3597-4a93-4974-b1e9-4d065989ea25" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.MetricValue", "CampusId", c => c.Int());
            DropForeignKey("dbo.Metric", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.EntityType", "SingleValueFieldTypeId", "dbo.FieldType");
            DropForeignKey("dbo.EntityType", "MultiValueFieldTypeId", "dbo.FieldType");
            DropIndex("dbo.MetricValue", new[] { "EntityId" });
            DropIndex("dbo.MetricValue", new[] { "MetricValueDateTime" });
            DropIndex("dbo.Metric", new[] { "EntityTypeId" });
            DropIndex("dbo.EntityType", new[] { "MultiValueFieldTypeId" });
            DropIndex("dbo.EntityType", new[] { "SingleValueFieldTypeId" });
            DropColumn("dbo.MetricValue", "EntityId");
            DropColumn("dbo.Metric", "EntityTypeId");
            DropColumn("dbo.EntityType", "MultiValueFieldTypeId");
            DropColumn("dbo.EntityType", "SingleValueFieldTypeId");
            RenameIndex(table: "dbo.Metric", name: "IX_MetricChampionPersonAliasId", newName: "IX_StewardPersonAliasId");
            RenameColumn(table: "dbo.Metric", name: "MetricChampionPersonAliasId", newName: "StewardPersonAliasId");
            CreateIndex("dbo.MetricValue", "CampusId");
            AddForeignKey("dbo.MetricValue", "CampusId", "dbo.Campus", "Id");
        }
    }
}
