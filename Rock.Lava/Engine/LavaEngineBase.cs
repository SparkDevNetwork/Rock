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
using System.Reflection;
using System.Threading;

namespace Rock.Lava
{
    /// <summary>
    /// Provides base functionality for an engine that can parse and render Lava Templates.
    /// </summary>
    public abstract class LavaEngineBase : ILavaEngine
    {
        private List<string> _defaultEnabledCommands = new List<string>();

        void ILavaService.OnInitialize( object settings )
        {
            Initialize( settings as LavaEngineConfigurationOptions );
        }

        /// <summary>
        /// Initializes the Lava engine with the specified options.
        /// </summary>
        public void Initialize( LavaEngineConfigurationOptions options )
        {
            if ( options == null )
            {
                options = new LavaEngineConfigurationOptions();
            }

            _hostService = options.HostService;

            // Initialize the cache service for the current Lava Engine type.
            _cacheService = options.CacheService;

            if ( _cacheService != null )
            {
                _cacheService.Initialize( this.GetType().Name );
            }

            _defaultEnabledCommands = options.DefaultEnabledCommands;

            if ( options.ExceptionHandlingStrategy != null )
            {
                this.ExceptionHandlingStrategy = options.ExceptionHandlingStrategy.Value;
            }

            OnSetConfiguration( options );
        }

        /// <summary>
        /// The set of Lava commands that are enabled by default when a new context is created.
        /// </summary>
        public List<string> DefaultEnabledCommands
        {
            get
            {
                return _defaultEnabledCommands;
            }
            set
            {
                _defaultEnabledCommands = value ?? new List<string>();
            }
        }

        /// <summary>
        /// Override this method to set configuration options for the specific Liquid framework engine implementation.
        /// </summary>
        /// <param name="options"></param>
        public abstract void OnSetConfiguration( LavaEngineConfigurationOptions options );

        /// <summary>
        /// Gets the descriptive name of the current Liquid engine that is providing template parsing and rendering functionality for the Lava library.
        /// </summary>
        public abstract string EngineName { get; }

        #region Events

        /// <summary>
        /// An event that is triggered when the LavaEngine encounters a processing exception.
        /// </summary>
        public event EventHandler<LavaEngineExceptionEventArgs> ExceptionEncountered;

        #endregion

        /// <summary>
        /// Create a new template context.
        /// </summary>
        /// <returns></returns>
        public ILavaRenderContext NewRenderContext()
        {
            return NewRenderContextInternal( null, this.DefaultEnabledCommands );
        }

        /// <summary>
        /// Create a new template context and add the specified merge fields.
        /// </summary>
        /// <param name="enabledCommands"></param>
        /// <returns></returns>
        public ILavaRenderContext NewRenderContext( IEnumerable<string> enabledCommands )
        {
            return NewRenderContextInternal( null, enabledCommands );
        }

        /// <summary>
        /// Create a new template context and add the specified merge fields.
        /// </summary>
        /// <param name="mergeFields"></param>
        /// <returns></returns>
        public ILavaRenderContext NewRenderContext( ILavaDataDictionary mergeFields, IEnumerable<string> enabledCommands = null )
        {
            return NewRenderContextInternal( mergeFields, enabledCommands );
        }

        /// <summary>
        /// Create a new template context and add the specified merge fields.
        /// </summary>
        /// <param name="mergeFields"></param>
        /// <returns></returns>
        public ILavaRenderContext NewRenderContext( IDictionary<string, object> mergeFields, IEnumerable<string> enabledCommands = null )
        {
            return NewRenderContextInternal( mergeFields, enabledCommands );
        }

        /// <summary>
        /// Create a new template context and add the specified merge fields.
        /// </summary>
        /// <param name="mergeFields"></param>
        /// <returns></returns>
        private ILavaRenderContext NewRenderContextInternal( object mergeFields, IEnumerable<string> enabledCommands )
        {
            var context = OnCreateRenderContext();

            if ( context == null )
            {
                throw new LavaException( "Failed to create a new render context." );
            }

            if ( mergeFields is IDictionary<string, object> dictionary )
            {
                context.SetMergeFields( dictionary );
            }
            else if ( mergeFields is ILavaDataDictionary ldd )
            {
                context.SetMergeFields( ldd );
            }

            InitializeRenderContext( context, enabledCommands ?? this.DefaultEnabledCommands );

            return context;
        }

