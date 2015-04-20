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
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class BinaryFileStorageSettings : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.BinaryFile", "StorageEntitySettings", c => c.String());
            AddColumn("dbo.BinaryFile", "Url", c => c.String(maxLength: 2083));

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
	<param name='Id' datatype='int'>The binary id to use</param>
	<param name='Guid' datatype='uniqueidentifier'>The binaryfile guid to use</param>
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
        , bf.[FileName] 
        , bf.[MimeType]
        , bf.[ModifiedDateTime]
        , bf.[Description]
        , bf.[StorageEntityTypeId]
        , bf.[Guid]
		, bf.[StorageEntitySettings]
		, bf.[Url]
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

            Sql( @"
    DECLARE @DatabaseEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '0AA42802-04FD-4AEC-B011-FEB127FC85CD' )
    DECLARE @FileSystemEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = 'A97B6002-454E-4890-B529-B99F8F2F376A' )
    DECLARE @AttributeId int 
    DECLARE @DefaultValue nvarchar(max) 

	UPDATE [BinaryFile] SET 
		[StorageEntityTypeId] = @DatabaseEntityTypeId
	WHERE [StorageEntityTypeId] IS NULL

    SELECT TOP 1 
	    @AttributeId = [Id],
	    @DefaultValue = [DefaultValue]
    FROM [Attribute] WHERE [Guid] = '3CAFA34D-9208-439B-A046-CB727FB729DE'

    UPDATE [Attribute]
    SET [Description] = 'The relative path where files should be stored on the file system ( Default: ''App_Data\Files'' ).'
    WHERE [Id] = @AttributeId

    UPDATE F SET 
	    StorageEntitySettings = '{ ""RootPath"": ""' + 
	    REPLACE( REPLACE ( REPLACE ( COALESCE ( AV.[Value], @DefaultValue, '' ), '~/', '' ), '/', '\' ), '\', '\\' ) +
	    '"" }'
    FROM [BinaryFile] F
    INNER JOIN [BinaryFileType] T 
	    ON T.[Id] = F.[BinaryFileTypeId]
    LEFT OUTER JOIN [AttributeValue] AV 
	    ON AV.[EntityId] = T.[Id]
	    AND AV.[AttributeId] = @AttributeId
    WHERE T.[StorageEntityTypeId] = @FileSystemEntityTypeId
    AND F.[StorageEntitySettings] IS NULL

    UPDATE [BinaryFile] SET Url = '~/GetImage.ashx?guid=' + CAST( [Guid] AS varchar(60) )
    WHERE [Url] IS NULL
    AND [MimeType] LIKE 'image%'

    UPDATE [BinaryFile] SET Url = '~/GetFile.ashx?guid=' + CAST( [Guid] AS varchar(60) )
    WHERE [Url] IS NULL
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.BinaryFile", "Url");
            DropColumn("dbo.BinaryFile", "StorageEntitySettings");
        }
    }
}
