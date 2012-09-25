namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AttributeQualifierValueNotRequired : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.coreAttributeQualifier", "Value", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.coreAttributeQualifier", "Value", c => c.String(nullable: false));
        }
    }
}
