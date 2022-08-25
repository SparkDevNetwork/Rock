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

using Rock.Model;
using Rock.Personalization.SegmentFilters;

namespace Rock.Personalization
{
    /// <summary>
    /// Configuration class for Additional Filters on <see cref="PersonalizationSegment"/>.
    /// </summary>
    public class PersonalizationSegmentAdditionalFilterConfiguration
    {
        /// <summary>
        /// Gets or sets <see cref="FilterExpressionType"/>
        /// </summary>
        /// <value>The type of the session filter expression.</value>
        public FilterExpressionType SessionFilterExpressionType { get; set; } = FilterExpressionType.GroupAll;

        /// <summary>
        /// Gets or sets the session segment filters. These are either AND'd or OR'd depending on <see cref="SessionFilterExpressionType"/>.
        /// </summary>
        /// <value>The session segment filters.</value>
        public List<SessionCountSegmentFilter> SessionSegmentFilters { get; set; } = new List<SessionCountSegmentFilter>();

        /// <summary>
        /// Gets or sets the type of the page view filter expression.
        /// </summary>
        /// <value>The type of the page view filter expression.</value>
        public FilterExpressionType PageViewFilterExpressionType { get; set; } = FilterExpressionType.GroupAll;

        /// <summary>
        /// Gets or sets the page view segment filters. These are either AND'd or OR'd depending on <see cref="PageViewFilterExpressionType"/>.
        /// </summary>
        /// <value>The page view segment filters.</value>
        public List<PageViewSegmentFilter> PageViewSegmentFilters { get; set; } = new List<PageViewSegmentFilter>();

        /// <summary>
        /// Gets or sets the type of the interaction filter expression.
        /// </summary>
        /// <value>The type of the interaction filter expression.</value>
        public FilterExpressionType InteractionFilterExpressionType { get; set; } = FilterExpressionType.GroupAll;

        /// <summary>
        /// Gets or sets the interaction segment filters. These are either AND'd or OR'd depending on <see cref="InteractionFilterExpressionType"/>.
        /// </summary>
        /// <value>The interaction segment filters.</value>
        public List<InteractionSegmentFilter> InteractionSegmentFilters { get; set; } = new List<InteractionSegmentFilter>();
    }
}
