//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class FinancialBatch : RockMigration_5
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddDefinedType( "Financial", "Batch Type", "Batch Types", "9E358FBE-2321-4C54-895F-C888E29298AE" );
            AddDefinedValue( "9E358FBE-2321-4C54-895F-C888E29298AE", "ACH", "ACH", "E6F877F3-D2CC-443E-976A-4402502F544F" );
            AddDefinedValue( "9E358FBE-2321-4C54-895F-C888E29298AE", "Visa", "Visa", "24CC2E82-B2B6-4037-87AE-39EEAFE06712" );
            AddDefinedValue( "9E358FBE-2321-4C54-895F-C888E29298AE", "MasterCard", "MasterCard", "50F625F8-F1BE-4FA0-B99F-3FA852D87DD1" );
            AddDefinedValue( "9E358FBE-2321-4C54-895F-C888E29298AE", "Discover", "Discover", "18DF8254-0C68-4FE0-973E-C0B1767EFD3F" );
            AddDefinedValue( "9E358FBE-2321-4C54-895F-C888E29298AE", "Amex", "Amex", "378D8EAD-7FA6-4D0D-862D-ED6E04B17770" );
            AddDefinedValue( "9E358FBE-2321-4C54-895F-C888E29298AE", "PayPal", "PayPal", "4832DA18-DD18-477F-BFDB-ABFC28FE5743" );

            //add the pages 
            AddPage( "18C9E5C3-3E28-4AA3-84F6-78CD4EA2DD3C", "Financial Batch", "Display financial batches", "Ef65EFF2-99AC-4081-8E09-32A04518683A" );
            AddPage( "Ef65EFF2-99AC-4081-8E09-32A04518683A", "Financial Batch Detail", "Display financial batch detail", "606BDA31-A8FE-473A-B3F8-A00ECF7E06EC" );
            AddPage( "606BDA31-A8FE-473A-B3F8-A00ECF7E06EC", "Financial Transaction Detail ", "Display financial transactions detail", "B67E38CB-2EF1-43EA-863A-37DAA1C7340F" );

            // add the block types
            AddBlockType( "Finance - Batch List", "", "~/Blocks/Finance/BatchList.ascx", "AB345CE7-5DC6-41AF-BBDC-8D23D52AFE25" );
            AddBlockType( "Finance - Batch Detail", "", "~/Blocks/Finance/BatchDetail.ascx", "CE34CE43-2CCF-4568-9AEB-3BE203DB3470" );
            AddBlockType( "Finance - Transaction Detail", "", "~/Blocks/Finance/TransactionDetail.ascx", "1DE16F87-4A49-4A3C-A03E-B8488ECBEEBE" );

            // add blocks
            AddBlock( "Ef65EFF2-99AC-4081-8E09-32A04518683A", "AB345CE7-5DC6-41AF-BBDC-8D23D52AFE25", "Financial Batch", "", "Content", 0, "59F60553-095C-4DC5-9BF0-A5A1289D878B" );
            AddBlock( "606BDA31-A8FE-473A-B3F8-A00ECF7E06EC", "CE34CE43-2CCF-4568-9AEB-3BE203DB3470", "Batch Detail", "", "Content", 0, "E7C8C398-0E1D-4BCE-BC54-A02957228514" );
            AddBlock( "B67E38CB-2EF1-43EA-863A-37DAA1C7340F", "1DE16F87-4A49-4A3C-A03E-B8488ECBEEBE", "Transaction Detail", "", "Content", 0, "F125E7EB-DA78-4840-9D00-4C8DD0DD4A27" );

            AddBlockTypeAttribute( "AB345CE7-5DC6-41AF-BBDC-8D23D52AFE25", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "Advanced", "", 0, "", "2290806C-9E87-4960-9019-D4D7327591BB" );
            // Attrib Value for FinancialBatch:Detail Page Guid
            AddBlockAttributeValue( "59F60553-095C-4DC5-9BF0-A5A1289D878B", "2290806C-9E87-4960-9019-D4D7327591BB", "606BDA31-A8FE-473A-B3F8-A00ECF7E06EC" );

            AddBlockTypeAttribute( "CE34CE43-2CCF-4568-9AEB-3BE203DB3470", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "", "", 0, "", "EC445D46-24FE-4338-9005-25D13D65347D" );
            // Attrib Value for Batch Detail:Detail Page Guid
            AddBlockAttributeValue( "E7C8C398-0E1D-4BCE-BC54-A02957228514", "EC445D46-24FE-4338-9005-25D13D65347D", "B67E38CB-2EF1-43EA-863A-37DAA1C7340F" );

            AddBlockTypeAttribute( "AB345CE7-5DC6-41AF-BBDC-8D23D52AFE25", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "", "", 0, "", "9E427F58-2F87-498C-AD65-4A1C3DF38E6F" );
            // Attrib Value for Financial Batch:Detail Page Guid
            AddBlockAttributeValue( "59F60553-095C-4DC5-9BF0-A5A1289D878B", "9E427F58-2F87-498C-AD65-4A1C3DF38E6F", "606bda31-a8fe-473a-b3f8-a00ecf7e06ec" );

            AddBlockTypeAttribute( "1DE16F87-4A49-4A3C-A03E-B8488ECBEEBE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "", "", 0, "", "4635F59E-E41E-4695-8858-291B88B439CF" );
            // Attrib Value for Transaction Detail:Detail Page Guid
            AddBlockAttributeValue( "F125E7EB-DA78-4840-9D00-4C8DD0DD4A27", "4635F59E-E41E-4695-8858-291B88B439CF", "606bda31-a8fe-473a-b3f8-a00ecf7e06ec" );


        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteDefinedType( "9E358FBE-2321-4C54-895F-C888E29298AE" ); // Financial Batch Types

            DeleteAttribute( "EC445D46-24FE-4338-9005-25D13D65347D" ); // Detail Page Guid
            DeleteAttribute( "2290806C-9E87-4960-9019-D4D7327591BB" ); // Detail Page Guid
            DeleteAttribute( "9E427F58-2F87-498C-AD65-4A1C3DF38E6F" ); // Detail Page Guid
            DeleteAttribute( "4635F59E-E41E-4695-8858-291B88B439CF" ); // Detail Page Guid

            DeleteBlock( "59F60553-095C-4DC5-9BF0-A5A1289D878B" ); // Financial Batch
            DeleteBlock( "E7C8C398-0E1D-4BCE-BC54-A02957228514" );
            DeleteBlock( "F125E7EB-DA78-4840-9D00-4C8DD0DD4A27" );

            DeleteBlockType( "AB345CE7-5DC6-41AF-BBDC-8D23D52AFE25" );
            DeleteBlockType( "ce34ce43-2ccf-4568-9aeb-3be203db3470" );
            DeleteBlockType( "1DE16F87-4A49-4A3C-A03E-B8488ECBEEBE" );

            DeletePage( "B67E38CB-2EF1-43EA-863A-37DAA1C7340F" );
            DeletePage( "606BDA31-A8FE-473A-B3F8-A00ECF7E06EC" );
            DeletePage( "Ef65EFF2-99AC-4081-8E09-32A04518683A" );
        }
    }
}
