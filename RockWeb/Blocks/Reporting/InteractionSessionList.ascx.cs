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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// List all the Interaction Session.
    /// </summary>
    [DisplayName( "Interaction Session List" )]
    [Category( "Reporting" )]
    [Description( "List all the Interaction Session" )]

    [LinkedPage( "Component Detail Page", "Page reference to the component detail page. This will be included as a variable in the Lava.", false, order: 0 )]
    [LinkedPage( "Interaction Detail Page", "Page reference to the interaction detail page. This will be included as a variable in the Lava.", false, order: 1 )]
    [CodeEditorField( "Default Template", "The Lava template to use as default.", Rock.Web.UI.Controls.CodeEditorMode.Lava, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 300, false, order: 2, defaultValue: @"
{% if InteractionChannel != null and InteractionChannel != '' %}
    {% for session in WebSessions %}
        <div class='panel panel-widget pageviewsession'>
	        <header class='panel-heading'>
	        <div class='pull-left'>
		        <h4 class='panel-title'>
		            {{ session.PersonAlias.Person.FullName }}
			        <small class='d-block d-sm-inline mt-1 mb-2 my-sm-0'>
			            Started {{ session.StartDateTime }} /
			            Duration: {{ session.StartDateTime | HumanizeTimeSpan:session.EndDateTime, 1 }}
			        </small>
		        </h4>
		        <span class='label label-primary'></span>
		        <span class='label label-info'>{{ InteractionChannel.Name }}</span>
		        </div>
		        {% assign icon = '' %}
		        {% case session.InteractionSession.DeviceType.ClientType %}
			        {% when 'Desktop' %}{% assign icon = 'fa-desktop' %}
			        {% when 'Tablet' %}{% assign icon = 'fa-tablet' %}
			        {% when 'Mobile' %}{% assign icon = 'fa-mobile-phone' %}
			        {% else %}{% assign icon = '' %}
		        {% endcase %}
		        {% if icon != '' %}
    		        <div class='pageviewsession-client d-flex align-items-center ml-2 ml-sm-auto'>
                        <div class='pull-left'>
                            <small>{{ session.InteractionSession.DeviceType.Application }} <br>
                            {{ session.InteractionSession.DeviceType.OperatingSystem }} </small>
                        </div>
                        <i class='fa {{ icon }} fa-2x pull-left d-none d-sm-block margin-l-sm'></i>
                    </div>
                {% endif %}
	        </header>
	        <div class='panel-body'>
		        <ol>
		        {% for interaction in session.Interactions %}
    			    <li><a href='{{ interaction.InteractionData }}'>{{ interaction.InteractionSummary }}</a></li>
		        {% endfor %}
		        </ol>
	        </div>
        </div>
    {% endfor %}
{% endif %}" )]
    [IntegerField( "Session Count", "The number of sessions to show per page.", true, 20, "", 3 )]
    [ContextAware( typeof( Person ) )]
    [Rock.SystemGuid.BlockTypeGuid( "EA90EF4F-C783-48CD-B575-AD785DE896E9" )]
    public partial class InteractionSessionList : Rock.Web.UI.RockBlock
    {
        #region Fields

        private int? _channelId = null;
        private DateTime startDate = DateTime.MinValue;
        private DateTime endDate = DateTime.MaxValue;
        private int pageNumber = 0;
        private int? _personId = null;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _channelId = PageParameter( "ChannelId" ).AsIntegerOrNull();
            if ( !_channelId.HasValue )
            {
                upnlContent.Visible = false;
            }
            else
            {
                upnlContent.Visible = true;
            }

            _personId = GetPersonId();
            ppPerson.Visible = !_personId.HasValue;

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
                if ( _channelId.HasValue )
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

                    ShowList();
                }
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            _personId = GetPersonId();
            ppPerson.Visible = !_personId.HasValue;
            ShowList();
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

            if ( ppPerson.PersonId.HasValue && ppPerson.Visible )
            {
                _personId = ppPerson.PersonId;
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
            int sessionCount = GetAttributeValue( "SessionCount" ).AsInteger();

            int skipCount = pageNumber * sessionCount;

            using ( var rockContext = new RockContext() )
            {
                var interactionChannel = new InteractionChannelService( rockContext ).Get( _channelId.Value );
                if ( interactionChannel != null )
                {
                    var interactionService = new InteractionService( rockContext );

                    // Start the interaction qry to filter on those that belong to selected channel and have a valid person and session
                    var interactionQry = interactionService
                        .Queryable().AsNoTracking()
                        .Where( a =>
                            a.InteractionComponent.InteractionChannelId == _channelId.Value &&
                            a.PersonAliasId.HasValue &&
                            a.InteractionSessionId.HasValue );

                    // Filter by start date
                    if ( startDate != DateTime.MinValue )
                    {
                        interactionQry = interactionQry.Where( s => s.InteractionDateTime > drpDateFilter.LowerValue );
                    }

                    // Filter by end date
                    if ( endDate != DateTime.MaxValue )
                    {
                        interactionQry = interactionQry.Where( s => s.InteractionDateTime < drpDateFilter.UpperValue );
                    }

                    // Filter by person
                    if ( _personId.HasValue )
                    {
                        interactionQry = interactionQry.Where( s => s.PersonAlias.PersonId == _personId.Value );
                    }

                    // Select minimal data to speed up query and group the interactions by the session id
                    var grpInteractions = interactionQry
                        .Select( i => new
                        {
                            i.Id,
                            i.InteractionDateTime,
                            i.InteractionSessionId
                        } )
                        .GroupBy( s => s.InteractionSessionId.Value )
                        .Select( s => new WebSession
                        {
                            InteractionSessionId = s.Key,
                            StartDateTime = s.Min( x => x.InteractionDateTime ),
                            EndDateTime = s.Max( x => x.InteractionDateTime ),
                        } );

                    // Skip/Take only the sessions for the currently selected page number
                    var webSessions = grpInteractions.OrderByDescending( p => p.EndDateTime )
                        .Skip( skipCount )
                        .Take( sessionCount + 1 )
                        .ToList();

                    // Now that we know the sessions, requery for all of the interaction data just for these sessions
                    var pageSessionIds = webSessions.Select( s => s.InteractionSessionId ).ToList();
                    var currentPageInteractions = interactionQry
                        .Where( i => pageSessionIds.Contains( i.InteractionSessionId.Value ) )
                        .ToList();
                    foreach ( var webSession in webSessions )
                    {
                        var sessionInteractions = currentPageInteractions
                            .Where( i =>
                                i.InteractionSessionId.Value == webSession.InteractionSessionId )
                            .ToList();
                        if ( sessionInteractions.Any() )
                        {
                            webSession.Interactions = sessionInteractions;

                            var firstInteraction = sessionInteractions.First();
                            if ( firstInteraction != null )
                            {
                                webSession.PersonAlias = firstInteraction.PersonAlias;
                                webSession.InteractionSession = firstInteraction.InteractionSession;
                            }
                        }
                    }

                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                    mergeFields.Add( "ComponentDetailPage", LinkedPageRoute( "ComponentDetailPage" ) );
                    mergeFields.Add( "InteractionDetailPage", LinkedPageRoute( "InteractionDetailPage" ) );
                    mergeFields.Add( "InteractionChannel", interactionChannel );
                    mergeFields.Add( "WebSessions", webSessions.Take( sessionCount ) );

                    lContent.Text = interactionChannel.SessionListTemplate.IsNotNullOrWhiteSpace() ?
                        interactionChannel.SessionListTemplate.ResolveMergeFields( mergeFields ) :
                        GetAttributeValue( "DefaultTemplate" ).ResolveMergeFields( mergeFields );

                    // set next button
                    if ( webSessions.Count() > sessionCount )
                    {
                        hlNext.Visible = hlNext.Enabled = true;
                        Dictionary<string, string> queryStringNext = new Dictionary<string, string>();
                        queryStringNext.Add( "ChannelId", _channelId.ToString() );
                        queryStringNext.Add( "Page", ( pageNumber + 1 ).ToString() );

                        if ( _personId.HasValue )
                        {
                            queryStringNext.Add( "PersonId", _personId.Value.ToString() );
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
                        queryStringPrev.Add( "ChannelId", _channelId.ToString() );
                        queryStringPrev.Add( "Page", ( pageNumber - 1 ).ToString() );

                        if ( _personId.HasValue )
                        {
                            queryStringPrev.Add( "PersonId", _personId.Value.ToString() );
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
            }
        }

        /// <summary>
        /// Get the person through query list or context.
        /// </summary>
        public int? GetPersonId()
        {
            int? personId = PageParameter( "PersonId" ).AsIntegerOrNull();
            int? personAliasId = PageParameter( "PersonAliasId" ).AsIntegerOrNull();

            if ( personAliasId.HasValue )
            {
                personId = new PersonAliasService( new RockContext() ).GetPersonId( personAliasId.Value );
            }

            if ( !personId.HasValue )
            {
                var person = ContextEntity<Person>();
                if ( person != null )
                {
                    personId = person.Id;
                }
            }

            return personId;
        }

        #endregion

    }

    /// <summary>
    /// Helper class for binding sessions
    /// </summary>
    [DotLiquid.LiquidType( "InteractionSession", "PersonAlias", "StartDateTime", "EndDateTime", "Interactions" )]
    public class WebSession : LavaDataObject
    {
        /// <summary>
        /// Gets or sets the interaction session identifier.
        /// </summary>
        /// <value>
        /// The interaction session identifier.
        /// </value>
        public int InteractionSessionId { get; set; }

        /// <summary>
        /// Gets or sets the page view session.
        /// </summary>
        /// <value>
        /// The page view session.
        /// </value>
        public InteractionSession InteractionSession { get; set; }

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
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        public PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the interactions.
        /// </summary>
        /// <value>
        /// The interactions.
        /// </value>
        public ICollection<Interaction> Interactions { get; set; }
    }
}