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

namespace Rock.StatementGenerator
{
    /// <summary>
    /// 
    /// </summary>
    public class StatementGeneratorOptions : DotLiquid.Drop
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
        /// Gets or sets the transaction account ids.
        /// </summary>
        /// <value>
        /// The transaction account ids.
        /// </value>
        public List<int> TransactionAccountIds { get; set; }

        /// <summary>
        /// Gets or sets the currency type ids cash.
        /// </summary>
        /// <value>
        /// The currency type ids cash.
        /// </value>
        public List<int> CurrencyTypeIdsCash { get; set; }

        /// <summary>
        /// Gets or sets the currency type ids non cash.
        /// </summary>
        /// <value>
        /// The currency type ids non cash.
        /// </value>
        public List<int> CurrencyTypeIdsNonCash { get; set; }

        /// <summary>
        /// Gets a value indicating whether [pledges include child accounts].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [pledges include child accounts]; otherwise, <c>false</c>.
        /// </value>
        public bool PledgesIncludeChildAccounts { get; set; }

        /// <summary>
        /// Gets a value indicating whether [pledges include non cash gifts].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [pledges include non cash gifts]; otherwise, <c>false</c>.
        /// </value>
        public bool PledgesIncludeNonCashGifts { get; set; }

        /// <summary>
        /// Gets or sets the pledges account ids.
        /// </summary>
        /// <value>
        /// The pledges account ids.
        /// </value>
        public List<int> PledgesAccountIds { get; set; }

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
        public Guid? LayoutDefinedValueGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [order by postal code].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [order by postal code]; otherwise, <c>false</c>.
        /// </value>
        public bool OrderByPostalCode { get; set; } = true;

        /// <summary>
        /// Gets or sets the name of the base file.
        /// </summary>
        /// <value>
        /// The name of the base file.
        /// </value>
        public string BaseFileName { get; set; }

        /// <summary>
        /// Gets or sets the save directory.
        /// </summary>
        /// <value>
        /// The save directory.
        /// </value>
        public string SaveDirectory { get; set; }

        /// <summary>
        /// The maximum number of statements in a single pdf file
        /// </summary>
        /// <value>
        /// The size of the chapter.
        /// </value>
        public int? StatementsPerChapter { get; set; }
        
    }
}