        /// <summary>
        /// Create a new template context and add the specified merge fields.
        /// </summary>
        /// <param name="mergeFields"></param>
        /// <returns></returns>
        public ILavaRenderContext NewRenderContext( LavaDataDictionary mergeFields, IEnumerable<string> enabledCommands = null )
        {
            // This method exists as a convenience to disambiguate method calls using the LavaDataDictionary parameter, because
            //  it supports both the ILavaDataDictionary and IDictionary<string, object> interfaces.
            return NewRenderContext( ( ILavaDataDictionary ) mergeFields, enabledCommands );
        }

        private static string _contextKeyLavaEngine = LavaUtilityHelper.GetContextKeyFromType( typeof( ILavaEngine ) );
        private static string _contextKeyHostService = LavaUtilityHelper.GetContextKeyFromType( typeof( ILavaHost ) );

        /// <summary>
        /// Initializes a new template context.
        /// </summary>
        /// <returns></returns>
        protected void InitializeRenderContext( ILavaRenderContext context, IEnumerable<string> enabledCommands = null )
        {
            if ( context == null )
            {
                return;
            }

            if ( enabledCommands != null )
            {
                context.SetEnabledCommands( enabledCommands );
            }

            // Set a reference to the current Lava Engine.
            context.SetInternalField( _contextKeyLavaEngine, this );
            context.SetInternalField( _contextKeyHostService, _hostService );
        }

        /// <summary>
        /// Implement this method to provide a Liquid framework-specific instance of a new render context. 
        /// </summary>
        /// <returns></returns>
        protected abstract ILavaRenderContext OnCreateRenderContext();

        private ILavaTemplateCacheService _cacheService;

        /// <summary>
        /// Gets the current cache service for the Lava Engine.
        /// </summary>
        /// <returns>A reference to the current caching service, or null if caching is not configured.</returns>
        public ILavaTemplateCacheService TemplateCacheService
        {
            get
            {
                return _cacheService;
            }
        }

        private ILavaHost _hostService;

        /// <summary>
        /// Gets the current Host service for the Lava Engine.
        /// </summary>
        /// <returns>A reference to the current host service.</returns>
        public ILavaHost LavaHostService
        {
            get
            {
                return _hostService;
            }
        }

        /// <summary>
        /// Gets the type of third-party framework used to render and parse Lava/Liquid documents.
        /// </summary>
        public abstract Guid EngineIdentifier { get; }

        /// <summary>
        /// Register a type that can be referenced in a template during the rendering process.
        /// </summary>
        /// <param name="type"></param>
        /// <remarks>
        /// The [LavaVisible] and [LavaHidden] custom attributes can be applied to determine the visibility of individual properties.
        /// If these attributes are not applied to any members of the type, all members are visible by default.
        /// </remarks>
        public void RegisterSafeType( Type type )
        {
            RegisterSafeType( type, null );
        }

        /// <summary>
        /// Register a type that can be referenced in a template during the rendering process.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="allowedMembers">
        /// The names of the properties that are visible to the Lava renderer.
        /// Specifying this parameter overrides the effect of any [LavaVisible] and [LavaHidden] custom attributes applied to the type.
        /// </param>
        public abstract void RegisterSafeType( Type type, IEnumerable<string> allowedMembers );

