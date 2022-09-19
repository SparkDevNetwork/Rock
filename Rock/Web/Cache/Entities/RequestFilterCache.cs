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

using Rock.Data;
using Rock.Model;
using Rock.Personalization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a RequestFilter
    /// </summary>
    [Serializable]
    [DataContract]
    public class RequestFilterCache : ModelCache<RequestFilterCache, RequestFilter>
    {
        #region Properties

        /// <inheritdoc cref="RequestFilter.Name"/>
        [DataMember]
        public string Name { get; private set; }

        /// <inheritdoc cref="RequestFilter.RequestFilterKey"/>
        [DataMember]
        public string RequestFilterKey { get; private set; }

        /// <inheritdoc cref="RequestFilter.SiteId"/>
        [DataMember]
        public int? SiteId { get; private set; }

        /// <inheritdoc cref="RequestFilter.IsActive"/>
        [DataMember]
        public bool IsActive { get; private set; }

        /// <inheritdoc cref="RequestFilter.FilterJson"/>
        [DataMember]
        public string FilterJson
        {
            get => FilterConfiguration?.ToJson();
            private set => FilterConfiguration = value?.FromJsonOrNull<Rock.Personalization.PersonalizationRequestFilterConfiguration>();
        }

        /// <inheritdoc cref="RequestFilter.FilterConfiguration"/>
        public PersonalizationRequestFilterConfiguration FilterConfiguration { get; private set; }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Returns true if the HttpRequest meets the criteria defined in the FilterConfiguration.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="site">The site.</param>
        /// <returns>
        ///   <c>true</c> if XXXX, <c>false</c> otherwise.
        /// </returns>
        public bool RequestMeetsCriteria( System.Web.HttpRequest request, SiteCache site)
        {
            return Rock.Model.RequestFilter.RequestMeetsCriteria( this.Id, request, site );
        }

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            if ( entity is RequestFilter requestFilter )
            {
                Name = requestFilter.Name;
                RequestFilterKey = requestFilter.RequestFilterKey;
                IsActive = requestFilter.IsActive;
                SiteId = requestFilter.SiteId;
                FilterJson = requestFilter.FilterJson;
            }
        }

        /// <summary>
        /// Gets the active filters having keys matching those in the specified list.
        /// </summary>
        /// <param name="filterKeys">A delimited list of filter keys.</param>
        /// <param name="delimiter">The delimiter used to separate the keys in the list.</param>
        /// <returns></returns>
        public static List<RequestFilterCache> GetByKeys( string filterKeys, string delimiter = "," )
        {
            var results = new List<RequestFilterCache>();

            if ( string.IsNullOrWhiteSpace( filterKeys ) )
            {
                return results;
            }

            var filters = RequestFilterCache.All()
                .Where( rf => rf.IsActive );

            var filterKeyList = filterKeys.SplitDelimitedValues( delimiter, StringSplitOptions.RemoveEmptyEntries );
            foreach ( var filterKey in filterKeyList )
            {
                if ( string.IsNullOrWhiteSpace( filterKey ) )
                {
                    continue;
                }

                // Retrieve the filter by matching key, ignoring leading/trailing whitespace and case.
                var matchedFilters = filters.Where( s => s.RequestFilterKey.Equals( filterKey.Trim(), StringComparison.OrdinalIgnoreCase ) )
                    .ToList();
                if ( matchedFilters.Any() )
                {
                    results.AddRange( matchedFilters );
                }
            }

            return results;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion Public Methods
    }
}