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
namespace com.ccvonline.CommandCenter.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class EmptyForCore : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
/* Skipped Operations for tables that are not part of CommandCenterContext: Review these comments to verify the proper things were skipped */
/* To disable skipping, edit your Migrations\Configuration.cs so that CodeGenerator = new RockCSharpMigrationCodeGenerator<CommandCenterContext>(false); */

// Up()...
// DropForeignKeyOperation for TableName GroupTypeGroupType, column GroupType_Id.
// DropForeignKeyOperation for TableName GroupTypeGroupType, column GroupType_Id1.
// DropForeignKeyOperation for TableName PersonAccount, column PersonId.
// DropIndexOperation for TableName GroupTypeGroupType, column GroupType_Id.
// DropIndexOperation for TableName GroupTypeGroupType, column GroupType_Id1.
// DropIndexOperation for TableName PersonAccount, column PersonId.
// RenameColumnOperation for TableName Location, column Device_Id.
// AddColumnOperation for TableName Campus, column ShortCode.
// AddColumnOperation for TableName Location, column GeoPoint.
// AddColumnOperation for TableName Location, column GeoFence.
// AddColumnOperation for TableName GroupType, column InheritedGroupTypeId.
// AddColumnOperation for TableName GroupType, column LocationSelectionMode.
// AddColumnOperation for TableName GroupType, column GroupType_Id.
// AddColumnOperation for TableName GroupType, column GroupType_Id1.
// AddColumnOperation for TableName BinaryFile, column StorageEntityTypeId.
// AddColumnOperation for TableName BinaryFileType, column StorageEntityTypeId.
// AddColumnOperation for TableName GroupMember, column GroupMemberStatus.
// AddColumnOperation for TableName Device, column LocationId.
// CreateIndexOperation for TableName GroupType, column GroupType_Id.
// CreateIndexOperation for TableName GroupType, column GroupType_Id1.
// CreateIndexOperation for TableName BinaryFileType, column StorageEntityTypeId.
// CreateIndexOperation for TableName BinaryFile, column StorageEntityTypeId.
// CreateIndexOperation for TableName GroupType, column InheritedGroupTypeId.
// CreateIndexOperation for TableName Device, column LocationId.
// CreateIndexOperation for TableName Location, column PrinterDeviceId.
// AddForeignKeyOperation for TableName GroupType, column GroupType_Id.
// AddForeignKeyOperation for TableName GroupType, column GroupType_Id1.
// AddForeignKeyOperation for TableName BinaryFileType, column StorageEntityTypeId.
// AddForeignKeyOperation for TableName BinaryFile, column StorageEntityTypeId.
// AddForeignKeyOperation for TableName GroupType, column InheritedGroupTypeId.
// AddForeignKeyOperation for TableName Device, column LocationId.
// AddForeignKeyOperation for TableName Location, column PrinterDeviceId.
// DropColumnOperation for TableName Location, column LocationPoint.
// DropColumnOperation for TableName Location, column Perimeter.
// DropColumnOperation for TableName Device, column GeoPoint.
// DropColumnOperation for TableName Device, column GeoFence.
// DropTableOperation for TableName PersonAccount.
// DropTableOperation for TableName GroupTypeGroupType.

// Down()...
// CreateTableOperation for TableName GroupTypeGroupType.
// CreateTableOperation for TableName PersonAccount.
// AddColumnOperation for TableName Device, column GeoFence.
// AddColumnOperation for TableName Device, column GeoPoint.
// AddColumnOperation for TableName Location, column Perimeter.
// AddColumnOperation for TableName Location, column LocationPoint.
// DropForeignKeyOperation for TableName Location, column PrinterDeviceId.
// DropForeignKeyOperation for TableName Device, column LocationId.
// DropForeignKeyOperation for TableName GroupType, column InheritedGroupTypeId.
// DropForeignKeyOperation for TableName BinaryFile, column StorageEntityTypeId.
// DropForeignKeyOperation for TableName BinaryFileType, column StorageEntityTypeId.
// DropForeignKeyOperation for TableName GroupType, column GroupType_Id1.
// DropForeignKeyOperation for TableName GroupType, column GroupType_Id.
// DropIndexOperation for TableName Location, column PrinterDeviceId.
// DropIndexOperation for TableName Device, column LocationId.
// DropIndexOperation for TableName GroupType, column InheritedGroupTypeId.
// DropIndexOperation for TableName BinaryFile, column StorageEntityTypeId.
// DropIndexOperation for TableName BinaryFileType, column StorageEntityTypeId.
// DropIndexOperation for TableName GroupType, column GroupType_Id1.
// DropIndexOperation for TableName GroupType, column GroupType_Id.
// DropColumnOperation for TableName Device, column LocationId.
// DropColumnOperation for TableName GroupMember, column GroupMemberStatus.
// DropColumnOperation for TableName BinaryFileType, column StorageEntityTypeId.
// DropColumnOperation for TableName BinaryFile, column StorageEntityTypeId.
// DropColumnOperation for TableName GroupType, column GroupType_Id1.
// DropColumnOperation for TableName GroupType, column GroupType_Id.
// DropColumnOperation for TableName GroupType, column LocationSelectionMode.
// DropColumnOperation for TableName GroupType, column InheritedGroupTypeId.
// DropColumnOperation for TableName Location, column GeoFence.
// DropColumnOperation for TableName Location, column GeoPoint.
// DropColumnOperation for TableName Campus, column ShortCode.
// RenameColumnOperation for TableName Location, column PrinterDeviceId.
// CreateIndexOperation for TableName PersonAccount, column PersonId.
// CreateIndexOperation for TableName GroupTypeGroupType, column GroupType_Id1.
// CreateIndexOperation for TableName GroupTypeGroupType, column GroupType_Id.
// AddForeignKeyOperation for TableName PersonAccount, column PersonId.
// AddForeignKeyOperation for TableName GroupTypeGroupType, column GroupType_Id1.
// AddForeignKeyOperation for TableName GroupTypeGroupType, column GroupType_Id.
