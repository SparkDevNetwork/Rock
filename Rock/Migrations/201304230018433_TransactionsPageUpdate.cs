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
    public partial class TransactionsPageUpdate : RockMigration
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

            AddBlockTypeAttribute( "E04320BC-67C3-452D-9EF6-D74D8C177154", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "", "", 0, "", "9F32738F-EA63-49F8-B2AB-1BB324C98057" );
            // Attrib Value for Financial Transaction:Detail Page Guid
            AddBlockAttributeValue( "B447AB11-3A19-4527-921A-2266A6B4E181", "9F32738F-EA63-49F8-B2AB-1BB324C98057", "B67E38CB-2EF1-43EA-863A-37DAA1C7340F" );

            // Relink Transaction Detail
            DeleteBlock( "F125E7EB-DA78-4840-9D00-4C8DD0DD4A27" );
            AddBlock( "B67E38CB-2EF1-43EA-863A-37DAA1C7340F", "1DE16F87-4A49-4A3C-A03E-B8488ECBEEBE", "Transaction Detail", "", "Content", 0, "F125E7EB-DA78-4840-9D00-4C8DD0DD4A27" );

            DeleteAttribute( "4635F59E-E41E-4695-8858-291B88B439CF" );

            AddBlockTypeAttribute( "E04320BC-67C3-452D-9EF6-D74D8C177154", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "", "", 0, "", "4635F59E-E41E-4695-8858-291B88B439CF" );
            AddBlockAttributeValue( "B447AB11-3A19-4527-921A-2266A6B4E181", "4635F59E-E41E-4695-8858-291B88B439CF", "CACC1871-C348-4753-8A43-456A70F325C7" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "4635F59E-E41E-4695-8858-291B88B439CF" ); // Detail Page Guid

            AddBlockTypeAttribute( "1DE16F87-4A49-4A3C-A03E-B8488ECBEEBE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "", "", 0, "", "4635F59E-E41E-4695-8858-291B88B439CF" );
            // Attrib Value for Transaction Detail:Detail Page Guid
            AddBlockAttributeValue( "F125E7EB-DA78-4840-9D00-4C8DD0DD4A27", "4635F59E-E41E-4695-8858-291B88B439CF", "606BDA31-A8FE-473A-B3F8-A00ECF7E06EC" );

            DeleteAttribute( "9F32738F-EA63-49F8-B2AB-1BB324C98057" ); // Detail Page Guid

            // Move Transaction Detail back under Financial Batch
            DeletePage( "B67E38CB-2EF1-43EA-863A-37DAA1C7340F" ); // Transaction Detail
            AddPage( "606BDA31-A8FE-473A-B3F8-A00ECF7E06EC", "Financial Transaction Detail ", "Display financial transactions detail", "B67E38CB-2EF1-43EA-863A-37DAA1C7340F" );

            // Rename Financial Transactions to generic "Transactions"
            DeleteBlock( "B447AB11-3A19-4527-921A-2266A6B4E181" );
            DeleteBlockType( "E04320BC-67C3-452D-9EF6-D74D8C177154" );
            AddBlockType( "Finance - Transactions", "View and search financial transactions", "~/Blocks/Finance/Transactions.ascx", "E04320BC-67C3-452D-9EF6-D74D8C177154" );
            AddBlock( "7CA317B5-5C47-465D-B407-7D614F2A568F", "E04320BC-67C3-452D-9EF6-D74D8C177154", "Financial Transaction", "", "Content", 0, "B447AB11-3A19-4527-921A-2266A6B4E181" );

            // Relink Transaction Detail
            DeleteBlock( "F125E7EB-DA78-4840-9D00-4C8DD0DD4A27" );
            AddBlock( "B67E38CB-2EF1-43EA-863A-37DAA1C7340F", "1DE16F87-4A49-4A3C-A03E-B8488ECBEEBE", "Transaction Detail", "", "Content", 0, "F125E7EB-DA78-4840-9D00-4C8DD0DD4A27" );

        }
    }
}
