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
using DotLiquid;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// List all the Interaction Channel.
    /// </summary>
    [DisplayName( "Interaction Channel List" )]
    [Category( "Reporting" )]
    [Description( "List all the Interaction Channel" )]

    [LinkedPage( "Session List Page", "Page reference to the session list page. This will be included as a variable in the Lava.", false, order: 0 )]
    [LinkedPage( "Component List Page", "Page reference to the component list page. This will be included as a variable in the Lava.", false, order: 1 )]
    [CodeEditorField( "Default Template", "The Lava template to use as default.", Rock.Web.UI.Controls.CodeEditorMode.Lava, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 300, false, order: 2, defaultValue: @"{% if InteractionChannel != null and InteractionChannel != '' %}
    <a href='{% if InteractionChannel.UsesSession == true %}{{ SessionListPage }}{% else %}{{ ComponentListPage }}{% endif %}?ChannelId={{ InteractionChannel.Id }}'>
        <div class='panel panel-widget collapsed'>
            <div class='panel-heading clearfix'>
                {% if InteractionChannel.Name != '' %}<h1 class='panel-title pull-left'>{{ InteractionChannel.Name }}</h1>{% endif %}

                <div class='panel-labels d-flex align-items-center'>
                    {% if InteractionChannel.ChannelTypeMediumValue != null and InteractionChannel.ChannelTypeMediumValue != '' %}<span class='label label-info'>{{ InteractionChannel.ChannelTypeMediumValue.Value }}</span>{% endif %}
                    <i class='fa fa-chevron-right margin-l-md'></i>
                </div>
            </div>
        </div>
    </a>
{% endif %}" )]

    [InteractionChannelsField( "Interaction Channels", "Select interaction channel to limit the display. No selection will show all.", false, "", "", order: 3 )]
    [ContextAware( typeof( Person ) )]
    [Rock.SystemGuid.BlockTypeGuid( "FBC2066B-8E7C-43CB-AFD2-FA9408F6699D" )]
    public partial class InteractionChannelList : Rock.Web.UI.RockBlock
    {
        #region Fields

        private const string MEDIUM_TYPE_FILTER = "Medium Type";
        private const string INCLUDE_INACTIVE_FILTER = "Include Inactive";

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
                BindFilter();
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
        /// Handles the Apply Filter event for the GridFilter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gfFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfFilter.SetFilterPreference( MEDIUM_TYPE_FILTER, ddlMediumValue.SelectedValue );
            gfFilter.SetFilterPreference( INCLUDE_INACTIVE_FILTER, cbIncludeInactive.Checked.ToString() );
            ShowList();
        }

        /// <summary>
        /// Handles displaying the stored filter values.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e as DisplayFilterValueArgs (hint: e.Key and e.Value).</param>
        protected void gfFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {

            switch ( e.Key )
            {
                case "Medium Type":
                    var mediumTypeValueId = e.Value.AsIntegerOrNull();
                    if ( mediumTypeValueId.HasValue )
                    {
                        var mediumTypeValue = DefinedValueCache.Get( mediumTypeValueId.Value );
                        e.Value = mediumTypeValue.Value;
                    }
                    break;
                case INCLUDE_INACTIVE_FILTER:
                    var includeFilterValue = e.Value.AsBooleanOrNull();
                    if ( includeFilterValue.HasValue )
                    {
                        e.Value = includeFilterValue.Value.ToYesNo();
                    }
                    break;
                default:
                    {
                        e.Value = string.Empty;
                        break;
                    }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            var definedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.INTERACTION_CHANNEL_MEDIUM.AsGuid() );
            ddlMediumValue.DefinedTypeId = definedType.Id;

            var channelMediumValueId = gfFilter.GetFilterPreference( MEDIUM_TYPE_FILTER ).AsIntegerOrNull();
            ddlMediumValue.SetValue( channelMediumValueId );

            var includeInactive = gfFilter.GetFilterPreference( INCLUDE_INACTIVE_FILTER ).AsBooleanOrNull() ?? false;
            cbIncludeInactive.Checked = includeInactive;
        }

        /// <summary>
        /// Shows the list.
        /// </summary>
        public void ShowList()
        {
            using ( var rockContext = new RockContext() )
            {
                var channelQry = new InteractionChannelService( rockContext )
                    .Queryable().AsNoTracking();

                var channelMediumValueId = gfFilter.GetFilterPreference( MEDIUM_TYPE_FILTER ).AsIntegerOrNull();
                if ( channelMediumValueId.HasValue )
                {
                    channelQry = channelQry.Where( a => a.ChannelTypeMediumValueId == channelMediumValueId.Value );
                }

                if ( !cbIncludeInactive.Checked )
                {
                    channelQry = channelQry.Where( a => a.IsActive );
                }

                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "InteractionChannels" ) ) )
                {
                    var selectedChannelIds = Array.ConvertAll( GetAttributeValue( "InteractionChannels" ).Split( ',' ), s => new Guid( s ) ).ToList();
                    channelQry = channelQry.Where( a => selectedChannelIds.Contains( a.Guid ) );
                }

                var personId = GetPersonId();
                if ( personId.HasValue )
                {
                    var interactionQry = new InteractionService( rockContext ).Queryable();
                    channelQry = channelQry.Where( a => interactionQry.Any( b => b.PersonAlias.PersonId == personId.Value && b.InteractionComponent.InteractionChannelId == a.Id ) );
                }

                // Parse the default template so that it does not need to be parsed multiple times.
                Template defaultTemplate = null;
                ILavaTemplate defaultLavaTemplate = null;

                if ( LavaService.RockLiquidIsEnabled )
                {
                    defaultTemplate = LavaHelper.CreateDotLiquidTemplate( GetAttributeValue( "DefaultTemplate" ) );

                    LavaHelper.VerifyParseTemplateForCurrentEngine( GetAttributeValue( "DefaultTemplate" ) );
                }
                else
                {
                    var parseResult = LavaService.ParseTemplate( GetAttributeValue( "DefaultTemplate" ) );

                    defaultLavaTemplate = parseResult.Template;
                }

                var options = new Rock.Lava.CommonMergeFieldsOptions();
                options.GetPageContext = false;
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, options );
                mergeFields.Add( "ComponentListPage", LinkedPageRoute( "ComponentListPage" ) );
                mergeFields.Add( "SessionListPage", LinkedPageRoute( "SessionListPage" ) );

                var channelItems = new List<ChannelItem>();

                if ( LavaService.RockLiquidIsEnabled )
                {
                    foreach ( var channel in channelQry )
                    {
                        if ( !channel.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                        {
                            continue;
                        }
                        var channelMergeFields = new Dictionary<string, object>( mergeFields );
                        channelMergeFields.Add( "InteractionChannel", channel );

                        string html = channel.ChannelListTemplate.IsNotNullOrWhiteSpace() ?
                            channel.ChannelListTemplate.ResolveMergeFields( channelMergeFields ) :
                            defaultTemplate.Render( Hash.FromDictionary( channelMergeFields ) );

                        channelItems.Add( new ChannelItem
                        {
                            Id = channel.Id,
                            ChannelHtml = html
                        } );
                    }
                }
                else
                {
                    foreach ( var channel in channelQry )
                    {
                        if ( !channel.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                        {
                            continue;
                        }
                        var channelMergeFields = new Dictionary<string, object>( mergeFields );
                        channelMergeFields.Add( "InteractionChannel", channel );

                        string html = channel.ChannelListTemplate.IsNotNullOrWhiteSpace() ?
                            channel.ChannelListTemplate.ResolveMergeFields( channelMergeFields ) :
                            LavaService.RenderTemplate( defaultLavaTemplate, channelMergeFields ).Text;

                        channelItems.Add( new ChannelItem
                        {
                            Id = channel.Id,
                            ChannelHtml = html
                        } );
                    }
                }

                rptChannel.DataSource = channelItems;
                rptChannel.DataBind();
            }
        }

        /// <summary>
        /// Get the person through query list or context.
        /// </summary>
        private int? GetPersonId()
        {
            int? personId = PageParameter( "PersonId" ).AsIntegerOrNull();
            if ( !personId.HasValue )
            {
                var person = ContextEntity<Person>();
                if ( person != null )
                {
                    personId = person.Id;
                }
            }

			if ( !personId.HasValue )
			{
	            int? personAliasId = PageParameter( "PersonAliasId" ).AsIntegerOrNull();
	            if ( personAliasId.HasValue )
	            {
	                personId = new PersonAliasService( new RockContext() ).GetPersonId( personAliasId.Value );
	            }
			}

            return personId;
        }

        #endregion
    }

    #region Helper Classes

    /// <summary>
    /// Class for binding repeater
    /// </summary>
    public class ChannelItem
    {
        public int Id { get; set; }

        public string ChannelHtml { get; set; }
    }

    #endregion
}