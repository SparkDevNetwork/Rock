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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// SparkData and NCOA
    /// </summary>
    [MigrationNumber( 51, "1.8.1" )]
    public class SparkData : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
//            #region Job

//            #region Add GetNcoa Job

//            Sql( $@"IF NOT EXISTS(SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.GetNcoa')
//BEGIN
//    INSERT INTO [dbo].[ServiceJob] (
//         [IsSystem]
//        ,[IsActive]
//        ,[Name]
//        ,[Description]
//        ,[Class]
//        ,[CronExpression]
//        ,[NotificationStatus]
//        ,[Guid]
//    )
//    VALUES (
//         0 
//        ,0 
//        ,'Get National Change of Address (NCOA)'
//        ,'Job to get a National Change of Address (NCOA) report for all active people''s addresses.'
//        ,'Rock.Jobs.GetNcoa'
//        ,'0 0/25 * 1/1 * ? *'
//        ,1
//        ,'{ServiceJob.GET_NCOA}');
//END" );

//            #endregion

//            // Delete ProcessNcoaResults job
//            Sql( "DELETE FROM [dbo].[ServiceJob] WHERE Class = 'Rock.Jobs.ProcessNcoaResults'" );

//            #endregion

//            #region Page and block

//            // Add the new page
//            RockMigrationHelper.AddPage( true, "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Spark Data Settings", "", "0591e498-0ad6-45a5-b8ca-9bca5c771f03", "fa fa-database", "74fb3214-8f11-4d40-a0e9-1aea377e9217" ); // Site:Spark Data
//            RockMigrationHelper.UpdateBlockType( "Spark Data Settings", "Block used to set values specific to Spark Data (NCOA, Etc).", "~/Blocks/Administration/SparkDataSettings.ascx", "Administration", "6B6A429D-E42C-70B5-4A04-98E886C45E7A" );
//            RockMigrationHelper.AddBlock( true, "0591e498-0ad6-45a5-b8ca-9bca5c771f03", "", "6B6A429D-E42C-70B5-4A04-98E886C45E7A", "Spark Data Settings", "Main", @"", @"", 0, "E7BA08B2-F8CC-2FA8-4677-EA3E776F4EEB" );

//            // Remove Ncoa History Detail BlockType: ~/Blocks/Crm/NcoaHistoryDetail.ascx
//            RockMigrationHelper.DeleteBlockType( "972b7955-ecf9-43b9-80b2-bff40675ffb8" );

//            // Data Automation Settings: Remove 'NCOA' from description
//            RockMigrationHelper.UpdateBlockType( "Data Automation Settings", "Block used to set values specific to data automation (Updating Person Status, Family Campus, Etc).", "~/Blocks/Administration/DataAutomationSettings.ascx", "Administration", "E34C45E9-97CA-4902-803B-1EFAC9174083" );

//            // Add the new page
//            RockMigrationHelper.AddPage( true, "84FD84DF-F58B-4B9D-A407-96276C40AB7E", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "NCOA Results", "", "B0C9AF70-752E-0F9A-4BA5-29B2E4C69B11", "fa fa-people-carry", "a2d5f989-1e30-47b9-aafc-f7ec627aff21" ); // Site:NCOA Results
//            RockMigrationHelper.AddBlock( true, "B0C9AF70-752E-0F9A-4BA5-29B2E4C69B11", "", "3997fe75-e069-4879-b8ba-c8b19c367cd3", "NCOA Results", "Main", @"", @"", 0, "06F05271-BED0-BB9C-4231-CE00348F1035" );

//            #endregion

//            #region System e-mail
//            RockMigrationHelper.UpdateSystemEmail( "System", "Spark Data Notification", "", "", "", "", "", "Spark Data: {{ SparkDataService }}", @"{{ 'Global' | Attribute:'EmailHeader' }}

//<p>
//    {{ Person.NickName }},
//</p>

//<p>
//    The '{{ SparkDataService }}' job has {{ Status }}.
//</p>

//{{ 'Global' | Attribute:'EmailFooter' }}", SystemGuid.SystemEmail.SPARK_DATA_NOTIFICATION );

//            #endregion
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //RockMigrationHelper.DeletePage( "B0C9AF70-752E-0F9A-4BA5-29B2E4C69B11" );
            //RockMigrationHelper.DeleteBlock( "E7BA08B2-F8CC-2FA8-4677-EA3E776F4EEB" );
            //RockMigrationHelper.DeleteBlockType( "6B6A429D-E42C-70B5-4A04-98E886C45E7A" );
            //RockMigrationHelper.DeletePage( "0591e498-0ad6-45a5-b8ca-9bca5c771f03" );
            //Sql( $@"DELETE FROM [dbo].[ServiceJob] WHERE [Guid] = '{ServiceJob.GET_NCOA}'" );
            //RockMigrationHelper.DeleteSystemEmail( SystemGuid.SystemEmail.SPARK_DATA_NOTIFICATION );
        }
    }
}
