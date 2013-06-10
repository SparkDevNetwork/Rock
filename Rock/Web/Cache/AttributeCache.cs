//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.Serialization;
using System.Web.UI;

using Rock.Field;
using Rock.Model;
using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about an attribute that is required by the rendering engine.
    /// This information will be cached by Rock. 
    /// 
    /// NOTE: Because this attribute object is cached and shared by all entities 
    /// using the attribute, a particlar instance's values are not included as a 
    /// property of this attribute object.
    /// </summary>
    [Serializable]
    [DataContract]
    public class AttributeCache : ISecured
    {
        #region constructors

        private AttributeCache() 
        {
        }

        private AttributeCache( Rock.Model.Attribute model )
        {
            CopyFromModel( model );
        }

        private AttributeCache( Rock.Model.Attribute model, Dictionary<string, string> qualifiers )
        {
            CopyFromModel( model, qualifiers );
        }

        #endregion

        #region Properties

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
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the field type id.
        /// </summary>
        /// <value>
        /// The field type id.
        /// </value>
        [DataMember]
        public int FieldTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity type id.
        /// </summary>
        /// <value>
        /// The entity type id.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier column.
        /// </summary>
        /// <value>
        /// The entity type qualifier column.
        /// </value>
        [DataMember]
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier value.
        /// </summary>
        /// <value>
        /// The entity type qualifier value.
        /// </value>
        [DataMember]
        public string EntityTypeQualifierValue { get; set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        [DataMember]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is grid column.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is grid column; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsGridColumn { get; set; }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        [DataMember]
        public string DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is multi value.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is multi value; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsMultiValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is required.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is required; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets the type of the field.
        /// </summary>
        /// <value>
        /// The type of the field.
        /// </value>
        [DataMember]
        public FieldTypeCache FieldType 
        {
            get { return FieldTypeCache.Read( FieldTypeId ); }
        }

        /// <summary>
        /// Gets the qualifier values if any have been defined for the attribute
        /// </summary>
        [DataMember]
        public Dictionary<string, ConfigurationValue> QualifierValues { get; private set; }

        /// <summary>
        /// Gets the categories.
        /// </summary>
        /// <value>
        /// The categories.
        /// </value>
        public List<CategoryCache> Categories
        {
            get
            {
                List<CategoryCache> categories = new List<CategoryCache>();

                if ( categoryIds != null )
                {
                    foreach ( int id in categoryIds.ToList() )
                    {
                        categories.Add( CategoryCache.Read( id ) );
                    }
                }

                return categories;
            }
        }
        private List<int> categoryIds = null;


        #endregion

        #region Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        public void CopyFromModel( Rock.Model.Attribute attribute )
        {
            var qualifiers = new Dictionary<string, string>();

            if ( attribute.AttributeQualifiers != null )
            {
                foreach ( Rock.Model.AttributeQualifier qualifier in attribute.AttributeQualifiers )
                {
                    qualifiers.Add( qualifier.Key, qualifier.Value );
                }
            }

            CopyFromModel( attribute, qualifiers );
        }

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="qualifiers">The qualifiers.</param>
        public void CopyFromModel( Rock.Model.Attribute attribute, Dictionary<string, string> qualifiers )
        {
            this.Id = attribute.Id;
            this.Guid = attribute.Guid;
            this.IsSystem = attribute.IsSystem;
            this.FieldTypeId = attribute.FieldTypeId;
            this.EntityTypeId = attribute.EntityTypeId;
            this.EntityTypeQualifierColumn = attribute.EntityTypeQualifierColumn;
            this.EntityTypeQualifierValue = attribute.EntityTypeQualifierValue;
            this.Key = attribute.Key;
            this.Name = attribute.Name;
            this.Description = attribute.Description;
            this.Order = attribute.Order;
            this.IsGridColumn = attribute.IsGridColumn;
            this.DefaultValue = attribute.DefaultValue;
            this.IsMultiValue = attribute.IsMultiValue;
            this.IsRequired = attribute.IsRequired;

            this.TypeId = attribute.TypeId;
            this.TypeName = attribute.TypeName;
            this.SupportedActions = attribute.SupportedActions;

            this.QualifierValues = new Dictionary<string, ConfigurationValue>();
            foreach ( var qualifier in qualifiers )
                this.QualifierValues.Add( qualifier.Key, new ConfigurationValue( qualifier.Value ) );

            this.categoryIds = attribute.Categories.Select( c => c.Id ).ToList();
        }

        /// <summary>
        /// Creates the control.
        /// </summary>
        /// <returns></returns>
        public Control CreateControl()
        {
            return CreateControl( string.Empty, false, false );
        }

        /// <summary>
        /// Creates a <see cref="System.Web.UI.Control"/> based on the attribute's field type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="setValue">if set to <c>true</c> set the control's value</param>
        /// <param name="setId">if set to <c>true</c> [set id].</param>
        /// <returns></returns>
        public Control CreateControl( string value, bool setValue, bool setId)
        {
            Control editControl = this.FieldType.Field.EditControl( QualifierValues);
            if ( setId )
            {
                editControl.ID = string.Format( "attribute_field_{0}", this.Id );
                editControl.ClientIDMode = ClientIDMode.AutoID;
            }

            if ( setValue )
            {
                this.FieldType.Field.SetEditValue( editControl, QualifierValues, value );
            }

            return editControl;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

        #region ISecured implementation

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
                    return new AttributeCache();
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

        #region Static Methods

        private static string CacheKey( int id )
        {
            return string.Format( "Rock:Attribute:{0}", id );
        }

        /// <summary>
        /// Returns Attribute object from cache.  If attribute does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The id of the Attribute to read</param>
        /// <returns></returns>
        public static AttributeCache Read( int id )
        {
            string cacheKey = AttributeCache.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            AttributeCache attribute = cache[cacheKey] as AttributeCache;

            if ( attribute != null )
            {
                return attribute;
            }
            else
            {
                var attributeService = new Rock.Model.AttributeService();
                var attributeModel = attributeService.Get( id );
                if ( attributeModel != null )
                {
                    attribute = new AttributeCache( attributeModel );
                    cache.Set( cacheKey, attribute, new CacheItemPolicy() );
                    return attribute;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Adds Attribute model to cache, and returns cached object
        /// </summary>
        /// <param name="attributeModel">The attributeModel to cache</param>
        /// <returns></returns>
        public static AttributeCache Read( Rock.Model.Attribute attributeModel )
        {
            string cacheKey = AttributeCache.CacheKey( attributeModel.Id );

            ObjectCache cache = MemoryCache.Default;
            AttributeCache attribute = cache[cacheKey] as AttributeCache;

            if ( attribute != null )
            {
                return attribute;
            }
            else
            {
                attribute = new AttributeCache( attributeModel );
                cache.Set( cacheKey, attribute, new CacheItemPolicy() );
                return attribute;
            }
        }

        /// <summary>
        /// Adds Attribute model to cache, and returns cached object.  
        /// </summary>
        /// <param name="attributeModel">The attribute model.</param>
        /// <param name="qualifiers">The qualifiers.</param>
        /// <returns></returns>
        public static AttributeCache Read( Rock.Model.Attribute attributeModel, Dictionary<string, string> qualifiers )
        {
            AttributeCache attribute = new AttributeCache( attributeModel, qualifiers );

            string cacheKey = AttributeCache.CacheKey( attributeModel.Id );

            ObjectCache cache = MemoryCache.Default;
            cache.Set( cacheKey, attribute, new CacheItemPolicy() );

            return attribute;
        }

        /// <summary>
        /// Removes attribute from cache
        /// </summary>
        /// <param name="id">The id of the attribute to remove from cache</param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( AttributeCache.CacheKey( id ) );
        }

        #endregion


    }
}