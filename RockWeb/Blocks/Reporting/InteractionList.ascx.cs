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
    [CodeEditorField( "Default Template",
        Description = "The Lava template to use as default.",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorHeight = 300,
        IsRequired = false,
        Order = 2,
        DefaultValue = @"
{% for interaction in Interactions %}
    {% if InteractionDetailPage != null and InteractionDetailPage != '' %}
        <a href = '{{ InteractionDetailPage }}?InteractionId={{ interaction.IdKey }}'>
    {% endif %}
    
    <div class='panel panel-widget'>
        <div class='panel-heading'>
            <div class='w-100'>
                <div class='row'>
                    <div class='col-md-12'>
                        <span class='label label-info pull-left margin-r-md'>{{ interaction.Operation }}</span>
                    
                        {% if InteractionChannel.Name != '' %}<h1 class='panel-title pull-left'>{{ interaction.InteractionDateTime }}</h1>{% endif %}
                        
                        <div class='pull-right'><i class='ti ti-chevron-right'></i></div>
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

        #endregion Fields

        #region Keys

        private static class AttributeKey
        {
            public const string DefaultTemplate = "DefaultTemplate";
            public const string PageSize = "PageSize";
        }

        private static class PageParameterKey
        {
            public const string ComponentId = "ComponentId";
            public const string PersonId = "PersonId";
            public const string PersonAliasId = "PersonAliasId";
            public const string StartDate = "StartDate";
            public const string EndDate = "EndDate";
            public const string Page = "Page";
        }

        private static class MergeFieldKey
        {
            public const string CurrentPerson = "CurrentPerson";
            public const string InteractionDetailPage = "InteractionDetailPage";
            public const string InteractionChannel = "InteractionChannel";
            public const string InteractionComponent = "InteractionComponent";
            public const string Interactions = "Interactions";
        }

        private static class PageNavigationKey
        {
            public const string NextPageNavigateUrl = "NextPageNavigateUrl";
            public const string PreviousPageNavigateUrl = "PreviousPageNavigateUrl";
        }

        #endregion Keys

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
            if ( !Page.IsPostBack )
            {
                var startDateParam = PageParameter( PageParameterKey.StartDate );
                if ( !string.IsNullOrWhiteSpace( startDateParam ) )
                {
                    startDate = startDateParam.AsDateTime() ?? DateTime.MinValue;
                    if ( startDate != DateTime.MinValue )
                    {
                        drpDateFilter.LowerValue = startDate;
                    }
                }

                var endDateParam = PageParameter( PageParameterKey.EndDate );
                if ( !string.IsNullOrWhiteSpace( endDateParam ) )
                {
                    endDate = endDateParam.AsDateTime() ?? DateTime.MaxValue;
                    if ( endDate != DateTime.MaxValue )
                    {
                        drpDateFilter.UpperValue = endDate;
                    }
                }

                var pageParam = PageParameter( PageParameterKey.Page );
                if ( !string.IsNullOrEmpty( pageParam ) )
                {
                    pageNumber = pageParam.AsInteger();
                }

                int? componentId = null;
                using ( var rockContext = new RockContext() )
                {
                    componentId = new InteractionComponentService( rockContext ).GetSelect(
                        PageParameter( PageParameterKey.ComponentId ),
                        c => (int?) c.Id,
                        !PageCache.Layout.Site.DisablePredictableIds
                    );
                }

                if ( componentId.HasValue )
                {
                    ShowList( componentId.Value );
                }
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
            _personId = GetPersonId();
            ppPerson.Visible = !_personId.HasValue;

            int? componentId = null;
            using ( var rockContext = new RockContext() )
            {
                componentId = new InteractionComponentService( rockContext ).GetSelect(
                    PageParameter( PageParameterKey.ComponentId ),
                    ic => (int?) ic.Id,
                    !PageCache.Layout.Site.DisablePredictableIds
                );
            }

            if ( componentId.HasValue )
            {
                ShowList( componentId.Value );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnFilter_Click( object sender, EventArgs e )
        {
            startDate = drpDateFilter.LowerValue ?? DateTime.MinValue;
            endDate = drpDateFilter.UpperValue ?? DateTime.MaxValue;

            if ( ppPerson.PersonId.HasValue && ppPerson.Visible )
            {
                _personId = ppPerson.PersonId;
            }

            pageNumber = 0;

            int? componentId = null;
            using ( var rockContext = new RockContext() )
            {
                componentId = new InteractionComponentService( rockContext ).GetSelect(
                    PageParameter( PageParameterKey.ComponentId ),
                    ic => (int?)ic.Id,
                    !PageCache.Layout.Site.DisablePredictableIds
                );
            }

            if ( componentId.HasValue )
            {
                ShowList( componentId.Value );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the list.
        /// </summary>
        public void ShowList( int componentId )
        {
            int pageSize = GetAttributeValue( AttributeKey.PageSize ).AsInteger();

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
                        interactions = interactions.Where( s => s.InteractionDateTime >= startDate );
                    }

                    if ( endDate != DateTime.MaxValue )
                    {
                        interactions = interactions.Where( s => s.InteractionDateTime <= endDate );
                    }

                    if ( _personId.HasValue )
                    {
                        interactions = interactions.Where( s => s.PersonAlias.PersonId == _personId.Value );
                    }

                    interactions = interactions
                             .OrderByDescending( a => a.InteractionDateTime )
                             .Skip( skipCount )
                             .Take( pageSize + 1 );

                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                    mergeFields.TryAdd( MergeFieldKey.CurrentPerson, CurrentPerson );
                    mergeFields.Add( MergeFieldKey.InteractionDetailPage, LinkedPageRoute( MergeFieldKey.InteractionDetailPage ) );
                    mergeFields.Add( MergeFieldKey.InteractionChannel, component.InteractionChannel );
                    mergeFields.Add( MergeFieldKey.InteractionComponent, component );
                    mergeFields.Add( MergeFieldKey.Interactions, interactions.ToList().Take( pageSize ) );

                    lContent.Text = component.InteractionChannel.InteractionListTemplate.IsNotNullOrWhiteSpace() ?
                        component.InteractionChannel.InteractionListTemplate.ResolveMergeFields( mergeFields ) :
                        GetAttributeValue( AttributeKey.DefaultTemplate ).ResolveMergeFields( mergeFields );

                    // set next button
                    if ( interactions.Count() > pageSize )
                    {
                        hlNext.Visible = hlNext.Enabled = true;
                        Dictionary<string, string> queryStringNext = new Dictionary<string, string>();

                        queryStringNext.Add( PageParameterKey.ComponentId, componentId.ToString() );
                        queryStringNext.Add( PageParameterKey.Page, ( pageNumber + 1 ).ToString() );

                        if ( _personId.HasValue )
                        {
                            queryStringNext.Add( PageParameterKey.PersonId, _personId.Value.ToString() );
                        }

                        if ( startDate != DateTime.MinValue )
                        {
                            queryStringNext.Add( PageParameterKey.StartDate, startDate.ToShortDateString() );
                        }

                        if ( endDate != DateTime.MaxValue )
                        {
                            queryStringNext.Add( PageParameterKey.EndDate, endDate.ToShortDateString() );
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
                        queryStringPrev.Add( PageParameterKey.ComponentId, componentId.ToString() );
                        queryStringPrev.Add( PageParameterKey.Page, ( pageNumber - 1 ).ToString() );

                        if ( _personId.HasValue )
                        {
                            queryStringPrev.Add( PageParameterKey.PersonId, _personId.Value.ToString() );
                        }

                        if ( startDate != DateTime.MinValue )
                        {
                            queryStringPrev.Add( PageParameterKey.StartDate, startDate.ToShortDateString() );
                        }

                        if ( endDate != DateTime.MaxValue )
                        {
                            queryStringPrev.Add( PageParameterKey.EndDate, endDate.ToShortDateString() );
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
            using ( var rockContext = new RockContext() )
            {
                var personId = new PersonService( rockContext ).GetSelect(
                    PageParameter( PageParameterKey.PersonId ),
                    p => (int?) p.Id,
                    !PageCache.Layout.Site.DisablePredictableIds
                );

                if ( !personId.HasValue )
                {
                    var personAliasId = new PersonAliasService( rockContext ).GetSelect(
                        PageParameter( PageParameterKey.PersonAliasId ),
                        pa => (int?) pa.Id,
                        !PageCache.Layout.Site.DisablePredictableIds
                    );

                    if ( personAliasId.HasValue )
                    {
                        personId = new PersonAliasService( rockContext ).GetPersonId( personAliasId.Value );
                    }
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
        }

        #endregion

    }
}