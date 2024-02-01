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
    /// Represents the Lava Engine that is responsible for compiling and rendering templates.
    /// </summary>
    public interface ILavaEngine : ILavaService
    {
        /// <summary>
        /// An event that is triggered when the LavaEngine encounters a processing exception.
        /// </summary>
        event EventHandler<LavaEngineExceptionEventArgs> ExceptionEncountered;

        /// <summary>
        /// Remove all items from the template cache.
        /// </summary>
        void ClearTemplateCache();

        /// <summary>
        /// Gets the component that implements template caching for the Lava Engine.
        /// </summary>
        ILavaTemplateCacheService TemplateCacheService { get; }

        /// <summary>
        /// Set configuration options for the Lava engine.
        /// </summary>
        /// <param name="options"></param>
        void Initialize( LavaEngineConfigurationOptions options = null );

        /// <summary>
        /// The descriptive name of the Liquid framework on which Lava is currently operating.
        /// </summary>
        string EngineName { get; }

        /// <summary>
        /// The Liquid framework currently used to parse and render Lava templates.
        /// </summary>
        Guid EngineIdentifier { get; }

        /// <summary>
        /// Creates a new render context instance.
        /// </summary>
        /// <returns></returns>
        ILavaRenderContext NewRenderContext();

        /// <summary>
        /// Creates a new render context instance with the specified Lava commands enabled.
        /// </summary>
        /// <param name="enabledCommands"></param>
        /// <returns></returns>
        ILavaRenderContext NewRenderContext( IEnumerable<string> enabledCommands );

        /// <summary>
        /// Creates a new render context instance.
        /// </summary>
        /// <param name="mergeFields"></param>
        /// <param name="enabledCommands"></param>
        /// <returns></returns>
        ILavaRenderContext NewRenderContext( IDictionary<string, object> mergeFields, IEnumerable<string> enabledCommands = null );

        /// <summary>
        /// Creates a new render context instance.
        /// </summary>
        /// <param name="mergeFields"></param>
        /// <param name="enabledCommands"></param>
        /// <returns></returns>
        ILavaRenderContext NewRenderContext( ILavaDataDictionary mergeFields, IEnumerable<string> enabledCommands = null );

        /// <summary>
        /// Creates a new render context instance.
        /// </summary>
        /// <param name="mergeFields"></param>
        /// <param name="enabledCommands"></param>
        /// <returns></returns>
        /// <remarks>This method overload exists to disambiguate calls using the LavaDataDictionary parameter.</remarks>
        ILavaRenderContext NewRenderContext( LavaDataDictionary mergeFields, IEnumerable<string> enabledCommands = null );

        /// <summary>
        /// Register one or more filter functions that are implemented by the supplied Type.
        /// A filter must be defined as a public static function that returns a value.
        /// </summary>
        /// <param name="implementingType"></param>
        void RegisterFilters( Type implementingType );

        /// <summary>
        /// Register a filter function.
        /// A filter must be defined as a public static function that returns a value.
        /// </summary>
        /// <param name="filterMethod"></param>
        /// <param name="filterName"></param>
        void RegisterFilter( MethodInfo filterMethod, string filterName = null );

        /// <summary>
        /// Register a Lava Tag element.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factoryMethod"></param>
        void RegisterTag( string name, Func<string, ILavaTag> factoryMethod );

        /// <summary>
        /// Register a Lava Block element.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factoryMethod"></param>
        void RegisterBlock( string name, Func<string, ILavaBlock> factoryMethod );

        /// <summary>
        /// Registers a shortcode definition that can be used to create new instances of a shortcode during the rendering process.
        /// </summary>
        /// <param name="shortcodeDefinition"></param>
        void RegisterShortcode( DynamicShortcodeDefinition shortcodeDefinition );

        /// <summary>
        /// Registers a shortcode with a factory method that provides the definition of the shortcode on demand.
        /// The supplied definition is used to create a new DynamicShortcode instance to render the shortcode.
        /// This method of registration is suitable for shortcodes that can be modified at runtime, such as user-defined shortcodes stored in a Rock database.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factoryMethod"></param>
        void RegisterShortcode( string name, Func<string, DynamicShortcodeDefinition> factoryMethod );

        /// <summary>
        /// Registers a shortcode with a factory method that provides a new instance of the shortcode.
        /// This method of registration is suitable for shortcodes that are defined by a code component and cannot be modified at runtime.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factoryMethod"></param>
        void RegisterShortcode( string name, Func<string, ILavaShortcode> factoryMethod );

        /// <summary>
        /// Remove the registration entry for a Tag with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>true, if the item was previously registered.</returns>
        bool DeregisterTag( string name );

        /// <summary>
        /// Remove the registration entry for a Block with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>true, if the item was previously registered.</returns>
        bool DeregisterBlock( string name );

        /// <summary>
        /// Remove the registration entry for a Shortcode with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>true, if the item was previously registered.</returns>
        bool DeregisterShortcode( string name );

        /// <summary>
        /// Gets the collection of all registered Lava document elements.
        /// </summary>
        /// <returns></returns>
        Dictionary<string, ILavaElementInfo> GetRegisteredElements();

        /// <summary>
        /// Gets the collection of all registered Lava filters.
        /// </summary>
        /// <returns></returns>
        List<string> GetRegisteredFilterNames();

        /// <summary>
        /// Parse the provided text into a compiled Lava template object. The resulting template can be used to render output with a variety of render contexts.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns>A compiled template object.</returns>
        LavaParseResult ParseTemplate( string inputTemplate );

        /// <summary>
        /// Render the provided template.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns>
        /// The rendered output of the template.
        /// If the template is invalid, returns an error message or an empty string according to the current ExceptionHandlingStrategy setting.
        /// </returns>
        LavaRenderResult RenderTemplate( string inputTemplate );

        /// <summary>
        /// Render the provided template in a new context with the specified merge fields.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="mergeFields">The collection of merge fields to be added to the context used to render the template.</param>
        /// <returns>
        /// The rendered output of the template.
        /// If the template is invalid, returns an error message or an empty string according to the current ExceptionHandlingStrategy setting.
        /// </returns>
        LavaRenderResult RenderTemplate( string inputTemplate, ILavaDataDictionary mergeFields );

        /// <summary>
        /// Render the provided template in a new context with the specified parameters.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="parameters">The settings applied to the rendering process.</param>
        /// <returns>
        /// A LavaRenderResult object, containing the rendered output of the template or any errors encountered during the rendering process.
        /// </returns>
        LavaRenderResult RenderTemplate( string inputTemplate, LavaRenderParameters parameters );

        /// <summary>
        /// Render the provided template in a new context with the specified merge fields.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="parameters">The settings applied to the rendering process.</param>
        /// <returns>
        /// A LavaRenderResult object, containing the rendered output of the template or any errors encountered during the rendering process.
        /// </returns>
        LavaRenderResult RenderTemplate( ILavaTemplate inputTemplate, LavaRenderParameters parameters );

        /// <summary>
        /// Render the provided template in a new context with the specified merge fields.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="context">The context in which the template should be rendered.</param>
        /// <returns>
        /// A LavaRenderResult object, containing the rendered output of the template or any errors encountered during the rendering process.
        /// </returns>
        LavaRenderResult RenderTemplate( ILavaTemplate inputTemplate, ILavaRenderContext context );

        /// <summary>
        /// Register a type that can be referenced in a template during the rendering process.
        /// </summary>
        /// <param name="type"></param>
        /// <remarks>
        /// The [LavaVisible] and [LavaHidden] custom attributes can be applied to determine the visibility of individual properties.
        /// If these attributes are not applied to any members of the type, all members are visible by default.
        /// </remarks>
        void RegisterSafeType( Type type );

        /// <summary>
        /// Register a type that can be referenced in a template during the rendering process.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="allowedMembers">
        /// The names of the properties that are visible to the Lava renderer.
        /// Specifying this parameter overrides the effect of any [LavaVisible] and [LavaHidden] custom attributes applied to the type.
        /// </param>
        void RegisterSafeType( Type type, IEnumerable<string> allowedMembers );

        /// <summary>
        /// Compare two objects for equivalence according to the applicable Lava equality rules for the input object types.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>True if the two objects are considered equal.</returns>
        bool AreEqualValue( object left, object right );

        /// <summary>
        /// Gets or sets the strategy for handling exceptions encountered during the rendering process.
        /// </summary>
        ExceptionHandlingStrategySpecifier ExceptionHandlingStrategy { get; set; }
    }

    #region Enumerations

    /// <summary>
    /// Specifies a strategy for handling exceptions encountered during the template rendering process.
    /// </summary>
    public enum ExceptionHandlingStrategySpecifier
    {
        /// <summary>
        /// Throw the exception to be handled by the caller.
        /// </summary>
        Throw = 0,
        /// <summary>
        /// Render the exception message as template output.
        /// </summary>
        RenderToOutput = 1,
        /// <summary>
        /// Ignore the exception and do not render any output.
        /// </summary>
        Ignore = 2
    }

    #endregion

    #region Support Classes

    /// <summary>
    /// Contains the result of a Lava template rendering operation.
    /// </summary>
    public class LavaRenderResult
    {
        /// <summary>
        /// The final output of the render process.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The top-level exception encountered while processing the render operation, or null if the operation succeeded.
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        /// A flag indicating if the operation encountered any errors.
        /// </summary>
        public bool HasErrors
        {
            get
            {
                return this.Error != null;
            }
        }

        /// <summary>
        /// Get the Error result as a LavaException.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public LavaException GetLavaException( string message = null )
        {
            if ( this.Error == null )
            {
                return null;
            }

            if ( string.IsNullOrWhiteSpace( message ) )
            {
                message = this.Error.Message;
            }

            if ( this.Error is LavaException le )
            {
                return le;
            }
            else
            {
                return new LavaException( message, this.Error );
            }
        }
    }

    /// <summary>
    /// Contains the result of a Lava template parsing operation.
    /// </summary>
    public class LavaParseResult
    {
        /// <summary>
        /// A compiled template object that is the result of the parse process.
        /// </summary>
        public ILavaTemplate Template;

        /// <summary>
        /// The top-level exception encountered while processing the parse operation, or null if the operation succeeded.
        /// </summary>
        public Exception Error;

        /// <summary>
        /// A flag indicating if the operation encountered any errors.
        /// </summary>
        public bool HasErrors
        {
            get
            {
                return this.Error != null;
            }
        }

        /// <summary>
        /// Get the Error result as a LavaException.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public LavaException GetLavaException()
        {
            if ( this.Error == null )
            {
                return null;
            }

            if ( this.Error is LavaException le )
            {
                return le;
            }
            else
            {
                return new LavaException( "An error occurred while parsing a Lava template.", this.Error );
            }
        }
    }

    /// <summary>
    /// Contains details of an event that is fired when the Lava Engine encounters a processing exception.
    /// </summary>
    public class LavaEngineExceptionEventArgs : EventArgs
    {
        public LavaException Exception;
    }

    #endregion
}
