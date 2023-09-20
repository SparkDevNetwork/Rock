using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;



namespace RockWeb.Plugins.org_lakepointe.Finance
{
    [DisplayName( "Contributions by Date and Range" )]
    [Category( "LPC > Finance" )]
    [Description( "Filter settings for an SQL report." )]

    //[BooleanField( "Show Audience Filter", "Determines if the Audience Filter is shown.", true, Order = 0 )]
    //[TextField("Audience Filter Title", "The title to display on the audience filter.", false, Order = 1)]
    //[DefinedValueField( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE, "Filter Audiences", "Determines which audiences should be displayed in the filter.", false, true, key: "FilterCategories", order: 2 )]
    //[TextField( "Campus Filter Title", "The title to display on the campus filter.", false, "Campus", Order = 4)]
    //[CampusesField( "Filter Campuses", "Determines which campues should be displayed in the fiter.", required: false, order: 5, includeInactive: false, key: "FilterCampuses" )]
    //[BooleanField("Show Campus Filter", "Determines if the campus filter is shown.", true, Order = 3)]
   
    
    public partial class ContributionByDateAndRange : RockBlock
    {
        private RockContext _context;

        #region Properties

        string _campuses = string.Empty;
        string _startDate = string.Empty;
        string _endDate = string.Empty;
        string _minimum = string.Empty;
        string _maximum = string.Empty;

        #endregion

        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            _context = new RockContext();

            _campuses = PageParameter( "campus" );
            _startDate = PageParameter( "startDate" );
            _endDate = PageParameter( "endDate" );
            _minimum = PageParameter( "minimum" );
            _maximum = PageParameter( "maximum" );
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnInit( e );

            if ( !Page.IsPostBack )
            {
                cpCampuses.IncludeInactive = false; // this is a hack for force loading of cpCampuses.Items

                var ids = _campuses.Split( '+' );
                foreach ( var id in ids )
                {
                    if ( id.IsNotNullOrWhiteSpace() )
                    {
                        cpCampuses.Items.FindByValue( id ).Selected = true;
                    }
                }

                DateTime result;
                if ( DateTime.TryParse( _startDate, out result ) )
                {
                    dpStartDate.SelectedDate = result;
                }
                if ( DateTime.TryParse( _endDate, out result ) )
                {
                    dpEndDate.SelectedDate = result;
                }

                nbMinimum.Text = _minimum;
                nbMaximum.Text = _maximum;
            }
        }

        #endregion  

        #region Events

        protected void bbExecute_Click( object sender, EventArgs e )
        {
            ReloadPage();
        }

        #endregion

        #region Methods

        private void ReloadPage()
        {
            Dictionary<string, string> qs = new Dictionary<string, string>();

            var sb = new StringBuilder("+");
            cpCampuses.SelectedValues.ForEach( c => sb.AppendFormat( "{0}+", c ) );
            //if (sb.Length > 0)
            //{
            //    sb.Remove( sb.Length - 1, 1 ); // remove the trailing +
            //}
            qs.Add( "campus", sb.ToString() );

            qs.Add( "startDate", dpStartDate.SelectedDate.Value.ToShortDateString() );
            qs.Add( "endDate", dpEndDate.SelectedDate.Value.ToShortDateString() );

            qs.Add( "minimum", nbMinimum.Text );
            qs.Add( "maximum", nbMaximum.Text );

            NavigateToCurrentPage( qs );
        }
        #endregion
    }
}