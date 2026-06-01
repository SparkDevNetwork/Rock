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

namespace Rock.ViewModels.Blocks.Crm.BulkUpdate
{
    /// <summary>
    /// Represents a person targeted in a bulk update operation.
    /// </summary>
    public class BulkUpdatePersonBag
    {
        /// <summary>
        /// Gets or sets the person's primary alias unique identifier. This is
        /// the identifier emitted by the PersonPicker control. The server
        /// resolves this to the underlying integer PersonId before processing.
        /// </summary>
        public Guid PersonAliasGuid { get; set; }

        /// <summary>
        /// Gets or sets the full name of the person.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the photo URL of the person.
        /// </summary>
        public string PhotoUrl { get; set; }
    }
}
