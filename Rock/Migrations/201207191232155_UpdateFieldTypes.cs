namespace Rock.Migrations
{
    using System.Data.Entity.Migrations;
    
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
