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
    /// Field Type used to display a dropdown list of System Phone Numbers.
    /// Stored as either a single Guid or a comma-delimited list of Guids (if AllowMultiple).
    /// </summary>
    [Serializable]
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.SYSTEM_PHONE_NUMBER )]
    public class SystemPhoneNumberFieldType : FieldType, IEntityFieldType, ICachedEntitiesFieldType, IEntityReferenceFieldType
    {
        #region Configuration

        private const string ALLOW_MULTIPLE_KEY = "allowMultiple";
        private const string INCLUDE_INACTIVE_KEY = "includeInactive";
        private const string REPEAT_COLUMNS_KEY = "repeatColumns";

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string value, Dictionary<string, string> configurationValues )
        {
            string formattedValue = string.Empty;

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                var names = new List<string>();
                foreach ( Guid guid in value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList() )
                {
                    var systemPhoneNumber = SystemPhoneNumberCache.Get( guid );
                    if ( systemPhoneNumber != null )
                    {
                        names.Add(systemPhoneNumber.Name );
                    }
                }

                formattedValue = names.AsDelimited( ", " );
            }

            return formattedValue;
        }

        #endregion

        #region Edit Control

        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var guids = privateValue.SplitDelimitedValues().AsGuidList();

            var systemPhoneNumbers = new List<SystemPhoneNumberCache>();
            foreach ( var guid in guids )
            {
                var systemPhoneNumber = SystemPhoneNumberCache.Get( guid );
                if ( systemPhoneNumber != null )
                {
                    systemPhoneNumbers.Add( systemPhoneNumber );
                }
            }

            return new ListItemBag
            {
                Value = privateValue,
                Text = systemPhoneNumbers.Select( v => v.Name ).JoinStrings( ", " )
            }.ToCamelCaseJson( false, true );
        }

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            var value = publicValue.FromJsonOrNull<ListItemBag>();

            return value?.Value ?? string.Empty;
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
            Guid? guid = value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                rockContext = rockContext ?? new RockContext();
                return new SystemPhoneNumberService( rockContext ).Get( guid.Value );
            }

            return null;
        }

        #endregion

        #region ICachedEntitiesFieldType

        /// <summary>
        /// Gets the cached defined values.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public List<IEntityCache> GetCachedEntities( string value )
        {
            var systemPhoneNumbers = new List<IEntityCache>();

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                foreach ( Guid guid in value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList() )
                {
                    var systemPhoneNumber = SystemPhoneNumberCache.Get( guid );
                    if ( systemPhoneNumber != null )
                    {
                        systemPhoneNumbers.Add( systemPhoneNumber );
                    }
                }
            }

            return systemPhoneNumbers;
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

            var systemPhoneNumberEntityTypeId = EntityTypeCache.GetId<SystemPhoneNumber>().Value;

            return privateValue
                .Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries )
                .AsGuidList()
                .Select( g => SystemPhoneNumberCache.Get( g ) )
                .Where( spn => spn != null )
                .Select( spn => new ReferencedEntity( systemPhoneNumberEntityTypeId, spn.Id ) )
                .ToList();
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the Name property of a SystemPhoneNumber
            // and should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<SystemPhoneNumber>().Value, nameof( SystemPhoneNumber.Name ) )
            };
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Returns a list of the configuration keys.
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();

            configKeys.Add( ALLOW_MULTIPLE_KEY );
            configKeys.Add( INCLUDE_INACTIVE_KEY );
            configKeys.Add( REPEAT_COLUMNS_KEY );

            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // Add checkbox for deciding if the system phone numbers list is
            // rendered as a drop down list or a checkbox list.
            var cbAllowMultipleValues = new RockCheckBox
            {
                AutoPostBack = true,
                Label = "Allow Multiple Values",
                Help = "When set, allows multiple system phone numbers to be selected."
            };

            cbAllowMultipleValues.CheckedChanged += OnQualifierUpdated;

            // Add checkbox for deciding if the list should include inactive items
            var cbIncludeInactive = new RockCheckBox
            {
                AutoPostBack = true,
                Label = "Include Inactive",
                Help = "When set, inactive system phone numbers will be included in the list."
            };

            cbIncludeInactive.CheckedChanged += OnQualifierUpdated;

            var tbRepeatColumns = new NumberBox
            {
                AutoPostBack = true,
                Label = "Repeat Columns",
                Help = "Select how many columns the list should use before going to the next row. If 0 then the options are put next to each other and wrap around. If blank then 4 columns will be displayed. There is no upper limit enforced here however the block this is used in might add contraints due to available space.",
                MinimumValue = "0"
            };

            tbRepeatColumns.TextChanged += OnQualifierUpdated;

            controls.Add( cbAllowMultipleValues );
            controls.Add( cbIncludeInactive );
            controls.Add( tbRepeatColumns );

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>
            {
                { ALLOW_MULTIPLE_KEY, new ConfigurationValue( "Allow Multiple Values", "When set, allows multiple system phone numbers to be selected.", string.Empty ) },
                { INCLUDE_INACTIVE_KEY, new ConfigurationValue( "Include Inactive", "When set, inactive system phone numbers will be included in the list.", string.Empty ) },
                { REPEAT_COLUMNS_KEY, new ConfigurationValue( "Repeat Columns", "Select how many columns the list should use before going to the next row, if not set 4 is used. This setting has no effect if 'Enhance For Long Lists' is selected since that will not use a checkbox list.", string.Empty ) }
            };

            if ( controls != null )
            {
                CheckBox cbAllowMultipleValues = controls.Count > 0 ? controls[0] as CheckBox : null;
                CheckBox cbIncludeInactive = controls.Count > 1 ? controls[1] as CheckBox : null;
                NumberBox nbRepeatColumns = controls.Count > 2 ? controls[2] as NumberBox : null;

                if ( cbAllowMultipleValues != null )
                {
                    configurationValues[ALLOW_MULTIPLE_KEY].Value = cbAllowMultipleValues.Checked.ToString();
                }

                if ( cbIncludeInactive != null )
                {
                    configurationValues[INCLUDE_INACTIVE_KEY].Value = cbIncludeInactive.Checked.ToString();
                }

                if ( nbRepeatColumns != null )
                {
                    configurationValues[REPEAT_COLUMNS_KEY].Value = nbRepeatColumns.Text;
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
            if ( controls != null && configurationValues != null )
            {
                CheckBox cbAllowMultipleValues = controls.Count > 0 ? controls[0] as CheckBox : null;
                CheckBox cbIncludeInactive = controls.Count > 1 ? controls[1] as CheckBox : null;
                NumberBox nbRepeatColumns = controls.Count > 2 ? controls[2] as NumberBox : null;

                if ( cbAllowMultipleValues != null )
                {
                    cbAllowMultipleValues.Checked = configurationValues.GetValueOrNull( ALLOW_MULTIPLE_KEY ).AsBooleanOrNull() ?? false;
                }

                if ( cbIncludeInactive != null )
                {
                    cbIncludeInactive.Checked = configurationValues.GetValueOrNull( INCLUDE_INACTIVE_KEY ).AsBooleanOrNull() ?? false;
                }

                if ( nbRepeatColumns != null )
                {
                    nbRepeatColumns.Text = configurationValues.GetValueOrNull( REPEAT_COLUMNS_KEY );
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
        /// Returns the value that should be used for sorting, using the most appropriate datatype
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override object SortValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                // if there are multiple defined values, just pick the first one as the sort value
                var guid = value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList().FirstOrDefault();
                var systemPhoneNumber = SystemPhoneNumberCache.Get( guid );

                if ( systemPhoneNumber != null )
                {
                    return systemPhoneNumber.Name;
                }
            }

            return base.SortValue( parentControl, value, configurationValues );
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
            if ( configurationValues == null )
            {
                return null;
            }

            Control editControl;

            int repeatColumns = ( configurationValues.ContainsKey( REPEAT_COLUMNS_KEY ) ? configurationValues[REPEAT_COLUMNS_KEY].Value.AsIntegerOrNull() : null ) ?? 4;
            bool allowMultiple = configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) && configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean();
            bool includeInactive = configurationValues.ContainsKey( INCLUDE_INACTIVE_KEY ) && configurationValues[INCLUDE_INACTIVE_KEY].Value.AsBoolean();

            if ( allowMultiple )
            {
                editControl = new SystemPhoneNumbersPicker
                {
                    ID = id,
                    RepeatColumns = repeatColumns,
                    IncludeInactive = includeInactive
                };
            }
            else
            {
                editControl = new SystemPhoneNumberPicker
                {
                    ID = id,
                    IncludeInactive = includeInactive
                };
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
            var systemPhoneNumberIdList = new List<int>();

            if ( control is SystemPhoneNumberPicker numberPicker )
            {
                if ( numberPicker.SelectedSystemPhoneNumberId.HasValue )
                {
                    systemPhoneNumberIdList.Add( numberPicker.SelectedSystemPhoneNumberId.Value );
                }
            }
            else if ( control is SystemPhoneNumbersPicker numbersPicker )
            {
                systemPhoneNumberIdList = numbersPicker.SelectedValuesAsInt.ToList();
            }

            var guids = new List<Guid>();

            foreach ( int systemPhoneNumberId in systemPhoneNumberIdList )
            {
                var systemPhoneNumber = SystemPhoneNumberCache.Get( systemPhoneNumberId );
                if ( systemPhoneNumber != null )
                {
                    guids.Add( systemPhoneNumber.Guid );
                }
            }

            return guids.AsDelimited( "," );
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
                var ids = new List<int>();
                foreach ( Guid guid in value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList() )
                {
                    var systemPhoneNumber = SystemPhoneNumberCache.Get( guid );
                    if ( systemPhoneNumber != null )
                    {
                        ids.Add( systemPhoneNumber.Id );
                    }
                }

                if ( control is SystemPhoneNumberPicker numberPicker )
                {
                    if ( ids.Any() )
                    {
                        numberPicker.SelectedSystemPhoneNumberId = ids.First();
                    }
                }
                else if ( control is SystemPhoneNumbersPicker numbersPicker )
                {
                    numbersPicker.SelectedSystemPhoneNumberIds = ids.ToArray();
                }
            }
        }

        /// <summary>
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return false;
        }

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            Guid guid = GetEditValue( control, configurationValues ).AsGuid();
            var item = SystemPhoneNumberCache.Get( guid );

            return item?.Id;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            SystemPhoneNumberCache item = null;

            if ( id.HasValue )
            {
                item = SystemPhoneNumberCache.Get( id.Value );
            }

            string guidValue = item != null ? item.Guid.ToString() : string.Empty;

            SetEditValue( control, configurationValues, guidValue );
        }

#endif
        #endregion
    }
}