        /// <summary>
        /// Register a shortcode that is defined and implemented in code.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="shortcodeFactoryMethod"></param>
        public void RegisterShortcode( string name, Func<string, ILavaShortcode> shortcodeFactoryMethod )
        {
            var instance = shortcodeFactoryMethod( name );

            if ( instance == null )
            {
                throw new Exception( $"Shortcode factory could not provide a valid instance for \"{name}\" ." );
            }

            // Get a decorated name for the shortcode that will not collide with a regular tag name.
            var registrationKey = LavaUtilityHelper.GetLiquidElementNameFromShortcodeName( name );

            if ( instance.ElementType == LavaShortcodeTypeSpecifier.Inline )
            {
                var tagFactoryMethod = shortcodeFactoryMethod as Func<string, ILavaTag>;

                RegisterTag( registrationKey, tagFactoryMethod );
            }
            else
            {
                RegisterBlock( registrationKey, ( blockName ) =>
               {
                   // Get a shortcode instance using the provided shortcut factory.
                   var shortcode = shortcodeFactoryMethod( registrationKey );

                   // Return the shortcode instance as a RockLavaBlock
                   return shortcode as ILavaBlock;
               } );
            }
        }

        /// <summary>
        /// Register a shortcode that is defined and implemented by a provider component.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="shortcodeFactoryMethod"></param>
        public void RegisterShortcode( string name, ILavaShortcodeProvider provider )
        {
            // Get a decorated name for the shortcode that will not collide with a regular tag name.
            var registrationKey = LavaUtilityHelper.GetLiquidElementNameFromShortcodeName( name );

            var info = provider.GetShortcodeDefinition( name );

            if ( info == null )
            {
                throw new Exception( $"Shortcode provider could not provide a valid instance for \"{name}\" ." );
            }

            if ( info.ElementType == LavaShortcodeTypeSpecifier.Inline )
            {
                RegisterTag( registrationKey, ( tagName ) => { return GetLavaShortcodeInstanceFromProvider<ILavaTag>( provider, tagName, this ); } );
            }
            else
            {
                RegisterBlock( registrationKey, ( blockName ) => { return GetLavaShortcodeInstanceFromProvider<ILavaBlock>( provider, blockName, this ); } );
            }
        }

        /// <summary>
        /// Static factory method to return a named shortcode instance from a shortcode provider.
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="provider"></param>
        /// <param name="tagName"></param>
        /// <returns></returns>
        private static TElement GetLavaShortcodeInstanceFromProvider<TElement>( ILavaShortcodeProvider provider, string tagName, ILavaEngine engine )
            where TElement : class, IRockLavaElement
        {
            var instance = provider.GetShortcodeInstance( tagName, engine ) as TElement;

            if ( instance == null )
            {
                throw new Exception( $"Shortcode provider could not provide a valid instance for \"{tagName}\" ." );
            }

            return instance;
        }

        /// <summary>
        /// Register a shortcode that is defined in the data store.
        /// The definition of a dynamic shortcode can be changed at runtime.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="shortcodeFactoryMethod"></param>
        public void RegisterShortcode( DynamicShortcodeDefinition shortcodeDefinition )
        {
            if ( shortcodeDefinition == null )
            {
                throw new Exception( "Shortcode definition is required." );
            }

            if ( shortcodeDefinition.ElementType == LavaShortcodeTypeSpecifier.Inline )
            {
                // Create a new factory method that returns an initialized Shortcode Tag element.
                Func<string, ILavaTag> tagFactoryMethod = ( tagName ) =>
                {
                    // Call the factory method we have been passed to retrieve the definition of the shortcode.
                    // The definition may change at runtime, so we need to execute the factory method every time we create a new shortcode instance.
                    var shortCodeName = LavaUtilityHelper.GetShortcodeNameFromLiquidElementName( tagName );

                    var shortcodeInstance = new DynamicShortcodeTag();

                    shortcodeInstance.Initialize( shortcodeDefinition, this );

                    return shortcodeInstance;
                };

                // Register the shortcode as a custom tag, but use a decorated registration name that will not collide with a regular element name.
                var registrationKey = LavaUtilityHelper.GetLiquidElementNameFromShortcodeName( shortcodeDefinition.Name );

                RegisterTag( registrationKey, tagFactoryMethod );
            }
            else
            {
                // Create a new factory method that returns an initialized Shortcode Block element.
                Func<string, ILavaBlock> blockFactoryMethod = ( blockName ) =>
                {
                    // Call the factory method we have been passed to retrieve the definition of the shortcode.
                    // The definition may change at runtime, so we need to execute the factory method for each new shortcode instance.
                    var shortCodeName = LavaUtilityHelper.GetShortcodeNameFromLiquidElementName( blockName );

                    var shortcodeInstance = new DynamicShortcodeBlock( shortcodeDefinition, this );

                    return shortcodeInstance;
                };

                // Register the shortcode as a custom block, but use a decorated registration name that will not collide with a regular element name.
                var registrationKey = LavaUtilityHelper.GetLiquidElementNameFromShortcodeName( shortcodeDefinition.Name );

                RegisterBlock( registrationKey, blockFactoryMethod );
            }
        }

