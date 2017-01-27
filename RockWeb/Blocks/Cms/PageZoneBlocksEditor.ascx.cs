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
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using HtmlAgilityPack;
using Rock;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System.Web.Compilation;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Page/Zone Blocks Editor" )]
    [Category( "CMS" )]
    [Description( "Edit the Blocks for a Zone on a specific page/layout." )]
    public partial class PageZoneBlocksEditor : RockBlock, IDetailBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
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
                ShowDetail( PageParameter( "Page" ).AsInteger() );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlZones control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlZones_SelectedIndexChanged( object sender, EventArgs e )
        {
            ShowDetailForZone( ddlZones.SelectedValue );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="pageId">The page identifier.</param>
        public void ShowDetail( int pageId )
        {
            hfPageId.Value = pageId.ToString();
            var page = Rock.Web.Cache.PageCache.Read( pageId );

            this.Visible = page != null;
            LoadDropDowns();

            ShowDetailForZone( ddlZones.SelectedValue );
        }

        /// <summary>
        /// Shows the detail for zone.
        /// </summary>
        /// <param name="zoneName">Name of the zone.</param>
        private void ShowDetailForZone( string zoneName )
        {
            int pageId = hfPageId.Value.AsInteger();
            var page = Rock.Web.Cache.PageCache.Read( pageId );
            lZoneTitle.Text = string.Format( "{0} Zone", zoneName );
            lZoneIcon.Text = "<i class='fa fa-th-large'></i>";
            if ( page != null )
            {
                var zoneBlocks = page.Blocks.Where( a => a.Zone == zoneName ).ToList();

                var layoutBlocks = zoneBlocks.Where( a => a.LayoutId.HasValue ).OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
                var pageBlocks = zoneBlocks.Where( a => !a.LayoutId.HasValue ).OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();

                rptLayoutBlocks.DataSource = layoutBlocks;
                rptLayoutBlocks.DataBind();

                rptPageBlocks.DataSource = pageBlocks;
                rptPageBlocks.DataBind();
            }
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        public void LoadDropDowns()
        {
            int pageId = hfPageId.Value.AsInteger();

            ddlZones.Items.Clear();
            var page = Rock.Web.Cache.PageCache.Read( pageId );
            if ( page != null )
            {

                var zoneNames = FindZoneNames( page );

                foreach ( var zoneName in zoneNames )
                {
                    var zoneBlockCount = page.Blocks.Where( a => a.Zone.Equals( zoneName, StringComparison.OrdinalIgnoreCase ) ).Count();
                    ddlZones.Items.Add( new ListItem( string.Format( "{0} ({1})", zoneName, zoneBlockCount ), zoneName ) );
                }

                // default to Main Zone (if there is one)
                ddlZones.SetValue( "Main" );

            }
        }

        /// <summary>
        /// Parses the ASPX file and its MasterPage for Rock:Zone controls
        /// </summary>
        /// <param name="layoutPath">The layout path.</param>
        /// <returns></returns>
        private List<string> FindZoneNames( Rock.Web.Cache.PageCache page )
        {
            string theme = page.Layout.Site.Theme;
            string layout = page.Layout.FileName;
            string layoutPath = Rock.Web.Cache.PageCache.FormatPath( theme, layout );

            HtmlAgilityPack.HtmlDocument layoutAspx = new HtmlAgilityPack.HtmlDocument();
            layoutAspx.OptionFixNestedTags = true;
            string layoutFullPath = HttpContext.Current.Server.MapPath( layoutPath );
            layoutAspx.Load( layoutFullPath );

            List<HtmlNode> masterControlNodes = new List<HtmlNode>();

            Regex masterPageRegEx = new Regex( "<%@.*MasterPageFile=\"([^\"]*)\".*%>", RegexOptions.Compiled );

            var match = masterPageRegEx.Match( layoutAspx.DocumentNode.FirstChild.InnerText );
            if ( match.Success && match.Groups.Count > 1 )
            {
                string masterPageFileName = Path.Combine( Path.GetDirectoryName( layoutFullPath ), match.Groups[1].Value );
                HtmlAgilityPack.HtmlDocument masterAspx = new HtmlAgilityPack.HtmlDocument();
                masterAspx.OptionFixNestedTags = true;
                masterAspx.Load( masterPageFileName );
                FindAllZoneControls( masterAspx.DocumentNode.ChildNodes, masterControlNodes );
            }

            List<HtmlNode> layoutControlNodes = new List<HtmlNode>();
            FindAllZoneControls( layoutAspx.DocumentNode.ChildNodes, layoutControlNodes );

            List<string> zoneNames = new List<string>();
            foreach ( var masterNode in masterControlNodes )
            {
                if ( masterNode.Name.Equals( "Rock:Zone", StringComparison.OrdinalIgnoreCase ) )
                {
                    zoneNames.Add( masterNode.Attributes["Name"].Value );
                }
                else if ( masterNode.Name.Equals( "asp:ContentPlaceHolder", StringComparison.OrdinalIgnoreCase ) && masterNode.Id.Equals( "main" ) )
                {
                    zoneNames.AddRange( layoutControlNodes.Where( a => a.Attributes["Name"] != null ).Select( a => a.Attributes["Name"].Value ).ToList() );
                }
            }

            return zoneNames;
        }

        /// <summary>
        /// Finds all zone controls.
        /// derived from http://stackoverflow.com/a/19395924
        /// </summary>
        /// <param name="htmlNodeCollection">The HTML node collection.</param>
        /// <param name="controlNodes">The control nodes.</param>
        private static void FindAllZoneControls( HtmlNodeCollection htmlNodeCollection, List<HtmlNode> controlNodes )
        {
            foreach ( HtmlNode childNode in htmlNodeCollection )
            {
                if ( childNode.Name.Equals( "Rock:Zone", StringComparison.OrdinalIgnoreCase ) )
                {
                    controlNodes.Add( childNode );
                }
                else if ( childNode.Name.Equals( "asp:ContentPlaceHolder", StringComparison.OrdinalIgnoreCase ) )
                {
                    // also add add any ContentPlaceFolder nodes so we know where to put the Zones of the Layout file
                    controlNodes.Add( childNode );
                }
                else
                {
                    FindAllZoneControls( childNode.ChildNodes, controlNodes );
                }
            }
        }

        #endregion

        private void AddAdminControls( BlockCache block, Panel pnlLayoutItem )
        {
            string adminButtonsHtmlFormat =
 @"<div class='block-config-buttons pull-right'>
    <a title='Block Properties' class='properties' id='aBlockProperties' href='javascript: Rock.controls.modal.show($(this), ""/BlockProperties/{0}?t=Block Properties"")' height='500px'><i class='fa fa-cog'></i></a>
    <a title='Block Security' class='security' id='aSecureBlock' href='javascript: Rock.controls.modal.show($(this), ""/Secure/{2}/{0}?t=Block Security&amp;pb=&amp;sb=Done"")' height='500px'><i class='fa fa-lock'></i></a>
    <a title='Move Block' class='block-move' href='{0}' data-zone-location='Page' data-zone='Main'><i class='fa fa-external-link'></i></a>
    <a class='delete block-delete {1}' href='{0}' title='Delete Block'><i class='fa fa-times-circle-o'></i></a>
</div>";

            string adminButtonsHtml = string.Format( 
                adminButtonsHtmlFormat, 
                block.Id, // {0}
                block.IsSystem ? "disabled js-disabled" : "",// {1} 
                EntityTypeCache.Read<Rock.Model.Block>().Id  // {2}
                ); 

            Literal lAdminButtonsHtml = new Literal();
            lAdminButtonsHtml.Text = adminButtonsHtml;
            pnlLayoutItem.Controls.Add( lAdminButtonsHtml );

            /*
            RockBlock blockControl = this.Page.TemplateControl.LoadControl( block.BlockType.Path ) as RockBlock;
            blockControl.SetBlock( block.Page, block, true, true );
            var adminControls = blockControl.GetAdministrateControls( true, true );
            foreach ( var adminControl in adminControls )
            {
                if ( adminControl is LinkButton )
                {
                    WebControl btn = ( adminControl as WebControl );
                    if ( btn != null && btn.Attributes["onclick"] != null )
                    {
                        // some admincontrols will toggle the BlockConfig bar, but this isn't a block config bar, so remove the javascript
                        btn.Attributes["onclick"] = btn.Attributes["onclick"].Replace( "Rock.admin.pageAdmin.showBlockConfig()", string.Empty );
                    }
                }
                pnlLayoutItem.Controls.Add( adminControl );
            }*/
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptPageBlocks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptPageOrLayoutBlocks_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            BlockCache block = e.Item.DataItem as BlockCache;
            if ( block != null )
            {
                Panel pnlBlockEditWidget = e.Item.FindControl( "pnlBlockEditWidget" ) as Panel;
                Literal lPanelHeading = new Literal();

                lPanelHeading.Text = string.Format(
                    @"<div class='panel-heading'>
                        <a class='btn btn-link btn-xs panel-widget-reorder js-stop-immediate-propagation'><i class='fa fa-bars'></i></a>
                        <span>{0} ({1})</span>
                      
                        <div class='block-config-buttons pull-right'>
                        ",
                    block.Name,
                    block.BlockType );

                pnlBlockEditWidget.Controls.Add( lPanelHeading );

                AddAdminControls( block, pnlBlockEditWidget );

                Literal lPanelFooter = new Literal();
                lPanelFooter.Text = "</div></div>";

                pnlBlockEditWidget.Controls.Add( lPanelFooter );


            }
        }

        /// <summary>
        /// Handles the DeleteClick event of the pwLayoutBlock control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void pwLayoutBlock_DeleteClick( object sender, EventArgs e )
        {
            PanelWidget pwLayoutBlock = sender as PanelWidget;
        }
    }
}