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
using System.Linq;

using Rock.Attribute;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Cms;
using Rock.Web.Cache;

namespace Rock.Blocks
{
    /// <summary>
    /// Client Block Type
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    /// <seealso cref="IRockObsidianBlockType" />
    public abstract class RockObsidianBlockType : RockBlockType, IRockObsidianBlockType
    {
        #region Properties

        /// <summary>
        /// The browser not supported markup that will be displayed on the
        /// browser if it is not supported.
        /// </summary>
        /// <remarks>
        /// This might eventually become a virtual property to allow subclasses
        /// to customize the error message.
        /// </remarks>
        private const string BrowserNotSupportedMarkup = @"<div class=""alert alert-warning"">
    It looks like you are using a browser that is not supported, you will need to update before using this feature.
</div>";

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

                // Get all the namespaces after the first one with the name "Blocks".
                // Standard namespacing for blocks is to be one of:
                // Rock.Blocks.x.y.z
                // com.rocksolidchurchdemo.Blocks.x.y.z
                var namespaces = type.Namespace.Split( '.' )
                    .SkipWhile( n => n != "Blocks" )
                    .Skip( 1 )
                    .ToList();

                // Filename convention is camelCase.
                var fileName = $"{type.Name.Substring( 0, 1 ).ToLower()}{type.Name.Substring( 1 )}";

                return $"/Obsidian/Blocks/{namespaces.AsDelimited("/")}/{fileName}";
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
        public override object GetBlockInitialization( RockClientType clientType )
        {
            if ( clientType == RockClientType.Web )
            {
                return GetObsidianBlockInitialization();
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
        public string GetControlMarkup()
        {
            var rootElementId = $"obsidian-{BlockCache.Guid}";

            if ( !IsBrowserSupported() )
            {
                return BrowserNotSupportedMarkup;
            }

            var config = GetConfigBag( rootElementId );

            return
$@"<div id=""{rootElementId}""></div>
<script type=""text/javascript"">
Obsidian.onReady(() => {{
    System.import('@Obsidian/Templates/rockPage.js').then(module => {{
        module.initializeBlock({config.ToCamelCaseJson( false, true )});
    }});
}});
</script>";
        }

        /// <summary>
        /// Gets the configuration bag that will contains all the required
        /// information to initialize a block on an Obsidian page.
        /// </summary>
        /// <param name="rootElementId">The identifier of the root element the block will be rendered in.</param>
        /// <returns>The configuration bag for this block instance.</returns>
        private ObsidianBlockConfigBag GetConfigBag( string rootElementId )
        {
            List<BlockCustomActionBag> configActions = null;

            if ( this is IHasCustomActions customActionsBlock )
            {
                var canEdit = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
                var canAdministrate = BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );

                configActions = customActionsBlock.GetCustomActions( canEdit, canAdministrate );
            }

            var blockPreferences = new ObsidianBlockPreferencesBag
            {
                EntityTypeKey = EntityTypeCache.Get<Rock.Model.Block>().IdKey,
                EntityKey = BlockCache.IdKey,
                Values = GetBlockPersonPreferences().GetAllValueBags().ToList()
            };

            return new ObsidianBlockConfigBag
            {
                BlockFileUrl = BlockFileUrl,
                RootElementId = rootElementId,
                BlockGuid = BlockCache.Guid,
                ConfigurationValues = GetBlockInitialization( RockClientType.Web ),
                CustomConfigurationActions = configActions,
                Preferences = blockPreferences
            };

        }

        /// <summary>
        /// Determines whether the client browser is supported by this block.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the browser is supported; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// In the future this might become a virtual method to allow for
        /// stricter checks by subclasses.
        /// </remarks>
        private bool IsBrowserSupported()
        {
            var browser = RequestContext.ClientInformation.Browser;

            var family = browser.UA.Family;
            var major = browser.UA.Major.AsIntegerOrNull();

            // Logic taken from https://caniuse.com/?search=es6
            // Vue 3 uses ES6 functionality heavily.

            if ( major.HasValue )
            {
                if ( ( family == "Chrome" || family == "Chromium" ) && major.Value < 51 )
                {
                    return false;
                }
                else if ( family == "Edge" && major.Value < 15 )
                {
                    return false;
                }
                else if ( family == "Firefox" && major.Value < 54 )
                {
                    return false;
                }
                else if ( family == "IE" )
                {
                    return false;
                }
                else if ( ( family == "Safari" || family == "Mobile Safari" ) && major.Value < 10 )
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets any non-empty EntityTypeQualifiedColumn values for the entity
        /// specified by the generic type. If this entity type has no attributes
        /// with qualified columns then an empty list will be returned.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity whose attributes will be inspected.</typeparam>
        /// <returns>A list of distinct EntityTypeQualifiedColumn values for <typeparamref name="TEntity"/>.</returns>
        protected List<string> GetAttributeQualifiedColumns<TEntity>()
        {
            var entityTypeId = EntityTypeCache.Get<TEntity>( false )?.Id;

            if ( !entityTypeId.HasValue )
            {
                return new List<string>();
            }

            var attributes = AttributeCache.GetByEntityType( entityTypeId );

            var qualifiedColumns = attributes.Select( a => a.EntityTypeQualifierColumn )
                .Distinct()
                .Where( c => !c.IsNullOrWhiteSpace() )
                .ToList();

            return qualifiedColumns;
        }

        #endregion Methods

        #region Standard Block Actions

        /// <summary>
        /// Gets all the block configuration data that can be used to initialize
        /// the Obsidian block. This is used when a block's settings have been
        /// changed and the block needs to be reloaded on the page.
        /// </summary>
        /// <returns>An action result that contains the block configuration data.</returns>
        [BlockAction]
        [RockInternal( "1.14" )]
        public BlockActionResult RefreshObsidianBlockInitialization()
        {
            var rootElementId = $"obsidian-{BlockCache.Guid}";

            return ActionOk( GetConfigBag( rootElementId ) );
        }

        #endregion
    }
}
