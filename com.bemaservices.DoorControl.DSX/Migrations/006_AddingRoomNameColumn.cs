using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Plugin;

namespace com.bemaservices.DoorControl.DSX.Migrations
{
    [MigrationNumber( 6, "1.8.0" )]
    public class AddingRoomNameColumn : Migration
    {
        public override void Up()
        {
            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_DoorLock]
                ADD [RoomName] [varchar](255)
            " );
        }

        public override void Down()
        {

        }
    }
}
