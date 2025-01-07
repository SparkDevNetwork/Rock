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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Cms;
using Rock.Web.Cache;

namespace Rock.Blocks
{
    /// <summary>
    /// The most basic block type that all other blocks should inherit from. Provides
    /// default implementations of the <seealso cref="IRockBlockType"/> interface as
    /// well as a number of helper methods and properties to subclasses.
    /// </summary>
    /// <seealso cref="Rock.Blocks.IRockBlockType" />
    public abstract class RockBlockType : IRockBlockType, IRockObsidianBlockType, IRockMobileBlockType
    {
        #region Constants

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

        #endregion

        #region Fields

        /// <summary>
        /// The serializer settings to use when encoding block configurationd ata.
        /// </summary>
        private static readonly Lazy<Newtonsoft.Json.JsonSerializerSettings> _serializerSettings = new Lazy<Newtonsoft.Json.JsonSerializerSettings>( () =>
        {
            var settings = Rock.JsonExtensions.CreateSerializerSettings( false, true, true );

            settings.StringEscapeHandling = Newtonsoft.Json.StringEscapeHandling.EscapeHtml;

            return settings;
        } );

        #endregion

        #region Properties

        /// <summary>
        /// Gets the block identifier.
        /// </summary>
        /// <value>
        /// The block identifier.
        /// </value>
        public int BlockId => BlockCache.Id;

        /// <summary>
        /// Gets or sets the block cache.
        /// </summary>
        /// <value>
        /// The block cache.
        /// </value>
        public BlockCache BlockCache { get; set ; }

        /// <summary>
        /// Gets or sets the page cache.
        /// </summary>
        /// <value>
        /// The page cache.
        /// </value>
        public PageCache PageCache { get; set; }

        /// <summary>
        /// Gets or sets the request context.
        /// </summary>
        /// <value>
        /// The request context.
        /// </value>
        public RockRequestContext RequestContext { get; set; }

        /// <summary>
        /// Gets the response context for this request. This can be used to
        /// add or update information that will be sent to the client.
        /// </summary>
        /// <value>
        /// The response context.
        /// </value>
        public IRockResponseContext ResponseContext => RequestContext.Response;

        /// <summary>
        /// Gets the database context to use for this block instance. It can
        /// be used for both reading and writing data, though it should be up
        /// to the subclass implementation to decide when to call SaveChanges().
        /// </summary>
        /// <value>
        /// The database context to use for this block instance.
        /// </value>
        public RockContext RockContext { get; internal set; }

        /// <inheritdoc/>
        [Obsolete( "Use RequiredMobileVersion instead." )]
        [RockObsolete( "1.16" )]
        public virtual int RequiredMobileAbiVersion => RequiredMobileVersion?.Minor ?? 1;

        /// <inheritdoc/>
        public virtual Version RequiredMobileVersion { get; } = new Version( 0, 0, 0, 0 );

        /// <inheritdoc/>
        [Obsolete( "Use MobileBlockTypeGuid instead." )]
        [RockObsolete( "1.16" )]
        public virtual string MobileBlockType => null;

        /// <inheritdoc/>
        public virtual Guid? MobileBlockTypeGuid => null;

        /// <inheritdoc/>
        public virtual string ObsidianFileUrl => GetObsidianFileUrl();

        #endregion

        #region Methods

        /// <summary>
        /// Gets the object that will be used to initialize the block by the client.
        /// </summary>
        /// <param name="clientType">The type of client that is requesting the configuration data.</param>
        /// <returns>An object that will be JSON encoded and sent to the client.</returns>
        [Obsolete( "Use GetBlockInitializationAsync instead." )]
        [RockObsolete( "1.16.4" )]
        public virtual object GetBlockInitialization( RockClientType clientType )
        {
            return null;
        }

        /// <inheritdoc/>
        public virtual Task<object> GetBlockInitializationAsync( RockClientType clientType )
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var legacyInitialization = GetBlockInitialization( clientType );
#pragma warning restore CS0618 // Type or member is obsolete

