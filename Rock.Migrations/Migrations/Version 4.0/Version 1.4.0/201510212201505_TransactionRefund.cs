// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class TransactionRefund : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    UPDATE [BenevolenceRequest] SET
        [HomePhoneNumber] = LEFT([HomePhoneNumber],20),
	    [CellPhoneNumber] = LEFT([CellPhoneNumber],20),
	    [WorkPhoneNumber] = LEFT([WorkPhoneNumber],20),
	    [GovernmentId] = LEFT([GovernmentId],100)
" );

            AddColumn("dbo.Person", "RecordStatusLastModifiedDateTime", c => c.DateTime());
            AddColumn("dbo.FinancialTransactionRefund", "OriginalTransactionId", c => c.Int());
            AlterColumn("dbo.BenevolenceRequest", "HomePhoneNumber", c => c.String(maxLength: 20));
            AlterColumn("dbo.BenevolenceRequest", "CellPhoneNumber", c => c.String(maxLength: 20));
            AlterColumn("dbo.BenevolenceRequest", "WorkPhoneNumber", c => c.String(maxLength: 20));
            AlterColumn("dbo.BenevolenceRequest", "GovernmentId", c => c.String(maxLength: 100));
            CreateIndex("dbo.FinancialTransactionRefund", "OriginalTransactionId");
            AddForeignKey("dbo.FinancialTransactionRefund", "OriginalTransactionId", "dbo.FinancialTransaction", "Id");

            // Attrib for BlockType: Transaction Entry:Payment Comment
            RockMigrationHelper.AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Payment Comment", "PaymentComment", "", "The comment to include with the payment transaction when sending to Gateway", 28, @"Online Contribution", "1F37A67E-AAA5-4D2F-918D-DDA11C00CCC1" );
            // Attrib for BlockType: Transaction Detail:Refund Batch Name Suffix
            RockMigrationHelper.AddBlockTypeAttribute( "1DE16F87-4A49-4A3C-A03E-B8488ECBEEBE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Refund Batch Name Suffix", "RefundBatchNameSuffix", "", "The suffix to append to new batch name when refunding transactions. If left blank, the batch name will be the same as the original transaction's batch name.", 3, @" - Refund", "2FC63A9E-5A5B-46E9-9B49-306571526440" );
            // Attrib for BlockType: Transaction Detail:Registration Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "1DE16F87-4A49-4A3C-A03E-B8488ECBEEBE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Detail Page", "RegistrationDetailPage", "", "Page used to view an event registration.", 2, @"", "59461EEC-5E7D-42E5-BC0C-29CC8950AFAC" );
            // Attrib for BlockType: Registration Entry:Confirm Account Template
            RockMigrationHelper.AddBlockTypeAttribute( "CABD2BFB-DFFF-42CD-BF1A-14F3BEE583DD", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF", "Confirm Account Template", "ConfirmAccountTemplate", "", "Confirm Account Email Template", 4, @"17aaceef-15ca-4c30-9a3a-11e6cf7e6411", "7023DC3C-DED9-4CBE-B760-9DF9BA5E3B2D" );

            // Attrib for BlockType: Group List:Display Group Path
            RockMigrationHelper.AddBlockTypeAttribute( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Group Path", "DisplayGroupPath", "", "Should the Group path be displayed?", 4, @"False", "6F229535-B44E-44C2-A9AF-28244600E244" );

            // Attrib Value for Block:Transaction Detail, Attribute:Refund Batch Name Suffix Page: Transaction Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "25F645D5-50B9-4DCC-951F-C780C49CD913", "2FC63A9E-5A5B-46E9-9B49-306571526440", @"- Refund" );
            // Attrib Value for Block:Transaction Detail, Attribute:Registration Detail Page Page: Transaction Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "25F645D5-50B9-4DCC-951F-C780C49CD913", "59461EEC-5E7D-42E5-BC0C-29CC8950AFAC", @"fc81099a-2f98-4eba-ac5a-8300b2fe46c4" );
            // Attrib Value for Block:Transaction Detail, Attribute:Refund Batch Name Suffix Page: Transaction Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F125E7EB-DA78-4840-9D00-4C8DD0DD4A27", "2FC63A9E-5A5B-46E9-9B49-306571526440", @"- Refund" );
            // Attrib Value for Block:Transaction Detail, Attribute:Registration Detail Page Page: Transaction Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F125E7EB-DA78-4840-9D00-4C8DD0DD4A27", "59461EEC-5E7D-42E5-BC0C-29CC8950AFAC", @"fc81099a-2f98-4eba-ac5a-8300b2fe46c4" );

            Sql( @"
    DROP INDEX [IX_EntityTypeId_EntityTypeQualifierColumn_EntityTypeQualifierValue_Key] ON [dbo].[Attribute]

    -- a sql server index entry cannot be longer than 900 bytes (450 chars), so move the [Key] to an include column on the index
    CREATE NONCLUSTERED INDEX [IX_EntityTypeId_EntityTypeQualifierColumn_EntityTypeQualifierValue_Key] ON [dbo].[Attribute] (
        [EntityTypeId] ASC
        ,[EntityTypeQualifierColumn] ASC
        ,[EntityTypeQualifierValue] ASC
        ) INCLUDE ([Key])
" );
            Sql( @"
    DELETE FROM [DataViewFilter]
    WHERE [Guid] = '841466E9-FB82-4AC1-BAFB-87B6153CE3EF'
" );

            Sql( @"
    UPDATE WorkflowActionForm SET Actions = replace(Actions, 'Your information has been submitted succesfully', 'Your information has been submitted successfully') where Actions like '%succesfully%'
" );

            // Attrib for BlockType: Transaction List:Image Height
            RockMigrationHelper.AddBlockTypeAttribute( "E04320BC-67C3-452D-9EF6-D74D8C177154", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Image Height", "ImageHeight", "", "If the Show Images option is selected, the image height", 4, @"200", "00EBFDFE-C6AE-48F2-B284-809D1765D489" );
            // Attrib for BlockType: Transaction List:Show Options
            RockMigrationHelper.AddBlockTypeAttribute( "E04320BC-67C3-452D-9EF6-D74D8C177154", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Options", "ShowOptions", "", "Show an Options button in the title panel for showing images or summary.", 3, @"False", "0227D124-D207-4F68-8B77-4A4A88CBBE6F" );
            // Attrib Value for Block:Transaction List, Attribute:Show Options Page: Financial Batch Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "1133795B-3325-4D81-B603-F442F0AC892B", "0227D124-D207-4F68-8B77-4A4A88CBBE6F", @"True" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.FinancialTransactionRefund", "OriginalTransactionId", "dbo.FinancialTransaction");
            DropIndex("dbo.FinancialTransactionRefund", new[] { "OriginalTransactionId" });
            AlterColumn("dbo.BenevolenceRequest", "GovernmentId", c => c.String());
            AlterColumn("dbo.BenevolenceRequest", "WorkPhoneNumber", c => c.String());
            AlterColumn("dbo.BenevolenceRequest", "CellPhoneNumber", c => c.String());
            AlterColumn("dbo.BenevolenceRequest", "HomePhoneNumber", c => c.String());
            DropColumn("dbo.FinancialTransactionRefund", "OriginalTransactionId");
            DropColumn("dbo.Person", "RecordStatusLastModifiedDateTime");
        }
    }
}
