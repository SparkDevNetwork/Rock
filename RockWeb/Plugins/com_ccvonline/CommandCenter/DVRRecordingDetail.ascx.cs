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

            if ( !Page.IsPostBack )
            {
                string weekendDate = PageParameter( "WeekendDate" );
                string campusGuid = PageParameter( "CampusGuid" );
                string venueType = PageParameter( "VenueType" );

                if ( !string.IsNullOrWhiteSpace( weekendDate ) ||
                    !string.IsNullOrWhiteSpace( campusGuid ) ||
                    !string.IsNullOrWhiteSpace( venueType ) )
                    ShowDetail( weekendDate, campusGuid, venueType );
                else
                    pnlDetails.Visible = false;

            }
        }

        #endregion

        #region Internal Methods

        public void ShowDetail( string weekendValue, string campusGuidValue, string venueTypevalue )
        {
            pnlDetails.Visible = true;

            DateTime weekendDateTime = DateTime.Parse( weekendValue );

            RecordingService service = new RecordingService( new CommandCenterContext() );

            var queryable = service.Queryable()
                                   .Select( g =>
                                       new {
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
                                                ( g.CampusGuid.ToString() == campusGuidValue ) &&
                                                ( g.VenueType == venueTypevalue ) )
                                   .ToList();

            string[] date = queryable.Select( s => s.WeekendDate.ToString() ).FirstOrDefault().Split( ' ' );

            string weekendDate = date[0];

            lblTitle.Text = "Weekend of " + weekendDate;

            lblCampus.Text = queryable.Select( c => c.Campus.Name ).FirstOrDefault();
            lblVenueType.Text = queryable.Select( v => v.VenueType ).FirstOrDefault();

            var campusServiceTimeLists = queryable.Select( r => new { r.StartTime, r.RecordingDayAndTime, r.RecordingName } ).OrderBy( r => r.StartTime ).ToList();

            foreach ( var campusServiceTimeList in campusServiceTimeLists )
            {
                string[] dayAndTime = campusServiceTimeList.RecordingDayAndTime.Split( ' ' );
                string time = dayAndTime[1];

                LinkButton button = new LinkButton();
                button.Text = time;
                button.ID = time;
                button.Click += Button_Click;

                button.Attributes.Add( "class", "btn btn-primary" );
                plcServiceTimeButtons.Controls.Add( button );
            }

        }

        protected void Button_Click( object sender, EventArgs e )
        {         
            LinkButton lb = sender as LinkButton;
        }

        #endregion
    }
}