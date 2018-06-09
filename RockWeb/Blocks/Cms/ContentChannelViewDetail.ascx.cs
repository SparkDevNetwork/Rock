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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Transactions;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Block to display a specific content channel item.
    /// </summary>
    [DisplayName( "Content Channel View Detail" )]
    [Category( "CMS" )]
    [Description( "Block to display a specific content channel item." )]

    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this content channel item block.", false, category: "CustomSetting" )]

    [ContentChannelField( "Content Channel", "Limits content channel items to a specific channel, or leave blank to leave unrestricted.", false, "", category: "CustomSetting" )]
    [TextField( "Content Channel Query Parameter", @"
Specify the URL parameter to use to determine which Content Channel Item to show, or leave blank to use whatever the first parameter is. 
The type of the value will determine how the content channel item will be determined as follows:

Integer - ContentChannelItem Id
String - ContentChannelItem Slug
Guid - ContentChannelItem Guid

", required: false, category: "CustomSetting" )]

    [CodeEditorField( "Lava Template", "The template to use when formatting the content channel item.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, false, category: "CustomSetting", defaultValue: @"
<h1>{{ Item.Title }}</h1>
{{ Item.Content }}" )]

    [IntegerField( "Output Cache Duration", "Number of seconds to cache the resolved output. Only cache the output if you are not personalizing the output based on current user, current page, or any other merge field value.", required: false, key: "OutputCacheDuration", category: "CustomSetting" )]
    [BooleanField( "Set Page Title", "Determines if the block should set the page title with the channel name or content item.", category: "CustomSetting" )]

    [BooleanField( "Log Interactions", category: "CustomSetting" )]
    [BooleanField( "Write Interaction Only If Individual Logged In", "Set to true to only write interactions for logged in users, or set to false to write interactions for both logged in and anonymous users.", category: "CustomSetting" )]

    [WorkflowTypeField( "Workflow Type", "The workflow type to launch when the content is viewed.", category: "CustomSetting" )]
    [BooleanField( "Launch Workflow Only If Individual Logged In", "Set to true to only launch a workflow for logged in users, or set to false to launch for both logged in and anonymous users.", category: "CustomSetting" )]
    [EnumField( "Launch Workflow Condition", "", typeof( LaunchWorkflowCondition ), defaultValue: "1", category: "CustomSetting" )]

    [TextField( "Meta Description Attribute", required: false, category: "CustomSetting" )]
    [TextField( "Open Graph Type", required: false, category: "CustomSetting" )]
    [TextField( "Open Graph Title Attribute", required: false, category: "CustomSetting" )]
    [TextField( "Open Graph Description Attribute", required: false, category: "CustomSetting" )]
    [TextField( "Open Graph Image Attribute", required: false, category: "CustomSetting" )]
    [TextField( "Twitter Title Attribute", required: false, category: "CustomSetting" )]
    [TextField( "Twitter Description Attribute", required: false, category: "CustomSetting" )]
    [TextField( "Twitter Image Attribute", required: false, category: "CustomSetting" )]
    [TextField( "Twitter Card", required: false, defaultValue: "none", category: "CustomSetting" )]
    public partial class ContentChannelViewDetail : RockBlockCustomSettings
    {
        #region Fields

        /// <summary>
        /// The output cache key prefix
        /// </summary>
        private const string OUTPUT_CACHE_KEY_PREFIX = "Output_";

        /// <summary>
        /// The pagetitle cache key prefix
        /// </summary>
        private const string PAGETITLE_CACHE_KEY_PREFIX = "PageTitle_";

        /// <summary>
        /// The cache key to store a list of CacheKeys that have been used by this block
        /// </summary>
        private const string CACHEKEYS_CACHE_KEY = "CacheKeys";
        
        /// <summary>
        /// 
        /// </summary>
        private enum LaunchWorkflowCondition
        {
            Always = 0,
            OncePerPersonPerContentChannelItem = 1,
            OncePerPerson = 2
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowView();
            }
        }

        #endregion Base Control Methods

        #region Settings

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {
            pnlSettings.Visible = true;
            ddlContentChannel.Items.Clear();
            ddlContentChannel.Items.Add( new ListItem() );
            foreach ( var contentChannel in CacheContentChannel.All().OrderBy( a => a.Name ) )
            {
                ddlContentChannel.Items.Add( new ListItem( contentChannel.Name, contentChannel.Guid.ToString() ) );
            }

            ddlLaunchWorkflowCondition.Items.Clear();
            foreach ( LaunchWorkflowCondition launchWorkflowCondition in Enum.GetValues( typeof( LaunchWorkflowCondition ) ) )
            {
                ddlLaunchWorkflowCondition.Items.Add( new ListItem( launchWorkflowCondition.ConvertToString( true ), launchWorkflowCondition.ConvertToInt().ToString() ) );
            }

            var channelGuid = this.GetAttributeValue( "ContentChannel" ).AsGuidOrNull();
            ddlContentChannel.SetValue( channelGuid );
            UpdateSocialMediaDropdowns( channelGuid );

            tbContentChannelQueryParameter.Text = this.GetAttributeValue( "ContentChannelQueryParameter" );
            ceLavaTemplate.Text = this.GetAttributeValue( "LavaTemplate" );
            nbOutputCacheDuration.Text = this.GetAttributeValue( "OutputCacheDuration" );
            cbSetPageTitle.Checked = this.GetAttributeValue( "SetPageTitle" ).AsBoolean();

            cbLogInteractions.Checked = this.GetAttributeValue( "LogInteractions" ).AsBoolean();
            cbWriteInteractionOnlyIfIndividualLoggedIn.Checked = this.GetAttributeValue( "WriteInteractionOnlyIfIndividualLoggedIn" ).AsBoolean();

            var rockContext = new RockContext();

            // Workflow
            Guid? workflowTypeGuid = this.GetAttributeValue( "WorkflowType" ).AsGuidOrNull();
            if ( workflowTypeGuid.HasValue )
            {
                wtpWorkflowType.SetValue( new WorkflowTypeService( rockContext ).GetNoTracking( workflowTypeGuid.Value ) );
            }
            else
            {
                wtpWorkflowType.SetValue( null );
            }

            cbLaunchWorkflowOnlyIfIndividualLoggedIn.Checked = this.GetAttributeValue( "LaunchWorkflowOnlyIfIndividualLoggedIn" ).AsBoolean();
            ddlLaunchWorkflowCondition.SetValue( this.GetAttributeValue( "LaunchWorkflowCondition" ) );

            // Social Media
            ddlMetaDescriptionAttribute.SetValue( this.GetAttributeValue( "MetaDescriptionAttribute" ) );
            ddlOpenGraphType.SetValue( this.GetAttributeValue( "OpenGraphType" ) );
            ddlOpenGraphTitleAttribute.SetValue( this.GetAttributeValue( "OpenGraphTitleAttribute" ) );
            ddlOpenGraphDescriptionAttribute.SetValue( this.GetAttributeValue( "OpenGraphDescriptionAttribute" ) );
            ddlOpenGraphImageAttribute.SetValue( this.GetAttributeValue( "OpenGraphImageAttribute" ) );

            ddlTwitterTitleAttribute.SetValue( this.GetAttributeValue( "TwitterTitleAttribute" ) );
            ddlTwitterDescriptionAttribute.SetValue( this.GetAttributeValue( "TwitterDescriptionAttribute" ) );
            ddlTwitterImageAttribute.SetValue( this.GetAttributeValue( "TwitterImageAttribute" ) );
            rblTwitterCard.SetValue( this.GetAttributeValue( "TwitterCard" ) );

            mdSettings.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdSettings_SaveClick( object sender, EventArgs e )
        {
            this.SetAttributeValue( "ContentChannel", ddlContentChannel.SelectedValue );
            this.SetAttributeValue( "ContentChannelQueryParameter", tbContentChannelQueryParameter.Text );
            this.SetAttributeValue( "LavaTemplate", ceLavaTemplate.Text );
            this.SetAttributeValue( "OutputCacheDuration", nbOutputCacheDuration.Text );
            this.SetAttributeValue( "SetPageTitle", cbSetPageTitle.Checked.ToString() );
            this.SetAttributeValue( "LogInteractions", cbLogInteractions.Checked.ToString() );
            this.SetAttributeValue( "WriteInteractionOnlyIfIndividualLoggedIn", cbWriteInteractionOnlyIfIndividualLoggedIn.Checked.ToString() );
            int? selectedWorkflowTypeId = wtpWorkflowType.SelectedValue.AsIntegerOrNull();
            Guid? selectedWorkflowTypeGuid = null;
            if ( selectedWorkflowTypeId.HasValue )
            {
                selectedWorkflowTypeGuid = CacheWorkflowType.Get( selectedWorkflowTypeId.Value ).Guid;
            }

            this.SetAttributeValue( "WorkflowType", selectedWorkflowTypeGuid.ToString() );
            this.SetAttributeValue( "LaunchWorkflowOnlyIfIndividualLoggedIn", cbLaunchWorkflowOnlyIfIndividualLoggedIn.Checked.ToString() );
            this.SetAttributeValue( "LaunchWorkflowCondition", ddlLaunchWorkflowCondition.SelectedValue );
            this.SetAttributeValue( "MetaDescriptionAttribute", ddlMetaDescriptionAttribute.SelectedValue );
            this.SetAttributeValue( "OpenGraphType", ddlOpenGraphType.SelectedValue );
            this.SetAttributeValue( "OpenGraphTitleAttribute", ddlOpenGraphTitleAttribute.SelectedValue );
            this.SetAttributeValue( "OpenGraphDescriptionAttribute", ddlOpenGraphDescriptionAttribute.SelectedValue );
            this.SetAttributeValue( "OpenGraphImageAttribute", ddlOpenGraphImageAttribute.SelectedValue );
            this.SetAttributeValue( "TwitterTitleAttribute", ddlTwitterTitleAttribute.SelectedValue );
            this.SetAttributeValue( "TwitterDescriptionAttribute", ddlTwitterDescriptionAttribute.SelectedValue );
            this.SetAttributeValue( "TwitterImageAttribute", ddlTwitterImageAttribute.SelectedValue );
            this.SetAttributeValue( "TwitterCard", rblTwitterCard.SelectedValue );

            SaveAttributeValues();

            var cacheKeys = GetCacheItem( CACHEKEYS_CACHE_KEY ) as HashSet<string>;
            if ( cacheKeys != null )
            {
                foreach ( var cacheKey in cacheKeys )
                {
                    RemoveCacheItem( cacheKey );
                }
            }

            RemoveCacheItem( CACHEKEYS_CACHE_KEY );

            mdSettings.Hide();
            pnlSettings.Visible = false;

            // reload the page to make sure we have a clean load
            NavigateToCurrentPageReference();
        }

        #endregion Settings

        #region Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlContentChannel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlContentChannel_SelectedIndexChanged( object sender, EventArgs e )
        {
            var channelGuid = ddlContentChannel.SelectedValue.AsGuidOrNull();
            UpdateSocialMediaDropdowns( channelGuid );
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Shows the view.
        /// </summary>
        private void ShowView()
        {
            int? outputCacheDuration = GetAttributeValue( "OutputCacheDuration" ).AsIntegerOrNull();

            string outputContents = null;
            string pageTitle = null;

            var contentChannelItemParameterValue = GetContentChannelItemParameterValue();
            string outputCacheKey = OUTPUT_CACHE_KEY_PREFIX + contentChannelItemParameterValue;
            string pageTitleCacheKey = PAGETITLE_CACHE_KEY_PREFIX + contentChannelItemParameterValue;

            if ( outputCacheDuration.HasValue && outputCacheDuration.Value > 0 )
            {
                outputContents = GetCacheItem( outputCacheKey ) as string;
                pageTitle = GetCacheItem( pageTitleCacheKey ) as string;
            }

            bool setPageTitle = GetAttributeValue( "SetPageTitle" ).AsBoolean();

            if ( outputContents == null )
            {
                ContentChannelItem contentChannelItem = GetContentChannelItem( GetContentChannelItemParameterValue() );

                if ( contentChannelItem == null )
                {
                    return;
                }

                // if a Channel was specified, verify that the ChannelItem is part of the channel
                var channelGuid = this.GetAttributeValue( "ContentChannel" ).AsGuidOrNull();
                if ( channelGuid.HasValue )
                {
                    var channel = CacheContentChannel.Get( channelGuid.Value );
                    if ( channel != null )
                    {
                        if ( contentChannelItem.ContentChannelId != channel.Id )
                        {
                            return;
                        }
                    }
                }

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
                mergeFields.Add( "RockVersion", Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );
                mergeFields.Add( "Item", contentChannelItem );

                string metaDescriptionValue = GetMetaValueFromAttribute( this.GetAttributeValue( "MetaDescriptionAttribute" ), contentChannelItem );

                if ( !string.IsNullOrWhiteSpace( metaDescriptionValue ) )
                {
                    // remove default meta description
                    RockPage.Header.Description = metaDescriptionValue.SanitizeHtml( true );
                }

                AddHtmlMeta( "og:type", this.GetAttributeValue( "OpenGraphType" ) );
                AddHtmlMeta( "og:title", GetMetaValueFromAttribute( this.GetAttributeValue( "OpenGraphTitleAttribute" ), contentChannelItem ) );
                AddHtmlMeta( "og:description", GetMetaValueFromAttribute( this.GetAttributeValue( "OpenGraphDescriptionAttribute" ), contentChannelItem ) );
                AddHtmlMeta( "og:image", GetMetaValueFromAttribute( this.GetAttributeValue( "OpenGraphImageAttribute" ), contentChannelItem ) );
                AddHtmlMeta( "twitter:title", GetMetaValueFromAttribute( this.GetAttributeValue( "TwitterTitleAttribute" ), contentChannelItem ) );
                AddHtmlMeta( "twitter:description", GetMetaValueFromAttribute( this.GetAttributeValue( "TwitterDescriptionAttribute" ), contentChannelItem ) );
                AddHtmlMeta( "twitter:image", GetMetaValueFromAttribute( this.GetAttributeValue( "TwitterImageAttribute" ), contentChannelItem ) );
                AddHtmlMeta( "twitter:card", this.GetAttributeValue( "TwitterCard" ) );

                string lavaTemplate = this.GetAttributeValue( "LavaTemplate" );
                string enabledLavaCommands = this.GetAttributeValue( "EnabledLavaCommands" );
                outputContents = lavaTemplate.ResolveMergeFields( mergeFields, enabledLavaCommands );

                if ( setPageTitle )
                {
                    pageTitle = contentChannelItem.Title;
                }

                if ( outputCacheDuration.HasValue && outputCacheDuration.Value > 0 )
                {
                    var cacheKeys = GetCacheItem( CACHEKEYS_CACHE_KEY ) as HashSet<string> ?? new HashSet<string>();
                    cacheKeys.Add( outputCacheKey );
                    cacheKeys.Add( pageTitleCacheKey );
                    AddCacheItem( CACHEKEYS_CACHE_KEY, cacheKeys );
                    AddCacheItem( outputCacheKey, outputContents, outputCacheDuration.Value );
                    AddCacheItem( pageTitleCacheKey, pageTitle, outputCacheDuration.Value );
                }
            }

            phContent.Controls.Add( new LiteralControl( outputContents ) );

            if ( setPageTitle && pageTitle != null )
            {
                RockPage.PageTitle = pageTitle;
                RockPage.BrowserTitle = string.Format( "{0} | {1}", pageTitle, RockPage.Site.Name );
                RockPage.Header.Title = string.Format( "{0} | {1}", pageTitle, RockPage.Site.Name );
            }

            LaunchWorkflow();

            LaunchInteraction();
        }

        /// <summary>
        /// Gets the content channel item using the first page parameter or ContentChannelQueryParameter
        /// </summary>
        /// <returns></returns>
        private static ContentChannelItem GetContentChannelItem( string contentChannelItemKey )
        {
            ContentChannelItem contentChannelItem = null;

            if ( string.IsNullOrEmpty( contentChannelItemKey ) )
            {
                // nothing specified, so don't show anything
                return null;
            }

            // look up the ContentChannelItem from either the Id, Guid, or Slug depending on the datatype of the ContentChannelQueryParameter value
            int? contentChannelItemId = contentChannelItemKey.AsIntegerOrNull();
            Guid? contentChannelItemGuid = contentChannelItemKey.AsGuidOrNull();

            var rockContext = new RockContext();
            if ( contentChannelItemId.HasValue )
            {
                contentChannelItem = new ContentChannelItemService( rockContext ).Get( contentChannelItemId.Value );
            }
            else if ( contentChannelItemGuid.HasValue )
            {
                contentChannelItem = new ContentChannelItemService( rockContext ).Get( contentChannelItemGuid.Value );
            }
            else
            {
                contentChannelItem = new ContentChannelItemService( rockContext ).Queryable().Where( a => a.ContentChannelItemSlugs.Any( s => s.Slug == contentChannelItemKey ) ).FirstOrDefault();
            }

            return contentChannelItem;
        }

        /// <summary>
        /// Gets the content channel item parameter using the first page parameter or ContentChannelQueryParameter
        /// </summary>
        /// <returns></returns>
        private string GetContentChannelItemParameterValue()
        {
            string contentChannelItemKey = null;

            // Determine the ContentChannelItem from the ContentChannelQueryParameter or the first parameter
            string contentChannelQueryParameter = this.GetAttributeValue( "ContentChannelQueryParameter" );
            if ( !string.IsNullOrEmpty( contentChannelQueryParameter ) )
            {
                contentChannelItemKey = this.PageParameter( contentChannelQueryParameter );
            }
            else
            {
                contentChannelItemKey = this.PageParameters().Select( a => a.Value.ToString() ).FirstOrDefault();
            }

            return contentChannelItemKey;
        }

        /// <summary>
        /// Launches the interaction if configured
        /// </summary>
        private void LaunchInteraction()
        {
            bool logInteractions = this.GetAttributeValue( "LogInteractions" ).AsBoolean();
            if ( !logInteractions )
            {
                return;
            }

            bool writeInteractionOnlyIfIndividualLoggedIn = this.GetAttributeValue( "WriteInteractionOnlyIfIndividualLoggedIn" ).AsBoolean();
            if ( writeInteractionOnlyIfIndividualLoggedIn && this.CurrentPerson == null )
            {
                // don't log interaction if WriteInteractionOnlyIfIndividualLoggedIn = true and nobody is logged in
                return;
            }

            var contentChannelItem = GetContentChannelItem( GetContentChannelItemParameterValue() );

            var interactionTransaction = new InteractionTransaction(
                CacheDefinedValue.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_CONTENTCHANNEL.AsGuid() ),
                contentChannelItem.ContentChannel,
                contentChannelItem );

            interactionTransaction.InteractionSummary = contentChannelItem.Title;

            interactionTransaction.Enqueue();
        }

        /// <summary>
        /// Launches the workflow if configured
        /// </summary>
        private void LaunchWorkflow()
        {
            // Check to see if a workflow should be launched when viewed
            CacheWorkflowType workflowType = null;
            Guid? workflowTypeGuid = GetAttributeValue( "WorkflowType" ).AsGuidOrNull();
            if ( !workflowTypeGuid.HasValue )
            {
                return;
            }

            workflowType = CacheWorkflowType.Get( workflowTypeGuid.Value );

            if ( workflowType == null || ( workflowType.IsActive != true ) )
            {
                return;
            }

            bool launchWorkflowOnlyIfIndividualLoggedIn = this.GetAttributeValue( "LaunchWorkflowOnlyIfIndividualLoggedIn" ).AsBoolean();
            if ( launchWorkflowOnlyIfIndividualLoggedIn && this.CurrentPerson == null )
            {
                // don't launch a workflow if LaunchWorkflowOnlyIfIndividualLoggedIn = true and nobody is logged in
                return;
            }

            var launchWorkflowCondition = this.GetAttributeValue( "LaunchWorkflowCondition" ).ConvertToEnum<LaunchWorkflowCondition>();

            if ( launchWorkflowCondition != LaunchWorkflowCondition.Always && this.CurrentPerson == null )
            {
                // don't launch a workflow if LaunchWorkflowOnlyIfIndividualLoggedIn = true and nobody is logged in
                return;
            }

            var contentChannelItem = GetContentChannelItem( this.GetContentChannelItemParameterValue() );

            // use BlockUserPreference to store whether the Workflow was already launched depending on LaunchWorkflowCondition
            string alreadyLaunchedKey = null;
            if ( launchWorkflowCondition == LaunchWorkflowCondition.OncePerPersonPerContentChannelItem )
            {
                alreadyLaunchedKey = string.Format( "WorkflowLaunched_{0}_{1}", workflowType.Id, contentChannelItem.Id );
            }
            else if ( launchWorkflowCondition == LaunchWorkflowCondition.OncePerPerson )
            {
                alreadyLaunchedKey = string.Format( "WorkflowLaunched_{0}", workflowType.Id );
            }

            if ( alreadyLaunchedKey != null )
            {
                var alreadyLaunched = this.GetBlockUserPreference( alreadyLaunchedKey ).AsBooleanOrNull();
                if ( alreadyLaunched == true )
                {
                    return;
                }
                else
                {
                    this.SetBlockUserPreference( alreadyLaunchedKey, true.ToString(), true );
                }
            }

            var workflowAttributeValues = new Dictionary<string, string>();
            workflowAttributeValues.Add( "ContentChannelItem", contentChannelItem.Guid.ToString() );

            LaunchWorkflowTransaction launchWorkflowTransaction;
            if ( this.CurrentPersonId.HasValue )
            {
                workflowAttributeValues.Add( "Person", this.CurrentPerson.Guid.ToString() );
                launchWorkflowTransaction = new Rock.Transactions.LaunchWorkflowTransaction<Person>( workflowType.Id, null, this.CurrentPersonId.Value );
            }
            else
            {
                launchWorkflowTransaction = new Rock.Transactions.LaunchWorkflowTransaction( workflowType.Id, null );
            }

            if ( workflowAttributeValues != null )
            {
                launchWorkflowTransaction.WorkflowAttributeValues = workflowAttributeValues;
            }

            launchWorkflowTransaction.InitiatorPersonAliasId = this.CurrentPersonAliasId;
            launchWorkflowTransaction.Enqueue();
        }

        /// <summary>
        /// Adds the HTML meta from attribute value.
        /// </summary>
        /// <param name="metaName">Name of the meta.</param>
        /// <param name="metaContent">Content of the meta.</param>
        private void AddHtmlMeta( string metaName, string metaContent )
        {
            if ( string.IsNullOrEmpty( metaContent ) )
            {
                return;
            }

            RockPage.Header.Controls.Add( new HtmlMeta
            {
                Name = metaName,
                Content = metaContent
            } );
        }

        /// <summary>
        /// Gets the meta value from attribute.
        /// </summary>
        /// <param name="attributeComputedKey">The attribute computed key.</param>
        /// <param name="contentChannelItem">The content channel item.</param>
        /// <returns>
        /// a string value
        /// </returns>
        private string GetMetaValueFromAttribute( string attributeComputedKey, ContentChannelItem contentChannelItem )
        {
            if ( string.IsNullOrEmpty( attributeComputedKey ) )
            {
                return null;
            }

            string attributeEntityType = attributeComputedKey.Split( '^' )[0].ToString() ?? "C";
            string attributeKey = attributeComputedKey.Split( '^' )[1].ToString() ?? string.Empty;

            string attributeValue = string.Empty;

            object mergeObject;

            if ( attributeEntityType == "C" )
            {
                mergeObject = CacheContentChannel.Get( contentChannelItem.ContentChannelId );
            }
            else
            {
                mergeObject = contentChannelItem;
            }

            // use Lava to get the Attribute value formatted for the MetaValue, and specify the Url param in case the Attribute supports rendering the value as a Url (for example, Image)
            string metaTemplate = string.Format( "{{{{ mergeObject | Attribute:'{0}':'Url' }}}}", attributeKey );

            string resolvedValue = metaTemplate.ResolveMergeFields( new Dictionary<string, object> { { "mergeObject", mergeObject } } );

            return resolvedValue;
        }

        /// <summary>
        /// Updates the social media dropdowns.
        /// </summary>
        /// <param name="channelGuid">The channel unique identifier.</param>
        private void UpdateSocialMediaDropdowns( Guid? channelGuid )
        {
            List<CacheAttribute> channelAttributes = new List<CacheAttribute>();
            List<CacheAttribute> itemAttributes = new List<CacheAttribute>();

            if ( channelGuid.HasValue )
            {
                var rockContext = new RockContext();
                var channel = new ContentChannelService( rockContext ).GetNoTracking( channelGuid.Value );

                // add channel attributes
                channel.LoadAttributes();
                channelAttributes = channel.Attributes.Select( a => a.Value ).ToList();

                // add item attributes
                AttributeService attributeService = new AttributeService( rockContext );
                itemAttributes = attributeService.GetByEntityTypeId( new ContentChannelItem().TypeId, false ).AsQueryable()
                                        .Where( a => (
                                                a.EntityTypeQualifierColumn.Equals( "ContentChannelTypeId", StringComparison.OrdinalIgnoreCase ) &&
                                                a.EntityTypeQualifierValue.Equals( channel.ContentChannelTypeId.ToString() )
                                            ) || (
                                                a.EntityTypeQualifierColumn.Equals( "ContentChannelId", StringComparison.OrdinalIgnoreCase ) &&
                                                a.EntityTypeQualifierValue.Equals( channel.Id.ToString() )
                                            ) )
                                        .OrderByDescending( a => a.EntityTypeQualifierColumn )
                                        .ThenBy( a => a.Order )
                                        .ToCacheAttributeList();
            }

            RockDropDownList[] attributeDropDowns = new RockDropDownList[]
            {
                ddlMetaDescriptionAttribute,
                ddlOpenGraphTitleAttribute,
                ddlOpenGraphDescriptionAttribute,
                ddlOpenGraphImageAttribute,
                ddlTwitterTitleAttribute,
                ddlTwitterDescriptionAttribute,
                ddlTwitterImageAttribute,
                ddlMetaDescriptionAttribute
            };

            RockDropDownList[] attributeDropDownsImage = new RockDropDownList[]
            {
                ddlOpenGraphImageAttribute,
                ddlTwitterImageAttribute,
            };

            foreach ( var attributeDropDown in attributeDropDowns )
            {
                attributeDropDown.Items.Clear();
                attributeDropDown.Items.Add( new ListItem() );
                foreach ( var attribute in channelAttributes )
                {
                    string computedKey = "C^" + attribute.Key;
                    if ( attributeDropDownsImage.Contains( attributeDropDown ) )
                    {
                        if ( attribute.FieldType.Name == "Image" )
                        {
                            attributeDropDown.Items.Add( new ListItem( "Channel: " + attribute.Name, computedKey ) );
                        }
                    }
                    else
                    {
                        attributeDropDown.Items.Add( new ListItem( "Channel: " + attribute.Name, computedKey ) );
                    }
                }

                // get all the possible Item attributes for items in this Content Channel and add those as options too
                foreach ( var attribute in itemAttributes.DistinctBy( a => a.Key ).ToList() )
                {
                    string computedKey = "I^" + attribute.Key;
                    if ( attributeDropDownsImage.Contains( attributeDropDown ) )
                    {
                        if ( attribute.FieldType.Name == "Image" )
                        {
                            attributeDropDown.Items.Add( new ListItem( "Item: " + attribute.Name, computedKey ) );
                        }
                    }
                    else
                    {
                        attributeDropDown.Items.Add( new ListItem( "Item: " + attribute.Name, computedKey ) );
                    }
                }
            }
        }

        #endregion Methods
    }
}