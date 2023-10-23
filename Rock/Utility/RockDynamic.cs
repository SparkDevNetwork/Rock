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
using System.Dynamic;
using System.Linq;
using System.Reflection;

using Rock.Data;
using Rock.Lava;

namespace Rock.Utility
{
    /// <summary>
    /// RockDynamic can be as a base class for C# POCO objects that need to be available to Lava.
    /// It can also be used to create a Lava Proxy for C# objects that can't inherit from RockDynamic.
    /// </summary>
    /// <seealso cref="System.Dynamic.DynamicObject" />
    /// <seealso cref="Rock.Lava.ILavaDataDictionary" />
    public class RockDynamic : DynamicObject, Lava.ILiquidizable, ILavaDataDictionary
    {
        private Dictionary<string, object> _members = new Dictionary<string, object>();

        private object _instance;

        private Type _instanceType;

        private Dictionary<string, PropertyInfo> InstancePropertyLookup
        {
            get
            {
                if ( _instancePropertyInfoLookup == null && _instance != null )
                {
                    var rockDynamicType = typeof( RockDynamic );
                    _instancePropertyInfoLookup = _instance.GetType().GetProperties().Where( a => a.DeclaringType != rockDynamicType ).ToDictionary( k => k.Name, v => v );
                }

                return _instancePropertyInfoLookup;
            }
        }

        private Dictionary<string, PropertyInfo> _instancePropertyInfoLookup;

        #region Properties

        /// <summary>
        /// Gets the instance of the object that is rapped in RockDynamic.
        /// </summary>
        public object Instance { get { return _instance; } }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="RockDynamic"/> class.
        /// </summary>
        public RockDynamic()
        {
            _instance = this;
            _instanceType = GetType();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockDynamic"/> class as a proxy that makes the object available to lava
        /// </summary>
        /// <param name="obj">The object.</param>
        public RockDynamic( object obj )
        {
            _instance = obj;
            _instanceType = obj.GetType();
        }

        /// <summary>
        /// Provides the implementation for operations that get member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject" /> class can override this method to specify dynamic behavior for operations such as getting a value for a property.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member on which the dynamic operation is performed. For example, for the Console.WriteLine(sampleObject.SampleProperty) statement, where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject" /> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
        /// <param name="result">The result of the get operation. For example, if the method is called for a property, you can assign the property value to <paramref name="result" />.</param>
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a run-time exception is thrown.)
        /// </returns>
        public override bool TryGetMember( GetMemberBinder binder, out object result )
        {
            result = null;

            // first check the dictionary for member
            if ( _members.Keys.Contains( binder.Name ) )
            {
                result = _members[binder.Name];
                return true;
            }

            // next check for public properties via Reflection
            try
            {
                return GetProperty( _instance, binder.Name, out result );
            }
            catch
            {
            }

            // failed to retrieve a property
            result = null;
            return false;
        }

        /// <summary>
        /// Provides the implementation for operations that set member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject" /> class can override this method to specify dynamic behavior for operations such as setting a value for a property.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member to which the value is being assigned. For example, for the statement sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject" /> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
        /// <param name="value">The value to set to the member. For example, for sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject" /> class, the <paramref name="value" /> is "Test".</param>
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)
        /// </returns>
        public override bool TrySetMember( SetMemberBinder binder, object value )
        {
            // first check to see if there's a native property to set
            if ( _instance != null )
            {
                try
                {
                    bool result = SetProperty( this, binder.Name, value );
                    if ( result )
                    {
                        return true;
                    }
                }
                catch
                {
                }
            }

            _members[binder.Name] = value;
            return true;
        }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        protected bool GetProperty( object instance, string name, out object result )
        {
            if ( instance == null )
            {
                instance = this;
            }

            var prop = InstancePropertyLookup.GetValueOrNull( name );
            if ( prop != null )
            {
                result = prop.GetValue( instance, null );
                return true;
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Sets the property.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected bool SetProperty( object instance, string name, object value )
        {
            if ( instance == null )
            {
                instance = this;
            }

            if ( name == null )
            {
                return false;
            }

            var prop = InstancePropertyLookup.GetValueOrNull( name );
            if ( prop != null )
            {
                prop.SetValue( _instance, value, null );
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public object this[string key]
        {
            get
            {
                // try to get from properties collection first
                if ( _members.ContainsKey( key ) )
                {
                    return _members[key];
                }

                // try reflection on instanceType
                object result = null;
                if ( GetProperty( _instance, key, out result ) )
                {
                    return result;
                }

                // nope doesn't exist
                return null;
            }

            set
            {
                if ( key != null )
                {
                    if ( _members.ContainsKey( key ) )
                    {
                        _members[key] = value;
                        return;
                    }

                    // check instance for existence of type first
                    var prop = InstancePropertyLookup.GetValueOrNull( key );
                    if ( prop != null )
                    {
                        SetProperty( _instance, key, value );
                    }
                    else
                    {
                        _members[key] = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <param name="includeInstanceProperties">if set to <c>true</c> [include instance properties].</param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, object>> GetProperties( bool includeInstanceProperties = false )
        {
            if ( includeInstanceProperties && _instance != null )
            {
                foreach ( var prop in this.InstancePropertyLookup.Values )
                {
                    yield return new KeyValuePair<string, object>( prop.Name, prop.GetValue( _instance, null ) );
                }
            }

            foreach ( var key in this._members.Keys )
            {
                yield return new KeyValuePair<string, object>( key, this._members[key] );
            }
        }

        /// <summary>
        /// Returns the enumeration of all dynamic member names.
        /// </summary>
        /// <returns>
        /// A sequence that contains dynamic member names.
        /// </returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            List<string> propertyNames = new List<string>();

            foreach ( var propName in this.InstancePropertyLookup.Keys )
            {
                propertyNames.Add( propName );
            }

            foreach ( var key in this._members.Keys )
            {
                propertyNames.Add( key );
            }

            return propertyNames;
        }

        /// <summary>
        /// Determines whether this object contains the specified key and value.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="includeInstanceProperties">if set to <c>true</c> [include instance properties].</param>
        /// <returns></returns>
        public bool Contains( KeyValuePair<string, object> item, bool includeInstanceProperties = false )
        {
            bool res = _members.ContainsKey( item.Key );
            if ( res )
            {
                return _members[item.Key].Equals( item.Value );
            }

            if ( includeInstanceProperties && _instance != null )
            {
                return InstancePropertyLookup.ContainsKey( item.Key );
            }

            return false;
        }

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public object this[object key]
        {
            get
            {
                var propertyKey = key.ToStringSafe();
                return this[propertyKey];
            }
        }

        #region ILavaDataDictionary Implementation

        /// <summary>
        /// Gets the available keys (for debugging info).
        /// </summary>
        /// <value>
        /// The available keys.
        /// </value>
        [LavaHidden]
        public List<string> AvailableKeys
        {
            get
            {
                return GetDynamicMemberNames().ToList();
            }
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetValue( string key )
        {
            return this[key];
        }

        /// <summary>
        /// Determines whether this object holds a value with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool ContainsKey( string key )
        {
            return this.GetDynamicMemberNames().Contains( key );
        }

        #endregion

        #region ILiquidizable

        /// <summary>
        /// Determines whether this object holds a value with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool ContainsKey( object key )
        {
            return this.GetDynamicMemberNames().Contains( key );
        }

        /// <summary>
        /// Returns liquid for the object
        /// </summary>
        /// <returns></returns>
        public object ToLiquid()
        {
            return this;
        }

        #endregion
    }
}
