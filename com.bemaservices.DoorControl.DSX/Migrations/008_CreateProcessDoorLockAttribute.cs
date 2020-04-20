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
    [MigrationNumber( 8, "1.8.0" )]
    public class CreateProcessDoorLockAttribute : Migration
    {
        public override void Up()
        {
            // Process Door Lock Attribute
            RockMigrationHelper.AddEntityAttribute( "com.centralaz.RoomManagement.Model.Reservation", Rock.SystemGuid.FieldType.BOOLEAN, "", "", "Process Door Lock", "", "Determines if a door lock should be added for the reservation.", 0, "", com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.RESERVATION_PROCESS_DOOR_LOCK_ATTRIBUTE );
            RockMigrationHelper.AddAttributeQualifier(
                com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.RESERVATION_PROCESS_DOOR_LOCK_ATTRIBUTE,
                "falseText",
                "No",
                com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.ATTRIBUTEQUALIFIER_RESERVATION_PROCESS_DOOR_LOCK_FALSETEXT
            );
            RockMigrationHelper.AddAttributeQualifier(
               com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.RESERVATION_PROCESS_DOOR_LOCK_ATTRIBUTE,
               "truetext",
               "Yes",
               com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.ATTRIBUTEQUALIFIER_RESERVATION_PROCESS_DOOR_LOCK_TRUETEXT
           );
        }

        public override void Down()
        {

        }
    }
}
