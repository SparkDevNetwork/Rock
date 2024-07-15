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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Reporting.TithingOverview
{
    /// <summary>
    /// Contains all the tooltip information for the Tithing Overview block charts.
    /// </summary>
    public class TithingOverviewToolTipBag
    {
        /// <summary>
        /// Gets or sets the campus identifier key.
        /// </summary>
        /// <value>
        /// The campus identifier key.
        /// </value>
        public int CampusId { get; set; }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>
        /// The date.
        /// </value>
        public string Date { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public decimal? Value { get; set; }

        /// <summary>
        /// Gets or sets the campus.
        /// </summary>
        /// <value>
        /// The campus.
        /// </value>
        public string Campus { get; set; }

        /// <summary>
        /// Gets or sets the tithe metric.
        /// </summary>
        /// <value>
        /// The tithe metric.
        /// </value>
        public decimal? TitheMetric { get; set; }

        /// <summary>
        /// Gets or sets the campus age.
        /// </summary>
        /// <value>
        /// The campus age.
        /// </value>
        public int? CampusAge { get; set; }

        /// <summary>
        /// Gets or sets the giving house holds.
        /// </summary>
        /// <value>
        /// The giving house holds.
        /// </value>
        public decimal? GivingHouseHolds { get; set; }

        /// <summary>
        /// Gets or sets the tithing house holds.
        /// </summary>
        /// <value>
        /// The tithing house holds.
        /// </value>
        public decimal? TithingHouseHolds { get; set; }

        /// <summary>
        /// Gets or sets the campus opening date.
        /// </summary>
        /// <value>
        /// The campus opening date.
        /// </value>
        public DateTime? CampusOpenedDate { get; set; }

        /// <summary>
        /// Gets or sets the campus closing date.
        /// </summary>
        /// <value>
        /// The campus closing date.
        /// </value>
        public DateTime? CampusClosedDate { get; set; }

        /// <summary>
        /// Gets or sets the campus short code.
        /// </summary>
        /// <value>
        /// The campus short code.
        /// </value>
        public string CampusShortCode { get; set; }

        /// <summary>
        /// Gets or sets the currency symbol.
        /// </summary>
        /// <value>
        /// The currency symbol.
        /// </value>
        public CurrencyInfoBag CurrencyInfo { get; set; }
    }
}
