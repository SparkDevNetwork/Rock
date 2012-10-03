namespace Rock.Migrations
{
    using System.Data.Entity.Migrations;
#pragma warning disable 1591
    public partial class UpdateFieldTypes : DbMigration
    {
        public override void Up()
        {
            DropColumn("coreAttributeQualifier", "Name");
            
            DataUp();

        }

        public override void Down()
        {
            DataDown();

            AddColumn( "coreAttributeQualifier", "Name", c => c.String( nullable: false, maxLength: 100 ) );
        }
    }
}
