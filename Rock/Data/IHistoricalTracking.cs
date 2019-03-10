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

namespace Rock.Data
{
    /// <summary>
    /// Interface for Models that represent historical snapshots of a record
    /// </summary>
    public interface IHistoricalTracking : IModel
    {
        /// <summary>
        /// Gets or sets the effective date.
        /// This is the starting date that the tracked record had the values reflected in this record
        /// </summary>
        /// <value>
        /// The effective date.
        /// </value>
        DateTime EffectiveDateTime { get; set; }

        /// <summary>
        /// Gets or sets the expire date time
        /// This is the last date that the tracked record had the values reflected in this record
        /// For example, if a tracked record's Name property changed on '2016-07-14', the ExpireDate of the previously current record will be '2016-07-13', and the EffectiveDate of the current record will be '2016-07-14'
        /// If this is most current record, the ExpireDate will be '9999-01-01'
        /// </summary>
        /// <value>
        /// The expire date.
        /// </value>
        DateTime ExpireDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [current row indicator].
        /// This will be True if this represents the same values as the current tracked record for this
        /// </summary>
        /// <value>
        ///   <c>true</c> if [current row indicator]; otherwise, <c>false</c>.
        /// </value>
        bool CurrentRowIndicator { get; set; }
    }

    /// <summary>
    /// Constants related to IHistoralTracking
    /// </summary>
    public static class HistoricalTracking
    {
        /// <summary>
        /// The ExpireDateTime ('9999-01-01') that should be used when CurrentRowIndicator = true
        /// </summary>
        public readonly static DateTime MaxExpireDateTime = new DateTime( 9999, 1, 1 );
    }
}
