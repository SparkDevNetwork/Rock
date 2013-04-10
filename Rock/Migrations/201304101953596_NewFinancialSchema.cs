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
    public partial class NewFinancialSchema : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.Pledge", "PersonId", "dbo.Person");
            DropForeignKey("dbo.Pledge", "FundId", "dbo.Fund");
            DropForeignKey("dbo.Pledge", "FrequencyTypeValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.Fund", "ParentFundId", "dbo.Fund");
            DropForeignKey("dbo.Fund", "FundTypeValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.FinancialTransactionFund", "TransactionId", "dbo.FinancialTransaction");
            DropForeignKey("dbo.FinancialTransactionFund", "FundId", "dbo.Fund");
            DropForeignKey("dbo.FinancialTransaction", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.FinancialTransaction", "PaymentGatewayId", "dbo.PaymentGateway");
            DropForeignKey("dbo.FinancialTransactionDetail", "TransactionId", "dbo.FinancialTransaction");
            DropIndex("dbo.Pledge", new[] { "PersonId" });
            DropIndex("dbo.Pledge", new[] { "FundId" });
            DropIndex("dbo.Pledge", new[] { "FrequencyTypeValueId" });
            DropIndex("dbo.Fund", new[] { "ParentFundId" });
            DropIndex("dbo.Fund", new[] { "FundTypeValueId" });
            DropIndex("dbo.FinancialTransactionFund", new[] { "TransactionId" });
            DropIndex("dbo.FinancialTransactionFund", new[] { "FundId" });
            DropIndex("dbo.FinancialTransaction", new[] { "EntityTypeId" });
            DropIndex("dbo.FinancialTransaction", new[] { "PaymentGatewayId" });
            DropIndex("dbo.FinancialTransactionDetail", new[] { "TransactionId" });
            CreateTable(
                "dbo.FinancialAccount",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ParentAccountId = c.Int(),
                        CampusId = c.Int(),
                        Name = c.String(maxLength: 50),
                        PublicName = c.String(maxLength: 50),
                        Description = c.String(),
                        IsTaxDeductible = c.Boolean(nullable: false),
                        GlCode = c.String(maxLength: 50),
                        Order = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        AccountTypeValueId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FinancialAccount", t => t.ParentAccountId)
                .ForeignKey("dbo.Campus", t => t.CampusId)
                .ForeignKey("dbo.DefinedValue", t => t.AccountTypeValueId)
                .Index(t => t.ParentAccountId)
                .Index(t => t.CampusId)
                .Index(t => t.AccountTypeValueId);
            
            CreateIndex( "dbo.FinancialAccount", "Guid", true );
            CreateTable(
                "dbo.FinancialGateway",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                        Description = c.String(),
                        ApiUrl = c.String(maxLength: 100),
                        ApiKey = c.String(maxLength: 100),
                        ApiSecret = c.String(maxLength: 100),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex( "dbo.FinancialGateway", "Guid", true );
            CreateTable(
                "dbo.FinancialTransactionImage",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TransactionId = c.Int(nullable: false),
                        BinaryFileId = c.Int(nullable: false),
                        TransactionImageTypeValueId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FinancialTransaction", t => t.TransactionId)
                .ForeignKey("dbo.BinaryFile", t => t.BinaryFileId)
                .ForeignKey("dbo.DefinedValue", t => t.TransactionImageTypeValueId)
                .Index(t => t.TransactionId)
                .Index(t => t.BinaryFileId)
                .Index(t => t.TransactionImageTypeValueId);
            
            CreateIndex( "dbo.FinancialTransactionImage", "Guid", true );
            CreateTable(
                "dbo.FinancialPledge",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonId = c.Int(),
                        AccountId = c.Int(),
                        TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PledgeFrequencyValueId = c.Int(),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.PersonId)
                .ForeignKey("dbo.FinancialAccount", t => t.AccountId)
                .ForeignKey("dbo.DefinedValue", t => t.PledgeFrequencyValueId)
                .Index(t => t.PersonId)
                .Index(t => t.AccountId)
                .Index(t => t.PledgeFrequencyValueId);
            
            CreateIndex( "dbo.FinancialPledge", "Guid", true );
            AddColumn("dbo.FinancialTransaction", "AuthorizedPersonId", c => c.Int(nullable: false));
            AddColumn("dbo.FinancialTransaction", "GatewayId", c => c.Int());
            AddColumn("dbo.FinancialTransaction", "TransactionTypeValueId", c => c.Int(nullable: false));
            AddColumn("dbo.FinancialBatch", "Status", c => c.Int(nullable: false));
            AddColumn("dbo.FinancialBatch", "AccountingSystemCode", c => c.String(maxLength: 100));
            AddColumn("dbo.FinancialBatch", "ControlAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.FinancialTransactionDetail", "AccountId", c => c.Int(nullable: false));
            AddColumn("dbo.FinancialTransactionDetail", "EntityTypeId", c => c.Int());
            AlterColumn("dbo.FinancialTransaction", "Summary", c => c.String());
            AlterColumn("dbo.FinancialTransactionDetail", "TransactionId", c => c.Int(nullable: false));
            AlterColumn("dbo.FinancialTransactionDetail", "EntityId", c => c.Int());
            AddForeignKey("dbo.FinancialBatch", "CampusId", "dbo.Campus", "Id");
            AddForeignKey("dbo.FinancialTransaction", "AuthorizedPersonId", "dbo.Person", "Id");
            AddForeignKey("dbo.FinancialTransaction", "GatewayId", "dbo.FinancialGateway", "Id");
            AddForeignKey("dbo.FinancialTransaction", "TransactionTypeValueId", "dbo.DefinedValue", "Id");
            AddForeignKey("dbo.FinancialTransactionDetail", "TransactionId", "dbo.FinancialTransaction", "Id", cascadeDelete: true);
            AddForeignKey("dbo.FinancialTransactionDetail", "AccountId", "dbo.FinancialAccount", "Id");
            AddForeignKey("dbo.FinancialTransactionDetail", "EntityTypeId", "dbo.EntityType", "Id");
            CreateIndex("dbo.FinancialBatch", "CampusId");
            CreateIndex("dbo.FinancialTransaction", "AuthorizedPersonId");
            CreateIndex("dbo.FinancialTransaction", "GatewayId");
            CreateIndex("dbo.FinancialTransaction", "TransactionTypeValueId");
            CreateIndex("dbo.FinancialTransactionDetail", "TransactionId");
            CreateIndex("dbo.FinancialTransactionDetail", "AccountId");
            CreateIndex("dbo.FinancialTransactionDetail", "EntityTypeId");
            DropColumn("dbo.FinancialTransaction", "Description");
            DropColumn("dbo.FinancialTransaction", "EntityTypeId");
            DropColumn("dbo.FinancialTransaction", "EntityId");
            DropColumn("dbo.FinancialTransaction", "RefundTransactionId");
            DropColumn("dbo.FinancialTransaction", "TransactionImageId");
            DropColumn("dbo.FinancialTransaction", "PaymentGatewayId");
            DropColumn("dbo.FinancialBatch", "IsClosed");
            DropColumn("dbo.FinancialBatch", "Entity");
            DropColumn("dbo.FinancialBatch", "EntityId");
            DropColumn("dbo.FinancialBatch", "ForeignReference");
            DropColumn("dbo.FinancialTransactionDetail", "Entity");
            DropTable("dbo.Pledge");
            DropTable("dbo.Fund");
            DropTable("dbo.FinancialTransactionFund");
            DropTable("dbo.PaymentGateway");

            AddDefinedType( "Financial", "Account Type", "Types of Accounts", "752DA126-471F-4221-8503-5297593C99FF" );
            AddDefinedType( "Financial", "Transaction Type", "The type of financial transaction (i.e. Contribution, Event Registration, etc.)", "FFF62A4B-5D88-4DEB-AF8F-8E6178E41FE5");
            AddDefinedValue( "FFF62A4B-5D88-4DEB-AF8F-8E6178E41FE5", "Contribution", "A Contribution Transaction", "2D607262-52D6-4724-910D-5C6E8FB89ACC" );
            AddDefinedValue( "FFF62A4B-5D88-4DEB-AF8F-8E6178E41FE5", "Event Registration", "An Event Registration Transaction", "33CB96DD-8752-4BEE-A142-88DB7DE538F0" );
            AddDefinedType( "Financial", "Transaction Image Type", "The type of image associated with a transaction", "0745D5DE-2D09-44B3-9017-40C1DA83CB39" );
            AddDefinedValue( "0745D5DE-2D09-44B3-9017-40C1DA83CB39", "Front of Check", "Front of check image", "A52EDD34-D3A2-420F-AF45-21B323FB21D6" );
            AddDefinedValue( "0745D5DE-2D09-44B3-9017-40C1DA83CB39", "Back of Check", "Back of check image", "87D9347D-64E6-4DD4-8F05-2AA17419B5E8" );
            AddDefinedValue( "0745D5DE-2D09-44B3-9017-40C1DA83CB39", "Front of Envelope", "Front of envelope", "654ABEC4-7414-402F-BEA4-0AA833683AD6" );
            AddDefinedValue( "0745D5DE-2D09-44B3-9017-40C1DA83CB39", "Back of Envelope", "Back of envelope", "746FBD46-AA4C-4A84-A7DA-080763CED187" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteDefinedType( "752DA126-471F-4221-8503-5297593C99FF" );
            DeleteDefinedType( "FFF62A4B-5D88-4DEB-AF8F-8E6178E41FE5" );
            DeleteDefinedType( "0745D5DE-2D09-44B3-9017-40C1DA83CB39" );

            CreateTable(
                "dbo.PaymentGateway",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                        Description = c.String(maxLength: 500),
                        ApiUrl = c.String(maxLength: 100),
                        ApiKey = c.String(maxLength: 100),
                        ApiSecret = c.String(maxLength: 100),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            // TableName: PaymentGateway
            // The given key was not present in the dictionary.
            // TableName: PaymentGateway
            // The given key was not present in the dictionary.
            // TableName: PaymentGateway
            // The given key was not present in the dictionary.
            // TableName: PaymentGateway
            // The given key was not present in the dictionary.
            // TableName: PaymentGateway
            // The given key was not present in the dictionary.
            // TableName: PaymentGateway
            // The given key was not present in the dictionary.
            // TableName: PaymentGateway
            // The given key was not present in the dictionary.
            CreateTable(
                "dbo.FinancialTransactionFund",
                c => new
                    {
                        TransactionId = c.Int(nullable: false),
                        FundId = c.Int(nullable: false),
                        Amount = c.Decimal(precision: 18, scale: 2),
                    })
                .PrimaryKey(t => new { t.TransactionId, t.FundId });
            
            // TableName: FinancialTransactionFund
            // The given key was not present in the dictionary.
            // TableName: FinancialTransactionFund
            // The given key was not present in the dictionary.
            // TableName: FinancialTransactionFund
            // The given key was not present in the dictionary.
            CreateTable(
                "dbo.Fund",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                        PublicName = c.String(maxLength: 50),
                        Description = c.String(maxLength: 250),
                        ParentFundId = c.Int(),
                        IsTaxDeductible = c.Boolean(nullable: false),
                        Order = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        IsPledgable = c.Boolean(nullable: false),
                        GlCode = c.String(maxLength: 50),
                        FundTypeValueId = c.Int(),
                        Entity = c.String(maxLength: 50),
                        EntityId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            // TableName: Fund
            // The given key was not present in the dictionary.
            // TableName: Fund
            // The given key was not present in the dictionary.
            // TableName: Fund
            // The given key was not present in the dictionary.
            // TableName: Fund
            // The given key was not present in the dictionary.
            // TableName: Fund
            // The given key was not present in the dictionary.
            // TableName: Fund
            // The given key was not present in the dictionary.
            // TableName: Fund
            // The given key was not present in the dictionary.
            // TableName: Fund
            // The given key was not present in the dictionary.
            // TableName: Fund
            // The given key was not present in the dictionary.
            // TableName: Fund
            // The given key was not present in the dictionary.
            // TableName: Fund
            // The given key was not present in the dictionary.
            // TableName: Fund
            // The given key was not present in the dictionary.
            // TableName: Fund
            // The given key was not present in the dictionary.
            // TableName: Fund
            // The given key was not present in the dictionary.
            // TableName: Fund
            // The given key was not present in the dictionary.
            // TableName: Fund
            // The given key was not present in the dictionary.
            CreateTable(
                "dbo.Pledge",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonId = c.Int(),
                        FundId = c.Int(),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        FrequencyTypeValueId = c.Int(),
                        FrequencyAmount = c.Decimal(precision: 18, scale: 2),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            // TableName: Pledge
            // The given key was not present in the dictionary.
            // TableName: Pledge
            // The given key was not present in the dictionary.
            // TableName: Pledge
            // The given key was not present in the dictionary.
            // TableName: Pledge
            // The given key was not present in the dictionary.
            // TableName: Pledge
            // The given key was not present in the dictionary.
            // TableName: Pledge
            // The given key was not present in the dictionary.
            // TableName: Pledge
            // The given key was not present in the dictionary.
            // TableName: Pledge
            // The given key was not present in the dictionary.
            // TableName: Pledge
            // The given key was not present in the dictionary.
            AddColumn("dbo.FinancialTransactionDetail", "Entity", c => c.String(maxLength: 50));
            AddColumn("dbo.FinancialBatch", "ForeignReference", c => c.String(maxLength: 50));
            AddColumn("dbo.FinancialBatch", "EntityId", c => c.Int());
            AddColumn("dbo.FinancialBatch", "Entity", c => c.String(maxLength: 50));
            AddColumn("dbo.FinancialBatch", "IsClosed", c => c.Boolean(nullable: false));
            AddColumn("dbo.FinancialTransaction", "PaymentGatewayId", c => c.Int());
            AddColumn("dbo.FinancialTransaction", "TransactionImageId", c => c.Int());
            AddColumn("dbo.FinancialTransaction", "RefundTransactionId", c => c.Int());
            AddColumn("dbo.FinancialTransaction", "EntityId", c => c.Int(nullable: false));
            AddColumn("dbo.FinancialTransaction", "EntityTypeId", c => c.Int(nullable: false));
            AddColumn("dbo.FinancialTransaction", "Description", c => c.String(maxLength: 250));
            DropIndex("dbo.FinancialPledge", new[] { "PledgeFrequencyValueId" });
            DropIndex("dbo.FinancialPledge", new[] { "AccountId" });
            DropIndex("dbo.FinancialPledge", new[] { "PersonId" });
            DropIndex("dbo.FinancialTransactionImage", new[] { "TransactionImageTypeValueId" });
            DropIndex("dbo.FinancialTransactionImage", new[] { "BinaryFileId" });
            DropIndex("dbo.FinancialTransactionImage", new[] { "TransactionId" });
            DropIndex("dbo.FinancialTransactionDetail", new[] { "EntityTypeId" });
            DropIndex("dbo.FinancialTransactionDetail", new[] { "AccountId" });
            DropIndex("dbo.FinancialTransactionDetail", new[] { "TransactionId" });
            DropIndex("dbo.FinancialTransaction", new[] { "TransactionTypeValueId" });
            DropIndex("dbo.FinancialTransaction", new[] { "GatewayId" });
            DropIndex("dbo.FinancialTransaction", new[] { "AuthorizedPersonId" });
            DropIndex("dbo.FinancialBatch", new[] { "CampusId" });
            DropIndex("dbo.FinancialAccount", new[] { "AccountTypeValueId" });
            DropIndex("dbo.FinancialAccount", new[] { "CampusId" });
            DropIndex("dbo.FinancialAccount", new[] { "ParentAccountId" });
            DropForeignKey("dbo.FinancialPledge", "PledgeFrequencyValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.FinancialPledge", "AccountId", "dbo.FinancialAccount");
            DropForeignKey("dbo.FinancialPledge", "PersonId", "dbo.Person");
            DropForeignKey("dbo.FinancialTransactionImage", "TransactionImageTypeValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.FinancialTransactionImage", "BinaryFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.FinancialTransactionImage", "TransactionId", "dbo.FinancialTransaction");
            DropForeignKey("dbo.FinancialTransactionDetail", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.FinancialTransactionDetail", "AccountId", "dbo.FinancialAccount");
            DropForeignKey("dbo.FinancialTransactionDetail", "TransactionId", "dbo.FinancialTransaction");
            DropForeignKey("dbo.FinancialTransaction", "TransactionTypeValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.FinancialTransaction", "GatewayId", "dbo.FinancialGateway");
            DropForeignKey("dbo.FinancialTransaction", "AuthorizedPersonId", "dbo.Person");
            DropForeignKey("dbo.FinancialBatch", "CampusId", "dbo.Campus");
            DropForeignKey("dbo.FinancialAccount", "AccountTypeValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.FinancialAccount", "CampusId", "dbo.Campus");
            DropForeignKey("dbo.FinancialAccount", "ParentAccountId", "dbo.FinancialAccount");
            AlterColumn("dbo.FinancialTransactionDetail", "EntityId", c => c.String());
            AlterColumn("dbo.FinancialTransactionDetail", "TransactionId", c => c.Int());
            AlterColumn("dbo.FinancialTransaction", "Summary", c => c.String(maxLength: 500));
            DropColumn("dbo.FinancialTransactionDetail", "EntityTypeId");
            DropColumn("dbo.FinancialTransactionDetail", "AccountId");
            DropColumn("dbo.FinancialBatch", "ControlAmount");
            DropColumn("dbo.FinancialBatch", "AccountingSystemCode");
            DropColumn("dbo.FinancialBatch", "Status");
            DropColumn("dbo.FinancialTransaction", "TransactionTypeValueId");
            DropColumn("dbo.FinancialTransaction", "GatewayId");
            DropColumn("dbo.FinancialTransaction", "AuthorizedPersonId");
            DropTable("dbo.FinancialPledge");
            DropTable("dbo.FinancialTransactionImage");
            DropTable("dbo.FinancialGateway");
            DropTable("dbo.FinancialAccount");
            CreateIndex("dbo.FinancialTransactionDetail", "TransactionId");
            CreateIndex("dbo.FinancialTransaction", "PaymentGatewayId");
            CreateIndex("dbo.FinancialTransaction", "EntityTypeId");
            CreateIndex("dbo.FinancialTransactionFund", "FundId");
            CreateIndex("dbo.FinancialTransactionFund", "TransactionId");
            CreateIndex("dbo.Fund", "FundTypeValueId");
            CreateIndex("dbo.Fund", "ParentFundId");
            CreateIndex("dbo.Pledge", "FrequencyTypeValueId");
            CreateIndex("dbo.Pledge", "FundId");
            CreateIndex("dbo.Pledge", "PersonId");
            AddForeignKey("dbo.FinancialTransactionDetail", "TransactionId", "dbo.FinancialTransaction", "Id");
            AddForeignKey("dbo.FinancialTransaction", "PaymentGatewayId", "dbo.PaymentGateway", "Id");
            AddForeignKey("dbo.FinancialTransaction", "EntityTypeId", "dbo.EntityType", "Id");
            AddForeignKey("dbo.FinancialTransactionFund", "FundId", "dbo.Fund", "Id", cascadeDelete: true);
            AddForeignKey("dbo.FinancialTransactionFund", "TransactionId", "dbo.FinancialTransaction", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Fund", "FundTypeValueId", "dbo.DefinedValue", "Id");
            AddForeignKey("dbo.Fund", "ParentFundId", "dbo.Fund", "Id");
            AddForeignKey("dbo.Pledge", "FrequencyTypeValueId", "dbo.DefinedValue", "Id");
            AddForeignKey("dbo.Pledge", "FundId", "dbo.Fund", "Id");
            AddForeignKey("dbo.Pledge", "PersonId", "dbo.Person", "Id");
        }
    }
}