        /// <summary>
        /// Register a shortcode that is defined in the data store.
        /// The definition of a dynamic shortcode can be changed at runtime.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="shortcodeFactoryMethod"></param>
        public void RegisterShortcode( string name, Func<string, DynamicShortcodeDefinition> shortcodeFactoryMethod )
        {
            // Create a default instance so we can retrieve the properties of the shortcode.
            var instance = shortcodeFactoryMethod( name );

            if ( instance == null )
            {
                throw new Exception( $"Shortcode factory could not provide a valid instance for \"{name}\" ." );
            }

            if ( instance.ElementType == LavaShortcodeTypeSpecifier.Inline )
            {
                // Create a new factory method that returns an initialized Shortcode Tag element.
                Func<string, ILavaTag> tagFactoryMethod = ( tagName ) =>
                {
                    var shortcodeInstance = GetShortcodeFromFactory<DynamicShortcodeTag>( tagName, shortcodeFactoryMethod );

                    return shortcodeInstance;
                };

                // Register the shortcode as a custom tag, but use a decorated registration name that will not collide with a regular element name.
                var registrationKey = LavaUtilityHelper.GetLiquidElementNameFromShortcodeName( name );

                RegisterTag( registrationKey, tagFactoryMethod );
            }
            else
            {
                // Create a new factory method that returns an initialized Shortcode Block element.
                Func<string, ILavaBlock> blockFactoryMethod = ( blockName ) =>
                {
                    // Call the factory method we have been passed to retrieve the definition of the shortcode.
                    // The definition may change at runtime, so we need to execute the factory method for each new shortcode instance.
                    var shortCodeName = LavaUtilityHelper.GetShortcodeNameFromLiquidElementName( blockName );

                    var shortcodeDefinition = shortcodeFactoryMethod( shortCodeName );

                    var shortcodeInstance = new DynamicShortcodeBlock( shortcodeDefinition, this );

                    return shortcodeInstance;
                };

                // Register the shortcode as a custom block, but use a decorated registration name that will not collide with a regular element name.
                var registrationKey = LavaUtilityHelper.GetLiquidElementNameFromShortcodeName( name );

                RegisterBlock( registrationKey, blockFactoryMethod );
            }
        }

        private T GetShortcodeFromFactory<T>( string shortcodeInternalName, Func<string, DynamicShortcodeDefinition> shortcodeFactoryMethod )
            where T : DynamicShortcode, new()
        {
            // Call the factory method we have been passed to retrieve the definition of the shortcode.
            // The definition may change at runtime, so we need to execute the factory method every time we create a new shortcode instance.
            var shortCodeName = LavaUtilityHelper.GetShortcodeNameFromLiquidElementName( shortcodeInternalName );

            var shortcodeDefinition = shortcodeFactoryMethod( shortCodeName );

            var shortcodeInstance = new T();

            shortcodeInstance.Initialize( shortcodeDefinition, this );

            return shortcodeInstance;
        }

        /// <summary>
        /// Render the provided template.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns>
        /// The rendered output of the template.
        /// If the template is invalid, returns an error message or an empty string according to the current ExceptionHandlingStrategy setting.
        /// </returns>
        public LavaRenderResult RenderTemplate( string inputTemplate )
        {
            return RenderTemplate( inputTemplate, LavaRenderParameters.Default );
        }

