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
    DECLARE @FileSystemEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = 'A97B6002-454E-4890-B529-B99F8F2F376A' )
    DECLARE @AttributeId int 
    DECLARE @DefaultValue nvarchar(max) 
    SELECT TOP 1 
	    @AttributeId = [Id],
	    @DefaultValue = [DefaultValue]
    FROM [Attribute] WHERE [Guid] = '3CAFA34D-9208-439B-A046-CB727FB729DE'

    UPDATE F SET 
	    Url = '{ ""RootPath"": ""' + 
	    REPLACE( REPLACE ( REPLACE ( COALESCE ( AV.[Value], @DefaultValue, '' ), '~/', '' ), '/', '\' ), '\', '\\' ) +
	    '"" }',
	    StorageEntitySettings = REPLACE( REPLACE ( COALESCE ( AV.[Value], @DefaultValue, '' ), '~/', '' ), '/', '\' )
    FROM [BinaryFile] F
    INNER JOIN [BinaryFileType] T 
	    ON T.[Id] = F.[BinaryFileTypeId]
    LEFT OUTER JOIN [AttributeValue] AV 
	    ON AV.[EntityId] = T.[Id]
	    AND AV.[AttributeId] = @AttributeId
    WHERE T.[StorageEntityTypeId] = @FileSystemEntityTypeId
    AND F.[Url] IS NULL 
    AND F.[StorageEntitySettings] IS NULL

    UPDATE [Attribute]
    SET [Description] = 'The relative path where files should be stored on the file system ( Default: ''App_Data\Files'' ).'
    WHERE [Id] = @AttributeId
");
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
