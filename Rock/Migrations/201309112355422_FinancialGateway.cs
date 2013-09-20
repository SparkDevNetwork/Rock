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
    public partial class FinancialGateway : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.FinancialGateway", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.FinancialTransaction", "GatewayId", "dbo.FinancialGateway");
            DropForeignKey("dbo.FinancialPersonSavedAccount", "GatewayId", "dbo.FinancialGateway");
            DropForeignKey("dbo.FinancialScheduledTransaction", "GatewayId", "dbo.FinancialGateway");
            DropIndex("dbo.FinancialGateway", new[] { "EntityTypeId" });
            DropIndex("dbo.FinancialTransaction", new[] { "GatewayId" });
            DropIndex("dbo.FinancialPersonSavedAccount", new[] { "GatewayId" });
            DropIndex("dbo.FinancialScheduledTransaction", new[] { "GatewayId" });
            AddColumn("dbo.FinancialTransaction", "GatewayEntityTypeId", c => c.Int());
            AddColumn("dbo.FinancialTransaction", "ScheduledTransactionId", c => c.Int());
            AddColumn("dbo.FinancialPersonSavedAccount", "GatewayEntityTypeId", c => c.Int());
            AddColumn("dbo.FinancialScheduledTransaction", "NextPaymentDate", c => c.DateTime(storeType: "date"));
            AddColumn("dbo.FinancialScheduledTransaction", "LastStatusUpdateDateTime", c => c.DateTime(storeType: "date"));
            AddColumn("dbo.FinancialScheduledTransaction", "GatewayEntityTypeId", c => c.Int());
            AddColumn("dbo.FinancialScheduledTransaction", "GatewayScheduleId", c => c.String());
            CreateIndex("dbo.FinancialTransaction", "GatewayEntityTypeId");
            CreateIndex("dbo.FinancialScheduledTransaction", "GatewayEntityTypeId");
            CreateIndex("dbo.FinancialTransaction", "ScheduledTransactionId");
            CreateIndex("dbo.FinancialPersonSavedAccount", "GatewayEntityTypeId");
            AddForeignKey("dbo.FinancialTransaction", "GatewayEntityTypeId", "dbo.EntityType", "Id");
            AddForeignKey("dbo.FinancialScheduledTransaction", "GatewayEntityTypeId", "dbo.EntityType", "Id");
            AddForeignKey("dbo.FinancialTransaction", "ScheduledTransactionId", "dbo.FinancialScheduledTransaction", "Id");
            AddForeignKey("dbo.FinancialPersonSavedAccount", "GatewayEntityTypeId", "dbo.EntityType", "Id");
            DropColumn("dbo.FinancialTransaction", "GatewayId");
            DropColumn("dbo.FinancialPersonSavedAccount", "GatewayId");
            DropColumn("dbo.FinancialScheduledTransaction", "GatewayId");
            DropTable("dbo.FinancialGateway");

            // Delete Payment Type defined type (not needed)
            DeleteDefinedType( "23E80D98-017E-47B9-BAF3-AC442A1EC3EE" );

            // Combine the pledge and transaction frequency types into one
            Sql( @"
    DECLARE @FrequencyValueTypeId int
    SET @FrequencyValueTypeId = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '1F645CFB-5BBD-4465-B9CA-0D2104A1479B')

    UPDATE P SET [PledgeFrequencyValueId] = NV.[Id]
    FROM [FinancialPledge] P
    INNER JOIN [DefinedValue] OV ON OV.[Id] = P.[PledgeFrequencyValueId]
    INNER JOIN [DefinedValue] NV ON NV.[DefinedTypeId] = @FrequencyValueTypeId AND NV.[Name] = OV.[Name]
