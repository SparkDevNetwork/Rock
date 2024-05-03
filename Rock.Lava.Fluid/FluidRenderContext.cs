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
        private const string _InternalFieldKeyPrefix = "$_";
        private const string _InternalFieldKeyEnabledCommands = "EnabledCommands";

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
            // Internal values are stored in the current scope with a prefix to identify them as private.
            return GetFieldPrivate( _InternalFieldKeyPrefix + key, defaultValue, allowInternalFieldAccess: true );
        }

        /// <summary>
        /// Gets the collection of variables defined for internal use only.
        /// Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        public override LavaDataDictionary GetInternalFields()
        {
            var values = new LavaDataDictionary();

            var internalKeys = _context.ValueNames.Where( x => x.StartsWith( _InternalFieldKeyPrefix ) );
            foreach ( var key in internalKeys )
            {
                values.AddOrReplace( key, _context.GetValue( key ) );
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
            // Internal values are stored in the current scope with a prefix to identify them as private.
            if ( !key.StartsWith( _InternalFieldKeyPrefix ) )
            {
                key = _InternalFieldKeyPrefix + key;
            }
            SetFieldPrivate( key, value, allowInternalFieldAccess: true );
        }

        /// <summary>
        /// Get the value of a field that is accessible for merging into a template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public override object GetMergeField( string key, object defaultValue )
        {
            return GetFieldPrivate( key, defaultValue, allowInternalFieldAccess: false );
        }

        /// <summary>
        /// Gets the dictionary of values that are active in the local scope.
        /// Values are defined by the outermost container first, and overridden by values defined in a contained scope.
        /// </summary>
        /// <returns></returns>
        public override LavaDataDictionary GetMergeFields()
        {
            var localScope = _contextScopeInternalField.GetValue( _context ) as Scope;

            var dictionary = new LavaDataDictionary( GetScopeAggregatedValues( localScope, includeInternalFields: false ) );

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
            SetFieldPrivate( key, value, allowInternalFieldAccess: false, scope );
        }

        /// <summary>
        /// Gets the Lava Commands that are enabled for templates resolved in the current scope.
        /// This setting is also effective for any child scopes that do not explicitly redefine it.
        /// </summary>
        public override List<string> GetEnabledCommands()
        {
            // The set of enabled Lava Commands is stored in the current scope.

            var enabledCommands = GetInternalField( _InternalFieldKeyEnabledCommands );

            if ( enabledCommands != null )
            {
                return enabledCommands.ToString().Split( ",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries ).ToList();
            }

            return new List<string>();
        }

        /// <summary>
        /// Sets the Lava Commands that are enabled for templates resolved in the current scope.
        /// This setting is also effective for any child scopes that do not explicitly redefine it.
        /// </summary>
        /// <param name="commands"></param>
        public override void SetEnabledCommands( IEnumerable<string> commands )
        {
            if ( commands == null )
            {
                SetInternalField( _InternalFieldKeyEnabledCommands, string.Empty );
            }
            else
            {
                SetInternalField( _InternalFieldKeyEnabledCommands, commands.JoinStrings( "," ) );
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
        /// Get a field value in the current scope.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="allowInternalFieldAccess"></param>
        /// <returns></returns>
        private object GetFieldPrivate( string key, object defaultValue, bool allowInternalFieldAccess )
        {
            if ( !allowInternalFieldAccess && key.StartsWith( _InternalFieldKeyPrefix ) )
            {
                throw new Exception( "GetMergeField failed. Invalid key." );
            }

            var value = _context.GetValue( key );

            if ( value == null )
            {
                return defaultValue;
            }

            var rawValue = value.ToRealObjectValue();
            // If the value is wrapped to prevent Fluid from changing the type, unwrap it and return the original value.
            if ( rawValue is FluidRawValueProxy wrapper )
            {
                rawValue = wrapper.Value;
            }

            return rawValue;
        }

        /// <summary>
        /// Set a merge field value within the specified scope.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="scopeReference">root|parent|current</param>
        private void SetFieldPrivate( string key, object value, bool allowInternalFieldAccess, LavaContextRelativeScopeSpecifier scope = LavaContextRelativeScopeSpecifier.Current )
        {
            var isInternalField = key.StartsWith( _InternalFieldKeyPrefix );
            if ( isInternalField && !allowInternalFieldAccess )
            {
                // Prevent merge fields from using the internal field key prefix.
                throw new Exception( "SetFieldValue failed. The key contains invalid data." );
            }

            var localScope = _contextScopeInternalField.GetValue( _context ) as Scope;

            // When adding a collection to the context, Fluid reprocesses the collection to ensure the elements can be referenced
            // from a template, and the original type information of the collection may be lost.
            // This is problematic for internal fields that are not intended to be visible to templates, so 
            // we wrap the object in a proxy to prevent this behavior.
            if ( isInternalField && value is IEnumerable && !( value is string ) )
            {
                value = new FluidRawValueProxy( value );
            }

            if ( scope == LavaContextRelativeScopeSpecifier.Parent )
            {
                var parentScope = localScope.Parent ?? localScope;

                parentScope.SetValue( key, FluidValue.Create( value, _context.Options ) );
            }
            else
            {
                _context.SetValue( key, value );
            }
        }

        /// <summary>
        /// Gets an aggregated set of key/value pairs for variables in the current scope and outer scopes.
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        private Dictionary<string, object> GetScopeAggregatedValues( Scope scope, bool includeInternalFields )
        {
            var dictionary = new Dictionary<string, object>();

            while ( scope != null )
            {
                var properties = GetScopeDefinedValues( scope, includeInternalFields );

                foreach ( var key in properties.Keys )
                {
                    dictionary.TryAdd( key, properties[key] );
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
        private Dictionary<string, object> GetScopeDefinedValues( Scope scope, bool includeInternalFields )
        {
            var dictionary = new Dictionary<string, object>();

            var properties = scope.Properties;
            if ( !includeInternalFields )
            {
                properties = properties.Where( x => !x.StartsWith( _InternalFieldKeyPrefix ) );
            }
            foreach ( var key in properties )
            {
                dictionary.AddOrReplace( key, scope.GetValue( key ).ToRealObjectValue() );
            }

            return dictionary;
        }

        #region Helper Classes

        /// <summary>
        /// An internal proxy for a value that should be stored in the Fluid Context without any modification.
        /// Some unproxied value types are stored in a modified form by the Fluid framework to ensure they
        /// can be more easily referenced from a template. This can cause unwanted results when storing and retrieving
        /// values that are intended for internal use only.
        /// </summary>
        internal class FluidRawValueProxy
        {
            public FluidRawValueProxy( object value )
            {
                Value = value;
            }

            public object Value { get; private set; }
        }

        #endregion
    }
}
