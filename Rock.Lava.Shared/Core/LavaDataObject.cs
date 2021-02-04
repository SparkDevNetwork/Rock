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

namespace Rock.Lava
{
    /// <summary>
    /// LavaDataObject can be as a base class for C# classes that need to be available to Lava.
    /// It can also be used to create a Lava Proxy for C# objects that cannot directly inherit from this class.
    /// </summary>
    /// <seealso cref="System.Dynamic.DynamicObject" />
    public class LavaDataObject : DynamicObject, ILavaDataDictionary, IDictionary<string, object>
    {
        private Dictionary<string, object> _members = new Dictionary<string, object>();

        private object _instance;

        private Dictionary<string, PropertyInfo> InstancePropertyLookup
        {
            get
            {
                if ( _instancePropertyInfoLookup == null && _instance != null )
                {
                    var rockDynamicType = typeof( LavaDataObject );
                    _instancePropertyInfoLookup = _instance.GetType().GetProperties().Where( a => a.DeclaringType != rockDynamicType ).ToDictionary( k => k.Name, v => v );
                }

                return _instancePropertyInfoLookup;
            }
        }

        private Dictionary<string, PropertyInfo> _instancePropertyInfoLookup;

        /// <summary>
        /// Initializes a new instance of the <see cref="LavaDataObject"/> class.
        /// </summary>
        public LavaDataObject()
        {
            _instance = this;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LavaDataObject"/> class as a proxy that makes the object available to lava
        /// </summary>
        /// <param name="obj">The object.</param>
        public LavaDataObject( object obj )
        {
            _instance = obj;
        }

        /// <summary>
        /// Return a string representation of the dynamic object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // If we are wrapping an object instance, return the ToString() for the object,
            // otherwise return the first value in the property dictionary.
            if ( _instance != null )
            {
                if ( _instance == this )
                {
                    return null;
                }
                return _instance.ToString();
            }

            if ( _members != null )
            {
                var firstKey = _members.Keys.FirstOrDefault();

                if ( firstKey != null )
                {
                    var firstValue = _members[firstKey] ?? string.Empty;

                    return firstValue.ToString();
                }
            }

            return base.ToString();
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
            return TryGetMember( binder.Name, out result );
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
                    bool result = SetProperty( binder.Name, value );
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
        /// Gets the property Value of the object's property as specified by propertyPathName.
        /// If the object is a dictionary, retrieves the value associated with the matching key.
        /// </summary>
        /// <param name="rootObj">The root obj.</param>
        /// <param name="propertyPathName">The named path to the property, which may include references to nested properties in a dot-separated list.</param>
        /// <returns></returns>
        protected bool GetProperty( object rootObj, string propertyPathName, out object result )
        {
            if ( string.IsNullOrWhiteSpace( propertyPathName ) )
            {
                result = null;
                return false;
            }

            if ( rootObj == null )
            {
                rootObj = this;
            }

            var propPath = propertyPathName.Split( new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries ).ToList<string>();

            object obj = rootObj;

            while ( propPath.Any() && obj != null )
            {
                // Get the property accessor.
                var propName = propPath.First();

                PropertyInfo prop;
                bool getPropertyValue;

                if ( obj == this )
                {
                    // Get the property accessor for this object.
                    InstancePropertyLookup.TryGetValue( propName, out prop );
                    getPropertyValue = true;
                }
                else if ( obj is LavaDataObject dataObject )
                {
                    // Get the property value for the dynamic object.
                    dataObject.GetProperty( obj, propName, out obj );
                    prop = null;
                    getPropertyValue = false;
                }
                else
                {
                    // Get the property accessor for the child property value.
                    prop = obj.GetType().GetProperty( propName );
                    getPropertyValue = true;
                }

                if ( getPropertyValue )
                {
                    if ( prop == null )
                    {
                        result = null;
                        return false;
                    }

                    // Get the value from the property
                    obj = prop.GetValue( obj, null );
                }

                propPath = propPath.Skip( 1 ).ToList();
            }

            result = obj;
            return true;
        }

        /// <summary>
        /// Sets the property.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected bool SetProperty( string name, object value )
        {
            if ( name == null )
            {
                return false;
            }

            PropertyInfo prop;

            InstancePropertyLookup.TryGetValue( name, out prop );

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
                    PropertyInfo prop;

                    InstancePropertyLookup.TryGetValue( key, out prop );

                    if ( prop != null )
                    {
                        SetProperty( key, value );
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
        /// Provides the implementation for operations that get member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject" /> class can override this method to specify dynamic behavior for operations such as getting a value for a property.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member on which the dynamic operation is performed. For example, for the Console.WriteLine(sampleObject.SampleProperty) statement, where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject" /> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
        /// <param name="result">The result of the get operation. For example, if the method is called for a property, you can assign the property value to <paramref name="result" />.</param>
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a run-time exception is thrown.)
        /// </returns>
        public virtual bool TryGetMember( string memberName, out object result )
        {
            // first check the dictionary for member
            if ( _members.Keys.Contains( memberName ) )
            {
                result = _members[memberName];
                return true;
            }

            // next check for public properties via Reflection
            try
            {
                return GetProperty( _instance, memberName, out result );
            }
            catch
            {
            }

            // failed to retrieve a property
            result = null;
            return false;
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
                var propertyKey = ( key == null ) ? string.Empty : key.ToString();
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
                return InstancePropertyLookup.ContainsKey( item.Key );
            }

            return false;
        }

        /// <summary>
        /// Determines whether the object property dictionary contains the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool ContainsKey( object key )
        {
            return this.GetDynamicMemberNames().Contains( key.ToString() );
        }

        #endregion

        /// <summary>
        /// Gets a dictionary populated with the properties of the dynamic object.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> AsDictionary()
        {
            /* [2020-10-01] DJL
             * Although it would be preferable to implement the IDictionary<string, object> interface directly,
             * doing so breaks the use of the RockDynamic class to store an individual row in the result set of a LINQ query.
             * LINQ does not support the use of an IEnumerable to receive an individual row of data, and
             * this technique is used in a number of places in the Rock codebase.
             */
            var memberNames = this.GetDynamicMemberNames();

            var dictionary = new Dictionary<string, object>( _members );

            foreach ( var memberName in memberNames )
            {
                if ( dictionary.ContainsKey( memberName ) )
                {
                    dictionary[memberName] = this[memberName];
                }
                else
                {
                    dictionary.Add( memberName, this[memberName] );
                }
            }

            return dictionary;
        }

        /// <summary>
        /// Get the value associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetValue( object key )
        {
            object value;

            if ( key == null )
            {
                return null;
            }

            GetProperty( this, key.ToString(), out value );

            return value;
        }

        #region IDictionary<string, object> implementation.

        public ICollection<string> Keys
        {
            get
            {
                return this.AvailableKeys;
            }
        }

        public ICollection<object> Values
        {
            get
            {
                var dictionary = GetValueDictionary();

                return dictionary.Values;
            }
        }

        public int Count
        {
            get
            {
                return this.GetDynamicMemberNames().Count();
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool ContainsKey( string key )
        {
            return this.GetDynamicMemberNames().Contains( key );
        }

        public void Add( string key, object value )
        {
            this[key] = value;
        }

        public bool Remove( string key )
        {
            return ( (IDictionary<string, object>)_members ).Remove( key );
        }

        public bool TryGetValue( string key, out object value )
        {
            return this.TryGetMember( key, out value );
        }

        public void Add( KeyValuePair<string, object> item )
        {
            this[item.Key] = item.Value;
        }

        public void Clear()
        {
            ( (IDictionary<string, object>)_members ).Clear();
        }

        public bool Contains( KeyValuePair<string, object> item )
        {
            return ( (IDictionary<string, object>)_members ).Contains( item );
        }

        public void CopyTo( KeyValuePair<string, object>[] array, int arrayIndex )
        {
            ( (IDictionary<string, object>)_members ).CopyTo( array, arrayIndex );
        }

        public bool Remove( KeyValuePair<string, object> item )
        {
            return ( (IDictionary<string, object>)_members ).Remove( item );
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this.GetValueDictionary().GetEnumerator();
        }

        private Dictionary<string, object> GetValueDictionary()
        {
            var memberNames = this.GetDynamicMemberNames();

            var dictionary = new Dictionary<string, object>( _members );

            foreach ( var memberName in memberNames )
            {
                if ( dictionary.ContainsKey( memberName ) )
                {
                    dictionary[memberName] = this[memberName];
                }
                else
                {
                    dictionary.Add( memberName, this[memberName] );
                }
            }

            return dictionary;

        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}
