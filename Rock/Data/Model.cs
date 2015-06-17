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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Data.Services;
using System.Runtime.Serialization;

using Rock.Attribute;
using Rock.Model;
using Rock.Security;

namespace Rock.Data
{
    /// <summary>
    /// Represents an entity that can be secured and have attributes. 
    /// </summary>
    [IgnoreProperties( new[] { "ParentAuthority", "SupportedActions", "AuthEntity", "AttributeValues" } )]
    [IgnoreModelErrors( new[] { "ParentAuthority", "AttributeValues" } )]
    [DataContract]
    public abstract class Model<T> : Entity<T>, IModel, ISecured, IHasAttributes
        where T : Model<T>, ISecured, new()
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the created date time.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        [DataMember]
        [IncludeForReporting]
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the modified date time.
        /// </summary>
        /// <value>
        /// The modified date time.
        /// </value>
        [DataMember]
        [IncludeForReporting]
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the created by person alias identifier.
        /// </summary>
        /// <value>
        /// The created by person alias identifier.
        /// </value>
        [DataMember]
        public int? CreatedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the modified by person alias identifier.
        /// </summary>
        /// <value>
        /// The modified by person alias identifier.
        /// </value>
        [DataMember]
        public int? ModifiedByPersonAliasId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the created by person alias.
        /// </summary>
        /// <value>
        /// The created by person alias.
        /// </value>
        public virtual PersonAlias CreatedByPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the modified by person alias.
        /// </summary>
        /// <value>
        /// The modified by person alias.
        /// </value>
        public virtual PersonAlias ModifiedByPersonAlias { get; set; }

