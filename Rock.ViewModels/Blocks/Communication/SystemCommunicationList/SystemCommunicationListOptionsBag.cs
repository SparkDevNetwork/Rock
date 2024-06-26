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

namespace Rock.ViewModels.Blocks.Communication.SystemCommunicationList
{
    /// <summary>
    /// The additional configuration options for the System Communication List block.
    /// </summary>
    public class SystemCommunicationListOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the current user has the Administrate permission.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the user can administrate; otherwise, <c>false</c>.
        /// </value>
        public bool CanAdministrate { get; set; }
    }
}
