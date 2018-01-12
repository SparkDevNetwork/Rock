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
using Rock.Model;

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
    {% for personSessionList in PersonSessionLists %}
	    <div class='panel panel-block'>
            <header class='panel-heading clearfix'>
			    <div class='pull-left'>
					<h4 class='panel-title'>
                        <i class='fa fa-desktop'></i>
                        {{ personSessionList.PersonAlias.Person.FullName }}
                    </h4>
			    </div>
		    </header>
	        <div class='panel-body'>
		        {% for session in personSessionList.Sessions %}
		        <div class='panel panel-widget pageviewsession'>
			        <header class='panel-heading clearfix'>
				        <div class='pull-left'>
					        <h4 class='panel-title'>
					        Started {{ session.StartDateTime | HumanizeDateTime }}
					        <small>
					        Duration: {{ session.StartDateTime | HumanizeTimeSpan:session.EndDateTime, 1 }}
					        </small>
					        </h4>
					        <span class='label label-info'>{{ InteractionChannel.Name }}</span>
				        </div> 
				        {% assign icon = '' %}
				        {% case session.InteractionSession.DeviceType.ClientType %}
				            {% when 'Desktop' %}{% assign icon = 'fa-desktop' %}
				            {% when 'Tablet' %}{% assign icon = 'fa-tablet' %}
				            {% when 'Mobile' %}{% assign icon = 'fa-mobile-phone' %}
				            {% else %}{% assign icon = '' %}
				        {% endcase %}
				        <div class='pageviewsession-client pull-right'>
                            <div class='pull-left'>
                                <small>{{ session.InteractionSession.DeviceType.Application }} <br>
                                {{ session.InteractionSession.DeviceType.OperatingSystem }} </small>
                            </div>
                            <i class='fa {{ icon }} fa-2x pull-right'></i>
                        </div>
			        </header>
			        <div class='panel-body'>
				        {% assign interactionCount = 0 %}
				        <ol>
				        {% for interaction in session.Interactions %}
				            {% assign interactionCount = interactionCount | Plus: 1 %}
				            {% assign componentDetailPage = interaction.InteractionData %}
				            {% if ComponentDetailPage != null and ComponentDetailPage != '' %}
    				            {% assign componentDetailPage = ComponentDetailPage %}
				            {% endif %}
				            <li><a href = '{{ componentDetailPage }}?ComponentId={{ interaction.InteractionComponentId }}'>{{ interaction.InteractionComponent.Name }}</a></li>
				        {% endfor %}				
				        </ol>
			        </div>
		        </div>
		        {% endfor %}
	        </div>
	    </div>
    {% endfor %}
{% endif %}" )]
    public partial class InteractionSessionList : Rock.Web.UI.RockBlock
    {
        #region Fields

        private int? _channelId = null;
        private DateTime startDate = DateTime.MinValue;
        private DateTime endDate = DateTime.MaxValue;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _channelId = PageParameter( "channelId" ).AsIntegerOrNull();
            if ( !_channelId.HasValue )
            {
                upnlContent.Visible = false;
            }
            else
            {
                upnlContent.Visible = true;
            }

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

            if ( !Page.IsPostBack )
            {
                if ( _channelId.HasValue )
                {
                    ShowList();
                }
            }
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

            ShowList();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the list.
        /// </summary>
        public void ShowList()
        {
            using ( var rockContext = new RockContext() )
            {
                var interactionChannel = new InteractionChannelService( rockContext ).Get( _channelId.Value );
                if ( interactionChannel != null )
                {
                    var interactionQry = new InteractionService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( a =>
                            a.InteractionComponent.ChannelId == _channelId.Value &&
                            a.PersonAliasId.HasValue &&
                            a.InteractionSessionId.HasValue );

                    if ( startDate != DateTime.MinValue )
                    {
                        interactionQry = interactionQry.Where( s => s.InteractionDateTime > drpDateFilter.LowerValue );
                    }

                    if ( endDate != DateTime.MaxValue )
                    {
                        interactionQry = interactionQry.Where( s => s.InteractionDateTime < drpDateFilter.UpperValue );
                    }

                    var groupedSessions = interactionQry
                        .GroupBy( s => new
                        {
                            s.InteractionSession,
                            s.PersonAlias,
                        } )
                        .Select( s => new
                        {
                            PersonAlias = s.Key.PersonAlias,
                            InteractionSession = s.Key.InteractionSession,
                            StartDateTime = s.Min( x => x.InteractionDateTime ),
                            EndDateTime = s.Max( x => x.InteractionDateTime ),
                            Interactions = s.ToList()
                        } )
                        .ToList();

                    var personSessionLists = groupedSessions
                        .GroupBy( a => a.PersonAlias.Id )
                        .Select( a => new PersonSessionList
                        {
                            PersonAlias = a.Select( b => b.PersonAlias ).First( b => b.Id == a.Key ),
                            Sessions = a
                                .Select( b => new WebSession
                                {
                                    InteractionSession = b.InteractionSession,
                                    Interactions = b.Interactions,
                                    StartDateTime = b.StartDateTime,
                                    EndDateTime = b.EndDateTime,
                                } )
                                .OrderByDescending( b => b.StartDateTime ).ToList()
                        } )
                        .ToList();

                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                    mergeFields.Add( "ComponentDetailPage", LinkedPageRoute( "ComponentDetailPage" ) );
                    mergeFields.Add( "InteractionDetailPage", LinkedPageRoute( "InteractionDetailPage" ) );
                    mergeFields.Add( "InteractionChannel", interactionChannel );
                    mergeFields.Add( "PersonSessionLists", personSessionLists );

                    lContent.Text = interactionChannel.SessionListTemplate.IsNotNullOrWhitespace() ?
                        interactionChannel.SessionListTemplate.ResolveMergeFields( mergeFields ) :
                        GetAttributeValue( "DefaultTemplate" ).ResolveMergeFields( mergeFields );
                }
            }
        }

        #endregion

    }
    [DotLiquid.LiquidType( "PersonAlias", "Sessions" )]
    public class PersonSessionList
    {
        public PersonAlias PersonAlias { get; set; }

        public List<WebSession> Sessions { get; set; }
    }

    [DotLiquid.LiquidType( "InteractionSession", "StartDateTime", "EndDateTime", "Interactions" )]
    public class WebSession
    {
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
        /// Gets or sets the interactions.
        /// </summary>
        /// <value>
        /// The interactions.
        /// </value>
        public ICollection<Interaction> Interactions { get; set; }
    }
}