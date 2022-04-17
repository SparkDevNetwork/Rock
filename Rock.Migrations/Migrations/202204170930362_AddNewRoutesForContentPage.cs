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
    public partial class AddNewRoutesForContentPage : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql(@"
            DECLARE @ContentPageId INT = (SELECT TOP 1 [Id] FROM Page WHERE Guid = '117B547B-9D71-4EE9-8047-176676F5DC8C')

            IF NOT EXISTS (
               SELECT [Id]
               FROM [PageRoute]
               WHERE [Route] = 'web/content/{CategoryGuid}/{ContentChannelGuid}' )
            BEGIN
	            INSERT INTO
                    [PageRoute]
                    (IsSystem, PageId, Route, Guid, IsGlobal)
                VALUES 
                    (1, @ContentPageId, 'web/content/{CategoryGuid}/{ContentChannelGuid}', NEWID(), 0)
            END

            IF NOT EXISTS (
               SELECT [Id]
               FROM [PageRoute]
               WHERE [Route] = 'web/content/category/{CategoryGuid}' )
            BEGIN
	            INSERT INTO
                    [PageRoute]
                    (IsSystem, PageId, Route, Guid, IsGlobal)
                VALUES 
                    (1, @ContentPageId, 'web/content/category/{CategoryGuid}', NEWID(), 0)
            END
            ");
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql(@"
            DELETE FROM PageRoute WHERE Route = 'web/content/{CategoryGuid}/{ContentChannelGuid}'
            DELETE FROM PageRoute WHERE Route = 'web/content/category/{CategoryGuid}'
            ");
        }
    }
}
