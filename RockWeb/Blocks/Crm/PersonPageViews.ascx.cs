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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using UAParser;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Person Page Views" )]
    [Category( "CRM" )]
    [Description( "Lists a persons web sessions with details." )]
    public partial class PersonPageViews : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods
        
        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

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

            ShowList();
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated(object sender, EventArgs e)
        {
            ShowList();
        }

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
                    switch ( session.ClientType )
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
                    Parser uaParser = Parser.GetDefault();
                    ClientInfo client = uaParser.Parse( session.UserAgent );

                    lClientIcon.Text = String.Format( "<div class='pageviewsession-client pull-right'><div class='pull-left'><small>{0}<br>{1}</small></div><i class='fa {2} fa-2x pull-right'></i></div>", 
                                            client.UserAgent, 
                                            client.OS,
                                            icon );
                    
                    var lSessionDuration = e.Item.FindControl( "lSessionDuration" ) as Literal;
                    TimeSpan duration = (DateTime)session.EndDateTime - (DateTime)session.StartDateTime;

                    if ( duration.Hours > 0 )
                    {
                        lSessionDuration.Text = String.Format( "{0}h {1}m", duration.Hours, duration.Minutes );
                    }
                    else
                    {
                        lSessionDuration.Text = String.Format( "{0}m", duration.Minutes );
                    }
                }
            }
        }

        protected void rptPageViews_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                int currentIndex = e.Item.ItemIndex;
                if ( currentIndex > 0 )
                {
                    var lPageViewDuration = e.Item.FindControl( "lPageViewDuration" ) as Literal;
                    var rptPageViews = sender as Repeater;
                    //lPageViewDuration.Text = rptPageViews.Items.Count.ToString();
                    //l.visible = e.item.dataitem("AccountID") <> s.items(e.item.itemindex-1).dataitem("AccountID")
                    //DateTime prevPageTime = (DateTime)rptPageViews.Items[currentIndex - 1].DataItem[""]
                    try
                    {
                        var item = rptPageViews.Items[currentIndex + 1];
                        lPageViewDuration.Text = "-" + (string)DataBinder.Eval( item.DataItem, "SessionId" ); ;
                    }
                    catch ( Exception ex ) { }
                }
            }
        }

        #endregion

        #region Methods

        void ShowList() {

            string itemId = PageParameter( "personId" );
            if ( !string.IsNullOrWhiteSpace( itemId ) )
            {
                int personId = int.Parse( itemId );
                PageViewService pageviewService = new PageViewService();

                var pageViews = pageviewService.Queryable();
                
                var sessionInfo = pageviewService.Queryable()
                    .Where( s => s.PersonAlias.PersonId == personId )
                    .GroupBy( s => new { s.SessionId, s.SiteId, SiteName = s.Site.Name, s.ClientType, s.IpAddress, s.UserAgent })
                    .Select( s => new WebSession
                    {
                                    SessionId = s.Key.SessionId,
                                    StartDateTime = s.Min(x => x.DateTimeViewed),
                                    EndDateTime = s.Max(x => x.DateTimeViewed),
                                    SiteId = s.Key.SiteId,
                                    Site = s.Key.SiteName,
                                    ClientType = s.Key.ClientType,
                                    IpAddress = s.Key.IpAddress,
                                    UserAgent = s.Key.UserAgent,
                                    PageViews = pageViews.Where(p=> p.SessionId == s.Key.SessionId).ToList()
                    })
                    .OrderByDescending(p => p.StartDateTime)
                    .Take(20);

                rptSessions.DataSource = sessionInfo.ToList();
                rptSessions.DataBind();
            }
            else
            {
                lMessages.Text = "<div class='alert alert-warning'>No person provided to show results for.</div>";
            }
            
            
        }

        #endregion

        public class WebSession
        {
            public Guid? SessionId { get; set; }
            public DateTime? StartDateTime { get; set; }
            public DateTime? EndDateTime { get; set; }
            public int? SiteId { get; set; }
            public string Site { get; set; }
            public string ClientType { get; set; }
            public string IpAddress { get; set; }
            public string UserAgent { get; set; }
            public ICollection<PageView> PageViews { get; set; }
        }

}
}