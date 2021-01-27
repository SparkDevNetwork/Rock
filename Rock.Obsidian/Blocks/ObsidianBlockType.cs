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

using System.Linq;
using Rock.Blocks;
using Rock.Obsidian.Util;

namespace Rock.Obsidian.Blocks
{
    /// <summary>
    /// Defines the properties and methods that all Obsidian blocks must implement.
    /// </summary>
    /// <seealso cref="IRockBlockType" />
    public interface IObsidianBlockType : IRockBlockType
    {
        /// <summary>
        /// Gets the block markup file identifier.
        /// </summary>
        /// <value>
        /// The block markup file identifier.
        /// </value>
        string BlockFileUrl { get; }

        /// <summary>
        /// Gets the property values that will be sent to the block.
        /// </summary>
        /// <returns>A collection of string/object pairs.</returns>
        object GetBlockSettings();
    }

    /// <summary>
    /// Obsidian Block Type
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    /// <seealso cref="IObsidianBlockType" />
    public abstract class ObsidianBlockType : RockBlockType, IObsidianBlockType
    {
        #region Properties

        /// <summary>
        /// Gets the block markup file identifier.
        /// </summary>
        /// <value>
        /// The block markup file identifier.
        /// </value>
        public virtual string BlockFileUrl
        {
            get
            {
                var type = GetType();
                var lastNamespace = type.Namespace.Split( '.' ).Last();
                return $"/ObsidianJs/Generated/Blocks/{lastNamespace}/{type.Name}";
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets the property values that will be sent to the browser.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public virtual object GetBlockSettings() {
            return null;
        }

        /// <summary>
        /// Renders the control.
        /// </summary>
        /// <returns></returns>
        public override string GetControlMarkup()
        {
            var rootElementId = $"obsidian-{BlockCache.Guid}";

            return
$@"<div id=""{rootElementId}""></div>
<script type=""text/javascript"">
System.import('/ObsidianJs/Generated/Index.js').then(Obsidian => {{
    Obsidian.initializeBlock({{
        blockFileUrl: '{BlockFileUrl}',
        rootElement: document.getElementById('{rootElementId}'),
        blockGuid: '{BlockCache.Guid}',
        blockSettings: {JavaScript.ToJavaScriptObject( GetBlockSettings() ?? new object() )}
    }});
}});
</script>";
        }

        #endregion Methods
    }
}
