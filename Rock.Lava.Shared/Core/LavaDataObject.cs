﻿// <copyright>
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
    /// A container for exposing data that is intended to be accessible to a Lava template.
    /// This Type can be used as a base class for a derived Type, as a proxy for an existing object, or
    /// as a dictionary of values populated dynamically at runtime.
    /// </summary>
    /// <remarks>
    /// Objects of this type are always serialized and deserialized as a dictionary of values.
    /// If a proxy object exists, it will not be recreated during deserialization.
    /// </remarks>
    [Serializable]
    public class LavaDataObject : ILavaDataDictionary, IDictionary, IDynamicMetaObjectProvider
    {
        // The LavaDataObject is a simple wrapper for LavaDataObjectInternal, with the purpose of hiding some public methods
        // that are unnecessary or otherwise confusing for Lava developers seeking to implement a simple Lava data source.
        // LavaDataObjectInternal is an extended implementation of DynamicObject.

        [NonSerialized]
        private LavaDataObjectInternal _lavaDataObjectInternal = null;

        #region Factory Methods

        /// <summary>
        /// Creates a new instance of the <see cref="LavaDataObject"/> class as a proxy that makes the properties of the supplied object available to Lava.
        /// Additional properties may be added at runtime.
        /// </summary>
        /// <param name="obj">The proxy object.</param>
        public static LavaDataObject FromAnonymousObject( object obj )
        {
            return new LavaDataObject( obj );
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LavaDataObject"/> class as a Lava-accessible dictionary.
        /// </summary>
        public LavaDataObject()
        {
            SetLavaDataObjectInternal( this );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LavaDataObject"/> class as a proxy that makes the properties of the supplied object available to Lava.
        /// Additional properties may be added at runtime.
        /// </summary>
        /// <param name="obj">The proxy object.</param>
        public LavaDataObject( object obj )
        {
            SetLavaDataObjectInternal( obj );
        }

        #endregion

        /// <summary>
        /// Gets the LavaDataObjectInternal instance, or instantiates a new instance if it has not been created.
        /// Lazy instantiation of the internal helper object occurs if this instance is created by deserialization.
        /// </summary>
        /// <remarks>
        /// The LavaDataObjectInternal is an implementation of a DynamicObject.
        /// </remarks>
        /// <returns></returns>
        private LavaDataObjectInternal GetLavaDataObjectInternal()
        {
            if ( _lavaDataObjectInternal == null )
            {
                SetLavaDataObjectInternal( this );
            }

            return _lavaDataObjectInternal;
        }

        /// <summary>
        /// Initializes the internal implementation of the LavaDataObject.
        /// </summary>
        /// <returns></returns>
        private void SetLavaDataObjectInternal( object wrappedObject )
        {
            _lavaDataObjectInternal = new LavaDataObjectInternal( wrappedObject );

            _lavaDataObjectInternal.TryGetMemberCallback = OnTryGetValue;
            _lavaDataObjectInternal.TrySetMemberCallback = OnTrySetValue;
        }

        /// <summary>
        /// Override this method to provide a custom implementation for retrieving a property value from this data object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="result"></param>
        /// <returns>True if the value was resolved.</returns>
        protected virtual bool OnTryGetValue( string key, out object result )
        {
            if ( key == null )
            {
                result = null;
                return false;
            }

            var ldo = GetLavaDataObjectInternal();

            return ldo.TryGetMember( key, out result, withCallback: false, includeDynamicMembers: true );
        }

        /// <summary>
        /// Override this method to provide a custom implementation for retrieving a property value from this data object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="result"></param>
        /// <returns>True if the value was resolved.</returns>
        protected virtual bool OnTrySetValue( string key, object value )
        {
            var ldo = GetLavaDataObjectInternal();

            return ldo.TrySetMember( key, value );
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
                var ldo = GetLavaDataObjectInternal();

                ldo[key] = value;
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
            return TryGetValueInternal( key, out value );
        }

        /// <summary>
        /// Try to get the value associated with the specified key.
        /// </summary>
        /// <param name="memberName"></param>
        /// <param name="result"></param>
        /// <param name="withCallback">A flag indicating if the internal callback function should be processed when attempting to retrieve the value.</param>
        /// <param name="includeDynamicMembers"></param>
        /// <returns></returns>
        internal bool TryGetValueInternal( string memberName, out object result, bool withCallback = true, bool includeDynamicMembers = true )
        {
            var ldo = GetLavaDataObjectInternal();

            return ldo.TryGetMember( memberName, out result, withCallback, includeDynamicMembers );
        }

        #region ILavaDataDictionary

        /// <summary>
        /// Gets the collection of available property keys.
        /// </summary>
        /// <value>
        /// The available keys.
        /// </value>
        [LavaHidden]
        public virtual List<string> AvailableKeys
        {
            get
            {
                var ldo = GetLavaDataObjectInternal();

                return ldo.GetDynamicMemberNames().ToList();
            }
        }

        /// <summary>
        /// Determines whether the object property dictionary contains the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool ContainsKey( string key )
        {
            // First, check if this is a defined member name or dynamic property.
            if ( AvailableKeys.Contains( key ) )
            {
                return true;
            }

            // As a fallback, see if a value can be retrieved for this key.
            // This code will only execute if:
            // 1. The property key is not valid for this object; or
            // 2. An override for OnTryGetValue exists without a corresponding override for AvailableKeys.
            //    In this case, it may be best to override the AvailableKeys method to expose the property key and improve
            //    the efficiency of the implementation. However, this may not be possible in cases where
            //     obtaining an exhaustive list of available keys is expensive or impractical, such as where
            //     the property value is only created on request.
            var ldo = GetLavaDataObjectInternal();

            return ldo.TryGetMember( key, out _ );
        }

        /// <summary>
        /// Get the value associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
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
            var dictionary = GetLavaDataObjectInternal().ToDictionary();

            return dictionary.GetEnumerator();
        }

        void ICollection.CopyTo( Array array, int index )
        {
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
            var dictionary = GetLavaDataObjectInternal().ToDictionary();

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
                var ldo = GetLavaDataObjectInternal();

                return ldo.GetProperties().Select( x => x.Value ).ToList();
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
                var ldo = GetLavaDataObjectInternal();

                return ldo.GetProperties().Count();
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

                var ldo = GetLavaDataObjectInternal();

                ldo[key.ToString()] = value;
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
            var ldo = GetLavaDataObjectInternal();

            var metaObject = ( ( IDynamicMetaObjectProvider ) ldo ).GetMetaObject( parameter );

            return metaObject;
        }

        #endregion
    }

    /// <summary>
    /// An implementation of a DynamicObject that can expose the properties of a proxy object to the Lava framework.
    /// This Type can either be used as a base class for a Type whose properties should be visible in Lava,
    /// or as a proxy for an object that does not inherit from this base class.
    /// Additional member values may also be defined dynamically at runtime.
    /// </summary>
    /// <seealso cref="System.Dynamic.DynamicObject" />
    internal class LavaDataObjectInternal : DynamicObject
    {
        public delegate bool TryGetValueDelegate( string memberName, out object memberValue );
        public delegate bool TrySetValueDelegate( string memberName, object memberValue );

        private Dictionary<string, object> _dynamicMembers = new Dictionary<string, object>();
        private Dictionary<string, PropertyInfo> _instancePropertyInfoLookup;

        private object _targetObject;
        private IDictionary _targetObjectAsDictionary = null;
        private TypeConverter _targetDictionaryKeyConverter = null;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LavaDataObject"/> class as a proxy that makes the target object available to Lava.
        /// </summary>
        /// <param name="obj">The target object.</param>
        public LavaDataObjectInternal( object obj )
        {
            _targetObject = obj;

            _targetObjectAsDictionary = _targetObject as IDictionary;
        }

        #endregion

        /// <summary>
        /// Gets the target object that is proxied by this dynamic object.
        /// </summary>
        public object TargetObject
        {
            get
            {
                return _targetObject;
            }
        }

        /// <summary>
        /// Gets the collection of the members defined at run-time for this instance.
        /// </summary>
        public IDictionary<string, object> DynamicMembers
        {
            get
            {
                return _dynamicMembers;
            }
        }

        /// <summary>
        /// Defines a method that can be used to implement a custom member getter.
        /// </summary>
        public TryGetValueDelegate TryGetMemberCallback { get; set; }

        /// <summary>
        /// Defines a method that can be used to implement a custom member setter.
        /// </summary>
        public TrySetValueDelegate TrySetMemberCallback { get; set; }

        /// <summary>
        /// Gets the collection of properties that are defined by the target object type.
        /// This does not include member values added dynamically at runtime.
        /// </summary>
        private Dictionary<string, PropertyInfo> GetInstanceProperties()
        {
            if ( _instancePropertyInfoLookup == null
                    && _targetObject != null )
            {
                var lavaBaseType = typeof( LavaDataObject );
                _instancePropertyInfoLookup = _targetObject.GetType().GetProperties().Where( a => a.DeclaringType != lavaBaseType ).ToDictionary( k => k.Name, v => v );
            }

            return _instancePropertyInfoLookup;
        }

        /// <summary>
        /// Determines whether the data object has a definition for the specified property.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey( string key )
        {
            if ( _dynamicMembers.ContainsKey( key ) )
            {
                return true;
            }

            var properties = GetInstanceProperties();
            if ( properties.ContainsKey( key ) )
            {
                return true;
            }

            if ( _targetObjectAsDictionary != null )
            {
                var converter = GetTargetDictionaryKeyConverter();

                if ( converter != null )
                {
                    var typedKey = _targetDictionaryKeyConverter.ConvertFromString( key );
                    return _targetObjectAsDictionary.Contains( typedKey );
                }
                else
                {
                    return _targetObjectAsDictionary.Contains( key );
                }
            }

            return false;
        }

        /// <summary>
        /// Return a string representation of the dynamic object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // If we are wrapping an object instance, return the ToString() for the object,
            // otherwise return the first value in the member values dictionary.
            if ( _targetObject != null )
            {
                if ( _targetObject == this )
                {
                    return null;
                }

                return _targetObject.ToString();
            }

            if ( _dynamicMembers != null )
            {
                var firstKey = _dynamicMembers.Keys.FirstOrDefault();

                if ( firstKey != null )
                {
                    var firstValue = _dynamicMembers[firstKey] ?? string.Empty;

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
        /// <param name="memberName"></param>
        /// <param name="result"></param>
        /// <param name="withCallback"></param>
        /// <param name="includeDynamicMembers"></param>
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a run-time exception is thrown.)
        /// </returns>
        public bool TryGetMember( string memberName, out object result, bool withCallback = true, bool includeDynamicMembers = true )
        {
            // If a custom member lookup is defined, try to get the member value from there first.
            // This feature is implemented as a delegate method here rather than simply allowing the user to override TryGetMember,
            // because we want to hide the complexity of the DynamicObject implementation from the caller.
            if ( withCallback &&
                 TryGetMemberCallback != null )
            {
                var exists = TryGetMemberCallback( memberName, out result );

                if ( exists )
                {
                    return true;
                }
            }

            // Check for public properties defined on the target object instance.
            return GetPropertyValue( memberName, includeDynamicMembers, out result );
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
            return TrySetMember( binder.Name, value );
        }

        /// <summary>
        /// Provides the implementation for operations that set member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject" /> class can override this method to specify dynamic behavior for operations such as setting a value for a property.
        /// </summary>
        /// <param name="name">The name of the property to set.</param>
        /// <param name="value">The value to set to the member. For example, for sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject" /> class, the <paramref name="value" /> is "Test".</param>
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)
        /// </returns>
        public bool TrySetMember( string name, object value )
        {
            // If the member exists as a property of the target object, set it.
            if ( _targetObject != null )
            {
                try
                {
                    bool result = SetProperty( name, value );
                    if ( result )
                    {
                        return true;
                    }
                }
                catch
                {
                }
            }

            // If a target property property could not be set, add or update the value of a dynamic member.
            _dynamicMembers[name] = value;

            return true;
        }

        private TypeConverter GetTargetDictionaryKeyConverter()
        {
            // This value is set in the constructor if the target object is a dictionary.
            // If it is, we collect some metadata about the dictionary to improve access efficiency.
            if ( _targetObjectAsDictionary == null )
            {
                return null;
            }

            if ( _targetDictionaryKeyConverter == null )
            {
                var targetObjectType = _targetObject.GetType();
                Type keyType;

                if ( _targetObject is ILavaDataDictionary )
                {
                    keyType = typeof( string );
                }
                else if ( targetObjectType.IsGenericType )
                {
                    keyType = targetObjectType.GetGenericArguments()[0];
                }
                else
                {
                    keyType = typeof( object );
                }

                // If the key type is not a string, get a converter for the key value.
                if ( keyType != typeof( string ) )
                {
                    try
                    {
                        _targetDictionaryKeyConverter = TypeDescriptor.GetConverter( keyType );
                    }
                    catch ( NotSupportedException )
                    {
                        // There is no way to convert a property path string to the same type as the dictionary key,
                        // so we can only return values for property references.
                        _targetDictionaryKeyConverter = null;
                        _targetObjectAsDictionary = null;
                    }
                }
            }

            return _targetDictionaryKeyConverter;
        }

        /// <summary>
        /// Gets the value of the specified property.
        /// The property may refer to any of the following:
        /// 1. An entry in the local property dictionary, added dynamically at runtime.
        /// 2. If the target object is a dictionary, a value with a matching key.
        /// 3. A member property of the target object.
        /// </summary>
        /// <param name="propertyPathName">The named path to the property, which may include references to nested object properties in a dot-separated list.</param>
        /// <param name="includeDynamicMembers">
        /// If true, the search will include properties defined in the local or target object dictionaries; otherwise, only
        /// properties of the target object will be searched.
        /// </param>
        /// <param name="result"></param>
        /// <returns></returns>
        private bool GetPropertyValue( string propertyPathName, bool includeDynamicMembers, out object result )
        {
            if ( string.IsNullOrWhiteSpace( propertyPathName ) )
            {
                result = null;
                return false;
            }

            /*
             * Evaluation of the member reference should proceed from least to most expensive operations.
             * Dictionary searches should be performed first, and operations involving Reflection should occur last.
             */
            if ( includeDynamicMembers )
            {
                // First, try to resolve a reference to a dynamic member defined for this object instance.
                if ( _dynamicMembers.ContainsKey( propertyPathName ) )
                {
                    result = _dynamicMembers[propertyPathName];
                    return true;
                }

                // If we have a proxied dictionary object, check if this is a reference to a key in that dictionary.
                if ( _targetObjectAsDictionary != null )
                {
                    if ( _targetObject is LavaDataObject ldo )
                    {
                        // Although the LavaDataObject implements ILavaDataDictionary, we can use a more efficient
                        // access method here.
                        return ldo.TryGetValueInternal( propertyPathName, out result, false, false );
                    }
                    else if ( _targetObject is ILavaDataDictionary ldd )
                    {
                        if ( ldd.ContainsKey( propertyPathName ) )
                        {
                            result = ldd.GetValue( propertyPathName );
                            return true;
                        }
                    }
                    else
                    {
                        var keyConverter = GetTargetDictionaryKeyConverter();

                        if ( keyConverter != null )
                        {
                            // Convert the property path to the same type as the dictionary key and attempt a dictionary lookup.
                            try
                            {
                                var keyValue = keyConverter.ConvertFromInvariantString( propertyPathName );

                                if ( keyValue != null
                                     && _targetObjectAsDictionary.Contains( keyValue ) )
                                {
                                    result = _targetObjectAsDictionary[keyValue];
                                    return true;
                                }
                            }
                            catch ( NotSupportedException )
                            {
                                // The property path can't be converted to the same type as the dictionary key.
                                throw new Exception( "LavaDataObject internal error. The property path name is invalid." );
                            }
                        }
                    }
                }
            }

            // Finally, try to resolve the reference as a property path.
            var propPath = propertyPathName.Split( new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries ).ToList<string>();

            object obj = _targetObject ?? this;

            while ( propPath.Any() && obj != null )
            {
                // Get the property accessor.
                var propName = propPath.First();

                PropertyInfo prop;
                bool getPropertyValue;

                if ( obj == this )
                {
                    // Get the property accessor for this object.
                    var properties = GetInstanceProperties();

                    properties.TryGetValue( propName, out prop );

                    getPropertyValue = true;
                }
                else if ( obj is LavaDataObjectInternal dataObject )
                {
                    // Get the property value for the dynamic object.
                    dataObject.GetPropertyValue( propName, true, out obj );
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

            var properties = GetInstanceProperties();

            properties.TryGetValue( name, out prop );

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
                object result;
                if ( GetPropertyValue( key, true, out result ) )
                {
                    return result;
                }

                return null;
            }

            set
            {
                if ( key != null )
                {
                    if ( _dynamicMembers.ContainsKey( key ) )
                    {
                        _dynamicMembers[key] = value;
                        return;
                    }

                    // check instance for existence of type first
                    PropertyInfo prop;

                    var properties = GetInstanceProperties();

                    properties.TryGetValue( key, out prop );

                    if ( prop != null )
                    {
                        SetProperty( key, value );
                    }
                    else
                    {
                        _dynamicMembers[key] = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a dictionary of properties and values defined for this object instance.
        /// </summary>
        /// <param name="includeInstanceProperties">if set to <c>true</c> [include instance properties].</param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, object>> GetProperties( bool includeInstanceProperties = false )
        {
            if ( includeInstanceProperties && _targetObject != null )
            {
                foreach ( var prop in GetInstanceProperties().Values )
                {
                    yield return new KeyValuePair<string, object>( prop.Name, prop.GetValue( _targetObject, null ) );
                }
            }

            foreach ( var key in _dynamicMembers.Keys )
            {
                yield return new KeyValuePair<string, object>( key, _dynamicMembers[key] );
            }
        }

        /// <summary>
        /// Creates a dictionary containing the current values of the data object properties.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> ToDictionary()
        {
            var dictionary = new Dictionary<string, object>();

            var memberNames = this.GetDynamicMemberNames();

            foreach ( var memberName in memberNames )
            {
                object value;

                var exists = TryGetMember( memberName, out value );

                if ( !exists )
                {
                    continue;
                }

                dictionary.Add( memberName, value );
            }

            return dictionary;
        }

        /// <summary>
        /// Returns a collection of all member names defined for the current object, both static and dynamic.
        /// </summary>
        /// <returns>
        /// A sequence that contains dynamic member names.
        /// </returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            var memberNames = new List<string>();

            // Add the names of the properties defined on the object instance.
            foreach ( var propName in GetInstanceProperties().Keys )
            {
                memberNames.Add( propName );
            }

            // Add the names of any dynamic members added at runtime.
            foreach ( var key in _dynamicMembers.Keys )
            {
                memberNames.Add( key );
            }

            return memberNames;
        }
    }
}
