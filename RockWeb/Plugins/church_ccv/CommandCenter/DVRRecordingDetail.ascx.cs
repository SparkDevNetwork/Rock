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
using System.Linq;
using System.Web.UI.HtmlControls;

using Rock;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using church.ccv.CommandCenter.Model;
using church.ccv.CommandCenter.Data;

namespace RockWeb.Plugins.church_ccv.CommandCenter
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

            // Using the newest version of flowplayer and plugins (as of 9/1/2014) for this block since it works best
            // for DVR playback.  The older versions would struggle to play video when a different recording was
            // selected.
            RockPage.AddScriptLink( "~/Plugins/church_ccv/CommandCenter/Scripts/flowplayer-3.2.13.min.js" );
            RockPage.AddCSSLink( "~/Plugins/church_ccv/CommandCenter/Styles/commandcenter.css" );
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

                // Using an uri to remove the port number from the url
                var url = new Uri( Request.Url.OriginalString.Split('?')[0] );
                hfBaseUrl.Value = url.GetComponents(UriComponents.AbsoluteUri & ~UriComponents.Port, UriFormat.UriEscaped);

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
            var recipients = new List<RecipientData>();
            var orgFrom = GlobalAttributesCache.Read().GetValue( "OrganizationEmail" );

            Guid? commandCenterEmailTemplateGuid = new Guid( "1F88430F-D1B6-4819-BFC1-57E13B4B2098" );
                
            if ( commandCenterEmailTemplateGuid.HasValue )
            {
                var mergeObjects = GlobalAttributesCache.GetMergeFields( null );
                mergeObjects.Add( "Person", this.CurrentPerson );
                mergeObjects.Add( "RecordingUrl", tbLink.Text );
                mergeObjects.Add( "RecordingMessage", tbEmailMessage.Text );

                recipients.Add( new RecipientData( tbEmailTo.Text, mergeObjects ) );
                Email.Send( commandCenterEmailTemplateGuid.Value, recipients, ResolveRockUrl( "~/" ), ResolveRockUrl( "~~/" ) );
            }
        }

        protected void btnSendEmail_Click( object sender, EventArgs e )
        {
            SendMessage();
        }

        #endregion
    }
}