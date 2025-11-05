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

using System;

using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Finance.BenevolenceRequestDetail
{
    /// <summary>
    /// Contains details of the location <u>that are relevevant in the context of a benevolence request.</u>
    /// </summary>
    public class LocationBag
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Gets or sets the guid.
        /// </summary>
        /// <value>
        /// The guid.
        /// </value>
        public Guid? Guid { get; set; }

        /// <summary>
        /// Gets or sets the Id of the LocationType Rock.Model.DefinedValue that is used to identify the type of Rock.Model.Location
        /// that this is. Examples: Campus, Building, Room, etc
        /// </summary>
        public ListItemBag LocationTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the Location's Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the address fields.
        /// </summary>
        /// <value>The address fields.</value>
        public AddressControlBag AddressFields { get; set; }
    }
}