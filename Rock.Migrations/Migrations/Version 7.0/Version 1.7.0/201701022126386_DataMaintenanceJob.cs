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
    public partial class DataMaintenanceJob : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // add stored proc
            Sql( MigrationSQL._201701022126386_DataMaintenanceJob );

            // add job
            Sql( @"
    INSERT INTO [ServiceJob] (
         [IsSystem]
        ,[IsActive]
        ,[Name]
        ,[Description]
        ,[Class]
        ,[CronExpression]
        ,[NotificationStatus]
        ,[Guid] )
    VALUES (
         0
        ,1
        ,'Database Maintenance'
        ,'Performs routine SQL Server database maintenance.'
        ,'Rock.Jobs.DatabaseMaintenance'
        ,'0 30 2 1/1 * ? *'
        ,1
        ,'DE104981-6E52-719A-459D-72D6B3C172B1');
" );

            // Add job attributes
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.ServiceJob", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Class", "Rock.Jobs.DatabaseMaintenance", "Run Integrity Check", "", "Determines if an integrity check should be performed.", 0, "True", "05832364-EFEB-C8A3-44C4-3FBA16DD9ACC", "RunIntegrityCheck" );
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.ServiceJob", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Class", "Rock.Jobs.DatabaseMaintenance", "Run Index Rebuild", "", "Determines if indexes should be rebuilt.", 1, "True", "D276060E-F78F-4D82-4026-D04BF178A063", "RunIndexRebuild" );
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.ServiceJob", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Class", "Rock.Jobs.DatabaseMaintenance", "Run Statistics Update", "", "Determines if the statistics should be updated.", 2, "True", "FC2502D2-36BC-7292-493F-D36C1E8DFD14", "RunStatisticsUpdate" );

            RockMigrationHelper.AddEntityAttribute( "Rock.Model.ServiceJob", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Class", "Rock.Jobs.DatabaseMaintenance", "Alert Email", "", "Email address to send alerts to errors occur (multiple address delimited with comma).", 3, "", "5A1FC9FC-C287-26BD-4472-E1FA9A1167FC", "AlertEmail" );

            RockMigrationHelper.AddEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "Rock.Jobs.DatabaseMaintenance", "Command Timeout", "", "Maximum amount of time (in seconds) to wait for each step to complete.", 4, "900", "70A7DC56-6554-61B5-4AB1-400D82570843", "CommandTimeout" );
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "Rock.Jobs.DatabaseMaintenance", "Minimum Index Page Count", "", "The minimum size in pages that an index must be before it's considered for being re-built. Default value is 100.", 4, "100", "668D9FE7-5A81-80A9-418D-E6A63073D7A0", "MinimumIndexPageCount" );
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "Rock.Jobs.DatabaseMaintenance", "Minimum Fragmentation Percentage", "", "The minimum fragmentation percentage for an index to be considered for re-indexing. If the fragmentation is below is amount nothing will be done. Default value is 10%.", 4, "10", "7A238F20-69E0-928C-47AE-AC9BEE799257", "MinimumFragmentationPercentage" );
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "Rock.Jobs.DatabaseMaintenance", "Rebuild Threshold Percentage", "", "The threshold percentage where a REBUILD will be completed instead of a REORGANIZE. Default value is 30%.", 4, "30", "42724DAC-74DB-2FA0-4308-80F001A6F84B", "RebuildThresholdPercentage" );

            // Add job attribute values
            Sql( @"
                DECLARE @JobId int = ( SELECT TOP 1 [Id] FROM [ServiceJob] WHERE [Guid] = 'DE104981-6E52-719A-459D-72D6B3C172B1' )
	            
                DECLARE @RunIntegrityCheckAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '05832364-EFEB-C8A3-44C4-3FBA16DD9ACC' )
                DECLARE @RunIndexRebuildAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'D276060E-F78F-4D82-4026-D04BF178A063' )
                DECLARE @RunStatisticsUpdateAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'FC2502D2-36BC-7292-493F-D36C1E8DFD14' )
    
                DECLARE @AlertEmailAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '5A1FC9FC-C287-26BD-4472-E1FA9A1167FC' )

                DECLARE @CommandTimeoutAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '70A7DC56-6554-61B5-4AB1-400D82570843' )
                DECLARE @MinimumIndexPageCountAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '668D9FE7-5A81-80A9-418D-E6A63073D7A0' )
                DECLARE @MinimumFragmentationPercentageAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '7A238F20-69E0-928C-47AE-AC9BEE799257' )
                DECLARE @RebuildThresholdPercentageAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '42724DAC-74DB-2FA0-4308-80F001A6F84B' )
	            
                IF @JobId IS NOT NULL 
	            BEGIN
		                -- bools
                        INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
		                VALUES ( 0, @RunIntegrityCheckAttributeId, @JobId, 'True', NEWID() )

                        INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
		                VALUES ( 0, @RunIndexRebuildAttributeId, @JobId, 'True', NEWID() )

                        INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
		                VALUES ( 0, @RunStatisticsUpdateAttributeId, @JobId, 'True', NEWID() )                     

                        -- ints
                        INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
		                VALUES ( 0, @CommandTimeoutAttributeId, @JobId, '900', NEWID() )

                        INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
		                VALUES ( 0, @MinimumIndexPageCountAttributeId, @JobId, '100', NEWID() )

                        INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
		                VALUES ( 0, @MinimumFragmentationPercentageAttributeId, @JobId, '10', NEWID() )

                        INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
		                VALUES ( 0, @RebuildThresholdPercentageAttributeId, @JobId, '30', NEWID() )

	            END" );

            // migration rollup
            RockMigrationHelper.UpdateEntityType( "Rock.UniversalSearch.IndexComponents.ElasticSearchAws", "Elastic Search Aws", "Rock.UniversalSearch.IndexComponents.ElasticSearchAws, Rock, Version=1.6.1.0, Culture=neutral, PublicKeyToken=null", false, true, "1E04435C-7B58-4A84-9352-E0612B9E8A0A" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "05832364-EFEB-C8A3-44C4-3FBA16DD9ACC" );
            RockMigrationHelper.DeleteAttribute( "D276060E-F78F-4D82-4026-D04BF178A063" );
            RockMigrationHelper.DeleteAttribute( "FC2502D2-36BC-7292-493F-D36C1E8DFD14" );

            RockMigrationHelper.DeleteAttribute( "5A1FC9FC-C287-26BD-4472-E1FA9A1167FC" );

            RockMigrationHelper.DeleteAttribute( "70A7DC56-6554-61B5-4AB1-400D82570843" );
            RockMigrationHelper.DeleteAttribute( "668D9FE7-5A81-80A9-418D-E6A63073D7A0" );
            RockMigrationHelper.DeleteAttribute( "7A238F20-69E0-928C-47AE-AC9BEE799257" );
            RockMigrationHelper.DeleteAttribute( "42724DAC-74DB-2FA0-4308-80F001A6F84B" );

            Sql( "DELETE [ServiceJob] WHERE [Guid] = 'DE104981-6E52-719A-459D-72D6B3C172B1'" );

            Sql( @"DROP PROCEDURE spDbaRebuildIndexes" );
        }
    }
}
