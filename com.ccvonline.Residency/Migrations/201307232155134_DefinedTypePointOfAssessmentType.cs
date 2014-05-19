// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace com.ccvonline.Residency.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class DefinedTypePointOfAssessmentType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo._com_ccvonline_Residency_ProjectPointOfAssessment", "CompetencyTypeValueId", "dbo.DefinedValue");
            DropIndex("dbo._com_ccvonline_Residency_ProjectPointOfAssessment", new[] { "CompetencyTypeValueId" });
            AddColumn("dbo._com_ccvonline_Residency_ProjectPointOfAssessment", "PointOfAssessmentTypeValueId", c => c.Int());
            CreateIndex("dbo._com_ccvonline_Residency_ProjectPointOfAssessment", "PointOfAssessmentTypeValueId");
            AddForeignKey("dbo._com_ccvonline_Residency_ProjectPointOfAssessment", "PointOfAssessmentTypeValueId", "dbo.DefinedValue", "Id");
            DropColumn("dbo._com_ccvonline_Residency_ProjectPointOfAssessment", "CompetencyTypeValueId");

            Sql( "update [DefinedType] set [Name] = 'Residency Point of Assessment Type', [Description] = 'Used by the ccvonline Residency plugin to be assigned to a Residency Point of Assessment' where [Guid] = '338A8802-4022-404F-9FA2-150F1FB3838F'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo._com_ccvonline_Residency_ProjectPointOfAssessment", "CompetencyTypeValueId", c => c.Int());
            DropForeignKey("dbo._com_ccvonline_Residency_ProjectPointOfAssessment", "PointOfAssessmentTypeValueId", "dbo.DefinedValue");
            DropIndex("dbo._com_ccvonline_Residency_ProjectPointOfAssessment", new[] { "PointOfAssessmentTypeValueId" });
            DropColumn("dbo._com_ccvonline_Residency_ProjectPointOfAssessment", "PointOfAssessmentTypeValueId");
            CreateIndex("dbo._com_ccvonline_Residency_ProjectPointOfAssessment", "CompetencyTypeValueId");
            AddForeignKey("dbo._com_ccvonline_Residency_ProjectPointOfAssessment", "CompetencyTypeValueId", "dbo.DefinedValue", "Id");

            Sql( "update [DefinedType] set [Name] = 'Residency Competency Type', [Description] = 'Used by the ccvonline Residency plugin to be assigned to a Residency Competency' where [Guid] = '338A8802-4022-404F-9FA2-150F1FB3838F'" );
        }
    }
}
/* Skipped Operations for tables that are not part of ResidencyContext: Review these comments to verify the proper things were skipped */
/* To disable skipping, edit your Migrations\Configuration.cs so that CodeGenerator = new RockCSharpMigrationCodeGenerator<ResidencyContext>(false); */

// Up()...
// DropForeignKeyOperation for TableName GroupTypeGroupType, column GroupType_Id.
// DropForeignKeyOperation for TableName GroupTypeGroupType, column GroupType_Id1.
// DropIndexOperation for TableName GroupTypeGroupType, column GroupType_Id.
// DropIndexOperation for TableName GroupTypeGroupType, column GroupType_Id1.
// RenameColumnOperation for TableName Location, column Device_Id.
// AddColumnOperation for TableName GroupMember, column GroupMemberStatus.
// AddColumnOperation for TableName GroupType, column InheritedGroupTypeId.
// AddColumnOperation for TableName GroupType, column LocationType.
// AddColumnOperation for TableName GroupType, column GroupType_Id.
// AddColumnOperation for TableName GroupType, column GroupType_Id1.
// AddColumnOperation for TableName Location, column GeoPoint.
// AddColumnOperation for TableName Location, column GeoFence.
// AddColumnOperation for TableName Device, column LocationId.
// CreateIndexOperation for TableName GroupType, column GroupType_Id.
// CreateIndexOperation for TableName GroupType, column GroupType_Id1.
// CreateIndexOperation for TableName GroupType, column InheritedGroupTypeId.
// CreateIndexOperation for TableName Device, column LocationId.
// CreateIndexOperation for TableName Location, column PrinterDeviceId.
// AddForeignKeyOperation for TableName GroupType, column GroupType_Id.
// AddForeignKeyOperation for TableName GroupType, column GroupType_Id1.
// AddForeignKeyOperation for TableName GroupType, column InheritedGroupTypeId.
// AddForeignKeyOperation for TableName Device, column LocationId.
// AddForeignKeyOperation for TableName Location, column PrinterDeviceId.
// DropColumnOperation for TableName Location, column LocationPoint.
// DropColumnOperation for TableName Location, column Perimeter.
// DropColumnOperation for TableName Device, column GeoPoint.
// DropColumnOperation for TableName Device, column GeoFence.
// DropTableOperation for TableName GroupTypeGroupType.

// Down()...
// CreateTableOperation for TableName GroupTypeGroupType.
// AddColumnOperation for TableName Device, column GeoFence.
// AddColumnOperation for TableName Device, column GeoPoint.
// AddColumnOperation for TableName Location, column Perimeter.
// AddColumnOperation for TableName Location, column LocationPoint.
// DropForeignKeyOperation for TableName Location, column PrinterDeviceId.
// DropForeignKeyOperation for TableName Device, column LocationId.
// DropForeignKeyOperation for TableName GroupType, column InheritedGroupTypeId.
// DropForeignKeyOperation for TableName GroupType, column GroupType_Id1.
// DropForeignKeyOperation for TableName GroupType, column GroupType_Id.
// DropIndexOperation for TableName Location, column PrinterDeviceId.
// DropIndexOperation for TableName Device, column LocationId.
// DropIndexOperation for TableName GroupType, column InheritedGroupTypeId.
// DropIndexOperation for TableName GroupType, column GroupType_Id1.
// DropIndexOperation for TableName GroupType, column GroupType_Id.
// DropColumnOperation for TableName Device, column LocationId.
// DropColumnOperation for TableName Location, column GeoFence.
// DropColumnOperation for TableName Location, column GeoPoint.
// DropColumnOperation for TableName GroupType, column GroupType_Id1.
// DropColumnOperation for TableName GroupType, column GroupType_Id.
// DropColumnOperation for TableName GroupType, column LocationType.
// DropColumnOperation for TableName GroupType, column InheritedGroupTypeId.
// DropColumnOperation for TableName GroupMember, column GroupMemberStatus.
// RenameColumnOperation for TableName Location, column PrinterDeviceId.
// CreateIndexOperation for TableName GroupTypeGroupType, column GroupType_Id1.
// CreateIndexOperation for TableName GroupTypeGroupType, column GroupType_Id.
// AddForeignKeyOperation for TableName GroupTypeGroupType, column GroupType_Id1.
// AddForeignKeyOperation for TableName GroupTypeGroupType, column GroupType_Id.
