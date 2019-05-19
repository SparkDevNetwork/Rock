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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Services;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using Rock.Attribute;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Data
{
    /// <summary>
    /// Represents an entity that can be secured and have attributes. 
    /// </summary>
    [IgnoreProperties( new[] { "ParentAuthority", "SupportedActions", "AuthEntity", "AttributeValues" } )]
    [IgnoreModelErrors( new[] { "ParentAuthority" } )]
    [DataContract]
    public abstract class Model<T> : Entity<T>, IModel, ISecured, IHasAttributes, IHasInheritedAttributes
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
        [RockClientInclude( "Leave this as NULL to let Rock set this" )]
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the modified date time.
        /// </summary>
        /// <value>
        /// The modified date time.
        /// </value>
        [DataMember]
        [RockClientInclude( "This does not need to be set or changed. Rock will always set this to the current date/time when saved to the database." )]
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the created by person alias identifier.
        /// </summary>
        /// <value>
        /// The created by person alias identifier.
        /// </value>
        [DataMember]
        [HideFromReporting]
        [RockClientInclude( "Leave this as NULL to let Rock set this" )]
        public int? CreatedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the modified by person alias identifier.
        /// </summary>
        /// <value>
        /// The modified by person alias identifier.
        /// </value>
        [DataMember]
        [HideFromReporting]
        [RockClientInclude( "If you need to set this manually, set ModifiedAuditValuesAlreadyUpdated=True to prevent Rock from setting it" )]
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
        [HideFromReporting]
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
        [HideFromReporting]
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
        [HideFromReporting]
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
        [HideFromReporting]
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

        /// <summary>
        /// Gets or sets a value indicating whether the ModifiedByPersonAliasId value has already been
        /// updated to reflect who/when model was updated. If this value is false (default) the framework will update
        /// the value with the current user when the model is saved. Set this value to true if this automatic
        /// update should not be done.
        /// </summary>
        /// <value>
        /// <c>false</c> if rock should set the ModifiedByPersonAliasId to current user when saving model; otherwise, <c>true</c>.
        /// </value>
        [NotMapped]
        [DataMember]
        [RockClientInclude("If the ModifiedByPersonAliasId is being set manually and should not be overwritten with current user when saved, set this value to true")]
        public virtual bool ModifiedAuditValuesAlreadyUpdated { get; set; }

        /// <summary>
        /// Gets or sets a field that can be used for custom sorting.
        /// </summary>
        /// <value>
        /// The sort value.
        /// </value>
        [NotMapped]
        public virtual object CustomSortValue { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Method that will be called on an entity immediately before the item is saved by context
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="state"></param>
        public virtual void PreSaveChanges(  Rock.Data.DbContext dbContext, EntityState state )
        {
        }

        /// <summary>
        /// Method that will be called on an entity immediately before the item is saved by context
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="entry"></param>
        public virtual void PreSaveChanges( Rock.Data.DbContext dbContext, DbEntityEntry entry )
        {
            PreSaveChanges( dbContext, entry.State );
        }

        /// <summary>
        /// Method that will be called on an entity immediately after the item is saved by context
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        /// <param name="state">The state.</param>
        public virtual void PreSaveChanges( Rock.Data.DbContext dbContext, DbEntityEntry entry, EntityState state )
        {
            PreSaveChanges( dbContext, entry );
        }

        /// <summary>
        /// Posts the save changes.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public virtual void PostSaveChanges( Rock.Data.DbContext dbContext )
        {
        }

        /// <summary>
        /// Gets the created audit HTML.
        /// </summary>
        /// <param name="rootUrl">The root URL.</param>
        /// <returns></returns>
        public virtual string GetCreatedAuditHtml( string rootUrl )
        {
            return GetAuditHtml( CreatedByPersonAlias, CreatedDateTime, rootUrl );
        }

        /// <summary>
        /// Gets the modified audit HTML.
        /// </summary>
        /// <param name="rootUrl">The root URL.</param>
        /// <returns></returns>
        public virtual string GetModifiedAuditHtml( string rootUrl )
        {
            return GetAuditHtml( ModifiedByPersonAlias, ModifiedDateTime, rootUrl );
        }

        /// <summary>
        /// Gets the audit HTML.
        /// </summary>
        /// <param name="personAlias">The person alias.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="rootUrl">The root URL.</param>
        /// <returns></returns>
        private string GetAuditHtml( PersonAlias personAlias, DateTime? dateTime, string rootUrl )
        {
            var sb = new StringBuilder();

            if ( personAlias != null &&
                personAlias.Person != null )
            {
                sb.AppendFormat( "<a href={0}Person/{1}>{2}</a>", rootUrl, personAlias.PersonId, personAlias.Person.FullName );

                if ( dateTime.HasValue )
                {
                    sb.AppendFormat( " <small class='js-date-rollover' data-toggle='tooltip' data-placement='top' title='{0}'>({1})</small>", dateTime.Value.ToString(), dateTime.Value.ToRelativeDateString() );
                }
            }

            return sb.ToString();
        }
        
        #endregion

        #region ISecured implementation

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check the default authorization on the current type, and 
        /// then the authorization on the Rock.Security.GlobalDefault entity
        /// </summary>
        [NotMapped]
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
        public virtual ISecured ParentAuthorityPre
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
            return Authorization.Authorized( this, action, person );
        }

        /// <summary>
        /// If a user or role is not specifically allowed or denied to perform the selected action,
        /// return <c>true</c> if they should be allowed anyway or <c>false</c> if not.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public virtual bool IsAllowedByDefault( string action )
        {
            // Model is the ultimate base Parent Authority of child classes of Models, so if Authorization wasn't specifically Denied until now, this is what all actions default to.
            // In the case of VIEW or TAG, we want to default to Allowed.
            return action == Authorization.VIEW || action == Authorization.TAG;
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
            return Authorization.IsPrivate( this, action, person  );
        }

        /// <summary>
        /// Makes the action on the current entity private (Only the current user will have access).
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        public virtual void MakePrivate( string action, Person person, RockContext rockContext = null )
        {
            Authorization.MakePrivate( this, action, person, rockContext );
        }

        /// <summary>
        /// If the action on the current entity is private, removes the auth rules that made it private.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        public virtual void MakeUnPrivate( string action, Person person, RockContext rockContext = null )
        {
            Authorization.MakeUnPrivate( this, action, person, rockContext );
        }

        /// <summary>
        /// Adds an 'Allow' rule for the current person as the first rule for the selected action
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        public virtual void AllowPerson( string action, Person person, RockContext rockContext = null )
        {
            Authorization.AllowPerson( this, action, person, rockContext );
        }

        /// <summary>
        /// Allows the security role.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="group">The group.</param>
        /// <param name="rockContext">The rock context.</param>
        public virtual void AllowSecurityRole( string action, Group group, RockContext rockContext = null )
        {
            Authorization.AllowSecurityRole( this, action, group, rockContext );
        }

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <remarks>
        /// This method is only necessary to support the old way of getting attribute values in 
        /// liquid templates (e.g. {{ Person.BaptismData }} ).  Once support for this method is 
        /// deprecated ( in v4.0 ), and only the new method of using the Attribute filter is 
        /// supported (e.g. {{ Person | Attribute:'BaptismDate' }} ), this method can be removed
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
                    var lavaSupportLevel = GlobalAttributesCache.Get().LavaSupportLevel; 
                    
                    if (this.Attributes == null)
                    {
                        this.LoadAttributes();
                    }

                    if ( keyString == "AttributeValues" )
                    {
                        return AttributeValues.Select( a => a.Value ).ToList();
                    }

                    // The remainder of this method is only necessary to support the old way of getting attribute 
                    // values in liquid templates (e.g. {{ Person.BaptismData }} ).  Once support for this method is 
                    // deprecated ( in v4.0 ), and only the new method of using the Attribute filter is 
                    // supported (e.g. {{ Person | Attribute:'BaptismDate' }} ), the remainder of this method 
                    // can be removed

                    if ( lavaSupportLevel == Lava.LavaSupportLevel.NoLegacy )
                    {
                        return null;
                    }
                    

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
                                if ( lavaSupportLevel == Lava.LavaSupportLevel.LegacyWithWarning )
                                {
                                    Rock.Model.ExceptionLogService.LogException( new Rock.Lava.LegacyLavaSyntaxDetectedException( this.GetType().GetFriendlyTypeName(), attributeKey ), System.Web.HttpContext.Current );
                                }

                                if ( unformatted )
                                {
                                    return GetAttributeValueAsType( attribute.Key );
                                }

                                var field = attribute.FieldType.Field;
                                string value = GetAttributeValue( attribute.Key );

                                if ( url && field is Rock.Field.ILinkableFieldType )
                                {
                                    return ( (Rock.Field.ILinkableFieldType)field ).UrlLink( value, attribute.QualifierValues );
                                }

                                return field.FormatValue( null, attribute.EntityTypeId, this.Id, value, attribute.QualifierValues, false );
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
        /// This method is only necessary to support the old way of getting attribute values in 
        /// liquid templates (e.g. {{ Person.BaptismData }} ).  Once support for this method is 
        /// deprecated ( in v4.0 ), and only the new method of using the Attribute filter is 
        /// supported (e.g. {{ Person | Attribute:'BaptismDate' }} ), this method can be removed
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
        public virtual Dictionary<string, AttributeCache> Attributes { get; set; }

        /// <summary>
        /// Dictionary of all attributes and their value.  Key is the attribute key, and value is the associated attribute value
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        [NotMapped]
        [DataMember]
        [LavaIgnore]
        public virtual Dictionary<string, AttributeValueCache> AttributeValues { get; set; }

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
        /// Gets the type of the attribute value as.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public object GetAttributeValueAsType( string key )
        {
            if ( this.AttributeValues != null &&
                this.AttributeValues.ContainsKey( key ) )
            {
                return this.AttributeValues[key].ValueAsType;
            }

            if ( this.Attributes != null &&
                this.Attributes.ContainsKey( key ) )
            {
                return this.Attributes[key].DefaultValueAsType;
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

        #region IHasInheritedAttributes implementation

        /// <summary>
        /// Get a list of all inherited Attributes that should be applied to this entity.
        /// </summary>
        /// <returns>A list of all inherited AttributeCache objects.</returns>
        public virtual List<AttributeCache> GetInheritedAttributes( Rock.Data.RockContext rockContext )
        {
            return null;
        }

        /// <summary>
        /// Get any alternate Ids that should be used when loading attribute value for this entity.
        /// </summary>
        /// <returns>A list of any alternate entity Ids that should be used when loading attribute values.</returns>
        public virtual List<int> GetAlternateEntityIds( Rock.Data.RockContext rockContext )
        {
            return null;
        }

        #endregion

    }

}
