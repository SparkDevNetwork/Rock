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
    public partial class BinaryFileSecurity : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.BinaryFile", "AdditionalInformation", c => c.String());
            AddColumn("dbo.BinaryFile", "ParentEntityTypeId", c => c.Int());
            AddColumn("dbo.BinaryFile", "ParentEntityId", c => c.Int());
            Update_spCore_BinaryFileGet();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.BinaryFile", "ParentEntityId");
            DropColumn("dbo.BinaryFile", "ParentEntityTypeId");
            DropColumn("dbo.BinaryFile", "AdditionalInformation");
        }

        private void Update_spCore_BinaryFileGet()
        {
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
		                , bf.[Path]
		                , bf.[FileSize]
		                , bf.[ParentEntityTypeId]
		                , bf.[ParentEntityId]
                        /* if the BinaryFile as StorageEntityTypeId set, use that. Otherwise use the default StorageEntityTypeId from BinaryFileType  */
                        , COALESCE (bfse.[Name],bftse.[Name] ) as [StorageEntityTypeName]
                        , bfd.[Content]
                    FROM [BinaryFile] bf 
                    LEFT JOIN [BinaryFileData] bfd ON bf.[Id] = bfd.[Id]
                    LEFT JOIN [EntityType] bfse ON bf.[StorageEntityTypeId] = bfse.[Id]
                    LEFT JOIN [BinaryFileType] bft ON bf.[BinaryFileTypeId] = bft.[Id]
                    LEFT JOIN [EntityType] bftse ON bft.[StorageEntityTypeId] = bftse.[Id]
                    WHERE (@Id > 0 AND bf.[Id] = @Id)
                        OR (bf.[Guid] = @Guid)
                END" );
        }
    }
}
