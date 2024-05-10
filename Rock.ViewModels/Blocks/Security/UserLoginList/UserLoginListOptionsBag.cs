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

namespace Rock.ViewModels.Blocks.Security.UserLoginList
{
    /// <summary>
    /// The additional configuration options for the User Login List block.
    /// </summary>
    public class UserLoginListOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the block has a person context entity.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this block has a person context entity; otherwise, <c>false</c>.
        /// </value>
        public bool IsContextEntityPerson { get; set; }
    }
}
