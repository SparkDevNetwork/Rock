using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;
using System.Data;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

using org.newpointe.Stars.Data;
using org.newpointe.Stars.Model;

namespace RockWeb.Plugins.org_newpointe.Stars
{
    /// <summary>
    /// Template block for a TreeView.
    /// </summary>
    [DisplayName( "Stars Report" )]
    [Category( "NewPointe Stars" )]
    [Description( "Report to show star totals." )]
    public partial class StarsReport : Rock.Web.UI.RockBlock
    {
        private readonly RockContext _rockContext = new RockContext();
        private readonly StarsProjectContext _starsProjectContext = new StarsProjectContext();

        public Person SelectedPerson;


        protected void Page_Load( object sender, EventArgs e )
        {
            if ( !IsPostBack )
            {
                cpCampus.DataSource = CampusCache.All();
                cpCampus.DataBind();
                BindGrid();
            }

            starsFilters.Show();

            mypMonth.MinimumYear = DateTime.Now.Year - 10;
            mypMonth.MaximumYear = DateTime.Now.Year;

            mypMonth.SelectedDate = mypMonth.SelectedDate ?? DateTime.Now;

        }


        protected void BindGrid()
        {
            StarsService starsService = new StarsService( _starsProjectContext );

            DateTime selectedDate = mypMonth.SelectedDate ?? DateTime.Now;
            var selectedDateBefore = selectedDate.SundayDate().AddDays( -6 );
            var selectedDateAfter = selectedDate.AddMonths( 1 ).SundayDate().AddDays( -6 );
            
            var starsList = starsService.Queryable()
                .Where( x => x.TransactionDateTime > selectedDateBefore && x.TransactionDateTime < selectedDateAfter )
                .GroupBy( x => x.PersonAlias.Person, (x, y) => new { Person = x, Sum = y.Sum( z => z.Value ) })
                .ToList()
                .Select( x => new { x.Person, x.Sum, Campus = x.Person.GetCampus() } );
            
            //Filter Campuses
            var selectedCampuses = cpCampus.SelectedValues;
            if ( selectedCampuses.Count > 0 )
            {
                starsList = starsList.Where( x => x.Campus == null || selectedCampuses.Contains( x.Campus.Name )).ToList();
            }

            //Get Sum of stars
            var startsList = starsList.Select( g =>
            {
                var personLoc = g.Person.GetHomeLocation();
                return new
                {
                    Month = selectedDate.Month,
                    g.Person.Id,
                    g.Person,
                    g.Sum,
                    g.Campus,
                    PersonZip = personLoc != null ? personLoc.PostalCode : ""
                };
            } );

            //Filter Star Levels
            int starsValueFilter = 0;

            if ( !string.IsNullOrWhiteSpace(ddlStars.SelectedValue) )
            {
                starsValueFilter = ddlStars.SelectedValue.AsInteger();
                startsList = startsList.Where( a => a.Sum >= starsValueFilter && a.Sum < starsValueFilter + 10 );
            }

            //Order the list
            startsList = startsList.OrderBy( g => g.Campus != null ? g.Campus.Name : "" ).ThenBy( g => g.PersonZip ).ThenBy( g => g.Person.LastName ).ThenBy( g => g.Person.FirstName );

            //Bind list to grid
            gStars.DataSource = startsList.ToList();
            gStars.DataBind();
            
        }


        protected void gStars_OnRowSelected( object sender, RowEventArgs e )
        {
            Response.Redirect( "~/Person/" + e.RowKeyValue + "/Stars" );
        }

        protected void filters_ApplyFilterClick( object sender, EventArgs e )
        {
            BindGrid();
            starsFilters.Show();
        }


        protected void gStars_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindGrid();
        }
    }
}