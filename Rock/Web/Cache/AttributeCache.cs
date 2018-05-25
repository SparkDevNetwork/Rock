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
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

using Rock.Cache;
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
    [JsonConverter( typeof( Utility.AttributeCacheJsonConverter ) )]
    [Obsolete( "Use Rock.Cache.CacheAttribute instead" )]
    public class AttributeCache : CachedModel<Model.Attribute>
    {
        #region constructors

        internal AttributeCache()
        {
        }

        private AttributeCache( CacheAttribute attribute )
        {
            CopyFromNewCache( attribute );
        }


        #endregion

        #region Properties

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
        /// Gets the default value as the most appropriate datatype
        /// </summary>
        /// <value>
        /// The default type of the value as.
        /// </value>
        public object DefaultValueAsType => FieldType.Field.ValueAsFieldType( null, DefaultValue, QualifierValues );

        /// <summary>
        /// Gets the default value to use for sorting as the most appropriate datatype
        /// </summary>
        /// <value>
        /// The default type of the value as.
        /// </value>
        public object DefaultSortValue => FieldType.Field.SortValue( null, DefaultValue, QualifierValues );

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
        /// Gets or sets whether this Attribute should be used in 'search by attribute value' UIs. 
        /// For example, if you had a UI where you would allow the user to find people based on a list of attributes
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow search]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowSearch { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is index enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is index enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsIndexEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is analytic.
        /// NOTE: Only applies if this is an Attribute on an Entity that implements IAnalytic 
        /// If this is true, the Analytic table for this entity should include a field for this attribute
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is analytic; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsAnalytic { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is analytic history.
        /// Only applies if this is an Attribute on an Entity that implements IAnalyticHistorical and IsAnalytic is True
        /// If this is true and IsAnalytic is also true, a change in value of this Attribute on the Entity makes the CurrentRowIndicator=1 record
        /// to become CurrentRowIndicator=0, sets teh ExpireDate, then a new row with CurrentRowIndicator=1 to be created
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is analytic history; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsAnalyticHistory { get; set; }

        /// <summary>
        /// Gets the type of the field.
        /// </summary>
        /// <value>
        /// The type of the field.
        /// </value>
        public FieldTypeCache FieldType => FieldTypeCache.Read( FieldTypeId );

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
                var categories = new List<CategoryCache>();

                if ( categoryIds == null ) return categories;

                foreach ( var id in categoryIds.ToList() )
                {
                    categories.Add( CategoryCache.Read( id ) );
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

        private List<int> categoryIds;

        #endregion

        #region Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( IEntity model )
        {
            if ( !( model is Model.Attribute ) ) return;
            var attribute = (Model.Attribute)model;

            var qualifiers = new Dictionary<string, string>();
            if ( attribute.AttributeQualifiers != null )
            {
                foreach ( var qualifier in attribute.AttributeQualifiers )
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
        public void CopyFromModel( Model.Attribute attribute, Dictionary<string, string> qualifiers )
        {
            base.CopyFromModel( attribute );

            IsSystem = attribute.IsSystem;
            FieldTypeId = attribute.FieldTypeId;
            EntityTypeId = attribute.EntityTypeId;
            EntityTypeQualifierColumn = attribute.EntityTypeQualifierColumn;
            EntityTypeQualifierValue = attribute.EntityTypeQualifierValue;
            Key = attribute.Key;
            Name = attribute.Name;
            Description = attribute.Description;
            Order = attribute.Order;
            IconCssClass = attribute.IconCssClass;
            IsGridColumn = attribute.IsGridColumn;
            DefaultValue = attribute.DefaultValue;
            IsMultiValue = attribute.IsMultiValue;
            IsRequired = attribute.IsRequired;
            AllowSearch = attribute.AllowSearch;
            IsIndexEnabled = attribute.IsIndexEnabled;
            IsAnalytic = attribute.IsAnalytic;
            IsAnalyticHistory = attribute.IsAnalyticHistory;

            QualifierValues = new Dictionary<string, ConfigurationValue>();
            foreach ( var qualifier in qualifiers )
            {
                QualifierValues.Add( qualifier.Key, new ConfigurationValue( qualifier.Value ) );
            }

            categoryIds = attribute.Categories.Select( c => c.Id ).ToList();
        }

        /// <summary>
        /// Copies properties from a new cached entity
        /// </summary>
        /// <param name="cacheEntity">The cache entity.</param>
        protected sealed override void CopyFromNewCache( IEntityCache cacheEntity )
        {
            base.CopyFromNewCache( cacheEntity );

            if ( !( cacheEntity is CacheAttribute ) ) return;

            var attribute = (CacheAttribute)cacheEntity;
            IsSystem = attribute.IsSystem;
            FieldTypeId = attribute.FieldTypeId;
            EntityTypeId = attribute.EntityTypeId;
            EntityTypeQualifierColumn = attribute.EntityTypeQualifierColumn;
            EntityTypeQualifierValue = attribute.EntityTypeQualifierValue;
            Key = attribute.Key;
            Name = attribute.Name;
            Description = attribute.Description;
            Order = attribute.Order;
            IconCssClass = attribute.IconCssClass;
            IsGridColumn = attribute.IsGridColumn;
            DefaultValue = attribute.DefaultValue;
            IsMultiValue = attribute.IsMultiValue;
            IsRequired = attribute.IsRequired;
            AllowSearch = attribute.AllowSearch;
            IsIndexEnabled = attribute.IsIndexEnabled;
            IsAnalytic = attribute.IsAnalytic;
            IsAnalyticHistory = attribute.IsAnalyticHistory;

            QualifierValues = new Dictionary<string, ConfigurationValue>( attribute.QualifierValues );

            categoryIds = attribute.Categories.Select( c => c.Id ).ToList();
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
        /// <param name="warningText">The warning text.</param>
        /// <returns></returns>
        public Control AddControl( ControlCollection controls, string value, string validationGroup, bool setValue, bool setId, bool? required = null, string labelText = null, string helpText = null, string warningText = null )
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
                    rockControl.Label = labelText;
                    rockControl.Help = helpText;
                    rockControl.Warning = warningText;
                    rockControl.Required = required.HasValue ? required.Value : this.IsRequired;
                    rockControl.ValidationGroup = validationGroup;

                    controls.Add( attributeControl );
                }
                else
                {
                    bool renderLabel = !string.IsNullOrEmpty( labelText );
                    bool renderHelp = !string.IsNullOrWhiteSpace( helpText );
                    bool renderWarning = !string.IsNullOrWhiteSpace( warningText );

                    if ( renderLabel || renderHelp || renderWarning )
                    {
                        DynamicControlsHtmlGenericControl div = new DynamicControlsHtmlGenericControl( "div" );
                        div.ID = $"_formgroup_div_{this.Id}";
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
                            label.ID = $"_label_{this.Id}";
                            div.Controls.Add( label );
                            label.ClientIDMode = ClientIDMode.AutoID;
                            label.Text = labelText;
                            label.CssClass = "control-label";
                            label.AssociatedControlID = attributeControl.ID;
                        }

                        if ( renderHelp )
                        {
                            var helpBlock = new Rock.Web.UI.Controls.HelpBlock();
                            helpBlock.ID = $"_helpBlock_{this.Id}";
                            div.Controls.Add( helpBlock );
                            helpBlock.ClientIDMode = ClientIDMode.AutoID;
                            helpBlock.Text = helpText;
                        }

                        if ( renderWarning )
                        {
                            var warningBlock = new Rock.Web.UI.Controls.WarningBlock();
                            warningBlock.ID = $"_warningBlock_{this.Id}";
                            div.Controls.Add( warningBlock );
                            warningBlock.ClientIDMode = ClientIDMode.AutoID;
                            warningBlock.Text = warningText;
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

        /// <summary>
        /// Returns Attribute object from cache.  If attribute does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The id of the Attribute to read</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static AttributeCache Read( int id, RockContext rockContext = null )
        {
            return new AttributeCache( CacheAttribute.Get( id, rockContext ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static AttributeCache Read( Guid guid, RockContext rockContext = null )
        {
            return new AttributeCache( CacheAttribute.Get( guid, rockContext ) );
        }

        /// <summary>
        /// Adds Attribute model to cache, and returns cached object
        /// </summary>
        /// <param name="attributeModel">The attributeModel to cache</param>
        /// <returns></returns>
        public static AttributeCache Read( Model.Attribute attributeModel )
        {
            return new AttributeCache( CacheAttribute.Get( attributeModel ) );
        }

        /// <summary>
        /// Adds Attribute model to cache, and returns cached object.  
        /// </summary>
        /// <param name="attributeModel">The attribute model.</param>
        /// <param name="qualifiers">The qualifiers.</param>
        /// <returns></returns>
        public static AttributeCache Read( Model.Attribute attributeModel, Dictionary<string, string> qualifiers )
        {
            return new AttributeCache( CacheAttribute.Get( attributeModel, qualifiers ) );
        }

        /// <summary>
        /// Removes attribute from cache
        /// </summary>
        /// <param name="id">The id of the attribute to remove from cache</param>
        public static void Flush( int id )
        {
            CacheAttribute.Remove( id );
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
            var entityAttributes = new List<EntityAttributes>();

            var cacheEntityAttributes = CacheAttribute.GetByEntity( entityTypeid );
            if ( cacheEntityAttributes == null ) return entityAttributes;

            foreach ( var cacheEntityAttribute in cacheEntityAttributes )
            {
                entityAttributes.Add( new EntityAttributes( cacheEntityAttribute ) );

            }

            return entityAttributes;
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
            return CacheAttribute.GetByEntity( entityTypeid, entityTypeQualifierColumn, entityTypeQualifierValue );
        }

        /// <summary>
        /// Loads the entity attributes.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        public static void LoadEntityAttributes( RockContext rockContext )
        {
            CacheEntityAttributes.Clear();
        }

        /// <summary>
        /// Flushes the entity attributes.
        /// </summary>
        public static void FlushEntityAttributes()
        {
            CacheEntityAttributes.Clear();
        }

        #endregion
    }

    #region Helper class for entity attributes

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [Obsolete( "Use Rock.Cache.EntityAttributes instead" )]
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

        public EntityAttributes( Rock.Cache.EntityAttributes entityAttributes )
        {
            if ( entityAttributes == null ) return;

            EntityTypeId = entityAttributes.EntityTypeId;
            EntityTypeQualifierColumn = entityAttributes.EntityTypeQualifierColumn;
            EntityTypeQualifierValue = entityAttributes.EntityTypeQualifierValue;
            AttributeIds = new List<int>( entityAttributes.AttributeIds );
        }

    }

    #endregion
}