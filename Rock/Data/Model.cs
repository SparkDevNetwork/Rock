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
    [IgnoreModelErrors( new[] { "ParentAuthority" } )]
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
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the modified date time.
        /// </summary>
        /// <value>
        /// The modified date time.
        /// </value>
        [DataMember]
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
        [DataMember]
        public virtual PersonAlias CreatedByPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the modified by person alias.
        /// </summary>
        /// <value>
        /// The modified by person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias ModifiedByPersonAlias { get; set; }

        /// <summary>
        /// Gets the created by person identifier.
        /// </summary>
        /// <value>
        /// The created by person identifier.
        /// </value>
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
        /// Gets the modified by person identifier.
        /// </summary>
        /// <value>
        /// The modified by person identifier.
        /// </value>
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
        /// A list of actions that this class supports.
        /// </summary>
        [NotMapped]
        public virtual List<string> SupportedActions
        {
            get { return _supportedActions; }
        }
        private List<string> _supportedActions = new List<string>() { "View", "Edit", "Administrate" };


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
            return action == "View";
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
        /// <param name="personId">The current person id.</param>
        public virtual void MakePrivate( string action, Person person, int? personId )
        {
            Security.Authorization.MakePrivate( this, action, person, personId );
        }

        /// <summary>
        /// If the action on the current entity is private, removes the auth rules that made it private.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="personId">The current person id.</param>
        public virtual void MakeUnPrivate( string action, Person person, int? personId )
        {
            Security.Authorization.MakeUnPrivate( this, action, person, personId );
        }

        /// <summary>
        /// Adds an 'Allow' rule for the current person as the first rule for the selected action
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="personId">The person identifier.</param>
        public virtual void AllowPerson( string action, Person person, int? personId )
        {
            Security.Authorization.AllowPerson( this, action, person, personId );
        }

        /// <summary>
        /// To the liquid.
        /// </summary>
        /// <returns></returns>
        public override object ToLiquid()
        {
            Dictionary<string, object> dictionary = base.ToLiquid() as Dictionary<string, object>;

            this.LoadAttributes();
            foreach ( var attribute in this.Attributes )
            {
                if (attribute.Value.IsAuthorized("View", null))
                {
                    dictionary.Add(attribute.Key, GetAttributeValue(attribute.Key));
                }
            }

            return dictionary;
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
        public virtual Dictionary<string, Rock.Web.Cache.AttributeCache> Attributes { get; set; }

        /// <summary>
        /// Dictionary of all attributes and their value.  Key is the attribute key, and value is the values
        /// associated with the attribute and object instance
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        [NotMapped]
        [DataMember]
        public virtual Dictionary<string, List<Rock.Model.AttributeValue>> AttributeValues { get; set; }

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
        /// Gets the first value of an attribute key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetAttributeValue( string key )
        {
            if ( this.AttributeValues != null &&
                this.AttributeValues.ContainsKey( key ) &&
                this.AttributeValues[key].Count > 0 )
            {
                return this.AttributeValues[key][0].Value;
            }
            return null;
        }

        /// <summary>
        /// Gets the first value of an attribute key - splitting that delimited value into a list of strings.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>A list of strings or an empty list if none exists.</returns>
        public List<string> GetAttributeValues( string key )
        {
            if ( this.AttributeValues != null &&
                this.AttributeValues.ContainsKey( key ) &&
                this.AttributeValues[key].Count > 0 )
            {
                return this.AttributeValues[key][0].Value.SplitDelimitedValues().ToList();
            }

            return new List<string>();
        }

        /// <summary>
        /// Sets the first value of an attribute key in memory.  Note, this will not persist value to database
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void SetAttributeValue( string key, string value )
        {
            if ( this.AttributeValues != null &&
                this.AttributeValues.ContainsKey( key ) )
            {
                if ( this.AttributeValues[key].Count == 0 )
                {
                    this.AttributeValues[key].Add( new AttributeValue() );
                }
                this.AttributeValues[key][0].Value = value;
            }
        }

        #endregion
    }

    //public partial class RockModelConfiguration<T> : EntityTypeConfiguration<T>
    //    where T : Model<T>, new()
    //{
    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="AuthConfiguration"/> class.
    //    /// </summary>
    //    public RockModelConfiguration()
    //    {
    //        this.HasOptional( m => m.CreatedByPersonAlias ).WithMany().HasForeignKey( m => m.CreatedByPersonAliasId).WillCascadeOnDelete( false );
    //        this.HasOptional( m => m.ModifiedByPersonAlias ).WithMany().HasForeignKey( m => m.ModifiedByPersonAliasId ).WillCascadeOnDelete( false );
    //    }
    //}
}
