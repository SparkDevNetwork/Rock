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
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Crm
{
    [DisplayName( "Person Page Views" )]
    [Category( "CRM" )]
    [Description( "Lists a persons web sessions with details." )]

    [IntegerField(
        "Session Count",
        Key = AttributeKey.SessionCount,
        Description = "The number of sessions to show per page.",
        IsRequired = true,
        DefaultIntegerValue = 20,
        Order = 0 )]

    [Rock.SystemGuid.BlockTypeGuid( "877156AE-8D61-4BD9-8E77-0A7FAD9AEACD" )]
    public partial class PersonPageViews : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys
        private static class AttributeKey
        {
            public const string SessionCount = "SessionCount";
        }
        #endregion Attribute Keys

        #region Fields

        private DateTime startDate = DateTime.MinValue;
        private DateTime endDate = DateTime.MaxValue;
        private int pageNumber = 0;
        private int siteId = -1;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                if ( !string.IsNullOrWhiteSpace( PageParameter( "StartDate" ) ) )
                {
                    startDate = PageParameter( "StartDate" ).AsDateTime() ?? DateTime.MinValue;
                    if ( startDate != DateTime.MinValue )
                    {
                        drpDateFilter.LowerValue = startDate;
                    }
                }

                if ( !string.IsNullOrWhiteSpace( PageParameter( "EndDate" ) ) )
                {
                    endDate = PageParameter( "EndDate" ).AsDateTime() ?? DateTime.MaxValue;
                    if ( endDate != DateTime.MaxValue )
                    {
                        drpDateFilter.UpperValue = endDate;
                    }
                }

                if ( !string.IsNullOrEmpty( PageParameter( "Page" ) ) )
                {
                    pageNumber = PageParameter( "Page" ).AsInteger();
                }

                if ( !string.IsNullOrEmpty( PageParameter( "SiteId" ) ) )
                {
                    siteId = PageParameter( "SiteId" ).AsInteger();
                }

                ShowList();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowList();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptSessions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptSessions_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var session = e.Item.DataItem as WebSession;
                if ( session != null )
                {
                    var lRelativeDate = e.Item.FindControl( "lRelativeDate" ) as Literal;
                    lRelativeDate.Text = session.StartDateTime.Value.ToRelativeDateString( 6 );

                    var lClientIcon = e.Item.FindControl( "lClientIcon" ) as Literal;
                    string icon = string.Empty;
                    switch ( session.PageViewSession.DeviceType.ClientType )
                    {
                        case "Desktop":
                            icon = "fa-desktop";
                            break;
                        case "Tablet":
                            icon = "fa-tablet";
                            break;
                        case "Mobile":
                            icon = "fa-mobile-phone";
                            break;
                    }

                    var lUserAgent = e.Item.FindControl( "lUserAgent" ) as Literal;

                    lClientIcon.Text = string.Format(
                        "<div class='pageviewsession-client pull-right'><div class='pull-left margin-r-sm'><small>{0}<br>{1}</small></div><i class='fa {2} fa-2x pull-right'></i></div>",
                        session.PageViewSession.DeviceType.Application,
                        session.PageViewSession.DeviceType.OperatingSystem,
                        icon );

                    var lSessionDuration = e.Item.FindControl( "lSessionDuration" ) as Literal;
                    TimeSpan duration = ( DateTime ) session.EndDateTime - ( DateTime ) session.StartDateTime;

                    if ( duration.Hours > 0 )
                    {
                        lSessionDuration.Text = string.Format( "{0}h {1}m", duration.Hours, duration.Minutes );
                    }
                    else if (duration.Minutes == 0 )
                    {
                        lSessionDuration.Text = " < 1m";
                    }
                    else
                    {
                        lSessionDuration.Text = string.Format( "{0}m", duration.Minutes );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnFilter_Click( object sender, EventArgs e )
        {
            if ( drpDateFilter.LowerValue.HasValue )
            {
                startDate = drpDateFilter.LowerValue.Value;
            }

            if ( drpDateFilter.UpperValue.HasValue )
            {
                endDate = drpDateFilter.UpperValue.Value;
            }

            pageNumber = 0;

            ShowList();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the list.
        /// </summary>
        public void ShowList()
        {
            var rockContext = new RockContext();

            int sessionCount = GetAttributeValue( AttributeKey.SessionCount ).AsInteger();

            int skipCount = pageNumber * sessionCount;

            Person person = null;
            Guid? personGuid = PageParameter( "PersonGuid" ).AsGuidOrNull();

            // NOTE: Since this block shows a history of sites a person visited in Rock, require Person.Guid instead of Person.Id to reduce the risk of somebody manually editing the URL to see somebody else pageview history
            if ( personGuid.HasValue )
            {
                person = new PersonService( rockContext ).Get( personGuid.Value );
            }
            else if ( !string.IsNullOrEmpty( PageParameter( "Person" ) ) )
            {
                // Just in case Person (Person Token) was used, look up by Impersonation Token
                person = new PersonService( rockContext ).GetByImpersonationToken( PageParameter( "Person" ), false, this.PageCache.Id );
            }

            if ( person != null )
            {
                lPersonName.Text = person.FullName;

                InteractionService interactionService = new InteractionService( rockContext );

                var pageViews = interactionService.Queryable();

                var sessionInfo = interactionService.Queryable()
                    .Where( s => s.PersonAlias.PersonId == person.Id );

                if ( startDate != DateTime.MinValue )
                {
                    sessionInfo = sessionInfo.Where( s => s.InteractionDateTime > drpDateFilter.LowerValue );
                }

                if ( endDate != DateTime.MaxValue )
                {
                    sessionInfo = sessionInfo.Where( s => s.InteractionDateTime < drpDateFilter.UpperValue );
                }

                if ( siteId != -1 )
                {
                    var site = SiteCache.Get( siteId );

                    string siteName = string.Empty;
                    if (site != null )
                    {
                        siteName = site.Name;
                    }
                    // lookup the interactionDeviceType, and create it if it doesn't exist
                    int channelMediumValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE.AsGuid() ).Id;

                    var interactionChannelId = new InteractionChannelService( rockContext ).Queryable()
                                                        .Where( a => a.ChannelTypeMediumValueId == channelMediumValueId && a.ChannelEntityId == siteId )
                                                        .Select( a => a.Id)
                                                        .FirstOrDefault();

                    sessionInfo = sessionInfo.Where( p => p.InteractionComponent.InteractionChannelId == interactionChannelId );
                }

                var pageviewInfo = sessionInfo.GroupBy( s => new
                {
                    s.InteractionSession,
                    s.InteractionComponent.InteractionChannel,
                } )
                                .Select( s => new WebSession
                                {
                                    PageViewSession = s.Key.InteractionSession,
                                    StartDateTime = s.Min( x => x.InteractionDateTime ),
                                    EndDateTime = s.Max( x => x.InteractionDateTime ),
                                    SiteId = siteId,
                                    Site = s.Key.InteractionChannel.Name,
                                    PageViews = pageViews.Where( p => p.InteractionSessionId == s.Key.InteractionSession.Id && p.InteractionComponent.InteractionChannelId == s.Key.InteractionChannel.Id ).OrderBy( p => p.InteractionDateTime ).ToList()
                                } );

                pageviewInfo = pageviewInfo.OrderByDescending( p => p.StartDateTime )
                                .Skip( skipCount )
                                .Take( sessionCount + 1 );

                rptSessions.DataSource = pageviewInfo.ToList().Take( sessionCount );
                rptSessions.DataBind();

                // set next button
                if ( pageviewInfo.Count() > sessionCount )
                {
                    hlNext.Visible = hlNext.Enabled = true;
                    Dictionary<string, string> queryStringNext = new Dictionary<string, string>();
                    queryStringNext.Add( "Page", ( pageNumber + 1 ).ToString() );
                    queryStringNext.Add( "Person", person.UrlEncodedKey );
                    if ( siteId != -1 )
                    {
                        queryStringNext.Add( "SiteId", siteId.ToString() );
                    }

                    if ( startDate != DateTime.MinValue )
                    {
                        queryStringNext.Add( "StartDate", startDate.ToShortDateString() );
                    }

                    if ( endDate != DateTime.MaxValue )
                    {
                        queryStringNext.Add( "EndDate", endDate.ToShortDateString() );
                    }

                    var pageReferenceNext = new Rock.Web.PageReference( CurrentPageReference.PageId, CurrentPageReference.RouteId, queryStringNext );
                    hlNext.NavigateUrl = pageReferenceNext.BuildUrl();
                }
                else
                {
                    hlNext.Visible = hlNext.Enabled = false;
                }

                // set prev button
                if ( pageNumber == 0 )
                {
                    hlPrev.Visible = hlPrev.Enabled = false;
                }
                else
                {
                    hlPrev.Visible = hlPrev.Enabled = true;
                    Dictionary<string, string> queryStringPrev = new Dictionary<string, string>();
                    queryStringPrev.Add( "Page", ( pageNumber - 1 ).ToString() );
                    queryStringPrev.Add( "Person", person.UrlEncodedKey );
                    if ( siteId != -1 )
                    {
                        queryStringPrev.Add( "SiteId", siteId.ToString() );
                    }

                    if ( startDate != DateTime.MinValue )
                    {
                        queryStringPrev.Add( "StartDate", startDate.ToShortDateString() );
                    }

                    if ( endDate != DateTime.MaxValue )
                    {
                        queryStringPrev.Add( "EndDate", endDate.ToShortDateString() );
                    }

                    var pageReferencePrev = new Rock.Web.PageReference( CurrentPageReference.PageId, CurrentPageReference.RouteId, queryStringPrev );
                    hlPrev.NavigateUrl = pageReferencePrev.BuildUrl();
                }
            }
            else
            {
                lMessages.Text = "<div class='alert alert-warning'>No person provided to show results for.</div>";
            }
        }

        #endregion

        /// <summary>
        /// Special class just for this block
        /// </summary>
        public class WebSession
        {
            /// <summary>
            /// Gets or sets the page view session.
            /// </summary>
            /// <value>
            /// The page view session.
            /// </value>
            public InteractionSession PageViewSession { get; set; }

            /// <summary>
            /// Gets or sets the start date time.
            /// </summary>
            /// <value>
            /// The start date time.
            /// </value>
            public DateTime? StartDateTime { get; set; }

            /// <summary>
            /// Gets or sets the end date time.
            /// </summary>
            /// <value>
            /// The end date time.
            /// </value>
            public DateTime? EndDateTime { get; set; }

            /// <summary>
            /// Gets or sets the site identifier.
            /// </summary>
            /// <value>
            /// The site identifier.
            /// </value>
            public int? SiteId { get; set; }

            /// <summary>
            /// Gets or sets the site.
            /// </summary>
            /// <value>
            /// The site.
            /// </value>
            public string Site { get; set; }

            /// <summary>
            /// Gets or sets the page views.
            /// </summary>
            /// <value>
            /// The page views.
            /// </value>
            public ICollection<Interaction> PageViews { get; set; }
        }
    }
}