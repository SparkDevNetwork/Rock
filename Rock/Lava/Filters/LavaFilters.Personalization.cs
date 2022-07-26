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
using System;
using System.Collections.Generic;
using System.Linq;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Lava
{
    internal static partial class LavaFilters
    {
        /// <summary>
        /// Gets the set of personalization items that are relevant to the specified person.
        /// </summary>
        /// <param name="context">The Lava context.</param>
        /// <param name="input">The filter input, a reference to a Person or a Person object.</param>
        /// <param name="itemTypeList">A comma-delimited list of item types to return.</param>
        /// <returns>The value of the user preference.</returns>
        public static List<PersonalizationItemInfo> PersonalizationItems( ILavaRenderContext context, object input, string itemTypeList )
        {
            var items = new List<PersonalizationItemInfo>();
            var person = LavaHelper.GetPersonFromInputParameter( input, context );

            var personalizationTypes = itemTypeList.SplitDelimitedValues( "," )
                .Select( x => x.ToStringSafe().Trim().ToLower() )
                .Where ( x => x != string.Empty )
                .ToList();

            // Add the Personalization Segments if we have a specified person.
            if ( person != null )
            {
                if ( !personalizationTypes.Any() || personalizationTypes.Contains( "segments" ) )
                {
                    var rockContext = LavaHelper.GetRockContextFromLavaContext( context );
                    var personSegmentIdList = LavaPersonalizationHelper.GetPersonalizationSegmentIdListForPerson( person, rockContext );

                    var segments = PersonalizationSegmentCache.All()
                        .Where( ps => personSegmentIdList.Contains( ps.Id ) )
                        .Select( ps => new PersonalizationItemInfo
                        {
                            Type = PersonalizationType.Segment,
                            Id = ps.Id,
                            Key = ps.SegmentKey,
                            Name = ps.Name
                        } );
                    items.AddRange( segments );
                }
            }

            // Get the Request Filters that are relevant to the current request.
            if ( !personalizationTypes.Any() || personalizationTypes.Contains( "requestfilters" ) )
            {
                var requestFilterIdList = LavaPersonalizationHelper.GetPersonalizationRequestFilterIdList();

                var filters = RequestFilterCache.All()
                    .Where( rf => requestFilterIdList.Contains( rf.Id ) )
                    .Select( rf => new PersonalizationItemInfo
                    {
                        Type = PersonalizationType.RequestFilter,
                        Id = rf.Id,
                        Key = rf.RequestFilterKey,
                        Name = rf.Name
                    } );
                items.AddRange( filters );
            }

            // Return an ordered list of results.
            items = items.OrderBy( i => i.Type ).ThenBy( i => i.Key ).ToList();
            return items;
        }
    }

    /// <summary>
    /// A Lava data object that represents a Personalization Item.
    /// </summary>
    public class PersonalizationItemInfo : RockDynamic
    {
        /// <summary>
        /// The unique identifier of the personalization item.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The unique key by which the personalization item is referenced in Lava.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The type of personalization item.
        /// For example, a Personalization Segment or a Request Filter.
        /// </summary>
        public PersonalizationType Type { get; set; }

        /// <summary>
        /// The friendly name of the personalization item.
        /// </summary>
        public string Name { get; set; }
    }
}
