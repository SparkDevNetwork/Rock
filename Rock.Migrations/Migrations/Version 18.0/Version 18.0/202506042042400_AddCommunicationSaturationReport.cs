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

    using Rock.Migrations.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class AddCommunicationSaturationReport : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Add [spCommunication_SaturationReport] (dropping it first).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spCommunication_SaturationReport]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spCommunication_SaturationReport];" );

            Sql( RockMigrationSQL._202506042042400_AddCommunicationSaturationReport_spCommunication_SaturationReport );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );

            // Add Page            
            //  Internal Name: Communication Saturation Report              
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "7F79E512-B9DB-4780-9887-AD6D63A39050", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Communication Saturation Report", "", "DA0F647C-5317-4368-9514-E408F2254E24", "" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.SmsConversations
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.CommunicationSaturationReport", "Communication Saturation Report", "Rock.Blocks.Communication.CommunicationSaturationReport, Rock.Blocks, Version=18.0.8.0, Culture=neutral, PublicKeyToken=null", false, false, "44637bbe-3460-4133-8164-a14351389fb5" );

            // Add/Update Obsidian Block Type              
            //   Name:Communication Saturation Report              
            //   Category:Communication              
            //   EntityType:Rock.Blocks.Communication.CommunicationSaturationReport
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Communication Saturation Report", "Shows analytics for communications.", "Rock.Blocks.Communication.CommunicationSaturationReport", "Communication", "6EE7BCF5-88A4-4484-A590-C8C03A4C143F" );

            // Add Block               
            //  Block Name: Communication Saturation Report              
            //  Page Name: Communication Saturation Report              
            //  Layout: -              
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "DA0F647C-5317-4368-9514-E408F2254E24".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "6EE7BCF5-88A4-4484-A590-C8C03A4C143F".AsGuid(), "Communication Saturation Report", "Main", @"", @"", 0, "2BFA5F21-1B1F-4875-847B-9C376FC5E1CD" );

            // Attribute for BlockType
            //   BlockType: Communication Saturation Report              
            //   Category: Communication              
            //   Attribute: core.CustomActionsConfigs              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6EE7BCF5-88A4-4484-A590-C8C03A4C143F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "A81FACFD-9793-4A0F-97A7-4A19A081C766" );

            // Attribute for BlockType
            //   BlockType: Communication Saturation Report              
            //   Category: Communication              
            //   Attribute: core.EnableDefaultWorkflowLauncher              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6EE7BCF5-88A4-4484-A590-C8C03A4C143F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "30DAA189-95FD-4BBE-8815-C8A5A02DB1D9" );

            // Attribute for BlockType
            //   BlockType: Communication Saturation Report              
            //   Category: Communication              
            //   Attribute: Email Bucket Ratio              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6EE7BCF5-88A4-4484-A590-C8C03A4C143F", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Email Bucket Ratio", "EmailBucketRatio", "MaxCommunicationsToList", @"This ratio determines the number of days each x-axis bucket represents on the chart. A value of 10 means that for every 10 days in the date range, 1 day is added to the bucket size.", 1, @"10", "49125E4A-BA2F-46F6-BFB1-59896222F1F2" );

            // Attribute for BlockType
            //   BlockType: Communication Saturation Report              
            //   Category: Communication              
            //   Attribute: Push Notifications Bucket Ratio              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6EE7BCF5-88A4-4484-A590-C8C03A4C143F", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Push Notifications Bucket Ratio", "PushNotificationBucketRatio", "Push Notifications Bucket Ratio", @"This ratio determines the number of days each x-axis bucket represents on the chart. A value of 10 means that for every 10 days in the date range, 1 day is added to the bucket size.", 3, @"20", "B84D9757-454A-4AEE-8BAB-3BB8DAC3B521" );

            // Attribute for BlockType
            //   BlockType: Communication Saturation Report              
            //   Category: Communication              
            //   Attribute: Max Recipients To List              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6EE7BCF5-88A4-4484-A590-C8C03A4C143F", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Recipients To List", "MaxRecipientsToList", "Max Recipients To List", @"The maximum number of rows to display in the report when listing by recipient.", 4, @"100", "18117BD6-B7A0-4024-B2C4-6690C0752844" );

            // Attribute for BlockType
            //   BlockType: Communication Saturation Report              
            //   Category: Communication              
            //   Attribute: Max Communications To List              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6EE7BCF5-88A4-4484-A590-C8C03A4C143F", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Communications To List", "MaxCommunicationsToList", "Max Communications To List", @"The maximum number of rows to display in the report when listing by communication.", 5, @"100", "B603BBE8-9397-431F-872E-F6B6C29A9568" );

            // Attribute for BlockType
            //   BlockType: Communication Saturation Report              
            //   Category: Communication              
            //   Attribute: SMS Bucket Ratio              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6EE7BCF5-88A4-4484-A590-C8C03A4C143F", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "SMS Bucket Ratio", "SmsBucketRatio", "SMS Bucket Ratio", @"This ratio determines the number of days each x-axis bucket represents on the chart. A value of 10 means that for every 10 days in the date range, 1 day is added to the bucket size.", 2, @"20", "663299CA-7D55-4318-8677-8C9F89D04D7E" );

            // Attribute for BlockType
            //   BlockType: Communication Saturation Report              
            //   Category: Communication              
            //   Attribute: Communication Detail Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6EE7BCF5-88A4-4484-A590-C8C03A4C143F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Communication Detail Page", "CommunicationDetailPage", "Communication Detail Page", @"The page that will show the communication details.", 0, @"", "FBC0B7B7-6994-4F13-82D2-ECCFA971F389" );


            // Add Block Attribute Value              
            //   Block: Communication Saturation Report              
            //   BlockType: Communication Saturation Report              
            //   Category: Communication              
            //   Block Location: Page=Communication Saturation Report, Site=Rock RMS              
            //   Attribute: Communication Detail Page
            /*   Attribute Value: 2a22d08d-73a8-4aaf-ac7e-220e8b2e7857,79c0c1a7-41b6-4b40-954d-660a4b39b8ce */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "2BFA5F21-1B1F-4875-847B-9C376FC5E1CD", "FBC0B7B7-6994-4F13-82D2-ECCFA971F389", @"2a22d08d-73a8-4aaf-ac7e-220e8b2e7857,79c0c1a7-41b6-4b40-954d-660a4b39b8ce" );

            // Add Block Attribute Value              
            //   Block: Communication Saturation Report              
            //   BlockType: Communication Saturation Report              
            //   Category: Communication              
            //   Block Location: Page=Communication Saturation Report, Site=Rock RMS              
            //   Attribute: SMS Bucket Ratio              
            /*   Attribute Value: 20 */
            //   Skip If Already Exists: true              
            RockMigrationHelper.AddBlockAttributeValue( true, "2BFA5F21-1B1F-4875-847B-9C376FC5E1CD", "663299CA-7D55-4318-8677-8C9F89D04D7E", @"20" );

            // Add Block Attribute Value              
            //   Block: Communication Saturation Report              
            //   BlockType: Communication Saturation Report              
            //   Category: Communication              
            //   Block Location: Page=Communication Saturation Report, Site=Rock RMS              
            //   Attribute: Max Recipients To List              
            /*   Attribute Value: 100 */
            //   Skip If Already Exists: true              
            RockMigrationHelper.AddBlockAttributeValue( true, "2BFA5F21-1B1F-4875-847B-9C376FC5E1CD", "18117BD6-B7A0-4024-B2C4-6690C0752844", @"100" );

            // Add Block Attribute Value              
            //   Block: Communication Saturation Report              
            //   BlockType: Communication Saturation Report              
            //   Category: Communication              
            //   Block Location: Page=Communication Saturation Report, Site=Rock RMS              
            //   Attribute: Max Communications To List              
            /*   Attribute Value: 100 */
            //   Skip If Already Exists: true              
            RockMigrationHelper.AddBlockAttributeValue( true, "2BFA5F21-1B1F-4875-847B-9C376FC5E1CD", "B603BBE8-9397-431F-872E-F6B6C29A9568", @"100" );

            // Add Block Attribute Value              
            //   Block: Communication Saturation Report              
            //   BlockType: Communication Saturation Report              
            //   Category: Communication              
            //   Block Location: Page=Communication Saturation Report, Site=Rock RMS              
            //   Attribute: Email Bucket Ratio              
            /*   Attribute Value: 10 */
            //   Skip If Already Exists: true              
            RockMigrationHelper.AddBlockAttributeValue( true, "2BFA5F21-1B1F-4875-847B-9C376FC5E1CD", "49125E4A-BA2F-46F6-BFB1-59896222F1F2", @"10" );

            // Add Block Attribute Value              
            //   Block: Communication Saturation Report              
            //   BlockType: Communication Saturation Report              
            //   Category: Communication              
            //   Block Location: Page=Communication Saturation Report, Site=Rock RMS              
            //   Attribute: Push Notifications Bucket Ratio              
            /*   Attribute Value: 20 */
            //   Skip If Already Exists: true              
            RockMigrationHelper.AddBlockAttributeValue( true, "2BFA5F21-1B1F-4875-847B-9C376FC5E1CD", "B84D9757-454A-4AEE-8BAB-3BB8DAC3B521", @"20" );

            // Add Block Attribute Value              
            //   Block: Communication Saturation Report              
            //   BlockType: Communication Saturation Report              
            //   Category: Communication              
            //   Block Location: Page=Communication Saturation Report, Site=Rock RMS              
            //   Attribute: core.CustomGridEnableStickyHeaders              
            /*   Attribute Value: False */
            //   Skip If Already Exists: true              
            RockMigrationHelper.AddBlockAttributeValue( true, "2BFA5F21-1B1F-4875-847B-9C376FC5E1CD", "09B89D52-EB18-46A7-937A-599AF6EFA3FB", @"False" );

            // Add Block Attribute Value              
            //   Block: Communication Saturation Report              
            //   BlockType: Communication Saturation Report              
            //   Category: Communication              
            //   Block Location: Page=Communication Saturation Report, Site=Rock RMS              
            //   Attribute: core.EnableDefaultWorkflowLauncher              
            /*   Attribute Value: True */
            //   Skip If Already Exists: true              
            RockMigrationHelper.AddBlockAttributeValue( true, "2BFA5F21-1B1F-4875-847B-9C376FC5E1CD", "30DAA189-95FD-4BBE-8815-C8A5A02DB1D9", @"True" );

            // Add Page Route              
            //   Page:Communication Saturation Report              
            //   Route:CommunicationSaturationReport
            RockMigrationHelper.AddOrUpdatePageRoute( "DA0F647C-5317-4368-9514-E408F2254E24", "CommunicationSaturationReport", "65E7797B-BC89-4881-8020-5F42CF8165C0" );

            // Add Page Route              
            //   Page:Communication Saturation Report              
            //   Route:communications/communication-saturation-report
            RockMigrationHelper.AddOrUpdatePageRoute( "DA0F647C-5317-4368-9514-E408F2254E24", "communications/communication-saturation-report", "5C6C72AA-0E1E-480B-9BBC-C895A2FE17FB" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Drop [spCommunication_SaturationReport]
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spCommunication_SaturationReport]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spCommunication_SaturationReport];" );

            // Attribute for BlockType
            //   BlockType: Communication Saturation Report
            //   Category: Communication
            //   Attribute: Communication Detail Page
            RockMigrationHelper.DeleteAttribute( "FBC0B7B7-6994-4F13-82D2-ECCFA971F389" );

            // Attribute for BlockType
            //   BlockType: Communication Saturation Report
            //   Category: Communication
            //   Attribute: SMS Bucket Ratio
            RockMigrationHelper.DeleteAttribute( "663299CA-7D55-4318-8677-8C9F89D04D7E" );

            // Attribute for BlockType
            //   BlockType: Communication Saturation Report
            //   Category: Communication
            //   Attribute: Max Communications To List
            RockMigrationHelper.DeleteAttribute( "B603BBE8-9397-431F-872E-F6B6C29A9568" );

            // Attribute for BlockType
            //   BlockType: Communication Saturation Report
            //   Category: Communication
            //   Attribute: Max Recipients To List
            RockMigrationHelper.DeleteAttribute( "18117BD6-B7A0-4024-B2C4-6690C0752844" );

            // Attribute for BlockType
            //   BlockType: Communication Saturation Report
            //   Category: Communication
            //   Attribute: Push Notifications Bucket Ratio
            RockMigrationHelper.DeleteAttribute( "B84D9757-454A-4AEE-8BAB-3BB8DAC3B521" );

            // Attribute for BlockType
            //   BlockType: Communication Saturation Report
            //   Category: Communication
            //   Attribute: Email Bucket Ratio
            RockMigrationHelper.DeleteAttribute( "49125E4A-BA2F-46F6-BFB1-59896222F1F2" );

            // Attribute for BlockType
            //   BlockType: Communication Saturation Report
            //   Category: Communication
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "30DAA189-95FD-4BBE-8815-C8A5A02DB1D9" );

            // Attribute for BlockType
            //   BlockType: Communication Saturation Report
            //   Category: Communication
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "A81FACFD-9793-4A0F-97A7-4A19A081C766" );

            // Remove Block
            //  Name: Communication Saturation Report, from Page: Communication Saturation Report, Site: Rock RMS
            //  from Page: Communication Saturation Report, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "2BFA5F21-1B1F-4875-847B-9C376FC5E1CD" );

            // Delete BlockType               
            //   Name: Communication Saturation Report              
            //   Category: Communication              
            //   Path: -              
            //   EntityType: Communication Saturation Report
            RockMigrationHelper.DeleteBlockType( "6EE7BCF5-88A4-4484-A590-C8C03A4C143F" );

            // Remove Route
            // CommunicationSaturationReport
            RockMigrationHelper.DeletePageRoute( "65E7797B-BC89-4881-8020-5F42CF8165C0" );

            // Remove Route
            // communications/communication-saturation-report
            RockMigrationHelper.DeletePageRoute( "5C6C72AA-0E1E-480B-9BBC-C895A2FE17FB" );

            // Delete Page
            //  Internal Name: Communication Saturation Report
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "DA0F647C-5317-4368-9514-E408F2254E24" );
        }
    }
}
