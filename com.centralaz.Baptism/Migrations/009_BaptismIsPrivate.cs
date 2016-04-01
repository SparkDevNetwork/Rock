using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock;
using Rock.Plugin;
using Rock.Web.Cache;
namespace com.centralaz.Baptism.Migrations
{
    [MigrationNumber( 9, "1.0.14" )]
    public class BaptismIsPrivate : Migration
    {
        public override void Up()
        {
            Sql( @"
            IF NOT EXISTS(SELECT * FROM sys.columns 
            WHERE Name = N'IsPrivateBaptism' AND Object_ID = Object_ID(N'[_com_centralaz_Baptism_Baptizee]'))
BEGIN
            ALTER TABLE [_com_centralaz_Baptism_Baptizee]
            ADD IsPrivateBaptism BIT
END
            " );
        }

        public override void Down()
        {

        }
    }
}