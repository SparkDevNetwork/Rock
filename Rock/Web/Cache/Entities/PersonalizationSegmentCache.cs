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
using System.Linq.Expressions;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;
using Rock.Personalization;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a Segment
    /// </summary>
    [Serializable]
    [DataContract]
    public class PersonalizationSegmentCache : ModelCache<PersonalizationSegmentCache, PersonalizationSegment>
    {
        #region Properties

        /// <inheritdoc cref="PersonalizationSegment.Name"/>
        [DataMember]
        public string Name { get; private set; }

        /// <inheritdoc cref="PersonalizationSegment.SegmentKey"/>
        [DataMember]
        public string SegmentKey { get; private set; }

        /// <inheritdoc cref="PersonalizationSegment.IsActive"/>
        [DataMember]
        public bool IsActive { get; private set; }

        /// <inheritdoc cref="PersonalizationSegment.FilterDataViewId"/>
        [DataMember]
        public int? FilterDataViewId { get; private set; }

        /// <inheritdoc cref="PersonalizationSegment.AdditionalFilterJson"/>
        [DataMember]
        public string AdditionalFilterJson
        {
            get => AdditionalFilterConfiguration?.ToJson();
            private set => AdditionalFilterConfiguration = value?.FromJsonOrNull<PersonalizationSegmentAdditionalFilterConfiguration>();
        }

        /// <inheritdoc cref="PersonalizationSegment.AdditionalFilterConfiguration"/>
        public PersonalizationSegmentAdditionalFilterConfiguration AdditionalFilterConfiguration { get; private set; }

        /// <summary>
        /// Gets the active segments with an option to include Segments that have a DataView that is not persisted.
        /// Note that Segments that have a DataView that is not persisted are considered inactive.
        /// </summary>
        /// <param name="includeSegmentsWithNonPersistedDataViews">if set to <c>true</c> [include segments with non persisted data views].</param>
        /// <returns></returns>
        public static IEnumerable<PersonalizationSegmentCache> GetActiveSegments( bool includeSegmentsWithNonPersistedDataViews )
        {
            var activeSegments = All().Where( a => a.IsActive );
            if ( includeSegmentsWithNonPersistedDataViews )
            {
                return activeSegments;
            }

            var segmentFilterDataViewIds = activeSegments.Where( a => a.FilterDataViewId.HasValue ).Select( a => a.FilterDataViewId.Value ).ToList();
            if ( !segmentFilterDataViewIds.Any() )
            {
                return activeSegments;
            }

            var nonPersistedDataFilterDataViewIds = new DataViewService( new RockContext() ).GetByIds( segmentFilterDataViewIds )
                .Where( a => a.PersistedScheduleIntervalMinutes == null ).Select( a => a.Id );

            if ( nonPersistedDataFilterDataViewIds.Any() )
            {
                /* 06/22/2022 MP

                Personalization Segments require that if it has a DataView, it must be a persisted DataView.
                The UI will prevent saving the configurion if the the selected Dataview is not persisted,
                but in case the DataView is changed to not persisted after they configured the segment filter,
                we should treat the segment filter as inactive/disabled.

                See discussion at https://app.asana.com/0/0/1202399967339503/f

                */

                activeSegments = activeSegments.Where( a => !a.FilterDataViewId.HasValue || !nonPersistedDataFilterDataViewIds.Contains( a.FilterDataViewId.Value ) );
            }

            return activeSegments;
        }

        /// <summary>
        /// Gets the active segments having keys matching those in the specified list.
        /// </summary>
        /// <param name="segmentKeys">A delimited list of segment keys.</param>
        /// <param name="delimiter">The delimiter used to separate the keys in the list.</param>
        /// <returns></returns>
        public static List<PersonalizationSegmentCache> GetByKeys( string segmentKeys, string delimiter = "," )
        {
            var results = new List<PersonalizationSegmentCache>();

            if ( string.IsNullOrWhiteSpace( segmentKeys ) )
            {
                return results;
            }

            var segmentKeyList = segmentKeys.SplitDelimitedValues( delimiter, StringSplitOptions.RemoveEmptyEntries );
            var segments = PersonalizationSegmentCache.All()
                .Where( ps => ps.IsActive );

            foreach ( var segmentKey in segmentKeyList )
            {
                if ( string.IsNullOrWhiteSpace( segmentKey ) )
                {
                    continue;
                }

                // Retrieve the segment by matching key, ignoring leading/trailing whitespace and case.
                var matchedSegments = segments.Where( s => s.SegmentKey.Equals( segmentKey.Trim(), StringComparison.OrdinalIgnoreCase ) )
                    .ToList();
                if ( matchedSegments.Any() )
                {
                    results.AddRange( matchedSegments );
                }
            }

            return results;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the person alias filters where expression.
        /// </summary>
        /// <param name="personAliasService">The person alias service.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns>Expression.</returns>
        public Expression GetPersonAliasFiltersWhereExpression( PersonAliasService personAliasService, ParameterExpression parameterExpression )
        {
            return PersonalizationSegment.GetPersonAliasFiltersWhereExpression( this.Id, personAliasService, parameterExpression );
        }

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            if ( entity is PersonalizationSegment segment )
            {
                Name = segment.Name;
                SegmentKey = segment.SegmentKey;
                IsActive = segment.IsActive;
                FilterDataViewId = segment.FilterDataViewId;
                AdditionalFilterJson = segment.AdditionalFilterJson;
            }
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