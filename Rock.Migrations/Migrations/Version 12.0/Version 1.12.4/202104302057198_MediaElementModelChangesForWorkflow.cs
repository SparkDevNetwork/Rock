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
    public partial class MediaElementModelChangesForWorkflow : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ModelChangesUp();
            PageChangesUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            PageChangesDown();
            ModelChangesDown();
        }

        /// <summary>
        /// Runs the SQL operations to make the changes to existing pages.
        /// </summary>
        private void PageChangesUp()
        {
            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = '52548B49-6D09-467E-BEA9-04DD6F51637D'" );
            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = '65DE6218-2850-4924-AA55-6F6FB572E9A3'" );
            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = 'F1AB34EE-941F-41D6-9BA1-22348D09724C'" );
        }

        /// <summary>
        /// Runs the SQL operations to undo the changes to existing pages.
        /// </summary>
        private void PageChangesDown()
        {
            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 1 WHERE [Guid] = 'F1AB34EE-941F-41D6-9BA1-22348D09724C'" );
            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 1 WHERE [Guid] = '65DE6218-2850-4924-AA55-6F6FB572E9A3'" );
            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 1 WHERE [Guid] = '52548B49-6D09-467E-BEA9-04DD6F51637D'" );
        }

        /// <summary>
        /// Runs the operations to add the model changes.
        /// </summary>
        private void ModelChangesUp()
        {
            AddColumn( "dbo.MediaAccount", "SourceData", c => c.String() );
            AddColumn( "dbo.MediaAccount", "MetricData", c => c.String() );

            AddColumn( "dbo.MediaFolder", "ContentChannelItemStatus", c => c.Int() );
            AddColumn( "dbo.MediaFolder", "WorkflowTypeId", c => c.Int() );

            AddColumn( "dbo.MediaElement", "DurationSeconds", c => c.Int() );
            AddColumn( "dbo.MediaElement", "ThumbnailDataJson", c => c.String() );
            AddColumn( "dbo.MediaElement", "FileDataJson", c => c.String() );

            CreateIndex( "dbo.MediaFolder", "WorkflowTypeId" );
            AddForeignKey( "dbo.MediaFolder", "WorkflowTypeId", "dbo.WorkflowType", "Id" );

            DropColumn( "dbo.MediaFolder", "Status" );

            DropColumn( "dbo.MediaElement", "Duration" );
            DropColumn( "dbo.MediaElement", "ThumbnailData" );
            DropColumn( "dbo.MediaElement", "MediaElementData" );
            DropColumn( "dbo.MediaElement", "DownloadData" );
        }

        /// <summary>
        /// Runs the operations to undo the model changes.
        /// </summary>
        private void ModelChangesDown()
        {
            AddColumn( "dbo.MediaElement", "DownloadData", c => c.String() );
            AddColumn( "dbo.MediaElement", "MediaElementData", c => c.String() );
            AddColumn( "dbo.MediaElement", "ThumbnailData", c => c.String() );
            AddColumn( "dbo.MediaElement", "Duration", c => c.Int() );

            AddColumn( "dbo.MediaFolder", "Status", c => c.Int() );

            DropForeignKey( "dbo.MediaFolder", "WorkflowTypeId", "dbo.WorkflowType" );
            DropIndex( "dbo.MediaFolder", new[] { "WorkflowTypeId" } );

            DropColumn( "dbo.MediaElement", "FileDataJson" );
            DropColumn( "dbo.MediaElement", "ThumbnailDataJson" );
            DropColumn( "dbo.MediaElement", "DurationSeconds" );

            DropColumn( "dbo.MediaFolder", "WorkflowTypeId" );
            DropColumn( "dbo.MediaFolder", "ContentChannelItemStatus" );

            DropColumn( "dbo.MediaAccount", "MetricData" );
            DropColumn( "dbo.MediaAccount", "SourceData" );
        }
    }
}
