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
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock.Data;
using Rock.Field;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI.Controls;

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
    [JsonConverter( typeof( Rock.Utility.AttributeCacheJsonConverter ) )]
    public class AttributeCache : ISecured, Lava.ILiquidizable
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
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [DataMember]
        public string IconCssClass { get; set; }

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

        /// <summary>
        /// Gets or sets the category ids.
        /// </summary>
        /// <value>
        /// The category ids.
        /// </value>
        [DataMember]
        public List<int> CategoryIds
        {
            get { return categoryIds; }
            set { categoryIds = value; }
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
            this.IconCssClass = attribute.IconCssClass;
            this.IsGridColumn = attribute.IsGridColumn;
            this.DefaultValue = attribute.DefaultValue;
            this.IsMultiValue = attribute.IsMultiValue;
            this.IsRequired = attribute.IsRequired;

            this.TypeId = attribute.TypeId;
            this.TypeName = attribute.TypeName;
            this.SupportedActions = attribute.SupportedActions;

            this.QualifierValues = new Dictionary<string, ConfigurationValue>();
            foreach ( var qualifier in qualifiers )
            {
                this.QualifierValues.Add( qualifier.Key, new ConfigurationValue( qualifier.Value ) );
            }

            this.categoryIds = attribute.Categories.Select( c => c.Id ).ToList();
        }

        /// <summary>
        /// Adds the control.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="value">The value.</param>
        /// <param name="validationGroup">The validation group.</param>
        /// <param name="setValue">if set to <c>true</c> [set value].</param>
        /// <param name="setId">if set to <c>true</c> [set id].</param>
        /// <param name="required">The required.</param>
        /// <param name="labelText">The label text.</param>
        /// <returns></returns>
        public Control AddControl( ControlCollection controls, string value, string validationGroup, bool setValue, bool setId, bool? required = null, string labelText = null )
        {
            if ( labelText == null )
            {
                labelText = this.Name;
            }

            Control attributeControl = this.FieldType.Field.EditControl( QualifierValues, setId ? string.Format( "attribute_field_{0}", this.Id ) : string.Empty );
            if ( attributeControl != null )
            {
                if ( setId )
                {
                    attributeControl.ClientIDMode = ClientIDMode.AutoID;
                }

                // If the control is a RockControl
                var rockControl = attributeControl as IRockControl;
                if ( rockControl != null )
                {
                    controls.Add( attributeControl );

                    rockControl.Label = labelText;
                    rockControl.Help = this.Description;
                    rockControl.Required = required.HasValue ? required.Value : this.IsRequired;
                    rockControl.ValidationGroup = validationGroup;
                }
                else
                {
                    bool renderLabel = !string.IsNullOrEmpty( labelText );
                    bool renderHelp = !string.IsNullOrWhiteSpace( Description );

                    if ( renderLabel || renderHelp )
                    {
                        HtmlGenericControl div = new HtmlGenericControl( "div" );
                        controls.Add( div );

                        div.Controls.Clear();
                        div.AddCssClass( "form-group" );
                        if ( this.IsRequired )
                        {
                            div.AddCssClass( "required" );
                        }

                        div.ClientIDMode = ClientIDMode.AutoID;

                        if ( renderLabel )
                        {
                            Label label = new Label();
                            div.Controls.Add( label );
                            label.ClientIDMode = ClientIDMode.AutoID;
                            label.Text = labelText;
                            label.CssClass = "control-label";
                            label.AssociatedControlID = attributeControl.ID;
                        }

                        if ( renderHelp )
                        {
                            var helpBlock = new Rock.Web.UI.Controls.HelpBlock();
                            div.Controls.Add( helpBlock );
                            helpBlock.ClientIDMode = ClientIDMode.AutoID;
                            helpBlock.Text = this.Description;
                        }

                        div.Controls.Add( attributeControl );
                    }
                    else
                    {
                        controls.Add( attributeControl );
                    }
                }

                if ( setValue )
                {
                    this.FieldType.Field.SetEditValue( attributeControl, QualifierValues, value );
                }
            }

            return attributeControl;
        }

        /// <summary>
        /// Gets the field control from the control that was added using the CreateControl method
        /// </summary>
        /// <param name="attributeControl">The attribute control.</param>
        /// <returns></returns>
        public Control GetControl( Control attributeControl )
        {
            string id = string.Format( "attribute_field_{0}", this.Id );

            if ( attributeControl.ID == id )
            {
                return attributeControl;
            }

            {
                return attributeControl.FindControl( id );
            }
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
                return new Rock.Model.Attribute { Id = 0, EntityTypeId = this.EntityTypeId };
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
            if ( rockContext != null )
            {
                Security.Authorization.MakePrivate( this, action, person, rockContext );
            }
            else
            {
                Security.Authorization.MakePrivate( this, action, person );
            }
        }

        /// <summary>
        /// If action on the current entity is private, removes security that made it private.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        public virtual void MakeUnPrivate( string action, Person person, RockContext rockContext = null )
        {
            if ( rockContext != null )
            {
                Security.Authorization.MakeUnPrivate( this, action, person, rockContext );
            }
            else
            {
                Security.Authorization.MakeUnPrivate( this, action, person );
            }
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
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static AttributeCache Read( int id, RockContext rockContext = null )
        {
            string cacheKey = AttributeCache.CacheKey( id );

            ObjectCache cache = RockMemoryCache.Default;
            AttributeCache attribute = cache[cacheKey] as AttributeCache;

            if ( attribute == null )
            {
                if ( rockContext != null )
                {
                    attribute = LoadById( id, rockContext );
                }
                else
                {
                    using ( var myRockContext = new RockContext() )
                    {
                        attribute = LoadById( id, myRockContext );
                    }
                }

                if ( attribute != null )
                {
                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( cacheKey, attribute, cachePolicy );
                    cache.Set( attribute.Guid.ToString(), attribute.Id, cachePolicy );
                }
            }

            return attribute;
        }

        private static AttributeCache LoadById( int id, RockContext rockContext )
        {
            var attributeService = new Rock.Model.AttributeService( rockContext );
            var attributeModel = attributeService.Get( id );
            if ( attributeModel != null )
            {
                return new AttributeCache( attributeModel );
            }

            return null;
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static AttributeCache Read( Guid guid, RockContext rockContext = null )
        {
            ObjectCache cache = RockMemoryCache.Default;
            object cacheObj = cache[guid.ToString()];

            AttributeCache attribute = null;
            if ( cacheObj != null )
            {
                attribute = Read( (int)cacheObj, rockContext );
            }

            if ( attribute == null )
            {
                if ( rockContext != null )
                {
                    attribute = LoadByGuid( guid, rockContext );
                }
                else
                {
                    using ( var myRockContext = new RockContext() )
                    {
                        attribute = LoadByGuid( guid, myRockContext );
                    }
                }

                if ( attribute != null )
                {
                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( AttributeCache.CacheKey( attribute.Id ), attribute, cachePolicy );
                    cache.Set( attribute.Guid.ToString(), attribute.Id, cachePolicy );
                }
            }

            return attribute;
        }

        private static AttributeCache LoadByGuid( Guid guid, RockContext rockContext )
        {
            var attributeService = new AttributeService( rockContext );
            var attributeModel = attributeService.Get( guid );
            if ( attributeModel != null )
            {
                return new AttributeCache( attributeModel );
            }

            return null;
        }

        /// <summary>
        /// Adds Attribute model to cache, and returns cached object
        /// </summary>
        /// <param name="attributeModel">The attributeModel to cache</param>
        /// <returns></returns>
        public static AttributeCache Read( Rock.Model.Attribute attributeModel )
        {
            string cacheKey = AttributeCache.CacheKey( attributeModel.Id );

            ObjectCache cache = RockMemoryCache.Default;
            AttributeCache attribute = cache[cacheKey] as AttributeCache;

            if ( attribute != null )
            {
                attribute.CopyFromModel( attributeModel );
            }
            else
            {
                attribute = new AttributeCache( attributeModel );
                var cachePolicy = new CacheItemPolicy();
                cache.Set( cacheKey, attribute, cachePolicy );
                cache.Set( attribute.Guid.ToString(), attribute.Id, cachePolicy );
            }

            return attribute;
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

            ObjectCache cache = RockMemoryCache.Default;

            var cachePolicy = new CacheItemPolicy();
            cache.Set( cacheKey, attribute, cachePolicy );
            cache.Set( attribute.Guid.ToString(), attribute.Id, cachePolicy );

            return attribute;
        }

        /// <summary>
        /// Removes attribute from cache
        /// </summary>
        /// <param name="id">The id of the attribute to remove from cache</param>
        public static void Flush( int id )
        {
            ObjectCache cache = RockMemoryCache.Default;
            cache.Remove( AttributeCache.CacheKey( id ) );
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
        public List<string> AvailableKeys
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
        public object this[object key]
        {
            get
            {
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
        public bool ContainsKey( object key )
        {
            var propInfo = GetType().GetProperty( key.ToStringSafe() );
            if ( propInfo != null && propInfo.GetCustomAttributes( typeof( Rock.Data.LavaIgnoreAttribute ) ).Count() <= 0 )
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}