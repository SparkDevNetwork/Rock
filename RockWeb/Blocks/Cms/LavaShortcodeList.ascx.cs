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
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Lava;
using Rock.Lava.Shortcodes;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Core;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Lava Shortcode List" )]
    [Category( "CMS" )]
    [Description( "Lists Lava Shortcode in the system." )]

    #region Block Attributes

    [LinkedPage(
       "Detail Page",
        Key = AttributeKey.DetailPage )]

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "C26C7979-81C1-4A20-A167-35415CD7FED3" )]
    public partial class LavaShortcodeList : RockBlock
    {

        public bool canAddEditDelete = false;

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Attribute Keys

        #region User Preference Keys

        private static class UserPreferenceKey
        {
            public const string CategoryId = "CategoryId";
            public const string ShowInactive = "ShowInactive";
        }

        #endregion User Preference Keys

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            canAddEditDelete = IsUserAuthorized( Authorization.EDIT );

            if ( !Page.IsPostBack )
            {
                LoadLavaShortcodes();
                LoadShortcodeCategories();
                LoadUserPreferences();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadLavaShortcodes();
        }

        /// <summary>
        /// Handles the Click event of the btnAddShortcut control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddShortcut_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, "LavaShortcodeId", 0 );
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            LinkButton btn = ( LinkButton ) sender;
            RepeaterItem item = ( RepeaterItem ) btn.NamingContainer;
            HiddenField hfShortcodeId = ( HiddenField ) item.FindControl( "hfShortcodeId" );

            NavigateToLinkedPage( AttributeKey.DetailPage, "LavaShortcodeId", hfShortcodeId.ValueAsInt() );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            LinkButton btn = ( LinkButton ) sender;
            RepeaterItem item = ( RepeaterItem ) btn.NamingContainer;
            HiddenField hfShortcodeId = ( HiddenField ) item.FindControl( "hfShortcodeId" );

            var rockContext = new RockContext();
            LavaShortcodeService lavaShortcodeService = new LavaShortcodeService( rockContext );
            LavaShortcode lavaShortcode = lavaShortcodeService.Get( hfShortcodeId.ValueAsInt() );

            if ( lavaShortcode != null )
            {
                // unregister the shortcode
                LavaService.DeregisterShortcode( lavaShortcode.TagName );

                lavaShortcodeService.Delete( lavaShortcode );
                rockContext.SaveChanges();
            }

            LoadLavaShortcodes();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptShortcodes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptShortcodes_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                LavaShortcode dataItem = ( LavaShortcode ) e.Item.DataItem;
                e.Item.FindControl( "btnEdit" ).Visible = canAddEditDelete;
                e.Item.FindControl( "btnDelete" ).Visible = canAddEditDelete && !dataItem.IsSystem;
                e.Item.FindControl( "divViewPanel" ).Visible = !canAddEditDelete;

                var shortcode = e.Item.DataItem as LavaShortcode;

                var sbItems = new StringBuilder();
                foreach( var cat in shortcode.Categories )
                {
                    sbItems.AppendLine( $"<span class='label label-info'>{cat}</span>" );
                }

                var itemLitCategories = e.Item.FindControl( "litCategories" ) as Literal;
                itemLitCategories.Text = sbItems.ToString();

                // Add special logic for shortcodes in c# assemblies
                if ( shortcode.Id == -1 )
                {
                    // This is a shortcode from a c# assembly
                    e.Item.FindControl( "btnView" ).Visible = false;
                    e.Item.FindControl( "divEditPanel" ).Visible = false;
                    var lMessages = ( Literal ) e.Item.FindControl( "lMessages" );

                    if ( lMessages != null )
                    {
                        lMessages.Text = "<div class='margin-t-md alert alert-info'>This shortcode is defined in code (versus being stored in the database) and therefore can not be modified.</div>";
                    }
                }
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the swShowActive control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void swShowInactive_CheckedChanged( object sender, EventArgs e )
        {
            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( UserPreferenceKey.ShowInactive, swShowInactive.Checked.ToTrueFalse() );
            preferences.Save();

            LoadLavaShortcodes();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlCategoryFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlCategoryFilter_SelectedIndexChanged( object sender, EventArgs e )
        {
            var selectedItem = ddlCategoryFilter.SelectedItem;

            if ( selectedItem != null )
            {
                var preferences = GetBlockPersonPreferences();

                preferences.SetValue( UserPreferenceKey.CategoryId, selectedItem.Value );
                preferences.Save();

                LoadLavaShortcodes();
            }
        }
        #endregion

        #region Internal Methods

        /// <summary>
        /// Loads the shortcodes.
        /// </summary>
        private void LoadLavaShortcodes()
        {
            if ( LavaService.RockLiquidIsEnabled )
            {
                LoadShortcodes();
                return;
            }

            var lavaShortcodeService = new LavaShortcodeService( new RockContext() );

            var lavaShortcodes = lavaShortcodeService.Queryable();

            if ( swShowInactive.Checked == false )
            {
                lavaShortcodes = lavaShortcodes.Where( s => s.IsActive == true );
            }

            // To list the items from the database as we now need to add
            // items in c# assemblies
            var shortcodeList = lavaShortcodes.ToList();

            // Start with block items
            var shortcodeTypes = Rock.Reflection.FindTypes( typeof( ILavaShortcode ) ).Values.ToList();

            foreach ( var shortcode in shortcodeTypes )
            {
                var shortcodeMetadataAttribute = shortcode.GetCustomAttributes( typeof( LavaShortcodeMetadataAttribute ), true ).FirstOrDefault() as LavaShortcodeMetadataAttribute;

                // ignore shortcodes with no metadata
                if ( shortcodeMetadataAttribute == null )
                {
                    continue;
                }

                try
                {
                    var shortcodeInstance = Activator.CreateInstance( shortcode ) as ILavaShortcode;
                    var shortcodeType = shortcodeInstance.ElementType;


                    shortcodeList.Add( new LavaShortcode
                    {
                        Id = -1,
                        Name = shortcodeMetadataAttribute.Name,
                        TagName = shortcodeMetadataAttribute.TagName,
                        TagType = ( shortcodeType == LavaShortcodeTypeSpecifier.Inline ) ? TagType.Inline : TagType.Block,
                        IsActive = true,
                        IsSystem = true,
                        Description = shortcodeMetadataAttribute.Description,
                        Documentation = shortcodeMetadataAttribute.Documentation,
                        Categories = GetCategoriesFromMetaData( shortcodeMetadataAttribute )
                    } );

                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                }
            }

            // Now filter them based on any selected filter.
            var selectedCategoryId = ddlCategoryFilter.SelectedValue.AsInteger();
            if ( selectedCategoryId > 0 )
            {
                shortcodeList = shortcodeList.Where( s => s.Categories.Any( v => v.Id == selectedCategoryId ) ).ToList();
            }

            rptShortcodes.DataSource = shortcodeList.OrderBy( s => s.Name );
            rptShortcodes.DataBind();
        }

        /// <summary>
        /// Gets the categories from meta data. If the metaData does not include any
        /// categories, that's still ok.  In that case this will just return an empty
        /// list of categories.
        /// </summary>
        /// <param name="metaData">The meta data.</param>
        /// <returns>List&lt;Category&gt;.</returns>
        private List<Category> GetCategoriesFromMetaData( LavaShortcodeMetadataAttribute metaData )
        {
            List<Category> categories = new List<Category>();

            var categoryService = new CategoryService( new RockContext() );
            var shortcodeCategoryGuids = metaData.Categories.Split( ',' ).AsGuidList();
            shortcodeCategoryGuids.ForEach( g => categories.Add( categoryService.Get( g ) ) );

            return categories;
        }

        /// <summary>
        /// Loads the shortcode categories.
        /// </summary>
        private void LoadShortcodeCategories()
        {
            ddlCategoryFilter.Items.Add( new ListItem { Text = "All Shortcodes", Value = "0" } );

            var lavaShortcodeCategories = CategoryCache.All()
                .Where( t => t.EntityTypeId == EntityTypeCache.GetId<Rock.Model.LavaShortcode>() )
                .OrderBy( t => t.Order )
                .ThenBy( t => t.Name );

            if ( lavaShortcodeCategories != null )
            {
                foreach ( var cat in lavaShortcodeCategories )
                {
                    ddlCategoryFilter.Items.Add( new ListItem { Text = cat.Name, Value = cat.Id.ToString() } );
                }
            }
        }
        #endregion

        #region RockLiquid Lava implementation

        /// <summary>
        /// Loads the shortcodes.
        /// </summary>
        private void LoadShortcodes()
        {
            LavaShortcodeService lavaShortcodeService = new LavaShortcodeService( new RockContext() );
            var lavaShortcodes = lavaShortcodeService.Queryable();

            if ( swShowInactive.Checked == false )
            {
                lavaShortcodes = lavaShortcodes.Where( s => s.IsActive == true );
            }

            // To list the items from the database as we now need to add
            // items in c# assemblies
            var shortcodeList = lavaShortcodes.ToList();

            // Start with block items
            foreach ( var shortcodeInCode in Rock.Reflection.FindTypes( typeof( Rock.Lava.Shortcodes.RockLavaShortcodeBlockBase ) ).ToList() )
            {
                var shortcode = shortcodeInCode.Value;
                var shortcodeMetadataAttribute = shortcode.GetCustomAttributes( typeof( LavaShortcodeMetadataAttribute ), true ).FirstOrDefault() as LavaShortcodeMetadataAttribute;

                // ignore shortcodes with no metadata
                if ( shortcodeMetadataAttribute == null )
                {
                    continue;
                }

                shortcodeList.Add( new LavaShortcode
                {
                    Id = -1,
                    Name = shortcodeMetadataAttribute.Name,
                    TagName = shortcodeMetadataAttribute.TagName,
                    TagType = TagType.Block,
                    IsActive = true,
                    IsSystem = true,
                    Description = shortcodeMetadataAttribute.Description,
                    Documentation = shortcodeMetadataAttribute.Documentation,
                    Categories = GetCategoriesFromMetaData( shortcodeMetadataAttribute )
                } );
            }

            // Next add inline items
            foreach ( var shortcodeInCode in Rock.Reflection.FindTypes( typeof( Rock.Lava.Shortcodes.RockLavaShortcodeBase ) ).ToList() )
            {
                var shortcode = shortcodeInCode.Value;
                var shortcodeMetadataAttribute = shortcode.GetCustomAttributes( typeof( LavaShortcodeMetadataAttribute ), true ).FirstOrDefault() as LavaShortcodeMetadataAttribute;

                // ignore shortcodes with no metadata
                if ( shortcodeMetadataAttribute == null )
                {
                    continue;
                }

                shortcodeList.Add( new LavaShortcode
                {
                    Id = -1,
                    Name = shortcodeMetadataAttribute.Name,
                    TagName = shortcodeMetadataAttribute.TagName,
                    TagType = TagType.Inline,
                    IsActive = true,
                    IsSystem = true,
                    Description = shortcodeMetadataAttribute.Description,
                    Documentation = shortcodeMetadataAttribute.Documentation,
                } );
            }

            // Now filter them based on any selected filter.
            var selectedCategoryId = ddlCategoryFilter.SelectedValue.AsInteger();
            if ( selectedCategoryId > 0 )
            {
                shortcodeList = shortcodeList.Where( s => s.Categories.Any( v => v.Id == selectedCategoryId ) ).ToList();
            }

            rptShortcodes.DataSource = shortcodeList.OrderBy( s => s.Name );
            rptShortcodes.DataBind();
        }

        /// <summary>
        /// Loads the user preferences.
        /// </summary>
        private void LoadUserPreferences()
        {
            var preferences = GetBlockPersonPreferences();
            var categoryId = preferences.GetValue( UserPreferenceKey.CategoryId ).AsInteger();
            var currentCategoryVal = ddlCategoryFilter.SelectedValue.AsInteger();
            var showInactive = preferences.GetValue( UserPreferenceKey.ShowInactive ).AsBoolean();

            var loadData = false;
            if ( categoryId > 0 && currentCategoryVal != categoryId )
            {
                ddlCategoryFilter.SelectedValue = categoryId.ToString();
                loadData = true;
            }

            if ( swShowInactive.Checked != showInactive )
            {
                swShowInactive.Checked = showInactive;
                loadData = true;
            }

            if ( loadData )
            {
                LoadLavaShortcodes();
            }
        }

        #endregion
    }
}