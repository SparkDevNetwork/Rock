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
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rock.Lava
{
    /// <summary>
    /// A container for exposing data that should be accessible to a Lava template.
    /// This Type can be used as a base class for a derived Type, as a proxy for an existing object, or
    /// as a dictionary of values populated dynamically at runtime.
    /// </summary>
    /// <remarks>
    /// This class does not serialize dynamic properties, but is marked as serializable to allow derived classes to be serialized correctly.
    /// </remarks>
    [Serializable]
    public class LavaDataObject : ILavaDataDictionary, IDictionary, IDynamicMetaObjectProvider
    {
        // The internal implementation of the DynamicObject that is used to manage access to the LavaDataObject properties.
        // This class is implemented privately because it has a number of the public methods that are unnecessary or confusing
        // for Lava developers looking to implement a simple Lava data source.
        [NonSerialized]
        private LavaDataObjectInternal _lavaDataObjectInternal;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LavaDataObject"/> class as a Lava-accessible dictionary.
        /// </summary>
        public LavaDataObject()
        {
            _lavaDataObjectInternal = new LavaDataObjectInternal( this );

            _lavaDataObjectInternal.TryGetMemberCallback = OnTryGetValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LavaDataObject"/> class as a proxy that makes the properties of the supplied object available to Lava.
        /// </summary>
        /// <param name="obj">The object.</param>
        public LavaDataObject( object obj )
        {
            _lavaDataObjectInternal = new LavaDataObjectInternal( obj );

            _lavaDataObjectInternal.TryGetMemberCallback = OnTryGetValue;
        }

        #endregion

        /// <summary>
        /// Override this method to provide a custom implementation for retrieving a property value from this data object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="result"></param>
        /// <returns>True if the value was resolved.</returns>
        [LavaHidden]
        protected virtual bool OnTryGetValue( string key, out object result )
        {
            if ( key == null )
            {
                result = null;
                return false;
            }

            return _lavaDataObjectInternal.GetProperty( key.ToString(), out result );
        }

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified key.
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
                return GetValue( key );
            }
            set
            {
                _lavaDataObjectInternal[key] = value;
            }
        }

        /// <summary>
        /// Try to get the value associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>True if a value is available for the specified key.</returns>
        public bool TryGetValue( string key, out object value )
        {
            return _lavaDataObjectInternal.TryGetMember( key, out value );
        }

        #region ILavaDataDictionary Implementation

        /// <summary>
        /// Gets the collection of available keys.
        /// </summary>
        /// <value>
        /// The available keys.
        /// </value>
        [LavaHidden]
        public virtual List<string> AvailableKeys
        {
            get
            {
                return _lavaDataObjectInternal.GetDynamicMemberNames().ToList();
            }
        }

        /// <summary>
        /// Determines whether the object property dictionary contains the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        [LavaHidden]
        public bool ContainsKey( string key )
        {
            // First, check if this is a defined member name or or an existing dictionary entry.
            if ( AvailableKeys.Contains( key ) )
            {
                return true;
            }

            // As a fallback, see if a value can be retrieved for this key.
            // This code will only execute if the property does not exist,
            // or an override for OnTryGetValue exists without a corresponding override for AvailableKeys.
            // In this situation, overriding the AvailableKeys method is a more efficient implementation.
            return TryGetValue( key, out _ );
        }

        /// <summary>
        /// Get the value associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [LavaHidden]
        public object GetValue( string key )
        {
            object result;

            OnTryGetValue( key, out result );

            return result;
        }

        #endregion

        #region IDictionary

        bool IDictionary.Contains( object key )
        {
            if ( key == null )
            {
                return false;
            }

            return ContainsKey( key.ToString() );
        }

        void IDictionary.Add( object key, object value )
        {
            throw new NotImplementedException( "LavaDataObject operation failed. The LavaDataObject is read-only." );
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            var dictionary = _lavaDataObjectInternal.ToDictionary();

            return dictionary.GetEnumerator();
        }

        void ICollection.CopyTo( Array array, int index )
        {
            var dictionary = _lavaDataObjectInternal.ToDictionary();

            if ( array == null )
            {
                throw new ArgumentNullException( "array" );
            }

            var targetArray = array as KeyValuePair<string, object>[];

            if ( targetArray == null )
            {
                throw new ArgumentException( "Destination array must be of type KeyValuePair<string, object>[]" );
            }

            var source = this as ICollection<KeyValuePair<string, object>>;

            source.CopyTo( targetArray, index );
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            var dictionary = _lavaDataObjectInternal.ToDictionary();

            return dictionary.GetEnumerator();
        }

        ICollection IDictionary.Keys
        {
            get
            {
                return AvailableKeys;
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                return _lavaDataObjectInternal.GetProperties().Select( x => x.Value ).ToList();
            }
        }

        bool IDictionary.IsReadOnly
        {
            get
            {
                return true;
            }
        }

        bool IDictionary.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        int ICollection.Count
        {
            get
            {
                return _lavaDataObjectInternal.GetProperties().Count();
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                if ( key == null )
                {
                    return null;
                }

                return GetValue( key.ToString() );
            }
            set
            {
                if ( key == null )
                {
                    return;
                }

                _lavaDataObjectInternal[key.ToString()] = value;
            }
        }

        void IDictionary.Clear()
        {
            throw new NotImplementedException( "LavaDataObject operation failed. The LavaDataObject is read-only." );
        }

        void IDictionary.Remove( object key )
        {
            throw new NotImplementedException( "LavaDataObject operation failed. The LavaDataObject is read-only." );
        }

        #endregion

        #region IDynamicMetaObjectProvider

        public DynamicMetaObject GetMetaObject( Expression parameter )
        {
            var metaObject = ( ( IDynamicMetaObjectProvider ) _lavaDataObjectInternal ).GetMetaObject( parameter );

            return metaObject;
        }

        #endregion
    }

    /// <summary>
    /// An implementation of a DynamicObject that can be used to expose the properties of an object to Lava.
    /// This Type can either be used as a base class for a Type whose properties should be made visible in Lava,
    /// or as a proxy for an object that does not inherit from this base class.
    /// </summary>
    /// <seealso cref="System.Dynamic.DynamicObject" />
    internal class LavaDataObjectInternal : DynamicObject
    {
        public delegate bool TryGetValueDelegate( string memberName, out object memberValue );

        private Dictionary<string, object> _members = new Dictionary<string, object>();

        private object _targetObject;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LavaDataObject"/> class as a proxy that makes the target object available to Lava.
        /// </summary>
        /// <param name="obj">The target object.</param>
        public LavaDataObjectInternal( object obj )
        {
            _targetObject = obj;
        }

        #endregion

        /// <summary>
        /// Gets the collection of dynamic properties defined for this instance.
        /// </summary>
        public IDictionary<string, object> DynamicProperties
        {
            get
            {
                return _members;
            }
        }

        /// <summary>
        /// Defines a method that can be used to implement a custom member lookup.
        /// </summary>
        public TryGetValueDelegate TryGetMemberCallback { get; set; }

        private Dictionary<string, PropertyInfo> InstancePropertyLookup
        {
            get
            {
                if ( _instancePropertyInfoLookup == null && _targetObject != null )
                {
                    var lavaBaseType = typeof( LavaDataObject );
                    _instancePropertyInfoLookup = _targetObject.GetType().GetProperties().Where( a => a.DeclaringType != lavaBaseType ).ToDictionary( k => k.Name, v => v );
                }

                return _instancePropertyInfoLookup;
            }
        }

        private Dictionary<string, PropertyInfo> _instancePropertyInfoLookup;

        /// <summary>
        /// Return a string representation of the dynamic object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // If we are wrapping an object instance, return the ToString() for the object,
            // otherwise return the first value in the property dictionary.
            if ( _targetObject != null )
            {
                if ( _targetObject == this )
                {
                    return null;
                }
                return _targetObject.ToString();
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
        /// Provides the implementation for operations that get member values.
        /// </summary>
        /// <param name="binder">The name of the member on which the dynamic operation is performed.
        /// <param name="result">The result of the get operation. For example, if the method is called for a property, you can assign the property value to <paramref name="result" />.</param>
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a run-time exception is thrown.)
        /// </returns>
        public bool TryGetMember( string memberName, out object result )
        {
            // If a custom member lookup is defined, try to get the member value from there first.
            // This feature is implemented as a delegate method here rather than simply allowing the user to override TryGetMember,
            // because we do not want to expose the many other public methods of the DynamicObject implementation to the caller.
            if ( TryGetMemberCallback != null )
            {
                var exists = TryGetMemberCallback( memberName, out result );

                if ( exists )
                {
                    return true;
                }
            }

            // Check the dynamic dictionary of values for the member.
            if ( _members.Keys.Contains( memberName ) )
            {
                result = _members[memberName];
                return true;
            }

            // Check for public properties defined on the target object.
            try
            {
                return GetProperty( _targetObject, memberName, out result );
            }
            catch
            {
            }

            // Failed to find the property, so return nothing.
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
            if ( _targetObject != null )
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

        public bool GetProperty( string propertyPathName, out object result )
        {
            return GetProperty( _targetObject, propertyPathName, out result );
        }

        /// <summary>
        /// Gets the value of the target object property as specified by propertyPathName.
        /// If the target object is a dictionary, retrieves the value associated with the matching key.
        /// </summary>
        /// <param name="rootObj">The root obj.</param>
        /// <param name="propertyPathName">The named path to the property, which may include references to nested properties in a dot-separated list.</param>
        /// <returns></returns>
        public bool GetProperty( object rootObj, string propertyPathName, out object result )
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


            // First check if this is a reference to a dictionary key.
            if ( rootObj is IDictionary dictionary )
            {
                var dictionaryType = dictionary.GetType();

                Type keyType;

                if ( dictionaryType.IsGenericType )
                {
                    keyType = dictionaryType.GetGenericArguments()[0];
                }
                else
                {
                    keyType = typeof( object );
                }

                // Try to convert the property path to the same type as the dictionary key so we can attempt a lookup.
                try
                {
                    var keyValue = TypeDescriptor.GetConverter( keyType ).ConvertFromInvariantString( propertyPathName );

                    if ( keyValue != null
                         && dictionary.Contains( keyValue ) )
                    {
                        result = dictionary[keyValue];
                        return true;
                    }
                }
                catch ( NotSupportedException )
                {
                    // The property path cannot be converted to the same type as the dictionary key,
                    // so proceed to evaluate it as a class member reference instead.
                }
            }

            // Next, try to resolve the reference as a property path.
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
                else if ( obj is LavaDataObjectInternal dataObject )
                {
                    // Get the property value for the dynamic object.
                    dataObject.GetProperty( propName, out obj );
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
                prop.SetValue( _targetObject, value, null );
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
                if ( GetProperty( _targetObject, key, out result ) )
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
            if ( includeInstanceProperties && _targetObject != null )
            {
                foreach ( var prop in InstancePropertyLookup.Values )
                {
                    yield return new KeyValuePair<string, object>( prop.Name, prop.GetValue( _targetObject, null ) );
                }
            }

            foreach ( var key in _members.Keys )
            {
                yield return new KeyValuePair<string, object>( key, _members[key] );
            }
        }

        /// <summary>
        /// Creates a dictionary containing the current values of the data object properties.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> ToDictionary()
        {
            var dictionary = new Dictionary<string, object>();

            // Add the properties of the wrapped target object.
            if ( _targetObject != null )
            {
                foreach ( var prop in InstancePropertyLookup.Values )
                {
                    dictionary.Add( prop.Name, prop.GetValue( _targetObject, null ) );
                }
            }

            // Add the dynamic properties.
            foreach ( var key in _members.Keys )
            {
                dictionary.Add( key, _members[key] );
            }

            return dictionary;
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

            foreach ( var propName in InstancePropertyLookup.Keys )
            {
                propertyNames.Add( propName );
            }

            foreach ( var key in _members.Keys )
            {
                propertyNames.Add( key );
            }

            return propertyNames;
        }

    }
}
