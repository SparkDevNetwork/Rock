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
    public partial class UpdateIndexNames : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'EntityAttribute' AND object_id = OBJECT_ID('AttributeValue'))
BEGIN
	EXECUTE sp_rename @objname = N'dbo.AttributeValue.EntityAttribute', @newname = N'IX_EntityId_AttributeId', @objtype = N'INDEX'
END

IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'FirstLastName' AND object_id = OBJECT_ID('Person'))
BEGIN
	EXECUTE sp_rename @objname = N'dbo.Person.FirstLastName', @newname = N'IX_IsDeceased_FirstName_LastName', @objtype = N'INDEX'
END

IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'LastFirstName' AND object_id = OBJECT_ID('Person'))
BEGIN
	EXECUTE sp_rename @objname = N'dbo.Person.LastFirstName', @newname = N'IX_IsDeceased_LastName_FirstName', @objtype = N'INDEX'
END

IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'IDX_LastName' AND object_id = OBJECT_ID('PersonPreviousName'))
BEGIN
	EXECUTE sp_rename @objname = N'dbo.PersonPreviousName.IDX_LastName', @newname = N'IX_LastName', @objtype = N'INDEX'
END

IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'IDX_SearchTypeValueIdSearchValue' AND object_id = OBJECT_ID('PersonSearchKey'))
BEGIN
	EXECUTE sp_rename @objname = N'dbo.PersonSearchKey.IDX_SearchTypeValueIdSearchValue', @newname = N'IX_SearchTypeValueId_SearchValue', @objtype = N'INDEX'
END

IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'IDX_GroupRequirementTypeGroup' AND object_id = OBJECT_ID('GroupRequirement'))
BEGIN
	EXECUTE sp_rename @objname = N'dbo.GroupRequirement.IDX_GroupRequirementTypeGroup', @newname = N'IX_GroupId_GroupTypeId_GroupRequirementTypeId_GroupRoleId', @objtype = N'INDEX'
END

IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'IDX_SourceEntityTypeIdSourceEntityIdTargetEntityTypeIdTargetEntityIdPurposeKey' AND object_id = OBJECT_ID('RelatedEntity'))
BEGIN
	EXECUTE sp_rename @objname = N'dbo.RelatedEntity.IDX_SourceEntityTypeIdSourceEntityIdTargetEntityTypeIdTargetEntityIdPurposeKey', @newname = N'IX_SourceEntityTypeId_SourceEntityId_TargetEntityTypeId_TargetEntityId_PurposeKey', @objtype = N'INDEX'
END
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_EntityId_AttributeId' AND object_id = OBJECT_ID('AttributeValue'))
BEGIN
	EXECUTE sp_rename @objname = N'dbo.AttributeValue.IX_EntityId_AttributeId', @newname = N'EntityAttribute', @objtype = N'INDEX'
END

IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_IsDeceased_FirstName_LastName' AND object_id = OBJECT_ID('Person'))
BEGIN
	EXECUTE sp_rename @objname = N'dbo.Person.IX_IsDeceased_FirstName_LastName', @newname = N'FirstLastName', @objtype = N'INDEX'
END

IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_IsDeceased_LastName_FirstName' AND object_id = OBJECT_ID('Person'))
BEGIN
	EXECUTE sp_rename @objname = N'dbo.Person.IX_IsDeceased_LastName_FirstName', @newname = N'LastFirstName', @objtype = N'INDEX'
END

IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_LastName' AND object_id = OBJECT_ID('PersonPreviousName'))
BEGIN
	EXECUTE sp_rename @objname = N'dbo.PersonPreviousName.IX_LastName', @newname = N'IDX_LastName', @objtype = N'INDEX'
END

IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_SearchTypeValueId_SearchValue' AND object_id = OBJECT_ID('PersonSearchKey'))
BEGIN
	EXECUTE sp_rename @objname = N'dbo.PersonSearchKey.IX_SearchTypeValueId_SearchValue', @newname = N'IDX_SearchTypeValueIdSearchValue', @objtype = N'INDEX'
END

IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_GroupId_GroupTypeId_GroupRequirementTypeId_GroupRoleId' AND object_id = OBJECT_ID('GroupRequirement'))
BEGIN
	EXECUTE sp_rename @objname = N'dbo.GroupRequirement.IX_GroupId_GroupTypeId_GroupRequirementTypeId_GroupRoleId', @newname = N'IDX_GroupRequirementTypeGroup', @objtype = N'INDEX'
END

IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_SourceEntityTypeId_SourceEntityId_TargetEntityTypeId_TargetEntityId_PurposeKey' AND object_id = OBJECT_ID('RelatedEntity'))
BEGIN
	EXECUTE sp_rename @objname = N'dbo.RelatedEntity.IX_SourceEntityTypeId_SourceEntityId_TargetEntityTypeId_TargetEntityId_PurposeKey', @newname = N'IDX_SourceEntityTypeIdSourceEntityIdTargetEntityTypeIdTargetEntityIdPurposeKey', @objtype = N'INDEX'
END
" );
        }
    }
}