        /// <summary>
        /// Render the provided template in the specified context.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="context"></param>
        /// <returns>
        /// The rendered output of the template.
        /// If the template is invalid, returns an error message or an empty string according to the current ExceptionHandlingStrategy setting.
        /// </returns>
        public LavaRenderResult RenderTemplate( string inputTemplate, ILavaDataDictionary mergeFields )
        {
            var result = RenderTemplate( inputTemplate, LavaRenderParameters.WithContext( this.NewRenderContext( mergeFields ) ) );

            return result;
        }

        /// <summary>
        /// Render the provided template using the specified parameters.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="parameters"></param>
        /// <returns>
        /// The result of the render operation.
        /// If the template is invalid, the Text property may contain an error message or an empty string according to the current ExceptionHandlingStrategy setting.
        /// </returns>
        public LavaRenderResult RenderTemplate( string inputTemplate, LavaRenderParameters parameters )
        {
            ILavaTemplate template;
            LavaRenderParameters activeParameters;

            // Copy the render parameters so they can be altered without affecting the input object.
            if ( parameters == null )
            {
                activeParameters = new LavaRenderParameters();
            }
            else
            {
                activeParameters = parameters.Clone();
            }

            var renderResult = new LavaRenderResult();

            var exceptionStrategy = activeParameters.ExceptionHandlingStrategy ?? this.ExceptionHandlingStrategy;

            try
            {
                LavaParseResult parseResult = null;
                if ( _cacheService != null )
                {
                    var result = _cacheService.AddOrGetTemplate( this, inputTemplate, activeParameters.CacheKey );
                    template = result.Template;

                    if ( result.ParseError != null )
                    {
                        // If the template could not be parsed, it should be added to the cache as an error template.
                        // Subsequent requests for the same template should render the compiled error message rather than throw an exception.
                        throw result.ParseError;
                    }
                }
                else
                {
                    template = null;
                }

                if ( template == null )
                {
                    parseResult = ParseTemplate( inputTemplate );

                    if ( parseResult.HasErrors )
                    {
                        throw parseResult.Error;
                    }

                    template = parseResult.Template;
                }

                if ( activeParameters.Context == null )
                {
                    activeParameters.Context = NewRenderContext();
                }

                renderResult = RenderTemplate( template, activeParameters );
            }
            catch ( Exception ex )
            {
                var lre = GetLavaExceptionForRenderError( ex, inputTemplate );

                string message;

                if ( ex is System.Threading.ThreadAbortException )
                {
                    // If the requesting thread terminated unexpectedly, return an empty string.
                    // This may happen, for example, when a Lava template triggers a page redirect in a web application.
                    message = "{Request aborted}";
                }
                else
                {
                    ProcessException( lre, exceptionStrategy, out message );
                }

                renderResult.Error = ex;
                renderResult.Text = message;
            }

            return renderResult;
        }

        /// <summary>
        /// Render the provided template using the specified parameters.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="context"></param>
        /// <returns>
        /// The result of the render operation.
        /// If the template is invalid, the Text property may contain  an error message or an empty string according to the current ExceptionHandlingStrategy setting.
        /// </returns>
        public LavaRenderResult RenderTemplate( ILavaTemplate template, ILavaRenderContext context )
        {
            return RenderTemplate( template, new LavaRenderParameters { Context = context } );
        }

