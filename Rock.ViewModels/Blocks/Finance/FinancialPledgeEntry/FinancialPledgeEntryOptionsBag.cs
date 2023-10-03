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

using Rock.ViewModels.Utility;
using System;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Finance.FinancialPledgeEntry
{
    /// <summary>
    /// 
    /// </summary>
    public class FinancialPledgeEntryOptionsBag
    {
        /// <summary>
        /// Gets or sets the select group type unique identifier.
        /// </summary>
        /// <value>
        /// The select group type unique identifier.
        /// </value>
        public Guid? SelectGroupTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the note message.
        /// </summary>
        /// <value>
        /// The note message.
        /// </value>
        public string NoteMessage { get; set; }

        /// <summary>
        /// Gets or sets the save button text.
        /// </summary>
        /// <value>
        /// The save button text.
        /// </value>
        public string SaveButtonText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the pledge frequency option to the user..
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show pledge frequency]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowPledgeFrequency { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether require that a user select a specific pledge frequency (when pledge frequency is shown)
        /// </summary>
        /// <value>
        ///   <c>true</c> if [require pledge frequency]; otherwise, <c>false</c>.
        /// </value>
        public bool RequirePledgeFrequency { get; set; }

        /// <summary>
        /// Gets or sets the pledge frequencies.
        /// </summary>
        /// <value>
        /// The pledge frequencies.
        /// </value>
        public List<ListItemBag> PledgeFrequencies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show date range].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show date range]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowDateRange { get; set; }

        /// <summary>
        /// Gets or sets the groups.
        /// </summary>
        /// <value>
        /// The groups.
        /// </value>
        public List<ListItemBag> Groups { get; set; }

        /// <summary>
        /// Gets or sets the groups label.
        /// </summary>
        /// <value>
        /// The groups label.
        /// </value>
        public string GroupsLabel { get; set; }

        /// <summary>
        /// Gets or sets the pledge term.
        /// </summary>
        /// <value>
        /// The pledge term.
        /// </value>
        public string PledgeTerm { get; set; }
    }
}
