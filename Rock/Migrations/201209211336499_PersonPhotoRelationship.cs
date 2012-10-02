namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PersonPhotoRelationship : DbMigration
    {
        public override void Up()
        {
            AddForeignKey("dbo.crmPerson", "PhotoId", "dbo.cmsFile", "Id");
            CreateIndex("dbo.crmPerson", "PhotoId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.crmPerson", new[] { "PhotoId" });
            DropForeignKey("dbo.crmPerson", "PhotoId", "dbo.cmsFile");
        }
    }
}
