namespace Rock.Migrations
    
    using System;
    using System.Data.Entity.Migrations;
#pragma warning disable 1591
    public partial class AddCampusForeignKeys : DbMigration
        
        public override void Up()
            
            AddForeignKey("dbo.crmCampus", "CreatedByPersonId", "dbo.crmPerson", "Id");
            AddForeignKey("dbo.crmCampus", "ModifiedByPersonId", "dbo.crmPerson", "Id");
            CreateIndex("dbo.crmCampus", "CreatedByPersonId");
            CreateIndex("dbo.crmCampus", "ModifiedByPersonId");
        }
        
        public override void Down()
            
            DropIndex("dbo.crmCampus", new[]      "ModifiedByPersonId" });
            DropIndex("dbo.crmCampus", new[]      "CreatedByPersonId" });
            DropForeignKey("dbo.crmCampus", "ModifiedByPersonId", "dbo.crmPerson");
            DropForeignKey("dbo.crmCampus", "CreatedByPersonId", "dbo.crmPerson");
        }
    }
}
