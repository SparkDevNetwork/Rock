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

namespace Rock.ViewModels.Blocks.CheckIn.Manager.AttendanceDetail
{
    /// <summary>
    /// The response returned by the Delete block action.
    /// </summary>
    public class AttendanceDetailDeleteResponseBag
    {
        /// <summary>
        /// Gets or sets the URL of the Person Profile page to navigate to
        /// once the attendance has been deleted. When empty (e.g. the block
        /// setting was cleared or the person has no Guid), the client stays
        /// on the current page.
        /// </summary>
        public string RedirectUrl { get; set; }
    }
}
