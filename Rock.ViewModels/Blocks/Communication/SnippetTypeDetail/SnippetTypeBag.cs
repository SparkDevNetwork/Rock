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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.SnippetTypeDetail
{
    /// <summary>
    /// Class SnippetTypeBag.
    /// Implements the <see cref="Rock.ViewModels.Utility.EntityBagBase" />
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class SnippetTypeBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        public string HelpText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is personal allowed.
        /// </summary>
        public bool IsPersonalAllowed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is shared allowed.
        /// </summary>
        public bool IsSharedAllowed { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user has the administrate permission.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the current user can administrate; otherwise, <c>false</c>.
        /// </value>
        public bool CanAdministrate { get; set; }
    }
}
