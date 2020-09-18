using System;
using com.bemaservices.DoorControl.DSX.Utility;
using Quartz;
using Rock;
using Rock.Attribute;
namespace com.bemaservices.DoorControl.DSX.Jobs
{
    [AttributeField( com.bemaservices.RoomManagement.SystemGuid.EntityType.RESERVATION, AttributeKeys.OverrideGroupAttribute, "This is the Reservation Attribute that stores the DSX Override Information.", true, false, com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.RESERVATION_DOOR_OVERRIDES_ATTRIBUTE )]
    [AttributeField( "0D6410AD-C83C-47AC-AF3D-616D09EDF63B", AttributeKeys.OverrideLocationAttribute, "This is the Location Attribute that stores the DSX Override Group Identifier", true, false, com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.LOCATION_OVERRIDE_GROUP )]
    [AttributeField( "0D6410AD-C83C-47AC-AF3D-616D09EDF63B", AttributeKeys.SharedLocationAttribute, "This is the Location Attribute that stores a list of shared rooms.", true, false, com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.LOCATION_SHARED_DOOR_ATTRIBUTE )]
    [AttributeField( "0D6410AD-C83C-47AC-AF3D-616D09EDF63B", AttributeKeys.RoomNameLocationAttribute, "This is the Location Attribute that stores the Room Name" )]
    [AttributeField( com.bemaservices.RoomManagement.SystemGuid.EntityType.RESERVATION, AttributeKeys.ProcessDoorLockAttribute, "This is the Reservation Attribute that determines if the normal door lock process should happen.", true, false, com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.RESERVATION_PROCESS_DOOR_LOCK_ATTRIBUTE )]
    [TextField( AttributeKeys.DSXSQLServer, "The DSX SQL Server Name we will be pushing data into", true, "", "DSX Settings", 0 )]
    [TextField( AttributeKeys.DSXDatabaseName, "The DSX Database we will be pushing data into", true, "", "DSX Settings", 1 )]
    [TextField( AttributeKeys.DSXSQLUsername, "The DSX SQL Username for connecting to the DB", true, "", "DSX Settings", 2 )]
    [TextField( AttributeKeys.DSXSQLPassword, "The DSX SQL Password for connecting to the DB", true, "", "DSX Settings", 3, null, true )]
    [BooleanField( AttributeKeys.TestMode, "Allows you to validate everything is working as expected without writing data", true )]

    [DisallowConcurrentExecution]
    public class UpdateDoorLocks : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public UpdateDoorLocks()
        {
        }

        private static class AttributeKeys
        {
            public const string OverrideLocationAttribute = "Override Location Attribute";
            public const string OverrideGroupAttribute = "Override Group Attribute";
            public const string ProcessDoorLockAttribute = "Process Door Lock";
            public const string DSXSQLServer = "DSX SQL Server";
            public const string DSXDatabaseName = "DSX Database Name";
            public const string DSXSQLUsername = "DSX SQL Username";
            public const string DSXSQLPassword = "DSX SQL Password";
            public const string DoorOverrides = "Door Overrides";
            public const string TestMode = "Test Mode";
            public const string SharedLocationAttribute = "Shared Location Attribute";
            public const string RoomNameLocationAttribute = "Room Name Location Attribute";
        }

        /// <summary>
        /// Job that will process DSX Results.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            UpdateDoorLocks_Config config = GetJobConfig( dataMap );
            var helpers = new Helpers();

            // Configuring job to run for data from tomorrow
            config.DateToSync = RockDateTime.Today.AddDays( 1 );

            // Return result to job
            var results = helpers.ProcessUnlocksForDay( config );
            context.Result = string.Join( System.Environment.NewLine, results.ToArray() );
        }

        /// <summary>
        /// Gets the job configuration and loads it into configuration class
        /// </summary>
        /// <param name="dataMap">The data map.</param>
        /// <returns></returns>
        private UpdateDoorLocks_Config GetJobConfig( JobDataMap dataMap )
        {
            var config = new UpdateDoorLocks_Config();

            // Getting RoomNameLocationAttribute
            if( dataMap.GetString(AttributeKeys.RoomNameLocationAttribute.Replace( " ", "" ) ).AsGuidOrNull() != null )
            {
                config.RoomNameLocationAttribute = dataMap.GetString( AttributeKeys.RoomNameLocationAttribute.Replace( " ", "" ) ).AsGuid();
            }

            // Getting OverrideLocationAttribute
            if ( dataMap.GetString( AttributeKeys.OverrideLocationAttribute.Replace( " ", "" ) ).AsGuidOrNull() != null )
            {
                config.OverrideLocationAttribute = dataMap.GetString( AttributeKeys.OverrideLocationAttribute.Replace( " ", "" ) ).AsGuid();
            }

            // Getting OverrideGroupAttribute
            if ( dataMap.GetString( AttributeKeys.OverrideGroupAttribute.Replace( " ", "" ) ).AsGuidOrNull() != null )
            {
                config.OverrideGroupAttribute = dataMap.GetString( AttributeKeys.OverrideGroupAttribute.Replace( " ", "" ) ).AsGuid();
            }

            // Getting ProcessDoorLockAttribute
            if ( dataMap.GetString( AttributeKeys.ProcessDoorLockAttribute.Replace( " ", "" ) ).AsGuidOrNull() != null )
            {
                config.ProcessDoorLockAttribute = dataMap.GetString( AttributeKeys.ProcessDoorLockAttribute.Replace( " ", "" ) ).AsGuid();
            }

            // Getting SharedLocationAttribute
            if ( dataMap.GetString( AttributeKeys.SharedLocationAttribute.Replace( " ", "" ) ).AsGuidOrNull() != null )
            {
                config.SharedLocationAttribute = dataMap.GetString( AttributeKeys.SharedLocationAttribute.Replace( " ", "" ) ).AsGuid();
            }

            // Getting DSX Connection String
            if (
                !string.IsNullOrEmpty( dataMap.GetString( AttributeKeys.DSXSQLServer.Replace( " ", "" ) ) ) &&
                !string.IsNullOrEmpty( dataMap.GetString( AttributeKeys.DSXDatabaseName.Replace( " ", "" ) ) ) &&
                !string.IsNullOrEmpty( dataMap.GetString( AttributeKeys.DSXSQLUsername.Replace( " ", "" ) ) ) &&
                !string.IsNullOrEmpty( dataMap.GetString( AttributeKeys.DSXSQLPassword.Replace( " ", "" ) ) )
            )
            {
                config.DSXConnectionString = string.Format(
                    "Data Source={0};Initial Catalog={1}; User Id={2}; password={3};MultipleActiveResultSets=true",
                    dataMap.GetString( AttributeKeys.DSXSQLServer.Replace( " ", "" ) ),
                    dataMap.GetString( AttributeKeys.DSXDatabaseName.Replace( " ", "" ) ),
                    dataMap.GetString( AttributeKeys.DSXSQLUsername.Replace( " ", "" ) ),
                    dataMap.GetString( AttributeKeys.DSXSQLPassword.Replace( " ", "" ) )
                );
            }

            // Checking if we are in Test Mode
            config.TestMode = dataMap.GetString( AttributeKeys.TestMode.Replace( " ", "" ) ).AsBoolean();

            return config;
        }
    }
}

