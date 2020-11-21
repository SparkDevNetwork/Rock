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

namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    /// Adds DataViewId and RelatedDataViewId to the DataViewFilter Table.
    /// </summary>
    public partial class AddDataViewIdsToDataViewFilter : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.DataViewFilter", "DataViewId", c => c.Int() );
            AddColumn( "dbo.DataViewFilter", "RelatedDataViewId", c => c.Int() );

            UpdateDataFilterDataViewId();

            CreateIndex( "dbo.DataViewFilter", "DataViewId" );
            CreateIndex( "dbo.DataViewFilter", "RelatedDataViewId" );

            AddForeignKey( "dbo.DataViewFilter", "DataViewId", "dbo.DataView", "Id" );
            AddForeignKey( "dbo.DataViewFilter", "RelatedDataViewId", "dbo.DataView", "Id" );

            AddJobToUpdateDateKeyValues();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.DataViewFilter", "RelatedDataViewId", "dbo.DataView" );
            DropForeignKey( "dbo.DataViewFilter", "DataViewId", "dbo.DataView" );

            DropIndex( "dbo.DataViewFilter", new[] { "RelatedDataViewId" } );
            DropIndex( "dbo.DataViewFilter", new[] { "DataViewId" } );

            DropColumn( "dbo.DataViewFilter", "RelatedDataViewId" );
            DropColumn( "dbo.DataViewFilter", "DataViewId" );

            RemoveJobToPopulateRelatedDataviewId();
        }

        private void UpdateDataFilterDataViewId()
        {
            Sql( @"WITH [DataViewFilters] ([RootDataViewFilterId], [Id])
                    AS (
	                    SELECT [Id] AS [RootDataViewFilterId]
			                    , [Id]
	                    FROM [DataViewFilter]
	                    WHERE [ParentId] IS NULL
	                    UNION ALL
	                    SELECT [RootDataViewFilterId]
			                    , [DataViewFilter].[Id]
	                    FROM [DataViewFilter]
	                    INNER JOIN [DataViewFilters] ON [DataViewFilter].[ParentId] = [DataViewFilters].[Id]
                    )
                    SELECT * 
                    INTO #DataViewFilters
                    FROM [DataViewFilters]
                    
                    UPDATE [DataViewFilter]
                    SET [DataViewId] = [DataView].[Id]
                    FROM [DataViewFilter]
                    INNER JOIN #DataViewFilters [dvf] ON [dvf].[Id] = [DataViewFilter].[Id]
                    INNER JOIN [DataView] ON [DataView].[DataViewFilterId] = [dvf].[RootDataViewFilterId]

                    DROP TABLE #DataViewFilters
            " );
        }

        private void AddJobToUpdateDateKeyValues()
        {
            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.PostV110DataMigrationsPopulateRelatedDataViewId'
                                AND [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_110_POPULATE_RELATED_DATAVIEW_ID}'
            )
            BEGIN
                INSERT INTO [ServiceJob] (
                    [IsSystem]
                    ,[IsActive]
                    ,[Name]
                    ,[Description]
                    ,[Class]
                    ,[CronExpression]
                    ,[NotificationStatus]
                    ,[Guid]
                ) VALUES (
                    1
                    ,1
                    ,'Rock Update Helper v11.0 - Populate Related DataView Id'
                    ,'Runs data updates to set the populate the related dataview id added as part of the v11.0 update.'
                    ,'Rock.Jobs.PostV110DataMigrationsPopulateRelatedDataViewId'
                    ,'0 0 21 1/1 * ? *'
                    ,1
                    ,'{SystemGuid.ServiceJob.DATA_MIGRATIONS_110_POPULATE_RELATED_DATAVIEW_ID}'
                );
            END" );
        }

        private void RemoveJobToPopulateRelatedDataviewId()
        {
            Sql( $@"
                DELETE [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.PostV110DataMigrationsPopulateRelatedDataViewId'
                                AND [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_110_POPULATE_RELATED_DATAVIEW_ID}'
                " );
        }
    }
}
