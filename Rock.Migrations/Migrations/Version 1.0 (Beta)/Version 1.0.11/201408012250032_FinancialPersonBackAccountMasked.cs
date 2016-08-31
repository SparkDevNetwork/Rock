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
    public partial class FinancialPersonBackAccountMasked : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.FinancialPersonBankAccount", "AccountNumberMasked", c => c.String(nullable: false));
            
            // Transaction Matching and Bank Account List related
            RockMigrationHelper.AddPage( "606BDA31-A8FE-473A-B3F8-A00ECF7E06EC", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Transaction Matching", "", "CD18FE52-8D6A-49C9-81BF-DF97C5BA0302", "fa fa-money" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Transaction Matching", "Used to match transactions to an individual and allocate the check amount to financial account(s).", "~/Blocks/Finance/TransactionMatching.ascx", "Finance", "1A8BEE2A-E5BE-4BA5-AFDB-E9C9278419BA" );
            RockMigrationHelper.UpdateBlockType( "Bank Account List", "Lists bank accounts for a person", "~/Blocks/Crm/PersonDetail/BankAccountList.ascx", "CRM > Person Detail", "C4191011-0391-43DF-9A9D-BE4987C679A4" );
            
            // Add Block to Page: Transaction Matching, Site: Rock RMS
            RockMigrationHelper.AddBlock( "CD18FE52-8D6A-49C9-81BF-DF97C5BA0302", "", "1A8BEE2A-E5BE-4BA5-AFDB-E9C9278419BA", "Transaction Matching", "Main", "", "", 0, "A18A0A0A-0B71-43B4-B830-44B802C272D4" );

            // Add Block to Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlock( "53CF4CBE-85F9-4A50-87D7-0D72A3FB2892", "", "C4191011-0391-43DF-9A9D-BE4987C679A4", "Bank Account List", "SectionC1", "", "", 2, "7C698D61-81C9-4942-BFE3-9839130C1A3E" );

            // Attrib for BlockType: Transaction Matching:Accounts
            RockMigrationHelper.AddBlockTypeAttribute( "1A8BEE2A-E5BE-4BA5-AFDB-E9C9278419BA", "17033CDD-EF97-4413-A483-7B85A787A87F", "Accounts", "Accounts", "", "Select the accounts that check amounts can be allocated to.  Leave blank to show all accounts", 0, @"", "1EA5E62A-0FFE-4427-A0D1-1624A9478440" );

            // Attrib for BlockType: Transaction Matching:Add Family Link
            RockMigrationHelper.AddBlockTypeAttribute( "1A8BEE2A-E5BE-4BA5-AFDB-E9C9278419BA", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Add Family Link", "AddFamilyLink", "", "Select the page where a new family can be added. If specified, a link will be shown which will open in a new window when clicked", 0, @"6a11a13d-05ab-4982-a4c2-67a8b1950c74,af36e4c2-78c6-4737-a983-e7a78137ddc7", "D4909540-9D01-4BC6-8CFF-851BE101A821" );

            
            //// Misc Catchups
            
            //
            RockMigrationHelper.UpdateBlockType( "Scheduled Transaction Summary", "Block that shows a summary of the scheduled transactions for the currently logged in user.", "~/Blocks/Finance/ScheduledTransactionSummary.ascx", "Finance", "3FC83F0E-8BAA-4CB3-BAD0-0CFBE0E621AA" );

            // Attrib for BlockType: Batch List:Show Accounting Code
            RockMigrationHelper.AddBlockTypeAttribute( "AB345CE7-5DC6-41AF-BBDC-8D23D52AFE25", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Accounting Code", "ShowAccountingCode", "", "Should the accounting code column be displayed.", 1, @"False", "E155892B-D165-4455-9B04-8A8E3B7240D3" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //// Misc Catchups
            RockMigrationHelper.DeleteBlockType( "3FC83F0E-8BAA-4CB3-BAD0-0CFBE0E621AA" ); // Scheduled Transaction Summary
            // Attrib for BlockType: Batch List:Show Accounting Code
            RockMigrationHelper.DeleteAttribute( "E155892B-D165-4455-9B04-8A8E3B7240D3" );
            
            //// Transaction Matching...
            
            // Attrib for BlockType: Transaction Matching:Add Family Link
            RockMigrationHelper.DeleteAttribute( "D4909540-9D01-4BC6-8CFF-851BE101A821" );
            // Attrib for BlockType: Transaction Matching:Accounts
            RockMigrationHelper.DeleteAttribute( "1EA5E62A-0FFE-4427-A0D1-1624A9478440" );
            
            // Remove Block: Bank Account List, from Page: Contributions, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "7C698D61-81C9-4942-BFE3-9839130C1A3E" );
            // Remove Block: Transaction Matching, from Page: Transaction Matching, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "A18A0A0A-0B71-43B4-B830-44B802C272D4" );
            
            RockMigrationHelper.DeleteBlockType( "C4191011-0391-43DF-9A9D-BE4987C679A4" ); // Bank Account List
            RockMigrationHelper.DeleteBlockType( "1A8BEE2A-E5BE-4BA5-AFDB-E9C9278419BA" ); // Transaction Matching
            
            RockMigrationHelper.DeletePage( "CD18FE52-8D6A-49C9-81BF-DF97C5BA0302" ); //  Page: Transaction Matching, Layout: Full Width, Site: Rock RMS
            
            DropColumn("dbo.FinancialPersonBankAccount", "AccountNumberMasked");
        }
    }
}
