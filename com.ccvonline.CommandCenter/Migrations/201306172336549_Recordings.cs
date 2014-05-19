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
    public partial class Recordings : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo._com_ccvonline_CommandCenterRecording",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CampusId = c.Int(),
                        Date = c.DateTime(),
                        Label = c.String(maxLength: 100),
                        App = c.String(maxLength: 100),
                        StreamName = c.String(maxLength: 100),
                        RecordingName = c.String(maxLength: 100),
                        StartTime = c.DateTime(),
                        StartResponse = c.String(maxLength: 400),
                        StopTime = c.DateTime(),
                        StopResponse = c.String(maxLength: 400),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Campus", t => t.CampusId)
                .Index(t => t.CampusId);
            
            CreateIndex( "dbo._com_ccvonline_CommandCenterRecording", "Guid", true );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo._com_ccvonline_CommandCenterRecording", "CampusId", "dbo.Campus");
            DropIndex("dbo._com_ccvonline_CommandCenterRecording", new[] { "CampusId" });
            DropTable("dbo._com_ccvonline_CommandCenterRecording");
        }
    }
}
/* Skipped Operations for tables that are not part of CommandCenterContext: Review these comments to verify the proper things were skipped */
/* To disable skipping, edit your Migrations\Configuration.cs so that CodeGenerator = new RockCSharpMigrationCodeGenerator<CommandCenterContext>(false); */

// Up()...
// CreateTableOperation for TableName Campus.
// CreateTableOperation for TableName Group.
// CreateTableOperation for TableName GroupType.
// CreateTableOperation for TableName GroupTypeLocationType.
// CreateTableOperation for TableName DefinedValue.
// CreateTableOperation for TableName DefinedType.
// CreateTableOperation for TableName FieldType.
// CreateTableOperation for TableName BinaryFileType.
// CreateTableOperation for TableName BinaryFileData.
// CreateTableOperation for TableName GroupMember.
// CreateTableOperation for TableName Person.
// CreateTableOperation for TableName UserLogin.
// CreateTableOperation for TableName EmailTemplate.
// CreateTableOperation for TableName PhoneNumber.
// CreateTableOperation for TableName Category.
// CreateTableOperation for TableName EntityType.
// CreateTableOperation for TableName AttendanceCode.
// CreateTableOperation for TableName PersonAccount.
// CreateIndexOperation for TableName Group, column ParentGroupId.
// CreateIndexOperation for TableName Group, column GroupTypeId.
// CreateIndexOperation for TableName GroupTypeLocationType, column GroupTypeId.
// CreateIndexOperation for TableName DefinedValue, column DefinedTypeId.
// CreateIndexOperation for TableName DefinedType, column FieldTypeId.
// CreateIndexOperation for TableName GroupTypeLocationType, column LocationTypeValueId.
// CreateIndexOperation for TableName GroupType, column DefaultGroupRoleId.
// CreateIndexOperation for TableName BinaryFileType, column IconSmallFileId.
// CreateIndexOperation for TableName BinaryFileType, column IconLargeFileId.
// CreateIndexOperation for TableName GroupType, column IconSmallFileId.
// CreateIndexOperation for TableName GroupType, column IconLargeFileId.
// CreateIndexOperation for TableName Group, column CampusId.
// CreateIndexOperation for TableName UserLogin, column PersonId.
// CreateIndexOperation for TableName EmailTemplate, column PersonId.
// CreateIndexOperation for TableName PhoneNumber, column NumberTypeValueId.
// CreateIndexOperation for TableName PhoneNumber, column PersonId.
// CreateIndexOperation for TableName GroupMember, column PersonId.
// CreateIndexOperation for TableName Category, column ParentCategoryId.
// CreateIndexOperation for TableName Category, column EntityTypeId.
// CreateIndexOperation for TableName Category, column IconSmallFileId.
// CreateIndexOperation for TableName Category, column IconLargeFileId.
// CreateIndexOperation for TableName PersonAccount, column PersonId.
// CreateIndexOperation for TableName Person, column MaritalStatusValueId.
// CreateIndexOperation for TableName Person, column PersonStatusValueId.
// CreateIndexOperation for TableName Person, column RecordStatusValueId.
// CreateIndexOperation for TableName Person, column RecordStatusReasonValueId.
// CreateIndexOperation for TableName Person, column RecordTypeValueId.
// CreateIndexOperation for TableName Person, column SuffixValueId.
// CreateIndexOperation for TableName Person, column TitleValueId.
// CreateIndexOperation for TableName Person, column PhotoId.
// CreateIndexOperation for TableName GroupMember, column GroupId.
// CreateIndexOperation for TableName GroupMember, column GroupRoleId.
// CreateIndexOperation for TableName Campus, column LocationId.
// AddForeignKeyOperation for TableName Group, column ParentGroupId.
// AddForeignKeyOperation for TableName Group, column GroupTypeId.
// AddForeignKeyOperation for TableName GroupTypeLocationType, column GroupTypeId.
// AddForeignKeyOperation for TableName DefinedValue, column DefinedTypeId.
// AddForeignKeyOperation for TableName DefinedType, column FieldTypeId.
// AddForeignKeyOperation for TableName GroupTypeLocationType, column LocationTypeValueId.
// AddForeignKeyOperation for TableName GroupType, column DefaultGroupRoleId.
// AddForeignKeyOperation for TableName BinaryFileType, column IconSmallFileId.
// AddForeignKeyOperation for TableName BinaryFileType, column IconLargeFileId.
// AddForeignKeyOperation for TableName GroupType, column IconSmallFileId.
// AddForeignKeyOperation for TableName GroupType, column IconLargeFileId.
// AddForeignKeyOperation for TableName Group, column CampusId.
// AddForeignKeyOperation for TableName UserLogin, column PersonId.
// AddForeignKeyOperation for TableName EmailTemplate, column PersonId.
// AddForeignKeyOperation for TableName PhoneNumber, column NumberTypeValueId.
// AddForeignKeyOperation for TableName PhoneNumber, column PersonId.
// AddForeignKeyOperation for TableName GroupMember, column PersonId.
// AddForeignKeyOperation for TableName Category, column ParentCategoryId.
// AddForeignKeyOperation for TableName Category, column EntityTypeId.
// AddForeignKeyOperation for TableName Category, column IconSmallFileId.
// AddForeignKeyOperation for TableName Category, column IconLargeFileId.
// AddForeignKeyOperation for TableName PersonAccount, column PersonId.
// AddForeignKeyOperation for TableName Person, column MaritalStatusValueId.
// AddForeignKeyOperation for TableName Person, column PersonStatusValueId.
// AddForeignKeyOperation for TableName Person, column RecordStatusValueId.
// AddForeignKeyOperation for TableName Person, column RecordStatusReasonValueId.
// AddForeignKeyOperation for TableName Person, column RecordTypeValueId.
// AddForeignKeyOperation for TableName Person, column SuffixValueId.
// AddForeignKeyOperation for TableName Person, column TitleValueId.
// AddForeignKeyOperation for TableName Person, column PhotoId.
// AddForeignKeyOperation for TableName GroupMember, column GroupId.
// AddForeignKeyOperation for TableName GroupMember, column GroupRoleId.
// AddForeignKeyOperation for TableName Campus, column LocationId.

