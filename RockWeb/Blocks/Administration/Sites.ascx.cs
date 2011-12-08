//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;

namespace RockWeb.Blocks.Administration
{
    public partial class Sites : Rock.Web.UI.Block
    {
        private string _action = string.Empty;
        private int _siteId = 0;

        protected void Page_Init( object sender, EventArgs e )
        {
            _action = PageParameter( "action" ).ToLower();
            switch ( _action )
            {
                case "":
                case "list":
                    DisplayList();
                    break;
                case "add":
                    _siteId = 0;
                    DisplayEdit( _siteId );
                    break;
                case "edit":
                    if ( Int32.TryParse( PageParameter( "SiteId" ), out _siteId ) )
                        DisplayEdit( _siteId );
                    else
                        throw new System.Exception( "Invalid Site Id" );
                    break;
            }
        }

        protected void Page_Load( object sender, EventArgs e )
        {
        }

        private void DisplayList()
        {
            phList.Visible = true;
            phDetails.Visible = false;

            Rock.CMS.SiteService siteService = new Rock.CMS.SiteService();
            gvList.DataSource = siteService.Queryable().ToList();
            gvList.DataBind();
        }

        private void DisplayEdit( int siteId )
        {
            phList.Visible = false;
            phDetails.Visible = true;

            using ( new Rock.Data.UnitOfWorkScope() )
            {
                Rock.CMS.SiteService siteService = new Rock.CMS.SiteService();
                Rock.CMS.PageService pageService = new Rock.CMS.PageService();
                Rock.CMS.ThemeService themeService = new Rock.CMS.ThemeService( Request.RequestContext.HttpContext.Server.MapPath( "~" ) );

                ddlDefaultPage.DataSource = pageService.Queryable().Where( p => p.ParentPage == null ).ToList();
                ddlDefaultPage.DataBind();

				ddlTheme.DataSource = themeService.GetThemesNames();
				ddlTheme.DataBind();

                if ( siteId > 0 )
                {
                    Rock.CMS.Site site = siteService.Get( Convert.ToInt32( PageParameter( "SiteId" ) ) );
                    tbName.Text = site.Name;
                    tbDescription.Text = site.Description;
					//tbTheme.Text = site.Theme;
					ddlTheme.SelectedValue = site.Theme;
                    ddlDefaultPage.SelectedValue = site.DefaultPageId.ToString();
                }
                else
                {
                    tbName.Text = string.Empty;
                    tbDescription.Text = string.Empty;
                    //tbTheme.Text = string.Empty;
                }
            }
        }

        protected void lbSave_Click( object sender, EventArgs e )
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                Rock.CMS.SiteService siteService = new Rock.CMS.SiteService();
                Rock.CMS.PageService pageService = new Rock.CMS.PageService();

                Rock.CMS.Site site = _action == "add" ?
                    new Rock.CMS.Site() :
                    siteService.Get( _siteId );

                site.Name = tbName.Text;
                site.Description = tbDescription.Text;
				site.Theme = ddlTheme.SelectedValue;

                Rock.CMS.Page page = pageService.Get( Convert.ToInt32( ddlDefaultPage.SelectedValue ) );
                site.DefaultPage = page;

                if ( _action == "add" )
                    siteService.Add( site, CurrentPersonId );
                siteService.Save( site, CurrentPersonId );

                Response.Redirect( "~/site/list" );
            }
        }
    }
}