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
    public partial class BinaryFileSecurity : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.BinaryFileType", "RequiresSecurity", c => c.Boolean(nullable: false));
            Sql("UPDATE [BinaryFileType] SET [RequiresSecurity] = 1 WHERE [Guid] = '6D18A9C4-34AB-444A-B95B-C644019465AC'");

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
		, bft.[RequiresSecurity]
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

            AddSecurityAuthForBinaryFileType( "6D18A9C4-34AB-444A-B95B-C644019465AC", 0, Rock.Security.Authorization.VIEW, true, "6246A7EF-B7A3-4C8C-B1E4-3FF114B84559" /* Finance Admin */ , Rock.Model.SpecialRole.None, "47B92173-B3C2-4F13-9EF6-59CFA2337486" );
            AddSecurityAuthForBinaryFileType( "6D18A9C4-34AB-444A-B95B-C644019465AC", 1, Rock.Security.Authorization.VIEW, true, "2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9" /* Finance Users */, Rock.Model.SpecialRole.None, "76206834-C408-4118-820E-65BE24D7D235" );
            AddSecurityAuthForBinaryFileType( "6D18A9C4-34AB-444A-B95B-C644019465AC", 2, Rock.Security.Authorization.VIEW, false, null, Rock.Model.SpecialRole.AllUsers, "6C0EC7E2-271E-4DA9-A8A7-C4C4B0840E86" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.BinaryFileType", "RequiresSecurity");

            DeleteSecurityAuth( "47B92173-B3C2-4F13-9EF6-59CFA2337486" );
            DeleteSecurityAuth( "76206834-C408-4118-820E-65BE24D7D235" );
            DeleteSecurityAuth( "6C0EC7E2-271E-4DA9-A8A7-C4C4B0840E86" );

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
    }
}
