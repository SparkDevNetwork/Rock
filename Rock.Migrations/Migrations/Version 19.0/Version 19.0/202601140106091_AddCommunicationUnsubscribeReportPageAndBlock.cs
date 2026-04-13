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

    /// <summary>
    ///
    /// </summary>
    public partial class AddCommunicationUnsubscribeReportPageAndBlock : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            JPH_AddCommunicationUnsubscribeReportPageAndBlock_20260113_Up();
            JPH_AddCommunicationUnsubscribeReportIndex_20260113_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            JPH_AddCommunicationUnsubscribeReportPageAndBlock_20260113_Down();
        }

        /// <summary>
        /// JPH: Add the Communication Unsubscribe Report page and block - up.
        /// </summary>
        private void JPH_AddCommunicationUnsubscribeReportPageAndBlock_20260113_Up()
        {
            // Add Page 
            //  Internal Name: Unsubscribe Report
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, Rock.SystemGuid.Page.COMMUNICATION_REPORTS, "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Unsubscribe Report", "", Rock.SystemGuid.Page.COMMUNICATION_UNSUBSCRIBE_REPORT, "ti ti-circle-minus" );

            // Add Page Route
            //   Page:Unsubscribe Report
            //   Route:communications/reports/unsubscribe
            RockMigrationHelper.AddOrUpdatePageRoute( Rock.SystemGuid.Page.COMMUNICATION_UNSUBSCRIBE_REPORT, "communications/reports/unsubscribe", "66E10163-3B77-45C6-B421-CA74C15C3F3A" );

            // ----------------------------------

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.CommunicationUnsubscribeReport
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.CommunicationUnsubscribeReport", "Communication Unsubscribe Report", "Rock.Blocks.Communication.CommunicationUnsubscribeReport, Rock.Blocks, Version=19.0.3.0, Culture=neutral, PublicKeyToken=null", false, false, "FA66E8EA-EC5B-4E1B-BC08-20608AB3CD22" );

            // Add/Update Obsidian Block Type
            //   Name:Communication Unsubscribe Report
            //   Category:Communication
            //   EntityType:Rock.Blocks.Communication.CommunicationUnsubscribeReport
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Communication Unsubscribe Report", "Used for displaying details of recipients who have unsubscribed as a result of receiving communications.", "Rock.Blocks.Communication.CommunicationUnsubscribeReport", "Communication", "33AC3AE0-928E-42C4-B6AC-BA4AB1DA4520" );

            // ----------------------------------

            // Add Block 
            //  Block Name: Communication Unsubscribe Report
            //  Page Name: Unsubscribe Report
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, Rock.SystemGuid.Page.COMMUNICATION_UNSUBSCRIBE_REPORT.AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "33AC3AE0-928E-42C4-B6AC-BA4AB1DA4520".AsGuid(), "Communication Unsubscribe Report", "Main", @"", @"", 0, "F95FCB52-47E5-441F-8BA4-D7960A6A3EED" );
        }

        /// <summary>
        /// JPH: Add the Communication Unsubscribe Report page and block - down.
        /// </summary>
        private void JPH_AddCommunicationUnsubscribeReportPageAndBlock_20260113_Down()
        {
            // Remove Block
            //  Name: Communication Unsubscribe Report, from Page: Unsubscribe Report, Site: Rock RMS
            //  from Page: Unsubscribe Report, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "F95FCB52-47E5-441F-8BA4-D7960A6A3EED" );

            // ----------------------------------

            // Delete BlockType 
            //   Name: Communication Unsubscribe Report
            //   Category: Communication
            //   Path: -
            //   EntityType: Communication Unsubscribe Report
            RockMigrationHelper.DeleteBlockType( "33AC3AE0-928E-42C4-B6AC-BA4AB1DA4520" );

            // Delete Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.CommunicationUnsubscribeReport
            RockMigrationHelper.DeleteEntityType( "FA66E8EA-EC5B-4E1B-BC08-20608AB3CD22" );

            // ----------------------------------

            // Delete Page 
            //  Internal Name: Unsubscribe Report
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( Rock.SystemGuid.Page.COMMUNICATION_UNSUBSCRIBE_REPORT );
        }

        /// <summary>
        /// JPH: Add a post update job to add an index to ensure the Communication Unsubscribe Report performs efficiently.
        /// </summary>
        private void JPH_AddCommunicationUnsubscribeReportIndex_20260113_Up()
        {
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v19.0 - Add Index For Communication Unsubscribe Report",
                description: "This job will add an index to ensure the Communication Unsubscribe Report performs efficiently.",
                jobType: "Rock.Jobs.PostV19AddCommunicationUnsubscribeReportIndex",
                cronExpression: "0 0 2 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_190_ADD_COMMUNICATION_UNSUBSCRIBE_REPORT_INDEX );
        }
    }
}
