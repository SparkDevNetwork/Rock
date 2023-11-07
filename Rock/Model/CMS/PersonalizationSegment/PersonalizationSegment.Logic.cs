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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq.Expressions;

using Rock.Personalization;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class PersonalizationSegment
    {
        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return PersonalizationSegmentCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Data.DbContext dbContext )
        {
            PersonalizationSegmentCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion ICacheable

        #region PersonalizationSegmentAdditionalFilterConfiguration

        /// <summary>
        /// Configuration for the Additional Segment Filters
        /// </summary>
        /// <value>The additional filter.</value>
        [NotMapped]
        public virtual PersonalizationSegmentAdditionalFilterConfiguration AdditionalFilterConfiguration
        {
            get
            {
                if ( AdditionalFilterJson.IsNullOrWhiteSpace() )
                {
                    return new PersonalizationSegmentAdditionalFilterConfiguration();
                }

                return AdditionalFilterJson.FromJsonOrNull<PersonalizationSegmentAdditionalFilterConfiguration>() ?? new PersonalizationSegmentAdditionalFilterConfiguration();
            }

            set
            {
                AdditionalFilterJson = value?.ToJson();
            }
        }

        #endregion PersonalizationSegmentAdditionalFilterConfiguration

        #region Methods

        /// <summary>
        /// Gets the page view segment filters where expression.
        /// </summary>
        /// <param name="segmentFilters">The segment filters.</param>
        /// <param name="filterExpressionType">Type of the filter expression.</param>
        /// <param name="personAliasService">The person alias service.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns>Expression.</returns>
        private static Expression CombineSegmentFilters( IEnumerable<PersonalizationSegmentFilter> segmentFilters, FilterExpressionType filterExpressionType, PersonAliasService personAliasService, ParameterExpression parameterExpression )
        {
            Expression allPageViewSegmentFiltersExpression = null;

            foreach ( var pageViewSegment in segmentFilters )
            {
                var segmentWhereExpression = pageViewSegment.GetWherePersonAliasExpression( personAliasService, parameterExpression );
                allPageViewSegmentFiltersExpression = AppendExpression( allPageViewSegmentFiltersExpression, segmentWhereExpression, filterExpressionType );
            }

            if ( allPageViewSegmentFiltersExpression == null )
            {
                // if there aren't any 'where' expressions, don't filter
                allPageViewSegmentFiltersExpression = Expression.Constant( true );
            }

            return allPageViewSegmentFiltersExpression;
        }

        /// <summary>
        /// Appends the expression.
        /// </summary>
        /// <param name="allSegmentFiltersExpression">All segment filters expression.</param>
        /// <param name="segmentWhereExpression">The segment where expression.</param>
        /// <param name="filterExpressionType">Type of the filter expression.</param>
        /// <returns>Expression.</returns>
        private static Expression AppendExpression( Expression allSegmentFiltersExpression, Expression segmentWhereExpression, FilterExpressionType filterExpressionType )
        {
            if ( segmentWhereExpression == null )
            {
                return allSegmentFiltersExpression;
            }

            if ( allSegmentFiltersExpression == null )
            {
                allSegmentFiltersExpression = segmentWhereExpression;
            }
            else
            {
                if ( filterExpressionType == FilterExpressionType.GroupAll )
                {
                    allSegmentFiltersExpression = Expression.AndAlso( allSegmentFiltersExpression, segmentWhereExpression );
                }
                else if ( filterExpressionType == FilterExpressionType.GroupAny )
                {
                    allSegmentFiltersExpression = Expression.Or( allSegmentFiltersExpression, segmentWhereExpression );
                }
            }

            return allSegmentFiltersExpression;
        }

        /// <summary>
        /// Gets the final person alias filters where expression.
        /// </summary>
        /// <param name="personalizationSegmentId">The personalization segment identifier.</param>
        /// <param name="personAliasService">The person alias service.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns>System.Linq.Expressions.Expression.</returns>
        internal static Expression GetPersonAliasFiltersWhereExpression( int personalizationSegmentId, PersonAliasService personAliasService, ParameterExpression parameterExpression )
        {
            Expression finalExpression = null;
            var personalizationSegmentCache = PersonalizationSegmentCache.Get(personalizationSegmentId );
            var filterConfiguration = personalizationSegmentCache.AdditionalFilterConfiguration;
            var sessionSegmentFiltersWhereExpression = CombineSegmentFilters( filterConfiguration.SessionSegmentFilters, filterConfiguration.SessionFilterExpressionType, personAliasService, parameterExpression );
            var pageViewSegmentFiltersWhereExpression = CombineSegmentFilters( filterConfiguration.PageViewSegmentFilters, filterConfiguration.PageViewFilterExpressionType, personAliasService, parameterExpression );
            var interactionSegmentFiltersWhereExpression = CombineSegmentFilters( filterConfiguration.InteractionSegmentFilters, filterConfiguration.InteractionFilterExpressionType, personAliasService, parameterExpression );

            finalExpression = AppendExpression( finalExpression, sessionSegmentFiltersWhereExpression, FilterExpressionType.GroupAll );
            finalExpression = AppendExpression( finalExpression, pageViewSegmentFiltersWhereExpression, FilterExpressionType.GroupAll );
            finalExpression = AppendExpression( finalExpression, interactionSegmentFiltersWhereExpression, FilterExpressionType.GroupAll );

            return finalExpression;
        }

        #endregion Methods
    }
}