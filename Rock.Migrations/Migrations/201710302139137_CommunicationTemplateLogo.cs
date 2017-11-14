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
    public partial class CommunicationTemplateLogo : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.InteractionChannel", "ComponentCacheDuration", c => c.Int());

            AddColumn("dbo.CommunicationTemplate", "LogoBinaryFileId", c => c.Int());
            AddColumn("dbo.CommunicationTemplate", "CategoryId", c => c.Int());
            CreateIndex("dbo.CommunicationTemplate", "LogoBinaryFileId");
            CreateIndex("dbo.CommunicationTemplate", "CategoryId");
            AddForeignKey("dbo.CommunicationTemplate", "CategoryId", "dbo.Category", "Id");
            AddForeignKey("dbo.CommunicationTemplate", "LogoBinaryFileId", "dbo.BinaryFile", "Id");

            // MP: Add "Admin" page route for page / 12
            RockMigrationHelper.AddPageRoute( Rock.SystemGuid.Page.INTERNAL_HOMEPAGE, "Admin" );

            // MP: Job for Processing BI Analytics
            // Remove old BI Jobs since we have a new one that does them all
            Sql( @"
    DELETE FROM [ServiceJob] where [Guid] in (
        'BBBB1D16-E4B5-439E-94F6-52AB14AE5292', -- Rock.Jobs.ProcessAnalyticsDimPerson
        '447B248B-2187-4368-9EE3-6E17B8F542A7', -- Process BI Analytics ETL (AnalyticsFactFinancialTransaction, AnalyticsFactAttendance)
        '23629583-8618-4FAF-8088-AFCC7545A2E0' -- Rock.Jobs.ProcessAnalyticsDimFamily
    )
" );

            // Add Process BI Analytics ETL Job 
            Sql( @"
    INSERT INTO [dbo].[ServiceJob] (
         [IsSystem]
        ,[IsActive]
        ,[Name]
        ,[Description]
        ,[Class]
        ,[CronExpression]
        ,[NotificationStatus]
        ,[Guid]
    )
    VALUES (
         0 
        ,1 
        ,'Process BI Analytics'
        ,'Job to take care of schema changes ( dynamic Attribute Value Fields ) and data updates to the BI related analytic tables'
        ,'Rock.Jobs.ProcessBIAnalytics'
        ,'0 0 5 1/1 * ? *'
        ,3
        ,'B6C89428-3ECA-49CC-87FD-C22EE6B38630')
" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Delete the 'Process BI Analytics' job
            Sql( "DELETE FROM [ServiceJob] where [Guid] = 'B6C89428-3ECA-49CC-87FD-C22EE6B38630'" );

            DropForeignKey("dbo.CommunicationTemplate", "LogoBinaryFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.CommunicationTemplate", "CategoryId", "dbo.Category");
            DropIndex("dbo.CommunicationTemplate", new[] { "CategoryId" });
            DropIndex("dbo.CommunicationTemplate", new[] { "LogoBinaryFileId" });
            DropColumn("dbo.CommunicationTemplate", "CategoryId");
            DropColumn("dbo.CommunicationTemplate", "LogoBinaryFileId");

            DropColumn( "dbo.InteractionChannel", "ComponentCacheDuration" );

        }
    }
}
