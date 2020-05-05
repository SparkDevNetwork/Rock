using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Model;
using Rock.Plugin;

namespace com.bemaservices.DoorControl.DSX.Migrations
{
    [MigrationNumber( 9, "1.8.0" )]
    public class IsHvacOnly : Migration
    {
        public override void Up()
        {
            // Deletes the Door Lock if the reservation or location is removed
            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_DoorLock] ADD IsHvacOnly bit null
                " );

           
        }

        public override void Down()
        {

        }
    }
}
