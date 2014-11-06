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

            // Add/Update FieldTypes for Audio and Video
            RockMigrationHelper.UpdateFieldType( "Audio", "", "Rock", "Rock.Field.Types.AudioFieldType", "0D842975-7439-4D2E-BB94-BAD8DDF22260" );
            RockMigrationHelper.UpdateFieldType( "Video", "", "Rock", "Rock.Field.Types.VideoFieldType", "FA398F9D-5B01-41EA-9A93-112F910A277D" );

            // add 
            RockMigrationHelper.UpdateFieldType( "Encrypted Text", "", "Rock", "Rock.Field.Types.EncryptedTextFieldType", "36167F3E-8CB2-44F9-9022-102F171FBC9A" );

            // Filter date field type
            RockMigrationHelper.UpdateFieldType( "Filter Date", "", "Rock", "Rock.Field.Types.FilterDateFieldType", "4F879A48-63DA-446F-837B-7458799298C0" );

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
