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
    public partial class MediaFileFieldType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add BinaryFileType "Media File"
            Sql( @"
DECLARE @StorageEntityTypeFileSystemId INT = (
        SELECT Id
        FROM EntityType
        WHERE [Guid] = 'A97B6002-454E-4890-B529-B99F8F2F376A'
        )

IF NOT EXISTS (
        SELECT Id
        FROM BinaryFileType
        WHERE [Guid] = '6CBEA3B0-E983-40C1-9712-BD3FA2466EAE'
        )
BEGIN
    INSERT INTO [dbo].[BinaryFileType] (
        [IsSystem]
        ,[Name]
        ,[Description]
        ,[IconCssClass]
        ,[StorageEntityTypeId]
        ,[AllowCaching]
        ,[Guid]
        ,[RequiresViewSecurity]
        )
    VALUES (
        1
        ,'Media File'
        ,'File type for audio and video files'
        ,'fa fa-play'
        ,@StorageEntityTypeFileSystemId
        ,0
        ,'6CBEA3B0-E983-40C1-9712-BD3FA2466EAE'
        ,0
        );
END
ELSE
BEGIN
    UPDATE [dbo].[BinaryFileType]
    SET NAME = 'Media File'
        ,[Description] = 'File type for audio and video files'
        ,[IconCssClass] = 'fa fa-play'
        ,[StorageEntityTypeId] = @StorageEntityTypeFileSystemId
        ,[AllowCaching] = 0
        ,[RequiresViewSecurity] = 0
    WHERE [Guid] = '6CBEA3B0-E983-40C1-9712-BD3FA2466EAE'
END" );

            // add Attribute of Root Path for BinaryFileType of FileSystem
            RockMigrationHelper.AddEntityAttribute( typeof( Rock.Model.BinaryFileType ).FullName, Rock.SystemGuid.FieldType.TEXT, "StorageEntityTypeId", "52", "Root Path", "", "The root path where files should be stored on the file system", 0, "~/App_Data/Files", "3CAFA34D-9208-439B-A046-CB727FB729DE" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
