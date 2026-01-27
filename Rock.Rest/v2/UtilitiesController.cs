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

using Microsoft.Extensions.DependencyInjection;

using Rock.Configuration;
using Rock.Observability;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

#if WEBFORMS
using HttpGetAttribute = System.Web.Http.HttpGetAttribute;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using IActionResult = System.Web.Http.IHttpActionResult;
using RouteAttribute = System.Web.Http.RouteAttribute;
using RoutePrefixAttribute = System.Web.Http.RoutePrefixAttribute;
#endif

namespace Rock.Rest.v2
{
    /// <summary>
    /// API controller for the /api/v2/Utilities endpoints.
    /// </summary>
    /// <seealso cref="Rock.Rest.ApiControllerBase" />
    [RoutePrefix( "api/v2/utilities" )]
    [Rock.SystemGuid.RestControllerGuid( "AE3ABE89-D40C-4EB6-BAAE-C477DAAD71AD" )]
    public partial class UtilitiesController : ApiControllerBase
    {
        /// <summary>
        /// Get a flat list of all the folders and subfolders in a given root folder, excluding a given folder and its children.
        /// </summary>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent all the folders.</returns>
        [HttpPost]
        [Route( "GetImageFileExtensions" )]
        [Authenticate]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_READ, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]
        [Rock.SystemGuid.RestActionGuid( "D0609BC8-CFDB-426C-A2A2-2685BEB63527" )]
        public IActionResult GetImageFileExtensions()
        {
            return Ok( GlobalAttributesCache.Get().GetValue( "ContentImageFiletypeWhitelist" ) );
        }

        /// <summary>
        /// Gets the debug trace details for the specified trace. This endpoint
        /// is used internally and is not intended for public use.
        /// </summary>
        /// <param name="traceId">The identifier of the trace to retrieve.</param>
        /// <returns>A list of objects that represent the trace spans.</returns>
        [HttpGet]
        [Route( "DebugTraceDetails" )]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_READ, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]
        [Rock.SystemGuid.RestActionGuid( "9bca1f10-942d-45c6-97f8-e1b54f76a521" )]
        public IActionResult GetDebugTraceDetails( string traceId )
        {
            var activities = RockApp.Current.GetRequiredService<DebugTraceObserver>().GetTraceActivities( traceId );

            var items = activities
                .Select( a => new
                {
                    TraceId = a.TraceId.ToString(),
                    SpanId = a.SpanId.ToString(),
                    ParentSpanId = a.ParentSpanId != default ? a.ParentSpanId.ToString() : null,
                    Name = a.DisplayName,
                    StartTime = a.StartTimeUtc.ToJavascriptMilliseconds(),
                    Duration = a.Duration.TotalMilliseconds,
                    Tags = a.TagObjects
                        .Select( t => new ListItemBag { Value = t.Key, Text = t.Value.ToStringSafe() } )
                        .OrderBy( t => t.Value )
                        .ToList(),
                } )
                .ToList();

            return Ok( items );
        }
    }
}