        /// <summary>
        /// Render the provided template using the specified parameters.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="parameters"></param>
        /// <returns>
        /// The result of the render operation.
        /// If the template is invalid, the Text property may contain  an error message or an empty string according to the current ExceptionHandlingStrategy setting.
        /// </returns>
        public LavaRenderResult RenderTemplate( ILavaTemplate template, LavaRenderParameters parameters )
        {
            parameters = parameters ?? new LavaRenderParameters();

            LavaRenderResult result;
            LavaRenderParameters callParameters;

            try
            {
                if ( parameters == null )
                {
                    callParameters = new LavaRenderParameters();
                }
                else if ( parameters.Context == null )
                {
                    callParameters = parameters.Clone();
                    callParameters.Context = NewRenderContext();
                }
                else
                {
                    if ( parameters.Context.GetType() == typeof( LavaRenderContext ) )
                    {
                        callParameters = parameters.Clone();

                        // Convert the default context to an engine-specific implementation.
                        var engineContext = NewRenderContext();

                        engineContext.SetInternalFields( parameters.Context.GetInternalFields() );
                        engineContext.SetMergeFields( parameters.Context.GetMergeFields() );
                        engineContext.SetEnabledCommands( parameters.Context.GetEnabledCommands() );

                        // Create a copy of the parameters to ensure the input parameter remains unchanged.
                        callParameters.Context = engineContext;
                    }
                    else
                    {
                        callParameters = parameters;
                    }
                }

                result = OnRenderTemplate( template, callParameters );

                if ( result.Error != null )
                {
                    result.Error = GetLavaExceptionForRenderError( result.Error, template?.GetDescription() );
                }
            }
            catch ( LavaInterruptException )
            {
                // This exception is intentionally thrown by a component to halt the render process.
                result = new LavaRenderResult();

                result.Text = string.Empty;
            }
            catch ( Exception ex )
            {
                if ( ex is ThreadAbortException )
                {
                    // Ignore this exception, the calling thread is terminating and no result is required.
                    return null;
                }

                result = new LavaRenderResult();

                var lre = GetLavaExceptionForRenderError( ex, template?.GetDescription() );

                string message;

                ProcessException( lre, parameters.ExceptionHandlingStrategy, out message );

                result.Error = lre;
                result.Text = message;
            }

            return result;
        }

        /// <summary>
        /// Override this method to render the Lava template using the underlying rendering engine.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected abstract LavaRenderResult OnRenderTemplate( ILavaTemplate inputTemplate, LavaRenderParameters parameters );

        /// <summary>
        /// Compare two objects for equivalence according to the applicable Lava equality rules for the input object types.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>True if the two objects are considered equal.</returns>
        public abstract bool AreEqualValue( object left, object right );

        /// <summary>
        /// Attempt to parse and compile the specified Lava source text into a valid Lava template.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns>A LavaParseResult containing the result of the operation.</returns>
        public LavaParseResult ParseTemplate( string inputTemplate )
        {
            var result = new LavaParseResult();

            try
            {
                result.Template = OnParseTemplate( inputTemplate );
            }
            catch ( ThreadAbortException )
            {
                // Ignore this exception, the calling thread is terminating.
            }
            catch ( Exception ex )
            {
                var lpe = ex as LavaParseException ?? new LavaParseException( this.EngineName, inputTemplate, ex.Message );

                result.Error = lpe;

                ProcessException( lpe, null, out _ );
            }

            return result;
        }

        /// <summary>
        /// Override this method to implement parsing and compilation of Lava source text.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        protected abstract ILavaTemplate OnParseTemplate( string inputTemplate );

        /// <summary>
        /// Get the collection of registered Lava template elements.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, ILavaElementInfo> GetRegisteredElements()
        {
            var tags = new Dictionary<string, ILavaElementInfo>();

            foreach ( var tagWrapper in _lavaElements )
            {
                var info = new LavaTagInfo();

                info.Name = tagWrapper.Key;

                info.SystemTypeName = tagWrapper.Value.SystemTypeName;

                tags.Add( info.Name, info );
            }

            return tags;
        }

        /// <inheritdoc/>
        public abstract List<string> GetRegisteredFilterNames();

        #region Tags

        private Dictionary<string, ILavaElementInfo> _lavaElements = new Dictionary<string, ILavaElementInfo>( StringComparer.OrdinalIgnoreCase );

        /// <summary>
        /// Register a Lava Tag element.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factoryMethod"></param>
        public virtual void RegisterTag( string name, Func<string, ILavaTag> factoryMethod )
        {
            if ( string.IsNullOrWhiteSpace( name ) )
            {
                throw new ArgumentException( "Name must be specified." );
            }

            name = name.Trim().ToLower();

            var tagInstance = factoryMethod( name );

            var tagInfo = new LavaTagInfo();

            tagInfo.Name = name;
            tagInfo.FactoryMethod = factoryMethod;

            tagInfo.IsAvailable = ( tagInstance != null );

            if ( tagInstance != null )
            {
                tagInfo.SystemTypeName = tagInstance.GetType().FullName;
            }

            _lavaElements[name] = tagInfo;
        }

