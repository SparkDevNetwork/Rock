// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Utility;

namespace Rock.AI.Agent.Skills;

internal sealed partial class PersonSkill
{
    #region Tool(s)

    [Description( "Lists personal devices for the provided person." )]
    [AgentToolGuid( "29B7A989-59C4-4956-9C45-1D1297D3E673" )]
    public AgentToolResult ListPersonalDevicesForPerson( string personIdKey )
    {
        var personId = IdHasher.Instance.GetId( personIdKey );

        if ( !personId.HasValue )
        {
            return Error( "The personIdKey is required." )
                .WithInstructions( "You can call SearchPerson to find the corresponding key." );
        }

        var person = new PersonService( AgentRequestContext.RockContext ).Get( personId.Value );

        if ( person == null )
        {
            return Error( "No person could be found with the provided personIdKey." );
        }

        var personalDeviceService = new PersonalDeviceService( AgentRequestContext.RockContext );

        var devices = personalDeviceService.Queryable()
            .AsNoTracking()
            .Where( pd => pd.PersonAliasId == person.PrimaryAliasId );

        if ( !devices.Any() )
        {
            return NoData();
        }

        var results = devices
            .AsEnumerable()
            .Select( pd => new PersonalDeviceResult
            {
                Id = pd.Id,
                Guid = pd.Guid,
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

        return Success( results )
            .WithHistoryContent( historyContent, $"{personIdKey}-devices" );
    }

    #endregion
}
