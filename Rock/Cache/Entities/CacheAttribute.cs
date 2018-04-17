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
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock.Data;
using Rock.Field;
using Rock.Security;

using Rock.Web.UI.Controls;

namespace Rock.Cache
{
    /// <summary>
    /// Cached Attribute model
    /// </summary>
    [Serializable]
    [DataContract]
    [JsonConverter( typeof( Utility.AttributeCacheJsonConverter ) )]
    public class CacheAttribute : ModelCache<CacheAttribute, Model.Attribute>
    {
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
        /// Gets or sets the field type identifier.
        /// </summary>
        /// <value>
        /// The field type identifier.
        /// </value>
        [DataMember]
        public int FieldTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity type identifier.
        /// </summary>
        /// <value>
        /// The entity type identifier.
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
        ///   <c>true</c> if this instance is grid column; otherwise, <c>false</c>.
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
        ///   <c>true</c> if this instance is multi value; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsMultiValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is required; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow search].
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
        ///   <c>true</c> if this instance is index enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsIndexEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is analytic.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is analytic; otherwise, <c>false</c>.
        /// </value>
        public bool IsAnalytic { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is analytic history.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is analytic history; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsAnalyticHistory { get; set; }

        /// <summary>
        /// Gets or sets the category ids.
        /// </summary>
        /// <value>
        /// The category ids.
        /// </value>
        [DataMember]
        public List<int> CategoryIds { get; set; }

        /// <summary>
        /// Gets the type of the field.
        /// </summary>
        /// <value>
        /// The type of the field.
        /// </value>
        public CacheFieldType FieldType => CacheFieldType.Get( FieldTypeId );

        /// <summary>
        /// Gets the categories.
        /// </summary>
        /// <value>
        /// The categories.
        /// </value>
        public List<CacheCategory> Categories
        {
            get
            {
                var categories = new List<CacheCategory>();

                if ( CategoryIds == null ) return categories;

                foreach ( var id in CategoryIds.ToList() )
                {
                    categories.Add( CacheCategory.Get( id ) );
                }

                return categories;
            }
        }

        /// <summary>
        /// Gets the qualifier values.
        /// </summary>
        /// <value>
        /// The qualifier values.
        /// </value>
        [DataMember]
        public Dictionary<string, ConfigurationValue> QualifierValues { get; private set; }

        /// <summary>
        /// Gets the default type of the value as.
        /// </summary>
        /// <value>
        /// The default type of the value as.
        /// </value>
        public object DefaultValueAsType => FieldType.Field.ValueAsFieldType( null, DefaultValue, QualifierValues );

        /// <summary>
        /// Gets the default sort value.
        /// </summary>
        /// <value>
        /// The default sort value.
        /// </value>
        public object DefaultSortValue => FieldType.Field.SortValue( null, DefaultValue, QualifierValues );

        #endregion

        #region Public Methods

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var attribute = entity as Model.Attribute;
            if ( attribute == null ) return;

            var qualifiers = new Dictionary<string, string>();
            if ( attribute.AttributeQualifiers != null )
            {
                foreach ( var qualifier in attribute.AttributeQualifiers )
                {
                    qualifiers.Add( qualifier.Key, qualifier.Value );
                }
            }

            SetFromEntity( attribute, qualifiers );
        }

        /// <summary>
        /// Sets from entity.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="qualifiers">The qualifiers.</param>
        public void SetFromEntity( Model.Attribute attribute, Dictionary<string, string> qualifiers )
        {
            base.SetFromEntity( attribute );

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

            CategoryIds = attribute.Categories.Select( c => c.Id ).ToList();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check access to the parent authority specified by this property.
        /// </summary>
        public override ISecured ParentAuthority => new Model.Attribute { Id = 0, EntityTypeId = EntityTypeId };

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
                labelText = Name;
            }

            if ( helpText == null )
            {
                helpText = Description;
            }

            var attributeControl = FieldType.Field.EditControl( QualifierValues, setId ? $"attribute_field_{Id}" : string.Empty );
            if ( attributeControl == null ) return null;

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
                rockControl.Required = required ?? IsRequired;
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
                    var div = new DynamicControlsHtmlGenericControl( "div" )
                    {
                        ID = $"_formgroup_div_{Id}"
                    };
                    controls.Add( div );

                    div.Controls.Clear();
                    div.AddCssClass( "form-group" );
                    if ( IsRequired )
                    {
                        div.AddCssClass( "required" );
                    }

                    div.ClientIDMode = ClientIDMode.AutoID;

                    if ( renderLabel )
                    {
                        var label = new Label
                        {
                            ID = $"_label_{Id}",
                            ClientIDMode = ClientIDMode.AutoID,
                            Text = labelText,
                            CssClass = "control-label",
                            AssociatedControlID = attributeControl.ID
                        };
                        div.Controls.Add( label );
                    }

                    if ( renderHelp )
                    {
                        var helpBlock = new HelpBlock
                        {
                            ID = $"_helpBlock_{Id}",
                            ClientIDMode = ClientIDMode.AutoID,
                            Text = helpText
                        };
                        div.Controls.Add( helpBlock );
                    }

                    if ( renderWarning )
                    {
                        var warningBlock = new WarningBlock
                        {
                            ID = $"_warningBlock_{Id}",
                            ClientIDMode = ClientIDMode.AutoID,
                            Text = warningText
                        };
                        div.Controls.Add( warningBlock );
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
                FieldType.Field.SetEditValue( attributeControl, QualifierValues, value );
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
            var id = $"attribute_field_{Id}";
            return attributeControl.ID == id ? attributeControl : attributeControl.FindControl( id );
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="qualifiers">The qualifiers.</param>
        /// <returns></returns>
        public static CacheAttribute Get( Model.Attribute entity, Dictionary<string, string> qualifiers )
        {
            if ( entity == null ) return null;

            var value = new CacheAttribute();
            value.SetFromEntity( entity, qualifiers );

            var key = entity.Id.ToString();
            RockCacheManager<CacheAttribute>.Instance.AddOrUpdate( key, value );
            RockCacheManager<int?>.Instance.AddOrUpdate( value.Guid.ToString(), value.Id );

            return value;

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
                if ( propInfo == null || propInfo.GetCustomAttributes( typeof( LavaIgnoreAttribute ) ).Any() ) return null;

                var propValue = propInfo.GetValue( this, null );

                return ( propValue as Guid? )?.ToString() ?? propValue;
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
            return propInfo != null && !propInfo.GetCustomAttributes( typeof( LavaIgnoreAttribute ) ).Any();
        }

        #endregion

        #region Entity Attributes Cache


        /// <summary>
        /// Gets the by entity.
        /// </summary>
        /// <param name="entityTypeid">The entity typeid.</param>
        /// <returns></returns>
        internal static List<EntityAttributes> GetByEntity( int? entityTypeid )
        {
            var allEntityAttributes = CacheEntityAttributes.Get();
            if ( allEntityAttributes != null )
            {
                return allEntityAttributes.EntityAttributes
                    .Where( a =>
                        a.EntityTypeId.Equals( entityTypeid ) )
                .ToList();
            }

            return new List<EntityAttributes>();
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
            return GetByEntity( entityTypeid )
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( entityTypeQualifierColumn ) &&
                    a.EntityTypeQualifierValue.Equals( entityTypeQualifierValue ) )
                .SelectMany( a => a.AttributeIds )
                .ToList();
        }

        /// <summary>
        /// Flushes the entity attributes.
        /// </summary>
        public static void RemoveEntityAttributes()
        {
            CacheEntityAttributes.Remove();
        }

        #endregion
    }

}