            if ( legacyInitialization != null )
            {
                return Task.FromResult( legacyInitialization );
            }

            if ( clientType == RockClientType.Web )
            {
                return GetObsidianBlockInitializationAsync();
            }
            else if ( clientType == RockClientType.Mobile )
            {
                return GetMobileConfigurationValuesAsync();
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
        /// Gets the property values that will be sent to the browser and available to the client side code as it initializes.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public virtual Task<object> GetObsidianBlockInitializationAsync()
        {
            return Task.FromResult( GetObsidianBlockInitialization() );
        }

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public virtual object GetMobileConfigurationValues()
        {
            return null;
        }

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public virtual Task<object> GetMobileConfigurationValuesAsync()
        {
            return Task.FromResult( GetMobileConfigurationValues() );
        }

        /// <summary>
        /// Renews the security grant token that should be used by controls with this block.
        /// </summary>
        /// <returns>A string that contains the security grant token.</returns>
        protected virtual string RenewSecurityGrantToken()
        {
            return string.Empty;
        }

        /// <summary>
        /// Gets the attribute value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetAttributeValue( string key )
        {
            return BlockCache.GetAttributeValue( key );
        }

        /// <summary>
        /// Gets the block attribute values given an attribute key - splitting that delimited value into a list of strings.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>A list of attribute value strings or an empty list if no attribute values exist.</returns>
        internal List<string> GetAttributeValues( string key )
        {
            return BlockCache.GetAttributeValues( key );
        }

        /// <summary>
        /// Gets the attribute value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public object GetAttributeValueAsFieldType( string key )
        {
            var stringValue = GetAttributeValue( key );
            var attribute = BlockCache.Attributes.GetValueOrNull( key );
            var field = attribute?.FieldType?.Field;

            if ( field == null )
            {
                return stringValue;
            }

            return field.ValueAsFieldType( stringValue, attribute.QualifierValues );
        }

        /// <summary>
        /// Gets the attribute value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public T GetAttributeValueAsFieldType<T>( string key ) where T : class
        {
            var asObject = GetAttributeValueAsFieldType( key );
            return asObject as T;
        }

        /// <summary>
        /// Gets the current person.
        /// </summary>
        /// <returns></returns>
        protected Person GetCurrentPerson()
        {
            return RequestContext.CurrentPerson;
        }

        /// <summary>
        /// Gets the entity object for this block based on the configuration of the
        /// <see cref="Rock.Web.UI.ContextAwareAttribute"/> attribute.
        /// </summary>
        /// <returns>A reference to the <see cref="IEntity"/> or <c>null</c> if none was found.</returns>
        public IEntity GetContextEntity()
        {
            var type = GetContextEntityType();

            if ( type == null )
            {
                return null;
            }

            return RequestContext.GetContextEntity( type );
        }

        /// <summary>
        /// Gets the entity type for this block based on the configuration of the
        /// <see cref="Rock.Web.UI.ContextAwareAttribute"/> attribute.
        /// </summary>
        /// <returns>A <see cref="Type"/> that identifies the expected context entity type or <c>null</c> if not configured.</returns>
        public Type GetContextEntityType()
        {
            var contextAttribute = this.GetType().GetCustomAttribute<Rock.Web.UI.ContextAwareAttribute>();

            if ( contextAttribute == null )
            {
                return null;
            }

            if ( contextAttribute.IsConfigurable )
            {
                var contextEntityTypeGuid = GetAttributeValue( "ContextEntityType" ).AsGuidOrNull();

                if ( !contextEntityTypeGuid.HasValue )
                {
                    return null;
                }

                return EntityTypeCache.Get( contextEntityTypeGuid.Value )
                    ?.GetEntityType();
            }
            else
            {
                return contextAttribute.Contexts
                    .FirstOrDefault()
                    ?.EntityType
                    ?.GetEntityType();
            }
        }

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
        /// Gets the JavaScript file URL to use for this block in Obsidian mode.
        /// </summary>
        /// <returns>A string that represents the path.</returns>
        private string GetObsidianFileUrl()
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

            return $"~/Obsidian/Blocks/{namespaces.AsDelimited( "/" )}/{fileName}.obs";
        }

