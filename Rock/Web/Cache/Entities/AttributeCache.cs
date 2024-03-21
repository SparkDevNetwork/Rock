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
using Rock.Model;
using Rock.Lava;
using Rock.Security;
using Rock.Web.UI.Controls;
using Rock.Attribute;

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
        #region Fields

        private const string AttributePropertyDependenciesCacheKey = "AttributeCache_AttributePropertyDependencyCacheKey";

        #endregion

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
        /// Gets the persisted text value.
        /// </summary>
        /// <value>The persisted text value.</value>
        [DataMember]
        public string DefaultPersistedTextValue { get; private set; }

        /// <summary>
        /// Gets the persisted HTML value.
        /// </summary>
        /// <value>The persisted HTML value.</value>
        [DataMember]
        public string DefaultPersistedHtmlValue { get; private set; }

        /// <summary>
        /// Gets the persisted condensed text value.
        /// </summary>
        /// <value>The persisted condensed text value.</value>
        [DataMember]
        public string DefaultPersistedCondensedTextValue { get; private set; }

        /// <summary>
        /// Gets the persisted condensed HTML value.
        /// </summary>
        /// <value>The persisted condensed HTML value.</value>
        [DataMember]
        public string DefaultPersistedCondensedHtmlValue { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the persisted values are
        /// considered dirty. If the values are dirty then it should be assumed
        /// that they are not in sync with the <see cref="DefaultValue"/> property.
        /// </summary>
        /// <value><c>true</c> if the persisted values are considered dirty; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsDefaultPersistedValueDirty { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this attribute supports persisted values.
        /// </summary>
        /// <value><c>true</c> if this attribute supports persisted values; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsPersistedValueSupported
        {
            get
            {
                if ( !_isPersistedValueSupported.HasValue )
                {
                    _isPersistedValueSupported = FieldType.Field?.IsPersistedValueSupported( ConfigurationValues ) == true;
                }

                return _isPersistedValueSupported.Value;
            }
        }
        private bool? _isPersistedValueSupported;

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

        /// <inheritdoc cref="Rock.Model.Attribute.AttributeColor"/>
        [DataMember]
        public string AttributeColor { get; private set; }

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
        [DataMember]
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
        /// Gets or sets a value indicating whether changes to this attribute's attribute values should be logged in AttributeValueHistorical.
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
        /// Gets or sets the shortened name of the attribute.
        /// If null or whitespace then the full name is returned.
        /// </summary>
        /// <value>
        /// The abbreviated name of the Attribute.
        /// </value>
        [DataMember]
        public string AbbreviatedName { get; private set; }

        /// <summary>
        /// Gets or sets a flag indicating if this attribute shows when doing a bulk entry form.
        /// </summary>
        [DataMember]
        public bool ShowOnBulk { get; private set; }

        /// <summary>
        /// Indicates whether or not this attribute should be displayed in public contexts (e.g., responding to an RSVP without logging in).
        /// </summary>
        /// <value>
        /// A boolean value.
        /// </value>
        [DataMember]
        public bool IsPublic { get; private set; }

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
        /// Gets a value indicating whether the <see cref="FieldType"/> is a referenced entity field type.
        /// </summary>
        /// <value><c>true</c> if this the <see cref="FieldType"/> is a referenced entity field type; otherwise, <c>false</c>.</value>
        public bool IsReferencedEntityFieldType
        {
            get
            {
                if ( !_isReferencedEntityFieldType.HasValue )
                {
                    _isReferencedEntityFieldType = FieldType?.Field is IEntityReferenceFieldType;
                }

                return _isReferencedEntityFieldType.Value;
            }
        }
        private bool? _isReferencedEntityFieldType;

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

                if ( CategoryIds == null )
                {
                    return categories;
                }

                foreach ( var id in CategoryIds.ToList() )
                {
                    categories.Add( CategoryCache.Get( id ) );
                }

                return categories;
            }
        }

        /// <summary>
        /// Gets the configuration values that define the behavior of the attribute.
        /// </summary>
        /// <value>The configuration values.</value>
        [DataMember]
        public Dictionary<string, string> ConfigurationValues { get; private set; }

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
        /// WARNING: This will contain all the Attribute records that in the database.
        /// This could be an expensive call.
        /// Please use <code> AttributeCache.AllForEntityType(int entityTypeId)</code> if you only need the attributes
        /// for a specific EntityType. This will be much more efficient.
        /// </summary>
        /// <returns></returns>
        public static new List<AttributeCache> All()
        {
            return All( null );
        }

        /// <summary>
        /// WARNING: This will contain all the Attribute records that in the database.
        /// This could be an expensive call.
        /// Please use <code> AttributeCache.AllForEntityType(int entityTypeId)</code> if you only need the attributes
        /// for a specific EntityType. This will be much more efficient.
        /// </summary>
        /// <returns></returns>
        public static new List<AttributeCache> All( RockContext rockContext )
        {
            return ModelCache<AttributeCache, Model.Attribute>.All( rockContext );
        }

        /// <summary>
        /// Gets all <seealso cref="AttributeCache">Attributes</seealso> for a specific entityTypeId.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <returns></returns>
        public static AttributeCache[] AllForEntityType( int entityTypeId )
        {
            return GetByEntityType( entityTypeId );
        }

        /// <summary>
        /// Gets all <seealso cref="AttributeCache">Attributes</seealso> for a specific entityTypeId.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <returns></returns>
        internal static AttributeCache[] GetByEntityType( int? entityTypeId )
        {
            var attributeIds = EntityTypeAttributesCache.Get( entityTypeId ).AttributeIds;

            if ( attributeIds.Length == 0 )
            {
                return new AttributeCache[0];
            }

            return GetMany( attributeIds, null ).ToArray();
        }

        /// <summary>
        /// Gets a list of all <seealso cref="AttributeCache">Attributes</seealso> for a specific entityType.
        /// </summary>
        /// <returns></returns>
        public static AttributeCache[] AllForEntityType<T>()
        {
            var entityTypeId = EntityTypeCache.Get<T>()?.Id;
            return AllForEntityType( entityTypeId ?? 0 );
        }

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var attribute = entity as Model.Attribute;
            if ( attribute == null )
            {
                return;
            }

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
            DefaultPersistedTextValue = attribute.DefaultPersistedTextValue;
            DefaultPersistedHtmlValue = attribute.DefaultPersistedHtmlValue;
            DefaultPersistedCondensedTextValue = attribute.DefaultPersistedCondensedTextValue;
            DefaultPersistedCondensedHtmlValue = attribute.DefaultPersistedCondensedHtmlValue;
            IsDefaultPersistedValueDirty = attribute.IsDefaultPersistedValueDirty;
            IsMultiValue = attribute.IsMultiValue;
            IsRequired = attribute.IsRequired;
            AllowSearch = attribute.AllowSearch;
            AttributeColor = attribute.AttributeColor;
            IsIndexEnabled = attribute.IsIndexEnabled;
            IsAnalytic = attribute.IsAnalytic;
            IsAnalyticHistory = attribute.IsAnalyticHistory;
            IsActive = attribute.IsActive;
            EnableHistory = attribute.EnableHistory;
            PreHtml = attribute.PreHtml;
            PostHtml = attribute.PostHtml;
            AbbreviatedName = attribute.AbbreviatedName;
            ShowOnBulk = attribute.ShowOnBulk;
            IsPublic = attribute.IsPublic;

            ConfigurationValues = new Dictionary<string, string>( qualifiers );
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
            if ( attributeControl == null )
            {
                return null;
            }

            var hasAttributeIdControl = attributeControl as IHasAttributeId;
            if ( hasAttributeIdControl != null )
            {
                hasAttributeIdControl.AttributeId = this.Id;
            }

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
                var isRequired = options.Required ?? IsRequired;
                rockControl.Label = options.LabelText;
                rockControl.Help = options.HelpText;
                rockControl.Warning = options.WarningText;
                rockControl.Required = isRequired;
                rockControl.ValidationGroup = options.ValidationGroup;
                if ( options.LabelText.IsNullOrWhiteSpace() && isRequired && rockControl.RequiredErrorMessage.IsNullOrWhiteSpace() )
                {
                    rockControl.RequiredErrorMessage = $"{Name} is required.";
                }

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
        /// Gets a collection of <see cref="Rock.Model.Attribute">Attributes</see> by <see cref="Rock.Model.EntityType"/>, EntityQualifierColumn and EntityQualifierValue.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityQualifierColumn">The entity qualifier column.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <returns></returns>
        public static List<AttributeCache> GetByEntityTypeQualifier( int? entityTypeId, string entityQualifierColumn, string entityQualifierValue, bool includeInactive )
        {
            return EntityTypeAttributesCache.GetByEntityTypeQualifier( entityTypeId, entityQualifierColumn, entityQualifierValue, includeInactive ).ToList();
        }

        /// <summary>
        /// Gets an ordered list of attributes that match the <paramref name="entityQualifierColumn"/>
        /// and <paramref name="entityQualifierValue"/> values for the <paramref name="entityTypeId"/>.
        /// </summary>
        /// <returns>A list of <see cref="AttributeCache"/> objects.</returns>
        [RockInternal( "1.16" )]
        internal static List<AttributeCache> GetOrderedGridAttributes( int? entityTypeId, string entityQualifierColumn, string entityQualifierValue )
        {
            return GetByEntityTypeQualifier( entityTypeId, entityQualifierColumn, entityQualifierValue, false )
                .Where( a => a.IsGridColumn )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ThenBy( a => a.Id )
                .ToList();
        }

        /// <summary>
        /// Gets the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="qualifiers">The qualifiers.</param>
        /// <returns></returns>
        public static AttributeCache Get( Model.Attribute entity, Dictionary<string, string> qualifiers )
        {
            if ( entity == null )
            {
                return null;
            }

            var value = new AttributeCache();
            value.SetFromEntity( entity, qualifiers );

            RockCacheManager<AttributeCache>.Instance.AddOrUpdate( QualifiedKey( entity.Id.ToString() ), value );

            return value;
        }

        /// <summary>
        /// Clears the referenced entity dependency cache. This should be called
        /// anytime an Attribute is created, modified or deleted.
        /// </summary>
        internal static void ClearReferencedEntityDependencies()
        {
            RockCache.Remove( AttributePropertyDependenciesCacheKey );
        }

        /// <summary>
        /// Loads the referenced entity dependency cache if it is empty. This can be called
        /// before an Attribute is created, modified or deleted if there is the possibility
        /// the cache might be empty to prevent a deadlock.
        /// </summary>
        internal static void GetReferencedEntityDependencies()
        {
            RockCache.GetOrAddExisting( AttributePropertyDependenciesCacheKey, GetAttributePropertyDependencies );
        }

        /// <summary>
        /// Gets the dependencies that all attributes have on entity types
        /// whose properties get modified.
        /// </summary>
        /// <returns>
        /// A dictionary whose key is the entity type identifier and value
        /// is another dictionary whose key is the property name and value
        /// is the list of attribute identifiers.
        /// </returns>
        private static Dictionary<int, Dictionary<string, List<int>>> GetAttributePropertyDependencies()
        {
            var dependencies = new Dictionary<int, Dictionary<string, List<int>>>();
            var attributes = All().Where( a => a.FieldType.Field is IEntityReferenceFieldType );

            foreach ( var attribute in attributes )
            {
                var referencedProperties = ( ( IEntityReferenceFieldType ) attribute.FieldType.Field ).GetReferencedProperties( attribute.ConfigurationValues );

                foreach ( var referencedProperty in referencedProperties )
                {
                    if ( !dependencies.TryGetValue( referencedProperty.EntityTypeId, out var entityTypeDependencies ) )
                    {
                        entityTypeDependencies = new Dictionary<string, List<int>>();
                        dependencies.Add( referencedProperty.EntityTypeId, entityTypeDependencies );
                    }

                    if ( !entityTypeDependencies.TryGetValue( referencedProperty.PropertyName, out var attributeIds ) )
                    {
                        attributeIds = new List<int>();
                        entityTypeDependencies.Add( referencedProperty.PropertyName, attributeIds );
                    }

                    attributeIds.Add( attribute.Id );
                }
            }

            return dependencies;
        }

        /// <summary>
        /// Gets the dirty attribute identifiers for an entity of a given
        /// type whose properties were modified.
        /// </summary>
        /// <remarks>
        /// Determining the modified propery names can be a relatively expensive
        /// operation. Since most entity types won't be monitored for changes
        /// like this we use a factory so that we don't need to run that
        /// operation unless we absolutely need to.
        /// </remarks>
        /// <param name="entityTypeId">The entity type identifier of the entity that was modified.</param>
        /// <param name="modifiedPropertyNamesFactory">A factory method that returns the property names that were modified.</param>
        /// <returns>A list of attribute identifiers that might need their values updated.</returns>
        internal static List<int> GetDirtyAttributeIdsForPropertyChange( int entityTypeId, Func<IReadOnlyList<string>> modifiedPropertyNamesFactory )
        {
            var cache = ( Dictionary<int, Dictionary<string, List<int>>> ) RockCache.GetOrAddExisting( AttributePropertyDependenciesCacheKey, GetAttributePropertyDependencies );

            if ( !cache.TryGetValue(entityTypeId, out var entityDependencyCache ) )
            {
                return new List<int>();
            }

            var attributeIds = new List<int>();

            foreach ( var propertyName in modifiedPropertyNamesFactory() )
            {
                if ( entityDependencyCache.TryGetValue( propertyName, out var dependentAttributeIds ) )
                {
                    attributeIds.AddRange( dependentAttributeIds );
                }
            }

            return attributeIds.Distinct().ToList();
        }

        /// <summary>
        /// Gets any non-empty EntityTypeQualifiedColumn values for the entity
        /// specified by the generic type. If this entity type has no attributes
        /// with qualified columns then an empty list will be returned.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity whose attributes will be inspected.</typeparam>
        /// <returns>A list of distinct EntityTypeQualifiedColumn values for <typeparamref name="TEntity"/>.</returns>
        public static List<string> GetAttributeQualifiedColumns<TEntity>()
        {
            var entityTypeId = EntityTypeCache.Get<TEntity>( false )?.Id;

            if ( !entityTypeId.HasValue )
            {
                return new List<string>();
            }

            var attributes = GetByEntityType( entityTypeId );

            var qualifiedColumns = attributes.Select( a => a.EntityTypeQualifierColumn )
                .Distinct()
                .Where( c => !c.IsNullOrWhiteSpace() )
                .ToList();

            return qualifiedColumns;
        }

        /// <summary>
        /// Flushes the attributes for a block type.
        /// </summary>
        /// <param name="blockTypeId">The block type identifier.</param>
        [RockInternal( "1.16.1" )]
        public static void FlushAttributesForBlockType( int blockTypeId )
        {
            if ( blockTypeId <= 0 )
            {
                return;
            }

            var blockTypeIdString = blockTypeId.ToString();

            foreach ( var attribute in All() )
            {
                if ( attribute != null && attribute.EntityTypeQualifierColumn == "BlockTypeId" && attribute.EntityTypeQualifierValue == blockTypeIdString )
                {
                    AttributeCache.FlushItem( attribute.Id );
                }
            }
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
        [LavaHidden]
        public override object this[object key]
        {
            get
            {
                var propInfo = GetType().GetProperty( key.ToStringSafe() );
                if ( propInfo == null || propInfo.GetCustomAttributes( typeof( LavaHiddenAttribute ) ).Any() )
                {
                    return null;
                }

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
            return ContainsKey( key.ToStringSafe() );
        }

        #endregion

        #region ILavaDataDictionary

        /// <summary>
        /// Determines whether the specified Attribute key exists.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public override bool ContainsKey( string key )
        {
            var propInfo = GetType().GetProperty( key );
            return propInfo != null && !propInfo.GetCustomAttributes( typeof( LavaHiddenAttribute ) ).Any();
        }

        #endregion

        #region Entity Attributes Cache

        /// <summary>
        /// Flushes the entity attributes.
        /// </summary>
        [RockObsolete( "1.12" )]
        [Obsolete( "Use EntityTypeAttributesCache.Clear() instead." )]
        public static void RemoveEntityAttributes()
        {
            EntityAttributesCache.Remove();
        }

        /// <summary>
        /// Gets the person attributes of given list of field types class names. If no Field types are specified, all the person attributes are retrieved.
        /// </summary>
        /// <returns>A queryable of the personAttributes</returns>
        public static IEnumerable<AttributeCache> GetPersonAttributes( ICollection<string> desiredFieldTypeClassNames = null )
        {
            int entityTypeIdPerson = EntityTypeCache.GetId<Person>().Value;
            bool shouldReturnAllPersonAttributes = desiredFieldTypeClassNames == null || desiredFieldTypeClassNames.Count == 0;
            if ( shouldReturnAllPersonAttributes )
            {
                return GetByEntityType( entityTypeIdPerson );
            }

            List<FieldTypeCache> fieldTypes = FieldTypeCache.All();

            return GetByEntityType( entityTypeIdPerson )
                .Join( fieldTypes, personAttribute => personAttribute.FieldTypeId, fieldType => fieldType.Id,
                    ( personAtrribute, fieldType ) => new { PersonAttribute = personAtrribute, FieldTypeClassName = fieldType.Class } )
                .Where( a => desiredFieldTypeClassNames.Contains( a.FieldTypeClassName ) )
                .Select( a => a.PersonAttribute );
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
        /// Overrides the required value of the attribute. Leave null to use the normal IsRequired of the attribute
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
