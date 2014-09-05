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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
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

        Campus _campus;

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Plugins/com_ccvonline/CommandCenter/Assets/flowplayer-3.2.18.swf" );
            RockPage.AddScriptLink( "~/Plugins/com_ccvonline/CommandCenter/Assets/flowplayer.controls-3.2.16.swf" );
            RockPage.AddScriptLink( "~/Plugins/com_ccvonline/CommandCenter/Assets/flowplayer.rtmp-3.2.13.swf" );
            RockPage.AddScriptLink( "~/Plugins/com_ccvonline/CommandCenter/Assets/flowplayer.f4m-3.2.10.swf" );
            RockPage.AddScriptLink( "~/Plugins/com_ccvonline/CommandCenter/Scripts/flowplayer-3.2.13.min.js" );
            RockPage.AddCSSLink( "~/Plugins/com_ccvonline/CommandCenter/Styles/commandcenter.css" );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                // Get page parameters
                DateTime? weekendDateTime = PageParameter( "WeekendDate" ).AsDateTime();
                Guid? campusGuid = PageParameter( "CampusGuid" ).AsGuidOrNull();
                string venue = PageParameter( "Venue" );
                string clipUrl = PageParameter( "ClipURL" );
                string clipStart = PageParameter( "ClipStart" );
                string clipDuration = PageParameter( "ClipDuration" );

                hfBaseUrl.Value = Request.Url.OriginalString.Split('?')[0];

                RecordingService service = new RecordingService( new CommandCenterContext() );
                var rockContext = new RockContext();

                if ( campusGuid != null )
                {
                    _campus = new CampusService( rockContext ).Get( campusGuid.Value );
                }

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
                                               Venue = g.Venue,
                                               StartTime = g.StartTime
                                           } );                                                                    
            
                if ( !String.IsNullOrWhiteSpace( clipUrl ) )
                {
                    campusVenueWeekendTimeList = campusVenueWeekendTimeList.Where( g => g.RecordingName == clipUrl );

                    if ( !String.IsNullOrWhiteSpace( clipStart ) )
                    {
                        hfClipStart.Value = clipStart;
                    }

                    if ( !String.IsNullOrWhiteSpace( clipDuration ) )
                    {
                        hfClipDuration.Value = clipDuration;
                    }

                    pnlVideo.Visible = true;
                    pnlControls.Visible = false;
                    pnlShare.Visible = false;
                }
                else if ( weekendDateTime.HasValue || campusGuid.HasValue || !string.IsNullOrWhiteSpace( venue ) )
                {
                    campusVenueWeekendTimeList = campusVenueWeekendTimeList.Where( g => ( g.WeekendDate == weekendDateTime ) &&
                                                ( g.Campus.Guid == campusGuid ) &&
                                                ( g.Venue == venue ) );

                    pnlVideo.Visible = true;
                    pnlControls.Visible = true;
                    pnlShare.Visible = true;
                }
                else 
                {
                    mdWarning.Visible = true;             
                    pnlVideo.Visible = false;
                    pnlControls.Visible = false;
                    pnlShare.Visible = false;
                }

                campusVenueWeekendTimeList = campusVenueWeekendTimeList.OrderBy( a => a.StartTime );
                campusVenueWeekendTimeList.ToList();

                if ( campusVenueWeekendTimeList.Any() )
                {

                    lblTitle.Text = "Weekend of " + campusVenueWeekendTimeList.FirstOrDefault().WeekendDate.Value.ToShortDateString();
                    lblCampus.Text = campusVenueWeekendTimeList.FirstOrDefault().Campus.ToString();
                    lblVenue.Text = campusVenueWeekendTimeList.FirstOrDefault().Venue.ToString();

                    // set the recording to the first recording that we'll show
                    hfRecording.Value = campusVenueWeekendTimeList.FirstOrDefault().RecordingName;

                    // creating the service time buttons
                    foreach ( var campusServiceTimeList in campusVenueWeekendTimeList )
                    {
                        HtmlAnchor button = new HtmlAnchor();
                        button.InnerText = campusServiceTimeList.RecordingDayAndTime.Split( ' ' )[1];
                        button.ID = string.Format( "btnRecording_{0}", Guid.NewGuid().ToString( "n" ) );
                        button.Attributes["onclick"] = "javascript: ChangeRecording( " + campusServiceTimeList.RecordingName.Quoted( "'" ) + " );";
                        button.Attributes["recordingName"] = campusServiceTimeList.RecordingName;
                        button.Attributes.Add( "data-toggle", "tooltip" );
                        button.Attributes.Add( "data-placement", "top" );
                        button.Attributes.Add( "Type", "button" );
                        button.Title = campusServiceTimeList.RecordingName;
                        button.Attributes.Add( "class", "btn btn-default servicebutton" );
                        plcServiceTimeButtons.Controls.Add( button );
                    }
                }
                else
                {
                    mdWarning.Visible = true;
                    pnlVideo.Visible = false;
                    pnlShare.Visible = false;
                }
            }
        }

        #endregion
       
        #region Methods

        /// <summary>
        /// Sends an email.
        /// </summary>
        private void SendMessage()
        {
            var recipients = new List<string>();
            var orgFrom = GlobalAttributesCache.Read().GetValue( "OrganizationEmail" );

            string to = tbEmailTo.Text;
            if( !string.IsNullOrWhiteSpace( to ) )
            {
                recipients.Add( to );
            }

            if ( recipients.Any() )
            {
                var channelData = new Dictionary<string, string>();

                if ( !string.IsNullOrWhiteSpace( tbEmailFrom.Text ) )
                {
                    channelData.Add( "From", tbEmailFrom.Text );                   
                }
                else
                {
                    channelData.Add( "From", orgFrom );
                }

                channelData.Add( "Subject", "Command Center Recording" );

                string videoLink = "<a href='" + tbLink.Text + "'>Video Clip</a>";

                if ( !string.IsNullOrWhiteSpace( tbEmailMessage.Text ) )
                {
                    channelData.Add( "Body", "<html><body><p>" + tbEmailMessage.Text + "</p><p>"+ videoLink + "</p></body></html>" );
                }
                else
                {
                    channelData.Add( "Body", "<html><body><p>" + videoLink + "</p></body></html>" );
                }

                var channelEntity = EntityTypeCache.Read( Rock.SystemGuid.EntityType.COMMUNICATION_CHANNEL_EMAIL.AsGuid() );               
                if ( channelEntity != null )
                {
                    var channel = ChannelContainer.GetComponent( channelEntity.Name );
                    if ( channel != null && channel.IsActive )
                    {
                        var transport = channel.Transport;
                        if ( transport != null && transport.IsActive )
                        {
                            var appRoot = GlobalAttributesCache.Read().GetValue( "InternalApplicationRoot" );
                            transport.Send( channelData, recipients, appRoot, string.Empty );
                        }
                    }
                }
            }
        }

        protected void btnSendEmail_Click( object sender, EventArgs e )
        {
            SendMessage();
        }

        #endregion
    }
}