﻿// <copyright>
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
using System.Text;
using System.Web;
using System.Data.Entity;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Displays a list of active users of a website.
    /// </summary>
    [DisplayName( "Active Users" )]
    [Category( "CMS" )]
    [Description( "Displays a list of active users of a website." )]
    [SiteField( "Site", "Site to show current active users for.", true )]
    [BooleanField( "Show Site Name As Title", "Detmine whether to show the name of the site as a title above the list.", true )]
    [BooleanField( "Show Guest Visitors", "Displays the number of guests visiting the site. (Guests are considered users not logged in.)", true )]
    [LinkedPage( "Person Profile Page", "Page reference to the person profile page you would like to use as a link. Not providing a reference will suppress the creation of a link.", false )]
    [IntegerField( "Page View Count", "The number of past page views to show on roll-over. A value of 0 will disable the roll-over.", true, 5 )]
    public partial class ActiveUsers : Rock.Web.UI.RockBlock
    {
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
            base.OnLoad( e );

            ShowActiveUsers();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the active users control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowActiveUsers();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the active users.
        /// </summary>
        private void ShowActiveUsers()
        {
            int? siteId = GetAttributeValue( "Site" ).AsIntegerOrNull();
            if ( !siteId.HasValue )
            {
                lMessages.Text = "<div class='alert alert-warning'>No site is currently configured.</div>";
                return;
            }
            else
            {
                int pageViewCount = GetAttributeValue( "PageViewCount" ).AsIntegerOrNull() ?? 0;

                StringBuilder sbUsers = new StringBuilder();

                var site = SiteCache.Read( siteId.Value );
                lSiteName.Text = "<h4>" + site.Name + "</h4>";
                lSiteName.Visible = GetAttributeValue( "ShowSiteNameAsTitle" ).AsBoolean();

                if ( !site.EnablePageViews )
                {
                    lMessages.Text = "<div class='alert alert-warning'>Active " + site.Name + " users not available because page views are not enabled for site.</div>";
                    return;
                }

                lMessages.Text = string.Empty;
                string guestVisitorsStr = string.Empty;

                using ( var rockContext = new RockContext() )
                {
                    var qryPageViews = new PageViewService( rockContext ).Queryable();
                    var qryPersonAlias = new PersonAliasService( rockContext ).Queryable();
                    var pageViewQry = qryPageViews.Join(
                        qryPersonAlias,
                        pv => pv.PersonAliasId,
                        pa => pa.Id,
                        ( pv, pa ) =>
                        new
                        {
                            PersonAliasPersonId = pa.PersonId,
                            pv.DateTimeViewed,
                            pv.SiteId,
                            pv.PageViewSessionId,
                            PagePageTitle = pv.PageTitle
                        } );

                    var last24Hours = RockDateTime.Now.AddDays( -1 );

                    int pageViewTakeCount = pageViewCount;
                    if ( pageViewTakeCount == 0 )
                    {
                        pageViewTakeCount = 1;
                    }

                    // Query to get who is logged in and last visit was to selected site
                    var activeLogins = new UserLoginService( rockContext ).Queryable()
                        .Where( l =>
                            l.PersonId.HasValue &&
                            l.IsOnLine == true )
                        .OrderByDescending( l => l.LastActivityDateTime )
                        .Select( l => new
                        {
                            login = new
                            {
                                l.UserName,
                                l.LastActivityDateTime,
                                l.PersonId,
                                Person = new
                                {
                                    l.Person.NickName,
                                    l.Person.LastName,
                                    l.Person.SuffixValueId
                                }
                            },
                            pageViews = pageViewQry
                                .Where( v => v.PersonAliasPersonId == l.PersonId )
                                .Where( v => v.DateTimeViewed > last24Hours )
                                .OrderByDescending( v => v.DateTimeViewed )
                                .Take( pageViewTakeCount )
                        } )
                        .Select( a => new
                        {
                            a.login,
                            pageViews = a.pageViews.ToList()
                        } );

                    if ( CurrentUser != null )
                    {
                        activeLogins = activeLogins.Where( m => m.login.UserName != CurrentUser.UserName );
                    }

                    foreach ( var activeLogin in activeLogins )
                    {
                        var login = activeLogin.login;

                        if ( !activeLogin.pageViews.Any() || activeLogin.pageViews.FirstOrDefault().SiteId != site.Id )
                        {
                            // only show active logins with PageViews and the most recent pageview is for the specified site
                            continue;
                        }

                        var latestPageViewSessionId = activeLogin.pageViews.FirstOrDefault().PageViewSessionId;

                        TimeSpan tsLastActivity = login.LastActivityDateTime.HasValue ? RockDateTime.Now.Subtract( login.LastActivityDateTime.Value ) : TimeSpan.MaxValue;
                        string className = tsLastActivity.Minutes <= 5 ? "recent" : "not-recent";

                        // create link to the person
                        string personFullName = Person.FormatFullName( login.Person.NickName, login.Person.LastName, login.Person.SuffixValueId );
                        string personLink = personFullName;

                        if ( GetAttributeValue( "PersonProfilePage" ) != null )
                        {
                            string personProfilePage = GetAttributeValue( "PersonProfilePage" );
                            var pageParams = new Dictionary<string, string>();
                            pageParams.Add( "PersonId", login.PersonId.ToString() );
                            var pageReference = new Rock.Web.PageReference( personProfilePage, pageParams );
                            personLink = string.Format( @"<a href='{0}'>{1}</a>", pageReference.BuildUrl(), personFullName );
                        }

                        // determine whether to show last page views
                        if ( GetAttributeValue( "PageViewCount" ).AsInteger() > 0 )
                        {
                            string activeLoginFormat = @"
<li class='active-user {0}' data-toggle='tooltip' data-placement='top' title='{2}'>
    <i class='fa-li fa fa-circle'></i> {1}
</li>";
                            // define the formatting for each user entry
                            if ( activeLogin.pageViews != null )
                            {
                                string pageViewsHtml = activeLogin.pageViews
                                                    .Where( v => v.PageViewSessionId == latestPageViewSessionId )
                                                    .Select( v => HttpUtility.HtmlEncode( v.PagePageTitle ) ).ToList().AsDelimited( "<br> " );

                                sbUsers.Append( string.Format( activeLoginFormat, className, personLink, pageViewsHtml ) );
                            }
                        }
                        else
                        {
                            string inactiveLoginFormat = @"
<li class='active-user {0}'>
    <i class='fa-li fa fa-circle'></i> {1}
</li>";
                            sbUsers.Append( string.Format( inactiveLoginFormat, className, personLink ) );
                        }
                    }

                    // get the 'show guests' attribute and if it's true, determine how many guests there are.
                    bool showGuestVisitors = GetAttributeValue( "ShowGuestVisitors" ).AsBoolean();
                    if ( showGuestVisitors )
                    {
                        // build a list of unique sessions views in the past 15 minutes.
                        // We'll only take entries with a null personAliasID, which means they're not logged in,
                        // and thus ARE guests.
                        var last5Minutes = RockDateTime.Now.AddMinutes( -5 );
                        var last15Minutes = RockDateTime.Now.AddMinutes( -15 );

                        var qryGuests = new PageViewService( rockContext ).Queryable().AsNoTracking()
                                          .Where( 
                                                p => p.SiteId == site.Id 
                                                && p.DateTimeViewed > last15Minutes 
                                                && p.PersonAliasId == null 
                                                && p.PageViewSession.PageViewUserAgent.Browser != "Other" 
                                                && p.PageViewSession.PageViewUserAgent.ClientType != "Crawler" )
                                          .GroupBy( p => p.PageViewSessionId )
                                          .Select( g => new
                                          {
                                              SessionId = g.Key,
                                              LastVisit = g.Max( p => p.DateTimeViewed )
                                          } )
                                          .ToList();

                        var numRecentGuests = qryGuests.Where( g => g.LastVisit >= last5Minutes ).Count();
                        var numInactiveGuests = qryGuests.Where( g => g.LastVisit < last5Minutes ).Count();

                        // now build the formatted entry, which is "Current Guests (0) (1)" where the first is a green badge, and the second yellow.
                        if ( numRecentGuests > 0 || numInactiveGuests > 0 )
                        {
                            guestVisitorsStr = "Current Guests:";
                            if ( numRecentGuests > 0 )
                            {
                                guestVisitorsStr += string.Format( " <span class=\"badge badge-success\">{0}</span>", numRecentGuests );
                            }

                            if ( numInactiveGuests > 0 )
                            {
                                guestVisitorsStr += string.Format( " <span class=\"badge badge-warning\">{0}</span>", numInactiveGuests );
                            }
                        }
                    }
                }

                if ( sbUsers.Length > 0 )
                {
                    lUsers.Text = string.Format( @"<ul class='activeusers fa-ul'>{0}</ul>", sbUsers.ToString() );
                    lUsers.Text += string.Format( @"<p class='margin-l-sm'>{0}</p>", guestVisitorsStr );
                }
                else
                {
                    lMessages.Text = string.Format( "There are no logged in users on the {0} site.", site.Name );
                    lMessages.Text += "<br /><br />" + guestVisitorsStr;
                }
            }
        }

        #endregion
    }
}