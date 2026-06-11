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
using Rock.Web.UI;
using Rock.Utility;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// List all the Interaction Component.
    /// </summary>
    [DisplayName( "Interaction Component List" )]
    [Category( "Reporting" )]
    [Description( "List all the Interaction Component" )]

    [LinkedPage( "Component Detail Page", "Page reference to the component detail page. This will be included as a variable in the Lava.", false, order: 0 )]
    [LinkedPage( "Interaction Detail Page", "Page reference to the interaction detail page. This will be included as a variable in the Lava.", false, order: 1 )]

    [CodeEditorField( "Default Template",
        Description = "The Lava template to use as default.",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorHeight = 300,
        IsRequired = false,
        Order = 2,
        DefaultValue = @"
	<div class='panel panel-block'>
        <div class='panel-heading'>
			<h1 class='panel-title'>
                <i class='ti ti-grid-dots'></i>
                Components
            </h1>
        </div>
		<div class='panel-body'>
			{% for component in InteractionComponents %}
			
				 {% if ComponentDetailPage != null and ComponentDetailPage != '' %}
                    <a href = '{{ ComponentDetailPage }}?ComponentId={{ component.IdKey }}'>
                {% endif %}
                
				 <div class='panel panel-widget'>
                    <div class='panel-heading clearfix'>
                        {% if component.Name != '' %}<h1 class='panel-title pull-left'>{{ component.Name }}</h1>{% endif %}
                        <div class='pull-right'><i class='ti ti-chevron-right'></i></div>
                    </div>
                </div>
                {% if ComponentDetailPage != null and ComponentDetailPage != '' %}
                    </a>
                {% endif %}
				
			{% endfor %}	
            <div class ='nav-paging'>
            {% if PreviousPageNavigateUrl != null and PreviousPageNavigateUrl != '' %}
                <a Id ='lPrev' class = 'btn btn-primary btn-prev' href='{{ PreviousPageNavigateUrl }}'><i class='ti ti-chevron-left'></i> Prev<a/>
            {% endif %}
            {% if NextPageNavigateUrl != null and NextPageNavigateUrl != '' %}
                <a Id ='hlNext' class = 'btn btn-primary btn-next' href='{{ NextPageNavigateUrl }}'> Next <i class='ti ti-chevron-right'></i><a/>
            {% endif %}
            </div>
		</div>
	</div>" )]

    [IntegerField( "Page Size", "The number of components to show per page.", true, 20, "", 3 )]

    [ContextAware( typeof( Person ) )]
    [Rock.SystemGuid.BlockTypeGuid( "00FF58B1-A433-43AA-82C9-45F8F58FBE9F" )]
    public partial class InteractionComponentList : Rock.Web.UI.RockBlock
    {
        #region Fields

        private int pageNumber = 0;
        private int? _channelId = null;

        #endregion Fields

        #region Keys

        private static class AttributeKey
        {
            public const string DefaultTemplate = "DefaultTemplate";
            public const string PageSize = "PageSize";

        }

        private static class PageParameterKey
        {
            public const string ChannelId = "ChannelId";
            public const string Page = "Page";
            public const string PersonId = "PersonId";
            public const string PersonAliasId = "PersonAliasId";
        }

        private static class MergeFieldKey
        {
            public const string ComponentDetailPage = "ComponentDetailPage";
            public const string InteractionDetailPage = "InteractionDetailPage";
            public const string InteractionChannel = "InteractionChannel";
            public const string InteractionComponents = "InteractionComponents";
            public const string InteractionCounts = "InteractionCounts";
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

            using ( var rockContext = new RockContext() )
            {
                _channelId = new InteractionChannelService( rockContext ).GetSelect(
                    PageParameter( PageParameterKey.ChannelId ),
                    ic => (int?) ic.Id,
                    !PageCache.Layout.Site.DisablePredictableIds
                );
            }

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
            if ( !Page.IsPostBack )
            {
                if ( _channelId.HasValue )
                {
                    if ( !string.IsNullOrEmpty( PageParameter( PageParameterKey.Page ) ) )
                    {
                        pageNumber = PageParameter( PageParameterKey.Page ).AsInteger();
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
            ShowList();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the list.
        /// </summary>
        public void ShowList()
        {
            int pageSize = GetAttributeValue( AttributeKey.PageSize ).AsInteger();

            int skipCount = pageNumber * pageSize;

            using ( var rockContext = new RockContext() )
            {
                var interactionChannel = new InteractionChannelService( rockContext ).Get( _channelId.Value );
                if ( interactionChannel != null )
                {
                    var interactionComponentQry = new InteractionComponentService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( a =>
                            a.InteractionChannelId == _channelId.Value );

                    var personId = GetPersonId();
                    if ( personId.HasValue )
                    {
                        var interactionQry = new InteractionService( rockContext ).Queryable();
                        interactionComponentQry = interactionComponentQry.Where( a => interactionQry.Any( b => b.PersonAlias.PersonId == personId.Value && b.InteractionComponentId == a.Id ) );
                    }

                    interactionComponentQry = interactionComponentQry
                                        .OrderByDescending( a => a.ModifiedDateTime )
                                        .Skip( skipCount )
                                        .Take( pageSize + 1 );

                    var interactionComponents = interactionComponentQry.ToList().Take( pageSize );
                    var componentIdList = interactionComponents.Select( c => c.Id );

                    var componentInteractionCount = new InteractionService( rockContext ).Queryable().AsNoTracking()
                        .Where( i => componentIdList.Contains( i.InteractionComponentId ) )
                        .GroupBy( i => i.InteractionComponentId )
                        .Select( g => new InteractionCount
                        {
                            ComponentId = g.Key,
                            Count = g.Count()
                        } )
                        .ToList();

                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                    mergeFields.Add( MergeFieldKey.ComponentDetailPage, LinkedPageRoute( MergeFieldKey.ComponentDetailPage ) );
                    mergeFields.Add( MergeFieldKey.InteractionDetailPage, LinkedPageRoute( MergeFieldKey.InteractionDetailPage ) );
                    mergeFields.Add( MergeFieldKey.InteractionChannel, interactionChannel );
                    mergeFields.Add( MergeFieldKey.InteractionComponents, interactionComponents );
                    mergeFields.Add( MergeFieldKey.InteractionCounts, componentInteractionCount );

                    // set next button
                    if ( interactionComponentQry.Count() > pageSize )
                    {
                        Dictionary<string, string> queryStringNext = new Dictionary<string, string>();
                        queryStringNext.Add( PageParameterKey.ChannelId, _channelId.ToString() );
                        queryStringNext.Add( PageParameterKey.Page, ( pageNumber + 1 ).ToString() );

                        if ( personId.HasValue )
                        {
                            queryStringNext.Add( PageParameterKey.PersonId, personId.Value.ToString() );
                        }

                        var pageReferenceNext = new Rock.Web.PageReference( CurrentPageReference.PageId, CurrentPageReference.RouteId, queryStringNext );
                        mergeFields.Add( PageNavigationKey.NextPageNavigateUrl, pageReferenceNext.BuildUrl() );
                    }

                    // set prev button
                    if ( pageNumber != 0 )
                    {
                        Dictionary<string, string> queryStringPrev = new Dictionary<string, string>();
                        queryStringPrev.Add( PageParameterKey.ChannelId, _channelId.ToString() );
                        queryStringPrev.Add( PageParameterKey.Page, ( pageNumber - 1 ).ToString() );

                        if ( personId.HasValue )
                        {
                            queryStringPrev.Add( PageParameterKey.PersonId, personId.Value.ToString() );
                        }

                        var pageReferencePrev = new Rock.Web.PageReference( CurrentPageReference.PageId, CurrentPageReference.RouteId, queryStringPrev );
                        mergeFields.Add( PageNavigationKey.PreviousPageNavigateUrl, pageReferencePrev.BuildUrl() );
                    }

                    lContent.Text = interactionChannel.ComponentListTemplate.IsNotNullOrWhiteSpace() ?
                        interactionChannel.ComponentListTemplate.ResolveMergeFields( mergeFields ) :
                        GetAttributeValue( AttributeKey.DefaultTemplate ).ResolveMergeFields( mergeFields );
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


        /// <summary>
        /// POCO class for return interaction counts
        /// </summary>
        class InteractionCount : RockDynamic
        {
            /// <summary>
            /// Gets or sets the component identifier.
            /// </summary>
            /// <value>
            /// The component identifier.
            /// </value>
            public int ComponentId { get; set; }

            /// <summary>
            /// Gets or sets the count.
            /// </summary>
            /// <value>
            /// The count.
            /// </value>
            public int Count { get; set; }
        }
    }
}