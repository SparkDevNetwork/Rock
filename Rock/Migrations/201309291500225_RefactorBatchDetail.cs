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
    public partial class RefactorBatchDetail : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Block: Transaction List to Page: Financial Batch Detail
            AddBlock( "606BDA31-A8FE-473A-B3F8-A00ECF7E06EC", "E04320BC-67C3-452D-9EF6-D74D8C177154", "Transaction List", "", "Content", 1, "1133795B-3325-4D81-B603-F442F0AC892B" );

            // Attrib for BlockType: Finance - Transaction List:Context Entity Type
            AddBlockTypeAttribute( "E04320BC-67C3-452D-9EF6-D74D8C177154", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Entity Type", "ContextEntityType", "", "Context Entity Type", 0, "NULL", "29A6C37A-EFB3-41CC-A522-9CEFAAEEA910" );

            // Attrib Value for Page/BlockFinancial Batch Detail/Transaction List:Context Entity Type (FieldType: Text)
            AddBlockAttributeValue("1133795B-3325-4D81-B603-F442F0AC892B","29A6C37A-EFB3-41CC-A522-9CEFAAEEA910","Rock.Model.FinancialBatch");
                        
            // Attrib Value for Page/BlockFinancial Batch Detail/Transaction List:Detail Page (FieldType: Page Reference)
            AddBlockAttributeValue( "1133795B-3325-4D81-B603-F442F0AC892B", "C6D07A89-84C9-412A-A584-E37E59506566", "97716641-D003-4663-9EA2-D9BB94E7955B" );

            // Page Context for Financial Batch Detail Page: Transaction List Block
            AddPageContext( "606BDA31-A8FE-473A-B3F8-A00ECF7E06EC", "Rock.Model.FinancialBatch", "financialBatchId" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Page Context for Financial Batch Detail Page: Transaction List Block
            Sql( @"
                DELETE [PageContext]
                WHERE [Entity] = 'Rock.Model.FinancialBatch'
                AND [IdParameter] = 'financialBatchId'
            " );
            
            // Remove Attrib Value for Block: Transaction List, Attribute: Detail Page, Page: Financial Batch Detail
            DeleteBlockAttributeValue( "1133795B-3325-4D81-B603-F442F0AC892B", "C6D07A89-84C9-412A-A584-E37E59506566" );

            // Attrib Value for Page/BlockFinancial Batch Detail/Transaction List:Context Entity Type (FieldType: Text)
            DeleteBlockAttributeValue( "1133795B-3325-4D81-B603-F442F0AC892B", "29A6C37A-EFB3-41CC-A522-9CEFAAEEA910" );

            // Remove Block Type Attribute Transaction List
            DeleteBlockAttribute( "29A6C37A-EFB3-41CC-A522-9CEFAAEEA910" );

            // Remove Block: Transaction List, from Page: Batch Detail
            DeleteBlock( "1133795B-3325-4D81-B603-F442F0AC892B" );

        }
    }
}
