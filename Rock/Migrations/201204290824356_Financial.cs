namespace Rock.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class Financial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "financialPledge",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonId = c.Int(),
                        FundId = c.Int(),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        FrequencyTypeId = c.Int(),
                        FrequencyAmount = c.Decimal(precision: 18, scale: 2),
                        ModifiedDateTime = c.DateTime(),
                        CreatedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("crmPerson", t => t.PersonId)
                .ForeignKey("financialFund", t => t.FundId)
                .ForeignKey("coreDefinedValue", t => t.FrequencyTypeId)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.PersonId)
                .Index(t => t.FundId)
                .Index(t => t.FrequencyTypeId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "financialFund",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                        PublicName = c.String(maxLength: 50),
                        Description = c.String(maxLength: 250),
                        ParentFundId = c.Int(),
                        TaxDeductible = c.Boolean(nullable: false),
                        Order = c.Int(nullable: false),
                        Active = c.Boolean(nullable: false),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        Pledgable = c.Boolean(nullable: false),
                        GlCode = c.String(maxLength: 50),
                        FundTypeId = c.Int(),
                        Entity = c.String(maxLength: 50),
                        EntityId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("financialFund", t => t.ParentFundId)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.ParentFundId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "financialTransactionFund",
                c => new
                    {
                        TransactionId = c.Int(nullable: false),
                        FundId = c.Int(nullable: false),
                        Amount = c.Decimal(precision: 18, scale: 2),
                    })
                .PrimaryKey(t => new { t.TransactionId, t.FundId })
                .ForeignKey("financialTransaction", t => t.TransactionId, cascadeDelete: true)
                .ForeignKey("financialFund", t => t.FundId, cascadeDelete: true)
                .Index(t => t.TransactionId)
                .Index(t => t.FundId);
            
            CreateTable(
                "financialTransaction",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(maxLength: 250),
                        TransactionDate = c.DateTime(),
                        Entity = c.String(maxLength: 50),
                        EntityId = c.Int(),
                        BatchId = c.Int(),
                        CurrencyTypeId = c.Int(),
                        CreditCardTypeId = c.Int(),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RefundTransactionId = c.Int(),
                        TransactionImageId = c.Int(),
                        TransactionCode = c.String(maxLength: 50),
                        GatewayId = c.Int(),
                        SourceTypeId = c.Int(),
                        Summary = c.String(maxLength: 500),
                        ModifiedDateTime = c.DateTime(),
                        CreatedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("financialBatch", t => t.BatchId)
                .ForeignKey("coreDefinedValue", t => t.CurrencyTypeId)
                .ForeignKey("coreDefinedValue", t => t.CreditCardTypeId)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.BatchId)
                .Index(t => t.CurrencyTypeId)
                .Index(t => t.CreditCardTypeId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "financialBatch",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                        BatchDate = c.DateTime(),
                        Closed = c.Boolean(nullable: false),
                        CampusId = c.Int(),
                        Entity = c.String(maxLength: 50),
                        EntityId = c.Int(),
                        ForeignReference = c.String(maxLength: 50),
                        ModifiedDateTime = c.DateTime(),
                        CreatedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "financialTransactionDetail",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TransactionId = c.Int(),
                        Entity = c.String(maxLength: 50),
                        EntityId = c.String(),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Summary = c.String(maxLength: 500),
                        ModifiedDateTime = c.DateTime(),
                        CreatedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("financialTransaction", t => t.TransactionId)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.TransactionId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "fiancialPersonAccountLookup",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonId = c.Int(),
                        Account = c.String(maxLength: 50),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("crmPerson", t => t.PersonId)
                .Index(t => t.PersonId);
            
        }
        
        public override void Down()
        {
            DropIndex("fiancialPersonAccountLookup", new[] { "PersonId" });
            DropIndex("financialTransactionDetail", new[] { "ModifiedByPersonId" });
            DropIndex("financialTransactionDetail", new[] { "CreatedByPersonId" });
            DropIndex("financialTransactionDetail", new[] { "TransactionId" });
            DropIndex("financialBatch", new[] { "ModifiedByPersonId" });
            DropIndex("financialBatch", new[] { "CreatedByPersonId" });
            DropIndex("financialTransaction", new[] { "ModifiedByPersonId" });
            DropIndex("financialTransaction", new[] { "CreatedByPersonId" });
            DropIndex("financialTransaction", new[] { "CreditCardTypeId" });
            DropIndex("financialTransaction", new[] { "CurrencyTypeId" });
            DropIndex("financialTransaction", new[] { "BatchId" });
            DropIndex("financialTransactionFund", new[] { "FundId" });
            DropIndex("financialTransactionFund", new[] { "TransactionId" });
            DropIndex("financialFund", new[] { "ModifiedByPersonId" });
            DropIndex("financialFund", new[] { "CreatedByPersonId" });
            DropIndex("financialFund", new[] { "ParentFundId" });
            DropIndex("financialPledge", new[] { "ModifiedByPersonId" });
            DropIndex("financialPledge", new[] { "CreatedByPersonId" });
            DropIndex("financialPledge", new[] { "FrequencyTypeId" });
            DropIndex("financialPledge", new[] { "FundId" });
            DropIndex("financialPledge", new[] { "PersonId" });
            DropForeignKey("fiancialPersonAccountLookup", "PersonId", "crmPerson");
            DropForeignKey("financialTransactionDetail", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("financialTransactionDetail", "CreatedByPersonId", "crmPerson");
            DropForeignKey("financialTransactionDetail", "TransactionId", "financialTransaction");
            DropForeignKey("financialBatch", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("financialBatch", "CreatedByPersonId", "crmPerson");
            DropForeignKey("financialTransaction", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("financialTransaction", "CreatedByPersonId", "crmPerson");
            DropForeignKey("financialTransaction", "CreditCardTypeId", "coreDefinedValue");
            DropForeignKey("financialTransaction", "CurrencyTypeId", "coreDefinedValue");
            DropForeignKey("financialTransaction", "BatchId", "financialBatch");
            DropForeignKey("financialTransactionFund", "FundId", "financialFund");
            DropForeignKey("financialTransactionFund", "TransactionId", "financialTransaction");
            DropForeignKey("financialFund", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("financialFund", "CreatedByPersonId", "crmPerson");
            DropForeignKey("financialFund", "ParentFundId", "financialFund");
            DropForeignKey("financialPledge", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("financialPledge", "CreatedByPersonId", "crmPerson");
            DropForeignKey("financialPledge", "FrequencyTypeId", "coreDefinedValue");
            DropForeignKey("financialPledge", "FundId", "financialFund");
            DropForeignKey("financialPledge", "PersonId", "crmPerson");
            DropTable("fiancialPersonAccountLookup");
            DropTable("financialTransactionDetail");
            DropTable("financialBatch");
            DropTable("financialTransaction");
            DropTable("financialTransactionFund");
            DropTable("financialFund");
            DropTable("financialPledge");
        }
    }
}
