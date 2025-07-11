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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Runtime.Serialization;

using Newtonsoft.Json;
using Rock.Lava;
using Rock.Tasks;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Data
{
    /// <summary>
    /// Represents an entity object and is the base class for all model objects to inherit from
    /// </summary>
    /// <typeparam name="T">The Type entity that is being referenced <example>Entity&lt;Person&gt;</example></typeparam>
    [DataContract]
    public abstract class Entity<T> : IEntity, ILavaDataDictionary
        where T : Entity<T>, new()
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the value of the identifier.  This value is the primary field/key for the entity object.  This value is system and database
        /// dependent, and is not guaranteed to be unique. This id should only be used to identify an object internally to a single implementation
        /// of Rock since this value has a very high probability of not being consistent in an external implementation of Rock.
        /// </summary>
        /// <value>
        /// Primary and system dependent <see cref="System.Int32" /> based identity/key of an entity object in Rock.
        /// </value>
        [Key]
        [DataMember]
        [IncludeForReporting]
        public int Id { get; set; }

        /// <summary>
        /// Gets the <see cref="Id"/> as a hashed identifier that can be used
        /// to lookup the entity later. This hides the actual <see cref="Id"/>
        /// number so that individuals cannot attempt to guess the next sequential
        /// identifier numbers.
        /// </summary>
        /// <value>The hashed identifier key or an empty string if it could not be determined.</value>
        [DataMember]
        [NotMapped]
        public string IdKey
        {
            get
            {
                try
                {
                    return Id != 0 ? IdHasher.Instance.GetHash( Id ) : string.Empty;
                }
                catch
                {
                    return string.Empty;
                }
            }
            private set { /* Make DataContract happy. */ }
        }

        /// <summary>
        /// Gets or sets a <see cref="System.Guid"/> value that is a guaranteed unique identifier for the entity object.  This value
        /// is an alternate key for the object, and should be used when interacting with external systems and when comparing and synchronizing
        /// objects across across data stores or external /implementations of Rock
        /// </summary>
        /// <remarks>
        /// A good place for a Guid to be used is when comparing or syncing data across two implementations of Rock. For example, if you
        /// were creating a <see cref="Rock.Web.UI.RockBlock"/> with a data migration that adds/remove a new defined value object to the database. You would want to
        /// search based on the Guid because it would be guaranteed to be unique across all implementations of Rock.
        /// </remarks>
        /// <value>
        /// A <see cref="System.Guid"/> value that will uniquely identify the entity/object across all implementations of Rock.
        /// </value>
        [Index( IsUnique = true )]
        [DataMember]
        [IncludeForReporting]
        [NotEmptyGuidAttribute]
        public Guid Guid
        {
            get { return _guid; }
            set
            {
                _guid = value;
            }
        }
        private Guid _guid = Guid.NewGuid();

        /// <summary>
        /// Gets or sets an optional int foreign identifier.  This can be used for importing or syncing data to a foreign system
        /// </summary>
        /// <value>
        /// The foreign identifier.
        /// </value>
        [DataMember]
        [HideFromReporting]
        public int? ForeignId { get; set; }

        /// <summary>
        /// Gets or sets an optional Guid foreign identifier.  This can be used for importing or syncing data to a foreign system
        /// </summary>
        /// <value>
        /// The foreign identifier.
        /// </value>
        [DataMember]
        [HideFromReporting]
        public Guid? ForeignGuid { get; set; }

        /// <summary>
        /// Gets or sets an optional string foreign identifier.  This can be used for importing or syncing data to a foreign system
        /// </summary>
        /// <value>
        /// The foreign identifier.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        [HideFromReporting]
        public string ForeignKey { get; set; }

        #endregion

        #region Virtual Properties
        
        private int? _typeId = null;

        /// <summary>
        /// Gets the <see cref="Rock.Model.EntityType"/> Id for the Entity object type in Rock. If an <see cref="Rock.Model.EntityType"/> is not found
        /// for the object type it will be created
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> that represents the identifier for the current Entity object type.
        /// </value>
        [LavaVisible]
        public virtual int TypeId
        {
            get
            {
                if ( _typeId == null )
                {
                    // Once this instance is created, there is no need to set the _typeId more than once.
                    // Also, read should never return null since it will create entity type if it doesn't exist.
                    _typeId = EntityTypeCache.Get( typeof( T ) ).Id;
                }
                
                return _typeId.Value;
            }
        }

        private string _typeName = null;

        /// <summary>
        /// Gets the unique type name of the entity.  Typically this is the qualified name of the class
        /// </summary>
        /// <value>
        /// The name of the entity type.
        /// </value>
        [NotMapped]
        [LavaVisible]
        public virtual string TypeName
        {
            get
            {
                if ( _typeName.IsNullOrWhiteSpace() )
                {
                    // Once this instance is created, there is no need to set the _typeName more than once.
                    // Also, read should never return null since it will create entity type if it doesn't exist.
                    _typeName = typeof( T ).FullName;
                }

                return _typeName;
            }
        }

        /// <summary>
        /// Gets an URLEncoded encrypted string/key that represents the entity object
        /// </summary>
        /// <value>
        /// An encrypted <see cref="System.String"/> that represents the entity object.
        /// </value>
        [NotMapped]
        public virtual string ContextKey
        {
            get
            {
                string identifier =
                    TypeName + "|" +
                    this.Id.ToString() + ">" +
                    this.Guid.ToString();
                return System.Web.HttpUtility.UrlEncode( Rock.Security.Encryption.EncryptString( identifier ) );
            }
        }

        /// <summary>
        /// Gets the validation results for the entity. This is initialized by calling IsValid
        /// </summary>
        [NotMapped]
        public virtual List<ValidationResult> ValidationResults
        {
            get { return _validationResults; }
        }
        private List<ValidationResult> _validationResults;

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   A <see cref="System.Boolean"/> that is <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        [NotMapped]
        public virtual bool IsValid
        {
            get
            {
                var valContext = new ValidationContext( this, serviceProvider: null, items: null );
                _validationResults = new List<ValidationResult>();
                return Validator.TryValidateObject( this, valContext, _validationResults, true );
            }
        }

        /// <summary>
        /// Gets a publicly viewable unique key for the entity.
        /// NOTE: Will result in an empty string in a non-web app.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents a viewable version of the entity's unique key.
        /// </value>
        [NotMapped]
        public virtual string EncryptedKey
        {
            get
            {
                string identifier = this.Id.ToString() + ">" + this.Guid.ToString();
                string result = string.Empty;

                //// Non-web apps might not have the dataencryptionkey
                //// so just return empty string if we can't encrypt

                if ( Rock.Security.Encryption.TryEncryptString( identifier, out result ) )
                {
                    return result;
                }
                else
                {
                    return string.Empty; ;
                }
            }
        }

        /// <summary>
        /// Gets a URL friendly version of the EncryptedKey for the entity.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents a URL friendly version of the entity's unique key.
        /// </value>
        [NotMapped]
        [LavaVisible]
        public virtual string UrlEncodedKey
        {
            get
            {
                string encodedKey = System.Web.HttpUtility.UrlEncode( EncryptedKey );
                return encodedKey.Replace( '%', '!' );
            }
            private set { }
        }

        /// <summary>
        /// Gets the entity string value.
        /// </summary>
        /// <value>
        /// The entity string value.
        /// </value>
        [NotMapped]
        [LavaVisible]
        public virtual string EntityStringValue
        {
            get
            {
                return this.ToStringSafe();
            }
        }

        #endregion

        #region Static Properties

        /// <summary>
        /// Gets the entity object type's friendly name
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the entity object type's friendly name.
        /// </value>
        [NotMapped]
        public static string FriendlyTypeName
        {
            get
            {
                var type = typeof( T );
                return type.GetFriendlyTypeName();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a deep copy of this entity object, including all child objects.
        /// </summary>
        /// <returns>
        /// A complete copy of the entity object
        /// </returns>
        public virtual IEntity Clone()
        {
            var json = this.ToJson();
            return FromJson( json );
        }

        /// <summary>
        /// Creates a dictionary containing the majority of the entity object's properties. The only properties that are excluded
        /// are the Id, Guid and Order.
        /// </summary>
        /// <returns>A <see cref="Dictionary{String, Object}"/> that represents the current entity object. Each <see cref="KeyValuePair{String, Object}"/> includes the property
        /// name as the key and the property value as the value.</returns>
        public virtual Dictionary<string, object> ToDictionary()
        {
            var dictionary = new Dictionary<string, object>();
            HashSet<string> virtualPropsWhiteList = new HashSet<string>
            {
                "Id",
                "Guid",
                "ForeignId",
                "ForeignKey",
                "ForeignGuid",
                "Order",
                "IsActive",
                "CreatedByPersonAliasId",
                "CreatedDateTime",
                "ModifiedByPersonAliasId",
                "ModifiedDateTime"
            };

            foreach ( var propInfo in this.GetType().GetProperties() )
            {
                if ( ( propInfo.GetGetMethod() != null && !propInfo.GetGetMethod().IsVirtual ) || virtualPropsWhiteList.Contains(propInfo.Name) )
                {
                    dictionary.Add( propInfo.Name, propInfo.GetValue( this, null ) );
                }
            }

            return dictionary;
        }

        /// <summary>
        /// Populates the current entity object with the contents of the current entity object.
        /// </summary>
        /// <param name="properties">A <see cref="System.Collections.Generic.Dictionary{String, Object}" /> that contains <see cref="System.Collections.Generic.KeyValuePair{String, Object}">KeyValuePairs</see>
        /// of representing properties.</param>
        public virtual void FromDictionary( Dictionary<string, object> properties )
        {
            Type type = this.GetType();

            foreach ( var property in properties )
            {
                var propInfo = type.GetProperty( property.Key );
                if ( propInfo != null )
                {
                    propInfo.SetValue( this, property.Value );
                }
            }
        }

        /// <summary>
        /// Gets the available keys (for debugging info).
        /// </summary>
        /// <value>
        /// The available keys.
        /// </value>
        [LavaHidden]
        public virtual List<string> AvailableKeys
        {
            get
            {
                var availableKeys = new List<string>();

                foreach ( var propInfo in GetBaseType().GetProperties() )
                {
                    if ( propInfo != null && LiquidizableProperty( propInfo ) )
                    {
                        availableKeys.Add( propInfo.Name );
                    }
                }

                if ( this.AdditionalLavaFields != null )
                {
                    foreach ( var field in AdditionalLavaFields.Keys )
                    {
                        if ( !availableKeys.Contains( field ) )
                        {
                            availableKeys.Add( field );
                        }
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
        public object GetValue( string key )
        {
            return OnGetValue( key );
        }

        /// <summary>
        /// Override this method to return a value for a key that does not correspond to the name of a declared property,
        /// or to return an alternate value for an existing property.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual object OnGetValue( string key )
        {
            return this[key];
        }

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        [LavaHidden]
        public virtual object this[object key]
        {
            get
            {
                string propertyKey = key.ToStringSafe();
                var propInfo = GetBaseType().GetProperty( propertyKey );

                try
                {
                    object propValue = null;
                    if ( propInfo != null && LiquidizableProperty( propInfo ) )
                    {
                        propValue = propInfo.GetValue( this, null );
                    }
                    else if ( this.AdditionalLavaFields != null && this.AdditionalLavaFields.ContainsKey( propertyKey ) )
                    {
                        propValue = this.AdditionalLavaFields[propertyKey];
                    }

                    if ( propValue is Guid )
                    {
                        return ( ( Guid ) propValue ).ToString();
                    }
                    else
                    {
                        return propValue;
                    }
                }
                catch
                {
                    // intentionally ignore
                }

                return null;
            }
        }

        /// <summary>
        /// Gets or sets the additional fields that can be added as fields to this object for Lava
        /// </summary>
        /// <value>
        /// The additional Lava fields.
        /// </value>
        //[LavaIgnore]
        [LavaHidden]
        public virtual Dictionary<string, object> AdditionalLavaFields { get; set; }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public virtual bool ContainsKey( string key )
        {
            string propertyKey = key.ToStringSafe();
            var propInfo = GetBaseType().GetProperty( propertyKey );
            if ( propInfo != null && LiquidizableProperty( propInfo ) )
            {
                return true;
            }
            else if ( this.AdditionalLavaFields != null && this.AdditionalLavaFields.ContainsKey( propertyKey ) )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the type of the base.
        /// </summary>
        /// <returns></returns>
        private Type GetBaseType()
        {
            Type entityType = this.GetType();
            if ( entityType.IsDynamicProxyType() )
            {
                entityType = entityType.BaseType;
            }

            return entityType;
        }

        /// <summary>
        /// Determines whether the property is available to Lava
        /// </summary>
        /// <param name="propInfo">The property information.</param>
        /// <returns></returns>
        private bool LiquidizableProperty( PropertyInfo propInfo )
        {
            return Rock.Lava.LavaHelper.IsLavaProperty( propInfo );
        }

        /// <summary>
        /// Creates a transaction to launch a workflow for this entity.
        /// </summary>
        /// <param name="workflowTypeGuid">The workflow type unique identifier.</param>
        /// <param name="workflowName">Name of the workflow.</param>
        /// <param name="workflowAttributeValues">The workflow attribute values.</param>
        [Obsolete( "Use the override that does not provide the default values instead." )]
        [RockObsolete( "1.13" )]
        public void LaunchWorkflow( Guid? workflowTypeGuid, string workflowName = "", Dictionary<string, string> workflowAttributeValues = null )
        {
            LaunchWorkflow( workflowTypeGuid, workflowName, workflowAttributeValues, null );
        }

        /// <summary>
        /// Creates a transaction to launch a workflow for this entity.
        /// </summary>
        /// <param name="workflowTypeGuid">The workflow type unique identifier.</param>
        /// <param name="workflowName">Name of the workflow.</param>
        /// <param name="workflowAttributeValues">Any workflow attribute values that should be set.</param>
        /// <param name="initiatorPersonAliasId">The Initiator Person Alias Identifier.</param>
        public void LaunchWorkflow( Guid? workflowTypeGuid, string workflowName, Dictionary<string, string> workflowAttributeValues, int? initiatorPersonAliasId )
        {
            if ( workflowTypeGuid.HasValue )
            {
                var transaction = new Rock.Transactions.LaunchWorkflowTransaction<T>( workflowTypeGuid.Value, workflowName, Id );
                if ( workflowAttributeValues != null )
                {
                    transaction.WorkflowAttributeValues = workflowAttributeValues;
                }
                transaction.InitiatorPersonAliasId = initiatorPersonAliasId;

                transaction.Enqueue();
            }
        }

        /// <summary>
        /// Creates a transaction to launch a workflow for this entity.
        /// </summary>
        /// <param name="workflowTypeId">The workflow type identifier.</param>
        /// <param name="workflowName">Name of the workflow.</param>
        /// <param name="workflowAttributeValues">The workflow attribute values.</param>
        [Obsolete( "Use the override that does not provide the default values instead." )]
        [RockObsolete( "1.13" )]
        public void LaunchWorkflow( int? workflowTypeId, string workflowName = "", Dictionary<string, string> workflowAttributeValues = null )
        {
            LaunchWorkflow( workflowTypeId, workflowName, workflowAttributeValues, null );
        }

        /// <summary>
        /// Creates a transaction to launch a workflow for this entity.
        /// </summary>
        /// <param name="workflowTypeId">The workflow type identifier.</param>
        /// <param name="workflowName">Name of the workflow.</param>
        /// <param name="workflowAttributeValues">Any workflow attribute values that should be set.</param>
        /// <param name="initiatorPersonAliasId">The Initiator Person Alias Identifier.</param>
        public void LaunchWorkflow( int? workflowTypeId, string workflowName, Dictionary<string, string> workflowAttributeValues, int? initiatorPersonAliasId )
        {
            if ( workflowTypeId.HasValue )
            {
                var transaction = new Rock.Transactions.LaunchWorkflowTransaction<T>( workflowTypeId.Value, workflowName, Id );
                if ( workflowAttributeValues != null )
                {
                    transaction.WorkflowAttributeValues = workflowAttributeValues;
                }
                transaction.InitiatorPersonAliasId = initiatorPersonAliasId;

                transaction.Enqueue();
            }
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Returns an instance of the  entity based on a JSON representation of the entity object.
        /// </summary>
        /// <param name="json">A <see cref="System.String"/> containing a JSON formatted representation of the object.</param>
        /// <returns>An instance of the entity object based on the provided JSON string.</returns>
        [System.Diagnostics.DebuggerStepThrough()]
        public static T FromJson( string json )
        {
            return JsonConvert.DeserializeObject( json, typeof( T ) ) as T;
        }

        /// <summary>
        /// Returns a list instance of the entity based on a JSON representation of a list of the entity object.
        /// </summary>
        /// <param name="json">A <see cref="System.String"/> containing a JSON formatted representation of a list of the object.</param>
        /// <returns>A list of the entity object based on the provided JSON string.</returns>
        [System.Diagnostics.DebuggerStepThrough()]
        public static List<T> FromJsonAsList( string json )
        {
            return JsonConvert.DeserializeObject<List<T>>( json ); ;
        }

        /// <summary>
        /// Gets the index result template.
        /// </summary>
        /// <value>
        /// The index result template.
        /// </value>
        public static string GetIndexResultTemplate()
        {
            return EntityTypeCache.Get( typeof( T ) ).IndexResultTemplate;
        }

        #endregion

        #region ILiquidizable

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        [Obsolete("Use ContainsKey(string) instead.")]
        [RockObsolete( "1.13.0" )]
        public virtual bool ContainsKey( object key )
        {
            string propertyKey = key.ToStringSafe();
            var propInfo = GetBaseType().GetProperty( propertyKey );
            if ( propInfo != null && LiquidizableProperty( propInfo ) )
            {
                return true;
            }
            else if ( this.AdditionalLavaFields != null && this.AdditionalLavaFields.ContainsKey( propertyKey ) )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Creates a DotLiquid compatible dictionary that represents the current entity object.
        /// </summary>
        /// <returns>DotLiquid compatible dictionary.</returns>
        public object ToLiquid()
        {
            return this;
        }

        #endregion
    }

    #region KeyEntity

    /// <summary>
    /// Object used for current model (context) implementation
    /// </summary>
    internal class KeyEntity
    {
        public string Key { get; set; }

        public int? Id { get; set; }

        public Guid? Guid { get; set; }

        public IEntity Entity { get; set; }

        public KeyEntity( int id )
        {
            Id = id;
        }

        public KeyEntity( Guid guid )
        {
            Guid = guid;
        }

        public KeyEntity( string key )
        {
            Key = key;
        }
    }

    #endregion

}