" );
            DeleteDefinedType( "9E358FBE-2321-4C54-895F-C888E29298AE" );    // Batch Type
            DeleteDefinedType( "059F69C0-BF9B-4D53-B7CD-2D3B7C647C5F" );    // Frequency Type
            DeleteDefinedValue( "A5A12067-322E-44A4-94C4-561312F9913C" );   // One-Time - Future

            AddDefinedValue( "1F645CFB-5BBD-4465-B9CA-0D2104A1479B", "Quarterly", "Every Quarter", "BF08EA03-C52A-4364-B142-12EBCA7CA14A" );
            AddDefinedValue( "1F645CFB-5BBD-4465-B9CA-0D2104A1479B", "Twice a Year", "Every Six Months", "691BB8AB-5F96-4E88-847C-CB970D9E87FA" );
            AddDefinedValue( "1F645CFB-5BBD-4465-B9CA-0D2104A1479B", "Yearly", "Every Year", "AC88C37A-901E-4CBB-947B-11348C208192" );

            AddDefinedValue( "1D1304DE-E83A-44AF-B11D-0C66DD600B81", "Credit Card", "Credit Card", "928A2E04-C77B-4282-888F-EC549CEE026A" );
            AddDefinedValue( "1D1304DE-E83A-44AF-B11D-0C66DD600B81", "ACH", "Bank Account (ACH)", "DABEE8FD-AEDF-43E1-8547-4C97FA14D9B6" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteDefinedValue( "DABEE8FD-AEDF-43E1-8547-4C97FA14D9B6" );
            DeleteDefinedValue( "928A2E04-C77B-4282-888F-EC549CEE026A" );

            DeleteDefinedValue( "AC88C37A-901E-4CBB-947B-11348C208192" );
            DeleteDefinedValue( "691BB8AB-5F96-4E88-847C-CB970D9E87FA" );
            DeleteDefinedValue( "BF08EA03-C52A-4364-B142-12EBCA7CA14A" );

            AddDefinedValue( "1F645CFB-5BBD-4465-B9CA-0D2104A1479B", "One-Time (Future)", "One Time Future", "A5A12067-322E-44A4-94C4-561312F9913C" );

            AddDefinedType( "Financial", "Frequency Type", "Types of payment frequencies", "059F69C0-BF9B-4D53-B7CD-2D3B7C647C5F" );
            AddDefinedValue( "059F69C0-BF9B-4D53-B7CD-2D3B7C647C5F", "Weekly", "Every Week", "53957842-DE28-498C-AC61-65B32E8034CB" );
            AddDefinedValue( "059F69C0-BF9B-4D53-B7CD-2D3B7C647C5F", "Bi-Weekly", "Every Two Weeks", "FBD9315C-5E0B-49D8-9D28-27EBF268E67B" );
            AddDefinedValue( "059F69C0-BF9B-4D53-B7CD-2D3B7C647C5F", "Monthly", "Once a Month", "C53509B1-FC2B-46C8-A00E-58392FBE9408" );
            AddDefinedValue( "059F69C0-BF9B-4D53-B7CD-2D3B7C647C5F", "Twice a Month", "Twice a Month", "CA25B6D3-9BA4-4E88-9A5A-BF44B2898383" );

            Sql( @"
    DECLARE @FrequencyValueTypeId int
    SET @FrequencyValueTypeId = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '059F69C0-BF9B-4D53-B7CD-2D3B7C647C5F')

    UPDATE P SET [PledgeFrequencyValueId] = NV.[Id]
    FROM [FinancialPledge] P
    INNER JOIN [DefinedValue] OV ON OV.[Id] = P.[PledgeFrequencyValueId]
    INNER JOIN [DefinedValue] NV ON NV.[DefinedTypeId] = @FrequencyValueTypeId AND NV.[Name] = OV.[Name]
" );

            AddDefinedType( "Financial", "Payment Type", "The type of payment associated with a transaction", "23E80D98-017E-47B9-BAF3-AC442A1EC3EE" );
            AddDefinedValue( "23E80D98-017E-47B9-BAF3-AC442A1EC3EE", "Credit Card", "Credit Card payment type", "09412338-AAAA-4644-BA2A-4CADBE653468" );
            AddDefinedValue( "23E80D98-017E-47B9-BAF3-AC442A1EC3EE", "Checking/ACH", "Checking/ACH payment type", "FFAD975C-7504-418F-8959-30BD0C62CD30" );

            AddDefinedType( "Financial", "Batch Type", "Batch Types", "9E358FBE-2321-4C54-895F-C888E29298AE" );
            AddDefinedValue( "9E358FBE-2321-4C54-895F-C888E29298AE", "ACH", "ACH", "E6F877F3-D2CC-443E-976A-4402502F544F" );
            AddDefinedValue( "9E358FBE-2321-4C54-895F-C888E29298AE", "Visa", "Visa", "24CC2E82-B2B6-4037-87AE-39EEAFE06712" );
            AddDefinedValue( "9E358FBE-2321-4C54-895F-C888E29298AE", "MasterCard", "MasterCard", "50F625F8-F1BE-4FA0-B99F-3FA852D87DD1" );
            AddDefinedValue( "9E358FBE-2321-4C54-895F-C888E29298AE", "Discover", "Discover", "18DF8254-0C68-4FE0-973E-C0B1767EFD3F" );
            AddDefinedValue( "9E358FBE-2321-4C54-895F-C888E29298AE", "Amex", "Amex", "378D8EAD-7FA6-4D0D-862D-ED6E04B17770" );
            AddDefinedValue( "9E358FBE-2321-4C54-895F-C888E29298AE", "PayPal", "PayPal", "4832DA18-DD18-477F-BFDB-ABFC28FE5743" );

            CreateTable(
                "dbo.FinancialGateway",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(),
                        EntityTypeId = c.Int(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.FinancialScheduledTransaction", "GatewayId", c => c.Int());
            AddColumn("dbo.FinancialPersonSavedAccount", "GatewayId", c => c.Int());
            AddColumn("dbo.FinancialTransaction", "GatewayId", c => c.Int());
            DropForeignKey("dbo.FinancialPersonSavedAccount", "GatewayEntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.FinancialTransaction", "ScheduledTransactionId", "dbo.FinancialScheduledTransaction");
            DropForeignKey("dbo.FinancialScheduledTransaction", "GatewayEntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.FinancialTransaction", "GatewayEntityTypeId", "dbo.EntityType");
            DropIndex("dbo.FinancialPersonSavedAccount", new[] { "GatewayEntityTypeId" });
            DropIndex("dbo.FinancialTransaction", new[] { "ScheduledTransactionId" });
            DropIndex("dbo.FinancialScheduledTransaction", new[] { "GatewayEntityTypeId" });
            DropIndex("dbo.FinancialTransaction", new[] { "GatewayEntityTypeId" });
            DropColumn("dbo.FinancialScheduledTransaction", "GatewayScheduleId");
            DropColumn("dbo.FinancialScheduledTransaction", "GatewayEntityTypeId");
            DropColumn("dbo.FinancialScheduledTransaction", "LastStatusUpdateDateTime");
            DropColumn("dbo.FinancialScheduledTransaction", "NextPaymentDate");
            DropColumn("dbo.FinancialPersonSavedAccount", "GatewayEntityTypeId");
            DropColumn("dbo.FinancialTransaction", "ScheduledTransactionId");
            DropColumn("dbo.FinancialTransaction", "GatewayEntityTypeId");
            CreateIndex("dbo.FinancialScheduledTransaction", "GatewayId");
            CreateIndex("dbo.FinancialPersonSavedAccount", "GatewayId");
            CreateIndex("dbo.FinancialTransaction", "GatewayId");
            CreateIndex("dbo.FinancialGateway", "EntityTypeId");
            AddForeignKey("dbo.FinancialScheduledTransaction", "GatewayId", "dbo.FinancialGateway", "Id");
            AddForeignKey("dbo.FinancialPersonSavedAccount", "GatewayId", "dbo.FinancialGateway", "Id");
            AddForeignKey("dbo.FinancialTransaction", "GatewayId", "dbo.FinancialGateway", "Id");
            AddForeignKey("dbo.FinancialGateway", "EntityTypeId", "dbo.EntityType", "Id");
        }
    }
}
