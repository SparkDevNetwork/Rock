namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
#pragma warning disable 1591
    public partial class UpdateEnums : DbMigration
    {
        public override void Up()
        {
            Sql( @"
UPDATE [crmPerson] SET [Gender] = 0 WHERE [Gender] IS NULL
" ); 
            AlterColumn( "dbo.crmPerson", "Gender", c => c.Int( nullable: false ) );
        }
        
        public override void Down()
        {
            AlterColumn("dbo.crmPerson", "Gender", c => c.Int());
        }
    }
}
