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
    public abstract class CachedModel<T> : ISecured, Rock.Attribute.IHasAttributes
        where T : Rock.Data.Entity<T>, ISecured, Rock.Attribute.IHasAttributes, new()
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        [DataMember]
        public virtual int Id { get; set; }

        /// <summary>
        /// Gets or sets the GUID.
        /// </summary>
        /// <value>
        /// The GUID.
        /// </value>
        [DataMember]
        public virtual Guid Guid { get; set; }

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public virtual void CopyFromModel( Rock.Data.IEntity model )
        {
            this.Id = model.Id;
            this.Guid = model.Guid;

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
                }
                this.AttributeValues = attributeModel.AttributeValues;
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
        public virtual int TypeId { get; private set; }

        /// <summary>
        /// The auth entity. Classes that implement the <see cref="ISecured" /> interface should return
        /// a value that is unique across all <see cref="ISecured" /> classes.  Typically this is the
        /// qualified name of the class.
        /// </summary>
        [DataMember]
        public virtual string TypeName { get; private set; }

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check access to the parent authority specified by this property.
        /// </summary>
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
        /// A dictionary of actions that this class supports and the description of each.
        /// </summary>
        public virtual Dictionary<string, string> SupportedActions { get; private set; }

        /// <summary>
        /// Return <c>true</c> if the user is authorized to perform the selected action on this object.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        ///   <c>true</c> if the specified action is authorized; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsAuthorized( string action, Person person, RockContext rockContext = null )
        {
            return Security.Authorization.Authorized( this, action, person, rockContext );
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
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        ///   <c>true</c> if the specified action is private; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsPrivate( string action, Person person, RockContext rockContext = null )
        {
            return Security.Authorization.IsPrivate( this, action, person, rockContext );
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
        public virtual void MakeUnPrivate( string action, Person person, RockContext rockContext = null )
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
        protected List<int> AttributeIds = new List<int>();

        /// <summary>
        /// Dictionary of all attributes and their value.
        /// </summary>
        [DataMember]
        public Dictionary<string, Rock.Model.AttributeValue> AttributeValues { get; set; }

        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
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
            var service = new Rock.Data.Service<T>( new RockContext() );
            var model = service.Get( this.Id );

            if ( model != null )
            {
                model.LoadAttributes();

                this.AttributeValues = model.AttributeValues;
                this.Attributes = model.Attributes;
            }
        }

        #endregion
    }
}