        /// <summary>
        /// Renders the HTML markup needed to fully initialize this block. This
        /// method can be overridden to provide content for a fully static
        /// block. Fully static blocks do not reload automatically when the
        /// block settings have been modified.
        /// </summary>
        /// <returns>An HTML string.</returns>
        [Obsolete( "Use GetControlMarkupAsync instead." )]
        [RockObsolete( "1.16.4" )]
        public virtual string GetControlMarkup()
        {
            return null;
        }

        /// <inheritdoc/>
        public virtual async Task<string> GetControlMarkupAsync()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var legacyMarkup = GetControlMarkup();
#pragma warning restore CS0618 // Type or member is obsolete

            if ( legacyMarkup != null )
            {
                return legacyMarkup;
            }

            var rootElementId = $"obsidian-{BlockCache.Guid}";
            var rootElementStyle = "";

            if ( !IsBrowserSupported() )
            {
                return BrowserNotSupportedMarkup;
            }

            int? initialHeight = 400;
            var initialHeightAttribute = GetType().GetCustomAttribute<InitialBlockHeightAttribute>();

            if ( initialHeightAttribute != null )
            {
                initialHeight = initialHeightAttribute.Height;
            }

            if ( initialHeight.HasValue )
            {
                rootElementStyle += $" --initial-block-height: {initialHeight.Value}px";
            }

            var config = await GetConfigBagAsync( rootElementId );
            var initialContent = GetInitialHtmlContent() ?? string.Empty;

            // If any text value contains "</script>" then it will be interpreted
            // by the browser as the end of the main script tag, even if it is
            // inside a JavaScript string. Use custom JSON serializer settings
            // that have an option enabled to escape HTML characters in strings.
            var configJson = Newtonsoft.Json.JsonConvert.SerializeObject( config, _serializerSettings.Value );

            return
$@"<div id=""{rootElementId}"" class=""obsidian-block-loading"" style=""{rootElementStyle.Trim()}"">{initialContent}</div>
<script type=""text/javascript"">
Obsidian.onReady(() => {{
    System.import('@Obsidian/Templates/rockPage.js').then(module => {{
        module.initializeBlock({configJson});
    }});
}});
</script>";
        }

        /// <summary>
        /// Gets the initial HTML content to use when rendering an Obsidian
        /// block. This can be overridden to create a psuedo-static block. This
        /// content will be included in the HTML page for SEO indexing as well
        /// as initial page rendering. The Obsidian code can then choose to
        /// continue using this content or replace it once it loads.
        /// </summary>
        /// <returns>A string of HTML content.</returns>
        protected virtual string GetInitialHtmlContent()
        {
            return string.Empty;
        }

        /// <summary>
        /// Gets the configuration bag that will contains all the required
        /// information to initialize a block on an Obsidian page.
        /// </summary>
        /// <param name="rootElementId">The identifier of the root element the block will be rendered in.</param>
        /// <returns>The configuration bag for this block instance.</returns>
        private async Task<ObsidianBlockConfigBag> GetConfigBagAsync( string rootElementId )
        {
            List<BlockCustomActionBag> configActions = null;
            var reloadModeAttribute = GetType().GetCustomAttribute<ConfigurationChangedReloadAttribute>();

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
                Values = GetBlockPersonPreferences().GetAllValueBags().ToList(),
                TimeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
            };