// Down()...
// DropForeignKeyOperation for TableName Campus, column LocationId.
// DropForeignKeyOperation for TableName GroupMember, column GroupRoleId.
// DropForeignKeyOperation for TableName GroupMember, column GroupId.
// DropForeignKeyOperation for TableName Person, column PhotoId.
// DropForeignKeyOperation for TableName Person, column TitleValueId.
// DropForeignKeyOperation for TableName Person, column SuffixValueId.
// DropForeignKeyOperation for TableName Person, column RecordTypeValueId.
// DropForeignKeyOperation for TableName Person, column RecordStatusReasonValueId.
// DropForeignKeyOperation for TableName Person, column RecordStatusValueId.
// DropForeignKeyOperation for TableName Person, column PersonStatusValueId.
// DropForeignKeyOperation for TableName Person, column MaritalStatusValueId.
// DropForeignKeyOperation for TableName PersonAccount, column PersonId.
// DropForeignKeyOperation for TableName Category, column IconLargeFileId.
// DropForeignKeyOperation for TableName Category, column IconSmallFileId.
// DropForeignKeyOperation for TableName Category, column EntityTypeId.
// DropForeignKeyOperation for TableName Category, column ParentCategoryId.
// DropForeignKeyOperation for TableName GroupMember, column PersonId.
// DropForeignKeyOperation for TableName PhoneNumber, column PersonId.
// DropForeignKeyOperation for TableName PhoneNumber, column NumberTypeValueId.
// DropForeignKeyOperation for TableName EmailTemplate, column PersonId.
// DropForeignKeyOperation for TableName UserLogin, column PersonId.
// DropForeignKeyOperation for TableName Group, column CampusId.
// DropForeignKeyOperation for TableName GroupType, column IconLargeFileId.
// DropForeignKeyOperation for TableName GroupType, column IconSmallFileId.
// DropForeignKeyOperation for TableName BinaryFileType, column IconLargeFileId.
// DropForeignKeyOperation for TableName BinaryFileType, column IconSmallFileId.
// DropForeignKeyOperation for TableName GroupType, column DefaultGroupRoleId.
// DropForeignKeyOperation for TableName GroupTypeLocationType, column LocationTypeValueId.
// DropForeignKeyOperation for TableName DefinedType, column FieldTypeId.
// DropForeignKeyOperation for TableName DefinedValue, column DefinedTypeId.
// DropForeignKeyOperation for TableName GroupTypeLocationType, column GroupTypeId.
// DropForeignKeyOperation for TableName Group, column GroupTypeId.
// DropForeignKeyOperation for TableName Group, column ParentGroupId.
// DropIndexOperation for TableName Campus, column LocationId.
// DropIndexOperation for TableName GroupMember, column GroupRoleId.
// DropIndexOperation for TableName GroupMember, column GroupId.
// DropIndexOperation for TableName Person, column PhotoId.
// DropIndexOperation for TableName Person, column TitleValueId.
// DropIndexOperation for TableName Person, column SuffixValueId.
// DropIndexOperation for TableName Person, column RecordTypeValueId.
// DropIndexOperation for TableName Person, column RecordStatusReasonValueId.
// DropIndexOperation for TableName Person, column RecordStatusValueId.
// DropIndexOperation for TableName Person, column PersonStatusValueId.
// DropIndexOperation for TableName Person, column MaritalStatusValueId.
// DropIndexOperation for TableName PersonAccount, column PersonId.
// DropIndexOperation for TableName Category, column IconLargeFileId.
// DropIndexOperation for TableName Category, column IconSmallFileId.
// DropIndexOperation for TableName Category, column EntityTypeId.
// DropIndexOperation for TableName Category, column ParentCategoryId.
// DropIndexOperation for TableName GroupMember, column PersonId.
// DropIndexOperation for TableName PhoneNumber, column PersonId.
// DropIndexOperation for TableName PhoneNumber, column NumberTypeValueId.
// DropIndexOperation for TableName EmailTemplate, column PersonId.
// DropIndexOperation for TableName UserLogin, column PersonId.
// DropIndexOperation for TableName Group, column CampusId.
// DropIndexOperation for TableName GroupType, column IconLargeFileId.
// DropIndexOperation for TableName GroupType, column IconSmallFileId.
// DropIndexOperation for TableName BinaryFileType, column IconLargeFileId.
// DropIndexOperation for TableName BinaryFileType, column IconSmallFileId.
// DropIndexOperation for TableName GroupType, column DefaultGroupRoleId.
// DropIndexOperation for TableName GroupTypeLocationType, column LocationTypeValueId.
// DropIndexOperation for TableName DefinedType, column FieldTypeId.
// DropIndexOperation for TableName DefinedValue, column DefinedTypeId.
// DropIndexOperation for TableName GroupTypeLocationType, column GroupTypeId.
// DropIndexOperation for TableName Group, column GroupTypeId.
// DropIndexOperation for TableName Group, column ParentGroupId.
// DropTableOperation for TableName PersonAccount.
// DropTableOperation for TableName AttendanceCode.
// DropTableOperation for TableName EntityType.
// DropTableOperation for TableName Category.
// DropTableOperation for TableName PhoneNumber.
// DropTableOperation for TableName EmailTemplate.
// DropTableOperation for TableName UserLogin.
// DropTableOperation for TableName Person.
// DropTableOperation for TableName GroupMember.
// DropTableOperation for TableName BinaryFileData.
// DropTableOperation for TableName BinaryFileType.
// DropTableOperation for TableName FieldType.
// DropTableOperation for TableName DefinedType.
// DropTableOperation for TableName DefinedValue.
// DropTableOperation for TableName GroupTypeLocationType.
// DropTableOperation for TableName GroupType.
// DropTableOperation for TableName Group.
// DropTableOperation for TableName Campus.
