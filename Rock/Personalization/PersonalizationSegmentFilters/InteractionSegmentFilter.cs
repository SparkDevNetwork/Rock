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
using System.Linq;
using System.Linq.Expressions;

using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Personalization.SegmentFilters
{
    /// <summary>
    /// Class InteractionSegmentFilter.
    /// Implements the <see cref="Rock.Personalization.PersonalizationSegmentFilter" />
    /// </summary>
    /// <seealso cref="Rock.Personalization.PersonalizationSegmentFilter" />
    public class InteractionSegmentFilter : PersonalizationSegmentFilter
    {
        #region Configuration

        /// <summary>
        /// Gets or sets the type of the comparison.
        /// </summary>
        /// <value>The type of the comparison.</value>
        public ComparisonType ComparisonType { get; set; } = ComparisonType.GreaterThanOrEqualTo;

        /// <summary>
        /// Gets or sets the comparison value.
        /// The Number of Sessions.
        /// </summary>
        /// <value>The comparison value.</value>
        public int ComparisonValue { get; set; } = 4;

        /// <summary>
        /// <see cref="Rock.Model.InteractionChannel"/> that applies to this filter (required)
        /// </summary>
        /// <value>The interaction channel guid.</value>
        public Guid InteractionChannelGuid { get; set; }

        /// <summary>
        /// <see cref="Rock.Model.InteractionComponent"/> that applies to this filter (optional)
        /// </summary>
        /// <value>The interaction component guid.</value>
        public Guid? InteractionComponentGuid { get; set; }

        /// <inheritdoc cref="Interaction.Operation"/>
        public string Operation { get; set; }

        /// <summary>
        /// Gets or sets the sliding date range <see cref="Rock.Web.UI.Controls.SlidingDateRangePicker.DelimitedValues"/>
        /// </summary>
        /// <value>The sliding date range delimited values.</value>
        public string SlidingDateRangeDelimitedValues { get; set; }

        #endregion Configuration

        /// <summary>
        /// Gets the description based on how the filter is configured.
        /// </summary>
        /// <returns>System.String.</returns>
        public override string GetDescription()
        {
            // Interaction Segment filter doesn't need a detailed description. When shown in a grid, it'll show settings in multiple columns.
            return $"Filter on {InteractionChannelCache.Get( this.InteractionChannelGuid )}";
        }

        /// <summary>
        /// Gets Expression that will be used as one of the WHERE clauses for the PersonAlias query.
        /// </summary>
        /// <param name="personAliasService">The person alias service.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns>Expression.</returns>
        public override Expression GetWherePersonAliasExpression( PersonAliasService personAliasService, ParameterExpression parameterExpression )
        {
            var interactionChannelId = InteractionChannelCache.GetId( this.InteractionChannelGuid );
            if ( interactionChannelId == null )
            {
                // InteractionChannelGuid is required, so this shouldn't happen unless the interaction channel has been deleted
                return null;
            }

            var rockContext = personAliasService.Context as RockContext;
            var interactionsQuery = new InteractionService( rockContext ).Queryable().Where( a => a.PersonAliasId.HasValue && a.InteractionComponent.InteractionChannelId == interactionChannelId );

            int? interactionComponentId;
            if ( this.InteractionComponentGuid.HasValue )
            {
                interactionComponentId = InteractionComponentCache.GetId( this.InteractionComponentGuid.Value );
            }
            else
            {
                interactionComponentId = null;
            }

            if ( interactionComponentId != null )
            {
                interactionsQuery = interactionsQuery.Where( a => a.InteractionComponentId == interactionComponentId );
            }

            var operation = this.Operation;
            if ( operation.IsNotNullOrWhiteSpace() )
            {
                interactionsQuery = interactionsQuery.Where( a => a.Operation == operation );
            }

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( SlidingDateRangeDelimitedValues );
            if ( dateRange?.Start != null )
            {
                interactionsQuery = interactionsQuery.Where( a => a.InteractionDateTime >= dateRange.Start );
            }

            if ( dateRange?.End != null )
            {
                interactionsQuery = interactionsQuery.Where( a => a.InteractionDateTime < dateRange.End );
            }

            var personAliasQuery = personAliasService.Queryable();

            var comparisonType = this.ComparisonType;
            var comparisonValue = this.ComparisonValue;

            // Filter by the interaction count
            var personAliasCompareEqualQuery = personAliasQuery.Where( p =>
                interactionsQuery.Where( i => i.PersonAliasId == p.Id ).Count() == comparisonValue );

            BinaryExpression compareEqualExpression = FilterExpressionExtractor.Extract<Rock.Model.PersonAlias>( personAliasCompareEqualQuery, parameterExpression, "p" ) as BinaryExpression;
            return FilterExpressionExtractor.AlterComparisonType( comparisonType, compareEqualExpression, 0 );
        }
    }
}
