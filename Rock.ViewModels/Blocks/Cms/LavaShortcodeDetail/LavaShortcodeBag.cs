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

using System.Collections.Generic;

using Rock.ViewModels.Utility;
using Rock.Model;

namespace Rock.ViewModels.Blocks.Cms.LavaShortcodeDetail
{
    /// <summary>
    /// The Lava Short Code Bag
    /// </summary>
    public class LavaShortcodeBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the collection of Categories that this Rock.Model.LavaShortcode is associated with.
        /// NOTE: Since changes to Categories isn't tracked by ChangeTracker, set the ModifiedDateTime if Categories are modified.
        /// </summary>
        public List<ListItemBag> Categories { get; set; }

        /// <summary>
        /// Gets or sets the Description of the Lava Shortcode.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the documentation. This serves as the technical description of the internals of the shortcode.
        /// </summary>
        public string Documentation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        public string Markup { get; set; }

        /// <summary>
        /// Gets or sets the public name of the shortcode.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the tag.
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        /// Gets or sets the type of the tag (inline or block). A tag type of block requires an end tag.
        /// </summary>
        public string TagType { get; set; }

        /// <summary>
        /// Gets or sets the enabled commands.
        /// </summary>
        /// <value>
        /// The enabled commands.
        /// </value>
        public List<ListItemBag> EnabledCommands { get; set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public List<ListItemBag> Parameters { get; set; }
    }
}
