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

namespace Rock.ViewModels.Blocks.Engagement.StepTypeList
{
    /// <summary>
    /// The additional configuration options for the Step Type List block.
    /// </summary>
    public class StepTypeListOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the current user has edit permission.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the current user has edit permission; otherwise, <c>false</c>.
        /// </value>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the security column should be visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the security column is visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsSecurityColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the block should be visible, the block is displayed when a valid Step Program is found.
        /// </summary>
        /// <value>
        ///   <c>true</c> if a valid Step Program is available; otherwise, <c>false</c>.
        /// </value>
        public bool IsBlockVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Current User is authorized to view the Step Program.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this Current User is authorized to view the Step Program; otherwise, <c>false</c>.
        /// </value>
        public bool IsAuthorizedToViewProgram { get; set; }

        /// <summary>
        /// Gets or sets the step program identifier key.
        /// </summary>
        /// <value>
        /// The step program identifier key.
        /// </value>
        public string StepProgramIdKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the bulk entry column should be visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the bulk entry column is visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsBulkEntryColumnVisible { get; set; }
    }
}
