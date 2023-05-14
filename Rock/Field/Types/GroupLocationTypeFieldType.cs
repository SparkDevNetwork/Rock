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
#if WEBFORMS
using System.Web.UI;
using System.Web.UI.WebControls;
#endif
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a DropDown list of Group Location Types for a specific Group Type.
    /// Stored as GroupLocationTypeValue.Guid.
    /// </summary>
    [Serializable]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.GROUP_LOCATION_TYPE )]
    public class GroupLocationTypeFieldType : FieldType, IEntityReferenceFieldType
    {

        #region Configuration

        private const string GROUP_TYPE_KEY = "groupTypeGuid";
        private const string GROUP_TYPE_LOCATIONS_KEY = "groupTypeLocations";

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            string formattedValue = string.Empty;

            var definedValueGuid = privateValue.AsGuidOrNull();
            if ( definedValueGuid.HasValue )
            {
                var definedValue = DefinedValueCache.Get( definedValueGuid.Value );
                if ( definedValue != null )
                {
                    formattedValue = definedValue.Value;
                }
            }
            return formattedValue;
        }

        #endregion

        #region Edit Control

        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            var locationTypeValue = publicValue.FromJsonOrNull<ListItemBag>();

            if ( locationTypeValue != null )
            {
                return locationTypeValue.Value;
            }

            return base.GetPrivateEditValue( publicValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( !string.IsNullOrWhiteSpace( privateValue ) )
            {
                return new ListItemBag()
                {
                    Text = DefinedValueCache.Get( privateValue )?.Value,
                    Value = privateValue
                }.ToCamelCaseJson( false, true );
            }

            return base.GetPublicEditValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPrivateConfigurationValues( Dictionary<string, string> publicConfigurationValues )
        {
            var privateConfigurationValues = base.GetPrivateConfigurationValues( publicConfigurationValues );

            if ( publicConfigurationValues.ContainsKey( GROUP_TYPE_KEY ) )
            {
                var groupTypeValue= publicConfigurationValues[GROUP_TYPE_KEY].FromJsonOrNull<ListItemBag>();
                if ( groupTypeValue != null )
                {
                    privateConfigurationValues[GROUP_TYPE_KEY] = groupTypeValue.Value;
                }
            }

            privateConfigurationValues.Remove( GROUP_TYPE_LOCATIONS_KEY );

            return privateConfigurationValues;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string value )
        {
            var publicConfigurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, value );
            var groupTypes = new List<GroupTypeCache>();

            if ( publicConfigurationValues.ContainsKey( GROUP_TYPE_KEY ) )
            {
                var groupTypeValue = publicConfigurationValues[GROUP_TYPE_KEY];
                if ( Guid.TryParse( groupTypeValue, out Guid groupTypeGuid ) )
                {
                    var groupType = GroupTypeCache.Get( groupTypeGuid );
                    publicConfigurationValues[GROUP_TYPE_KEY] = new ListItemBag()
                    {
                        Text = groupType?.Name,
                        Value = groupTypeValue
                    }.ToCamelCaseJson( false, true );

                    // If in Edit mode add GroupType if any so we get its locations.
                    if ( usage == ConfigurationValueUsage.Edit && groupType != null )
                    {
                        groupTypes.Add( groupType );
                    }
                }
            }

            // If in Configure mode get all GroupTypes so we can get their locations
            if ( usage == ConfigurationValueUsage.Configure )
            {
                groupTypes = GroupTypeCache.All();
            }

            var locationTypes = new Dictionary<string, string>();
            foreach ( var groupType in groupTypes )
            {
                var locationTypeValues = groupType.LocationTypeValues.ConvertAll( g => new ListItemBag() { Text = g.Value, Value = g.Guid.ToString() } );
                locationTypes.Add( groupType.Guid.ToString(), locationTypeValues.ToCamelCaseJson( false, true ) );
            }

            publicConfigurationValues.Add( GROUP_TYPE_LOCATIONS_KEY, locationTypes.ToCamelCaseJson( false, true ) );

            return publicConfigurationValues;
        }

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            Guid? guid = privateValue.AsGuidOrNull();

            if ( !guid.HasValue )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var definedValueId = new DefinedValueService( rockContext ).GetId( guid.Value );

                if ( !definedValueId.HasValue )
                {
                    return null;
                }

                return new List<ReferencedEntity>
                {
                    new ReferencedEntity( EntityTypeCache.GetId<DefinedValue>().Value, definedValueId.Value )
                };
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the Name property of a Defined Value and
            // should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<DefinedValue>().Value, nameof( DefinedValue.Value ) )
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
            configKeys.Add( GROUP_TYPE_KEY );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // build a drop down list of group types (the one that gets selected is
            // used to build a list of group location type defined values that the
            // group type allows) 
            var ddlGroupType = new RockDropDownList();
            controls.Add( ddlGroupType );
            ddlGroupType.AutoPostBack = true;
            ddlGroupType.SelectedIndexChanged += OnQualifierUpdated;
            ddlGroupType.Label = "Group Type";
            ddlGroupType.Help = "The Group Type to select location types from.";

            Rock.Model.GroupTypeService groupTypeService = new Model.GroupTypeService( new RockContext() );
            foreach ( var groupType in groupTypeService.Queryable().AsNoTracking().OrderBy( g => g.Name ).Select( a => new { a.Name, a.Guid } ) )
            {
                ddlGroupType.Items.Add( new ListItem( groupType.Name, groupType.Guid.ToString() ) );
            }

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
            configurationValues.Add( GROUP_TYPE_KEY, new ConfigurationValue( "Group Type", "The Group Type to select location types from.", "" ) );

            if ( controls != null && controls.Count == 1 )
            {
                DropDownList ddlGroupType = controls[0] as DropDownList;
                if ( ddlGroupType != null )
                {
                    configurationValues[GROUP_TYPE_KEY].Value = ddlGroupType.SelectedValue;
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
            if ( controls != null && controls.Count == 1 && configurationValues != null )
            {
                DropDownList ddlGroupType = controls[0] as DropDownList;
                if ( ddlGroupType != null )
                {
                    ddlGroupType.SelectedValue = configurationValues.GetValueOrNull( GROUP_TYPE_KEY );
                }
            }
        }

        /// <summary>
        /// Returns the field's current value
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

            editControl = new Rock.Web.UI.Controls.RockDropDownList { ID = id };
            editControl.Items.Add( new ListItem() );

            if ( configurationValues != null && configurationValues.ContainsKey( GROUP_TYPE_KEY ) )
            {
                Guid? groupTypeGuid = configurationValues.GetValueOrNull( GROUP_TYPE_KEY ).AsGuidOrNull();
                if ( groupTypeGuid != null )
                {
                    var groupType = GroupTypeCache.Get( groupTypeGuid.Value );
                    if ( groupType != null )
                    {
                        var locationTypeValues = groupType.LocationTypeValues;
                        if ( locationTypeValues != null )
                        {
                            foreach ( var locationTypeValue in locationTypeValues )
                            {
                                editControl.Items.Add( new ListItem( locationTypeValue.Value, locationTypeValue.Id.ToString() ) );
                            }

                        }
                    }
                }
            }

            return editControl;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as RockDropDownList;
            if ( picker != null )
            {
                DefinedValueCache definedValue = null;
                int? definedValueId = picker.SelectedValue.AsIntegerOrNull();
                if ( definedValueId.HasValue )
                {
                    definedValue = DefinedValueCache.Get( definedValueId.Value );
                }

                return definedValue?.Guid.ToString() ?? string.Empty;
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
            var picker = control as RockDropDownList;
            if ( picker != null )
            {
                DefinedValueCache definedValue = null;
                Guid? definedValueGuid = value.AsGuidOrNull();
                if ( definedValueGuid.HasValue )
                {
                    definedValue = DefinedValueCache.Get( definedValueGuid.Value );
                }

                picker.SetValue( definedValue?.Id );
            }
        }

#endif
        #endregion
    }
}