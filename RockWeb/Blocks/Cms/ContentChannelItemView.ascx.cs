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
using Rock.Web.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Transactions;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Field.Types;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Block to display a specific content channel item.
    /// </summary>
    [DisplayName( "Content Channel Item View" )]
    [Category( "CMS" )]
    [Description( "Block to display a specific content channel item." )]

    #region Block Attributes

    [LavaCommandsField(
        "Enabled Lava Commands",
        Description = "The Lava commands that should be enabled for this content channel item block.",
        IsRequired = false,
        Key = AttributeKey.EnabledLavaCommands )]

    [ContentChannelField(
        "Content Channel",
        Description = "Limits content channel items to a specific channel.",
        IsRequired = true,
        DefaultValue = "",
        Category = "CustomSetting",
        Key = AttributeKey.ContentChannel )]

    [EnumsField(
        "Status",
        Description = "Include items with the following status.",
        EnumSourceType = typeof( ContentChannelItemStatus ),
        IsRequired = false,
        DefaultValue = "2",
        Category = "CustomSetting",
        Key = AttributeKey.Status)]

    [TextField(
        "Content Channel Query Parameter",
        Description = ContentChannelQueryParameterDescription,
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.ContentChannelQueryParameter )]

    [CodeEditorField(
        "Lava Template",
        Description = "The template to use when formatting the content channel item.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        Category = "CustomSetting",
        DefaultValue = LavaTemplateDefaultValue,
        Key = AttributeKey.LavaTemplate )]

    [IntegerField(
        "Output Cache Duration",
        Description = OutputCacheDurationDescription,
        IsRequired = false,
        Key = AttributeKey.OutputCacheDuration,
        Category = "CustomSetting" )]

    [IntegerField(
        "Item Cache Duration",
        Description = "Number of seconds to cache the content item specified by the parameter.",
        IsRequired = false,
        DefaultIntegerValue = 3600,
        Category = "CustomSetting",
        Order = 0,
        Key = AttributeKey.ItemCacheDuration )]

    [CustomCheckboxListField(
        "Cache Tags",
        Description = "Cached tags are used to link cached content so that it can be expired as a group",
        IsRequired = false,
        Key = AttributeKey.CacheTags,
        Category = "CustomSetting" )]

    [BooleanField(
        "Merge Content",
        Description = "Should the content data and attribute values be merged using the Lava template engine.",
        DefaultBooleanValue = false,
        Category = "CustomSetting",
        Key = AttributeKey.MergeContent )]

    [BooleanField(
        "Set Page Title",
        Description = "Determines if the block should set the page title with the channel name or content item.",
        Category = "CustomSetting",
        Key = AttributeKey.SetPageTitle )]

    [LinkedPage(
        "Detail Page",
        Description = "Page used to view a content item.",
        Order = 1,
        Category = "CustomSetting",
        Key = AttributeKey.DetailPage )]

    [BooleanField(
        "Log Interactions",
        Category = "CustomSetting",
        Key = AttributeKey.LogInteractions )]

    [BooleanField(
        "Write Interaction Only If Individual Logged In",
        Description = "Set to true to only write interactions for logged in users, or set to false to write interactions for both logged in and anonymous users.",
        Category = "CustomSetting",
        Key = AttributeKey.WriteInteractionOnlyIfIndividualLoggedIn )]

    [WorkflowTypeField(
        "Workflow Type",
        Description = "The workflow type to launch when the content is viewed.",
        Category = "CustomSetting",
        Key = AttributeKey.WorkflowType )]

    [BooleanField(
        "Launch Workflow Only If Individual Logged In",
        Description = "Set to true to only launch a workflow for logged in users, or set to false to launch for both logged in and anonymous users.",
        Category = "CustomSetting",
        Key = AttributeKey.LaunchWorkflowOnlyIfIndividualLoggedIn )]

    [EnumField(
        "Launch Workflow Condition",
        EnumSourceType = typeof( LaunchWorkflowCondition ),
        DefaultValue = "1",
        Category = "CustomSetting",
        Key = AttributeKey.LaunchWorkflowCondition )]

    [TextField(
        "Meta Description Attribute",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.MetaDescriptionAttribute )]

    [TextField(
        "Open Graph Type",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.OpenGraphType )]

    [TextField(
        "Open Graph Title Attribute",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.OpenGraphTitleAttribute )]

    [TextField(
        "Open Graph Description Attribute",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.OpenGraphDescriptionAttribute )]

    [TextField(
        "Open Graph Image Attribute",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.OpenGraphImageAttribute )]

    [TextField(
        "Twitter Title Attribute",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.TwitterTitleAttribute )]

    [TextField(
        "Twitter Description Attribute",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.TwitterDescriptionAttribute )]

    [TextField(
        "Twitter Image Attribute",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.TwitterImageAttribute )]

    [TextField(
        "Twitter Card",
        IsRequired = false,
        DefaultValue = "none",
        Category = "CustomSetting",
        Key = AttributeKey.TwitterCard )]

    #endregion Block Attributes
    public partial class ContentChannelItemView : RockBlockCustomSettings
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string ContentChannel = "ContentChannel";
            public const string Status = "Status";
            public const string ContentChannelQueryParameter = "ContentChannelQueryParameter";
            public const string LavaTemplate = "LavaTemplate";
            public const string OutputCacheDuration = "OutputCacheDuration";
            public const string ItemCacheDuration = "ItemCacheDuration";
            public const string CacheTags = "CacheTags";
            public const string MergeContent = "MergeContent";
            public const string SetPageTitle = "SetPageTitle";
            public const string DetailPage = "DetailPage";
            public const string LogInteractions = "LogInteractions";
            public const string WriteInteractionOnlyIfIndividualLoggedIn = "WriteInteractionOnlyIfIndividualLoggedIn";
            public const string WorkflowType = "WorkflowType";
            public const string LaunchWorkflowCondition = "LaunchWorkflowCondition";
            public const string LaunchWorkflowOnlyIfIndividualLoggedIn = "LaunchWorkflowOnlyIfIndividualLoggedIn";
            public const string MetaDescriptionAttribute = "MetaDescriptionAttribute";
            public const string OpenGraphType = "OpenGraphType";
            public const string OpenGraphTitleAttribute = "OpenGraphTitleAttribute";
            public const string OpenGraphDescriptionAttribute = "OpenGraphDescriptionAttribute";
            public const string OpenGraphImageAttribute = "OpenGraphImageAttribute";
            public const string TwitterTitleAttribute = "TwitterTitleAttribute";
            public const string TwitterDescriptionAttribute = "TwitterDescriptionAttribute";
            public const string TwitterImageAttribute = "TwitterImageAttribute";
            public const string TwitterCard = "TwitterCard";
            public const string EnabledLavaCommands = "EnabledLavaCommands";
        }

        #endregion Attribute Keys

        #region constants

        protected const string LavaTemplateDefaultValue = @"
