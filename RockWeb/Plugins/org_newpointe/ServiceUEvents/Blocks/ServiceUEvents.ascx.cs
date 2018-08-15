using System;
using Rock.Model;
using Rock.Web.UI;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using Rock;
using System.Data.SqlClient;
using Rock.Web.Cache;

namespace RockWeb.Plugins.org_newpointe.ServiceUEvents
{
    [DisplayName( "ServiceU Events" )]
    [Category( "NewPointe.org Web Blocks" )]
    [Description( "Upcoming events for each campus" )]
    public partial class ServiceUEvents : RockBlock
    {

        protected void Page_Load( object sender, EventArgs e )
        {
            this.RockPage.AddScriptLink( ResolveUrl( "../Scripts/jquery.autocomplete.min.js" ) );
            this.RockPage.AddScriptLink( ResolveUrl( "../Scripts/underscore.min.js" ) );
            this.RockPage.AddScriptLink( ResolveUrl( "../Scripts/jstz.min.js" ) );
            this.RockPage.AddScriptLink( ResolveUrl( "../Scripts/calendar.js" ) );
            this.RockPage.AddScriptLink( ResolveUrl( "../Scripts/ServiceUEvents.js" ) );

            this.RockPage.AddCSSLink( ResolveUrl( "../Styles/autocomplete-styles.css" ) );
            this.RockPage.AddCSSLink( ResolveUrl( "../Styles/ServiceUEvents.css" ) );
            this.RockPage.AddCSSLink( ResolveUrl( "../Styles/calendar.min.css" ) );
            var eventtitleroute = PageParameter( "eventcalendarroute" );
            GetCampus();

            if ( !Page.IsPostBack )
            {
                //i'm filtering out those axd calls becuase they are shwoing up for some reson as a valid valud of eventcalendarroute. 
                if ( !string.IsNullOrEmpty( eventtitleroute ) && eventtitleroute != "WebResource.axd" && eventtitleroute != "ScriptResource.axd" )
                {
                    var rc = new Rock.Data.RockContext();
                    // int eventid = 0;
                    string eventId = string.Empty;
                    using ( rc )
                    {
                        eventId = rc.Database.SqlQuery<string>( "exec newpointe_getEventIDbyUrl @url", new SqlParameter( "url", eventtitleroute ) ).ToList<string>()[0];
                    }
                    if ( string.IsNullOrEmpty( eventId ) )
                    {
                        SiteCache site = SiteCache.GetSiteByDomain( Request.Url.Host );

                        //site.RedirectToPageNotFoundPage();

                    }
                    else
                    {
                        hdnEventId.Value = eventId;
                    }
                }
                else if ( !string.IsNullOrEmpty( eventtitleroute ) )
                {
                    //Response.Redirect( eventtitleroute );
                }

                CampusService campusService = new CampusService( new Rock.Data.RockContext() );

                var qry = campusService.Queryable().Where( c => !c.Name.Contains( "Central Services" ) && !c.Name.Contains( "Online" ) && !c.Name.Contains( "Future" ) && ( c.IsActive ?? false ) ).Select( p => new { Name = p.Name.Replace( "Campus", "" ).Trim(), ShortCode = p.ShortCode } ).OrderBy( c => c.Name ).ToList();

                rptCampuses.DataSource = qry;
                rptCampuses.DataBind();

                qry.Insert( 0, new { Name = "ALL", ShortCode = "ALL" } );
                ddlCampusDropdown.DataSource = qry;
                ddlCampusDropdown.DataValueField = "ShortCode";
                ddlCampusDropdown.DataTextField = "Name";
                ddlCampusDropdown.DataBind();
            }

        }


        protected void lnk_DataBinding( object sender, EventArgs e )
        {
            var lnk = ( System.Web.UI.WebControls.HyperLink ) sender;
            var name = Eval( "Name" ).ToString();
            var code = Eval( "ShortCode" ).ToString();
            lnk.Text = name;
            lnk.Attributes.Add( "data-campuscode", code );
            lnk.CssClass = "btn btn-default btn-block-xs campus-" + code.ToLower() + "-hover";
        }

        protected void GetCampus()
        {
            String campusCode = hdnCampus.Value;

            if ( String.IsNullOrWhiteSpace( campusCode ) )
            {
                var cNameParam = PageParameter( "campusName" );
                if ( !String.IsNullOrWhiteSpace( cNameParam ) )
                {
                    var campus = new CampusService( new Rock.Data.RockContext() ).Queryable().Where( x => x.Name.Contains( cNameParam ) ).First();
                    campusCode = campus != null ? campus.ShortCode : campusCode;
                }

                var cIdParam = PageParameter( "campusID" );
                if ( !String.IsNullOrWhiteSpace( cIdParam ) )
                {
                    campusCode = cIdParam;
                }

                var campusEntityType = EntityTypeCache.Read( typeof( Campus ) );
                var currentCampus = RockPage.GetCurrentContext( campusEntityType ) as Campus;
                if ( currentCampus != null && !String.IsNullOrWhiteSpace( currentCampus.ShortCode ) )
                {
                    campusCode = currentCampus.ShortCode;
                }

                hdnCampus.Value = campusCode;
            }
        }
    }
}