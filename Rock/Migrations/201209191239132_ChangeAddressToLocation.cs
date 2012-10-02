namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeAddressToLocation : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.crmLocation",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Raw = c.String(maxLength: 400),
                        Street1 = c.String(maxLength: 100),
                        Street2 = c.String(maxLength: 100),
                        City = c.String(maxLength: 50),
                        State = c.String(maxLength: 50),
                        Country = c.String(maxLength: 50),
                        Zip = c.String(maxLength: 10),
                        Latitude = c.Double(nullable: false),
                        Longitude = c.Double(nullable: false),
                        ParcelId = c.String(maxLength: 50),
                        StandardizeAttempt = c.DateTime(),
                        StandardizeService = c.String(maxLength: 50),
                        StandardizeResult = c.String(maxLength: 50),
                        StandardizeDate = c.DateTime(),
                        GeocodeAttempt = c.DateTime(),
                        GeocodeService = c.String(maxLength: 50),
                        GeocodeResult = c.String(maxLength: 50),
                        GeocodeDate = c.DateTime(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("dbo.crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
			Sql( @"
				SET IDENTITY_INSERT crmLocation ON
				INSERT INTO [crmLocation] (
					 [Id]
					,[Raw]
					,[Street1]
					,[Street2]
					,[City]
					,[State]
					,[Country]
					,[Zip]
					,[Latitude]
					,[Longitude]
					,[ParcelId]
					,[StandardizeAttempt]
					,[StandardizeService]
					,[StandardizeResult]
					,[StandardizeDate]
					,[GeocodeAttempt]
					,[GeocodeService]
					,[GeocodeResult]
					,[GeocodeDate]
					,[CreatedDateTime]
					,[ModifiedDateTime]
					,[CreatedByPersonId]
					,[ModifiedByPersonId]
					,[Guid] )
				SELECT
					 [Id]
					,[Raw]
					,[Street1]
					,[Street2]
					,[City]
					,[State]
					,[Country]
					,[Zip]
					,[Latitude]
					,[Longitude]
					,NULL
					,[StandardizeAttempt]
					,[StandardizeService]
					,[StandardizeResult]
					,[StandardizeDate]
					,[GeocodeAttempt]
					,[GeocodeService]
					,[GeocodeResult]
					,[GeocodeDate]
					,[CreatedDateTime]
					,[ModifiedDateTime]
					,[CreatedByPersonId]
					,[ModifiedByPersonId]
					,[Guid]
				FROM [crmAddress]
				SET IDENTITY_INSERT crmLocation OFF
" );

            DropForeignKey("crmAddress", "CreatedByPersonId", "crmPerson");
            DropForeignKey("crmAddress", "ModifiedByPersonId", "crmPerson");
            DropIndex("crmAddress", new[] { "CreatedByPersonId" });
            DropIndex("crmAddress", new[] { "ModifiedByPersonId" });
            DropTable("crmAddress");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.crmAddress",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Raw = c.String(maxLength: 400),
                        Street1 = c.String(maxLength: 100),
                        Street2 = c.String(maxLength: 100),
                        City = c.String(maxLength: 50),
                        State = c.String(maxLength: 50),
                        Country = c.String(maxLength: 50),
                        Zip = c.String(maxLength: 10),
                        Latitude = c.Double(nullable: false),
                        Longitude = c.Double(nullable: false),
                        StandardizeAttempt = c.DateTime(),
                        StandardizeService = c.String(maxLength: 50),
                        StandardizeResult = c.String(maxLength: 50),
                        StandardizeDate = c.DateTime(),
                        GeocodeAttempt = c.DateTime(),
                        GeocodeService = c.String(maxLength: 50),
                        GeocodeResult = c.String(maxLength: 50),
                        GeocodeDate = c.DateTime(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("crmAddress", "ModifiedByPersonId");
            CreateIndex("crmAddress", "CreatedByPersonId");
            AddForeignKey("crmAddress", "ModifiedByPersonId", "crmPerson", "Id");
            AddForeignKey("crmAddress", "CreatedByPersonId", "crmPerson", "Id");

			Sql( @"
				SET IDENTITY_INSERT crmAddress ON
				INSERT INTO [crmAddress] (
					 [Id]
					,[Raw]
					,[Street1]
					,[Street2]
					,[City]
					,[State]
					,[Country]
					,[Zip]
					,[Latitude]
					,[Longitude]
					,[StandardizeAttempt]
					,[StandardizeService]
					,[StandardizeResult]
					,[StandardizeDate]
					,[GeocodeAttempt]
					,[GeocodeService]
					,[GeocodeResult]
					,[GeocodeDate]
					,[CreatedDateTime]
					,[ModifiedDateTime]
					,[CreatedByPersonId]
					,[ModifiedByPersonId]
					,[Guid] )
				SELECT
					 [Id]
					,[Raw]
					,[Street1]
					,[Street2]
					,[City]
					,[State]
					,[Country]
					,[Zip]
					,[Latitude]
					,[Longitude]
					,[StandardizeAttempt]
					,[StandardizeService]
					,[StandardizeResult]
					,[StandardizeDate]
					,[GeocodeAttempt]
					,[GeocodeService]
					,[GeocodeResult]
					,[GeocodeDate]
					,[CreatedDateTime]
					,[ModifiedDateTime]
					,[CreatedByPersonId]
					,[ModifiedByPersonId]
					,[Guid]
				FROM [crmLocation]
				SET IDENTITY_INSERT crmAddress OFF
" );

            DropIndex("dbo.crmLocation", new[] { "ModifiedByPersonId" });
            DropIndex("dbo.crmLocation", new[] { "CreatedByPersonId" });
            DropForeignKey("dbo.crmLocation", "ModifiedByPersonId", "dbo.crmPerson");
            DropForeignKey("dbo.crmLocation", "CreatedByPersonId", "dbo.crmPerson");
            DropTable("dbo.crmLocation");
        }
    }
}
