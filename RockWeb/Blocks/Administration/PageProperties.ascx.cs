//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    public partial class PageProperties : Rock.Web.UI.RockBlock
    {
        #region Fields

        private Rock.Web.Cache.PageCache _page = null;
        private string _zoneName = string.Empty;
        private List<string> tabs = new List<string> { "Basic Settings", "Menu Display", "Advanced Settings"} ;
        
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

        protected override void OnInit( EventArgs e )
        {
            Rock.Web.UI.DialogMasterPage masterPage = this.Page.Master as Rock.Web.UI.DialogMasterPage;
            if ( masterPage != null )
                masterPage.OnSave += new EventHandler<EventArgs>( masterPage_OnSave );

            try
            {
                int pageId = Convert.ToInt32( PageParameter( "Page" ) );
                _page = Rock.Web.Cache.PageCache.Read( pageId );

                if ( _page.IsAuthorized( "Configure", CurrentPerson ) )
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
                                if ( !blockContexts.Contains( context ) )
                                    blockContexts.Add( context );
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
                            tbContext.Text = _page.PageContexts[context];

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

        protected override void OnLoad( EventArgs e )
        {
            if (!Page.IsPostBack && _page.IsAuthorized( "Configure", CurrentPerson ) )
            {
                Rock.Cms.PageService pageService = new Rock.Cms.PageService();
                Rock.Cms.Page page = pageService.Get( _page.Id );
                
                rptProperties.DataSource = tabs;
                rptProperties.DataBind();

                LoadDropdowns();

                tbPageName.Text = _page.Name;
                tbPageTitle.Text = _page.Title;
                ddlParentPage.SelectedValue = _page.ParentPage != null ? _page.ParentPage.Id.ToString() : "0";
                ddlLayout.Text = _page.Layout;
                ddlMenuWhen.SelectedValue = ( ( Int32 )_page.DisplayInNavWhen ).ToString();
                cbMenuDescription.Checked = _page.MenuDisplayDescription;
                cbMenuIcon.Checked = _page.MenuDisplayIcon;
                cbMenuChildPages.Checked = _page.MenuDisplayChildPages;
                cbRequiresEncryption.Checked = _page.RequiresEncryption;
                cbEnableViewState.Checked = _page.EnableViewState;
                cbIncludeAdminFooter.Checked = _page.IncludeAdminFooter;
                tbCacheDuration.Text = _page.OutputCacheDuration.ToString();
                tbDescription.Text = _page.Description;
                tbPageRoute.Text = string.Join(",", page.PageRoutes.Select( route => route.Route ).ToArray());
              
            }

            base.OnLoad( e );

            if ( Page.IsPostBack )
                Rock.Attribute.Helper.SetErrorIndicators( phAttributes, _page );
        }

        #endregion

        #region Events

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

        void masterPage_OnSave( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                using ( new Rock.Data.UnitOfWorkScope() )
                {
                    var pageService = new Rock.Cms.PageService();
                    var routeService = new Rock.Cms.PageRouteService();
                    var contextService = new Rock.Cms.PageContextService();
                    
                    var page = pageService.Get( _page.Id );

                    int parentPage = Int32.Parse( ddlParentPage.SelectedValue );
                    if ( page.ParentPageId != parentPage )
                    {
                        if ( page.ParentPageId.HasValue )
                            Rock.Web.Cache.PageCache.Flush( page.ParentPageId.Value );

                        if ( parentPage != 0 )
                            Rock.Web.Cache.PageCache.Flush( parentPage );
                    }

                    page.Name = tbPageName.Text;
                    page.Title = tbPageTitle.Text;
                    if ( parentPage != 0 )
                        page.ParentPageId = parentPage;
                    else
                        page.ParentPageId = null;
                    page.Layout = ddlLayout.Text;
                    page.DisplayInNavWhen = (Rock.Cms.DisplayInNavWhen)Enum.Parse( typeof( Rock.Cms.DisplayInNavWhen ), ddlMenuWhen.SelectedValue );
                    page.MenuDisplayDescription = cbMenuDescription.Checked;
                    page.MenuDisplayIcon = cbMenuIcon.Checked;
                    page.MenuDisplayChildPages = cbMenuChildPages.Checked;
                    page.RequiresEncryption = cbRequiresEncryption.Checked;
                    page.EnableViewState = cbEnableViewState.Checked;
                    page.IncludeAdminFooter = cbIncludeAdminFooter.Checked;
                    page.OutputCacheDuration = Int32.Parse( tbCacheDuration.Text );
                    page.Description = tbDescription.Text;
                    
                    foreach ( var pageRoute in page.PageRoutes.ToList() )
                        routeService.Delete( pageRoute, CurrentPersonId );
                    page.PageRoutes.Clear();

                    foreach ( var pageContext in page.PageContexts.ToList() )
                        contextService.Delete( pageContext, CurrentPersonId);
                    page.PageContexts.Clear();

                    foreach ( string route in tbPageRoute.Text.SplitDelimitedValues() )
                    {
                        var pageRoute = new Rock.Cms.PageRoute();
                        pageRoute.Route = route;
                        pageRoute.Guid = Guid.NewGuid();
                        page.PageRoutes.Add( pageRoute );
                    }

                    if (phContextPanel.Visible)
                        foreach ( var control in phContext.Controls)
                            if ( control is LabeledTextBox )
                            {
                                var tbContext = control as LabeledTextBox;
                                var pageContext = new Rock.Cms.PageContext();
                                pageContext.Entity = tbContext.LabelText;
                                pageContext.IdParameter = tbContext.Text;
                                page.PageContexts.Add( pageContext );
                            }

                    pageService.Save( page, CurrentPersonId );

                    Rock.Attribute.Helper.GetEditValues( phAttributes, _page );
                    _page.SaveAttributeValues( CurrentPersonId );

                    Rock.Web.Cache.PageCache.Flush( _page.Id );
                }

                string script = "window.parent.closeModal()";
                ScriptManager.RegisterStartupScript( this.Page, this.GetType(), "close-modal", script, true );
            }
        }

        #endregion

        #region Internal Methods

        private void LoadDropdowns()
        {
            ddlParentPage.Items.Clear();
            ddlParentPage.Items.Add( new ListItem( "Root", "0" ) );
            foreach ( var page in new Rock.Cms.PageService().GetByParentPageId( null ) )
                AddPage( page, 1 );

            ddlLayout.Items.Clear();
            DirectoryInfo di = new DirectoryInfo( Path.Combine( this.Page.Request.MapPath( this.CurrentTheme ), "Layouts" ) );
            foreach ( FileInfo fi in di.GetFiles( "*.aspx" ) )
                ddlLayout.Items.Add( new ListItem( fi.Name.Remove( fi.Name.IndexOf( ".aspx" ) ) ) );

            ddlMenuWhen.BindToEnum( typeof( Rock.Cms.DisplayInNavWhen ) );
        }

        private void AddPage( Rock.Cms.Page page, int level )
        {
            string pageName = new string( '-', level ) + page.Name;
            ddlParentPage.Items.Add( new ListItem( pageName, page.Id.ToString() ) );
            foreach ( var childPage in page.Pages )
                AddPage( childPage, level + 1 );
        }

        private void DisplayError( string message )
        {
            pnlError.Controls.Clear();
            pnlError.Controls.Add( new LiteralControl( message ) );
            pnlError.Visible = true;

            phContent.Visible = false;
        }

        protected string GetTabClass( object property )
        {
            if ( property.ToString() == CurrentProperty )
                return "active";
            else
                return "";
        }

        private void ShowSelectedPane()
        {
            if (CurrentProperty.Equals( "Basic Settings" )) {
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

    }

    #endregion
}