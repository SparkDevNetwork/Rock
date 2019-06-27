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

namespace Rock.Utility
{
    /// <summary>
    /// RockDynamic can be as a base class for C# POCO objects that need to be available to Lava.
    /// It can also be used to create a Lava Proxy for C# objects that can't inherit from RockDynamic.
    /// </summary>
    /// <seealso cref="System.Dynamic.DynamicObject" />
    /// <seealso cref="Rock.Lava.ILiquidizable" />
    public class RockDynamic : DynamicObject, Lava.ILiquidizable
    {
        private Dictionary<string, object> _members = new Dictionary<string, object>();

        private object _instance;

        private Type _instanceType;

        private PropertyInfo[] InstancePropertyInfo
        {
            get
            {
                if ( _instancePropertyInfo == null && _instance != null )
                {
                    var rockDynamicType = typeof( RockDynamic );
                    _instancePropertyInfo = _instance.GetType().GetProperties().Where( a => a.DeclaringType != rockDynamicType ).ToArray();
                }

                return _instancePropertyInfo;
            }
        }

        private PropertyInfo[] _instancePropertyInfo;

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

            var miArray = _instanceType.GetMember( name, BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance );
            if ( miArray != null && miArray.Length > 0 )
            {
                var mi = miArray[0];
                if ( mi.MemberType == MemberTypes.Property )
                {
                    result = ( ( PropertyInfo ) mi ).GetValue( instance, null );
                    return true;
                }
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

            var miArray = _instanceType.GetMember( name, BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance );
            if ( miArray != null && miArray.Length > 0 )
            {
                var mi = miArray[0];
                if ( mi.MemberType == MemberTypes.Property )
                {
                    ( ( PropertyInfo ) mi ).SetValue( _instance, value, null );
                    return true;
                }
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
                try
                {
                    // try to get from properties collection first
                    return _members[key];
                }
                catch ( KeyNotFoundException )
                {
                    // try reflection on instanceType
                    object result = null;
                    if ( GetProperty( _instance, key, out result ) )
                    {
                        return result;
                    }

                    // nope doesn't exist
                    return null;
                }
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
                    var miArray = _instanceType.GetMember( key, BindingFlags.Public | BindingFlags.GetProperty );
                    if ( miArray != null && miArray.Length > 0 )
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
                foreach ( var prop in this.InstancePropertyInfo )
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

            foreach ( var prop in this.InstancePropertyInfo )
            {
                propertyNames.Add( prop.Name );
            }

            foreach ( var key in this._members.Keys )
            {
                propertyNames.Add( key );
            }

            return propertyNames;
        }

        #region ILiquid Implementation
        /// <summary>
        /// Gets the available keys (for debugging info).
        /// </summary>
        /// <value>
        /// The available keys.
        /// </value>
        [LavaIgnore]
        public List<string> AvailableKeys
        {
            get
            {
                return GetDynamicMemberNames().ToList();
            }
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

        /// <summary>
        /// Determines whether [contains] [the specified item].
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
                foreach ( var prop in this.InstancePropertyInfo )
                {
                    if ( prop.Name == item.Key )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Returns liquid for the object
        /// </summary>
        /// <returns></returns>
        public object ToLiquid()
        {
            return this;
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool ContainsKey( object key )
        {
            return this.GetDynamicMemberNames().Contains( key.ToString() );
        }

        #endregion
    }
}
