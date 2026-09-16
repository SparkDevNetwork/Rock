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

using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.RapidAttendanceEntry
{
    /// <summary>
    /// The editable family information shown in the Edit Family modal: its home address, the address location
    /// flags, and the configured family attributes.
    /// </summary>
    public class RapidAttendanceEntryEditFamilyBag
    {
        /// <summary>
        /// Gets or sets the family's unique identifier.
        /// </summary>
        public Guid FamilyGuid { get; set; }

        /// <summary>
        /// Gets or sets the family's home address. An empty street clears the address, removing it from the family.
        /// </summary>
        public AddressControlBag Address { get; set; }

        /// <summary>
        /// Gets or sets the family's home address rendered as formatted HTML, shown as the Previous Address when the
        /// operator marks the family as moved.
        /// </summary>
        public string AddressFormatted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the home address is the family's mailing location.
        /// </summary>
        public bool IsMailingLocation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the home address is the family's physical (mapped) location.
        /// </summary>
        public bool IsPhysicalLocation { get; set; }

        /// <summary>
        /// Gets or sets the prior address to preserve as a Previous Address, set when the operator marks the
        /// family as having moved. Null when the family has not moved.
        /// </summary>
        public AddressControlBag PreviousAddress { get; set; }

        /// <summary>
        /// Gets or sets the configured family attributes shown for editing.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the family attribute values.
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; set; }
    }
}
