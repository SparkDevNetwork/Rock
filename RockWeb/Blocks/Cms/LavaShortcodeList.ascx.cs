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
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using System.ComponentModel;
using Rock.Security;

using System.Web.UI.WebControls;
using Rock.Lava.Shortcodes;
using Rock.Lava;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName("Lava Shortcode List")]
    [Category("CMS")]
    [Description( "Lists Lava Shortcode in the system." )]

    #region Block Attributes

    [LinkedPage(
       "Detail Page",
        Key = AttributeKey.DetailPage )]

    #endregion Block Attributes
    public partial class LavaShortcodeList : RockBlock
    {

        public bool canAddEditDelete = false;

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Attribute Keys

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
                LavaEngine.CurrentEngine.DeregisterShortcode( lavaShortcode.TagName );

                lavaShortcodeService.Delete( lavaShortcode );
                rockContext.SaveChanges();
            }

            LoadLavaShortcodes();
        }

        protected void rptShortcodes_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem ) 
            {
                if ( !canAddEditDelete )
                {
                    e.Item.FindControl( "btnEdit" ).Visible = false;
                    e.Item.FindControl( "btnDelete" ).Visible = false;
                }

                LavaShortcode dataItem = (LavaShortcode)e.Item.DataItem;

                e.Item.FindControl( "divEditPanel" ).Visible = !dataItem.IsSystem;
                e.Item.FindControl( "divViewPanel" ).Visible = dataItem.IsSystem;

                // Add special logic for shortcodes in c# assemblies
                var shortcode = e.Item.DataItem as LavaShortcode;

                if( shortcode.Id == -1 )
                {
                    // This is a shortcode from a c# assembly
                    e.Item.FindControl( "btnView" ).Visible = false;
                    var lMessages = (Literal)e.Item.FindControl( "lMessages" );

                    if (lMessages != null )
                    {
                        lMessages.Text = "<div class='margin-t-md alert alert-info'>This shortcode is defined in code (verses being stored in the database) and therefore can not be modified.</div>";
                    }
                }
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglShowActive control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglShowActive_CheckedChanged( object sender, EventArgs e )
        {
            LoadLavaShortcodes();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Loads the shortcodes.
        /// </summary>
        private void LoadLavaShortcodes()
        {
            if ( LavaEngine.CurrentEngine.EngineType == LavaEngineTypeSpecifier.RockLiquid )
            {
                LoadShortcodes();
                return;
            }

            LavaShortcodeService lavaShortcodeService = new LavaShortcodeService( new RockContext() );
            var lavaShortcodes = lavaShortcodeService.Queryable();

            if ( tglShowActive.Checked )
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
                        Documentation = shortcodeMetadataAttribute.Documentation
                    } );

                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                }
            }

            rptShortcodes.DataSource = shortcodeList.ToList().OrderBy( s => s.Name );
            rptShortcodes.DataBind();
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

            if ( tglShowActive.Checked )
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
                    Documentation = shortcodeMetadataAttribute.Documentation
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
                    Documentation = shortcodeMetadataAttribute.Documentation
                } );
            }

            rptShortcodes.DataSource = shortcodeList.ToList().OrderBy( s => s.Name );
            rptShortcodes.DataBind();
        }

        #endregion
    }
}