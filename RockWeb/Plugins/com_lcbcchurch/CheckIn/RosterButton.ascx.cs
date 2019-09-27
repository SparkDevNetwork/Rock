// <copyright>
// Copyright by LCBC Church
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
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using RockWeb;


namespace RockWeb.Plugins.com_lcbcchurch.CheckIn
{
    [DisplayName( "Roster Button" )]
    [Category( "com_lcbcchurch > Check-in" )]
    [Description( "Displays a button to print rosters for location" )]
    [LinkedPage( "Roster Page" )]
    public partial class RosterButton : CheckInBlockMultiPerson
    {
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
        }
    
        protected void btnRoster_Click( object sender, EventArgs e )
        {
            var queryParams = new Dictionary<string, string>();

            if ( CurrentCheckInState != null )
            {
                var rockContext = new RockContext();
                //only gets the first campus it finds from the first location it pulls in kiosk config
                var location = new DeviceService( rockContext ).Get( CurrentCheckInState.Kiosk.Device.Guid ).Locations.FirstOrDefault();
                if ( location != null )
                {
                    string locationIdList = "";
                    if ( this.CurrentKioskId.HasValue )
                    {
                        var groupTypesLocations = this.GetGroupTypesLocations( rockContext );
                        locationIdList = groupTypesLocations
                            .Select( a => a.Id )
                            .ToList().AsDelimited( "," );
                    }

                    List<int> scheduleIdList = new List<int>();
                    if ( CurrentCheckInState != null && CurrentCheckInState.Kiosk != null )
                    {
                        foreach ( var groupType in CurrentCheckInState.Kiosk.KioskGroupTypes )
                        {
                            if ( CurrentCheckInState.ConfiguredGroupTypes.Contains( groupType.GroupType.Id ))
                            {
                                foreach ( var group in groupType.KioskGroups )
                                {
                                    foreach ( var kioskLocation in group.KioskLocations )
                                    {
                                        foreach ( var schedule in kioskLocation.KioskSchedules )
                                        {
                                            if ( schedule.Schedule.WasScheduleActive( RockDateTime.Now ) )
                                            {
                                                scheduleIdList.Add( schedule.Schedule.Id );
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    queryParams.Add( "CampusId", location.CampusId.ToString() );
                    queryParams.Add( "LocationIds", locationIdList );
                    queryParams.Add( "ScheduleIds", scheduleIdList.Distinct().ToList().AsDelimited( "," ) );
                }
            }

            NavigateToLinkedPage( "RosterPage", queryParams );
        }
    }
}