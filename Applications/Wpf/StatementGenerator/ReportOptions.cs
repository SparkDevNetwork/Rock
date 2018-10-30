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

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// 
    /// </summary>
    public class ReportOptions
    {
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the cash account ids.
        /// </summary>
        /// <value>
        /// The cash account ids.
        /// </value>
        public List<int> CashAccountIds { get; set; }

        /// <summary>
        /// Gets or sets the non cash account ids.
        /// </summary>
        /// <value>
        /// The non cash account ids.
        /// </value>
        public List<int> NonCashAccountIds { get; set; }

        /// <summary>
        /// Gets or sets the person unique identifier.
        /// NULL means to get all individuals
        /// </summary>
        /// <value>
        /// The person unique identifier.
        /// </value>
        public int? PersonId { get; set; }

        /// <summary>
        /// Gets or sets the Person DataViewId to filter the statements to
        /// </summary>
        /// <value>
        /// The data view identifier.
        /// </value>
        public int? DataViewId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include individuals with no address].
        /// </summary>
        /// <value>
        /// <c>true</c> if [include individuals with no address]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeIndividualsWithNoAddress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [exclude in active individuals].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [exclude in active individuals]; otherwise, <c>false</c>.
        /// </value>
        public bool ExcludeInActiveIndividuals { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [exclude opted out individuals].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [exclude opted out individuals]; otherwise, <c>false</c>.
        /// </value>
        public bool ExcludeOptedOutIndividuals { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether [include businesses].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include businesses]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeBusinesses { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [hide refunded transactions].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [hide refunded transactions]; otherwise, <c>false</c>.
        /// </value>
        public bool HideRefundedTransactions { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether [hide corrected transactions].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [hide corrected transactions]; otherwise, <c>false</c>.
        /// </value>
        public bool HideCorrectedTransactions { get; set; } = true;

        /// <summary>
        /// Gets the layout defined value unique identifier.
        /// </summary>
        /// <value>
        /// The layout defined value unique identifier.
        /// </value>
        public Guid? LayoutDefinedValueGuid { get; internal set; }

        /// <summary>
        /// Gets or sets the current report options
        /// </summary>
        /// <value>
        /// The current report options.
        /// </value>
        public static Rock.StatementGenerator.StatementGeneratorOptions Current
        {
            get
            {
                return _current;
            }
        }

        /// <summary>
        /// The current
        /// </summary>
        private static Rock.StatementGenerator.StatementGeneratorOptions _current = new Rock.StatementGenerator.StatementGeneratorOptions();
    }
}
