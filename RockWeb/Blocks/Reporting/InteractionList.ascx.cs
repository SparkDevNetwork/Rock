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
using Rock.Security;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// List all the Interactions.
    /// </summary>
    [DisplayName( "Interaction List" )]
    [Category( "Reporting" )]
    [Description( "List all the Interaction" )]

    [LinkedPage( "Interaction Detail Page", "Page reference to the interaction detail page. This will be included as a variable in the Lava.", false, order: 1 )]
    [CodeEditorField( "Default Template", "The Lava template to use as default.", Rock.Web.UI.Controls.CodeEditorMode.Lava, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 300, false, order: 2, defaultValue: @"
     <div class='panel panel-block'>
        <div class='panel-heading'>
	        <h1 class='panel-title'>
                <i class='fa fa-user'></i>
                Interactions
            </h1>
        </div>
        <div class='panel-body'>

	        {% for interaction in Interactions %}
		        {% if InteractionDetailPage != null and InteractionDetailPage != ''  %}
                    <a href = '{{ InteractionDetailPage }}?interactionId={{ interaction.Id }}'>
                {% endif %}
		        
		         <div class='panel panel-widget'>
                    <div class='panel-heading'>
                        
                        <div class='row'>
                            <div class='col-md-12'>
                                <span class='label label-info pull-left margin-r-md'>{{ interaction.Operation }}</span>
                            
                                {% if InteractionChannel.Name != '' %}<h1 class='panel-title pull-left'>{{ interaction.InteractionDateTime }}</h1>{% endif %}
                                
                                <div class='pull-right'><i class='fa fa-chevron-right'></i></div>
                            </div>
                        </div>
                        
                        <div class='row margin-t-md'>
                            {% if interaction.InteractionSummary && interaction.InteractionSummary != '' %}
                            <div class='col-md-6'>
                                <dl>
                                    <dt>Interaction Summary</dt>
                                    <dd>{{ interaction.InteractionSummary }}</dd>
                                </dl>
                            </div>
                            {% endif %}
                            
                            {% if interaction.InteractionData && interaction.InteractionData != '' %}
                            <div class='col-md-6'>
                                <dl>
                                    <dt>Interaction Data</dt>
                                    <dd>{{ interaction.InteractionData }}</dd>
                                </dl>
                            </div>
                            {% endif %}
                        </div>
                    </div>
                </div>
		        
		        {% if InteractionDetailPage != null and InteractionDetailPage != ''  %}
    		        </a>
		        {% endif %}
	        {% endfor %}	
	        <div class ='nav-paging'>
            {% if PreviousPageNavigateUrl != null and PreviousPageNavigateUrl != ''  %}
                <a Id ='lPrev' class = 'btn btn-primary btn-prev' href='{{ PreviousPageNavigateUrl }}'><i class='fa fa-chevron-left'></i>Prev<a/>
            {% endif %}
            {% if NextPageNavigateUrl != null and NextPageNavigateUrl != ''  %}
                <a Id ='hlNext' class = 'btn btn-primary btn-next' href='{{ NextPageNavigateUrl }}'> Next <i class='fa fa-chevron-right'></i><a/>
            {% endif %}
            </div>
        </div>
    </div>" )]
    [IntegerField( "Page Size", "The number of interactions to show per page.", true, 20, "", 3 )]
    public partial class InteractionList : Rock.Web.UI.RockBlock
    {
        #region Fields

        private int pageNumber = 0;

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
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                if ( !string.IsNullOrEmpty( PageParameter( "Page" ) ) )
                {
                    pageNumber = PageParameter( "Page" ).AsInteger();
                }

                ShowList( PageParameter( "componentId" ).AsInteger() );
            }
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
            ShowList( PageParameter( "componentId" ).AsInteger() );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the list.
        /// </summary>
        public void ShowList( int componentId )
        {
            int pageSize = GetAttributeValue( "PageSize" ).AsInteger();

            int skipCount = pageNumber * pageSize;

            using ( var rockContext = new RockContext() )
            {
                var component = new InteractionComponentService( rockContext ).Get( componentId );
                if ( component != null && ( UserCanEdit || component.IsAuthorized( Authorization.VIEW, CurrentPerson ) ) )
                {
                    var interactions = new InteractionService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( a =>
                            a.InteractionComponentId == componentId )
                        .OrderByDescending( a => a.InteractionDateTime )
                        .Skip( skipCount )
                        .Take( pageSize + 1 );

                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                    mergeFields.AddOrIgnore( "Person", CurrentPerson );
                    mergeFields.Add( "InteractionDetailPage", LinkedPageRoute( "InteractionDetailPage" ) );
                    mergeFields.Add( "InteractionChannel", component.Channel );
                    mergeFields.Add( "InteractionComponent", component );
                    mergeFields.Add( "Interactions", interactions.ToList().Take( pageSize ) );

                    // set next button
                    if ( interactions.Count() > pageSize )
                    {
                        Dictionary<string, string> queryStringNext = new Dictionary<string, string>();
                        queryStringNext.Add( "ComponentId", componentId.ToString() );
                        queryStringNext.Add( "Page", ( pageNumber + 1 ).ToString() );

                        var pageReferenceNext = new Rock.Web.PageReference( CurrentPageReference.PageId, CurrentPageReference.RouteId, queryStringNext );
                        mergeFields.Add( "NextPageNavigateUrl", pageReferenceNext.BuildUrl() );
                    }

                    // set prev button
                    if ( pageNumber != 0 )
                    {
                        Dictionary<string, string> queryStringPrev = new Dictionary<string, string>();
                        queryStringPrev.Add( "ComponentId", componentId.ToString() );
                        queryStringPrev.Add( "Page", ( pageNumber - 1 ).ToString() );

                        var pageReferencePrev = new Rock.Web.PageReference( CurrentPageReference.PageId, CurrentPageReference.RouteId, queryStringPrev );
                        mergeFields.Add( "PreviousPageNavigateUrl", pageReferencePrev.BuildUrl() );
                    }

                    lContent.Text = component.Channel.InteractionListTemplate.IsNotNullOrWhitespace() ?
                        component.Channel.InteractionListTemplate.ResolveMergeFields( mergeFields ) :
                        GetAttributeValue( "DefaultTemplate" ).ResolveMergeFields( mergeFields );

                }
            }
        }

        #endregion

    }
}