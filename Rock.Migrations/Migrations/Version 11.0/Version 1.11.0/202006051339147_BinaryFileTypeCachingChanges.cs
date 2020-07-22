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
    /// Update fields on BinaryFileType table.
    /// </summary>
    public partial class BinaryFileTypeCachingChanges : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RenameColumn( "dbo.BinaryFileType", "AllowCaching", "CacheToServerFileSystem" );
            AddColumn( "dbo.BinaryFileType", "CacheControlHeaderSettings", c => c.String( maxLength: 500 ) );
            SetCacheControlHeaderSettings();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RenameColumn( "dbo.BinaryFileType", "CacheToServerFileSystem", "AllowCaching" );
            DropColumn( "dbo.BinaryFileType", "CacheControlHeaderSettings" );
        }

        public void SetCacheControlHeaderSettings()
        {
            Sql( @"UPDATE [BinaryFileType]
                    SET [CacheControlHeaderSettings] = '{""RockCacheablityType"":0,""MaxAge"":{""Value"":31556952,""Unit"":0},""MaxSharedAge"":null}'
                    WHERE CacheToServerFileSystem = 1" );

            Sql( @"UPDATE [BinaryFileType]
                    SET [CacheControlHeaderSettings] = '{""RockCacheablityType"":3,""MaxAge"":null,""MaxSharedAge"":null}'
                    WHERE CacheToServerFileSystem = 0" );
        }
    }
}
