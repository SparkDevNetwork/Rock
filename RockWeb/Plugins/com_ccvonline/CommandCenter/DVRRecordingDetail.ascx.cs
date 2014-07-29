// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using com.ccvonline.CommandCenter.Model;
using com.ccvonline.CommandCenter.Data;

namespace RockWeb.Plugins.com_ccvonline.CommandCenter
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "DVR Recording Detail" )]
    [Category( "CCV > Command Center" )]
    [Description( "Displays the details of a Command Center DVR recording." )]
    public partial class DVRRecordingDetail : RockBlock
    {
        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Plugins/com_ccvonline/CommandCenter/Assets/flowplayer-3.2.18.swf" );
            RockPage.AddScriptLink( "~/Plugins/com_ccvonline/CommandCenter/Assets/flowplayer.controls-3.2.16.swf" );
            RockPage.AddScriptLink( "~/Plugins/com_ccvonline/CommandCenter/Assets/flowplayer.rtmp-3.2.13.swf" );
            RockPage.AddScriptLink( "~/Plugins/com_ccvonline/CommandCenter/Assets/flowplayer.f4m-3.2.10.swf" );
            RockPage.AddScriptLink( "~/Plugins/com_ccvonline/CommandCenter/Scripts/flowplayer-3.2.13.min.js" );
            RockPage.AddCSSLink( "~/Plugins/com_ccvonline/CommandCenter/Styles/dvr.css" );
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            DateTime? weekendDateTime = PageParameter( "WeekendDate" ).AsDateTime();
            Guid? campusGuid = PageParameter( "CampusGuid" ).AsGuidOrNull();
            string venueType = PageParameter( "VenueType" );

            if ( !Page.IsPostBack )
            {
                if ( !weekendDateTime.HasValue || !campusGuid.HasValue || string.IsNullOrWhiteSpace( venueType ) )
                {
                    // Hide the details panel if we didn't get the page parameters we expected
                    pnlDetails.Visible = false;
                }
                else 
                {
                    pnlDetails.Visible = true;
                }

            }


            RecordingService service = new RecordingService( new CommandCenterContext() );            
            var rockContext = new RockContext();
            var campus = new CampusService( rockContext ).Get( campusGuid.Value );

            lblTitle.Text = "Weekend of " + weekendDateTime.Value.Date.ToShortDateString();
            lblCampus.Text = campus.Name;
            lblVenueType.Text = venueType;

            var campusVenueWeekendTimeList = service.Queryable()
                                   .Select( g =>
                                       new
                                       {
                                           WeekendDate = DbFunctions.TruncateTime( SqlFunctions.DateAdd( "day", 1 - SqlFunctions.DatePart( "dw", g.Date ) % 7, g.Date ) ),
                                           Campus = g.Campus,
                                           CampusId = g.CampusId,
                                           CampusGuid = g.Campus.Guid,
                                           RecordingDayAndTime = g.Label,
                                           RecordingName = g.RecordingName,
                                           VenueType = g.VenueType,
                                           StartTime = g.StartTime
                                       } )
                                   .Where( g => ( g.WeekendDate == weekendDateTime ) &&
                                                ( g.Campus.Guid == campusGuid ) &&
                                                ( g.VenueType == venueType ) )
                                   .OrderBy( a => a.StartTime ).ToList();


            if ( campusVenueWeekendTimeList.Any() )
            {
                // set the recording to the first recording that we'll show
                hfRecording.Value = campusVenueWeekendTimeList.FirstOrDefault().RecordingName;
            }

            // creating the service time buttons
            foreach ( var campusServiceTimeList in campusVenueWeekendTimeList )
            {
                HtmlAnchor button = new HtmlAnchor();
                button.InnerText = campusServiceTimeList.RecordingDayAndTime.Split( ' ' )[1];
                button.ID = string.Format( "btnRecording_{0}", Guid.NewGuid().ToString("n") );              
                button.Attributes["onclick"] = "javascript: ChangeRecording( " + campusServiceTimeList.RecordingName.Quoted("'") + " );";

                button.Attributes.Add("class", "btn btn-primary servicebutton");
                plcServiceTimeButtons.Controls.Add( button );
            }

        }

        #endregion

    }
}