        /// <summary>
        /// Gets the created by person identifier.
        /// </summary>
        /// <value>
        /// The created by person identifier.
        /// </value>
        [LavaInclude]
        public virtual int? CreatedByPersonId
        {
            get
            {
                if (CreatedByPersonAlias != null)
                {
                    return CreatedByPersonAlias.PersonId;
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the name of the created by person.
        /// </summary>
        /// <value>
        /// The name of the created by person.
        /// </value>
        [LavaInclude]
        public virtual string CreatedByPersonName
        {
            get
            {
                if ( CreatedByPersonAlias != null && CreatedByPersonAlias.Person != null )
                {
                    return CreatedByPersonAlias.Person.FullName;
                }
                return string.Empty;
            }
        }
        /// <summary>
        /// Gets the modified by person identifier.
        /// </summary>
        /// <value>
        /// The modified by person identifier.
        /// </value>
        [LavaInclude]
        public virtual int? ModifiedByPersonId
        {
            get
            {
                if ( ModifiedByPersonAlias != null )
                {
                    return ModifiedByPersonAlias.PersonId;
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the name of the modified by person.
        /// </summary>
        /// <value>
        /// The name of the modified by person.
        /// </value>
        [LavaInclude]
        public virtual string ModifiedByPersonName
        {
            get
            {
                if ( ModifiedByPersonAlias != null && ModifiedByPersonAlias.Person != null )
                {
                    return ModifiedByPersonAlias.Person.FullName;
                }
                return string.Empty;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Method that will be called on an entity immediately before the item is saved by context
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="state"></param>
        public virtual void PreSaveChanges(  Rock.Data.DbContext dbContext, System.Data.Entity.EntityState state )
        {
        }

        /// <summary>
        /// Method that will be called on an entity immediately before the item is saved by context
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="entry"></param>
        public virtual void PreSaveChanges( Rock.Data.DbContext dbContext, System.Data.Entity.Infrastructure.DbEntityEntry entry )
        {
            PreSaveChanges( dbContext, entry.State );
        }

        #endregion

        #region ISecured implementation

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check the default authorization on the current type, and 
        /// then the authorization on the Rock.Security.GlobalDefault entity
        /// </summary>
        [NotMapped]
        public virtual Security.ISecured ParentAuthority
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
        public virtual Security.ISecured ParentAuthorityPre
        {
            get { return null; }
        }

        /// <summary>
        /// A dictionary of actions that this class supports and the description of each.
        /// </summary>
        [NotMapped]
        public virtual Dictionary<string, string> SupportedActions
        {
            get
            {
                if ( _supportedActions == null )
                {
                    _supportedActions = new Dictionary<string, string>();
                    _supportedActions.Add( Authorization.VIEW, "The roles and/or users that have access to view." );
                    _supportedActions.Add( Authorization.EDIT, "The roles and/or users that have access to edit." );
                    _supportedActions.Add( Authorization.ADMINISTRATE, "The roles and/or users that have access to administrate." );
                }
                return _supportedActions;
            }
        }
        private Dictionary<string, string> _supportedActions;


        /// <summary>
        /// Return <c>true</c> if the user is authorized to perform the selected action on this object.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        ///   <c>true</c> if the specified action is authorized; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsAuthorized( string action, Rock.Model.Person person )
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
            return Security.Authorization.IsPrivate( this, action, person  );
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
        /// If the action on the current entity is private, removes the auth rules that made it private.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        public virtual void MakeUnPrivate( string action, Person person, RockContext rockContext = null )
        {
            Security.Authorization.MakeUnPrivate( this, action, person, rockContext );
        }

        /// <summary>
        /// Adds an 'Allow' rule for the current person as the first rule for the selected action
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        public virtual void AllowPerson( string action, Person person, RockContext rockContext = null )
        {
            Security.Authorization.AllowPerson( this, action, person, rockContext );
        }

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <remarks>
        /// This method is only neccessary to support the old way of getting attribute values in 
        /// liquid templates (e.g. {{ Person.BaptismData }} ).  Once support for this method is 
        /// deprecated ( in v4.0 ), and only the new method of using the Attribute filter is 
        /// suported (e.g. {{ Person | Attribute:'BaptismDate' }} ), this method can be removed
        /// </remarks>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public override object this[object key]
        {
            get
            {
                string keyString = key.ToStringSafe();

                object item = base[key];
                if ( item == null )
                {
                    if (this.Attributes == null)
                    {
                        this.LoadAttributes();
                    }

                    if ( keyString == "AttributeValues" )
                    {
                        return AttributeValues.Select( a => a.Value ).ToList();
                    }

                    // The remainder of this method is only neccessary to support the old way of getting attribute 
                    // values in liquid templates (e.g. {{ Person.BaptismData }} ).  Once support for this method is 
                    // deprecated ( in v4.0 ), and only the new method of using the Attribute filter is 
                    // suported (e.g. {{ Person | Attribute:'BaptismDate' }} ), the remainder of this method 
                    // can be removed

                    if ( this.Attributes != null )
                    {
                        string attributeKey = keyString;
                        bool unformatted = false;
                        bool url = false;

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
                }

                return item;
            }
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <remarks>
        /// This method is only neccessary to support the old way of getting attribute values in 
        /// liquid templates (e.g. {{ Person.BaptismData }} ).  Once support for this method is 
        /// deprecated ( in v4.0 ), and only the new method of using the Attribute filter is 
        /// suported (e.g. {{ Person | Attribute:'BaptismDate' }} ), this method can be removed
        /// </remarks>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public override bool ContainsKey( object key )
        {
            string attributeKey = key.ToStringSafe();

            if ( attributeKey == "AttributeValues")
            {
                return true;
            }

            bool containsKey = base.ContainsKey( key );

            if ( !containsKey )
            {
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
            }

            return containsKey;
        }

        #endregion

        #region IHasAttributes implementation

        // Note: For complex/non-entity types, we'll need to decorate some classes with the IgnoreProperties attribute
        // to tell WCF Data Services not to worry about the associated properties.

        /// <summary>
        /// List of attributes associated with the object.  This property will not include the attribute values.
        /// The <see cref="AttributeValues"/> property should be used to get attribute values.  Dictionary key
        /// is the attribute key, and value is the cached attribute
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        [NotMapped]
        [DataMember]
        [LavaIgnore]
        public virtual Dictionary<string, Rock.Web.Cache.AttributeCache> Attributes { get; set; }

        /// <summary>
        /// Dictionary of all attributes and their value.  Key is the attribute key, and value is the associated attribute value
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        [NotMapped]
        [DataMember]
        [LavaIgnore]
        public virtual Dictionary<string, Rock.Model.AttributeValue> AttributeValues { get; set; }

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
        /// Gets the value of an attribute key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
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
        /// Sets the value of an attribute key in memory.  Note, this will not persist value to database
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void SetAttributeValue( string key, string value )
        {
            if ( this.AttributeValues != null &&
                this.AttributeValues.ContainsKey( key ) )
            {
                this.AttributeValues[key].Value = value;
            }
        }

        #endregion

    }

}
