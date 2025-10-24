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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.PersonalizationSegmentDetail
{
    /// <summary>
    /// Bag that represents Additional Filter Configuration for a Personalization Segment.
    /// </summary>
    public class AdditionalFilterConfigurationBag
    {
        /// <summary>
        /// Gets or sets the session filter expression type.
        /// Uses integer values of Reporting.FilterExpressionType.
        /// </summary>
        public int SessionFilterExpressionType { get; set; }

        /// <summary>
        /// Gets or sets the session segment filters.
        /// </summary>
        public List<SessionCountSegmentFilterBag> SessionSegmentFilters { get; set; }

        /// <summary>
        /// Gets or sets the page view filter expression type.
        /// Uses integer values of Reporting.FilterExpressionType.
        /// </summary>
        public int PageViewFilterExpressionType { get; set; }

        /// <summary>
        /// Gets or sets the page view segment filters.
        /// </summary>
        public List<PageViewSegmentFilterBag> PageViewSegmentFilters { get; set; }

        /// <summary>
        /// Gets or sets the interaction filter expression type.
        /// Uses integer values of Reporting.FilterExpressionType.
        /// </summary>
        public int InteractionFilterExpressionType { get; set; }

        /// <summary>
        /// Gets or sets the interaction segment filters.
        /// </summary>
        public List<InteractionSegmentFilterBag> InteractionSegmentFilters { get; set; }
    }

    /// <summary>
    /// Bag that represents a session count segment filter configuration.
    /// </summary>
    public class SessionCountSegmentFilterBag
    {
        /// <summary>
        /// Gets or sets the unique identifier for this filter configuration.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the comparison type.
        /// </summary>
        public int ComparisonType { get; set; }

        /// <summary>
        /// Gets or sets the comparison value.
        /// </summary>
        public int ComparisonValue { get; set; }

        /// <summary>
        /// Gets or sets the sites to include.
        /// </summary>
        public List<Guid> SiteGuids { get; set; }

        /// <summary>
        /// Gets or sets the sliding date range values.
        /// </summary>
        public string SlidingDateRangeDelimitedValues { get; set; }

        /// <summary>
        /// Gets or sets the human-readable description.
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// Bag that represents a page view segment filter configuration.
    /// </summary>
    public class PageViewSegmentFilterBag
    {
        /// <summary>
        /// Gets or sets the unique identifier for this filter configuration.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the comparison type.
        /// </summary>
        public int ComparisonType { get; set; }

        /// <summary>
        /// Gets or sets the comparison value.
        /// </summary>
        public int ComparisonValue { get; set; }

        /// <summary>
        /// Gets or sets the sites to include.
        /// </summary>
        public List<Guid> SiteGuids { get; set; }

        /// <summary>
        /// Gets or sets the pages to include.
        /// </summary>
        public List<ListItemBag> PageGuids { get; set; }

        /// <summary>
        /// Gets or sets the sliding date range values.
        /// </summary>
        public string SlidingDateRangeDelimitedValues { get; set; }

        /// <summary>
        /// Gets or sets the page URL comparison type.
        /// </summary>
        public int PageUrlComparisonType { get; set; }

        /// <summary>
        /// Gets or sets the page URL comparison value.
        /// </summary>
        public string PageUrlComparisonValue { get; set; }

        /// <summary>
        /// Gets or sets the referrer comparison type.
        /// </summary>
        public int PageReferrerComparisonType { get; set; }

        /// <summary>
        /// Gets or sets the referrer comparison value.
        /// </summary>
        public string PageReferrerComparisonValue { get; set; }

        /// <summary>
        /// Gets or sets the source comparison type.
        /// </summary>
        public int SourceComparisonType { get; set; }

        /// <summary>
        /// Gets or sets the source comparison value.
        /// </summary>
        public string SourceComparisonValue { get; set; }

        /// <summary>
        /// Gets or sets the medium comparison type.
        /// </summary>
        public int MediumComparisonType { get; set; }

        /// <summary>
        /// Gets or sets the medium comparison value.
        /// </summary>
        public string MediumComparisonValue { get; set; }

        /// <summary>
        /// Gets or sets the campaign comparison type.
        /// </summary>
        public int CampaignComparisonType { get; set; }

        /// <summary>
        /// Gets or sets the campaign comparison value.
        /// </summary>
        public string CampaignComparisonValue { get; set; }

        /// <summary>
        /// Gets or sets the content comparison type.
        /// </summary>
        public int ContentComparisonType { get; set; }

        /// <summary>
        /// Gets or sets the content comparison value.
        /// </summary>
        public string ContentComparisonValue { get; set; }

        /// <summary>
        /// Gets or sets the term comparison type.
        /// </summary>
        public int TermComparisonType { get; set; }

        /// <summary>
        /// Gets or sets the term comparison value.
        /// </summary>
        public string TermComparisonValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include child pages.
        /// </summary>
        public bool IncludeChildPages { get; set; }

        /// <summary>
        /// Gets or sets the human-readable description.
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// Bag that represents an interaction segment filter configuration.
    /// </summary>
    public class InteractionSegmentFilterBag
    {
        /// <summary>
        /// Gets or sets the unique identifier for this filter configuration.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the comparison type.
        /// </summary>
        public int ComparisonType { get; set; }

        /// <summary>
        /// Gets or sets the comparison value.
        /// </summary>
        public int ComparisonValue { get; set; }

        /// <summary>
        /// Gets or sets the interaction channel.
        /// </summary>
        public ListItemBag InteractionChannel { get; set; }

        /// <summary>
        /// Gets or sets the interaction component.
        /// </summary>
        public ListItemBag InteractionComponent { get; set; }

        /// <summary>
        /// Gets or sets the operation.
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Gets or sets the sliding date range values.
        /// </summary>
        public string SlidingDateRangeDelimitedValues { get; set; }
    }
}


