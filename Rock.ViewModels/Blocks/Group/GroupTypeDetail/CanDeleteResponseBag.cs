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

namespace Rock.ViewModels.Blocks.Group.GroupTypeDetail
{
    /// <summary>
    /// Response bag used to indicate whether a specific entity can be deleted,
    /// along with an optional explanation when deletion is not allowed.
    /// </summary>
    public class CanDeleteResponseBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the entity can be deleted.
        /// </summary>
        public bool CanDelete { get; set; }

        /// <summary>
        /// Gets or sets the error message explaining why the entity cannot be deleted.
        /// This value should be <c>null</c> or empty when <see cref="CanDelete"/> is <c>true</c>.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}