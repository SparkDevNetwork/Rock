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

namespace Rock.ViewModels.Blocks.Engagement.StepProgramDetail
{
    /// <summary>
    /// The step status details for the Step Program Detail workflow trigger grid.
    /// </summary>
    public class StepStatusBag
    {
        /// <summary>
        /// Gets or sets the Step Status identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this status is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the status is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this status is complete.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this status is complete; otherwise, <c>false</c>.
        /// </value>
        public bool IsCompleteStatus { get; set; }

        /// <summary>
        /// Gets or sets the color of the status.
        /// </summary>
        /// <value>
        /// The color of the status.
        /// </value>
        public string StatusColor { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid? Guid { get; set; }
    }
}
