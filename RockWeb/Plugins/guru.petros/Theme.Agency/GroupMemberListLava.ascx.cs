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
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Caching;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using DotLiquid;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Field.Types;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Guru.Petros.Theme.Agency
{
    /// <summary>
    /// Block to display content items, html, xml, or transformed xml based on a SQL query or stored procedure.
    /// </summary>
    [DisplayName( "Group Member List Lava" )]
    [Category( "Petros Guru" )]
    [Description( "Block to display a list of group members using a lava template" )]

    // Block Properties
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this content channel block.", false, order: 0 )]
    [LinkedPage( "Detail Page", "The page to navigate to for details.", false, "", "", 1 )]
    [BooleanField( "Enable Legacy Global Attribute Lava", "This should only be enabled if your lava is using legacy Global Attributes. Enabling this option, will negatively affect the performance of this block.", false, "", 2, "SupportLegacy" )]
	[GroupField( "Group", "The group who's members will be listed", true ,"", "", 3 )]
    // Custom Settings
 
    [CodeEditorField( "Template", "The template to use when formatting the list of items.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 600, false, @"", "CustomSetting" )]
    [IntegerField( "Item Cache Duration", "Number of seconds to cache the content items returned by the selected filter.", false, 3600, "CustomSetting", 0, "CacheDuration" )]
    [IntegerField( "Output Cache Duration", "Number of seconds to cache the resolved output. Only cache the output if you are not personalizing the output based on current user, current page, or any other merge field value.", false, 0, "CustomSetting", 0, "OutputCacheDuration" )]
   
    public partial class GroupMemberListLava : RockBlockCustomSettings
    {
        #region Fields

        private readonly string CONTENT_CACHE_KEY = "Content";
        private readonly string TEMPLATE_CACHE_KEY = "Template";
        private readonly string OUTPUT_CACHE_KEY = "Output";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the duration of the item cache.
        /// </summary>
        /// <value>
        /// The duration of the item cache.
        /// </value>
        public int? ItemCacheDuration { get; set; }

        /// <summary>
        /// Gets or sets the duration of the output cache.
        /// </summary>
        /// <value>
        /// The duration of the output cache.
        /// </value>
        public int? OutputCacheDuration { get; set; }

        /// <summary>
        /// Gets or sets the group's unique identifier.
        /// </summary>
        /// <value>
        /// The group's unique identifier.
        /// </value>
        public Guid? GroupGuid { get; set; }

        /// <summary>
        /// Gets the settings tool tip.
        /// </summary>
        /// <value>
        /// The settings tool tip.
        /// </value>
        public override string SettingsToolTip
        {
            get
            {
                return "Edit Criteria";
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            GroupGuid = ViewState["GroupGuid"] as Guid?;

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            ItemCacheDuration = GetAttributeValue( "CacheDuration" ).AsIntegerOrNull();
            OutputCacheDuration = GetAttributeValue( "OutputCacheDuration" ).AsIntegerOrNull();

            this.BlockUpdated += ContentDynamic_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            Button btnTrigger = new Button();
            btnTrigger.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            btnTrigger.ID = "rock-config-cancel-trigger";
            btnTrigger.Click += btnTrigger_Click;
            pnlEditModal.Controls.Add( btnTrigger );

            AsyncPostBackTrigger trigger = new AsyncPostBackTrigger();
            trigger.ControlID = "rock-config-cancel-trigger";
            trigger.EventName = "Click";
            upnlContent.Triggers.Add( trigger );
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
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["GroupGUid"] = GroupGuid;

            return base.SaveViewState();
        }

        #endregion

        #region Events

        void ContentDynamic_BlockUpdated( object sender, EventArgs e )
        {
            FlushCacheItem( CONTENT_CACHE_KEY );
            FlushCacheItem( TEMPLATE_CACHE_KEY );
            FlushCacheItem( OUTPUT_CACHE_KEY );

            ShowView();
        }

        void btnTrigger_Click( object sender, EventArgs e )
        {
            mdEdit.Hide();
            pnlEditModal.Visible = false;

            ShowView();
        }

        protected void ddlGroup_SelectedIndexChanged( object sender, EventArgs e )
        {
            GroupGuid = ddlGroup.SelectedValue.AsGuidOrNull();
            ShowEdit();
        }

        protected void lbSave_Click( object sender, EventArgs e )
        {

            if ( !Page.IsValid )
            {
                return;
            }

            SetAttributeValue( "Group", ddlGroup.SelectedValue );
            SetAttributeValue( "Template", ceTemplate.Text );
            SetAttributeValue( "CacheDuration", ( nbItemCacheDuration.Text.AsIntegerOrNull() ?? 0 ).ToString() );
            SetAttributeValue( "OutputCacheDuration", ( nbOutputCacheDuration.Text.AsIntegerOrNull() ?? 0 ).ToString() );

            var ppFieldType = new PageReferenceFieldType();
            SetAttributeValue( "DetailPage", ppFieldType.GetEditValue( ppDetailPage, null ) );

            SaveAttributeValues();

            FlushCacheItem( CONTENT_CACHE_KEY );
            FlushCacheItem( TEMPLATE_CACHE_KEY );
            FlushCacheItem( OUTPUT_CACHE_KEY );

            mdEdit.Hide();
            pnlEditModal.Visible = false;
            upnlContent.Update();

            ShowView();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {
            // Switch does not automatically initialize again after a partial-postback.  This script 
            // looks for any switch elements that have not been initialized and re-intializes them.
            string script = @"
$(document).ready(function() {
    $('.switch > input').each( function () {
        $(this).parent().switch('init');
    });
});
";
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "toggle-switch-init", script, true );

            pnlEditModal.Visible = true;
            upnlContent.Update();
            mdEdit.Show();

            var rockContext = new RockContext();
            ddlGroup.DataSource = new GroupService( rockContext ).Queryable()
                .OrderBy( c => c.Name )
                .Select( c => new { c.Guid, c.Name } )
                .ToList();
            ddlGroup.DataBind();
            ddlGroup.Items.Insert( 0, new ListItem( "", "" ) );
            ddlGroup.SetValue( GetAttributeValue( "Group" ) );
            GroupGuid = ddlGroup.SelectedValue.AsGuidOrNull();

            ceTemplate.Text = GetAttributeValue( "Template" );
            nbItemCacheDuration.Text = GetAttributeValue( "CacheDuration" );
            nbOutputCacheDuration.Text = GetAttributeValue( "OutputCacheDuration" );

            var ppFieldType = new PageReferenceFieldType();
            ppFieldType.SetEditValue( ppDetailPage, null, GetAttributeValue( "DetailPage" ) );

            ShowEdit();

            upnlContent.Update();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void ShowView()
        {

			GroupGuid = GetAttributeValue("Group").AsGuidOrNull();

			nbContentError.Visible = false;
            upnlContent.Update();

            string outputContents = null;

            if ( OutputCacheDuration.HasValue && OutputCacheDuration.Value > 0 )
            {
                outputContents = GetCacheItem( OUTPUT_CACHE_KEY ) as string;
            }

            if ( outputContents == null )
            {
                var pageRef = CurrentPageReference;
                pageRef.Parameters.AddOrReplace( "Page", "PageNum" );

                Dictionary<string, object> linkedPages = new Dictionary<string, object>();
                linkedPages.Add( "DetailPage", LinkedPageRoute( "DetailPage" ) );


                var mergeFieldOptions = new Rock.Lava.CommonMergeFieldsOptions();
                mergeFieldOptions.GetLegacyGlobalMergeFields = GetAttributeValue( "SupportLegacy" ).AsBoolean();
                var commonMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, mergeFieldOptions );

                var mergeFields = new Dictionary<string, object>( commonMergeFields );
                mergeFields.Add( "LinkedPages", linkedPages );
                mergeFields.Add( "RockVersion", Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );

				var group = new Group();
				var members = new List<GroupMember>();

				if(GroupGuid.HasValue)
				{
					var rockContext = new RockContext();
					group = new GroupService(rockContext).GetByGuid(GroupGuid.Value);
					members = new GroupMemberService(rockContext).Queryable("GroupRole,Person", true)
							.Where(m =>
							   m.GroupId == group.Id
							   )
							.OrderBy(m => m.GroupRole.Order)
							.ToList();

				}

				mergeFields.Add("Group", group);
				mergeFields.Add("Members", members);



				// TODO: When support for "Person" is not supported anymore (should use "CurrentPerson" instead), remove this line
				mergeFields.AddOrIgnore( "Person", CurrentPerson );

                var template = GetTemplate();

                if ( template.Registers.ContainsKey( "EnabledCommands" ) )
                {
                    template.Registers["EnabledCommands"] = GetAttributeValue( "EnabledLavaCommands" );
                }
                else // this should never happen
                {
                    template.Registers.Add( "EnabledCommands", GetAttributeValue( "EnabledLavaCommands" ) );
                }

                outputContents = template.Render( Hash.FromDictionary( mergeFields ) );

                if ( OutputCacheDuration.HasValue && OutputCacheDuration.Value > 0 )
                {
                    var cacheItemPolicy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddSeconds( OutputCacheDuration.Value ) };
                    AddCacheItem( OUTPUT_CACHE_KEY, outputContents, cacheItemPolicy );
                }
            }

            phContent.Controls.Add( new LiteralControl( outputContents ) );
        }

        private Template GetTemplate()
        {
            Template template = null;

            try
            {

                // only load from the cache if a cacheDuration was specified
                if ( ItemCacheDuration.HasValue && ItemCacheDuration.Value > 0 )
                {
                    template = GetCacheItem( TEMPLATE_CACHE_KEY ) as Template;
                }

                if ( template == null )
                {
                    template = Template.Parse( GetAttributeValue( "Template" ) );

                    if ( ItemCacheDuration.HasValue && ItemCacheDuration.Value > 0 )
                    {
                        var cacheItemPolicy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddSeconds( ItemCacheDuration.Value ) };
                        AddCacheItem( TEMPLATE_CACHE_KEY, template, cacheItemPolicy );
                    }
                }
            }
            catch ( Exception ex )
            {
                template = Template.Parse( string.Format( "Lava error: {0}", ex.Message ) );
            }

            return template;
        }


        /// <summary>
        /// Shows the edit.
        /// </summary>
        public void ShowEdit()
        {

        }

        /// <summary>
        /// Sets the list value.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="value">The value.</param>
        private void SetListValue( ListControl listControl, string value )
        {
            foreach ( ListItem item in listControl.Items )
            {
                item.Selected = ( item.Value == value );
            }
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// 
        /// </summary>
        public class Pagination : DotLiquid.Drop
        {

            /// <summary>
            /// Gets or sets the item count.
            /// </summary>
            /// <value>
            /// The item count.
            /// </value>
            public int ItemCount { get; set; }

            /// <summary>
            /// Gets or sets the size of the page.
            /// </summary>
            /// <value>
            /// The size of the page.
            /// </value>
            public int PageSize { get; set; }

            /// <summary>
            /// Gets or sets the current page.
            /// </summary>
            /// <value>
            /// The current page.
            /// </value>
            public int CurrentPage { get; set; }

            /// <summary>
            /// Gets the previous page.
            /// </summary>
            /// <value>
            /// The previous page.
            /// </value>
            public int PreviousPage
            {
                get
                {
                    CurrentPage = CurrentPage > TotalPages ? TotalPages : CurrentPage;
                    return ( CurrentPage > 1 ) ? CurrentPage - 1 : -1;
                }
            }

            /// <summary>
            /// Gets the next page.
            /// </summary>
            /// <value>
            /// The next page.
            /// </value>
            public int NextPage
            {
                get
                {
                    CurrentPage = CurrentPage > TotalPages ? TotalPages : CurrentPage;
                    return ( CurrentPage < TotalPages ) ? CurrentPage + 1 : -1;
                }
            }

            /// <summary>
            /// Gets the total pages.
            /// </summary>
            /// <value>
            /// The total pages.
            /// </value>
            public int TotalPages
            {
                get
                {
                    if ( PageSize == 0 )
                    {
                        return 1;
                    }
                    else
                    {
                        return Convert.ToInt32( Math.Abs( ItemCount / PageSize ) ) +
                            ( ( ItemCount % PageSize ) > 0 ? 1 : 0 );
                    }
                }
            }

            public string UrlTemplate { get; set; }

            /// <summary>
            /// Gets or sets the pages.
            /// </summary>
            /// <value>
            /// The pages.
            /// </value>
            public List<PaginationPage> Pages
            {
                get
                {
                    var pages = new List<PaginationPage>();

                    for ( int i = 1; i <= TotalPages; i++ )
                    {
                        pages.Add( new PaginationPage( UrlTemplate, i ) );
                    }

                    return pages;
                }
            }

            /// <summary>
            /// Gets the current page items.
            /// </summary>
            /// <param name="allItems">All items.</param>
            /// <returns></returns>
            public List<GroupMember> GetCurrentPageItems( List<GroupMember> allItems )
            {
                if ( PageSize > 0 )
                {
                    CurrentPage = CurrentPage > TotalPages ? TotalPages : CurrentPage;
                    return allItems.Skip( ( CurrentPage - 1 ) * PageSize ).Take( PageSize ).ToList();
                }

                return allItems;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public class PaginationPage : DotLiquid.Drop
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PaginationPage"/> class.
            /// </summary>
            /// <param name="urlTemplate">The URL template.</param>
            /// <param name="pageNumber">The page number.</param>
            public PaginationPage( string urlTemplate, int pageNumber )
            {
                UrlTemplate = urlTemplate;
                PageNumber = pageNumber;
            }

            private string UrlTemplate { get; set; }

            /// <summary>
            /// Gets the page number.
            /// </summary>
            /// <value>
            /// The page number.
            /// </value>
            public int PageNumber { get; private set; }

            /// <summary>
            /// Gets the page URL.
            /// </summary>
            /// <value>
            /// The page URL.
            /// </value>
            public string PageUrl
            {
                get
                {
                    if ( !string.IsNullOrWhiteSpace( UrlTemplate ) && UrlTemplate.Contains( "{0}" ) )
                    {
                        return string.Format( UrlTemplate, PageNumber );
                    }
                    else
                    {
                        return PageNumber.ToString();
                    }
                }
            }

            #endregion

        }
    }

}