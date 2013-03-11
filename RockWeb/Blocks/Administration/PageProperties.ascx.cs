//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    public partial class PageProperties : RockBlock
    {
        #region Fields

        private List<string> tabs = new List<string> { "Basic Settings", "Menu Display", "Advanced Settings" };

        /// <summary>
        /// Gets or sets the current property.
        /// </summary>
        /// <value>
        /// The current property.
        /// </value>
        protected string CurrentProperty
        {
            get
            {
                object currentProperty = ViewState["CurrentProperty"];
                return currentProperty != null ? currentProperty.ToString() : "Basic Settings";
            }

            set
            {
                ViewState["CurrentProperty"] = value;
            }
        }

        #endregion

        #region Overridden Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            Rock.Web.UI.DialogMasterPage masterPage = this.Page.Master as Rock.Web.UI.DialogMasterPage;
            if ( masterPage != null )
            {
                masterPage.OnSave += new EventHandler<EventArgs>( masterPage_OnSave );
            }

            try
            {
                int pageId = Convert.ToInt32( PageParameter( "Page" ) );
                Rock.Web.Cache.PageCache _page = Rock.Web.Cache.PageCache.Read( pageId );

                if ( _page.IsAuthorized( "Administrate", CurrentPerson ) )
                {
                    phAttributes.Controls.Clear();
                    Rock.Attribute.Helper.AddEditControls( _page, phAttributes, !Page.IsPostBack );

                    List<string> blockContexts = new List<string>();
                    foreach ( var block in _page.Blocks )
                    {
                        var blockControl = TemplateControl.LoadControl( block.BlockType.Path ) as Rock.Web.UI.RockBlock;
                        if ( blockControl != null )
                        {
                            blockControl.CurrentPage = _page;
                            blockControl.CurrentBlock = block;
                            foreach ( var context in blockControl.ContextTypesRequired )
                            {
                                if ( !blockContexts.Contains( context ) )
                                {
                                    blockContexts.Add( context );
                                }
                            }
                        }
                    }

                    phContextPanel.Visible = blockContexts.Count > 0;

                    int i = 0;
                    foreach ( string context in blockContexts )
                    {
                        var tbContext = new LabeledTextBox();
                        tbContext.ID = string.Format( "context_{0}", i++ );
                        tbContext.Required = true;
                        tbContext.LabelText = context;

                        if ( _page.PageContexts.ContainsKey( context ) )
                        {
                            tbContext.Text = _page.PageContexts[context];
                        }

                        phContext.Controls.Add( tbContext );
                    }
                }
                else
                {
                    DisplayError( "You are not authorized to edit this page" );
                }
            }
            catch ( SystemException ex )
            {
                DisplayError( ex.Message );
            }

            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            int pageId = Convert.ToInt32( PageParameter( "Page" ) );
            Rock.Web.Cache.PageCache _page = Rock.Web.Cache.PageCache.Read( pageId );

            if ( !Page.IsPostBack && _page.IsAuthorized( "Administrate", CurrentPerson ) )
            {
                Rock.Model.PageService pageService = new Rock.Model.PageService();
                Rock.Model.Page page = pageService.Get( _page.Id );

                rptProperties.DataSource = tabs;
                rptProperties.DataBind();

                LoadDropdowns();

                tbPageName.Text = _page.Name;
                tbPageTitle.Text = _page.Title;
                ppParentPage.SetValue( pageService.Get( page.ParentPageId ?? 0 ) );
                ddlLayout.Text = _page.Layout;
                ddlMenuWhen.SelectedValue = ( (int)_page.DisplayInNavWhen ).ToString();
                cbMenuDescription.Checked = _page.MenuDisplayDescription;
                cbMenuIcon.Checked = _page.MenuDisplayIcon;
                cbMenuChildPages.Checked = _page.MenuDisplayChildPages;
                tbIconCssClass.Text = _page.IconCssClass;
                cbRequiresEncryption.Checked = _page.RequiresEncryption;
                cbEnableViewState.Checked = _page.EnableViewState;
                cbIncludeAdminFooter.Checked = _page.IncludeAdminFooter;
                tbCacheDuration.Text = _page.OutputCacheDuration.ToString();
                tbDescription.Text = _page.Description;
                tbPageRoute.Text = string.Join( ",", page.PageRoutes.Select( route => route.Route ).ToArray() );
                imgIcon.ImageId = page.IconFileId;
            }

            base.OnLoad( e );

            if ( Page.IsPostBack )
            {
                Rock.Attribute.Helper.SetErrorIndicators( phAttributes, _page );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbProperty control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbProperty_Click( object sender, EventArgs e )
        {
            LinkButton lb = sender as LinkButton;
            if ( lb != null )
            {
                CurrentProperty = lb.Text;

                rptProperties.DataSource = tabs;
                rptProperties.DataBind();
            }

            ShowSelectedPane();
        }

        /// <summary>
        /// Handles the OnSave event of the masterPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void masterPage_OnSave( object sender, EventArgs e )
        {
            int pageId = Convert.ToInt32( PageParameter( "Page" ) );
            Rock.Web.Cache.PageCache _page = Rock.Web.Cache.PageCache.Read( pageId );

            if ( Page.IsValid )
            {
                using ( new Rock.Data.UnitOfWorkScope() )
                {
                    var pageService = new Rock.Model.PageService();
                    var routeService = new Rock.Model.PageRouteService();
                    var contextService = new Rock.Model.PageContextService();

                    var page = pageService.Get( _page.Id );

                    int parentPageId = ppParentPage.SelectedValueAsInt() ?? 0;
                    if ( page.ParentPageId != parentPageId )
                    {
                        if ( page.ParentPageId.HasValue )
                        {
                            Rock.Web.Cache.PageCache.Flush( page.ParentPageId.Value );
                        }

                        if ( parentPageId != 0 )
                        {
                            Rock.Web.Cache.PageCache.Flush( parentPageId );
                        }
                    }

                    page.Name = tbPageName.Text;
                    page.Title = tbPageTitle.Text;
                    if ( parentPageId != 0 )
                    {
                        page.ParentPageId = parentPageId;
                    }
                    else
                    {
                        page.ParentPageId = null;
                    }

                    page.Layout = ddlLayout.Text;
                    page.DisplayInNavWhen = (Rock.Model.DisplayInNavWhen)Enum.Parse( typeof( Rock.Model.DisplayInNavWhen ), ddlMenuWhen.SelectedValue );
                    page.MenuDisplayDescription = cbMenuDescription.Checked;
                    page.MenuDisplayIcon = cbMenuIcon.Checked;
                    page.IconFileId = imgIcon.ImageId;
                    page.MenuDisplayChildPages = cbMenuChildPages.Checked;
                    page.IconCssClass = tbIconCssClass.Text;
                    page.RequiresEncryption = cbRequiresEncryption.Checked;
                    page.EnableViewState = cbEnableViewState.Checked;
                    page.IncludeAdminFooter = cbIncludeAdminFooter.Checked;
                    page.OutputCacheDuration = int.Parse( tbCacheDuration.Text );
                    page.Description = tbDescription.Text;

                    // new or updated route
                    foreach ( var pageRoute in page.PageRoutes.ToList() )
                    {
                        var existingRoute = RouteTable.Routes.OfType<Route>().FirstOrDefault( a => a.RouteId() == pageRoute.Id );
                        if ( existingRoute != null )
                        {
                            RouteTable.Routes.Remove( existingRoute );
                        }

                        routeService.Delete( pageRoute, CurrentPersonId );
                    }

                    page.PageRoutes.Clear();

                    foreach ( string route in tbPageRoute.Text.SplitDelimitedValues() )
                    {
                        var pageRoute = new Rock.Model.PageRoute();
                        pageRoute.Route = route;
                        pageRoute.Guid = Guid.NewGuid();
                        page.PageRoutes.Add( pageRoute );
                    }

                    foreach ( var pageContext in page.PageContexts.ToList() )
                    {
                        contextService.Delete( pageContext, CurrentPersonId );
                    }

                    page.PageContexts.Clear();

                    if ( phContextPanel.Visible )
                    {
                        foreach ( var control in phContext.Controls )
                        {
                            if ( control is LabeledTextBox )
                            {
                                var tbContext = control as LabeledTextBox;
                                var pageContext = new Rock.Model.PageContext();
                                pageContext.Entity = tbContext.LabelText;
                                pageContext.IdParameter = tbContext.Text;
                                page.PageContexts.Add( pageContext );
                            }
                        }
                    }

                    pageService.Save( page, CurrentPersonId );

                    foreach ( var pageRoute in new Rock.Model.PageRouteService().GetByPageId( page.Id ) )
                    {
                        RouteTable.Routes.AddPageRoute( pageRoute );
                    }

                    Rock.Attribute.Helper.GetEditValues( phAttributes, _page );
                    _page.SaveAttributeValues( CurrentPersonId );

                    Rock.Web.Cache.PageCache.Flush( _page.Id );
                }

                string script = @"
if ( window.parent.closeModal != null)
{
    window.parent.closeModal();
}
";
                ScriptManager.RegisterStartupScript( this.Page, this.GetType(), "close-modal", script, true );
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Loads the dropdowns.
        /// </summary>
        private void LoadDropdowns()
        {
            ddlLayout.Items.Clear();
            DirectoryInfo di = new DirectoryInfo( Path.Combine( this.Page.Request.MapPath( this.CurrentTheme ), "Layouts" ) );
            foreach ( FileInfo fi in di.GetFiles( "*.aspx" ) )
            {
                ddlLayout.Items.Add( new ListItem( fi.Name.Remove( fi.Name.IndexOf( ".aspx" ) ) ) );
            }

            ddlMenuWhen.BindToEnum( typeof( Rock.Model.DisplayInNavWhen ) );
        }

        /// <summary>
        /// Displays the error.
        /// </summary>
        /// <param name="message">The message.</param>
        private void DisplayError( string message )
        {
            pnlError.Controls.Clear();
            pnlError.Controls.Add( new LiteralControl( message ) );
            pnlError.Visible = true;

            phContent.Visible = false;
        }

        /// <summary>
        /// Gets the tab class.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        protected string GetTabClass( object property )
        {
            if ( property.ToString() == CurrentProperty )
            {
                return "active";
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Shows the selected pane.
        /// </summary>
        private void ShowSelectedPane()
        {
            if ( CurrentProperty.Equals( "Basic Settings" ) )
            {
                pnlBasicProperty.Visible = true;
                pnlMenuDisplay.Visible = false;
                pnlAdvancedSettings.Visible = false;
                pnlBasicProperty.DataBind();
            }
            else if ( CurrentProperty.Equals( "Menu Display" ) )
            {
                pnlBasicProperty.Visible = false;
                pnlMenuDisplay.Visible = true;
                pnlAdvancedSettings.Visible = false;
                pnlMenuDisplay.DataBind();
            }
            else if ( CurrentProperty.Equals( "Advanced Settings" ) )
            {
                pnlBasicProperty.Visible = false;
                pnlMenuDisplay.Visible = false;
                pnlAdvancedSettings.Visible = true;
                pnlAdvancedSettings.DataBind();
            }

            upPanel.DataBind();
        }

        #endregion
    }
}