namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateEnums : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.crmPerson", "Gender", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.crmPerson", "Gender", c => c.Int());
        }
    }
}
