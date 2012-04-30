namespace Rock.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AttributeDefaultValueNotRequired : DbMigration
    {
        public override void Up()
        {
            AlterColumn("coreAttribute", "DefaultValue", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("coreAttribute", "DefaultValue", c => c.String(nullable: false));
        }
    }
}
