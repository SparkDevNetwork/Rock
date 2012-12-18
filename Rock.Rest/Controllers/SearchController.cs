//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Linq;
using System.Net;
using System.Web.Http;


namespace Rock.Controllers
{
    /// <summary>
    /// Search REST API
    /// </summary>
    public partial class SearchController : ApiController
    {
        // GET api/<controller>
        public IQueryable<string> Get()
        {
            string queryString = Request.RequestUri.Query;
            string type = System.Web.HttpUtility.ParseQueryString( queryString ).Get( "type" );
            string term = System.Web.HttpUtility.ParseQueryString( queryString ).Get( "term" );

            int key = int.MinValue;
            if (int.TryParse(type, out key))
            {
                var searchComponents = Rock.Search.SearchContainer.Instance.Components;
                if (searchComponents.ContainsKey(key))
                {
                    var component = searchComponents[key];
                    return component.Value.Search( term );
                }
            }

            throw new HttpResponseException(HttpStatusCode.BadRequest);
        }
    }
}
