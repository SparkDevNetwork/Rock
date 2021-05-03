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

namespace Rock.Lava
{
    /// <summary>
    /// Represents the Lava Engine that is responsible for compiling and rendering templates.
    /// </summary>
    public interface ILavaEngine
    {
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
        /// The descriptive name of the templating framework on which Lava is currently operating.
        /// </summary>
        string EngineName { get; }

        /// <summary>
        /// The Liquid template framework used to parse and render Lava templates.
        /// </summary>
        LavaEngineTypeSpecifier EngineType { get; }

        /// <summary>
        /// Creates a new render context instance containing the specified merge values.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="enabledCommands"></param>
        /// <returns></returns>
        ILavaRenderContext NewRenderContext();

        /// <summary>
        /// Creates a new render context instance with the specified Lava commands enabled.
        /// </summary>
        /// <param name="enabledCommands"></param>
        /// <returns></returns>
        ILavaRenderContext NewRenderContext( IEnumerable<string> enabledCommands );

        /// <summary>
        /// Creates a new render context instance containing the specified merge values.
        /// </summary>
        /// <param name="mergeFields"></param>
        /// <param name="enabledCommands"></param>
        /// <returns></returns>
        ILavaRenderContext NewRenderContext( IDictionary<string, object> mergeFields, IEnumerable<string> enabledCommands = null );

        /// <summary>
        /// Creates a new render context instance containing the specified merge values.
        /// </summary>
        /// <param name="mergeFields"></param>
        /// <param name="enabledCommands"></param>
        /// <returns></returns>
        ILavaRenderContext NewRenderContext( ILavaDataDictionary mergeFields, IEnumerable<string> enabledCommands = null );

        /// <summary>
        /// Creates a new render context instance containing the specified merge values.
        /// </summary>
        /// <param name="mergeFields"></param>
        /// <param name="enabledCommands"></param>
        /// <returns></returns>
        /// <remarks>This method overload exists to disambiguate calls using the LavaDataDictionary parameter.</remarks>
        ILavaRenderContext NewRenderContext( LavaDataDictionary mergeFields, IEnumerable<string> enabledCommands = null );

        /// <summary>
        /// Register one or more filter functions that are implemented by the supplied Type.
        /// A filter must be defined as a public static function that returns a string.
        /// </summary>
        /// <param name="implementingType"></param>
        void RegisterFilters( Type implementingType );

        /// <summary>
        /// Register a Lava Tag elemennt.
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
        /// Registers a shortcode with a factory method that provides the definition of the shortcode on demand.
        /// A dynamic shortcode is defined in the active Rock database, and its associated template can be modified at runtime.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factoryMethod"></param>
        void RegisterDynamicShortcode( string name, Func<string, DynamicShortcodeDefinition> factoryMethod );

        /// <summary>
        /// Registers a static shortcode with a factory method that provides the shortcode definition.
        /// A static shortcode is defined by a code component and its associated template cannot be modified at runtime.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factoryMethod"></param>
        void RegisterStaticShortcode( string name, Func<string, ILavaShortcode> factoryMethod );

        /// <summary>
        /// Deregister a shortcode.
        /// </summary>
        /// <param name="name"></param>
        void UnregisterShortcode( string name );

        /// <summary>
        /// Gets the collection of all registered Lava document elements.
        /// </summary>
        /// <returns></returns>
        Dictionary<string, ILavaElementInfo> GetRegisteredElements();

        /// <summary>
        /// Render the provided template.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns>
        /// The rendered output of the template.
        /// If the template is invalid, returns an error message or an empty string according to the current ExceptionHandlingStrategy setting.
        /// </returns>
        string RenderTemplate( string inputTemplate );

        /// <summary>
        /// Render the provided template in the specified context.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="context"></param>
        /// <returns>
        /// The rendered output of the template.
        /// If the template is invalid, returns an error message or an empty string according to the current ExceptionHandlingStrategy setting.
        /// </returns>
        string RenderTemplate( string inputTemplate, ILavaRenderContext context );

        /// <summary>
        /// Render the provided template in a new context with the specified merge fields.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="mergeFields">The collection of merge fields to be added to the context used to render the template.</param>
        /// <returns>
        /// The rendered output of the template.
        /// If the template is invalid, returns an error message or an empty string according to the current ExceptionHandlingStrategy setting.
        /// </returns>
        string RenderTemplate( string inputTemplate, ILavaDataDictionary mergeFields );

        /// <summary>
        /// Try to render the provided template.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        bool TryRenderTemplate( string inputTemplate, out string output, out List<Exception> errors );

        /// <summary>
        /// Try to render the provided template with the specified merge fields.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="mergeFields"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        bool TryRenderTemplate( string inputTemplate, ILavaDataDictionary mergeFields, out string output, out List<Exception> errors );

        /// <summary>
        /// Try to render the provided template in the specified context.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="context"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        bool TryRenderTemplate( string inputTemplate, ILavaRenderContext context, out string output, out List<Exception> errors );

        /// <summary>
        /// Try to render a compiled Lava template using the specified parameters.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="parameters"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        bool TryRenderTemplate( ILavaTemplate inputTemplate, LavaRenderParameters parameters, out string output, out List<Exception> errors );

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
        /// Try to parse the provided template.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        bool TryParseTemplate( string inputTemplate, out ILavaTemplate template );

        /// <summary>
        /// Parse the provided template.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <returns>A compiled template object.</returns>
        ILavaTemplate ParseTemplate( string inputTemplate );

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

    public enum LavaEngineTypeSpecifier
    {
        // A fork of the DotLiquid framework, customised to include Lava syntax and features.
        RockLiquid = 1,
        // DotLiquid is an open-source implementation of the Liquid templating language. [https://github.com/dotliquid/dotliquid]
        DotLiquid = 2,
        // Fluid is an open-source implementation of the Liquid templating language. [https://github.com/sebastienros/fluid]
        Fluid = 3,
    }

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
}
