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

namespace Rock.ViewModels.Blocks.Engagement.StepBulkEntry
{
    /// <summary>
    /// Represents a selected person in the Step Bulk Entry block's pill list.
    /// </summary>
    public class StepBulkEntryPersonItemBag
    {
        /// <summary>
        /// Gets or sets the person's display name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the person alias unique identifier.
        /// </summary>
        public Guid PersonAliasGuid { get; set; }

        /// <summary>
        /// Gets or sets the URL to the person's photo or avatar placeholder.
        /// </summary>
        public string PhotoUrl { get; set; }
    }
}