            return new ObsidianBlockConfigBag
            {
                BlockFileUrl = RequestContext.ResolveRockUrl( ObsidianFileUrl ),
                RootElementId = rootElementId,
                BlockGuid = BlockCache.Guid,
                BlockTypeGuid = BlockCache.BlockType.Guid,
                ConfigurationValues = await GetBlockInitializationAsync( RockClientType.Web ),
                CustomConfigurationActions = configActions,
                Preferences = blockPreferences,
                ReloadMode = reloadModeAttribute?.ReloadMode ?? Enums.Cms.BlockReloadMode.None
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

            // If no user agent, assume the browser is supported since it is
            // more likely to be supported than not supported.
            if ( browser == null )
            {
                return true;
            }

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

        #endregion

        #region Person Preferences

        /// <summary>
        /// Gets the global person preferences. These are unique to the person
        /// but global across the entire system. Global preferences should be
        /// used with extreme caution and care.
        /// </summary>
        /// <returns>An instance of <see cref="PersonPreferenceCollection"/> that provides access to the preferences. This will never return <c>null</c>.</returns>
        public PersonPreferenceCollection GetGlobalPersonPreferences()
        {
            return RequestContext.GetGlobalPersonPreferences();
        }

        /// <summary>
        /// Gets the person preferences scoped to the specified entity.
        /// </summary>
        /// <param name="scopedEntity">The entity to use when scoping the preferences for a particular use.</param>
        /// <returns>An instance of <see cref="PersonPreferenceCollection"/> that provides access to the preferences. This will never return <c>null</c>.</returns>
        public PersonPreferenceCollection GetScopedPersonPreferences( IEntity scopedEntity )
        {
            return RequestContext.GetScopedPersonPreferences( scopedEntity );
        }

        /// <summary>
        /// Gets the person preferences scoped to the specified entity.
        /// </summary>
        /// <param name="scopedEntity">The entity to use when scoping the preferences for a particular use.</param>
        /// <returns>An instance of <see cref="PersonPreferenceCollection"/> that provides access to the preferences. This will never return <c>null</c>.</returns>
        public PersonPreferenceCollection GetScopedPersonPreferences( IEntityCache scopedEntity )
        {
            return RequestContext.GetScopedPersonPreferences( scopedEntity );
        }

        /// <summary>
        /// Gets the person preferences scoped to the current block.
        /// </summary>
        /// <returns>An instance of <see cref="PersonPreferenceCollection"/> that provides access to the preferences. This will never return <c>null</c>.</returns>
        public PersonPreferenceCollection GetBlockPersonPreferences()
        {
            return RequestContext.GetScopedPersonPreferences( BlockCache );
        }

        /// <summary>
        /// Gets the person preferences scoped to the current block type.
        /// </summary>
        /// <returns>An instance of <see cref="PersonPreferenceCollection"/> that provides access to the preferences. This will never return <c>null</c>.</returns>
        public PersonPreferenceCollection GetBlockTypePersonPreferences()
        {
            return RequestContext.GetScopedPersonPreferences( BlockCache.BlockType );
        }

        #endregion

        #region Action Response Methods

        /// <summary>
        /// Creates a 200-OK response with no content.
        /// </summary>
        /// <returns>A BlockActionResult instance.</returns>
        protected virtual BlockActionResult ActionOk()
        {
            return new BlockActionResult( System.Net.HttpStatusCode.OK );
        }

        /// <summary>
        /// Create a 200-OK response with the given content value.
        /// </summary>
        /// <typeparam name="T">The type of the content being returned.</typeparam>
        /// <param name="value">The value to be sent to the client.</param>
        /// <returns>A BlockActionResult instance.</returns>
        protected virtual BlockActionResult ActionOk<T>( T value )
        {
            return new BlockActionResult( System.Net.HttpStatusCode.OK, value, typeof( T ) );
        }

        /// <summary>
        /// Create a response with the given status code.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <returns>A BlockActionResult instance.</returns>
        protected virtual BlockActionResult ActionStatusCode( System.Net.HttpStatusCode statusCode )
        {
            return new BlockActionResult( statusCode );
        }

        /// <summary>
        /// Creates a generic response of the specified status code for the content value.
        /// </summary>
        /// <typeparam name="T">The type of the content being returned.</typeparam>
        /// <param name="statusCode">The status code.</param>
        /// <param name="value">The value to be sent to the client.</param>
        /// <returns>A BlockActionResult instance.</returns>
        protected virtual BlockActionResult ActionContent<T>( System.Net.HttpStatusCode statusCode, T value )
        {
            return new BlockActionResult( statusCode, value, typeof( T ) );
        }

        /// <summary>
        /// Creates a 400-Bad Request response with an optional error message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>A BlockActionResult instance.</returns>
        protected BlockActionResult ActionBadRequest( string message = null )
        {
            if ( message == null )
            {
                return new BlockActionResult( System.Net.HttpStatusCode.BadRequest );
            }
            else
            {
                return new BlockActionResult( System.Net.HttpStatusCode.BadRequest )
                {
                    Error = message
                };
            }
        }

        /// <summary>
        /// Creates a 409-Conflict response with an optional error message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>A BlockActionResult instance.</returns>
        protected BlockActionResult ActionConflict( string message = null )
        {
            if ( message == null )
            {
                return new BlockActionResult( System.Net.HttpStatusCode.Conflict );
            }
            else
            {
                return new BlockActionResult( System.Net.HttpStatusCode.Conflict )
                {
                    Error = message
                };
            }
        }

        /// <summary>
        /// Creates a 401-Unauthorized response with an optional error message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>A BlockActionResult instance.</returns>
        protected BlockActionResult ActionUnauthorized( string message = null )
        {
            if ( message == null )
            {
                return new BlockActionResult( System.Net.HttpStatusCode.Unauthorized );
            }
            else
            {
                return new BlockActionResult( System.Net.HttpStatusCode.Unauthorized )
                {
                    Error = message
                };
            }
        }

        /// <summary>
        /// Creates a 403-Forbidden response with an optional error message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>A BlockActionResult instance.</returns>
        protected BlockActionResult ActionForbidden( string message = null )
        {
            if ( message == null )
            {
                return new BlockActionResult( System.Net.HttpStatusCode.Forbidden );
            }
            else
            {
                return new BlockActionResult( System.Net.HttpStatusCode.Forbidden )
                {
                    Error = message
                };
            }
        }

        /// <summary>
        /// Creates a 404-Not Found response with an optional error message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>A BlockActionResult instance.</returns>
        protected BlockActionResult ActionNotFound( string message = null )
        {
            if ( message == null )
            {
                return new BlockActionResult( System.Net.HttpStatusCode.NotFound );
            }
            else
            {
                return new BlockActionResult( System.Net.HttpStatusCode.NotFound )
                {
                    Error = message
                };
            }
        }

        /// <summary>
        /// Creats a 500-Internal Server Error response with an optional error message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>A BlockActionResult instance.</returns>
        protected virtual BlockActionResult ActionInternalServerError( string message = null)
        {
            return new BlockActionResult( System.Net.HttpStatusCode.InternalServerError )
            {
                Error = message
            };
        }

        #endregion

        #region Standard Block Actions

        /// <summary>
        /// Requests the renewal of the security grant token.
        /// </summary>
        /// <returns>A response that contains the new security grant token or an empty string.</returns>
        [BlockAction( "RenewSecurityGrantToken" )]
        [RockInternal( "1.14", true )]
        public BlockActionResult RenewSecurityGrantTokenAction()
        {
            return ActionOk( RenewSecurityGrantToken() );
        }

        /// <summary>
        /// Gets all the block configuration data that can be used to initialize
        /// the Obsidian block. This is used when a block's settings have been
        /// changed and the block needs to be reloaded on the page.
        /// </summary>
        /// <returns>An action result that contains the block configuration data.</returns>
        [BlockAction]
        [RockInternal( "1.14", true )]
        public async Task<BlockActionResult> RefreshObsidianBlockInitialization()
        {
            var rootElementId = $"obsidian-{BlockCache.Guid}";

            var config = await GetConfigBagAsync( rootElementId );

            config.InitialContent = GetInitialHtmlContent();

            return ActionOk( config );
        }

        #endregion
    }
}
