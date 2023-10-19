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
    /// Specifies the scope of a variable relative to the current Lava context.
    /// </summary>
    public enum LavaContextRelativeScopeSpecifier
    {
        Current = 0,
        Parent = 1,

        [Obsolete("The behavior of this setting is equivalent to Current. (v16.1).")]
        Root = 2
    }

    /// <summary>
    /// Represents the configuration and data used by the Lava Engine to resolve a Lava template.
    /// </summary>
    public interface ILavaRenderContext
    {
        /// <summary>
        /// Get an instance of a Lava service component of the specified type.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        TService GetService<TService>()
            where TService : class, ILavaService;

        /// <summary>
        /// Get an instance of a Lava service component of the specified type.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        ILavaService GetService( Type serviceType );

        /// <summary>
        /// Gets a named value that is for internal use only, by other components of the Lava engine.
        /// Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        /// <param name="key"></param>
        object GetInternalField( string key, object defaultValue = null );

        /// <summary>
        /// Gets the collection of variables defined for internal use only.
        /// Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        LavaDataDictionary GetInternalFields();

        /// <summary>
        /// Sets a named value that is for internal use only, by other components of the Lava engine.
        /// Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetInternalField( string key, object value );

        /// <summary>
        /// Sets a collection of named values for internal use only.
        /// Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        /// <param name="fieldValues"></param>
        void SetInternalFields( IDictionary<string, object> fieldValues );

        /// <summary>
        /// Sets a collection of named values for internal use only.
        /// Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        /// <param name="fieldValues"></param>
        void SetInternalFields( ILavaDataDictionary fieldValues );

        /// <summary>
        /// Sets a collection of named values for internal use only.
        /// Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        /// <param name="fieldValues"></param>
        /// <remarks>This method overload exists to disambiguate calls using the LavaDataDictionary parameter.</remarks>
        void SetInternalFields( LavaDataDictionary fieldValues );

        /// <summary>
        /// Gets the value of a field that is accessible for merging into a template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        object GetMergeField( string key, object defaultValue = null );

        /// <summary>
        /// Gets the user-defined variables in the current context that are accessible in a template.
        /// </summary>
        LavaDataDictionary GetMergeFields();

        /// <summary>
        /// Sets the value of a field that is accessible for merging into a template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="scope">Current, Parent, or Root.</param>
        /// <returns></returns>
        void SetMergeField( string key, object value, LavaContextRelativeScopeSpecifier scope = LavaContextRelativeScopeSpecifier.Current );

        /// <summary>
        /// Sets the user-defined variables in the current context that are internally available to custom filters and tags.
        /// </summary>
        /// <param name="fieldValues"></param>
        void SetMergeFields( ILavaDataDictionary fieldValues );

        /// <summary>
        /// Sets the user-defined variables in the current context that are internally available to custom filters and tags.
        /// </summary>
        /// <param name="fieldValues"></param>
        void SetMergeFields( IDictionary<string, object> fieldValues );

        /// <summary>
        /// Sets the user-defined variables in the current context that are internally available to custom filters and tags.
        /// </summary>
        /// <param name="fieldValues"></param>
        /// <remarks>This method overload exists to disambiguate calls using the LavaDataDictionary parameter.</remarks>
        void SetMergeFields( LavaDataDictionary fieldValues );

        /// <summary>
        /// Get or set the value of a field that is accessible for merging into a template.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object this[string key] { get; set; }

        /// <summary>
        /// Gets the Lava Commands that are enabled for this context.
        /// </summary>
        List<string> GetEnabledCommands();

        /// <summary>
        /// Sets the Lava commands enabled for this template.
        /// </summary>
        /// <param name="commands"></param>
        void SetEnabledCommands( IEnumerable<string> commands );

        /// <summary>
        /// Sets the Lava commands enabled for this template.
        /// </summary>
        /// <param name="commandList">A delimited list of command names.</param>
        /// <param name="delimiter">The list delimiter.</param>
        void SetEnabledCommands( string commandList, string delimiter = "," );

        /// <summary>
        /// Executes the specified action in a new child scope.
        /// </summary>
        /// <param name="callback"></param>
        void ExecuteInChildScope( Action<ILavaRenderContext> callback );

        /// <summary>
        /// Creates a new child scope. Values added to the child scope will be released once <see cref="ExitChildScope" /> is called.
        /// Values in the parent scope remain available to the child scope.
        /// </summary>
        void EnterChildScope();

        /// <summary>
        /// Exits the current scope that has been created by <see cref="EnterChildScope" />.
        /// </summary>
        void ExitChildScope();
    }
}
