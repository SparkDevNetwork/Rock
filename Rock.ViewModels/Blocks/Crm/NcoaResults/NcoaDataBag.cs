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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.ViewModels.Blocks.Crm.NcoaResults
{
    /// <summary>
    /// Represents a single NCOA history record for display in the NCOA Results block.
    /// </summary>
    public class NcoaDataBag
    {
        /// <summary>
        /// The encrypted identifier key for this NCOA history record.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// The identifier of the family group associated with this NCOA record.
        /// </summary>
        public int FamilyId { get; set; }

        /// <summary>
        /// The NCOA type of this record (e.g., "Move", "Month48Move").
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The move type for Move records (e.g., "Individual", "Family", "Business").
        /// Only meaningful when <see cref="Type"/> is "Move".
        /// </summary>
        public string MoveType { get; set; }

        /// <summary>
        /// The full name of the primary individual on this NCOA record.
        /// </summary>
        public string IndividualName { get; set; }

        /// <summary>
        /// The encrypted identifier key of the primary individual on this NCOA record.
        /// </summary>
        public string IndividualIdKey { get; set; }

        /// <summary>
        /// A comma-separated list of family member names for family move records.
        /// Empty for individual move records.
        /// </summary>
        public string FamilyMembers { get; set; }

        /// <summary>
        /// A comma-separated list of other family members who remain at the original
        /// address when this is an individual move. Non-empty indicates a split family move.
        /// </summary>
        public string OtherFamilyMembers { get; set; }

        /// <summary>
        /// The formatted original address HTML string.
        /// </summary>
        public string OriginalAddress { get; set; }

        /// <summary>
        /// The formatted new address HTML string. Only populated for Move records.
        /// </summary>
        public string NewAddress { get; set; }

        /// <summary>
        /// The short date string of the move date. Only populated for Move records.
        /// </summary>
        public string MoveDate { get; set; }

        /// <summary>
        /// The distance of the move in miles. Only populated for Move records.
        /// </summary>
        public decimal? MoveDistance { get; set; }

        /// <summary>
        /// The human-readable processing status (e.g., "Processed", "Not Processed").
        /// </summary>
        public string ProcessStatus { get; set; }

        /// <summary>
        /// The address status string (e.g., "Invalid", "Valid").
        /// </summary>
        public string AddressStatus { get; set; }
    }
}