        /// <summary>
        /// Register a Lava Block element.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factoryMethod"></param>
        public virtual void RegisterBlock( string name, Func<string, ILavaBlock> factoryMethod )
        {
            if ( string.IsNullOrWhiteSpace( name ) )
            {
                throw new ArgumentException( "Name must be specified." );
            }

            name = name.Trim().ToLower();

            var blockInstance = factoryMethod( name );

            var blockInfo = new LavaBlockInfo();

            blockInfo.Name = name;
            blockInfo.FactoryMethod = factoryMethod;

            blockInfo.IsAvailable = ( blockInstance != null );

            if ( blockInstance != null )
            {
                blockInfo.SystemTypeName = blockInstance.GetType().FullName;
            }

            _lavaElements[name] = blockInfo;
        }

        #endregion

        #region Filters

        /// <summary>
        /// Register a filter function.
        /// A filter must be defined as a public static function that returns a string.
        /// </summary>
        /// <param name="filterMethod"></param>
        /// <param name="filterName"></param>
        public void RegisterFilter( MethodInfo filterMethod, string filterName = null )
        {
            try
            {
                if ( filterMethod == null )
                {
                    throw new Exception( "Invalid filter method reference." );
                }

                OnRegisterFilter( filterMethod, filterName );
            }
            catch ( ThreadAbortException )
            {
                // Ignore this exception, the calling thread is terminating.
            }
            catch ( Exception ex )
            {
                throw new LavaException( ex );
            }
        }

        /// <summary>
        /// Register one or more filter functions that are implemented by the supplied Type.
        /// A filter must be defined as a public static function that returns a string.
        /// </summary>
        /// <param name="implementingType"></param>
        public void RegisterFilters( Type implementingType )
        {
            OnRegisterFilters( implementingType );
        }

        /// <summary>
        /// Override this method to register the filters defined by the provided Type with the underlying Liquid processing framework.
        /// </summary>
        /// <param name="implementingType"></param>
        protected abstract void OnRegisterFilters( Type implementingType );

        /// <summary>
        /// Override this method to register the filter defined by the provided method with the underlying Liquid processing framework.
        /// </summary>
        /// <param name="implementingType"></param>
        /// <param name="filterName"></param>
        protected abstract void OnRegisterFilter( MethodInfo filterMethod, string filterName );

        #endregion

        protected void ProcessException( Exception ex, ExceptionHandlingStrategySpecifier? exceptionStrategy )
        {
            ProcessException( ex, exceptionStrategy, out _ );
        }

        protected void ProcessException( Exception ex, ExceptionHandlingStrategySpecifier? exceptionStrategy, out string message )
        {
            // Raise an event to notify subscribers that an exception has occurred.
            if ( this.ExceptionEncountered != null )
            {
                ExceptionEncountered( this, new LavaEngineExceptionEventArgs { Exception = GetLavaException( ex ) } );
            }

            // Process the exception according to the specified exception strategy.
            exceptionStrategy = exceptionStrategy ?? this.ExceptionHandlingStrategy;

            if ( exceptionStrategy == ExceptionHandlingStrategySpecifier.RenderToOutput )
            {
                // [2021-06-04] DL
                // Some Lava custom components have been designed to throw base Exceptions that contain important configuration instructions.
                // However, there is currently no reliable method of identifying which exceptions in the stack offer an appropriate level of detail for display.
                // To accomodate this design, we will display the message associated with the highest level Exception that is not a LavaException.
                // In the future, this behavior could be more reliably implemented by defining a LavaConfigurationException that identifies
                // an error message as being suitable for display in the render output.
                var outputEx = ex;

                // Get an error message that is suitable for rendering to final output.
                if ( outputEx is LavaException le )
                {
                    message = le.GetUserMessage();
                }
                else
                {
                    message = $"Lava Error: { ex.Message }";
                }
            }
            else if ( exceptionStrategy == ExceptionHandlingStrategySpecifier.Ignore )
            {
                // Ignore the exception and return a null render result.
                // The caller must inspect the return object or subscribe to the ExceptionEncountered event in order to catch errors in the rendering process.
                message = null;
            }
            else
            {
                // Ensure that any exception thrown by the engine is a LavaException so it can be easily identified.
                throw GetLavaException( ex );
            }
        }

