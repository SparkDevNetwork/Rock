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
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Lava
{
    /// <summary>
    /// A helper that provides functions for working with personalization data in Lava.
    /// </summary>
    internal static class LavaPersonalizationHelper
    {

        /// <summary>
        /// Get a list of personalization segment identifiers that apply to the current visitor.
        /// </summary>
        /// <param name="currentPerson"></param>
        /// <param name="rockContext"></param>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        public static List<int> GetPersonalizationSegmentIdListForRequest( Person currentPerson, RockContext rockContext, HttpRequest httpRequest )
        {
            List<int> personSegmentIdList = null;
            RockPage rockPage = null;

            if ( httpRequest != null )
            {
                rockPage = System.Web.HttpContext.Current?.CurrentHandler as RockPage;
            }

            // Get Personalization Segments.
            if ( rockPage != null )
            {
                // If this block is executing in the context of a RockPage,
                // try to get the personalization segments that have been previously determined.
                personSegmentIdList = rockPage.PersonalizationSegmentIds?.ToList();
            }

            if ( personSegmentIdList == null )
            {
                // Get the segments for the person in the current context.
                if ( currentPerson == null )
                {
                    personSegmentIdList = new List<int>();
                }
                else
                {
                    personSegmentIdList = GetPersonalizationSegmentIdListForPerson( currentPerson, rockContext );
                }
            }

            return personSegmentIdList;
        }

        /// <summary>
        /// Get a list of personalization segment identifiers that apply to the specified person.
        /// </summary>
        /// <param name="person">A person object.</param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        public static List<int> GetPersonalizationSegmentIdListForPerson( Person person, RockContext rockContext )
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
    }
}
