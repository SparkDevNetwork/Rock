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
    public partial class CodeGenerated_20231101 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Types.Mobile.Connection.AddConnectionRequest
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Types.Mobile.Connection.AddConnectionRequest", "Add Connection Request", "Rock.Blocks.Types.Mobile.Connection.AddConnectionRequest, Rock, Version=1.16.1.11, Culture=neutral, PublicKeyToken=null", false, false, "F41E7BE3-2854-40FF-82C8-1FDEA12B8B2F");

            // Add/Update Mobile Block Type
            //   Name:Add Connection Request
            //   Category:Mobile > Connection
            //   EntityType:Rock.Blocks.Types.Mobile.Connection.AddConnectionRequest
            RockMigrationHelper.AddOrUpdateEntityBlockType("Add Connection Request", "Allows an individual to create and add a new Connection Request.", "Rock.Blocks.Types.Mobile.Connection.AddConnectionRequest", "Mobile > Connection", "1380115A-B3F0-49BC-A6BC-432A59DC27A2");

            // Attribute for BlockType
            //   BlockType: Acme Certificates
            //   Category: Blue Box Moon > Acme Certificate
            //   Attribute: Redirect Override
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A08FB359-E68E-46FC-85D6-53726EAE6E23", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Redirect Override", "RedirectOverride", "Redirect Override", @"If you enter a value here it will be used as the redirect URL for Acme Challenges to other sites instead of the automatically determined one.", 1, @"", "352D8C47-0E92-4275-9322-DF95F5CAC192" );

            // Attribute for BlockType
            //   BlockType: Add Connection Request
            //   Category: Mobile > Connection
            //   Attribute: Connection Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1380115A-B3F0-49BC-A6BC-432A59DC27A2", "E4E72958-4604-498F-956B-BA095976A60B", "Connection Types", "ConnectionTypes", "Connection Types", @"The connection types to limit this block to. Will only display if the person has access to see them. None selected means all will be available.", 0, @"", "00CDE4AE-DD60-4343-B053-DA7C913568A4" );

            // Attribute for BlockType
            //   BlockType: Add Connection Request
            //   Category: Mobile > Connection
            //   Attribute: Post Save Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1380115A-B3F0-49BC-A6BC-432A59DC27A2", "8AF3E49F-4FF1-47D8-BCD2-150201B7F1B8", "Post Save Action", "PostSaveAction", "Post Save Action", @"The navigation action to perform on save. 'ConnectionRequestIdKey' will be passed as a query string parameter.", 1, @"{""Type"": 4, ""PageGuid"": """"}", "181B220C-F03C-435D-8BE9-8AD131716C61" );

            // Attribute for BlockType
            //   BlockType: Add Connection Request
            //   Category: Mobile > Connection
            //   Attribute: Post Cancel Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1380115A-B3F0-49BC-A6BC-432A59DC27A2", "8AF3E49F-4FF1-47D8-BCD2-150201B7F1B8", "Post Cancel Action", "PostCancelAction", "Post Cancel Action", @"The navigation action to perform when the cancel button is pressed.", 2, @"{""Type"": 1, ""PopCount"": 1}", "F2FBF946-4ECA-48F9-B42B-2A2192041A36" );

            // Attribute for BlockType
            //   BlockType: Transaction List
            //   Category: Finance
            //   Attribute: Hide Transactions in Pending Batches
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E04320BC-67C3-452D-9EF6-D74D8C177154", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Transactions in Pending Batches", "HideTransactionsInPendingBatches", "Hide Transactions in Pending Batches", @"When enabled, transactions in a batch whose status is 'Pending' will be filtered out from the list.", 13, @"False", "5A1AB4C5-6892-4311-B5DF-7EBF82048510" );

            // Attribute for BlockType
            //   BlockType: Group Tree View
            //   Category: Groups
            //   Attribute: Limit to Security Role Groups
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Security Role Groups", "LimitToSecurityRoleGroups", "Limit to Security Role Groups", @"", 5, @"False", "1D2A80AC-DB57-4203-A533-AD273922B21A" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            
            // Attribute for BlockType
            //   BlockType: Group Tree View
            //   Category: Groups
            //   Attribute: Limit to Security Role Groups
            RockMigrationHelper.DeleteAttribute("1D2A80AC-DB57-4203-A533-AD273922B21A");

            // Attribute for BlockType
            //   BlockType: Transaction List
            //   Category: Finance
            //   Attribute: Hide Transactions in Pending Batches
            RockMigrationHelper.DeleteAttribute("5A1AB4C5-6892-4311-B5DF-7EBF82048510");

            // Attribute for BlockType
            //   BlockType: Add Connection Request
            //   Category: Mobile > Connection
            //   Attribute: Post Cancel Action
            RockMigrationHelper.DeleteAttribute("F2FBF946-4ECA-48F9-B42B-2A2192041A36");

            // Attribute for BlockType
            //   BlockType: Add Connection Request
            //   Category: Mobile > Connection
            //   Attribute: Post Save Action
            RockMigrationHelper.DeleteAttribute("181B220C-F03C-435D-8BE9-8AD131716C61");

            // Attribute for BlockType
            //   BlockType: Add Connection Request
            //   Category: Mobile > Connection
            //   Attribute: Connection Types
            RockMigrationHelper.DeleteAttribute("00CDE4AE-DD60-4343-B053-DA7C913568A4");

            // Attribute for BlockType
            //   BlockType: Acme Certificates
            //   Category: Blue Box Moon > Acme Certificate
            //   Attribute: Redirect Override
            RockMigrationHelper.DeleteAttribute("352D8C47-0E92-4275-9322-DF95F5CAC192");

            // Delete BlockType 
            //   Name: Add Connection Request
            //   Category: Mobile > Connection
            //   Path: -
            //   EntityType: Add Connection Request
            RockMigrationHelper.DeleteBlockType("1380115A-B3F0-49BC-A6BC-432A59DC27A2");
        }
    }
}
