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

using Rock.Data;
using Rock.Field;
using Rock.Security;

using Rock.Web.UI.Controls;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Cached Attribute model
    /// </summary>
    [Serializable]
    [DataContract]
    [JsonConverter( typeof( Utility.AttributeCacheJsonConverter ) )]
    public class AttributeCache : ModelCache<AttributeCache, Model.Attribute>
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; private set; }

        /// <summary>
        /// Gets or sets the field type identifier.
        /// </summary>
        /// <value>
        /// The field type identifier.
        /// </value>
        [DataMember]
        public int FieldTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the entity type identifier.
        /// </summary>
        /// <value>
        /// The entity type identifier.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the entity type qualifier column.
        /// </summary>
        /// <value>
        /// The entity type qualifier column.
        /// </value>
        [DataMember]
        public string EntityTypeQualifierColumn { get; private set; }

        /// <summary>
        /// Gets or sets the entity type qualifier value.
        /// </summary>
        /// <value>
        /// The entity type qualifier value.
        /// </value>
        [DataMember]
        public string EntityTypeQualifierValue { get; private set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        [DataMember]
        public string Key { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; private set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [DataMember]
        public string IconCssClass { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is grid column.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is grid column; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsGridColumn { get; private set; }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        [DataMember]
        public string DefaultValue { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is multi value.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is multi value; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsMultiValue { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is required; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsRequired { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow search].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow search]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowSearch { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is index enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is index enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsIndexEnabled { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is analytic.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is analytic; otherwise, <c>false</c>.
        /// </value>
        public bool IsAnalytic { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is analytic history.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is analytic history; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsAnalyticHistory { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this attribute is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable history]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool EnableHistory { get; private set; }

        /// <summary>
        /// Gets or sets any HTML to be rendered before the attribute's edit control 
        /// </summary>
        /// <value>
        /// The pre HTML.
        /// </value>
        [DataMember]
        public string PreHtml { get; private set; }

        /// <summary>
        /// Gets or sets any HTML to be rendered after the attribute's edit control 
        /// </summary>
        /// <value>
        /// The post HTML.
        /// </value>
        [DataMember]
        public string PostHtml { get; private set; }

        /// <summary>
        /// Gets a value indicating whether changes to this attribute's attribute values should be logged in AttributeValueHistorical
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; private set; }

        /// <summary>
        /// Gets or sets the category ids.
        /// </summary>
        /// <value>
        /// The category ids.
        /// </value>
        [DataMember]
        public List<int> CategoryIds { get; private set; }

        /// <summary>
        /// Gets the type of the field.
        /// </summary>
        /// <value>
        /// The type of the field.
        /// </value>
        public FieldTypeCache FieldType => FieldTypeCache.Get( FieldTypeId );

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

                if ( CategoryIds == null ) return categories;

                foreach ( var id in CategoryIds.ToList() )
                {
                    categories.Add( CategoryCache.Get( id ) );
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
        /// The default value using the most appropriate datatype
        /// </summary>
        /// <value>
        /// The default type of the value as.
        /// </value>
        public object DefaultValueAsType => FieldType.Field.ValueAsFieldType( null, DefaultValue, QualifierValues );

        /// <summary>
        /// The default value formatted based on the field type and qualifiers
        /// </summary>
        /// <value>
        /// The default value as formatted.
        /// </value>
        public string DefaultValueAsFormatted => FieldType.Field.FormatValue( null, DefaultValue, QualifierValues, false );

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
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        [RockObsolete( "1.8" )]
        [Obsolete("Use SetFromEntity instead")]
        public override void CopyFromModel( Data.IEntity model )
        {
            this.SetFromEntity( model );
        }

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
        /// Copies from model.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="qualifiers">The qualifiers.</param>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use SetFromEntity instead" )]
        public void CopyFromModel( Rock.Model.Attribute attribute, Dictionary<string, string> qualifiers )
        {
            this.SetFromEntity( attribute, qualifiers );
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
            IsActive = attribute.IsActive;
            EnableHistory = attribute.EnableHistory;
            PreHtml = attribute.PreHtml;
            PostHtml = attribute.PostHtml;

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
            return AddControl( controls, value, validationGroup, setValue, setId, required, labelText, helpText, warningText, null );
        }

        /// <summary>
        /// Adds the control.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="value">The value.</param>
        /// <param name="validationGroup">The validation group.</param>
        /// <param name="setValue">if set to <c>true</c> [set value].</param>
        /// <param name="setId">if set to <c>true</c> [set identifier].</param>
        /// <param name="required">The required.</param>
        /// <param name="labelText">The label text.</param>
        /// <param name="helpText">The help text.</param>
        /// <param name="warningText">The warning text.</param>
        /// <param name="attributeControlId">The attribute control identifier.</param>
        /// <returns></returns>
        public Control AddControl( ControlCollection controls, string value, string validationGroup, bool setValue, bool setId, bool? required, string labelText, string helpText, string warningText, string attributeControlId )
        {
            AttributeControlOptions attributeControlOptions = new AttributeControlOptions
            {
                Value = value,
                ValidationGroup = validationGroup,
                SetValue = setValue,
                SetId = setId,
                Required = required,
                LabelText = labelText,
                HelpText = helpText,
                WarningText = warningText,
                AttributeControlId = attributeControlId
            };

            return AddControl( controls, attributeControlOptions );
        }

        /// <summary>
        /// Adds the control.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public Control AddControl( ControlCollection controls, AttributeControlOptions options )
        {
            options.LabelText = options.LabelText ?? Name;
            options.HelpText = options.HelpText ?? Description;
            options.AttributeControlId = options.AttributeControlId ?? $"attribute_field_{Id}";

            EntityTypeCache entityType = EntityTypeId.HasValue ? EntityTypeCache.Get( this.EntityTypeId.Value ) : null;

            bool showPrePostHtml = ( entityType?.AttributesSupportPrePostHtml ?? false ) && ( options?.ShowPrePostHtml ?? true );

            var attributeControl = FieldType.Field.EditControl( QualifierValues, options.SetId ? options.AttributeControlId : string.Empty );
            if ( attributeControl == null ) return null;

            if ( options.SetId )
            {
                attributeControl.ClientIDMode = ClientIDMode.AutoID;
            }

            // If the control is a RockControl
            var rockControl = attributeControl as IRockControl;
            var controlHasRequired = attributeControl as IHasRequired;

            if ( showPrePostHtml )
            {
                if ( this.PreHtml.IsNotNullOrWhiteSpace() )
                {
                    controls.Add( new Literal { Text = this.PreHtml } );
                }
            }
            
            if ( rockControl != null )
            {
                rockControl.Label = options.LabelText;
                rockControl.Help = options.HelpText;
                rockControl.Warning = options.WarningText;
                rockControl.Required = options.Required ?? IsRequired;
                rockControl.ValidationGroup = options.ValidationGroup;

                controls.Add( attributeControl );
            }
            else
            {
                if ( controlHasRequired != null )
                {
                    controlHasRequired.Required = options.Required ?? IsRequired;
                    controlHasRequired.ValidationGroup = options.ValidationGroup;
                }

                bool renderLabel = !string.IsNullOrEmpty( options.LabelText );
                bool renderHelp = !string.IsNullOrWhiteSpace( options.HelpText );
                bool renderWarning = !string.IsNullOrWhiteSpace( options.WarningText );

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
                            Text = options.LabelText,
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
                            Text = options.HelpText
                        };
                        div.Controls.Add( helpBlock );
                    }

                    if ( renderWarning )
                    {
                        var warningBlock = new WarningBlock
                        {
                            ID = $"_warningBlock_{Id}",
                            ClientIDMode = ClientIDMode.AutoID,
                            Text = options.WarningText
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

            if ( options.ShowPrePostHtml )
            {
                if ( this.PostHtml.IsNotNullOrWhiteSpace() )
                {
                    controls.Add( new Literal { Text = this.PostHtml } );
                }
            }

            if ( options.SetValue )
            {
                FieldType.Field.SetEditValue( attributeControl, QualifierValues, options.Value );
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
        public static AttributeCache Get( Model.Attribute entity, Dictionary<string, string> qualifiers )
        {
            if ( entity == null ) return null;

            var value = new AttributeCache();
            value.SetFromEntity( entity, qualifiers );

            RockCacheManager<AttributeCache>.Instance.AddOrUpdate( QualifiedKey( entity.Id.ToString() ), value );
            RockCacheManager<int?>.Instance.AddOrUpdate( QualifiedKey( value.Guid.ToString() ), value.Id );

            return value;

        }

        /// <summary>
        /// Reads the specified attribute model.
        /// </summary>
        /// <param name="attributeModel">The attribute model.</param>
        /// <param name="qualifiers">The qualifiers.</param>
        /// <returns></returns>
        [RockObsolete( "1.8" )]
        [Obsolete("Use Get instead")]
        public static AttributeCache Read( Rock.Model.Attribute attributeModel, Dictionary<string, string> qualifiers )
        {
            return Get( attributeModel, qualifiers );
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
        /// Gets a list of AttributeIds for the specified entityTypeId
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <returns></returns>
        internal static List<EntityAttributes> GetByEntity( int? entityTypeId )
        {
            var allEntityAttributes = EntityAttributesCache.Get();
            if ( allEntityAttributes != null )
            {
                List<EntityAttributes> result;
                if ( entityTypeId.HasValue )
                {
                    result = allEntityAttributes.EntityAttributesByEntityTypeId.GetValueOrNull( entityTypeId.Value ) ?? new List<EntityAttributes>();
                }
                else
                {
                    result = allEntityAttributes.EntityAttributes.Where( a => !a.EntityTypeId.HasValue ).ToList();
                }

                return result;
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
            EntityAttributesCache.Remove();
        }

        /// <summary>
        /// Loads the entity attributes.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        [RockObsolete( "1.8" )]
        [Obsolete("No longer needed")]
        public static void LoadEntityAttributes( RockContext rockContext )
        {
            //
        }

        /// <summary>
        /// Flushes the entity attributes.
        /// </summary>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use RemoveEntityAttributes instead" )]
        public static void FlushEntityAttributes()
        {
            EntityAttributesCache.Remove();
        }

        /// <summary>
        /// Updates the <see cref="EntityAttributesCache" /> based on the attribute and entityState
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="entityState">State of the entity.</param>
        internal static void UpdateCacheEntityAttributes( Rock.Model.Attribute attribute, EntityState entityState )
        {
            EntityAttributesCache.UpdateCacheEntityAttributes( attribute, entityState );
        }

        #endregion
    }

    /// <summary>
    /// Defined options that be can used when calling AddControl
    /// </summary>
    public class AttributeControlOptions
    {

        /// <summary>
        /// The value that should be set to the control after it is created.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup { get; set; }

        /// <summary>
        /// Apply or not to apply the value provided in Value
        /// </summary>
        /// <value>
        ///   <c>true</c> if [set value]; otherwise, <c>false</c>.
        /// </value>
        public bool SetValue { get; set; }

        /// <summary>
        /// Set the control ID or not. This must be true in order to use the value in AttributeControlId
        /// </summary>
        /// <value>
        ///   <c>true</c> if [set identifier]; otherwise, <c>false</c>.
        /// </value>
        public bool SetId { get; set; }

        /// <summary>
        /// Gets or sets the required.
        /// </summary>
        /// <value>
        /// The required.
        /// </value>
        public bool? Required { get; set; }

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        public string LabelText { get; set; }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        public string HelpText { get; set; }

        /// <summary>
        /// Gets or sets the warning text.
        /// </summary>
        /// <value>
        /// The warning text.
        /// </value>
        public string WarningText { get; set; }

        /// <summary>
        /// If a custom ID is required put it here. If an auto generated one is okay then just set SetId to true and the control will be named attribute_field_{AttributeId}
        /// </summary>
        /// <value>
        /// The attribute control identifier.
        /// </value>
        public string AttributeControlId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show pre post HTML] (if EntityType supports it)
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show pre post HTML]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowPrePostHtml { get; set; }
    }
}


