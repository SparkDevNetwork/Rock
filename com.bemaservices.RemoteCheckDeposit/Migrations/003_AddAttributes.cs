using Rock.Plugin;

namespace com.bemaservices.RemoteCheckDeposit.Migrations
{
    [MigrationNumber( 3, "1.8.0" )]
    public class AddAttributes : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.FinancialBatch", Rock.SystemGuid.FieldType.BOOLEAN, "", "", "Deposited", "", "Shows if a Financial Batch has been Deposited", 0, "False", SystemGuid.Attribute.FINANCIAL_BATCH_DEPOSITED, "com.bemaservices.Deposited" );
        }

        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.FINANCIAL_BATCH_DEPOSITED );
        }
    }
}
