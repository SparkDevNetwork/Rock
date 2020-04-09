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
    [MigrationNumber( 5, "1.8.0" )]
    public class CreateLocationAttribute : Migration
    {
        public override void Up()
        {
            // Location Attribute
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.Location", Rock.SystemGuid.FieldType.KEY_VALUE_LIST, "", "", "Shared Doors", "", "Stores the Shared Door Relationships", 0, "", com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.LOCATION_SHARED_DOOR_ATTRIBUTE );
            RockMigrationHelper.AddAttributeQualifier(
                com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.LOCATION_SHARED_DOOR_ATTRIBUTE,
                "allowhtml",
                "False",
                com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.ATTRIBUTEQUALIFIER_LOCATION_SHARED_DOOR_ALLOWHTML
            );
            RockMigrationHelper.AddAttributeQualifier(
               com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.LOCATION_SHARED_DOOR_ATTRIBUTE,
               "customvalues",
               "SELECT [Id] AS [Value],[Name] AS [Text] FROM [Location] WHERE ISNULL([Name],'''') != '''' ORDER BY [Name] ASC",
               com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.ATTRIBUTEQUALIFIER_LOCATION_SHARED_DOOR_CUSTOMVALUES
           );
            RockMigrationHelper.AddAttributeQualifier(
               com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.LOCATION_SHARED_DOOR_ATTRIBUTE,
               "definedtype",
               "",
               com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.ATTRIBUTEQUALIFIER_LOCATION_SHARED_DOOR_DEFINEDTYPE
           );
            RockMigrationHelper.AddAttributeQualifier(
               com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.LOCATION_SHARED_DOOR_ATTRIBUTE,
               "displayvaluefirst",
               "False",
               com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.ATTRIBUTEQUALIFIER_LOCATION_SHARED_DOOR_DISPLAYVALUEFIRST
           );
            RockMigrationHelper.AddAttributeQualifier(
               com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.LOCATION_SHARED_DOOR_ATTRIBUTE,
               "keyprompt",
               "This is the DSX Override Group for the Shared Doors",
               com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.ATTRIBUTEQUALIFIER_LOCATION_SHARED_DOOR_KEYPROMPT
           );
            RockMigrationHelper.AddAttributeQualifier(
               com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.LOCATION_SHARED_DOOR_ATTRIBUTE,
               "valueprompt",
               "This is the Location on the other side of the Shared Doors",
               com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.ATTRIBUTEQUALIFIER_LOCATION_SHARED_DOOR_VALUEPROMPT
           );
        }

        public override void Down()
        {

        }
    }
}
