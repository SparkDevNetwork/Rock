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
using Fluid;
using Fluid.Values;

namespace Rock.Lava.Fluid
{
    /// <summary>
    /// An implementation of a Lava Template Context for the Fluid framework.
    /// </summary>
    public class FluidRenderContext : LavaRenderContextBase
    {
        private TemplateContext _context;

        #region Constructors

        public FluidRenderContext( TemplateContext context )
        {
            _context = context;

            // By default, built-in keywords are case-sensitive.
            // For ease of use, add these upper/lower case entries.
            _context.SetValue( "Blank", BlankValue.Instance );
            _context.SetValue( "blank", BlankValue.Instance );
            _context.SetValue( "Empty", EmptyValue.Instance );
            _context.SetValue( "empty", EmptyValue.Instance );
        }

        #endregion

        #region Properties

        public TemplateContext FluidContext
        {
            get
            {
                return _context;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets a named value that is for internal use only. Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        /// <param name="key"></param>
        public override object GetInternalField( string key, object defaultValue = null )
        {
            // In the Fluid framework, internal values are stored in the AmbientValues collection.
            object value;

            var exists = _context.AmbientValues.TryGetValue( key, out value );

            if ( exists )
            {
                return value;
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

            foreach ( var item in _context.AmbientValues )
            {
                values.AddOrReplace( item.Key, item.Value );
            }

            return values;
        }

        /// <summary>
        /// Sets a named value that is for internal use only. Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public override void SetInternalField( string key, object value )
        {
            // In the Fluid framework, internal values are stored in the AmbientValues collection.
            _context.AmbientValues[key] = value;
        }

        /// <summary>
        /// Get the value of a field that is accessible for merging into a template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public override object GetMergeField( string key, object defaultValue )
        {
            var value = _context.GetValue( key );

            if ( value == null )
            {
                return defaultValue;
            }

            return value.ToRealObjectValue();
        }

        /// <summary>
        /// Gets the dictionary of values that are active in the local scope.
        /// Values are defined by the outermost container first, and overridden by values defined in a contained scope.
        /// </summary>
        /// <returns></returns>
        public override LavaDataDictionary GetMergeFields()
        {
            var localScope = _contextScopeInternalField.GetValue( _context ) as Scope;

            var dictionary = new LavaDataDictionary( GetScopeAggregatedValues( localScope ) );

            // Remove fields that were added for internal use.
            dictionary.Remove( "Blank" );
            dictionary.Remove( "blank" );
            dictionary.Remove( "Empty" );
            dictionary.Remove( "empty" );

            return dictionary;
        }

        /// <summary>
        /// Set a merge field value within the specified scope.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="scopeReference">root|parent|current</param>
        public override void SetMergeField( string key, object value, LavaContextRelativeScopeSpecifier scope = LavaContextRelativeScopeSpecifier.Current )
        {
            var localScope = _contextScopeInternalField.GetValue( _context ) as Scope;

            if ( scope == LavaContextRelativeScopeSpecifier.Current )
            {
                _context.SetValue( key, value );
            }
            else if ( scope == LavaContextRelativeScopeSpecifier.Parent )
            {
                var parentScope = localScope.Parent ?? localScope;

                parentScope.SetValue( key, FluidValue.Create( value, _context.Options ) );
            }
            else if ( scope == LavaContextRelativeScopeSpecifier.Root )
            {
                var parentScope = _contextScopeInternalField.GetValue( _context ) as Scope;
                var outerScope = parentScope.Parent;

                while ( outerScope != null )
                {
                    parentScope = outerScope;
                    outerScope = outerScope.Parent;
                }

                parentScope.SetValue( key, FluidValue.Create( value, _context.Options ) );
            }
            else
            {
                throw new LavaException( $"SetMergeFieldValue failed. Scope reference \"{ scope }\" is invalid." );
            }
        }

        /// <summary>
        /// Gets the Lava Commands that are enabled for this context.
        /// </summary>
        public override List<string> GetEnabledCommands()
        {
            // The set of enabled Lava Commands is stored in the Fluid AmbientValues collection.
            if ( _context.AmbientValues?.ContainsKey( "EnabledCommands" ) == true )
            {
                return _context.AmbientValues["EnabledCommands"].ToString().Split( ",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries ).ToList();
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
                _context.AmbientValues["EnabledCommands"] = string.Empty;
            }
            else
            {
                _context.AmbientValues["EnabledCommands"] = commands.JoinStrings( "," );
            }
        }

        /// <summary>
        /// Creates a new child scope. Values added to the child scope will be released once <see cref="ExitChildScope" /> is called.
        /// Values in the parent scope remain available to the child scope.
        /// </summary>
        public override void EnterChildScope()
        {
            _context.EnterChildScope();
        }

        /// <summary>
        /// Exits the current scope that has been created by <see cref="EnterChildScope" />.
        /// </summary>
        public override void ExitChildScope()
        {
            _context.ReleaseScope();
        }

        #endregion

        private static PropertyInfo _contextScopeInternalField = typeof( TemplateContext ).GetProperty( "LocalScope", BindingFlags.NonPublic | BindingFlags.Instance );

        /// <summary>
        /// Gets an aggregated set of key/value pairs for variables in the current scope and outer scopes.
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        private Dictionary<string, object> GetScopeAggregatedValues( Scope scope )
        {
            var dictionary = new Dictionary<string, object>();

            while ( scope != null )
            {
                var properties = GetScopeDefinedValues( scope );

                foreach ( var key in properties.Keys )
                {
                    dictionary.AddOrIgnore( key, properties[key] );
                }

                scope = scope.Parent;
            }

            return dictionary;
        }

        /// <summary>
        /// Gets an aggregated set of key/value pairs for variables defined in the current scope.
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        private Dictionary<string, object> GetScopeDefinedValues( Scope scope )
        {
            var dictionary = new Dictionary<string, object>();

            foreach ( var key in scope.Properties )
            {
                dictionary.AddOrReplace( key, scope.GetValue( key ).ToRealObjectValue() );
            }

            return dictionary;
        }
    }
}
