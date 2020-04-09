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
    [MigrationNumber( 3, "1.8.0" )]
    public class CreateAttributes : Migration
    {
        public override void Up()
        {
            RockContext rockContext = new RockContext();
            AttributeMatrixTemplateService attributeMatrixTemplateService = new AttributeMatrixTemplateService( rockContext );

            // Reservation Attribute
            RockMigrationHelper.AddEntityAttribute( "com.centralaz.RoomManagement.Model.Reservation", "F16FC460-DC1E-4821-9012-5F21F974C677", "", "", "Door Overrides", "", "Stores Door Lock Overrides", 0, "", com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.RESERVATION_DOOR_OVERRIDES_ATTRIBUTE );
            RockMigrationHelper.AddAttributeQualifier(
                com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.RESERVATION_DOOR_OVERRIDES_ATTRIBUTE,
                "attributematrixtemplate",
                attributeMatrixTemplateService.Get( Guid.Parse( com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.RESERVATION_DOOR_OVERRIDES_ATTRIBUTEMATRIX ) ).Id.ToString(),
                com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.ATTRIBUTEQUALIFIER_RESERVATION_DOOR_OVERRIDES
            );

            // Campus
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.Campus", "F16FC460-DC1E-4821-9012-5F21F974C677", "", "", "Door Overrides", "", "Stores the Campus Door Lock Overrides", 0, "", com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.CAMPUS_DOOR_OVERRIDES_ATTRIBUTE );
            RockMigrationHelper.AddAttributeQualifier(
                com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.CAMPUS_DOOR_OVERRIDES_ATTRIBUTE,
                "attributematrixtemplate",
                attributeMatrixTemplateService.Get( Guid.Parse( com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.CAMPUS_DOOR_OVERRIDES_ATTRIBUTEMATRIX ) ).Id.ToString(),
                com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.ATTRIBUTEQUALIFIER_CAMPUS_DOOR_OVERRIDES
            );
        }

        public override void Down()
        {

        }
    }
}
