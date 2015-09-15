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
    public partial class InternalPrayerPagesReorg : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // MP: Missing DateTime index on FinancialBatch
            Sql( @"IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_BatchStartDateTime' AND object_id = OBJECT_ID('FinancialBatch'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_BatchStartDateTime] ON [dbo].[FinancialBatch]
    (
        [BatchStartDateTime] ASC
    )
END" );

            // MP: Index tuneup on FinancialTransaction
            Sql( @"/* Reorder the Index Fields and add some INCLUDE columns to increase performance */
DROP INDEX [IX_TransactionDateTime_TransactionTypeValueId_Person] ON [dbo].[FinancialTransaction]
GO
CREATE NONCLUSTERED INDEX [IX_TransactionDateTime_TransactionTypeValueId_Person] ON [dbo].[FinancialTransaction] (
    [TransactionTypeValueId] ASC
    ,[AuthorizedPersonAliasId] ASC
    ,[TransactionDateTime] ASC
    ) INCLUDE (
    [Id]
    ,[Summary]
    ,[FinancialPaymentDetailId]
    )" );

            // DT: Allow staff to add registration instances
            RockMigrationHelper.AddSecurityAuthForBlock( "467FA6BC-7F52-4AB7-87CC-B16518B0FF6F", 0, "Action", true, "2C112948-FF4C-46E7-981A-0257681EADF4", Model.SpecialRole.None, "324C07A9-206F-43C6-9B20-B5BD6B8C0983" );
            RockMigrationHelper.AddSecurityAuthForBlock( "467FA6BC-7F52-4AB7-87CC-B16518B0FF6F", 1, "Action", true, "300BA2C8-49A3-44BA-A82A-82E3FD8C3745", Model.SpecialRole.None, "E61E6301-6F5A-4E6A-9564-E7FE89C5925F" );
            RockMigrationHelper.AddSecurityAuthForPage( "844DC54B-DAEC-47B3-A63A-712DD6D57793", 0, "Action", true, "2C112948-FF4C-46E7-981A-0257681EADF4", 0, "00313D30-3926-411F-86E4-A3F90428574E" );
            RockMigrationHelper.AddSecurityAuthForPage( "844DC54B-DAEC-47B3-A63A-712DD6D57793", 1, "Action", true, "300BA2C8-49A3-44BA-A82A-82E3FD8C3745", 0, "E2A8851E-5D30-4F17-BB06-0DF0D61A88E2" );


            // MP: Reorg Internal Prayer Pages
            // delete PrayerRequestList and PrayerCommentList blocks from Prayer Management page, and move than to subpages...
            RockMigrationHelper.DeleteBlock( "601DDB93-555D-4E08-AAA3-EE0807BFD3E1" );
            RockMigrationHelper.DeleteBlock( "E27EA67E-AB6E-4F61-A03B-D7697BBE922C" );
            RockMigrationHelper.AddPage( "1A3437C8-D4CB-4329-A366-8D6A4CBF79BF", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Add Prayer Request", "", "36E22C5D-FC31-4754-8583-B63079217528", "fa fa-plus-circle" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "1A3437C8-D4CB-4329-A366-8D6A4CBF79BF", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Prayer Requests", "", "3149959B-EFAC-4C2D-B0E8-8CF4FA1BB2FF", "fa fa-cloud-upload", insertAfterPageGuid: "36E22C5D-FC31-4754-8583-B63079217528" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "1A3437C8-D4CB-4329-A366-8D6A4CBF79BF", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Prayer Comments", "", "D10364AD-5E65-484B-967C-B52475E91B4C", "fa fa-comment", insertAfterPageGuid: "3149959B-EFAC-4C2D-B0E8-8CF4FA1BB2FF" ); // Site:Rock RMS
            // move Prayer Request Detail under new PrayerRequests Page
            Sql( @"UPDATE [Page]
SET ParentPageId = (
        SELECT TOP 1 [Id]
        FROM [Page]
        WHERE [Guid] = '3149959B-EFAC-4C2D-B0E8-8CF4FA1BB2FF'
        )
WHERE [Guid] = '89C3DB4A-BAFD-45C8-88C6-45D8FEC48B48'
" );
            // Add Block to Page: Prayer, Site: Rock RMS
            RockMigrationHelper.AddBlock( "1A3437C8-D4CB-4329-A366-8D6A4CBF79BF", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Prayer Page Menu", "Main", "", "", 0, "B3D8987F-6860-48AB-AFC4-8DB5E2FA4582" );
            // Add Block to Page: Prayer Comments, Site: Rock RMS
            RockMigrationHelper.AddBlock( "D10364AD-5E65-484B-967C-B52475E91B4C", "", "DF0F5743-2BFF-40C0-8BEE-39F1DE7B4C22", "Prayer Comment List", "Main", "", "", 0, "4FA79CAF-0079-42A8-BDC8-2207D739F09C" );
            // Add Block to Page: Prayer Requests, Site: Rock RMS
            RockMigrationHelper.AddBlock( "3149959B-EFAC-4C2D-B0E8-8CF4FA1BB2FF", "", "4D6B686A-79DF-4EFC-A8BA-9841C248BF74", "Prayer Request List", "Main", "", "", 0, "DF3E6819-CAA5-46A4-B287-C268E9D7D981" );
            // Add Block to Page: Add Prayer Request, Site: Rock RMS
            RockMigrationHelper.AddBlock( "36E22C5D-FC31-4754-8583-B63079217528", "", "F791046A-333F-4B2A-9815-73B60326162D", "Add Prayer Request", "Main", "", "", 0, "0B227471-EBD0-4310-9F9F-A5B2E9A8CDE9" );
            // Attrib Value for Block:Prayer Page Menu, Attribute:CSS File Page: Prayer, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B3D8987F-6860-48AB-AFC4-8DB5E2FA4582", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" );
            // Attrib Value for Block:Prayer Page Menu, Attribute:Include Current Parameters Page: Prayer, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B3D8987F-6860-48AB-AFC4-8DB5E2FA4582", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );
            // Attrib Value for Block:Prayer Page Menu, Attribute:Number of Levels Page: Prayer, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B3D8987F-6860-48AB-AFC4-8DB5E2FA4582", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" );
            // Attrib Value for Block:Prayer Page Menu, Attribute:Include Current QueryString Page: Prayer, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B3D8987F-6860-48AB-AFC4-8DB5E2FA4582", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );
            // Attrib Value for Block:Prayer Page Menu, Attribute:Template Page: Prayer, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B3D8987F-6860-48AB-AFC4-8DB5E2FA4582", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsBlocks.lava' %}" );
            // Attrib Value for Block:Prayer Page Menu, Attribute:Root Page Page: Prayer, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B3D8987F-6860-48AB-AFC4-8DB5E2FA4582", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"1a3437c8-d4cb-4329-a366-8d6a4cbf79bf" );
            // Attrib Value for Block:Prayer Page Menu, Attribute:Is Secondary Block Page: Prayer, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B3D8987F-6860-48AB-AFC4-8DB5E2FA4582", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );
            // Attrib Value for Block:Prayer Page Menu, Attribute:Enable Debug Page: Prayer, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B3D8987F-6860-48AB-AFC4-8DB5E2FA4582", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" );
            // Attrib Value for Block:Prayer Page Menu, Attribute:Include Page List Page: Prayer, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B3D8987F-6860-48AB-AFC4-8DB5E2FA4582", "0A49DABE-42EE-40E5-9E06-0E6530944865", @"" );
            // Attrib Value for Block:Prayer Comment List, Attribute:Category Selection Page: Prayer Comments, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4FA79CAF-0079-42A8-BDC8-2207D739F09C", "960D7BBF-E737-42A4-B372-0E7A24050BF3", @"" );
            // Attrib Value for Block:Prayer Comment List, Attribute:Detail Page Page: Prayer Comments, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4FA79CAF-0079-42A8-BDC8-2207D739F09C", "8F9A2D85-E2AE-46A8-B64E-ADE8AC0555E8", @"89c3db4a-bafd-45c8-88c6-45d8fec48b48" );
            // Attrib Value for Block:Prayer Request List, Attribute:Detail Page Page: Prayer Requests, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DF3E6819-CAA5-46A4-B287-C268E9D7D981", "A1275318-F6B4-4C66-B782-99037E6E16C0", @"89c3db4a-bafd-45c8-88c6-45d8fec48b48" );
            // Attrib Value for Block:Prayer Request List, Attribute:Expires After (Days) Page: Prayer Requests, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DF3E6819-CAA5-46A4-B287-C268E9D7D981", "7FEB9A26-6AE6-41F8-B6B5-D5E432C832D0", @"14" );
            // Add/Update PageContext for Page:Prayer Request Detail, Entity: Rock.Model.PrayerRequest, Parameter: prayerRequestId
            RockMigrationHelper.UpdatePageContext( "89C3DB4A-BAFD-45C8-88C6-45D8FEC48B48", "Rock.Model.PrayerRequest", "prayerRequestId", "38DD91EC-19F4-4323-9204-8617ED57E4F3" );

            // MP: Fix Security for Deleting from CheckScanner
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransaction", 1, "Edit", true, "2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9", 0, "48119D27-27D3-4EEA-B7F8-F5092DE9534D" ); // EntityType:Rock.Model.FinancialTransaction Group: 2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9 ( RSR - Finance Worker ), 
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransaction", 0, "Edit", true, "6246A7EF-B7A3-4C8C-B1E4-3FF114B84559", 0, "BFE1DF95-90B8-4760-85FE-839FB06CBC41" ); // EntityType:Rock.Model.FinancialTransaction Group: 6246A7EF-B7A3-4C8C-B1E4-3FF114B84559 ( RSR - Finance Administration ), 
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // MP: Fix Security for Deleting from CheckScanner DOWN
            RockMigrationHelper.DeleteSecurityAuth( "48119D27-27D3-4EEA-B7F8-F5092DE9534D" ); // EntityType:Rock.Model.FinancialTransaction Group: 2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9 ( RSR - Finance Worker ), 
            RockMigrationHelper.DeleteSecurityAuth( "BFE1DF95-90B8-4760-85FE-839FB06CBC41" ); // EntityType:Rock.Model.FinancialTransaction Group: 6246A7EF-B7A3-4C8C-B1E4-3FF114B84559 ( RSR - Finance Administration ), 

            
            // MP: Reorg Internal Prayer Pages DOWN
            /* NOTE: This Down won't restore the old Prayer Blocks to their orig location, but it will at least let you re-run the Up() */
            // Remove Block: Add Prayer Request, from Page: Add Prayer Request, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "0B227471-EBD0-4310-9F9F-A5B2E9A8CDE9" );
            // Remove Block: Prayer Request List, from Page: Prayer Requests, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "DF3E6819-CAA5-46A4-B287-C268E9D7D981" );
            // Remove Block: Prayer Comment List, from Page: Prayer Comments, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "4FA79CAF-0079-42A8-BDC8-2207D739F09C" );
            // UN-move Prayer Request Detail under new PrayerRequests Page
            Sql( @"UPDATE [Page]
SET ParentPageId = NULL
WHERE [Guid] = '89C3DB4A-BAFD-45C8-88C6-45D8FEC48B48'
" );

            // Remove Block: Prayer Page Menu, from Page: Prayer, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B3D8987F-6860-48AB-AFC4-8DB5E2FA4582" );
            RockMigrationHelper.DeletePage( "36E22C5D-FC31-4754-8583-B63079217528" ); //  Page: Add Prayer Request, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "D10364AD-5E65-484B-967C-B52475E91B4C" ); //  Page: Prayer Comments, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "3149959B-EFAC-4C2D-B0E8-8CF4FA1BB2FF" ); //  Page: Prayer Requests, Layout: Full Width, Site: Rock RMS
        }
    }
}
