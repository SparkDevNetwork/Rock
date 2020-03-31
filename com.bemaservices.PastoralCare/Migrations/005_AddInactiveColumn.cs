using Rock.Plugin;

namespace com.bemaservices.PastoralCare.Migrations
{
    [MigrationNumber( 5, "1.9.4" )]
    public class AddInactiveColumn : Migration
    {
        public override void Up()
        {
            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareItem] ADD [IsActive] BIT
                    " );
        }
        public override void Down()
        {
        }
    }
}
