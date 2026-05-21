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

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// Represents an attribute available in the View Options modal's Attribute Filters
    /// section, optionally scoped to one or more Connection Opportunities so the client
    /// can hide it when an unrelated opportunity is selected as the active filter.
    /// </summary>
    public class ConnectionRequestAttributeFilterBag
    {
        /// <summary>
        /// Gets or sets the attribute, in public form, used by the client to render the filter UI.
        /// </summary>
        public PublicAttributeBag Attribute { get; set; }

        /// <summary>
        /// Gets or sets the Connection Opportunity Guids that scope this attribute. A null
        /// or empty list indicates the attribute is defined at the Connection Type level and
        /// is always available regardless of filter. When populated, the filter should only
        /// be shown when no opportunity filter is active or when the active filter matches
        /// one of these Guids.
        /// </summary>
        public List<Guid> ConnectionOpportunityGuids { get; set; }
    }
}
