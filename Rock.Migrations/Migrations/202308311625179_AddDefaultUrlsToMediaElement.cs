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
    public partial class AddDefaultUrlsToMediaElement : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.MediaElement", "DefaultFileUrl", c => c.String(maxLength: 2048));
            AddColumn("dbo.MediaElement", "DefaultThumbnailUrl", c => c.String(maxLength: 2048));

            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_160_UPDATE_MEDIA_ELEMENT_DEFAULT_URLS}'
            )
            BEGIN
                INSERT INTO [ServiceJob] (
                      [IsSystem]
                    , [IsActive]
                    , [Name]
                    , [Description]
                    , [Class]
                    , [CronExpression]
                    , [NotificationStatus]
                    , [Guid]
                ) VALUES (
                      1
                    , 1
                    , 'Rock Update Helper v16.0 - Update Media Element Default URLs'
                    , 'This job updates media element default urls to match calculated values.'
                    , 'Rock.Jobs.PostUpdateJobs.PostV16UpdateMediaElementDefaultUrls'
                    , '0 0 21 1/1 * ? *'
                    , 1
                    , '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_160_UPDATE_MEDIA_ELEMENT_DEFAULT_URLS}'
                );
            END" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.MediaElement", "DefaultThumbnailUrl");
            DropColumn("dbo.MediaElement", "DefaultFileUrl");
        }
    }
}
