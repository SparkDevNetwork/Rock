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

namespace Rock.ViewModels.Blocks.Group.GroupScheduleToolbox
{
    /// <summary>
    /// Gets the specific view model for the confirmation modal on mobile.
    /// </summary>
    public class ConfirmDeclineBag
    {
        /// <summary>
        /// Gets or sets the XAML content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets a boolean that states whether or not a decline reason is required.
        /// </summary>
        public bool DeclineReasonRequired { get; set; }
    }
}
