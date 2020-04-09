
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using com.bemaservices.DoorControl.DSX.Utility;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;

namespace com.bemaservices.DoorControl.DSX.Workflow.Action.BEMA_Services
{
    [ActionCategory( "BEMA Services > Workflow Extensions" )]
    [Description( "Pushed Schedule from Rock to DSX" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Update Door Lock Config in DSX" )]

    [TextField(
        "DSX SQL Server",
        Description = "The DSX SQL Server Name we will be pushing data into",
        IsRequired = true,
        DefaultValue = "",
        Category = "DSX Settings",
        Order = 0
    )]
    [TextField(
        "DSX Database Name",
        Description = "The DSX Database we will be pushing data into",
        IsRequired = true,
        DefaultValue = "",
        Category = "DSX Settings",
        Order = 1
    )]
    [TextField(
        "DSX SQL Username",
        Description = "The DSX SQL Username for connecting to the DB",
        IsRequired = true,
        DefaultValue = "",
        Category = "DSX Settings",
        Order = 2
    )]
    [TextField( "DSX SQL Password", "The DSX SQL Password for connecting to the DB", true, "", "DSX Settings", 3, "DSXSQLPassword", true )]
    [BooleanField(
        "Test Mode",
        Description = "Allows you to validate everything is working as expected without writing data",
        IsRequired = true
    )]
    [WorkflowAttribute( "Date To Process", "The Date we are going to push to DSX", true )]
    [AttributeField( com.centralaz.RoomManagement.SystemGuid.EntityType.RESERVATION, "Override Group Attribute", "This is the Reservation Attribute that stores the DSX Override Information.", true, false, com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.RESERVATION_DOOR_OVERRIDES_ATTRIBUTE, "Attributes" )]
    [AttributeField( "0D6410AD-C83C-47AC-AF3D-616D09EDF63B", "Override Location Attribute", "This is the Location Attribute that stores the DSX Override Group Identifier", true, false, com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.LOCATION_OVERRIDE_GROUP, "Attributes" )]
    [AttributeField( "0D6410AD-C83C-47AC-AF3D-616D09EDF63B", "Shared Location Attribute", "This is the Location Attribute that stores a list of shared rooms.", true, false, com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.LOCATION_SHARED_DOOR_ATTRIBUTE, "Attributes" )]
    [AttributeField( "0D6410AD-C83C-47AC-AF3D-616D09EDF63B", "Room Name Location Attribute", "This is the Location Attribute that stores the Room Name", true, false, "", "Attributes" )]
    [AttributeField( com.centralaz.RoomManagement.SystemGuid.EntityType.RESERVATION, "Process Door Lock", "This is the Reservation Attribute that determines if the normal door lock process should happen.", true, false, com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.RESERVATION_PROCESS_DOOR_LOCK_ATTRIBUTE )]

    public class UpdateDoorLocks : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            var config = GetConfig( action );

            var helpers = new Helpers();
            var results = helpers.ProcessUnlocksForDay( config );
            errorMessages.AddRange( results );

            return true;
        }

        /// <summary>
        /// Gets the configuration and loads it into configuration class
        /// </summary>
        /// <param name="dataMap">The data map.</param>
        /// <returns></returns>
        private UpdateDoorLocks_Config GetConfig( WorkflowAction action )
        {
            var config = new UpdateDoorLocks_Config();

            // Getting DateToSync
            if ( GetAttributeValue( action, "DateToProcess" ) != null )
            {
                var workflowAttributeGuid = GetAttributeValue( action, "DateToProcess" );

                if ( !string.IsNullOrEmpty( workflowAttributeGuid ) )
                {
                    var date = action.GetWorklowAttributeValue( Guid.Parse( workflowAttributeGuid ) );
                    config.DateToSync = DateTime.Parse( date );

                }
            }

            // Getting RoomNameLocationAttribute
            if ( GetAttributeValue( action, "RoomNameLocationAttribute" ).AsGuidOrNull() != null )
            {
                config.RoomNameLocationAttribute = GetAttributeValue( action, "RoomNameLocationAttribute" ).AsGuid();
            }

            // Getting OverrideLocationAttribute
            if ( GetAttributeValue( action, "OverrideLocationAttribute" ).AsGuidOrNull() != null )
            {
                config.OverrideLocationAttribute = GetAttributeValue( action, "OverrideLocationAttribute" ).AsGuid();
            }

            // Getting OverrideGroupAttribute
            if ( GetAttributeValue( action, "OverrideGroupAttribute" ).AsGuidOrNull() != null )
            {
                config.OverrideGroupAttribute = GetAttributeValue( action, "OverrideGroupAttribute" ).AsGuid();
            }

            // Getting DateToSync
            if ( GetAttributeValue( action, "ProcessDoorLock" ) != null )
            {
                config.ProcessDoorLockAttribute = GetAttributeValue( action, "ProcessDoorLock" ).AsGuid();
            }

            // Getting SharedLocationAttribute
            if ( GetAttributeValue( action, "SharedLocationAttribute" ).AsGuidOrNull() != null )
            {
                config.SharedLocationAttribute = GetAttributeValue( action, "SharedLocationAttribute" ).AsGuid();
            }

            // Getting DSX Connection String
            if (
                !string.IsNullOrEmpty( GetAttributeValue( action, "DSXSQLServer" ) ) &&
                !string.IsNullOrEmpty( GetAttributeValue( action, "DSXDatabaseName" ) ) &&
                !string.IsNullOrEmpty( GetAttributeValue( action, "DSXSQLUsername" ) ) &&
                !string.IsNullOrEmpty( GetAttributeValue( action, "DSXSQLPassword" ) )
            )
            {
                config.DSXConnectionString = string.Format(
                    "Data Source={0};Initial Catalog={1}; User Id={2}; password={3};MultipleActiveResultSets=true",
                    GetAttributeValue( action, "DSXSQLServer" ),
                    GetAttributeValue( action, "DSXDatabaseName" ),
                    GetAttributeValue( action, "DSXSQLUsername" ),
                    GetAttributeValue( action, "DSXSQLPassword" )
                );
            }

            // Checking if we are in Test Mode
            config.TestMode = GetAttributeValue( action, "TestMode" ).AsBoolean();

            return config;
        }
    }
}
