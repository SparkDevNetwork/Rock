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
{% for interaction in Interactions %}
    {% if InteractionDetailPage != null and InteractionDetailPage != '' %}
        <a href = '{{ InteractionDetailPage }}?InteractionId={{ interaction.Id }}'>
    {% endif %}
    
    <div class='panel panel-widget'>
        <div class='panel-heading'>
            <div class='w-100'>
                <div class='row'>
                    <div class='col-md-12'>
                        <span class='label label-info pull-left margin-r-md'>{{ interaction.Operation }}</span>
                    
                        {% if InteractionChannel.Name != '' %}<h1 class='panel-title pull-left'>{{ interaction.InteractionDateTime }}</h1>{% endif %}
                        
                        <div class='pull-right'><i class='fa fa-chevron-right'></i></div>
                    </div>
                </div>
                
                <div class='row margin-t-md'>
                    {% if interaction.InteractionSummary and interaction.InteractionSummary != '' %}
                    <div class='col-md-6'>
                        <dl class='mb-0'>
                            <dt>Interaction Summary</dt>
                            <dd>{{ interaction.InteractionSummary }}</dd>
                        </dl>
                    </div>
                    {% endif %}
                    
                    {% if interaction.InteractionData and interaction.InteractionData != '' %}
                    <div class='col-md-6'>
                        <dl class='mb-0'>
                            <dt>Interaction Data</dt>
                            <dd>{{ interaction.InteractionData }}</dd>
                        </dl>
                    </div>
                    {% endif %}
                </div>
            </div>
        </div>
    </div>
    
    {% if InteractionDetailPage != null and InteractionDetailPage != '' %}
        </a>
    {% endif %}
{% endfor %}
	      " )]
    [IntegerField( "Page Size", "The number of interactions to show per page.", true, 20, "", 3 )]
    [Rock.SystemGuid.BlockTypeGuid( "468119E3-41AB-4EC4-B631-77F326632B35" )]
    public partial class InteractionList : Rock.Web.UI.RockBlock
    {
        #region Fields

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
            base.OnLoad( e );

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

                ShowList( PageParameter( "ComponentId" ).AsInteger() );
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
            _personId = GetPersonId();
            ppPerson.Visible = !_personId.HasValue;
            ShowList( PageParameter( "ComponentId" ).AsInteger() );
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

            ShowList( PageParameter( "ComponentId" ).AsInteger() );
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
                            a.InteractionComponentId == componentId );

                    if ( startDate != DateTime.MinValue )
                    {
                        interactions = interactions.Where( s => s.InteractionDateTime > drpDateFilter.LowerValue );
                    }

                    if ( endDate != DateTime.MaxValue )
                    {
                        interactions = interactions.Where( s => s.InteractionDateTime < drpDateFilter.UpperValue );
                    }

                    if ( _personId.HasValue )
                    {
                        interactions = interactions.Where( s => s.PersonAlias.PersonId == _personId );
                    }

                    interactions = interactions
                             .OrderByDescending( a => a.InteractionDateTime )
                             .Skip( skipCount )
                             .Take( pageSize + 1 );

                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                    mergeFields.TryAdd( "CurrentPerson", CurrentPerson );
                    mergeFields.Add( "InteractionDetailPage", LinkedPageRoute( "InteractionDetailPage" ) );
                    mergeFields.Add( "InteractionChannel", component.InteractionChannel );
                    mergeFields.Add( "InteractionComponent", component );
                    mergeFields.Add( "Interactions", interactions.ToList().Take( pageSize ) );

                    lContent.Text = component.InteractionChannel.InteractionListTemplate.IsNotNullOrWhiteSpace() ?
                        component.InteractionChannel.InteractionListTemplate.ResolveMergeFields( mergeFields ) :
                        GetAttributeValue( "DefaultTemplate" ).ResolveMergeFields( mergeFields );

                    // set next button
                    if ( interactions.Count() > pageSize )
                    {
                        Dictionary<string, string> queryStringNext = new Dictionary<string, string>();
                        queryStringNext.Add( "ComponentId", componentId.ToString() );
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
                        Dictionary<string, string> queryStringPrev = new Dictionary<string, string>();
                        queryStringPrev.Add( "ComponentId", componentId.ToString() );
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
}