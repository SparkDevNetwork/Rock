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
    public partial class CoverTheFees : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Page Transaction Fee Report to Site:Rock RMS
            RockMigrationHelper.AddPage( true, "142627AE-6590-48E3-BFCA-3669260B8CF2", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Transaction Fee Report", "", "A3E321E9-2FBB-4BB9-8AEE-E810B7CC5914", "" );

            // Add/Update BlockType Transaction Fee Report
            RockMigrationHelper.UpdateBlockType( "Transaction Fee Report", "Block that reports transaction fees.", "~/Blocks/Finance/TransactionFeeReport.ascx", "Finance", "D75AF7AE-94B8-4604-B768-A124A2F55449" );

            // Add Block Transaction Fee Report to Page: Transaction Fee Report, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "A3E321E9-2FBB-4BB9-8AEE-E810B7CC5914".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D75AF7AE-94B8-4604-B768-A124A2F55449".AsGuid(), "Transaction Fee Report", "Main", @"", @"", 0, "BD815511-61F4-4A4B-AE91-9DA49D3D6CB8" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Transaction Fee Report, from Page: Transaction Fee Report, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "BD815511-61F4-4A4B-AE91-9DA49D3D6CB8" );

            // Delete BlockType Transaction Fee Report
            RockMigrationHelper.DeleteBlockType( "D75AF7AE-94B8-4604-B768-A124A2F55449" ); // Transaction Fee Report

            // Delete Page Transaction Fee Report from Site:Rock RMS
            RockMigrationHelper.DeletePage( "A3E321E9-2FBB-4BB9-8AEE-E810B7CC5914" ); //  Page: Transaction Fee Report, Layout: Full Width, Site: Rock RMS
        }
    }
}
