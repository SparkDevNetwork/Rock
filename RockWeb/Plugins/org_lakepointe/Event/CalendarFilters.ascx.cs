using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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



namespace RockWeb.Plugins.org_lakepointe.Event
{
    [DisplayName( "Calendar Filters" )]
    [Category( "LPC > Event" )]
    [Description( "Filters for calendars that use Lava." )]

    [BooleanField( "Show Audience Filter", "Determines if the Audience Filter is shown.", true, Order = 0 )]
    [TextField("Audience Filter Title", "The title to display on the audience filter.", false, Order = 1)]
    [DefinedValueField( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE, "Filter Audiences", "Determines which audiences should be displayed in the filter.", false, true, key: "FilterCategories", order: 2 )]
    [TextField( "Campus Filter Title", "The title to display on the campus filter.", false, "Campus", Order = 4)]
    [CampusesField( "Filter Campuses", "Determines which campues should be displayed in the fiter.", required: false, order: 5, includeInactive: false, key: "FilterCampuses" )]
    [BooleanField("Show Campus Filter", "Determines if the campus filter is shown.", true, Order = 3)]
   
    
    public partial class CalendarFilters : RockBlock
    {
        private RockContext _context;

        #region Properties
        private bool ShowCampus { get; set; }
        private bool ShowAudience { get; set; }
        #endregion

        #region Base Control Methods
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            _context = new RockContext();
        }
        protected override void OnLoad( EventArgs e )
        {
            base.OnInit( e );

            ShowCampus = GetAttributeValue( "ShowCampusFilter" ).AsBoolean();
            ShowAudience = GetAttributeValue( "ShowAudienceFilter" ).AsBoolean(); 
            if ( !Page.IsPostBack )
            {
                BuildFilters();
            }
        }
        #endregion  

        #region Events
        protected void ddlCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            ReloadPage();
        }

        protected void ddlAudience_SelectedIndexChanged( object sender, EventArgs e )
        {
            ReloadPage();
        }
        #endregion

        #region Methods
        private void BuildFilters()
        {

            string campusTitle = GetAttributeValue( "CampusFilterTitle" );
            string audienceTitle = GetAttributeValue( "AudienceFilterTitle" );

            if ( ShowCampus )
            {
                ddlCampus.Label = campusTitle;
                pnlCampus.Visible = true;


                var enabledCampuses = GetAttributeValue( "FilterCampuses" ).SplitDelimitedValues()
                    .Select( c => c.AsGuidOrNull() )
                    .ToList();

                var campusQry = new CampusService( _context ).Queryable()
                    .Where( c => c.IsActive.HasValue )
                    .Where( c => c.IsActive.Value );


                if ( enabledCampuses.Count > 0 )
                {
                    campusQry = campusQry.Where( c => enabledCampuses.Contains( c.Guid ) );
                }

                ddlCampus.DataSource = campusQry
                    .OrderBy( c => c.Order )
                    .ThenBy( c => c.Name )
                    .ToList();

                ddlCampus.DataValueField = "Id";
                ddlCampus.DataTextField = "Name";
                ddlCampus.DataBind();

                ddlCampus.Items.Insert( 0, new ListItem( "All Campuses", "0" ) );

                var selectedCampus = PageParameter( "campus" );

                if ( selectedCampus.IsNotNullOrWhiteSpace() && ddlCampus.Items.FindByValue( selectedCampus ) != null )
                {
                    ddlCampus.SelectedValue = selectedCampus;
                }

            }
            else
            {
                pnlCampus.Visible = false;
            }

            if ( ShowAudience )
            {
                ddlAudience.Label = audienceTitle;
                pnlAudience.Visible = true;
                var enabledAudiences = GetAttributeValue( "FilterCategories" ).SplitDelimitedValues()
                    .Select( a => a.AsGuidOrNull() )
                    .ToList();

                var audiencesDT = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE.AsGuid(), _context );

                var audienceQry = audiencesDT.DefinedValues.AsQueryable();

                if ( enabledAudiences.Count > 0 )
                {
                    audienceQry = audienceQry.Where( a => enabledAudiences.Contains( a.Guid ) );
                }

                ddlAudience.DataSource = audienceQry.OrderBy( a => a.Value );
                ddlAudience.DataValueField = "Id";
                ddlAudience.DataTextField = "Value";
                ddlAudience.DataBind();

                ddlAudience.Items.Insert( 0, new ListItem( "All Ministries", "0" ) );

                var selectedAudience = PageParameter( "ministry" );

                if ( selectedAudience.IsNotNullOrWhiteSpace() && ddlAudience.Items.FindByValue( selectedAudience ) != null )
                {
                    ddlAudience.SelectedValue = selectedAudience;
                }
            }
            else
            {
                pnlAudience.Visible = false;
            }

        }

        private void ReloadPage()
        {
            Dictionary<string, string> qs = new Dictionary<string, string>();

            if ( ShowCampus && ddlCampus.SelectedValue != "0" )
            {
                qs.Add( "campus", ddlCampus.SelectedValue );
            }

            if ( ShowAudience && ddlAudience.SelectedValue != "0" )
            {
                qs.Add( "ministry", ddlAudience.SelectedValue );
            }
            NavigateToCurrentPage( qs );
        }
        #endregion
    }
}