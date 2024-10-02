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
using System.Threading.Tasks;
#if WEBFORMS
using System.Web.UI;
using System.Web.UI.WebControls;
#endif
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a dropdown list of attributes
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.ATTRIBUTE )]
    public class AttributeFieldType : FieldType, ICachedEntitiesFieldType, IEntityFieldType, IEntityReferenceFieldType
    {
        #region Configuration

        private const string ENTITY_TYPE_KEY = "entitytype";
        private const string ALLOW_MULTIPLE_KEY = "allowmultiple";
        private const string QUALIFIER_COLUMN_KEY = "qualifierColumn";
        private const string QUALIFIER_VALUE_KEY = "qualifierValue";

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( privateValue.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            var names = new List<string>();
            RockContext rockContext = null;
            AttributeService attributeService = null;

            foreach ( var guid in privateValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList() )
            {
                // It's possible that this attribute field type is pointing
                // back to our own attribute which could now cause an infinite
                // loop. So try to get it from the cache without adding it and
                // if that fails load from the database.
                if ( AttributeCache.TryGet( guid, out var attributeCache ) )
                {
                    names.Add( attributeCache.Name );
                }
                else
                {
                    if ( rockContext == null )
                    {
                        rockContext = new RockContext();
                        attributeService = new AttributeService( rockContext );
                    }

                    var attributeName = attributeService.GetSelect( guid, a => a.Name );

                    if ( attributeName != null )
                    {
                        names.Add( attributeName );

                        // Start a task to make sure this attribute gets loaded
                        // into cache since we'll probably need it in the future.
                        Task.Run( async () =>
                        {
                            // Brief delay to let things settle.
                            await Task.Delay( 25 );
                            AttributeCache.Get( guid );
                        } );
                    }
                }
            }

            // Clean up if we created a context.
            if ( rockContext != null )
            {
                rockContext.Dispose();
            }

            return names.AsDelimited( ", " );
        }

        #endregion

        #region ICachedEntitiesFieldType Members
        /// <summary>
        /// Gets the cached attributes.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public List<IEntityCache> GetCachedEntities( string value )
        {
            var attributes = new List<IEntityCache>();

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                foreach ( Guid guid in value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList() )
                {
                    var attribute = AttributeCache.Get( guid );
                    if ( attribute != null )
                    {
                        attributes.Add( attribute );
                    }
                }
            }

            return attributes;
        }
        #endregion

        #region Edit Control

        #endregion

        #region Filter Control

        /// <summary>
        /// Gets the type of the filter comparison.
        /// </summary>
        /// <value>
        /// The type of the filter comparison.
        /// </value>
        public override ComparisonType FilterComparisonType
        {
            get
            {
                return ComparisonHelper.ContainsFilterComparisonTypes;
            }
        }

        #endregion

        #region IEntityFieldType

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value )
        {
            return GetEntity( value, null );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value, RockContext rockContext )
        {
            var guid = value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                rockContext = rockContext ?? new RockContext();
                return new Model.AttributeService( rockContext ).Get( guid.Value );
            }

            return null;
        }
        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( privateValue.IsNullOrWhiteSpace() )
            {
                return null;
            }

            RockContext rockContext = null;
            AttributeService attributeService = null;
            var entityReferences = new List<ReferencedEntity>();
            var attributeEntityTypeId = EntityTypeCache.Get<Rock.Model.Attribute>().Id;

            foreach ( var guid in privateValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList() )
            {
                // It's possible that this attribute field type is pointing
                // back to our own attribute which could now cause an infinite
                // loop. So try to get it from the cache without adding it and
                // if that fails load from the database.
                if ( AttributeCache.TryGet( guid, out var attributeCache ) )
                {
                    entityReferences.Add( new ReferencedEntity( attributeEntityTypeId, attributeCache.Id ) );
                }
                else
                {
                    if ( rockContext == null )
                    {
                        rockContext = new RockContext();
                        attributeService = new AttributeService( rockContext );
                    }

                    var attributeId = attributeService.GetSelect( guid, a => ( int? ) a.Id );

                    if ( attributeId.HasValue )
                    {
                        entityReferences.Add( new ReferencedEntity( attributeEntityTypeId, attributeId.Value ) );

                        // Start a task to make sure this attribute gets loaded
                        // into cache since we'll probably need it in the future.
                        Task.Run( async () =>
                        {
                            // Brief delay to let things settle.
                            await Task.Delay( 25 );
                            AttributeCache.Get( guid );
                        } );
                    }
                }
            }

            return entityReferences;
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // If the Name property of an Attribute we are referencing changes
            // then we need to update our persisted values.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<Rock.Model.Attribute>().Value, nameof( Rock.Model.Attribute.Name ) )
            };
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( ENTITY_TYPE_KEY );
            configKeys.Add( ALLOW_MULTIPLE_KEY );
            configKeys.Add( QUALIFIER_COLUMN_KEY );
            configKeys.Add( QUALIFIER_VALUE_KEY );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // build a drop down list of enity types (the one that gets selected is
            // used to build a list of attributes) 
            var etp = new EntityTypePicker();
            controls.Add( etp );
            etp.AutoPostBack = true;
            etp.SelectedIndexChanged += OnQualifierUpdated;
            etp.Label = "Entity Type";
            etp.Help = "The Entity Type to select attributes for.";

            var entityTypeList = new Model.EntityTypeService( new RockContext() ).GetEntities().ToList();
            etp.EntityTypes = entityTypeList;

            // Add checkbox for deciding if the defined values list is renedered as a drop
            // down list or a checkbox list.
            var cb = new RockCheckBox();
            controls.Add( cb );
            cb.AutoPostBack = true;
            cb.CheckedChanged += OnQualifierUpdated;
            cb.Label = "Allow Multiple Values";
            cb.Help = "When set, allows multiple attributes to be selected.";

            // Add textbox for the qualifier column
            var tbColumn = new RockTextBox();
            controls.Add( tbColumn );
            tbColumn.AutoPostBack = true;
            tbColumn.TextChanged += OnQualifierUpdated;
            tbColumn.Label = "Qualifier Column";
            tbColumn.Help = "Entity column qualifier.";

            // Add textbox for the qualifier value
            var tbValue = new RockTextBox();
            controls.Add( tbValue );
            tbValue.AutoPostBack = true;
            tbValue.TextChanged += OnQualifierUpdated;
            tbValue.Label = "Qualifier Value";
            tbValue.Help = "Entity column value.";
            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();
            configurationValues.Add( ENTITY_TYPE_KEY, new ConfigurationValue( "Entity Type", "The Entity Type to select attributes for.", "" ) );
            configurationValues.Add( ALLOW_MULTIPLE_KEY, new ConfigurationValue( "Allow Multiple Values", "When set, allows multiple attributes to be selected.", "" ) );
            configurationValues.Add( QUALIFIER_COLUMN_KEY, new ConfigurationValue( "Qualifier Column", "Entity column qualifier", "" ) );
            configurationValues.Add( QUALIFIER_VALUE_KEY, new ConfigurationValue( "Qualifier Value", "Entity column value", "" ) );

            if ( controls != null && controls.Count == 4 )
            {
                if ( controls[0] != null && controls[0] is EntityTypePicker )
                {
                    string value = string.Empty;
                    int? entityTypeId = ( ( EntityTypePicker ) controls[0] ).SelectedValue.AsIntegerOrNull();
                    if ( entityTypeId.HasValue )
                    {
                        var entityType = EntityTypeCache.Get( entityTypeId.Value );
                        if ( entityType != null )
                        {
                            value = entityType.Guid.ToString();
                        }
                    }
                    configurationValues[ENTITY_TYPE_KEY].Value = value;
                }

                if ( controls[1] != null && controls[1] is CheckBox )
                {
                    configurationValues[ALLOW_MULTIPLE_KEY].Value = ( ( CheckBox ) controls[1] ).Checked.ToString();
                }

                if ( controls[2] != null && controls[2] is TextBox )
                {
                    configurationValues[QUALIFIER_COLUMN_KEY].Value = ( ( TextBox ) controls[2] ).Text;
                }

                if ( controls[3] != null && controls[3] is TextBox )
                {
                    configurationValues[QUALIFIER_VALUE_KEY].Value = ( ( TextBox ) controls[3] ).Text;
                }
            }

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="configurationValues"></param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( controls != null && controls.Count == 4 && configurationValues != null )
            {
                if ( controls[0] != null && controls[0] is EntityTypePicker && configurationValues.ContainsKey( ENTITY_TYPE_KEY ) )
                {
                    string value = string.Empty;
                    Guid? entityTypeGuid = configurationValues[ENTITY_TYPE_KEY].Value.AsGuidOrNull();
                    if ( entityTypeGuid.HasValue )
                    {
                        var entityType = EntityTypeCache.Get( entityTypeGuid.Value );
                        if ( entityType != null )
                        {
                            value = entityType.Id.ToString();
                        }
                    }
                    ( ( EntityTypePicker ) controls[0] ).SelectedValue = value;
                }

                if ( controls[1] != null && controls[1] is CheckBox && configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) )
                {
                    ( ( CheckBox ) controls[1] ).Checked = configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean();
                }

                if ( controls[2] != null && controls[2] is TextBox && configurationValues.ContainsKey( QUALIFIER_COLUMN_KEY ) )
                {
                    ( ( TextBox ) controls[2] ).Text = configurationValues[QUALIFIER_COLUMN_KEY].Value;
                }

                if ( controls[3] != null && controls[3] is TextBox && configurationValues.ContainsKey( QUALIFIER_VALUE_KEY ) )
                {
                    ( ( TextBox ) controls[3] ).Text = configurationValues[QUALIFIER_VALUE_KEY].Value;
                }
            }
        }

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return !condensed
                ? GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            ListControl editControl;

            if ( configurationValues != null && configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) && configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean() )
            {
                editControl = new RockListBox { ID = id };
                editControl.AddCssClass( "checkboxlist-group" );
            }
            else
            {
                editControl = new RockDropDownList { ID = id, EnhanceForLongLists = true };
                editControl.Items.Add( new ListItem() );
            }

            if ( configurationValues != null && configurationValues.ContainsKey( ENTITY_TYPE_KEY ) )
            {
                Guid? entityTypeGuid = configurationValues[ENTITY_TYPE_KEY].Value.AsGuidOrNull();
                if ( entityTypeGuid.HasValue )
                {
                    var entityType = EntityTypeCache.Get( entityTypeGuid.Value );
                    if ( entityType != null )
                    {
                        Rock.Model.AttributeService attributeService = new Model.AttributeService( new RockContext() );
                        IQueryable<Rock.Model.Attribute> attributeQuery;
                        if ( configurationValues.ContainsKey( QUALIFIER_COLUMN_KEY ) && configurationValues.ContainsKey( QUALIFIER_VALUE_KEY ) )
                        {
                            attributeQuery = attributeService
                                .GetByEntityTypeQualifier( entityType.Id, configurationValues[QUALIFIER_COLUMN_KEY].Value, configurationValues[QUALIFIER_VALUE_KEY].Value, true );

                        }
                        else
                        {
                            attributeQuery = attributeService.GetByEntityTypeId( entityType.Id, true );
                        }

                        List<AttributeCache> attributeList = attributeQuery.ToAttributeCacheList();

                        if ( attributeList.Any() )
                        {
                            foreach ( var attribute in attributeList.OrderBy( a => a.Name ) )
                            {
                                editControl.Items.Add( new ListItem( attribute.Name, attribute.Id.ToString(), attribute.IsActive ) );
                            }
                        }
                        return editControl;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var ids = new List<string>();

            if ( control != null && control is ListControl )
            {
                if ( control is Rock.Web.UI.Controls.RockDropDownList )
                {
                    ids.Add( ( ( ListControl ) control ).SelectedValue );
                }
                else if ( control is Rock.Web.UI.Controls.RockListBox )
                {
                    var lbControl = control as Rock.Web.UI.Controls.RockListBox;

                    ids.AddRange( lbControl.Items.Cast<ListItem>()
                        .Where( i => i.Selected )
                        .Select( i => i.Value ) );
                }

                if ( ids.Count == 0 )
                {
                    return string.Empty;
                }

                var guids = new List<string>();

                foreach ( int attributeId in ids.AsIntegerList() )
                {
                    var attribute = Rock.Web.Cache.AttributeCache.Get( attributeId );
                    if ( attribute != null )
                    {
                        guids.Add( attribute.Guid.ToString() );
                    }
                }

                return guids.AsDelimited( "," );
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( value != null )
            {
                if ( control != null && control is ListControl )
                {
                    var ids = new List<string>();
                    foreach ( Guid guid in value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList() )
                    {
                        var attribute = Rock.Web.Cache.AttributeCache.Get( guid );
                        if ( attribute != null )
                        {
                            ids.Add( attribute.Id.ToString() );
                        }
                    }

                    var listControl = control as ListControl;

                    if ( ids.Any() )
                    {
                        foreach ( ListItem li in listControl.Items )
                        {
                            li.Selected = ids.Contains( li.Value );
                        }
                    }
                    else
                    {
                        if ( control is Rock.Web.UI.Controls.RockDropDownList && listControl.Items.Count > 0 )
                        {
                            listControl.SelectedIndex = 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the filter value control.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            var overrideConfigValues = new Dictionary<string, ConfigurationValue>();
            foreach ( var keyVal in configurationValues )
            {
                overrideConfigValues.Add( keyVal.Key, keyVal.Value );
            }
            overrideConfigValues.AddOrReplace( ALLOW_MULTIPLE_KEY, new ConfigurationValue( "false" ) );

            return base.FilterValueControl( overrideConfigValues, id, required, filterMode );
        }

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var guid = GetEditValue( control, configurationValues ).AsGuid();
            var item = Rock.Web.Cache.AttributeCache.Get( guid );
            return item != null ? item.Id : ( int? ) null;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            var item = Rock.Web.Cache.AttributeCache.Get( id ?? 0 );
            var guidValue = item != null ? item.Guid.ToString() : string.Empty;
            SetEditValue( control, configurationValues, guidValue );
        }

#endif
        #endregion
    }
}