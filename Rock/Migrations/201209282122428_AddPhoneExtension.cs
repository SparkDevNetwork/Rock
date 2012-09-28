namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPhoneExtension : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.crmPhoneNumber", "Extension", c => c.String(maxLength: 20));
        }
        
        public override void Down()
        {
            DropColumn("dbo.crmPhoneNumber", "Extension");
        }
    }
}
