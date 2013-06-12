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
    public partial class TransactionsPageUpdate : RockMigration_5
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Rename Financial Transactions to Transaction List
            DeleteBlock( "B447AB11-3A19-4527-921A-2266A6B4E181" );
            DeleteBlockType( "E04320BC-67C3-452D-9EF6-D74D8C177154" );
            AddBlockType( "Finance - Transaction List", "View and search financial transactions", "~/Blocks/Finance/TransactionList.ascx", "E04320BC-67C3-452D-9EF6-D74D8C177154" );
            AddBlock( "7CA317B5-5C47-465D-B407-7D614F2A568F", "E04320BC-67C3-452D-9EF6-D74D8C177154", "Financial Transaction", "", "Content", 0, "B447AB11-3A19-4527-921A-2266A6B4E181" );
                        
            // Move Transaction Detail to be with Transaction List
            DeletePage( "B67E38CB-2EF1-43EA-863A-37DAA1C7340F" ); // Transaction Detail
            AddPage( "7CA317B5-5C47-465D-B407-7D614F2A568F", "Transaction Detail ", "Display financial transactions detail", "B67E38CB-2EF1-43EA-863A-37DAA1C7340F" );

            AddPage( "606BDA31-A8FE-473A-B3F8-A00ECF7E06EC", "Transaction Detail", "", "Default", "97716641-D003-4663-9EA2-D9BB94E7955B" );
            AddBlock( "97716641-D003-4663-9EA2-D9BB94E7955B", "1DE16F87-4A49-4A3C-A03E-B8488ECBEEBE", "Transaction Detail", "", "Content", 0, "25F645D5-50B9-4DCC-951F-C780C49CD913" );

            // Relink Transaction Detail
            DeleteBlock( "F125E7EB-DA78-4840-9D00-4C8DD0DD4A27" );
            AddBlock( "B67E38CB-2EF1-43EA-863A-37DAA1C7340F", "1DE16F87-4A49-4A3C-A03E-B8488ECBEEBE", "Transaction Detail", "", "Content", 0, "F125E7EB-DA78-4840-9D00-4C8DD0DD4A27" );
            
            AddBlockTypeAttribute( "E04320BC-67C3-452D-9EF6-D74D8C177154", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "", "", 0, "", "C6D07A89-84C9-412A-A584-E37E59506566" );
            // Attrib Value for Financial Transaction:Detail Page Guid
            AddBlockAttributeValue( "B447AB11-3A19-4527-921A-2266A6B4E181", "C6D07A89-84C9-412A-A584-E37E59506566", "B67E38CB-2EF1-43EA-863A-37DAA1C7340F" );

            DeleteAttribute( "EC445D46-24FE-4338-9005-25D13D65347D" );
            AddBlockTypeAttribute( "CE34CE43-2CCF-4568-9AEB-3BE203DB3470", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "", "", 0, "", "EC445D46-24FE-4338-9005-25D13D65347D" );
            // Attrib Value for Batch Detail:Detail Page Guid
            AddBlockAttributeValue( "E7C8C398-0E1D-4BCE-BC54-A02957228514", "EC445D46-24FE-4338-9005-25D13D65347D", "97716641-d003-4663-9ea2-d9bb94e7955b" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "EC445D46-24FE-4338-9005-25D13D65347D" ); // Detail Page Guid
            DeleteAttribute( "C6D07A89-84C9-412A-A584-E37E59506566" ); // Detail Page Guid
            DeleteBlock( "25F645D5-50B9-4DCC-951F-C780C49CD913" ); // Transaction Detail
            DeletePage( "97716641-D003-4663-9EA2-D9BB94E7955B" ); // Transaction Detail

            // Move Transaction Detail back under Financial Batch
            DeletePage( "B67E38CB-2EF1-43EA-863A-37DAA1C7340F" ); // Transaction Detail
            AddPage( "606BDA31-A8FE-473A-B3F8-A00ECF7E06EC", "Financial Transaction Detail ", "Display financial transactions detail", "B67E38CB-2EF1-43EA-863A-37DAA1C7340F" );

            // Rename Financial Transactions to generic "Transactions"
            DeleteBlock( "B447AB11-3A19-4527-921A-2266A6B4E181" );
            DeleteBlockType( "E04320BC-67C3-452D-9EF6-D74D8C177154" );
            AddBlockType( "Finance - Transactions", "View and search financial transactions", "~/Blocks/Finance/Transactions.ascx", "E04320BC-67C3-452D-9EF6-D74D8C177154" );
            AddBlock( "7CA317B5-5C47-465D-B407-7D614F2A568F", "E04320BC-67C3-452D-9EF6-D74D8C177154", "Financial Transaction", "", "Content", 0, "B447AB11-3A19-4527-921A-2266A6B4E181" );

            AddBlockTypeAttribute( "1DE16F87-4A49-4A3C-A03E-B8488ECBEEBE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "", "", 0, "", "4635F59E-E41E-4695-8858-291B88B439CF" );
            // Attrib Value for Transaction Detail:Detail Page Guid
            AddBlockAttributeValue( "F125E7EB-DA78-4840-9D00-4C8DD0DD4A27", "4635F59E-E41E-4695-8858-291B88B439CF", "606BDA31-A8FE-473A-B3F8-A00ECf7E06EC" );

            AddBlockTypeAttribute( "CE34CE43-2CCF-4568-9AEB-3BE203DB3470", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "", "", 0, "", "EC445D46-24FE-4338-9005-25D13D65347D" );
            // Attrib Value for Batch Detail:Detail Page Guid
            AddBlockAttributeValue( "E7C8C398-0E1D-4BCE-BC54-A02957228514", "EC445D46-24FE-4338-9005-25D13D65347D", "B67E38CB-2EF1-43EA-863A-37DAA1C7340F" );

            // Relink Transaction Detail
            DeleteBlock( "F125E7EB-DA78-4840-9D00-4C8DD0DD4A27" );
            AddBlock( "B67E38CB-2EF1-43EA-863A-37DAA1C7340F", "1DE16F87-4A49-4A3C-A03E-B8488ECBEEBE", "Transaction Detail", "", "Content", 0, "F125E7EB-DA78-4840-9D00-4C8DD0DD4A27" );

        }
    }
}
