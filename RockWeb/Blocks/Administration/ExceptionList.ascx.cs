using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administraton
{
    [IntegerField("Summary Count Days", "Summary field for exceptions that have occurred within the last x days. Default value is 7.", false, 7)]
    [DetailPage]
    public partial class ExceptionList : RockBlock
    {
        #region Control Methods
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            btnClearExceptions.Visible = IsUserAuthorized( "Edit" );

            gExceptionList.DataKeyNames = new string[] { "Id" };
            gExceptionList.GridRebind += gExceptionList_GridRebind;
            gExceptionList.RowDataBound += gExceptionList_RowDataBound;

            fExceptionList.ApplyFilterClick += fExceptionList_ApplyFilterClick;
            fExceptionList.DisplayFilterValue += fExceptionList_DisplayFilterValue;
        }



        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }
        }

        #endregion

        #region Grid Events
        protected void fExceptionList_ApplyFilterClick( object sender, EventArgs e )
        {
            if ( ddlSite.SelectedValue != All.IdValue )
            {
                fExceptionList.SaveUserPreference( "Site", ddlSite.SelectedValue );
            }
            else
            {
                fExceptionList.SaveUserPreference( "Site", String.Empty );
            }

            int userPersonId;
            if ( int.TryParse( ppUser.SelectedValue, out userPersonId ) && userPersonId != None.Id && ppUser.PersonName != None.TextHtml)
            {
                fExceptionList.SaveUserPreference( "User", userPersonId.ToString() );
            }
            else
            {
                fExceptionList.SaveUserPreference( "User", String.Empty );
            }

            if ( ppPage.SelectedValueAsInt( false ) != None.Id )
            {
                fExceptionList.SaveUserPreference( "Page", ppPage.SelectedValueAsInt( false ).ToString() );
            }
            else
            {
                fExceptionList.SaveUserPreference( "Page", String.Empty );
            }
                
            fExceptionList.SaveUserPreference( "Status Code", txtStatusCode.Text );

            DateTime startDate;
            if ( DateTime.TryParse( dpStartDate.Text, out startDate ) )
            {
                fExceptionList.SaveUserPreference( "Start Date", startDate.ToShortDateString() );
            }
            else
            {
                fExceptionList.SaveUserPreference( "Start Date", String.Empty );
            }

            DateTime endDate;
            if ( DateTime.TryParse( dpEndDate.Text, out endDate ) )
            {
                fExceptionList.SaveUserPreference( "End Date", endDate.ToShortDateString() );
            }
            else
            {
                fExceptionList.SaveUserPreference( "End Date", String.Empty );
            }
            BindGrid();
        }

        protected void fExceptionList_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Site":
                    int siteId;
                    if ( int.TryParse( e.Value, out siteId ) )
                    {
                        SiteService siteService = new SiteService();
                        var site = siteService.Get( siteId );
                        if ( site != null )
                        {
                            e.Value = site.Name;
                        }
                    }
                    break;
                case "Page":
                    int pageId;
                    if ( int.TryParse( e.Value, out pageId ) )
                    {
                        PageService pageService = new PageService();
                        var page = pageService.Get( pageId );
                        if ( page != null )
                        {
                            e.Value = page.Name;
                        }
                    }
                    break;
                case "User":
                    int userPersonId;
                    if ( int.TryParse( e.Value, out userPersonId ) )
                    {
                        PersonService personService = new PersonService();
                        var user = personService.Get( userPersonId );
                        if ( user != null )
                        {
                            e.Value = user.FullName;
                        }
                    }
                    break;
            }
        }

        protected void gExceptionList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        void gExceptionList_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.Header )
            {
                Label headerLbl = (Label)e.Row.FindControl( "lblSubsetHeader" );
                int subsetDays = Convert.ToInt32( GetAttributeValue( "SummaryCountDays" ) );

                headerLbl.Text = string.Format( "Last {0} days", subsetDays );

            }
        }

        protected void gExceptionList_RowSelected( object sender, EventArgs e )
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Page Events
        protected void btnClearExceptions_Click( object sender, EventArgs e )
        {
            ClearExceptions();
            BindGrid();
        }
        #endregion

        #region Internal Methods
        private void BindFilter()
        {
            BindSites();

            int siteId;

            if ( int.TryParse( fExceptionList.GetUserPreference( "Site" ), out siteId ) 
                    && ddlSite.Items.FindByValue(siteId.ToString()) != null)
            {
                ddlSite.SelectedValue = siteId.ToString();
            }

            int pageId;
            if ( int.TryParse( fExceptionList.GetUserPreference( "Page" ), out pageId ) )
            {
                PageService pageService = new PageService();
                ppPage.SetValue( pageService.Get( pageId ) );
            }

            int userPersonId;
            if ( int.TryParse( fExceptionList.GetUserPreference( "User" ), out userPersonId ) )
            {
                PersonService personService = new PersonService();
                ppUser.SetValue( personService.Get( userPersonId ) );
            }

            if ( !String.IsNullOrEmpty( fExceptionList.GetUserPreference( "Status Code" ) ) )
            {
                txtStatusCode.Text = fExceptionList.GetUserPreference( "Status Code" );
            }

            DateTime startDate;
            if ( DateTime.TryParse( fExceptionList.GetUserPreference( "Start Date" ), out startDate ) )
            {
                dpStartDate.Text = startDate.ToShortDateString();
            }

            DateTime endDate;
            if ( DateTime.TryParse( fExceptionList.GetUserPreference( "End Date" ), out endDate ) )
            {
                dpEndDate.Text = endDate.ToShortDateString();
            }

        }

        private void BindGrid()
        {
            DateTime sevenDaysAgo = DateTime.Now.Date.AddDays(-7);

            var exceptionQuery = BuildExceptionListQuery();

            if ( gExceptionList.SortProperty != null )
            {
                gExceptionList.DataSource = exceptionQuery.Sort( gExceptionList.SortProperty ).ToList();
            }
            else
            {
                gExceptionList.DataSource = exceptionQuery.OrderByDescending( e => e.LastExceptionDate ).ToList();
            }

            gExceptionList.DataBind();
        
        }

        private void BindSites()
        {
            SiteService siteService = new SiteService();
            ddlSite.DataTextField = "Name";
            ddlSite.DataValueField = "Id";
            ddlSite.DataSource = siteService.Queryable().OrderBy( s => s.Name ).ToList();
            ddlSite.DataBind();

            ddlSite.Items.Insert( 0, new ListItem( All.Text, All.IdValue ) );
        }

        private IQueryable<ExceptionListSummaryResults> BuildExceptionListQuery()
        {
            ExceptionLogService exceptionLogService = new ExceptionLogService();
            IQueryable<ExceptionLog> query = exceptionLogService.Queryable();

            int siteId;
            if ( int.TryParse( fExceptionList.GetUserPreference( "Site" ), out siteId ) && siteId > 0 )
            {
                query = query.Where( e => e.SiteId == siteId );
            }

            int pageId;
            if ( int.TryParse( fExceptionList.GetUserPreference( "Page" ), out pageId ) && pageId > 0 )
            {
                query = query.Where( e => e.PageId == pageId );
            }

            int userPersonID;
            if ( int.TryParse( fExceptionList.GetUserPreference( "User" ), out userPersonID ) && userPersonID > 0 )
            {
                query = query.Where( e => e.CreatedByPersonId == userPersonID );
            }

            string statusCode = fExceptionList.GetUserPreference( "Status Code" );
            if ( !String.IsNullOrEmpty( statusCode ) )
            {
                query = query.Where( e => e.StatusCode == statusCode );
            }

            DateTime startDate;
            if( DateTime.TryParse( fExceptionList.GetUserPreference( "Start Date" ), out startDate ) )
            {
                startDate = startDate.Date;
                query = query.Where(e => e.ExceptionDateTime >= startDate);
            }

            DateTime endDate;
            if ( DateTime.TryParse( fExceptionList.GetUserPreference( "End Date" ), out endDate ) )
            {
                endDate = endDate.Date.AddDays( 1 );
                query = query.Where( e => e.ExceptionDateTime < endDate );
            }
            
            //Only look for inner exceptions
            query = query.Where( e => e.HasInnerException == null || e.HasInnerException == false );

            int summaryCountDays = Convert.ToInt32( GetAttributeValue( "SummaryCountDays" ) );
            DateTime minSummaryCountDate = DateTime.Now.Date.AddDays( - ( summaryCountDays ) );
            
            var exceptionGroups = query.GroupBy( e => new
                                                {
                                                    SiteName = e.Site.Name,
                                                    PageName = e.Page.Name,
                                                    Description = e.Description
                                                } )
                                        .Select( eg => new
                                                {
                                                    Id = eg.Max(e => e.Id),
                                                    SiteName = eg.Key.SiteName,
                                                    PageName = eg.Key.PageName,
                                                    Description = eg.Key.Description,
                                                    LastExceptionDate = eg.Max( e => e.ExceptionDateTime ), 
                                                    TotalCount = eg.Count(),
                                                    SubsetCount = eg.Count( e => e.ExceptionDateTime >= minSummaryCountDate) 
                                                } ).ToList();

            List<ExceptionListSummaryResults> exceptionListResults = new List<ExceptionListSummaryResults>();
            foreach ( var exceptionGroup in exceptionGroups )
            {
                exceptionListResults.Add( new ExceptionListSummaryResults( exceptionGroup.Id, exceptionGroup.SiteName, exceptionGroup.PageName, exceptionGroup.Description, 
                    exceptionGroup.LastExceptionDate, exceptionGroup.TotalCount, exceptionGroup.SubsetCount ) );
            }

            return exceptionListResults.AsQueryable();
        }

        private void ClearExceptions()
        {
            Service service = new Service();
            service.ExecuteCommand( "DELETE FROM ExceptionLog", new object[] { } );
        }

        #endregion

    }

    public class ExceptionListSummaryResults
    {
        public int Id { get; set; }
        public string SiteName { get; set; }
        public string PageName { get; set; }
        public string Description { get; set; }
        public DateTime LastExceptionDate { get; set; }
        public int TotalCount { get; set; }
        public int SubsetCount { get; set; }

        public ExceptionListSummaryResults() { }

        public ExceptionListSummaryResults( int id, string siteName, string pageName, string description, DateTime lastExceptionDate, int totalCount, int subsetCount )
        {
            Id = id;
            SiteName = siteName;
            PageName = pageName;
            Description = description;
            LastExceptionDate = lastExceptionDate;
            TotalCount = totalCount;
            SubsetCount = subsetCount;
        }
    }

}