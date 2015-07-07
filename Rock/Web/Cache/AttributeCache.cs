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
using System.Data.Entity;
using System.Linq;
using System.Reflection;
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
    public class AttributeCache : CachedModel<Rock.Model.Attribute>
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

        private object _obj = new object();

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
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Data.IEntity model )
        {
            if ( model is Rock.Model.Attribute )
            {
                var attribute = (Rock.Model.Attribute)model;

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
        }

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="qualifiers">The qualifiers.</param>
        public void CopyFromModel( Rock.Model.Attribute attribute, Dictionary<string, string> qualifiers )
        {
            base.CopyFromModel( attribute );

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
        /// <param name="helpText">The help text.</param>
        /// <returns></returns>
        public Control AddControl( ControlCollection controls, string value, string validationGroup, bool setValue, bool setId, bool? required = null, string labelText = null, string helpText = null )
        {
            if ( labelText == null )
            {
                labelText = this.Name;
            }

            if ( helpText == null )
            {
                helpText = this.Description;
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
                    rockControl.Help = helpText;
                    rockControl.Required = required.HasValue ? required.Value : this.IsRequired;
                    rockControl.ValidationGroup = validationGroup;
                }
                else
                {
                    bool renderLabel = !string.IsNullOrEmpty( labelText );
                    bool renderHelp = !string.IsNullOrWhiteSpace( helpText );

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
                            helpBlock.Text = helpText;
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
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check access to the parent authority specified by this property.
        /// </summary>
        public override ISecured ParentAuthority
        {
            get
            {
                return new Rock.Model.Attribute { Id = 0, EntityTypeId = this.EntityTypeId };
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
            return GetOrAddExisting( AttributeCache.CacheKey( id ),
                () => LoadById( id, rockContext ) );
        }

        private static AttributeCache LoadById( int id, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return LoadById2( id, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return LoadById2( id, rockContext2 );
            }
        }

        private static AttributeCache LoadById2( int id, RockContext rockContext )
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
            int id = GetOrAddExisting( guid.ToString(),
                () => LoadByGuid( guid, rockContext ) );

            return Read( id, rockContext );
        }

        private static int LoadByGuid( Guid guid, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return LoadByGuid2( guid, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return LoadByGuid2( guid, rockContext2 );
            }
        }

        private static int LoadByGuid2( Guid guid, RockContext rockContext )
        {
            var attributeService = new AttributeService( rockContext );
            return attributeService
                .Queryable().AsNoTracking()
                .Where( c => c.Guid.Equals( guid ) )
                .Select( c => c.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Adds Attribute model to cache, and returns cached object
        /// </summary>
        /// <param name="attributeModel">The attributeModel to cache</param>
        /// <returns></returns>
        public static AttributeCache Read( Rock.Model.Attribute attributeModel )
        {
            return GetOrAddExisting( AttributeCache.CacheKey( attributeModel.Id ),
                () => LoadByModel( attributeModel ) );
        }

        private static AttributeCache LoadByModel( Rock.Model.Attribute attributeModel )
        {
            if ( attributeModel != null )
            {
                return new AttributeCache( attributeModel );
            }
            return null;
        }

        /// <summary>
        /// Adds Attribute model to cache, and returns cached object.  
        /// </summary>
        /// <param name="attributeModel">The attribute model.</param>
        /// <param name="qualifiers">The qualifiers.</param>
        /// <returns></returns>
        public static AttributeCache Read( Rock.Model.Attribute attributeModel, Dictionary<string, string> qualifiers )
        {
            return GetOrAddExisting( AttributeCache.CacheKey( attributeModel.Id ),
                () => LoadByModel( attributeModel, qualifiers ) );
        }

        private static AttributeCache LoadByModel( Rock.Model.Attribute attributeModel, Dictionary<string, string> qualifiers )
        {
            if ( attributeModel != null )
            {
                return new AttributeCache( attributeModel, qualifiers );
            }
            return null;
        }

        /// <summary>
        /// Removes attribute from cache
        /// </summary>
        /// <param name="id">The id of the attribute to remove from cache</param>
        public static void Flush( int id )
        {
            FlushCache( AttributeCache.CacheKey( id ) );
        }

        #endregion

        #region ILiquidizable Implementation

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        [LavaIgnore]
        public override object this[object key]
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
        public override bool ContainsKey( object key )
        {
            var propInfo = GetType().GetProperty( key.ToStringSafe() );
            if ( propInfo != null && propInfo.GetCustomAttributes( typeof( Rock.Data.LavaIgnoreAttribute ) ).Count() <= 0 )
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Entity Attributes Cache

        /// <summary>
        /// The _lock
        /// </summary>
        private static object _lock = new object();

        /// <summary>
        /// Gets or sets all entity attributes.
        /// </summary>
        /// <value>
        /// All entity attributes.
        /// </value>
        private static List<EntityAttributes> AllEntityAttributes { get; set; }

        /// <summary>
        /// Gets the by entity.
        /// </summary>
        /// <param name="entityTypeid">The entity typeid.</param>
        /// <returns></returns>
        internal static List<EntityAttributes> GetByEntity( int? entityTypeid )
        {
            LoadEntityAttributes();

            return AllEntityAttributes
                .Where( a => a.EntityTypeId.Equals( entityTypeid ) )
                .ToList();
        }

        /// <summary>
        /// Gets the by entity.
        /// </summary>
        /// <param name="entityTypeid">The entity typeid.</param>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        /// <returns></returns>
        internal static List<int> GetByEntity( int? entityTypeid, string entityTypeQualifierColumn, string entityTypeQualifierValue )
        {
            LoadEntityAttributes();

            return AllEntityAttributes
                .Where( a =>
                    a.EntityTypeId.Equals( entityTypeid ) &&
                    a.EntityTypeQualifierColumn.Equals( entityTypeQualifierColumn ) &&
                    a.EntityTypeQualifierValue.Equals( entityTypeQualifierValue ) )
                .SelectMany( a => a.AttributeIds )
                .ToList();
        }

        /// <summary>
        /// Loads the entity attributes.
        /// </summary>
        private static void LoadEntityAttributes()
        {
            lock ( _lock )
            {
                if ( AllEntityAttributes == null )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        AllEntityAttributes = new AttributeService( rockContext )
                            .Queryable().AsNoTracking()
                            .GroupBy( a => new
                            {
                                a.EntityTypeId,
                                a.EntityTypeQualifierColumn,
                                a.EntityTypeQualifierValue
                            } )
                            .Select( a => new EntityAttributes()
                            {
                                EntityTypeId = a.Key.EntityTypeId,
                                EntityTypeQualifierColumn = a.Key.EntityTypeQualifierColumn,
                                EntityTypeQualifierValue = a.Key.EntityTypeQualifierValue,
                                AttributeIds = a.Select( v => v.Id ).ToList()
                            } )
                            .ToList();
                    }
                }
            }
        }

        /// <summary>
        /// Flushes the entity attributes.
        /// </summary>
        public static void FlushEntityAttributes()
        {
            lock ( _lock )
            {
                AllEntityAttributes = null;
            }
        }

        #endregion
    }

    #region Helper class for entity attributes

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    internal class EntityAttributes
    {
        /// <summary>
        /// Gets or sets the entity type id.
        /// </summary>
        /// <value>
        /// The entity type id.
        /// </value>
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier column.
        /// </summary>
        /// <value>
        /// The entity type qualifier column.
        /// </value>
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier value.
        /// </summary>
        /// <value>
        /// The entity type qualifier value.
        /// </value>
        public string EntityTypeQualifierValue { get; set; }

        /// <summary>
        /// Gets or sets the attribute ids.
        /// </summary>
        /// <value>
        /// The attribute ids.
        /// </value>
        public List<int> AttributeIds { get; set; }
    }

    #endregion
}