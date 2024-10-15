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

using Rock.Enums.Mobile;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Controls
{
    /// <summary>
    /// An item bag used by the Mobile Navigation Action Editor to represents it's value.
    /// </summary>
    public class MobileNavigationActionBag
    {
        /// <summary>
        /// The type of navigation to perform.
        /// </summary>
        public MobileNavigationActionType Type { get; set; }

        /// <summary>
        /// The number of pages to pop off the stack (if Pop action type).
        /// </summary>
        public int? PopCount { get; set; }

        /// <summary>
        /// The page to put on the stack where indicated by the action type.
        /// </summary>
        public ListItemBag Page { get; set; }
    }
}
