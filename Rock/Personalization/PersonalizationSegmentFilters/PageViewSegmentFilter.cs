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

using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Personalization.SegmentFilters
{
    /// <summary>
    /// Class PageViewSegmentFilter.
    /// Implements the <see cref="Rock.Personalization.PersonalizationSegmentFilter" />
    /// </summary>
    /// <seealso cref="Rock.Personalization.PersonalizationSegmentFilter" />
    public class PageViewSegmentFilter : PersonalizationSegmentFilter
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
        public int ComparisonValue { get; set; } = 0;

        /// <summary>
        /// List of <see cref="Rock.Model.Site">sites</see> that apply to this filter (Required)
        /// </summary>
        /// <value>The site guids.</value>
        public List<Guid> SiteGuids { get; set; } = new List<Guid>();

        /// <summary>
        /// List of <see cref="Rock.Model.Page">pages</see> that apply to this filter (Optional)
        /// </summary>
        /// <value>The site guids.</value>
        public List<Guid> PageGuids { get; set; } = new List<Guid>();

        /// <summary>
        /// Gets or sets the sliding date range <see cref="Rock.Web.UI.Controls.SlidingDateRangePicker.DelimitedValues"/>
        /// </summary>
        /// <value>The sliding date range delimited values.</value>
        public string SlidingDateRangeDelimitedValues { get; set; }

        /// <summary>
        /// Gets or sets the type of the page URL comparison.
        /// </summary>
        /// <value>
        /// The type of the page URL comparison.
        /// </value>
        public ComparisonType PageUrlComparisonType { get; set; } = ComparisonType.StartsWith;

        /// <summary>
        /// Gets or sets the page URL comparison value.
        /// </summary>
        /// <value>
        /// The page URL comparison value.
        /// </value>
        public string PageUrlComparisonValue { get; set; }

        /// <summary>
        /// Gets or sets the type of the page referrer comaprison.
        /// </summary>
        /// <value>
        /// The type of the page referrer comaprison.
        /// </value>
        public ComparisonType PageReferrerComparisonType { get; set; } = ComparisonType.StartsWith;

        /// <summary>
        /// Gets or sets the page referrer comaprison value.
        /// </summary>
        /// <value>
        /// The page referrer comaprison value.
        /// </value>
        public string PageReferrerComparisonValue { get; set; }

        #endregion Configuration

        /// <summary>
        /// Gets the selected sites.
        /// </summary>
        /// <returns>SiteCache[].</returns>
        public SiteCache[] GetSelectedSites() => SiteGuids?.Select( a => SiteCache.Get( a ) ).Where( a => a != null ).ToArray() ?? new SiteCache[0];

        /// <summary>
        /// Gets the selected pages.
        /// </summary>
        /// <returns>PageCache[].</returns>
        public PageCache[] GetSelectedPages() => PageGuids?.Select( a => PageCache.Get( a ) ).Where( a => a != null ).ToArray() ?? new PageCache[0];

        /// <summary>
        /// Gets the description based on how the filter is configured.
        /// </summary>
        /// <returns>System.String.</returns>
        public override string GetDescription()
        {
            ComparisonType comparisonType = this.ComparisonType;
            var comparisonValue = this.ComparisonValue;
            string comparisonPhrase;

            if ( comparisonType == ComparisonType.IsBlank )
            {
                comparisonPhrase = "Has had no page views";
            }
            else if ( comparisonType == ComparisonType.IsNotBlank )
            {
                comparisonPhrase = "Has had page views";
            }
            else
            {
                comparisonPhrase = $"Has had {comparisonType.ConvertToString().ToLower()} {ComparisonValue} page views";
            }

            var siteNames = GetSelectedSites().Select( a => a.Name ).ToList();
            string onTheSites = siteNames.AsDelimited( ", ", " or " ) + " website(s)";

            var dateRangeType = SlidingDateRangePicker.GetSlidingDateRangeTypeFromDelimitedValues( SlidingDateRangeDelimitedValues );
            string inTheDateRange;
            if ( dateRangeType == SlidingDateRangePicker.SlidingDateRangeType.DateRange )
            {
                inTheDateRange = "from " + SlidingDateRangePicker.FormatDelimitedValues( SlidingDateRangeDelimitedValues ).ToLower();
            }
            else if ( dateRangeType == SlidingDateRangePicker.SlidingDateRangeType.All )
            {
                // No Date Range specified
                inTheDateRange = "";
            }
            else
            {
                inTheDateRange = "in the " + SlidingDateRangePicker.FormatDelimitedValues( SlidingDateRangeDelimitedValues ).ToLower();
            }

            var pageNames = GetSelectedPages().Select( a => a.ToString() ).ToList();
            string limitedToPages = null;
            if ( pageNames.Any() )
            {
                limitedToPages = $"limited to the {pageNames.AsDelimited( ",", " and " )} pages";
            }

            string requestDetails = PageUrlComparisonValue.IsNotNullOrWhiteSpace() || PageReferrerComparisonValue.IsNotNullOrWhiteSpace() ? " where " : string.Empty;
            if ( PageUrlComparisonValue.IsNotNullOrWhiteSpace() )
            {
                requestDetails += $"page url {PageUrlComparisonType.ConvertToString()} {PageUrlComparisonValue} ";
            }

            if ( PageReferrerComparisonValue.IsNotNullOrWhiteSpace() )
            {
                requestDetails += $"and referrer {PageReferrerComparisonType.ConvertToString()} {PageReferrerComparisonValue}";
            }

            var description = $"{comparisonPhrase} {onTheSites} {inTheDateRange} {limitedToPages} {requestDetails}";
            return description.Trim() + ".";
        }

        /// <summary>
        /// Gets Expression that will be used as one of the WHERE clauses for the PersonAlias query.
        /// </summary>
        /// <param name="personAliasService">The person alias service.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns>Expression.</returns>
        public override Expression GetWherePersonAliasExpression( PersonAliasService personAliasService, ParameterExpression parameterExpression )
        {
            var siteIds = GetSelectedSites().Select( a => a.Id ).ToArray();
            if ( !siteIds.Any() )
            {
                return null;
            }

            var selectedPageIds = GetSelectedPages().Select( a => a.Id ).ToArray();

            IQueryable<Interaction> pageViewsInteractionsQuery;

            var rockContext = personAliasService.Context as RockContext;

            if ( selectedPageIds.Any() )
            {
                pageViewsInteractionsQuery = new InteractionService( rockContext ).GetPageViewsByPage( siteIds, selectedPageIds ).Where( a => a.PersonAliasId.HasValue );
            }
            else
            {
                pageViewsInteractionsQuery = new InteractionService( rockContext ).GetPageViewsBySite( siteIds ).Where( a => a.PersonAliasId.HasValue );
            }

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( SlidingDateRangeDelimitedValues );
            if ( dateRange?.Start != null )
            {
                pageViewsInteractionsQuery = pageViewsInteractionsQuery.Where( a => a.InteractionDateTime >= dateRange.Start );
            }

            if ( dateRange?.End != null )
            {
                pageViewsInteractionsQuery = pageViewsInteractionsQuery.Where( a => a.InteractionDateTime < dateRange.End );
            }

            if ( PageUrlComparisonValue.IsNotNullOrWhiteSpace() )
            {
                switch ( PageUrlComparisonType )
                {
                    case ComparisonType.EqualTo:
                        pageViewsInteractionsQuery = pageViewsInteractionsQuery.Where( i => i.InteractionData.ToUpper() == PageUrlComparisonValue.ToUpper() );
                        break;
                    case ComparisonType.NotEqualTo:
                        pageViewsInteractionsQuery = pageViewsInteractionsQuery.Where( i => i.InteractionData.ToUpper() != PageUrlComparisonValue.ToUpper() );
                        break;
                    case ComparisonType.StartsWith:
                        pageViewsInteractionsQuery = pageViewsInteractionsQuery.Where( i => i.InteractionData.StartsWith( PageUrlComparisonValue ) );
                        break;
                    case ComparisonType.Contains:
                        pageViewsInteractionsQuery = pageViewsInteractionsQuery.Where( i => i.InteractionData.Contains( PageUrlComparisonValue ) );
                        break;
                    case ComparisonType.DoesNotContain:
                        pageViewsInteractionsQuery = pageViewsInteractionsQuery.Where( i => !i.InteractionData.Contains( PageUrlComparisonValue ) );
                        break;
                    case ComparisonType.IsBlank:
                        pageViewsInteractionsQuery = pageViewsInteractionsQuery.Where( i => string.IsNullOrWhiteSpace( i.InteractionData ) );
                        break;
                    case ComparisonType.IsNotBlank:
                        pageViewsInteractionsQuery = pageViewsInteractionsQuery.Where( i => !string.IsNullOrWhiteSpace( i.InteractionData ) );
                        break;
                    case ComparisonType.EndsWith:
                        pageViewsInteractionsQuery = pageViewsInteractionsQuery.Where( i => i.InteractionData.EndsWith( PageUrlComparisonValue ) );
                        break;
                }
            }

            if ( PageReferrerComparisonValue.IsNotNullOrWhiteSpace() )
            {
                switch ( PageReferrerComparisonType )
                {
                    case ComparisonType.EqualTo:
                        pageViewsInteractionsQuery = pageViewsInteractionsQuery.Where( i => i.ChannelCustomIndexed1.ToUpper() == PageReferrerComparisonValue.ToUpper() );
                        break;
                    case ComparisonType.NotEqualTo:
                        pageViewsInteractionsQuery = pageViewsInteractionsQuery.Where( i => i.ChannelCustomIndexed1.ToUpper() != PageReferrerComparisonValue.ToUpper() );
                        break;
                    case ComparisonType.StartsWith:
                        pageViewsInteractionsQuery = pageViewsInteractionsQuery.Where( i => i.ChannelCustomIndexed1.StartsWith( PageReferrerComparisonValue ) );
                        break;
                    case ComparisonType.Contains:
                        pageViewsInteractionsQuery = pageViewsInteractionsQuery.Where( i => i.ChannelCustomIndexed1.Contains( PageReferrerComparisonValue ) );
                        break;
                    case ComparisonType.DoesNotContain:
                        pageViewsInteractionsQuery = pageViewsInteractionsQuery.Where( i => !i.ChannelCustomIndexed1.Contains( PageReferrerComparisonValue ) );
                        break;
                    case ComparisonType.IsBlank:
                        pageViewsInteractionsQuery = pageViewsInteractionsQuery.Where( i => string.IsNullOrWhiteSpace( i.InteractionData ) );
                        break;
                    case ComparisonType.IsNotBlank:
                        pageViewsInteractionsQuery = pageViewsInteractionsQuery.Where( i => !string.IsNullOrWhiteSpace( i.InteractionData ) );
                        break;
                    case ComparisonType.EndsWith:
                        pageViewsInteractionsQuery = pageViewsInteractionsQuery.Where( i => i.ChannelCustomIndexed1.EndsWith( PageReferrerComparisonValue ) );
                        break;
                }
            }

            var personAliasQuery = personAliasService.Queryable();

            var comparisonType = this.ComparisonType;
            var comparisonValue = this.ComparisonValue;

            // filter by the Page View count
            var personAliasCompareEqualQuery = personAliasQuery.Where( p =>
                pageViewsInteractionsQuery.Where( i => i.PersonAliasId == p.Id ).Count() == comparisonValue );

            BinaryExpression compareEqualExpression = FilterExpressionExtractor.Extract<Rock.Model.PersonAlias>( personAliasCompareEqualQuery, parameterExpression, "p" ) as BinaryExpression;
            BinaryExpression result = FilterExpressionExtractor.AlterComparisonType( comparisonType, compareEqualExpression, 0 );
            return result;
        }
    }
}