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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class BinaryFileTypeViewSecurity : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.BinaryFileType", "RequiresViewSecurity", c => c.Boolean( nullable: false ) );
            Sql( "UPDATE [BinaryFileType] SET [RequiresViewSecurity] = [RequiresSecurity]" );
            DropColumn( "dbo.BinaryFileType", "RequiresSecurity" );

            // Grant Edit to 'All Users' for all of our BinaryFileTypes except for CONTRIBUTION_IMAGE
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( Rock.SystemGuid.BinaryFiletype.DEFAULT, 0, Rock.Security.Authorization.EDIT, true, null, Rock.Model.SpecialRole.AllUsers, "949C5C8C-4F0B-4FC7-B90E-F266DAA4C9CE" );
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( Rock.SystemGuid.BinaryFiletype.CHECKIN_LABEL, 0, Rock.Security.Authorization.EDIT, true, null, Rock.Model.SpecialRole.AllUsers, "33E28A2C-7DD6-44B0-AA21-47194CEA6540" );
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( Rock.SystemGuid.BinaryFiletype.PERSON_IMAGE, 0, Rock.Security.Authorization.EDIT, true, null, Rock.Model.SpecialRole.AllUsers, "4E36C69E-86E4-4ADB-A3BC-35FFE79130BE" );
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( Rock.SystemGuid.BinaryFiletype.LOCATION_IMAGE, 0, Rock.Security.Authorization.EDIT, true, null, Rock.Model.SpecialRole.AllUsers, "7719D542-26AE-405B-A0D7-6CBC960432D9" );
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( Rock.SystemGuid.BinaryFiletype.CONTENT_CHANNEL_ITEM_IMAGE, 0, Rock.Security.Authorization.EDIT, true, null, Rock.Model.SpecialRole.AllUsers, "DBD25052-0ED1-4DC1-8327-42CEBFEC83AD" );

            // Grant Edit to 'Finance Administrators and Finance Users' for CONTRIBUTION_IMAGE (Rock Admins already have it because of GlobalDefault)
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( Rock.SystemGuid.BinaryFiletype.CONTRIBUTION_IMAGE, 0, Rock.Security.Authorization.EDIT, true, Rock.SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS, Rock.Model.SpecialRole.None, "B1E38251-E0EA-482F-B706-6A8D996726F1" );
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( Rock.SystemGuid.BinaryFiletype.CONTRIBUTION_IMAGE, 1, Rock.Security.Authorization.EDIT, true, Rock.SystemGuid.Group.GROUP_FINANCE_USERS, Rock.Model.SpecialRole.None, "596BE5F2-BC90-472B-8501-FA0885A180D5" );

            Sql( @" DECLARE @ObjectName varchar(50) = 'spCore_BinaryFileGet'
                    DECLARE @AlterSql nvarchar(MAX)
                    DECLARE @SchemaOwner varchar(12) = (SELECT TOP 1 s.name
										                    FROM sys.schemas AS s
											                    INNER JOIN sys.all_objects AS o ON s.[schema_id] = o.[schema_id]
										                    WHERE o.name = @ObjectName)

                    IF (@SchemaOwner != 'dbo')
	                    BEGIN
		                    SELECT @AlterSql = 'ALTER SCHEMA dbo TRANSFER [' + @SchemaOwner + '].' + @ObjectName
		                    EXEC sp_executesql @AlterSql
	                    END" );

            Sql( @"
-- create stored proc that retrieves a binaryfile record
/*
<doc>
	<summary>
 		This function returns the BinaryFile for a given Id or Guid, depending on which is specified
	</summary>

	<returns>
		* BinaryFile record
	</returns>
	<param name=""Id"" datatype=""int"">The binary id to use</param>
	<param name=""Guid"" datatype=""uniqueidentifier"">The binaryfile guid to use</param>
	<remarks>	
	</remarks>
	<code>
		EXEC [dbo].[spCore_BinaryFileGet] 14, null -- car-promo.jpg
	</code>
</doc>
*/
ALTER PROCEDURE [dbo].[spCore_BinaryFileGet]
    @Id int
    , @Guid uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    /* NOTE!: Column Order cannot be changed without changing BinaryFileService.partial.cs due to CommandBehavior.SequentialAccess */
    SELECT 
        bf.[Id]
        , bf.[IsTemporary] 
        , bf.[IsSystem]
        , bf.[BinaryFileTypeId]
		, bft.[RequiresViewSecurity]
        , bf.[Url]
        , bf.[FileName] 
        , bf.[MimeType]
        , bf.[ModifiedDateTime]
        , bf.[Description]
        , bf.[StorageEntityTypeId]
        , bf.[Guid]
        /* if the BinaryFile as StorageEntityTypeId set, use that. Otherwise use the default StorageEntityTypeId from BinaryFileType  */
        , COALESCE (bfse.[Name],bftse.[Name] ) as [StorageEntityTypeName]
        , bfd.[Content]
    FROM 
        [BinaryFile] bf 
    LEFT JOIN 
        [BinaryFileData] bfd ON bf.[Id] = bfd.[Id]
    LEFT JOIN 
        [EntityType] bfse ON bf.[StorageEntityTypeId] = bfse.[Id]
    LEFT JOIN 
        [BinaryFileType] bft ON bf.[BinaryFileTypeId] = bft.[Id]
    LEFT JOIN 
        [EntityType] bftse ON bft.[StorageEntityTypeId] = bftse.[Id]
    WHERE 
        (@Id > 0 and bf.[Id] = @Id)
        or
        (bf.[Guid] = @Guid)
END
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn( "dbo.BinaryFileType", "RequiresSecurity", c => c.Boolean( nullable: false ) );
            Sql( "UPDATE [BinaryFileType] SET [RequiresSecurity] = [RequiresViewSecurity]" );
            DropColumn( "dbo.BinaryFileType", "RequiresViewSecurity" );

            // Un-Grant Edit to 'All Users' for all of our BinaryFileTypes except for CONTRIBUTION_IMAGE
            RockMigrationHelper.DeleteSecurityAuth( "949C5C8C-4F0B-4FC7-B90E-F266DAA4C9CE" );
            RockMigrationHelper.DeleteSecurityAuth( "33E28A2C-7DD6-44B0-AA21-47194CEA6540" );
            RockMigrationHelper.DeleteSecurityAuth( "4E36C69E-86E4-4ADB-A3BC-35FFE79130BE" );
            RockMigrationHelper.DeleteSecurityAuth( "7719D542-26AE-405B-A0D7-6CBC960432D9" );
            RockMigrationHelper.DeleteSecurityAuth( "DBD25052-0ED1-4DC1-8327-42CEBFEC83AD" );

            // Un-Grant Edit to 'Finance Administrators and Finance Users' for CONTRIBUTION_IMAGE (Rock Admins already have it because of GlobalDefault)
            RockMigrationHelper.DeleteSecurityAuth( "B1E38251-E0EA-482F-B706-6A8D996726F1" );
            RockMigrationHelper.DeleteSecurityAuth( "596BE5F2-BC90-472B-8501-FA0885A180D5" );
        }
    }
}
