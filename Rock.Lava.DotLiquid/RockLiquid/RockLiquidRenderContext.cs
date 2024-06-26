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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotLiquid;

namespace Rock.Lava.DotLiquid
{
    /// <summary>
    /// An implementation of a Lava Context that wraps a DotLiquid framework context.
    /// </summary>
    public class RockLiquidRenderContext : LavaRenderContextBase
    {
        private Context _context;

        #region Constructors

        /// <summary>
        /// Create a new instance for the provided context.
        /// </summary>
        /// <param name="context"></param>
        public RockLiquidRenderContext( Context context )
        {
            _context = context;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the DotLiquid context wrapped by this Lava Context.
        /// </summary>
        public Context DotLiquidContext
        {
            get
            {
                return _context;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets a named value that is for internal use only, by other components of the Lava engine.
        /// Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        /// <param name="key"></param>
        public override object GetInternalField( string key, object defaultValue = null )
        {
            if ( _context.Registers.ContainsKey( key ) )
            {
                return _context.Registers[key];
            }

            return defaultValue;
        }

        /// <summary>
        /// Gets the collection of variables defined for internal use only.
        /// Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        public override LavaDataDictionary GetInternalFields()
        {
            var values = new LavaDataDictionary();

            foreach ( var item in _context.Registers )
            {
                values.AddOrReplace( item.Key, item.Value );
            }

            return values;
        }

        /// <summary>
        /// Sets a named value that is for internal use only, by other components of the Lava engine.
        /// Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public override void SetInternalField( string key, object value )
        {
            _context.Registers[key] = value;
        }

        /// <summary>
        /// Get the value of a field that is accessible for merging into a template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public override object GetMergeField( string key, object defaultValue = null )
        {
            if ( !_context.HasKey( key ) )
            {
                return defaultValue;
            }

            return UnwrapContextValue( _context[key] );
        }

        private object UnwrapContextValue( object contextValue )
        {
            if ( contextValue is ILiquidFrameworkDataObjectProxy proxy )
            {
                return proxy.GetProxiedDataObject();
            }

            return contextValue;
        }
        /// <summary>
        /// Gets the dictionary of values that are active in the local scope.
        /// Values are defined by the outermost container first, and overridden by values defined in a contained scope.
        /// </summary>
        /// <returns></returns>
        public override LavaDataDictionary GetMergeFields()
        {
            var fields = new LavaDataDictionary();

            // First, get all of the variables defined in the local lava context.
            // In DotLiquid, the innermost scope is the first element in the collection.
            foreach ( var scope in _context.Scopes )
            {
                foreach ( var item in scope )
                {
                    fields.TryAdd( item.Key, item.Value );
                }
            }

            // Second, add any variables defined by the block or container in which the template is being resolved.
            foreach ( var environment in _context.Environments )
            {
                foreach ( var item in environment )
                {
                    fields.TryAdd( item.Key, item.Value );
                }
            }

            return fields;
        }

        /// <summary>
        /// Sets a named value that is available to be resolved in the Lava Template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="scope"></param>
        public override void SetMergeField( string key, object value, LavaContextRelativeScopeSpecifier scope = LavaContextRelativeScopeSpecifier.Current )
        {
            int scopeIndex;

            // DotLiquid Scopes are ordered with the current level first.
            if ( scope == LavaContextRelativeScopeSpecifier.Parent && _context.Scopes.Count > 1 )
            {
                scopeIndex = _context.Scopes.Count - 1;
            }
            else
            {
                scopeIndex = 0;
            }

            var fieldValue = GetDotLiquidCompatibleValue( value );

            // Set the variable in the specified scope.
            _context.Scopes[scopeIndex][key] = fieldValue;
        }

        /// <summary>
        /// Gets the Lava Commands that are enabled for this context.
        /// </summary>
        public override List<string> GetEnabledCommands()
        {
            // The set of enabled Lava Commands is stored in the DotLiquid Registers collection.
            if ( _context.Registers?.ContainsKey( "EnabledCommands" ) == true )
            {
                return _context.Registers["EnabledCommands"].ToString().Split( ",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries ).ToList();
            }

            return new List<string>();
        }

        /// <summary>
        /// Sets the Lava commands enabled for this template.
        /// </summary>
        /// <param name="commands"></param>
        public override void SetEnabledCommands( IEnumerable<string> commands )
        {
            if ( commands == null )
            {
                _context.Registers["EnabledCommands"] = string.Empty;
            }
            else
            {
                _context.Registers["EnabledCommands"] = commands.JoinStrings( "," );
            }
        }

        /// <summary>
        /// Creates a new child scope. Values added to the child scope will be released once <see cref="ExitChildScope" /> is called.
        /// Values in the parent scope remain available to the child scope.
        /// </summary>
        public override void EnterChildScope()
        {
            // Push a new scope onto the stack.
            var newScope = new Hash();

            _context.Push( newScope );
        }

        /// <summary>
        /// Exits the current scope that has been created by <see cref="EnterChildScope" />.
        /// </summary>
        public override void ExitChildScope()
        {
            _context.Pop();
        }

        #endregion

        private object GetDotLiquidCompatibleValue( object value )
        {
            // Primitive values do not require any special processing.
            if ( value == null
                 || value is string
                 || value is IEnumerable
                 || value is decimal
                 || value is DateTime
                 || value is DateTimeOffset
                 || value is TimeSpan
                 || value is Guid
                 || value is Enum
                 || value is KeyValuePair<string, object>
                 )
            {
                return value;
            }

            var valueType = value.GetType();

            if ( valueType.IsPrimitive )
            {
                return value;
            }

            // For complex types, check if a specific transformer has been defined for the type.
            var safeTypeTransformer = Template.GetSafeTypeTransformer( valueType );

            if ( safeTypeTransformer != null )
            {
                return safeTypeTransformer( value );
            }

            if ( value is ILiquidizable )
            {
                return value;
            }

            // Check if the type is decorated with the LavaType attribute.
            var lavaInfo = LavaDataObjectHelper.GetLavaTypeInfo( valueType );

            return new DropProxy( value, lavaInfo.VisiblePropertyNames.ToArray() );
        }
    }
}