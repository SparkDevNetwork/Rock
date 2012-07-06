namespace Rock.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class test : DbMigration
    {
        public override void Up()
        {
            DropColumn("coreAttributeQualifier", "Name");
        }
        
        public override void Down()
        {
            AddColumn("coreAttributeQualifier", "Name", c => c.String(nullable: false, maxLength: 100));
        }
    }
}
