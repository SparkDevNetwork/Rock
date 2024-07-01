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
namespace Rock.ViewModels.CheckIn
{
    /// <summary>
    /// Defines a single generic check-in item.
    /// </summary>
    public class CheckInItemBag
    {
        /// <summary>
        /// Gets or sets the identifier of this item.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the display name of this item.
        /// </summary>
        /// <value>The display name.</value>
        public string Name { get; set; }
    }
}
