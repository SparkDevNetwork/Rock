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

using Rock.Common;

namespace Rock.Lava
{
    /// <summary>
    /// Stores the configuration and data used by the Lava Engine to resolve a Lava template.
    /// </summary>
    public abstract class LavaRenderContextBase : ILavaRenderContext
    {
        /// <summary>
        /// Gets a named value that is for internal use only, by other components of the Lava engine.
        /// Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        /// <param name="key"></param>
        public abstract object GetInternalField( string key, object defaultValue = null );

        /// <summary>
        /// Gets the collection of variables defined for internal use only.
        /// Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        public abstract LavaDataDictionary GetInternalFields();

        /// <summary>
        /// Sets a named value that is for internal use only, by other components of the Lava engine.
        /// Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public abstract void SetInternalField( string key, object value );

        /// <summary>
        /// Sets a collection of named values for internal use only.
        /// Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        /// <param name="values"></param>
        public void SetInternalFields( ILavaDataDictionary values )
        {
            SetInternalFields( values as IDictionary<string, object> );
        }

        /// <summary>
        /// Sets a collection of named values for internal use only.
        /// Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        /// <param name="fieldValues"></param>
        public virtual void SetInternalFields( IDictionary<string, object> fieldValues )
        {
            if ( fieldValues == null )
            {
                return;
            }

            foreach ( var kvp in fieldValues )
            {
                SetInternalField( kvp.Key, kvp.Value );
            }
        }

        /// <summary>
        /// Sets a collection of named values for internal use only.
        /// Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        /// <param name="values"></param>
        public void SetInternalFields( LavaDataDictionary values )
        {
            SetInternalFields( values as IDictionary<string, object> );
        }

        /// <summary>
        /// Gets the value of a field that is accessible for merging into a template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public abstract object GetMergeField( string key, object defaultValue = null );

        /// <summary>
        /// Gets the user-defined variables in the current context that are accessible in a template.
        /// </summary>
        public abstract LavaDataDictionary GetMergeFields();

        /// <summary>
        /// Sets a named value that is available to be resolved in the Lava Template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="scope"></param>
        public abstract void SetMergeField( string key, object value, LavaContextRelativeScopeSpecifier scope = LavaContextRelativeScopeSpecifier.Current );

        /// <summary>
        /// Sets the user-defined variables in the current context that are internally available to custom filters and tags.
        /// </summary>
        /// <param name="fieldValues"></param>
        public void SetMergeFields( ILavaDataDictionary fieldValues )
        {
            if ( fieldValues == null )
            {
                return;
            }

            foreach ( var key in fieldValues.AvailableKeys )
            {
                SetMergeField( key, fieldValues.GetValue( key ) );
            }
        }

        /// <summary>
        /// Sets the user-defined variables in the current context that are internally available to custom filters and tags.
        /// </summary>
        /// <param name="fieldValues"></param>
        public virtual void SetMergeFields( IDictionary<string, object> fieldValues )
        {
            if ( fieldValues == null )
            {
                return;
            }

            foreach ( var kvp in fieldValues )
            {
                SetMergeField( kvp.Key, kvp.Value );
            }
        }

        /// <summary>
        /// Sets the user-defined variables in the current context that are internally available to custom filters and tags.
        /// </summary>
        /// <param name="fieldValues"></param>
        /// <remarks>This method overload exists to disambiguate calls using the LavaDataDictionary parameter.</remarks>
        public void SetMergeFields( LavaDataDictionary fieldValues )
        {
            SetMergeFields( (ILavaDataDictionary)fieldValues );
        }

        /// <summary>
        /// Get or set the value of a field that is accessible for merging into a template.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key]
        {
            get
            {
                return GetMergeField( key, null );
            }
            set
            {
                SetMergeField( key, value );
            }
        }

        /// <summary>
        /// Gets the Lava Commands that are enabled for this context.
        /// </summary>
        public abstract List<string> GetEnabledCommands();

        /// <summary>
        /// Sets the Lava commands enabled for this template.
        /// </summary>
        /// <param name="commands"></param>
        public abstract void SetEnabledCommands( IEnumerable<string> commands );

        /// <summary>
        /// Sets the Lava commands enabled for this template.
        /// </summary>
        /// <param name="commandList">A delimited list of command names.</param>
        /// <param name="delimiter">The list delimiter.</param>
        public void SetEnabledCommands( string commandList, string delimiter = "," )
        {
            var commands = commandList.SplitDelimitedValues( delimiter );

            SetEnabledCommands( commands );
        }

        /// <summary>
        /// Executes the specified action in a new child scope.
        /// </summary>
        /// <param name="callback"></param>
        public void ExecuteInChildScope( Action<ILavaRenderContext> callback )
        {
            EnterChildScope();
            try
            {
                callback( this );
            }
            finally
            {
                ExitChildScope();
            }
        }

        /// <summary>
        /// Creates a new child scope. Values added to the child scope will be released once <see cref="ExitChildScope" /> is called.
        /// Values in the parent scope remain available to the child scope.
        /// </summary>
        public abstract void EnterChildScope();

        /// <summary>
        /// Exits the current scope that has been created by <see cref="EnterChildScope" />.
        /// </summary>
        public abstract void ExitChildScope();
    }
}