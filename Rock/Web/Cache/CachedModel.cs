//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

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
        /// A list of actions that this class supports.
        /// </summary>
        public virtual List<string> SupportedActions { get; private set; }

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

        #endregion

        #region IHasAttribute Implementation

        /// <summary>
        /// Dictionary of categorized attributes.  Key is the category name, and Value is list of attributes in the category
        /// </summary>
        /// <value>
        /// The attribute categories.
        /// </value>
        public Dictionary<string, List<string>> AttributeCategories
        {
            get
            {
                var attributes = new List<AttributeCache>();
                foreach ( int id in AttributeIds )
                {
                    attributes.Add( AttributeCache.Read( id ) );
                }

                var attributeCategories = new Dictionary<string, List<string>>();
                foreach ( var attribute in attributes.OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
                {
                    if ( !attributeCategories.ContainsKey( attribute.Category ) )
                        attributeCategories.Add( attribute.Category, new List<string>() );
                    attributeCategories[attribute.Category].Add( attribute.Key );
                }

                return attributeCategories;
            }

            set { }
        }

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

                foreach ( int id in AttributeIds )
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
        public Dictionary<string, List<Rock.Model.AttributeValue>> AttributeValues { get; set; }

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
        /// <param name="personId">The person id.</param>
        public virtual void SaveAttributeValues( int? personId )
        {
            var service = new Rock.Data.Service<T>();
            var model = service.Get( this.Id );

            if ( model != null )
            {
                model.LoadAttributes();
                foreach ( var attribute in model.Attributes )
                {
                    Rock.Attribute.Helper.SaveAttributeValues( model, attribute.Value, this.AttributeValues[attribute.Key], personId );
                }
            }
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
        /// Sets the first value of an attribute key.
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

        /// <summary>
        /// Reloads the attribute values.
        /// </summary>
        public virtual void ReloadAttributeValues()
        {
            var service = new Rock.Data.Service<T>();
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