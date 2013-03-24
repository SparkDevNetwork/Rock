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
            AddColumn("dbo.FinancialBatch", "BatchType_Id", c => c.Int());
            AddForeignKey("dbo.FinancialBatch", "CampusId", "dbo.Campus", "Id");
            AddForeignKey("dbo.FinancialBatch", "BatchType_Id", "dbo.DefinedType", "Id");
            CreateIndex("dbo.FinancialBatch", "CampusId");
            CreateIndex("dbo.FinancialBatch", "BatchType_Id");

            
            Sql( @"
                SET IDENTITY_INSERT [DefinedType] ON
                INSERT INTO [DefinedType] ([Id], [IsSystem], [FieldTypeId], [Order], [Category], [Name], [Description], [Guid]) VALUES (18, 1, 1, 0, N'FinancialBatch',N'FinancialBatch',N'Type of Batch','9E358FBE-2321-4C54-895F-C888E29298AE')
                SET IDENTITY_INSERT [DefinedType] OFF




                SET IDENTITY_INSERT [DefinedValue] ON
                INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (31,1,18,0,'ACH','ACH','E6F877F3-D2CC-443E-976A-4402502F544F')
                INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (32,1,18,1,'Visa','Visa','24CC2E82-B2B6-4037-87AE-39EEAFE06712')
                INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (33,1,18,2,'MasterCard','MasterCard','50F625F8-F1BE-4FA0-B99F-3FA852D87DD1')
                INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (34,1,18,3,'Discover','Discover','18DF8254-0C68-4FE0-973E-C0B1767EFD3F')
                INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (35,1,18,4,'Amex','Amex','378D8EAD-7FA6-4D0D-862D-ED6E04B17770')
                INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (36,1,18,5,'PayPal','PayPal','4832DA18-DD18-477F-BFDB-ABFC28FE5743')
                SET IDENTITY_INSERT [DefinedValue] OFF" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.FinancialBatch", new[] { "BatchType_Id" });
            DropIndex("dbo.FinancialBatch", new[] { "CampusId" });
            DropForeignKey("dbo.FinancialBatch", "BatchType_Id", "dbo.DefinedType");
            DropForeignKey("dbo.FinancialBatch", "CampusId", "dbo.Campus");
            DropColumn("dbo.FinancialBatch", "BatchType_Id");
        }
    }
}
