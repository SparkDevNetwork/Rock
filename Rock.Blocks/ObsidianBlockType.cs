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

namespace Rock.Blocks
{
    /// <summary>
    /// Client Block Type
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    /// <seealso cref="IRockClientBlockType" />
    public abstract class ObsidianBlockType : RockBlockType, IObsidianBlockType
    {
        #region Properties

        /// <summary>
        /// Gets the client block identifier.
        /// </summary>
        /// <value>
        /// The client block identifier.
        /// </value>
        public virtual string BlockFileUrl
        {
            get
            {
                var type = GetType();
                var lastNamespace = type.Namespace.Split( '.' ).Last();
                return $"/Obsidian/Blocks/{lastNamespace}/{type.Name}";
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Returns a page parameter.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public string PageParameter( string name )
        {
            return RequestContext?.GetPageParameter( name );
        }

        /// <summary>
        /// Gets the property values that will be sent to the block.
        /// </summary>
        /// <param name="clientType"></param>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public virtual object GetBlockInitialization( RockClientType clientType )
        {
            if ( clientType == RockClientType.Obsidian )
            {
                var initConfig = GetObsidianBlockInitialization() ?? new object();
                return JavaScript.ToJavaScriptObject( initConfig );
            }

            return null;
        }

        /// <summary>
        /// Gets the property values that will be sent to the browser and available to the client side code as it initializes.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public virtual object GetObsidianBlockInitialization()
        {
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
Obsidian.onReady(() => {{
    System.import('/Obsidian/Index.js').then(indexModule => {{
        indexModule.initializeBlock({{
            blockFileUrl: '{BlockFileUrl}',
            rootElement: document.getElementById('{rootElementId}'),
            blockGuid: '{BlockCache.Guid}',
            configurationValues: {GetBlockInitialization( RockClientType.Obsidian )}
        }});
    }});
}});
</script>";
        }

        #endregion Methods
    }
}
