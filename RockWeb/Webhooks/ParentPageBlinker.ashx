<%@ WebHandler Language="C#" Class="ParentPageBlinker" %>

using System;
using System.Web;
using System.Linq;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

public class ParentPageBlinker : IHttpHandler
{

    public void ProcessRequest( HttpContext context )
    {

        HttpRequest request = context.Request;
        HttpResponse response = context.Response;
        response.ContentType = "text/plain";

        if ( request.HttpMethod != "GET" )
        {
            response.Write( "Invalid request type." );
            return;
        }

        var rockContext = new RockContext();

        IQueryable<Workflow> activePagerWorkflows = new WorkflowService(rockContext).Queryable().Where(w => w.CompletedDateTime == null && w.WorkflowTypeId == 193);

        if ( !string.IsNullOrWhiteSpace( request.Params["Campus"] ) )
        {
            CampusCache campus = CampusCache.All( false ).FirstOrDefault( c => c.ShortCode.Equals( request.Params["Campus"], StringComparison.OrdinalIgnoreCase ) );
            if(campus != null)
                activePagerWorkflows = activePagerWorkflows.WhereAttributeValue(rockContext, "Campus", campus.Guid.ToString());
        }

        DefinedValueCache statusToShow = activePagerWorkflows.ToList().Select( p =>
        {
            p.LoadAttributes();
            return DefinedValueCache.Read( p.GetAttributeValue( "PagerStatus" ).AsGuid(), rockContext );
        } )
            .Where( dv => dv != null )
            .OrderBy( dv => dv.Order )
            .FirstOrDefault();

        if ( statusToShow == null )
        {
            response.Write( "#005500" );
            return;
        }

        var statusToShow2 = new DefinedValueService( rockContext ).Get( statusToShow.Id );
        statusToShow2.LoadAttributes();
        response.Write( statusToShow2.GetAttributeValue( request.Params["Production"] == "true" ? "ProductionTeamBlinkCode" : "KidsTeamBlinkCode" ) );
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}