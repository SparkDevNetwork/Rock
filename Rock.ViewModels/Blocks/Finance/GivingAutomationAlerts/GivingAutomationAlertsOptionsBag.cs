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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Finance.GivingAutomationAlerts
{
    /// <summary>
    /// The additional configuration options for the Giving Automation Alerts block.
    /// </summary>
    public class GivingAutomationAlertsOptionsBag
    {
        /// <summary>
        /// Gets or sets the alert types available for the "Alert Types" filter.
        /// </summary>
        public List<ListItemBag> AlertTypeItems { get; set; }

        /// <summary>
        /// Gets or sets the organization's currency formatting details used to render gift amounts.
        /// </summary>
        public CurrencyInfoBag CurrencyInfo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a specific person was supplied through the
        /// page parameters. When <c>true</c> the person filter and the "Name" column are hidden
        /// because the grid is already scoped to that person.
        /// </summary>
        public bool IsPersonContext { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a specific campus was supplied through the
        /// page parameters. When <c>true</c> the campus filter and the "Campus" column are hidden
        /// because the grid is already scoped to that campus.
        /// </summary>
        public bool IsCampusContext { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a specific alert type was supplied through the
        /// page parameters. When <c>true</c> the alert type and alert category filters are hidden
        /// because the grid is already scoped to that alert type.
        /// </summary>
        public bool IsAlertTypeContext { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a start and/or end date was supplied through the
        /// page parameters. When <c>true</c> the date range filter is hidden because the grid is
        /// already scoped to that date range.
        /// </summary>
        public bool IsDateRangeContext { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether more than one campus exists (including inactive
        /// campuses, to match the legacy block's behavior). When <c>false</c> the "Campus" column is
        /// hidden because it would add no information.
        /// </summary>
        public bool HasMultipleCampuses { get; set; }
    }
}
