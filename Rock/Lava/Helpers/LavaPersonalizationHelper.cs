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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Rock.Data;
using Rock.Model;
using Rock.Personalization;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Lava
{
    /// <summary>
    /// A helper that provides functions for working with personalization data in Lava.
    /// </summary>
    internal static class LavaPersonalizationHelper
    {
        private const string PersonalizationSegmentPrefix = "PersonalizationSegmentIdList_";

        /// <summary>
        /// Get a list of personalization segment identifiers that apply to a specified person and Lava render context.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="httpContext"></param>
        /// <param name="person">The person for whom the personalization segments will be retrieved. If not specified, the current user is assumed.</param>
        /// <returns></returns>
        public static List<int> GetPersonalizationSegmentIdListForPersonFromContextCookie( ILavaRenderContext context, HttpContext httpContext = null, Person person = null )
        {
            // Check if the personalization segments exist in the current lava context.
            person = person ?? LavaHelper.GetCurrentPerson( context );
            if ( person == null )
            {
                return new List<int>();
            }

            var key = $"{PersonalizationSegmentPrefix}{person.Guid}";
            var personSegmentIdList = context.GetInternalField( key, null ) as List<int>;

            if ( personSegmentIdList == null )
            {
                var rockContext = LavaHelper.GetRockContextFromLavaContext( context );

                if ( httpContext != null )
                {
                    // Try to get the segment list from the HttpContext cookie.
                    var segmentCookie = GetPersonalizationSegmentCookieData( person, httpContext );

                    if ( segmentCookie != null )
                    {
                        personSegmentIdList = segmentCookie.GetSegmentIds().ToList();
                    }
                }

                // Retrieve the personalization segments from the database.
                if ( personSegmentIdList == null )
                {
                    personSegmentIdList = GetPersonalizationSegmentIdListForPersonFromDatabase( person, rockContext );
                }

                // Cache the segment list in the render context.
                // It will be available for the remainder of the current template render operation.
                context.SetInternalField( key, personSegmentIdList );
            }

            return personSegmentIdList;
        }

        /// <summary>
        /// Sets the personalization segments for the current context.
        /// </summary>
        /// <param name="segmentIdList">The list of personalization segment identifiers to be set on the context.</param>
        /// <param name="lavaContext">The current lava context that is handling rendering.</param>
        /// <param name="httpContext"></param>
        /// <param name="person"></param>
        /// <returns></returns>
        public static void SetPersonalizationSegmentsForContext( List<int> segmentIdList, ILavaRenderContext lavaContext, HttpContext httpContext, Person person )
        {
            // If no target person is specified, get the current person from the Lava context.
            person = person ?? LavaHelper.GetCurrentPerson( lavaContext );

            var personalizationPersonAliasId = person?.PrimaryAliasId;
            if ( !personalizationPersonAliasId.HasValue )
            {
                return;
            }

            var key = $"{PersonalizationSegmentPrefix}{person.Guid}";

            lavaContext.SetInternalField( key, segmentIdList );

            if ( httpContext != null )
            {
                var segmentFilterCookieData = new SegmentFilterCookieData();
                segmentFilterCookieData.PersonAliasIdKey = IdHasher.Instance.GetHash( personalizationPersonAliasId.Value );
                segmentFilterCookieData.LastUpdateDateTime = RockDateTime.Now;

                var rockContext = LavaHelper.GetRockContextFromLavaContext( lavaContext );
                var personalizationService = new PersonalizationSegmentService( rockContext );

                var segmentIdKeys = segmentIdList
                    .Select( a => IdHasher.Instance.GetHash( a ) )
                    .ToArray();
                segmentFilterCookieData.SegmentIdKeys = segmentIdKeys;

                var newCookie = new HttpCookie( Rock.Personalization.RequestCookieKey.ROCK_SEGMENT_FILTERS, segmentFilterCookieData.ToJson() );

                SetCookie( httpContext, newCookie );
            }
        }

        /// <summary>
        /// Get a list of personalization segment identifiers that apply to the specified person.
        /// </summary>
        /// <param name="person">A person object.</param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        private static List<int> GetPersonalizationSegmentIdListForPersonFromDatabase( Person person, RockContext rockContext )
        {
            List<int> personSegmentIdList = null;

            if ( person == null )
            {
                personSegmentIdList = new List<int>();
            }
            else
            {
                // Get the active segments associated with the current person.
                var segmentService = new PersonalizationSegmentService( rockContext );

                var allPersonSegmentIdList = segmentService.GetPersonAliasPersonalizationSegmentQuery()
                    .Where( pap => pap.PersonAliasId == person.PrimaryAliasId.Value )
                    .Select( pap => pap.PersonalizationEntityId );

                personSegmentIdList = segmentService.Queryable()
                    .Where( s => s.IsActive && allPersonSegmentIdList.Contains( s.Id ) )
                    .Select( s => s.Id )
                    .ToList();
            }

            return personSegmentIdList;
        }

        /// <summary>
        /// Get a list of request filter identifiers that apply to the current request.
        /// </summary>
        /// <returns></returns>
        public static List<int> GetPersonalizationRequestFilterIdList()
        {
            var requestFilterIdList = new List<int>();

            var httpRequest = System.Web.HttpContext.Current?.Request;
            if ( httpRequest != null )
            {
                var rockPage = System.Web.HttpContext.Current?.CurrentHandler as RockPage;

                // Get Request Filters.
                // Prefer any previously determined filters if we are executing in the context of a RockPage,
                // otherwise evaluate the filters for the current request.
                if ( rockPage?.PersonalizationRequestFilterIds != null )
                {
                    requestFilterIdList = rockPage.PersonalizationRequestFilterIds.ToList();
                }
                else
                {
                    // Get the matching filters for the request.
                    var requestFilters = RequestFilterCache.All().Where( a => a.IsActive );
                    foreach ( var requestFilter in requestFilters )
                    {
                        if ( requestFilter.RequestMeetsCriteria( httpRequest, null ) )
                        {
                            requestFilterIdList.Add( requestFilter.Id );
                        }
                    }
                }
            }

            return requestFilterIdList;
        }

        private static SegmentFilterCookieData GetPersonalizationSegmentCookieData( Person currentPerson, HttpContext httpContext )
        {
            var rockSegmentFiltersCookie = GetCookie( httpContext, RequestCookieKey.ROCK_SEGMENT_FILTERS );
            var personalizationPersonAliasId = currentPerson.PrimaryAliasId;
            if ( !personalizationPersonAliasId.HasValue )
            {
                // no visitor or person logged in
                return new SegmentFilterCookieData();
            }

            var cookieValueJson = rockSegmentFiltersCookie?.Value;
            SegmentFilterCookieData segmentFilterCookieData = null;
            if ( cookieValueJson != null )
            {
                segmentFilterCookieData = cookieValueJson.FromJsonOrNull<SegmentFilterCookieData>();
                bool isCookieDataValid = false;
                if ( segmentFilterCookieData != null )
                {
                    if ( segmentFilterCookieData.IsSamePersonAlias( personalizationPersonAliasId.Value ) && segmentFilterCookieData.SegmentIdKeys != null )
                    {
                        isCookieDataValid = true;
                    }

                    if ( segmentFilterCookieData.IsStale( RockDateTime.Now ) )
                    {
                        isCookieDataValid = false;
                    }
                }

                if ( !isCookieDataValid )
                {
                    segmentFilterCookieData = null;
                }
            }

            if ( segmentFilterCookieData == null )
            {
                segmentFilterCookieData = new SegmentFilterCookieData();
                segmentFilterCookieData.PersonAliasIdKey = IdHasher.Instance.GetHash( personalizationPersonAliasId.Value );
                segmentFilterCookieData.LastUpdateDateTime = RockDateTime.Now;
                var segmentIdKeys = new PersonalizationSegmentService( new RockContext() ).GetPersonalizationSegmentIdKeysForPersonAliasId( personalizationPersonAliasId.Value );
                segmentFilterCookieData.SegmentIdKeys = segmentIdKeys;
            }

            return segmentFilterCookieData;
        }

        /// <summary>
        /// Gets the specified cookie. If the cookie is not found in the Request then it checks the Response, otherwise it will return null.
        /// </summary>
        /// <param name="httpContext">The context.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        private static HttpCookie GetCookie( HttpContext httpContext, string name )
        {
            // Make sure the Cookies AllKeys contains a cookie with that name first,
            // otherwise it will automatically create the cookie.
            var request = httpContext.Request;
            if ( request != null && request.Cookies.AllKeys.Contains( name ) )
            {
                return request.Cookies[name];
            }

            var response = httpContext.Response;
            if ( response != null && response.Cookies.AllKeys.Contains( name ) )
            {
                return response.Cookies[name];
            }

            return null;
        }

        /// <summary>
        /// Sets the specified cookie in the active HttpContext.
        /// </summary>
        /// <param name="httpContext">The context.</param>
        /// <param name="cookie">The cookie.</param>
        /// <returns></returns>
        private static void SetCookie( HttpContext httpContext, HttpCookie cookie )
        {
            // Make sure the Cookies AllKeys contains a cookie with that name first,
            // otherwise it will automatically create the cookie.
            var request = httpContext.Request;
            if ( request != null )
            {
                request.Cookies.Set( cookie );
            }

            var response = httpContext.Response;
            if ( response != null )
            {
                response.Cookies.Set( cookie );
            }
        }
    }
}
