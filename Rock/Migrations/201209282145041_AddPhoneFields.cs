namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPhoneFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.crmPhoneNumber", "Extension", c => c.String(maxLength: 20));
            AddColumn("dbo.crmPhoneNumber", "IsMessagingEnabled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.crmPhoneNumber", "IsMessagingEnabled");
            DropColumn("dbo.crmPhoneNumber", "Extension");
        }
    }
}