        /// <summary>
        /// Returns a top-level Lava Exception that can be identified and processed by a caller of the Lava library.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private LavaException GetLavaException( Exception ex )
        {
            if ( ex == null )
            {
                return new LavaException( "Lava Processing Error: Undefined exception." );
            }

            if ( ex.GetType() == typeof( LavaException ) )
            {
                // If this is a LavaException, return it unchanged.
                return (LavaException) ex;
            }

            // Return a top-level summary message.
            if ( ex is LavaException le )
            {
                // If the exception is derived from LavaException, return the user message.
                return new LavaException( le.GetUserMessage(), ex );
            }

            return new LavaException( $"Lava Processing Error: { ex.Message }", ex );
        }

        private LavaException GetLavaExceptionForRenderError( Exception ex, string templateText = "{compiled}" )
        {
            if ( ex is LavaRenderException lre )
            {
                return lre;
            }
            // If this is a parse exception, there is no need to include a subsequent render exception.
            if ( ex is LavaParseException lpe )
            {
                return lpe;
            }

            return new LavaRenderException( this.EngineName, templateText, ex );
        }

        /// <summary>
        /// Gets or sets the strategy for handling exceptions encountered during the rendering process.
        /// </summary>
        public ExceptionHandlingStrategySpecifier ExceptionHandlingStrategy { get; set; } = ExceptionHandlingStrategySpecifier.RenderToOutput;

        public string ServiceName
        {
            get
            {
                return this.EngineName;
            }
        }

        public Guid ServiceIdentifier
        {
            get
            {
                return this.EngineIdentifier;
            }
        }

        private static LavaToLiquidTemplateConverter _lavaToLiquidConverter = new LavaToLiquidTemplateConverter();

        /// <summary>
        /// Convert a Lava template to a Liquid-compatible template by replacing Lava-specific syntax and keywords.
        /// </summary>
        /// <param name="lavaTemplateText"></param>
        /// <returns></returns>
        public string ConvertToLiquid( string lavaTemplateText )
        {
            return _lavaToLiquidConverter.ConvertToLiquid( lavaTemplateText );
        }

        /// <summary>
        /// Remove all entries from the template cache.
        /// </summary>
        public void ClearTemplateCache()
        {
            if ( _cacheService != null )
            {
                _cacheService.ClearCache();
            }
        }

        /// <summary>
        /// Remove the registration entry for a Tag with the specified name.
        /// </summary>
        /// <param name="name"></param>
        public bool DeregisterTag( string name )
        {
            var registrationKey = LavaUtilityHelper.GetLiquidElementNameFromShortcodeName( name );

            if ( !_lavaElements.ContainsKey( registrationKey ) )
            {
                return false;
            }

            _lavaElements.Remove( registrationKey );

            return true;
        }

        /// <summary>
        /// Remove the registration entry for a Block with the specified name.
        /// </summary>
        /// <param name="name"></param>
        public bool DeregisterBlock( string name )
        {
            if ( !_lavaElements.ContainsKey( name ) )
            {
                return false;
            }

            _lavaElements.Remove( name );

            return true;
        }

        /// <summary>
        /// Remove the registration entry for a Shortcode with the specified name.
        /// </summary>
        /// <param name="name"></param>
        public bool DeregisterShortcode( string name )
        {
            // Get the decorated name for the shortcode.
            var registrationKey = LavaUtilityHelper.GetLiquidElementNameFromShortcodeName( name );

            if ( !_lavaElements.ContainsKey( registrationKey ) )
            {
                return false;
            }

            _lavaElements.Remove( registrationKey );

            return true;
        }
    }
}