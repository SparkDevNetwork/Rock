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
    public partial class FinancialBatchNewFields : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.FinancialBatch", "BatchDateStart", c => c.DateTime());
            AddColumn("dbo.FinancialBatch", "BatchDateEnd", c => c.DateTime());
            AddColumn("dbo.FinancialBatch", "BatchTypeValueId", c => c.Int(nullable: false));
            AddColumn("dbo.FinancialBatch", "ControlAmount", c => c.Single(nullable: false));
            AddForeignKey("dbo.FinancialBatch", "CampusId", "dbo.Campus", "Id");
            AddForeignKey("dbo.FinancialBatch", "BatchTypeValueId", "dbo.DefinedType", "Id", cascadeDelete: true);
            CreateIndex("dbo.FinancialBatch", "CampusId");
            CreateIndex("dbo.FinancialBatch", "BatchTypeValueId");
            DropColumn("dbo.FinancialBatch", "BatchDate");

            AddDefinedType( "Financial", "Batch Type", "Batch Types", "9e358fbe-2321-4c54-895f-c888e29298ae" );
            AddDefinedValue( "9e358fbe-2321-4c54-895f-c888e29298ae", "ACH", "ACH", "E6F877F3-D2CC-443E-976A-4402502F544F" );
            AddDefinedValue( "9e358fbe-2321-4c54-895f-c888e29298ae", "Visa", "Visa", "24CC2E82-B2B6-4037-87AE-39EEAFE06712" );
            AddDefinedValue( "9e358fbe-2321-4c54-895f-c888e29298ae", "MasterCard", "MasterCard", "50F625F8-F1BE-4FA0-B99F-3FA852D87DD1" );
            AddDefinedValue( "9e358fbe-2321-4c54-895f-c888e29298ae", "Discover", "Discover", "18DF8254-0C68-4FE0-973E-C0B1767EFD3F" );
            AddDefinedValue( "9e358fbe-2321-4c54-895f-c888e29298ae", "Amex", "Amex", "378D8EAD-7FA6-4D0D-862D-ED6E04B17770" );
            AddDefinedValue( "9e358fbe-2321-4c54-895f-c888e29298ae", "PayPal", "PayPal", "4832DA18-DD18-477F-BFDB-ABFC28FE5743" );

            //add the pages 
            AddPage( "18C9E5C3-3E28-4AA3-84F6-78CD4EA2DD3C", "Financial Batch", "Manage Financial Batches", "ef65eff2-99ac-4081-8e09-32a04518683a" );
            AddPage( "ef65eff2-99ac-4081-8e09-32a04518683a", "Financial Batch Detail", "", "606bda31-a8fe-473a-b3f8-a00ecf7e06ec" );
            AddPage( "606bda31-a8fe-473a-b3f8-a00ecf7e06ec", "Transaction Detail", "", "b67e38cb-2ef1-43ea-863a-37daa1c7340f" );

            // add the block types
            AddBlockType( "Finance - Financial Batch", "", "~/Blocks/Finance/FinancialBatch.ascx", "AB345CE7-5DC6-41AF-BBDC-8D23D52AFE25" );
            AddBlockType( "Finance - Batch Details", "", "~/Blocks/Finance/FinancialBatchDetail.ascx", "ce34ce43-2ccf-4568-9aeb-3be203db3470" );
            AddBlockType( "Finance - Transaction Details", "", "~/Blocks/Finance/TransactionBlock.ascx", "1DE16F87-4A49-4A3C-A03E-B8488ECBEEBE" );

            //add block
            //AddBlock( "ef65eff2-99ac-4081-8e09-32a04518683a", "AB345CE7-5DC6-41AF-BBDC-8D23D52AFE25", "Financial Batches", "default", "Content", 0, "B4B7A962-E162-47ED-8499-B7C7A7F41498" );
            //AddBlock( "606bda31-a8fe-473a-b3f8-a00ecf7e06ec", "ce34ce43-2ccf-4568-9aeb-3be203db3470", "Financial Batch Details", "default", "Content", 0, "e7c8c398-0e1d-4bce-bc54-a02957228514" );
            //AddBlock( "b67e38cb-2ef1-43ea-863a-37daa1c7340f", "1DE16F87-4A49-4A3C-A03E-B8488ECBEEBE", "Financial Transaction Details", "default", "Content", 0, "f125e7eb-da78-4840-9d00-4c8dd0dd4a27" );

            //( string blockTypeGuid, string fieldTypeGuid, string name, string key, string category, string description, int order, string defaultValue, string guid )
            AddBlockTypeAttribute( "AB345CE7-5DC6-41AF-BBDC-8D23D52AFE25", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "Advanced", "", 0, "", "2290806c-9e87-4960-9019-d4d7327591bb" );
            AddBlockTypeAttribute( "ce34ce43-2ccf-4568-9aeb-3be203db3470", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "Advanced", "", 0, "", "f70b3c73-3582-4fa4-b988-92880268310c" );

            // Attrib Value for FinancialBatch:Detail Page Guid
            //string blockGuid, string attributeGuid, string value
            AddBlockAttributeValue( "B4B7A962-E162-47ED-8499-B7C7A7F41498", "2290806c-9e87-4960-9019-d4d7327591bb", "606bda31-a8fe-473a-b3f8-a00ecf7e06ec" );
            AddBlockAttributeValue( "e7c8c398-0e1d-4bce-bc54-a02957228514", "f70b3c73-3582-4fa4-b988-92880268310c", "b67e38cb-2ef1-43ea-863a-37daa1c7340f" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.FinancialBatch", "BatchDate", c => c.DateTime());
            DropIndex("dbo.FinancialBatch", new[] { "BatchTypeValueId" });
            DropIndex("dbo.FinancialBatch", new[] { "CampusId" });
            DropForeignKey("dbo.FinancialBatch", "BatchTypeValueId", "dbo.DefinedType");
            DropForeignKey("dbo.FinancialBatch", "CampusId", "dbo.Campus");
            DropColumn("dbo.FinancialBatch", "ControlAmount");
            DropColumn("dbo.FinancialBatch", "BatchTypeValueId");
            DropColumn("dbo.FinancialBatch", "BatchDateEnd");
            DropColumn("dbo.FinancialBatch", "BatchDateStart");

            DeleteDefinedType( "9e358fbe-2321-4c54-895f-c888e29298ae" ); // Financial Batch Types
           
            DeleteAttribute( "2290806c-9e87-4960-9019-d4d7327591bb" );
            DeleteAttribute( "f70b3c73-3582-4fa4-b988-92880268310c" );

            DeleteBlock( "B4B7A962-E162-47ED-8499-B7C7A7F41498" );
            DeleteBlock( "e7c8c398-0e1d-4bce-bc54-a02957228514" );
            DeleteBlock( "f125e7eb-da78-4840-9d00-4c8dd0dd4a27" );

            DeleteBlockType( "AB345CE7-5DC6-41AF-BBDC-8D23D52AFE25" );
            DeleteBlockType( "ce34ce43-2ccf-4568-9aeb-3be203db3470" );
            DeleteBlockType( "1DE16F87-4A49-4A3C-A03E-B8488ECBEEBE" );

            DeletePage( "b67e38cb-2ef1-43ea-863a-37daa1c7340f" );
            DeletePage( "606bda31-a8fe-473a-b3f8-a00ecf7e06ec" );
            DeletePage( "ef65eff2-99ac-4081-8e09-32a04518683a" );
        }
    }
}
