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

namespace Rock.ViewModels.Blocks.Administration.PageProperties
{
    /// <summary>
    /// Details for the Copy page request
    /// </summary>
    public class CopyPageRequestBag
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include child pages].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include child pages]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeChildPages { get; set; }
    }
}
