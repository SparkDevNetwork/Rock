// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Linq;
using System.Net;
using System.Web.Http;

using Microsoft.AspNet.OData;

using Rock.Data;
using Rock.Rest;
using Rock.Rest.Filters;

namespace Rock.Controllers
{
    /// <summary>
    /// Search REST API
    /// </summary>
    [Rock.SystemGuid.RestControllerGuid( "1D08E8B4-61AF-4ED7-9201-B64FCC3C22AD")]
    public partial class SearchController : ApiController 
    {
        /// <summary>
        /// GET that returns a list of results based on the Search Type and Term
        /// </summary>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/search" )]
        [RockEnableQuery]
        [Rock.SystemGuid.RestActionGuid( "EBAEC60A-9C0A-45CD-954D-51E56B3BD162" )]
        public IQueryable<string> Get()
        {
            string queryString = Request.RequestUri.Query;
            string type = System.Web.HttpUtility.ParseQueryString( queryString ).Get( "type" );
            string term = System.Web.HttpUtility.ParseQueryString( queryString ).Get( "term" );

            int key = int.MinValue;
            if ( int.TryParse( type, out key ) )
            {
                var searchComponents = Rock.Search.SearchContainer.Instance.Components;
                if ( searchComponents.ContainsKey( key ) )
                {
                    var component = searchComponents[key];
                    return component.Value.Search( term );
                }
            }

            throw new HttpResponseException( HttpStatusCode.BadRequest );
        }
    }
}