<h1>{{ Item.Title }}</h1>
{{ Item.Content }}";

        private const string ContentChannelQueryParameterDescription = @"
Specify the URL parameter to use to determine which Content Channel Item to show, or leave blank to use whatever the first parameter is.
The type of the value will determine how the content channel item will be determined as follows:

Integer - ContentChannelItem Id
String - ContentChannelItem Slug
Guid - ContentChannelItem Guid

";

        private const string OutputCacheDurationDescription = @"Number of seconds to cache the resolved output. Only cache the output if you are not personalizing the output based on current user, current page, or any other merge field value.";

        #endregion constants

        #region Fields

        /// <summary>
        /// The output cache key prefix
        /// </summary>
        private const string OUTPUT_CACHE_KEY_PREFIX = "Output_";

        /// <summary>
        /// The item cache key prefix
        /// </summary>
        private const string ITEM_CACHE_KEY_PREFIX = "Item_";

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
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            // Create a hidden button that will show the view if they cancel the Settings modal
            Button btnTrigger = new Button();
            btnTrigger.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            btnTrigger.ID = "rock-config-cancel-trigger";
            btnTrigger.Click += btnTrigger_Click;
            btnTrigger.Style[HtmlTextWriterStyle.Display] = "none";
            pnlSettings.Controls.Add( btnTrigger );

            AsyncPostBackTrigger trigger = new AsyncPostBackTrigger();
            trigger.ControlID = "rock-config-cancel-trigger";
            trigger.EventName = "Click";
            upnlContent.Triggers.Add( trigger );

            base.OnInit( e );
        }

        /// <summary>
        /// Handles the Click event of the btnTrigger control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnTrigger_Click( object sender, EventArgs e )
        {
            mdSettings.Hide();
            pnlSettings.Visible = false;

            ShowView();
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
                ShowView();
            }
        }

        /// <summary>
        /// Shows or hides the workflow settings controls based on the selection in the workflowpicker.
        /// </summary>
        protected void ShowHideControls()
        {
            if ( wtpWorkflowType.SelectedValue == "0" )
            {
                cbLaunchWorkflowOnlyIfIndividualLoggedIn.Visible = false;
                ddlLaunchWorkflowCondition.Visible = false;
            }
            else
            {
                cbLaunchWorkflowOnlyIfIndividualLoggedIn.Visible = true;
                ddlLaunchWorkflowCondition.Visible = true;
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
            foreach ( var contentChannel in ContentChannelCache.All().OrderBy( a => a.Name ) )
            {
                ddlContentChannel.Items.Add( new ListItem( contentChannel.Name, contentChannel.Guid.ToString() ) );
            }

            ddlLaunchWorkflowCondition.Items.Clear();
            foreach ( LaunchWorkflowCondition launchWorkflowCondition in Enum.GetValues( typeof( LaunchWorkflowCondition ) ) )
            {
                ddlLaunchWorkflowCondition.Items.Add( new ListItem( launchWorkflowCondition.ConvertToString( true ), launchWorkflowCondition.ConvertToInt().ToString() ) );
            }

            var channelGuid = this.GetAttributeValue( AttributeKey.ContentChannel ).AsGuidOrNull();
            ddlContentChannel.SetValue( channelGuid );
            UpdateSocialMediaDropdowns( channelGuid );

            cblStatus.BindToEnum<ContentChannelItemStatus>();
            foreach ( string status in GetAttributeValue( AttributeKey.Status ).SplitDelimitedValues() )
            {
                var li = cblStatus.Items.FindByValue( status );
                if ( li != null )
                {
                    li.Selected = true;
                }
            }

            var ppFieldType = new PageReferenceFieldType();
            ppFieldType.SetEditValue( ppDetailPage, null, GetAttributeValue( AttributeKey.DetailPage ) );
            tbContentChannelQueryParameter.Text = this.GetAttributeValue( AttributeKey.ContentChannelQueryParameter );
            ceLavaTemplate.Text = this.GetAttributeValue( AttributeKey.LavaTemplate );
            nbOutputCacheDuration.Text = this.GetAttributeValue( AttributeKey.OutputCacheDuration );
            nbItemCacheDuration.Text = this.GetAttributeValue( AttributeKey.ItemCacheDuration );

            DefinedValueService definedValueService = new DefinedValueService( new RockContext() );
            cblCacheTags.DataSource = definedValueService.GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.CACHE_TAGS.AsGuid() ).Select( v => v.Value ).ToList();
            cblCacheTags.DataBind();
            string[] selectedCacheTags = this.GetAttributeValue( AttributeKey.CacheTags ).SplitDelimitedValues();
            foreach ( ListItem cacheTag in cblCacheTags.Items )
            {
                cacheTag.Selected = selectedCacheTags.Contains( cacheTag.Value );
            }

            cbSetPageTitle.Checked = this.GetAttributeValue( AttributeKey.SetPageTitle ).AsBoolean();
            cbMergeContent.Checked = GetAttributeValue( AttributeKey.MergeContent ).AsBoolean();

            if ( this.GetAttributeValue( AttributeKey.LogInteractions ).AsBoolean() )
            {
                cbLogInteractions.Checked = true;
                cbWriteInteractionOnlyIfIndividualLoggedIn.Visible = true;
                cbWriteInteractionOnlyIfIndividualLoggedIn.Checked = this.GetAttributeValue( AttributeKey.WriteInteractionOnlyIfIndividualLoggedIn ).AsBoolean();
            }
            else
            {
                cbLogInteractions.Checked = false;
                cbWriteInteractionOnlyIfIndividualLoggedIn.Visible = false;
                cbWriteInteractionOnlyIfIndividualLoggedIn.Checked = false;
            }

            var rockContext = new RockContext();

            // Workflow
            Guid? workflowTypeGuid = this.GetAttributeValue( AttributeKey.WorkflowType ).AsGuidOrNull();
            if ( workflowTypeGuid.HasValue )
            {
                wtpWorkflowType.SetValue( new WorkflowTypeService( rockContext ).GetNoTracking( workflowTypeGuid.Value ) );
            }
            else
            {
                wtpWorkflowType.SetValue( null );
            }

            ShowHideControls();

            cbLaunchWorkflowOnlyIfIndividualLoggedIn.Checked = this.GetAttributeValue( AttributeKey.LaunchWorkflowOnlyIfIndividualLoggedIn ).AsBoolean();
            ddlLaunchWorkflowCondition.SetValue( this.GetAttributeValue( AttributeKey.LaunchWorkflowCondition ) );

            // Social Media
            ddlMetaDescriptionAttribute.SetValue( this.GetAttributeValue( AttributeKey.MetaDescriptionAttribute ) );
            ddlOpenGraphType.SetValue( this.GetAttributeValue( AttributeKey.OpenGraphType ) );
            ddlOpenGraphTitleAttribute.SetValue( this.GetAttributeValue( AttributeKey.OpenGraphTitleAttribute ) );
            ddlOpenGraphDescriptionAttribute.SetValue( this.GetAttributeValue( AttributeKey.OpenGraphDescriptionAttribute ) );
            ddlOpenGraphImageAttribute.SetValue( this.GetAttributeValue( AttributeKey.OpenGraphImageAttribute ) );

            ddlTwitterTitleAttribute.SetValue( this.GetAttributeValue( AttributeKey.TwitterTitleAttribute ) );
            ddlTwitterDescriptionAttribute.SetValue( this.GetAttributeValue( AttributeKey.TwitterDescriptionAttribute ) );
            ddlTwitterImageAttribute.SetValue( this.GetAttributeValue( AttributeKey.TwitterImageAttribute ) );
            ddlTwitterCard.SetValue( this.GetAttributeValue( AttributeKey.TwitterCard ) );

            mdSettings.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdSettings_SaveClick( object sender, EventArgs e )
        {
            this.SetAttributeValue( AttributeKey.ContentChannel, ddlContentChannel.SelectedValue );
            this.SetAttributeValue( AttributeKey.Status, cblStatus.SelectedValuesAsInt.AsDelimited( "," ) );
            var ppFieldType = new PageReferenceFieldType();
            this.SetAttributeValue( AttributeKey.DetailPage, ppFieldType.GetEditValue( ppDetailPage, null ) );
            this.SetAttributeValue( AttributeKey.ContentChannelQueryParameter, tbContentChannelQueryParameter.Text );
            this.SetAttributeValue( AttributeKey.LavaTemplate, ceLavaTemplate.Text );
            this.SetAttributeValue( AttributeKey.OutputCacheDuration, nbOutputCacheDuration.Text );
            this.SetAttributeValue( AttributeKey.ItemCacheDuration, nbItemCacheDuration.Text );
            this.SetAttributeValue( AttributeKey.CacheTags, cblCacheTags.SelectedValues.AsDelimited( "," ) );
            this.SetAttributeValue( AttributeKey.MergeContent, cbMergeContent.Checked.ToString() );
            this.SetAttributeValue( AttributeKey.SetPageTitle, cbSetPageTitle.Checked.ToString() );
            this.SetAttributeValue( AttributeKey.LogInteractions, cbLogInteractions.Checked.ToString() );
            this.SetAttributeValue( AttributeKey.WriteInteractionOnlyIfIndividualLoggedIn, cbWriteInteractionOnlyIfIndividualLoggedIn.Checked.ToString() );
            int? selectedWorkflowTypeId = wtpWorkflowType.SelectedValueAsId();
            Guid? selectedWorkflowTypeGuid = null;
            if ( selectedWorkflowTypeId.HasValue )
            {
                selectedWorkflowTypeGuid = WorkflowTypeCache.Get( selectedWorkflowTypeId.Value ).Guid;
            }

            this.SetAttributeValue( AttributeKey.WorkflowType, selectedWorkflowTypeGuid.ToString() );
            this.SetAttributeValue( AttributeKey.LaunchWorkflowOnlyIfIndividualLoggedIn, cbLaunchWorkflowOnlyIfIndividualLoggedIn.Checked.ToString() );
            this.SetAttributeValue( AttributeKey.LaunchWorkflowCondition, ddlLaunchWorkflowCondition.SelectedValue );
            this.SetAttributeValue( AttributeKey.MetaDescriptionAttribute, ddlMetaDescriptionAttribute.SelectedValue );
            this.SetAttributeValue( AttributeKey.OpenGraphType, ddlOpenGraphType.SelectedValue );
            this.SetAttributeValue( AttributeKey.OpenGraphTitleAttribute, ddlOpenGraphTitleAttribute.SelectedValue );
            this.SetAttributeValue( AttributeKey.OpenGraphDescriptionAttribute, ddlOpenGraphDescriptionAttribute.SelectedValue );
            this.SetAttributeValue( AttributeKey.OpenGraphImageAttribute, ddlOpenGraphImageAttribute.SelectedValue );
            this.SetAttributeValue( AttributeKey.TwitterTitleAttribute, ddlTwitterTitleAttribute.SelectedValue );
            this.SetAttributeValue( AttributeKey.TwitterDescriptionAttribute, ddlTwitterDescriptionAttribute.SelectedValue );
            this.SetAttributeValue( AttributeKey.TwitterImageAttribute, ddlTwitterImageAttribute.SelectedValue );
            this.SetAttributeValue( AttributeKey.TwitterCard, ddlTwitterCard.SelectedValue );

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

        /// <summary>
        /// Handles the SelectItem event of the wtpWorkflowType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void wtpWorkflowType_SelectItem( object sender, EventArgs e )
        {
            ShowHideControls();
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Shows the no data found.
        /// </summary>
        private void ShowNoDataFound()
        {
            if ( IsUserAuthorized( Rock.Security.Authorization.ADMINISTRATE ) )
            {
                nbAlert.Text = "404 - No Content. If you did not have Administrate permissions on this block, you would have gotten a real 404 page.";
            }
            else
            {
                this.Response.StatusCode = 404;
            }
        }

        /// <summary>
        /// Shows the view.
        /// </summary>
        private void ShowView()
        {
            int? outputCacheDuration = GetAttributeValue( AttributeKey.OutputCacheDuration ).AsIntegerOrNull();

            string outputContents = null;
            string pageTitle = null;

            var contentChannelItemParameterValue = GetContentChannelItemParameterValue();
            if ( string.IsNullOrEmpty( contentChannelItemParameterValue ) )
            {
                // No item specified, so don't show anything
                ShowNoDataFound();
                return;
            }

            string outputCacheKey = OUTPUT_CACHE_KEY_PREFIX + contentChannelItemParameterValue;
            string pageTitleCacheKey = PAGETITLE_CACHE_KEY_PREFIX + contentChannelItemParameterValue;

            if ( outputCacheDuration.HasValue && outputCacheDuration.Value > 0 )
            {
                outputContents = GetCacheItem( outputCacheKey ) as string;
                pageTitle = GetCacheItem( pageTitleCacheKey ) as string;
            }

            bool isMergeContentEnabled = GetAttributeValue( AttributeKey.MergeContent ).AsBoolean();
            bool setPageTitle = GetAttributeValue( AttributeKey.SetPageTitle ).AsBoolean();

            if ( outputContents == null )
            {
                ContentChannelItem contentChannelItem = GetContentChannelItem( contentChannelItemParameterValue );

                if ( contentChannelItem == null )
                {
                    ShowNoDataFound();
                    return;
                }

                if ( contentChannelItem.ContentChannel.RequiresApproval )
                {
                    var statuses = new List<ContentChannelItemStatus>();
                    foreach ( var status in ( GetAttributeValue( AttributeKey.Status ) ?? "2" ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                    {
                        var statusEnum = status.ConvertToEnumOrNull<ContentChannelItemStatus>();
                        if ( statusEnum != null )
                        {
                            statuses.Add( statusEnum.Value );
                        }
                    }

                    if ( !statuses.Contains( contentChannelItem.Status ) )
                    {
                        ShowNoDataFound();
                        return;
                    }
                }

                // if a Channel was specified, verify that the ChannelItem is part of the channel
                var channelGuid = this.GetAttributeValue( AttributeKey.ContentChannel ).AsGuidOrNull();
                if ( channelGuid.HasValue )
                {
                    var channel = ContentChannelCache.Get( channelGuid.Value );
                    if ( channel != null )
                    {
                        if ( contentChannelItem.ContentChannelId != channel.Id )
                        {
                            ShowNoDataFound();
                            return;
                        }
                    }
                }

                var commonMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );

                // Merge content and attribute fields if block is configured to do so.
                if ( isMergeContentEnabled )
                {
                    var itemMergeFields = new Dictionary<string, object>( commonMergeFields );

                    var enabledCommands = GetAttributeValue( AttributeKey.EnabledLavaCommands );

                    itemMergeFields.AddOrReplace( "Item", contentChannelItem );
                    contentChannelItem.Content = contentChannelItem.Content.ResolveMergeFields( itemMergeFields, enabledCommands );
                    contentChannelItem.LoadAttributes();
                    foreach ( var attributeValue in contentChannelItem.AttributeValues )
                    {
                        attributeValue.Value.Value = attributeValue.Value.Value.ResolveMergeFields( itemMergeFields, enabledCommands );
                    }
                }

                var mergeFields = new Dictionary<string, object>( commonMergeFields );

                mergeFields.Add( "RockVersion", Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );
                mergeFields.Add( "Item", contentChannelItem );
                int detailPage = 0;
                var page = PageCache.Get( GetAttributeValue( AttributeKey.DetailPage ) );
                if ( page != null )
                {
                    detailPage = page.Id;
                }

                mergeFields.Add( "DetailPage", detailPage );

                string metaDescriptionValue = GetMetaValueFromAttribute( this.GetAttributeValue( AttributeKey.MetaDescriptionAttribute ), contentChannelItem );

                if ( !string.IsNullOrWhiteSpace( metaDescriptionValue ) )
                {
                    // remove default meta description
                    RockPage.Header.Description = metaDescriptionValue.SanitizeHtml( true );
                }

                AddHtmlMetaProperty( "og:type", this.GetAttributeValue( AttributeKey.OpenGraphType ) );
                AddHtmlMetaProperty( "og:title", GetMetaValueFromAttribute( this.GetAttributeValue( AttributeKey.OpenGraphTitleAttribute ), contentChannelItem ) );
                AddHtmlMetaProperty( "og:description", GetMetaValueFromAttribute( this.GetAttributeValue( AttributeKey.OpenGraphDescriptionAttribute ), contentChannelItem ) );
                AddHtmlMetaProperty( "og:image", GetMetaValueFromAttribute( this.GetAttributeValue( AttributeKey.OpenGraphImageAttribute ), contentChannelItem ) );
                AddHtmlMetaName( "twitter:title", GetMetaValueFromAttribute( this.GetAttributeValue( AttributeKey.TwitterTitleAttribute ), contentChannelItem ) );
                AddHtmlMetaName( "twitter:description", GetMetaValueFromAttribute( this.GetAttributeValue( AttributeKey.TwitterDescriptionAttribute ), contentChannelItem ) );
                AddHtmlMetaName( "twitter:image", GetMetaValueFromAttribute( this.GetAttributeValue( AttributeKey.TwitterImageAttribute ), contentChannelItem ) );
                var twitterCard = this.GetAttributeValue( AttributeKey.TwitterCard );
                if ( twitterCard.IsNotNullOrWhiteSpace() && twitterCard != "none" )
                {
                    AddHtmlMetaName( "twitter:card", twitterCard );
                }
                string lavaTemplate = this.GetAttributeValue( AttributeKey.LavaTemplate );
                string enabledLavaCommands = this.GetAttributeValue( AttributeKey.EnabledLavaCommands );
                outputContents = lavaTemplate.ResolveMergeFields( mergeFields, enabledLavaCommands );

                if ( setPageTitle )
                {
                    pageTitle = contentChannelItem.Title;
                }

                if ( outputCacheDuration.HasValue && outputCacheDuration.Value > 0 )
                {
                    string cacheTags = GetAttributeValue( AttributeKey.CacheTags ) ?? string.Empty;
                    var cacheKeys = GetCacheItem( CACHEKEYS_CACHE_KEY ) as HashSet<string> ?? new HashSet<string>();
                    cacheKeys.Add( outputCacheKey );
                    cacheKeys.Add( pageTitleCacheKey );
                    AddCacheItem( CACHEKEYS_CACHE_KEY, cacheKeys, TimeSpan.MaxValue, cacheTags );
                    AddCacheItem( outputCacheKey, outputContents, outputCacheDuration.Value, cacheTags );

                    if ( pageTitle != null )
                    {
                        AddCacheItem( pageTitleCacheKey, pageTitle, outputCacheDuration.Value, cacheTags );
                    }
                }
            }

            lContentOutput.Text = outputContents;

            if ( setPageTitle && pageTitle != null )
            {
                RockPage.PageTitle = pageTitle;
                RockPage.BrowserTitle = string.Format( "{0} | {1}", pageTitle, RockPage.Site.Name );
                RockPage.Header.Title = string.Format( "{0} | {1}", pageTitle, RockPage.Site.Name );

                var pageBreadCrumb = RockPage.PageReference.BreadCrumbs.FirstOrDefault();
                if ( pageBreadCrumb != null )
                {
                    pageBreadCrumb.Name = RockPage.PageTitle;
                }
            }

            LaunchWorkflow();

            LaunchInteraction();
        }

        /// <summary>
        /// Gets the content channel item using the first page parameter or ContentChannelQueryParameter
        /// </summary>
        /// <returns></returns>
        private ContentChannelItem GetContentChannelItem( string contentChannelItemKey )
        {
            int? itemCacheDuration = GetAttributeValue( AttributeKey.ItemCacheDuration ).AsIntegerOrNull();
            Guid? contentChannelGuid = GetAttributeValue( AttributeKey.ContentChannel ).AsGuidOrNull();

            ContentChannelItem contentChannelItem = null;

            if ( string.IsNullOrEmpty( contentChannelItemKey ) )
            {
                // nothing specified, so don't show anything
                return null;
            }

            string itemCacheKey = ITEM_CACHE_KEY_PREFIX + contentChannelGuid + "_" + contentChannelItemKey;

            if ( itemCacheDuration.HasValue && itemCacheDuration.Value > 0 )
            {
                contentChannelItem = GetCacheItem( itemCacheKey ) as ContentChannelItem;
                if ( contentChannelItem != null )
                {
                    return contentChannelItem;
                }
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
                var contentChannelQuery = new ContentChannelItemService( rockContext ).Queryable();
                if ( contentChannelGuid.HasValue )
                {

                    contentChannelQuery = contentChannelQuery.Where( c => c.ContentChannel.Guid == contentChannelGuid );
                }

                contentChannelItem = contentChannelQuery
                    .Where( a => a.ContentChannelItemSlugs.Any( s => s.Slug == contentChannelItemKey ) )
                    .FirstOrDefault();
            }

            if ( contentChannelItem != null && itemCacheDuration.HasValue && itemCacheDuration.Value > 0 )
            {
                string cacheTags = GetAttributeValue( AttributeKey.CacheTags ) ?? string.Empty;
                var cacheKeys = GetCacheItem( CACHEKEYS_CACHE_KEY ) as HashSet<string> ?? new HashSet<string>();
                cacheKeys.Add( itemCacheKey );
                AddCacheItem( CACHEKEYS_CACHE_KEY, cacheKeys, TimeSpan.MaxValue, cacheTags );
                AddCacheItem( itemCacheKey, contentChannelItem, itemCacheDuration.Value, cacheTags );
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
            string contentChannelQueryParameter = this.GetAttributeValue( AttributeKey.ContentChannelQueryParameter );
            if ( !string.IsNullOrEmpty( contentChannelQueryParameter ) )
            {
                contentChannelItemKey = this.PageParameter( contentChannelQueryParameter );
            }
            else
            {
                var currentRoute = ( ( System.Web.Routing.Route ) Page.RouteData.Route );

                // First, look for the item key via the route/slug so that something like this
                // continues to work when an external system (such as Facebook) tacks a parameter
                // onto the URL like this:
                // https://community.rockrms.com/connect/a-dedicated-new-home-for-the-rock-community?fbclid=IwAR2VRUjhh...-9biFY

                // if this is the standard "page/{PageId}" route, don't grab the Item from the route since it would just be the pageId
                if ( currentRoute == null || currentRoute.Url != "page/{PageId}" )
                {
                    // if no specific Parameter was specified, get whatever the last Parameter in the Route is
                    var key = this.Page.RouteData.Values.Keys.LastOrDefault();
                    if ( key.IsNotNullOrWhiteSpace() )
                    {
                        contentChannelItemKey = this.Page.RouteData.Values[key].ToString();
                    }
                }
                else if ( Request.QueryString.HasKeys() )
                {
                    contentChannelItemKey = this.PageParameter( Request.QueryString.Keys[0] );
                }
            }

            return contentChannelItemKey;
        }

        /// <summary>
        /// Launches the interaction if configured
        /// </summary>
        private void LaunchInteraction()
        {
            // don't log visits from crawlers
            var clientType = InteractionDeviceType.GetClientType( Request.UserAgent );
            if ( clientType == "Crawler" )
            {
                return;
            }

            bool logInteractions = this.GetAttributeValue( AttributeKey.LogInteractions ).AsBoolean();
            if ( !logInteractions )
            {
                return;
            }

            bool writeInteractionOnlyIfIndividualLoggedIn = this.GetAttributeValue( AttributeKey.WriteInteractionOnlyIfIndividualLoggedIn ).AsBoolean();
            if ( writeInteractionOnlyIfIndividualLoggedIn && this.CurrentPerson == null )
            {
                // don't log interaction if WriteInteractionOnlyIfIndividualLoggedIn = true and nobody is logged in
                return;
            }

            var contentChannelItem = GetContentChannelItem( GetContentChannelItemParameterValue() );

            var interactionTransaction = new InteractionTransaction(
                DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_CONTENTCHANNEL.AsGuid() ),
                contentChannelItem.ContentChannel,
                contentChannelItem, new InteractionTransactionInfo { InteractionSummary = contentChannelItem.Title } );

            interactionTransaction.Enqueue();
        }

        /// <summary>
        /// Launches the workflow if configured
        /// </summary>
        private void LaunchWorkflow()
        {
            // Check to see if a workflow should be launched when viewed
            WorkflowTypeCache workflowType = null;
            Guid? workflowTypeGuid = GetAttributeValue( AttributeKey.WorkflowType ).AsGuidOrNull();
            if ( !workflowTypeGuid.HasValue )
            {
                return;
            }

            workflowType = WorkflowTypeCache.Get( workflowTypeGuid.Value );

            if ( workflowType == null || ( workflowType.IsActive != true ) )
            {
                return;
            }

            bool launchWorkflowOnlyIfIndividualLoggedIn = this.GetAttributeValue( AttributeKey.LaunchWorkflowOnlyIfIndividualLoggedIn ).AsBoolean();
            if ( launchWorkflowOnlyIfIndividualLoggedIn && this.CurrentPerson == null )
            {
                // don't launch a workflow if LaunchWorkflowOnlyIfIndividualLoggedIn = true and nobody is logged in
                return;
            }

            var launchWorkflowCondition = this.GetAttributeValue( AttributeKey.LaunchWorkflowCondition ).ConvertToEnum<LaunchWorkflowCondition>();

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
        /// Adds the HTML meta name from attribute value.
        /// </summary>
        /// <param name="metaName">Name of the meta.</param>
        /// <param name="metaContent">Content of the meta.</param>
        private void AddHtmlMetaName( string metaName, string metaContent )
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
        /// Adds the HTML meta property from attribute value.
        /// </summary>
        /// <param name="metaProperty">Property of the meta.</param>
        /// <param name="metaContent">Content of the meta.</param>
        private void AddHtmlMetaProperty( string metaProperty, string metaContent )
        {
            if ( string.IsNullOrEmpty( metaContent ) )
            {
                return;
            }

            HtmlMeta tag = new HtmlMeta();
            tag.Attributes.Add("property", metaProperty);
            tag.Content = metaContent;

            RockPage.Header.Controls.Add( tag );
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
                mergeObject = ContentChannelCache.Get( contentChannelItem.ContentChannelId );
            }
            else
            {
                mergeObject = contentChannelItem;
            }

            // use Lava to get the Attribute value formatted for the MetaValue, and specify the URL param in case the Attribute supports rendering the value as a URL (for example, Image)
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
            List<AttributeCache> channelAttributes = new List<AttributeCache>();
            List<AttributeCache> itemAttributes = new List<AttributeCache>();

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
                                        .ToAttributeCacheList();
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

        /// <summary>
        /// Handles the CheckedChanged event of the cbLogInteractions control.
        /// If log interactions is not enabled then don't allow write interaction setting.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbLogInteractions_CheckedChanged( object sender, EventArgs e )
        {
            if ( cbLogInteractions.Checked )
            {
                cbWriteInteractionOnlyIfIndividualLoggedIn.Visible = true;
                cbWriteInteractionOnlyIfIndividualLoggedIn.Checked = true;
            }
            else
            {
                cbWriteInteractionOnlyIfIndividualLoggedIn.Visible = false;
                cbWriteInteractionOnlyIfIndividualLoggedIn.Checked = false;
            }
        }
    }
}