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

namespace Rock.ViewModels.Rest.Models
{
    /// <summary>
    /// The response to be sent for a newly created item via the REST v2 API.
    /// </summary>
    public class CreatedAtResponseBag
    {
        /// <summary>
        /// Gets or sets the integer identifier.
        /// </summary>
        /// <value>The integer identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>The unique identifier.</value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the identifier key.
        /// </summary>
        /// <value>The identifier key.</value>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the location URI that can be used to retrieve the
        /// new item from.
        /// </summary>
        /// <value>The location URI of the new item.</value>
        public string Location { get; set; }
    }
}
