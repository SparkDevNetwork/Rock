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

namespace Rock.Lava
{
    /// <summary>
    /// Provides base functionality for an engine that can parse and render Lava Templates.
    /// </summary>
    public abstract class LavaEngineBase : ILavaEngine
    {
        private List<string> _defaultEnabledCommands = new List<string>();

        /// <summary>
        /// Initializes the Lava engine with the specified options.
        /// </summary>
        public void Initialize( LavaEngineConfigurationOptions options )
        {
            if ( options == null )
            {
                options = new LavaEngineConfigurationOptions();
            }

            // Connect the cache service to this Lava Engine instance.
            _cacheService = options.CacheService;

            if ( _cacheService != null )
            {
                _cacheService.LavaEngine = this;
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

        /// <summary>
        /// Create a new template context.
        /// </summary>
        /// <returns></returns>
        public ILavaRenderContext NewRenderContext()
        {
            var context = OnCreateRenderContext();

            if ( context == null )
            {
                throw new LavaException( "Failed to create a new render context." );
            }

            context.SetEnabledCommands( this.DefaultEnabledCommands );

            return context;
        }

        /// <summary>
        /// Create a new template context and add the specified merge fields.
        /// </summary>
        /// <param name="enabledCommands"></param>
        /// <returns></returns>
        public ILavaRenderContext NewRenderContext( IEnumerable<string> enabledCommands )
        {
            var context = OnCreateRenderContext();

            if ( context == null )
            {
                throw new LavaException( "Failed to create a new render context." );
            }

            enabledCommands = enabledCommands ?? this.DefaultEnabledCommands;

            context.SetEnabledCommands( enabledCommands );

            return context;
        }

        /// <summary>
        /// Create a new template context and add the specified merge fields.
        /// </summary>
        /// <param name="mergeFields"></param>
        /// <returns></returns>
        public ILavaRenderContext NewRenderContext( ILavaDataDictionary mergeFields, IEnumerable<string> enabledCommands = null )
        {
            var context = OnCreateRenderContext();

            if ( context == null )
            {
                throw new LavaException( "Failed to create a new render context." );
            }

            context.SetMergeFields( mergeFields );

            enabledCommands = enabledCommands ?? this.DefaultEnabledCommands;

            context.SetEnabledCommands( enabledCommands );

            return context;
        }

        /// <summary>
        /// Create a new template context and add the specified merge fields.
        /// </summary>
        /// <param name="mergeFields"></param>
        /// <returns></returns>
        public ILavaRenderContext NewRenderContext( IDictionary<string, object> mergeFields, IEnumerable<string> enabledCommands = null )
        {
            var context = OnCreateRenderContext();

            if ( context == null )
            {
                throw new LavaException( "Failed to create a new render context." );
            }

            context.SetMergeFields( mergeFields );

            enabledCommands = enabledCommands ?? this.DefaultEnabledCommands;

            context.SetEnabledCommands( enabledCommands );

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
            return NewRenderContext( (ILavaDataDictionary)mergeFields, enabledCommands );
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

        /// <summary>
        /// Gets the type of third-party framework used to render and parse Lava/Liquid documents.
        /// </summary>
        public abstract LavaEngineTypeSpecifier EngineType { get; }

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
        /// <param name="shortcodeFactoryMethod"></param>
        public void RegisterStaticShortcode( Func<string, ILavaShortcode> shortcodeFactoryMethod )
        {
            var instance = shortcodeFactoryMethod( "default" );

            if ( instance == null )
            {
                throw new Exception( "Shortcode factory could not provide a valid instance for \"default\"." );
            }

            RegisterShortcode( instance.SourceElementName, shortcodeFactoryMethod );
        }

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
            return RenderTemplate( inputTemplate, LavaRenderParameters.Default() );
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
        //public string RenderTemplate( string inputTemplate, ILavaRenderContext context )
        //{
        //    string output;
        //    List<Exception> errors;

        //    TryRenderTemplate( inputTemplate, context, out output, out errors );

        //    return output;
        //}

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
        /// If the template is invalid, the Text property may contain  an error message or an empty string according to the current ExceptionHandlingStrategy setting.
        /// </returns>
        public LavaRenderResult RenderTemplate( ILavaTemplate template, LavaRenderParameters parameters )
        {
            parameters = parameters ?? new LavaRenderParameters();

            var renderResult = new LavaRenderResult();

            try
            {
                List<Exception> errors;

                if ( parameters.Context == null )
                {
                    parameters.Context = NewRenderContext();
                }

                string output;

                var isRendered = OnTryRender( template, parameters, out output, out errors );

                renderResult.Text = output;
                renderResult.Errors = errors;
            }
            catch ( Exception ex )
            {
                ProcessException( ex );

                renderResult.Errors = new List<Exception> { ex };
                renderResult.Text = null;
            }

            return renderResult;
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
            public LavaRenderResult RenderTemplate( string inputTemplate, LavaRenderParameters parameters )
        {
            ILavaTemplate template;

            parameters = parameters ?? new LavaRenderParameters();

            var renderResult = new LavaRenderResult();

            try
            {
                if ( _cacheService != null )
                {
                    template = _cacheService.GetOrAddTemplate( inputTemplate, parameters.CacheKey );
                }
                else
                {
                    template = null;
                }

                bool isParsed = ( template != null );

                List<Exception> errors;

                if ( !isParsed )
                {
                    var parseResult = ParseTemplate( inputTemplate ); //, out template, out errors );
                    //isParsed = TryParseTemplate( inputTemplate, out template, out errors );

                    if ( parseResult.HasErrors )
                    {
                        if ( parseResult.Template == null
                             || this.ExceptionHandlingStrategy == ExceptionHandlingStrategySpecifier.Throw )
                        {
                            throw new LavaException( "Lava Template render operation failed." );
                        }
                    }

                    template = parseResult.Template;
                }

                if ( parameters.Context == null )
                {
                    parameters.Context = NewRenderContext();
                }

                string output;

                var isRendered = OnTryRender( template, parameters, out output, out errors );

                renderResult.Text = output;
                renderResult.Errors = errors;
            }
            catch ( Exception ex )
            {
                ProcessException( ex );

                renderResult.Errors = new List<Exception> { ex };
                renderResult.Text = null;
            }

            return renderResult;
        }

        /// <summary>
        /// Try to render the provided template.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        //[Obsolete( "Use LavaRenderResult RenderTemplate() instead." )]
        //public bool TryRenderTemplate( string inputTemplate, out string output, out List<Exception> errors )
        //{
        //    return TryRenderTemplate( inputTemplate, mergeFields: null, out output, out errors );
        //}

        /// <summary>
        /// Try to render the provided template with the specified merge fields.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="mergeFields"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        //public bool TryRenderTemplate( string inputTemplate, ILavaDataDictionary mergeFields, out string output, out List<Exception> errors )
        //{
        //    ILavaRenderContext context;

        //    if ( mergeFields != null )
        //    {
        //        context = NewRenderContext();

        //        context.SetMergeFields( mergeFields );
        //    }
        //    else
        //    {
        //        context = null;
        //    }

        //    return TryRenderTemplate( inputTemplate, context, out output, out errors );
        //}

        /// <summary>
        /// Try to render the provided template in the specified context.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="context"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        //public bool TryRenderTemplate( string inputTemplate, ILavaRenderContext context, out string output, out List<Exception> errors )
        //{
        //    var result = RenderTemplate( inputTemplate, new LavaRenderParameters { Context = context } );

        //    output = result.Text;
        //    errors = result.Errors;

        //    return !result.HasErrors;            
        //}

        /// <summary>
        /// Try to render the specified Lava template using the specified parameters.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="parameters"></param>
        /// <param name="output"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        //public bool TryRenderTemplate( ILavaTemplate inputTemplate, LavaRenderParameters parameters, out string output, out List<Exception> errors )
        //{
        //    return OnTryRender( inputTemplate, parameters, out output, out errors );
        //}

        /// <summary>
        /// Override this method to render the Lava template using the underlying rendering engine.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="parameters"></param>
        /// <param name="output"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        protected abstract bool OnTryRender( ILavaTemplate inputTemplate, LavaRenderParameters parameters, out string output, out List<Exception> errors );

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
            catch ( Exception ex )
            {
                result.Errors = new List<Exception> { ex };

                string message;

                ProcessException( ex, out message );

                if ( !string.IsNullOrWhiteSpace( message ) )
                {
                    // Create a new template containing the error message.
                    if ( this.ExceptionHandlingStrategy == ExceptionHandlingStrategySpecifier.RenderToOutput )
                    {
                        result.Template = OnParseTemplate( message );
                    }
                }
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

        protected void ProcessException( Exception ex )
        {
            string discardedOutput;

            ProcessException( ex, out discardedOutput );
        }

        protected void ProcessException( Exception ex, out string message )
        {
            if ( this.ExceptionHandlingStrategy == ExceptionHandlingStrategySpecifier.RenderToOutput )
            {
                message = $"Lava Error: {ex.Message}";
            }
            else if ( this.ExceptionHandlingStrategy == ExceptionHandlingStrategySpecifier.Ignore )
            {
                // We should probably log the message here rather than failing silently, but this preserves current behavior.
                message = null;
            }
            else
            {
                // Ensure that any exception thrown by the engine is a LavaException so it can be easily identified.
                if ( ex is LavaException )
                {
                    throw ex;
                }
                else
                {
                    throw new LavaException( "A Lava Processing Error occurred. Check the inner exception for details.", ex );
                }                
            }
        }

        /// <summary>
        /// Gets or sets the strategy for handling exceptions encountered during the rendering process.
        /// </summary>
        public ExceptionHandlingStrategySpecifier ExceptionHandlingStrategy { get; set; } = ExceptionHandlingStrategySpecifier.RenderToOutput;

        /// <summary>
        /// Convert a Lava template to a Liquid-compatible template by replacing Lava-specific syntax and keywords.
        /// </summary>
        /// <param name="lavaTemplateText"></param>
        /// <returns></returns>
        public string ConvertToLiquid( string lavaTemplateText )
        {
            var converter = new LavaToLiquidTemplateConverter();

            return converter.ConvertToLiquid( lavaTemplateText );
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
        public void DeregisterTag( string name )
        {
            var registrationKey = LavaUtilityHelper.GetLiquidElementNameFromShortcodeName( name );

            if ( _lavaElements.ContainsKey( registrationKey ) )
            {
                _lavaElements.Remove( registrationKey );
            }
        }

        /// <summary>
        /// Remove the registration entry for a Block with the specified name.
        /// </summary>
        /// <param name="name"></param>
        public void DeregisterBlock( string name )
        {
            if ( _lavaElements.ContainsKey( name ) )
            {
                _lavaElements.Remove( name );
            }
        }

        /// <summary>
        /// Remove the registration entry for a Shortcode with the specified name.
        /// </summary>
        /// <param name="name"></param>
        public void DeregisterShortcode( string name )
        {
            // Get the decorated name for the shortcode.
            var registrationKey = LavaUtilityHelper.GetLiquidElementNameFromShortcodeName( name );

            if ( _lavaElements.ContainsKey( registrationKey ) )
            {
                _lavaElements.Remove( registrationKey );
            }
        }

        /// <summary>
        /// Parse the input text into a compiled Lava template.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns></returns>
        //public LavaParseResult ParseTemplate( string inputTemplate )
        //{
        //    ILavaTemplate template;
        //    List<Exception> errors;

        //    var isValid = TryParseTemplate( inputTemplate, out template, out errors );

        //    if ( !isValid )
        //    {
        //        throw new LavaException( "ParseTemplate failed. The Lava template is invalid." );
        //    }

        //    return template;
        //}
    }
}