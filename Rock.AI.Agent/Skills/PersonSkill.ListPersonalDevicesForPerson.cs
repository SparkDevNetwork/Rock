using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Utility;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class PersonSkill
    {
        #region Tool(s)

        /// <summary>
        /// Retrieves a list of personal devices associated with a specific person.
        /// </summary>
        /// <param name="personIdKey">The person.</param>
        /// <returns></returns>
        [Description( "Lists personal devices for the provided person." )]
        [AgentToolGuid( "29B7A989-59C4-4956-9C45-1D1297D3E673" )]
        public RockToolResult ListPersonalDevicesForPerson( string personIdKey )
        {
            var personId = IdHasher.Instance.GetId( personIdKey );

            if ( !personId.HasValue )
            {
                return RockToolResult.Error( "The personIdKey is required." )
                    .WithInstructions( "You can call SearchPerson to find the corresponding key." );
            }

            var person = new PersonService( AgentRequestContext.RockContext ).Get( personId.Value );

            if ( person == null )
            {
                return RockToolResult.Error( "No person could be found with the provided personIdKey." );
            }

            var personalDeviceService = new PersonalDeviceService( AgentRequestContext.RockContext );

            var devices = personalDeviceService.Queryable()
                .AsNoTracking()
                .Where( pd => pd.PersonAliasId == person.PrimaryAliasId );

            if ( !devices.Any() )
            {
                return RockToolResult.NoData();
            }

            var results = devices
                .AsEnumerable()
                .Select( pd => new PersonalDeviceResult
                {
                    Id = pd.Id,
                    CreatedDateTime = pd.CreatedDateTime,
                    Name = pd.Name,
                    IsBeaconMonitoringEnabled = pd.IsBeaconMonitoringEnabled,
                    LastSeenDateTime = pd.LastSeenDateTime,
                    Manufacturer = pd.Manufacturer,
                    Model = pd.Model,
                    NotificationsEnabled = pd.NotificationsEnabled,
                    LocationPermissionStatusValue = pd.LocationPermissionStatus,
                    IsPreciseLocationEnabled = pd.IsPreciseLocationEnabled,
                    LocationPermissionDisabledDateTime = pd.LocationPermissionDisabledDateTime,
                    PersonalDeviceTypeValueId = pd.PersonalDeviceTypeValueId,
                    PlatformValueId = pd.PlatformValueId
                } )
                .ToList();

            var historyContent = results.Select(
                d => new KeyNameResult
                {
                    IdKey = d.IdKey,
                    Name = d.Name
                }
            );

            return RockToolResult.Success( results )
                .WithHistoryContent( historyContent, $"{personIdKey}-devices" );
        }

        #endregion
    }
}
