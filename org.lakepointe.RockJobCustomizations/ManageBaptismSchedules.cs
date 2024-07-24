using System;
using System.Collections.Generic;
using System.Linq;

using Quartz;
using Rock;
using Rock.Data;
using Rock.Attribute;
using Rock.Model;
using System.Data.Entity;
using Rock.Utility.Enums;

namespace org.lakepointe.RockJobCustomizations
{
    [CodeEditorField(
        name: "Configuration JSON",
        description: "A JSON object containing the configuration values for this job.",
        required: true,
        order: 0 )]

    [DisallowConcurrentExecution]
    public class ManageBaptismSchedules : IJob
    {
        BaptismConfig _baptismConfig = null;

        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            _baptismConfig = dataMap.GetString( "ConfigurationJSON" ).FromJsonOrThrow<BaptismConfig>();
            string output = "";

            if ( _baptismConfig == null ) // Shouldn't happen since we're using FromJsonOrThrow, but we'll check just in case
            {
                throw new ArgumentException( "Invalid configuration JSON: Config was null." );
            }
            else if ( _baptismConfig.baptismTypes == null )
            {
                throw new ArgumentException( "Invalid configuration JSON: baptismTypes was not found." );
            }
            else if ( _baptismConfig.baptismTypes.Count <= 0 )
            {
                throw new ArgumentException( "Invalid configuration JSON: baptismTypes was empty." );
            }
            else if ( _baptismConfig.baptismTypes.Where( bt => bt.campuses != null && bt.campuses.Count >= 0 ).Any() == false )
            {
                throw new ArgumentException( "Invalid configuration JSON: No campuses were found." );
            }

            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );
            var groupLocationService = new GroupLocationService( rockContext );
            var campusService = new CampusService( rockContext );
            var scheduleService = new ScheduleService( rockContext );

            foreach ( var baptismType in _baptismConfig.baptismTypes ?? Enumerable.Empty<BaptismType>() )
            {
                string deletedOutput = "";
                string addedOutput = "";

                var baptismTypeObject = groupService.Get( baptismType.parentGroupId );

                foreach ( var campus in baptismType.campuses ?? Enumerable.Empty<BaptismCampus>() )
                {
                    var campusObject = campusService.Get( campus.campusId );

                    // Get all of the groups for this Baptism Type and Campus that have a checkin schedule attached
                    var groups = groupService.Queryable().AsNoTracking()
                        .Where( g =>
                            g.GroupTypeId == baptismType.groupTypeId
                            && g.ParentGroup.ParentGroupId == baptismType.parentGroupId
                            && g.CampusId == campus.campusId
                            && g.GroupLocations.Count > 0 );
                    foreach ( var group in groups )
                    {
                        foreach( var location in group.GroupLocations?.ToList() )
                        {
                            groupLocationService.Attach( location );
                            groupLocationService.Delete( location );
                        }
                        deletedOutput += $"Deleted GroupLocation from: {campusObject?.Name ?? "Campus"} {group.Name} ({group.Id})<br>";
                    }

                    // Add checkin schedules to the next groups for this Baptism Type and Campus (if there are any next week)
                    foreach ( var service in campus.services ?? Enumerable.Empty<BaptismService>() )
                    {
                        string sql = @"
                            Select *
                            From [Group] g
                            Join [Group] pg on pg.Id = g.ParentGroupId
                            Where
                                g.IsActive = 1
                                and g.IsArchived = 0
                                and g.GroupTypeId = {0}
                                and g.CampusId = {1}
                                and pg.ParentGroupId = {2}
                                and Try_Convert(DateTime, Replace(g.[Name], ' - ', ' ')) > {3}
                                and Try_Convert(DateTime, Replace(g.[Name], ' - ', ' ')) < {4}
                                and g.Name like '%' + {5}";
                        var group = groupService.ExecuteQuery( sql,
                            baptismType.groupTypeId,
                            campus.campusId,
                            baptismType.parentGroupId,
                            DateTime.Now,
                            DateTime.Now.AddDays( 7 ),
                            service.time ).FirstOrDefault();

                        if ( group != null )
                        {
                            var location = new GroupLocation
                            {
                                GroupId = group.Id,
                                LocationId = service.locationId,
                                GroupLocationTypeValueId = 209,
                                Order = 0,
                                Schedules = new List<Schedule> { scheduleService.Get( service.scheduleId ) }
                            };
                            groupLocationService.Add( location );

                            addedOutput += $"Added GroupLocation to: {campusObject?.Name ?? "Campus"} {group.Name} ({group.Id})<br>";
                        }
                    }
                }

                if ( deletedOutput != "" || addedOutput != "" )
                {
                    output += $"<strong>{baptismTypeObject?.Name ?? "Baptism Type"}</strong><hr>{deletedOutput}<br>{addedOutput}<br>";
                }
                else
                {
                    output += $"<strong>{baptismTypeObject?.Name ?? "Baptism Type"}</strong><hr><em>No changes were made.</em><br><br>";
                }
            }

            rockContext.SaveChanges();

            context.Result = output;
        }

        /// <summary>
        /// The configuration for a service. A service will be something like 6:00pm, 9:30am, 11:00am, etc.
        /// </summary>
        class BaptismService
        {
            /// <summary>
            /// A string representing the service time in <c>h:mmTT</c> format (6:00PM, 9:00AM, 11:30AM, etc.). This will be checked against the group name of the child groups that people are able to check in to.
            /// </summary>
            public string time { get; set; }

            /// <summary>
            /// The ID of the location that should be used on the Meeting Details (<c>GroupLocation</c>) to allow for checking into this group.
            /// </summary>
            public int locationId { get; set; }

            /// <summary>
            /// The ID of the schedule that should be used on the Meeting Details (<c>GroupLocation</c>) to allow for checking into this group.
            /// </summary>
            public int scheduleId { get; set; }
        }

        /// <summary>
        /// The configuration for a campus. A campus will be something like Rockwall, North Dallas, Forney, etc.
        /// </summary>
        class BaptismCampus
        {
            /// <summary>
            /// The ID of the campus that this configuration is for. This will be checked against the campus of the child groups that people are able to check in to.
            /// </summary>
            public int campusId { get; set; }

            /// <summary>
            /// A list of the configurations for all services that have configurations for this campus and baptism type.
            /// </summary>
            public List<BaptismService> services { get; set; }
        }

        /// <summary>
        /// The configuration for a baptism type. A baptism type will be something like Weekend Services, Baptism Weekend, Special Event, etc.
        /// </summary>
        class BaptismType
        {
            /// <summary>
            /// The ID of the parent group for this baptism type that contains all of the campus parent groups. These groups will be the Weekend Services, Baptism Weekend, Special Event, etc. groups.
            /// </summary>
            public int parentGroupId { get; set; }

            /// <summary>
            /// The ID of the group type used by all of the child groups that people are able to check in to.
            /// </summary>
            public int groupTypeId { get; set; }

            /// <summary>
            /// A list of the configurations for all campuses that have configurations for this baptism type.
            /// </summary>
            public List<BaptismCampus> campuses { get; set; }
        }

        /// <summary>
        /// Contains the entire baptism configuration. This is the top-level object in the configuration.
        /// </summary>
        class BaptismConfig
        {
            /// <summary>
            /// A list of the configurations for all baptism types that have configurations.
            /// </summary>
            public List<BaptismType> baptismTypes { get; set; }
        }
    }

}
