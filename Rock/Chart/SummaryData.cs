// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;

using Rock.Data;

namespace Rock.Chart
{
    /// <summary>
    /// 
    /// </summary>
    public class SummaryData : IChartData
    {
        /// <summary>
        /// Gets the date time stamp.
        /// </summary>
        /// <value>
        /// The date time stamp.
        /// </value>
        public long DateTimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the date time.
        /// </summary>
        /// <value>
        /// The date time.
        /// </value>
        [Previewable]
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Gets the y value.
        /// </summary>
        /// <value>
        /// The y value.
        /// </value>
        [Previewable]
        public decimal? YValue { get; set; }

        /// <summary>
        /// Gets the series identifier.
        /// </summary>
        /// <value>
        /// The series identifier.
        /// </value>
        [Previewable]
        public string SeriesId { get; set; }
    }
}
