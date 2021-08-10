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
namespace Rock.Lava.Shortcodes
{
    /// <summary>
    /// A custom attribute for defining the shortcode documentation that will show
    /// up in the shortcode admin blocks.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    public class LavaShortcodeMetadataAttribute : System.Attribute
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the tag.
        /// </summary>
        /// <value>
        /// The name of the tag.
        /// </value>
        public string TagName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the shortcode.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the documentation for the shortcode.
        /// </summary>
        /// <value>
        /// The documentation.
        /// </value>
        public string Documentation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the parameters available to the shortcode.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public string Parameters { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the enabled commands inside of the Lava.
        /// </summary>
        /// <value>
        /// The enabled commands.
        /// </value>
        public string EnabledCommands { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="LavaShortcodeMetadataAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="description">The description.</param>
        /// <param name="documentation">The documentation.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="enabledCommands">The enabled commands.</param>
        public LavaShortcodeMetadataAttribute( string name, string tagName, string description, string documentation, string parameters, string enabledCommands )
        {
            this.Name = name;
            this.TagName = tagName;
            this.Description = description;
            this.Documentation = documentation;
            this.Parameters = parameters;
            this.EnabledCommands = enabledCommands;
        }
    }
}
