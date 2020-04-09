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
    [MigrationNumber( 4, "1.8.0" )]
    public class CreateJob : Migration
    {
        public override void Up()
        {
            Sql(
                string.Format( @"INSERT INTO [ServiceJob] (
                    [IsSystem],
                    [IsActive],
                    [Name],
                    [Description],
                    [Class],
                    [CronExpression],
                    [NotificationStatus],
                    [Guid],
                    [CreatedDateTime],
                    [ModifiedDateTime]
                ) VALUES(
                    0,
                    0,
                    'Process Door Locks',
                    'Job that looks at Room Management and Campus configuration to automatically unlock doors at specfic times',
                    'com.bemaservices.DoorControl.DSX.Jobs.UpdateDoorLocks',
                    '0 5 0 1/1 * ? *',
                    1,
                    '{0}',
                    '{1}',
                    '{1}'
                )",
                com.bemaservices.DoorControl.DSX.SystemGuid.ServiceJob.PROCESS_DOOR_LOCKS,
                RockDateTime.Now)
            );

               
        }

        public override void Down()
        {

        }
    }
}
