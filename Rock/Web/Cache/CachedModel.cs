// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Runtime.Caching;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
    /// A Secured data transfer object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    [DataContract]
    public abstract class CachedModel<T> : CachedEntity<T>, ISecured, Rock.Attribute.IHasAttributes, Lava.ILiquidizable
        where T : Rock.Data.Entity<T>, ISecured, Rock.Attribute.IHasAttributes, new()
    {
        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Rock.Data.IEntity model )
        {
            base.CopyFromModel( model );

            var secureModel = model as ISecured;
            if ( secureModel != null )
            {
                this.TypeId = secureModel.TypeId;
                this.TypeName = secureModel.TypeName;
                this.SupportedActions = secureModel.SupportedActions;
            }

            var attributeModel = model as Rock.Attribute.IHasAttributes;
            if ( attributeModel != null )
            {
                if ( attributeModel.Attributes != null )
                {
                    this.Attributes = attributeModel.Attributes;
                    this.AttributeValues = attributeModel.AttributeValues;
                }
            }
        }

        #region ISecured Implementation

        /// <summary>
        /// Gets the Entity Type ID for this entity.
        /// </summary>
        /// <value>
        /// The type id.
        /// </value>
        [DataMember]
        [LavaIgnore]
        public virtual int TypeId { get; private set; }

        /// <summary>
        /// The auth entity. Classes that implement the <see cref="ISecured" /> interface should return
        /// a value that is unique across all <see cref="ISecured" /> classes.  Typically this is the
        /// qualified name of the class.
        /// </summary>
        [DataMember]
        [LavaIgnore]
        public virtual string TypeName { get; private set; }

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check access to the parent authority specified by this property.
        /// </summary>
        [LavaIgnore]
        public virtual ISecured ParentAuthority
        {
            get
            {
                if ( this.Id == 0 )
                {
                    return new GlobalDefault();
                }
                else
                {
                    return new T();
                }
            }
        }

        /// <summary>
        /// An optional additional parent authority.  (i.e for Groups, the GroupType is main parent
        /// authority, but parent group is an additional parent authority )
        /// </summary>
        [LavaIgnore]
        public virtual Security.ISecured ParentAuthorityPre
        {
            get { return null; }
        }

        /// <summary>
        /// A dictionary of actions that this class supports and the description of each.
        /// </summary>
        [LavaIgnore]
        public virtual Dictionary<string, string> SupportedActions { get; private set; }

        /// <summary>
        /// Return <c>true</c> if the user is authorized to perform the selected action on this object.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        ///   <c>true</c> if the specified action is authorized; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsAuthorized( string action, Person person )
        {
            return Security.Authorization.Authorized( this, action, person );
        }

        /// <summary>
        /// If a user or role is not specifically allowed or denied to perform the selected action,
        /// return <c>true</c> if they should be allowed anyway or <c>false</c> if not.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public virtual bool IsAllowedByDefault( string action )
        {
            return action == Authorization.VIEW;
        }

        /// <summary>
        /// Determines whether the specified action is private (Only the current user has access).
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        ///   <c>true</c> if the specified action is private; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsPrivate( string action, Person person )
        {
            return Security.Authorization.IsPrivate( this, action, person );
        }

        /// <summary>
        /// Makes the action on the current entity private (Only the current user will have access).
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        public virtual void MakePrivate( string action, Person person, RockContext rockContext = null )
        {
            Security.Authorization.MakePrivate( this, action, person, rockContext );
        }

        /// <summary>
        /// If action on the current entity is private, removes security that made it private.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        public virtual void MakeUnPrivate( string action, Person person, RockContext rockContext )
        {
            Security.Authorization.MakeUnPrivate( this, action, person, rockContext );
        }

        #endregion

        #region IHasAttribute Implementation

        /// <summary>
        /// List of attributes associated with the object.  This property will not include the attribute values.
        /// The <see cref="AttributeValues" /> property should be used to get attribute values.  Dictionary key
        /// is the attribute key, and value is the cached attribute
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        [LavaIgnore]
        public Dictionary<string, Rock.Web.Cache.AttributeCache> Attributes
        {
            get
            {
                var attributes = new Dictionary<string, Rock.Web.Cache.AttributeCache>();

                foreach ( int id in AttributeIds.ToList() )
                {
                    Rock.Web.Cache.AttributeCache attribute = AttributeCache.Read( id );
                    attributes.Add( attribute.Key, attribute );
                }

                return attributes;
            }

            set
            {
                this.AttributeIds = new List<int>();
                if ( value != null )
                {
                    foreach ( var attribute in value )
                    {
                        this.AttributeIds.Add( attribute.Value.Id );
                    }
                }
            }
        }
        /// <summary>
        /// The attribute ids
        /// </summary>
        [DataMember]
        [LavaIgnore]
        protected List<int> AttributeIds = new List<int>();

        /// <summary>
        /// Dictionary of all attributes and their value.
        /// </summary>
        [DataMember]
        [LavaIgnore]
        public virtual Dictionary<string, Rock.Model.AttributeValue> AttributeValues { get; set; }

        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        [LavaIgnore]
        public virtual Dictionary<string, string> AttributeValueDefaults
        {
            get { return null; }
        }

        /// <summary>
        /// Saves the attribute values.
        /// </summary>
        public virtual void SaveAttributeValues()
        {
            var rockContext = new Rock.Data.RockContext();
            var service = new Rock.Data.Service<T>( rockContext );
            var model = service.Get( this.Id );

            if ( model != null )
            {
                model.LoadAttributes();
                foreach ( var attribute in model.Attributes )
                {
                    if ( this.AttributeValues.ContainsKey( attribute.Key ) )
                    {
                        Rock.Attribute.Helper.SaveAttributeValue( model, attribute.Value, this.AttributeValues[attribute.Key].Value, rockContext );
                    }
                }
            }
        }

        /// <summary>
        /// Gets the value of an attribute key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The stored value as a string or null if none exists.</returns>
        public string GetAttributeValue( string key )
        {
            if ( this.AttributeValues != null &&
                this.AttributeValues.ContainsKey( key ) )
            {
                return this.AttributeValues[key].Value;
            }

            if ( this.Attributes != null &&
                this.Attributes.ContainsKey( key ) )
            {
                return this.Attributes[key].DefaultValue;
            }

            return null;
        }

        /// <summary>
        /// Gets the value of an attribute key - splitting that delimited value into a list of strings.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>A list of strings or an empty list if none exists.</returns>
        public List<string> GetAttributeValues( string key )
        {
            string value = GetAttributeValue( key );
            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                return value.SplitDelimitedValues().ToList();
            }

            return new List<string>();
        }

        /// <summary>
        /// Sets the value of an attribute key in memory. Once values have been set, use the <see cref="SaveAttributeValues()" /> method to save all values to database
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void SetAttributeValue( string key, string value )
        {
            if ( this.AttributeValues != null )
            {
                if ( this.AttributeValues.ContainsKey( key ) )
                {
                    this.AttributeValues[key].Value = value;
                }
                else if ( this.Attributes.ContainsKey( key ) )
                {
                    var attributeValue = new AttributeValue();
                    attributeValue.AttributeId = this.Attributes[key].Id;
                    attributeValue.Value = value;
                    this.AttributeValues.Add( key, attributeValue );
                }
            }
        }

        /// <summary>
        /// Reloads the attribute values.
        /// </summary>
        public virtual void ReloadAttributeValues()
        {
            using ( var rockContext = new RockContext() )
            {
                var service = new Rock.Data.Service<T>( rockContext );
                var model = service.Get( this.Id );

                if ( model != null )
                {
                    model.LoadAttributes( rockContext );

                    this.AttributeValues = model.AttributeValues;
                    this.Attributes = model.Attributes;
                }
            }
        }

        #endregion

        #region ILiquidizable Implementation

        /// <summary>
        /// To the liquid.
        /// </summary>
        /// <returns></returns>
        public object ToLiquid()
        {
            return this;
        }

        /// <summary>
        /// Gets the available keys (for debuging info).
        /// </summary>
        /// <value>
        /// The available keys.
        /// </value>
        [LavaIgnore]
        public virtual List<string> AvailableKeys
        {
            get
            {
                var availableKeys = new List<string>();

                foreach ( var propInfo in GetType().GetProperties() )
                {
                    if ( propInfo != null && propInfo.GetCustomAttributes( typeof( Rock.Data.LavaIgnoreAttribute ) ).Count() <= 0 )
                    {
                        availableKeys.Add( propInfo.Name );
                    }
                }

                return availableKeys;
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
        [LavaIgnore]
        public virtual object this[object key]
        {
            get
            {
                string keyString = key.ToStringSafe();

                if ( keyString == "AttributeValues" )
                {
                    return AttributeValues.Select( a => a.Value ).ToList();
                }

                var propInfo = GetType().GetProperty( key.ToStringSafe() );
                if ( propInfo != null && propInfo.GetCustomAttributes( typeof( Rock.Data.LavaIgnoreAttribute ) ).Count() <= 0 )
                {
                    object propValue = propInfo.GetValue( this, null );
                    if ( propValue is Guid )
                    {
                        return ( (Guid)propValue ).ToString();
                    }
                    else
                    {
                        return propValue;
                    }
                }

                // The remainder of this method is only neccessary to support the old way of getting attribute 
                // values in liquid templates (e.g. {{ Person.BaptismData }} ).  Once support for this method is 
                // deprecated ( in v4.0 ), and only the new method of using the Attribute filter is 
                // suported (e.g. {{ Person | Attribute:'BaptismDate' }} ), the remainder of this method 
                // can be removed

                if ( this.Attributes != null )
                {
                    bool unformatted = false;
                    bool url = false;

                    string attributeKey = key.ToStringSafe();
                    if ( attributeKey.EndsWith( "_unformatted" ) )
                    {
                        attributeKey = attributeKey.Replace( "_unformatted", "" );
                        unformatted = true;
                    }
                    else if ( attributeKey.EndsWith( "_url" ) )
                    {
                        attributeKey = attributeKey.Replace( "_url", "" );
                        url = true;
                    }

                    if ( this.Attributes.ContainsKey( attributeKey ) )
                    {
                        var attribute = this.Attributes[attributeKey];
                        if ( attribute.IsAuthorized( Authorization.VIEW, null ) )
                        {
                            var field = attribute.FieldType.Field;
                            string value = GetAttributeValue( attribute.Key );

                            if ( unformatted )
                            {
                                return value;
                            }

                            if ( url && field is Rock.Field.ILinkableFieldType )
                            {
                                return ( (Rock.Field.ILinkableFieldType)field ).UrlLink( value, attribute.QualifierValues );
                            }

                            return field.FormatValue( null, value, attribute.QualifierValues, false );
                        }
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public virtual bool ContainsKey( object key )
        {
            string attributeKey = key.ToStringSafe();

            if ( attributeKey == "AttributeValues" )
            {
                return true;
            }

            var propInfo = GetType().GetProperty( key.ToStringSafe() );
            if ( propInfo != null && propInfo.GetCustomAttributes( typeof( Rock.Data.LavaIgnoreAttribute ) ).Count() <= 0 )
            {
                return true;
            }

            if ( this.Attributes == null )
            {
                this.LoadAttributes();
            }

            if ( attributeKey.EndsWith( "_unformatted" ) )
            {
                attributeKey = attributeKey.Replace( "_unformatted", "" );
            }
            else if ( attributeKey.EndsWith( "_url" ) )
            {
                attributeKey = attributeKey.Replace( "_url", "" );
            }

            if ( this.Attributes != null && this.Attributes.ContainsKey( attributeKey ) )
            {
                var attribute = this.Attributes[attributeKey];
                if ( attribute.IsAuthorized( Authorization.VIEW, null ) )
                {
                    return true;
                }
            }

            return false;

        }

        #endregion

    }
}