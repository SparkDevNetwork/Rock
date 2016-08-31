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
    public partial class BinaryFileStoredProcUpdate : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"ALTER PROCEDURE [spBinaryFileGet]
    @Id int,
    @Guid uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    /* NOTE!: Column Order cannot be changed without changing BinaryFileService.partial.cs due to CommandBehavior.SequentialAccess */
    SELECT 
        bf.Id,
        bf.IsTemporary, 
        bf.IsSystem,
        bf.BinaryFileTypeId,
        bf.Url,
        bf.[FileName], 
        bf.MimeType,
        bf.ModifiedDateTime,
        bf.[Description],
        bf.StorageEntityTypeId,
        bf.[Guid],
        /* if the BinaryFile as StorageEntityTypeId set, use that. Otherwise use the default StorageEntityTypeId from BinaryFileType  */
        COALESCE (bfse.Name,bftse.Name ) as StorageEntityTypeName,
        bfd.Content
    FROM BinaryFile bf 
    LEFT JOIN BinaryFileData bfd
        ON bf.Id = bfd.Id
    LEFT JOIN EntityType bfse
        ON bf.StorageEntityTypeId = bfse.Id
    LEFT JOIN BinaryFileType bft
        on bf.BinaryFileTypeId = bft.Id
    LEFT JOIN EntityType bftse
        ON bft.StorageEntityTypeId = bftse.Id
    WHERE 
        (@Id > 0 and bf.Id = @Id)
        or
        (bf.[Guid] = @Guid)
END" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // intentionally blank
        }
    }
}
