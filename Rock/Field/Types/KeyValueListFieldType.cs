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
using System.Web;
using System.Web.UI;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a key/value list
    /// </summary>
    [Serializable]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [IconSvg( @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 16 16""><path d=""M13.25,1.88H2.75A1.74,1.74,0,0,0,1,3.62v8.76a1.74,1.74,0,0,0,1.75,1.74h10.5A1.74,1.74,0,0,0,15,12.38V3.62A1.74,1.74,0,0,0,13.25,1.88Zm.44,1.74v2H6V3.19h7.22A.44.44,0,0,1,13.69,3.62ZM6,6.91h7.66V9.09H6ZM4.72,9.09H2.31V6.91H4.72Zm-2-5.9h2v2.4H2.31v-2A.44.44,0,0,1,2.75,3.19Zm-.44,9.19v-2H4.72v2.4h-2A.44.44,0,0,1,2.31,12.38Zm10.94.43H6v-2.4h7.66v2A.44.44,0,0,1,13.25,12.81Z""/></svg>" )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.KEY_VALUE_LIST )]
    public class KeyValueListFieldType : ValueListFieldType, IEntityReferenceFieldType
    {
        private const string VALUES_KEY = "values";
        private const string DEFINED_TYPES_PROPERTY_KEY = "definedTypes";

        #region Configuration

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Insert(0, "keyprompt" );
            configKeys.Insert( 0, "displayvaluefirst" );
            return configKeys;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicEditConfigurationProperties( Dictionary<string, string> privateConfigurationValues )
        {
            var configurationProperties = base.GetPublicEditConfigurationProperties( privateConfigurationValues );

            // Get a list of all DefinedTypes that can be selected.
            var definedTypes = DefinedTypeCache.All()
                .OrderBy( t => t.Name )
                .Select( t => new ListItemBag
                {
                    Value = t.Guid.ToString(),
                    Text = t.Name
                } )
                .ToList();

            configurationProperties[DEFINED_TYPES_PROPERTY_KEY] = definedTypes.ToCamelCaseJson( false, true );

            return configurationProperties;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string privateValue )
        {
            var publicConfigurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, privateValue );

            var options = GetCustomValues( privateConfigurationValues.ToDictionary( k => k.Key, k => new ConfigurationValue( k.Value ) ) )
                .Select( kvp => new
                {
                    value = kvp.Key,
                    text = kvp.Value
                } )
                .ToCamelCaseJson( false, true );

            publicConfigurationValues[VALUES_KEY] = options;

            if ( usage != ConfigurationValueUsage.Configure )
            {
                publicConfigurationValues.Remove( "definedtype" );
                publicConfigurationValues.Remove( "customvalues" );
            }

            if ( publicConfigurationValues.ContainsKey( "definedtype" ) )
            {
                var definedTypeId = publicConfigurationValues["definedtype"].AsIntegerOrNull();

                if ( definedTypeId.HasValue )
                {
                    publicConfigurationValues["definedtype"] = DefinedTypeCache.Get( definedTypeId.Value )?.Guid.ToString() ?? "";
                }
                else
                {
                    publicConfigurationValues["definedtype"] = "";
                }
            }

            return publicConfigurationValues;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPrivateConfigurationValues( Dictionary<string, string> publicConfigurationValues )
        {
            var privateConfigurationValues = base.GetPrivateConfigurationValues( publicConfigurationValues );

            // Don't allow them to provide the actual value items.
            if ( privateConfigurationValues.ContainsKey( VALUES_KEY ) )
            {
                privateConfigurationValues.Remove( VALUES_KEY );
            }

            // Convert the defined type value from Guid to Id.
            if ( privateConfigurationValues.ContainsKey( "definedtype" ) )
            {
                var definedTypeGuid = privateConfigurationValues["definedtype"].AsGuidOrNull();

                if ( definedTypeGuid.HasValue )
                {
                    privateConfigurationValues["definedtype"] = DefinedTypeCache.Get( definedTypeGuid.Value )?.Id.ToString() ?? "";
                }
                else
                {
                    privateConfigurationValues["definedtype"] = "";
                }
            }

            return privateConfigurationValues;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            var tbKeyPrompt = new RockTextBox();
            controls.Insert(0, tbKeyPrompt );
            tbKeyPrompt.AutoPostBack = true;
            tbKeyPrompt.TextChanged += OnQualifierUpdated;
            tbKeyPrompt.Label = "Key Prompt";
            tbKeyPrompt.Help = "The text to display as a prompt in the key textbox.";

            var cbDisplayValueFirst = new RockCheckBox();
            controls.Insert( 5, cbDisplayValueFirst );
            cbDisplayValueFirst.Label = "Display Value First";
            cbDisplayValueFirst.Help = "Reverses the display order of the key and the value.";

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
            configurationValues.Add( "keyprompt", new ConfigurationValue( "Key Prompt", "The text to display as a prompt in the key textbox.", "" ) );
            configurationValues.Add( "valueprompt", new ConfigurationValue( "Label Prompt", "The text to display as a prompt in the label textbox.", "" ) );
            configurationValues.Add( "definedtype", new ConfigurationValue( "Defined Type", "Optional Defined Type to select values from, otherwise values will be free-form text fields", "" ) );
            configurationValues.Add( "customvalues", new ConfigurationValue( "Custom Values", "Optional list of options to use for the values.  Format is either 'value1,value2,value3,...', or 'value1^text1,value2^text2,value3^text3,...'.", "" ) );
            configurationValues.Add( "allowhtml", new ConfigurationValue( "Allow HTML", "Allow HTML content in values", "" ) );
            configurationValues.Add( "displayvaluefirst", new ConfigurationValue( "Display Value First", "Reverses the display order of the key and the value.", "" ) );

            if ( controls != null )
            {
                if ( controls.Count > 0 && controls[0] != null && controls[0] is RockTextBox   )
                {
                    configurationValues["keyprompt"].Value = ( (RockTextBox)controls[0] ).Text;
                }
                if ( controls.Count > 1 && controls[1] != null && controls[1] is RockTextBox  )
                {
                    configurationValues["valueprompt"].Value = ( (RockTextBox)controls[1] ).Text;
                }
                if ( controls.Count > 2 && controls[2] != null && controls[2] is RockDropDownList  )
                {
                    configurationValues["definedtype"].Value = ( (RockDropDownList)controls[2] ).SelectedValue;
                }
                if ( controls.Count > 3 && controls[3] != null && controls[3] is RockTextBox )
                {
                    configurationValues["customvalues"].Value = ( (RockTextBox)controls[3] ).Text;
                }
                if ( controls.Count > 4 && controls[4] != null && controls[4] is RockCheckBox )
                {
                    configurationValues["allowhtml"].Value = ( ( RockCheckBox ) controls[4] ).Checked.ToString();
                }
                if ( controls.Count > 5 && controls[5] != null && controls[5] is RockCheckBox )
                {
                    configurationValues["displayvaluefirst"].Value = ( (RockCheckBox)controls[5] ).Checked.ToString();
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
                if ( controls.Count > 0 && controls[0] != null && controls[0] is RockTextBox && configurationValues.ContainsKey( "keyprompt" ) )
                {
                    ( (RockTextBox)controls[0] ).Text = configurationValues["keyprompt"].Value;
                }
                if ( controls.Count > 1 && controls[1] != null && controls[1] is RockTextBox && configurationValues.ContainsKey( "valueprompt" ) )
                {
                    ( (RockTextBox)controls[1] ).Text = configurationValues["valueprompt"].Value;
                }
                if ( controls.Count > 2 && controls[2] != null && controls[2] is RockDropDownList && configurationValues.ContainsKey( "definedtype" ) )
                {
                    ( (RockDropDownList)controls[2] ).SelectedValue = configurationValues["definedtype"].Value;
                }
                if ( controls.Count > 3 && controls[3] != null && controls[3] is RockTextBox && configurationValues.ContainsKey( "customvalues" ) )
                {
                   ( (RockTextBox)controls[3] ).Text = configurationValues["customvalues"].Value;
                }
                if ( controls.Count > 4 && controls[4] != null && controls[4] is RockCheckBox && configurationValues.ContainsKey( "allowhtml" ) )
                {
                    ( ( RockCheckBox ) controls[4] ).Checked = configurationValues["allowhtml"].Value.AsBoolean();
                }
                if ( controls.Count > 5 && controls[5] != null && controls[5] is RockCheckBox && configurationValues.ContainsKey( "displayvaluefirst" ) )
                {
                    ( (RockCheckBox)controls[5] ).Checked = configurationValues["displayvaluefirst"].Value.AsBoolean();
                }
            }
        }

        /// <summary>
        /// Gets the custom values that have been defined. These reflect either the
        /// defined type values or the custom options entered into the custom values
        /// text box.
        /// </summary>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetCustomValues( Dictionary<string, ConfigurationValue> configurationValues )
        {
            var definedTypeId = configurationValues.GetConfigurationValueAsString( "definedtype" ).AsIntegerOrNull();

            if ( definedTypeId.HasValue )
            {
                var definedType = DefinedTypeCache.Get( definedTypeId.Value );

                if ( definedType != null )
                {
                    return definedType.DefinedValues
                        .ToDictionary( v => v.Id.ToString(), v => v.Value );
                }
            }

            return Helper.GetConfiguredValues( configurationValues, "customvalues" );
        }

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string value, Dictionary<string, string> configurationValues )
        {
            bool isDefinedType = configurationValues != null && configurationValues.ContainsKey( "definedtype" ) && configurationValues["definedtype"].AsIntegerOrNull().HasValue;

            var values = new List<string>();
            string[] nameValues = value?.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ) ?? new string[0];

            foreach ( string nameValue in nameValues )
            {
                string[] nameAndValue = nameValue.Split( new char[] { '^' } );

                // url decode array items just in case they were UrlEncoded (in the KeyValueList controls)
                nameAndValue = nameAndValue.Select( s => HttpUtility.UrlDecode( s ) ).ToArray();

                if ( nameAndValue.Length == 2 )
                {
                    if ( isDefinedType )
                    {
                        var definedValue = DefinedValueCache.Get( nameAndValue[1].AsInteger() );
                        if ( definedValue != null )
                        {
                            nameAndValue[1] = definedValue.Value;
                        }
                    }
                    else
                    {
                        var customValues = GetCustomValues( configurationValues.ToDictionary( k => k.Key, k => new ConfigurationValue( k.Value ) ) );
                        if ( customValues.ContainsKey( nameAndValue[1] ) )
                        {
                            nameAndValue[1] = customValues[nameAndValue[1]];
                        }
                    }

                    values.Add( string.Format( "{0}: {1}", nameAndValue[0], nameAndValue[1] ) );
                }
                else
                {
                    values.Add( nameValue );
                }
            }

            return values.AsDelimited( ", " );
        }

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues"></param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            // Never use condensed format for webforms.
            return GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

        #endregion

        #region Edit Control

        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var nameValues = privateValue?.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ) ?? new string[0];

            return nameValues
                .Select( nv => nv.Split( new char[] { '^' } ) )
                .Where( nv => nv.Length == 2 )
                .Select( nv => new PublicValue
                {
                    Key = HttpUtility.UrlDecode( nv[0] ),
                    Value = HttpUtility.UrlDecode( nv[1] )
                } )
                .ToCamelCaseJson( false, true );
        }

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            var values = publicValue.FromJsonOrNull<List<PublicValue>>();

            if ( values == null )
            {
                return string.Empty;
            }

            var customValues = GetCustomValues( privateConfigurationValues.ToDictionary( k => k.Key, k => new ConfigurationValue( k.Value ) ) );

            // If there are any custom values, then ensure that all values we
            // got from the public device are valid. If not, ignore them.
            if ( customValues.Any() )
            {
                values = values
                    .Where( v => customValues.ContainsKey( v.Value ) )
                    .ToList();
            }

            return values.Select( v => $"{HttpUtility.UrlEncode( v.Key )}^{HttpUtility.UrlEncode( v.Value )}" )
                .JoinStrings( "|" );
        }

        /// <summary>
        /// Edits the control.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override ValueList EditControl( string id )
        {
            return new KeyValueList { ID = id };
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
            var control = base.EditControl( configurationValues, id ) as KeyValueList;

            if ( configurationValues != null )
            {
                if ( configurationValues.ContainsKey( "keyprompt" ) )
                {
                    control.KeyPrompt = configurationValues["keyprompt"].Value;
                }

                if ( configurationValues.ContainsKey( "displayvaluefirst" ) )
                {
                    control.DisplayValueFirst = configurationValues["displayvaluefirst"].Value.AsBoolean();
                }
            }

            return control;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as KeyValueList;
            if ( picker != null )
            {
                return picker.Value;
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
            var picker = control as KeyValueList;
            if ( picker != null )
            {
                picker.Value = value;
            }
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Creates the control needed to filter (query) values using this field type.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override System.Web.UI.Control FilterControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, Rock.Reporting.FilterMode filterMode )
        {
            // This field type does not support filtering
            return null;
        }

        /// <summary>
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the values from string.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        public List<KeyValuePair<string, object>> GetValuesFromString( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            List<KeyValuePair<string, object>> values = new List<KeyValuePair<string, object>>();

            bool isDefinedType = configurationValues != null && configurationValues.ContainsKey( "definedtype" ) && configurationValues["definedtype"].Value.AsIntegerOrNull().HasValue;

            string[] nameValues = value.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );

            // url decode array items just in case they were UrlEncoded (in the KeyValueList controls)
            nameValues = nameValues.Select( s => HttpUtility.UrlDecode( s ) ).ToArray();

            foreach ( string nameValue in nameValues )
            {
                string[] nameAndValue = nameValue.Split( new char[] { '^' } );
                if ( nameAndValue.Length == 2 )
                {
                    if ( isDefinedType )
                    {
                        var definedValue = DefinedValueCache.Get( nameAndValue[1].AsInteger() );
                        if ( definedValue != null )
                        {
                            values.Add( new KeyValuePair<string, object>( nameAndValue[0], definedValue ) );
                        }
                        else
                        {
                            values.Add( new KeyValuePair<string, object>( nameAndValue[0], nameAndValue[1] ) );
                        }
                    }
                    else
                    {
                        values.Add( new KeyValuePair<string, object>( nameAndValue[0], nameAndValue[1] ) );
                    }
                }
                else
                {
                    values.Add( new KeyValuePair<string, object>( nameAndValue[0], null ) );
                }
            }

            return values;
        }

        #endregion

        #region Persistence

        /// <inheritdoc/>
        public override bool IsPersistedValueInvalidated( Dictionary<string, string> oldPrivateConfigurationValues, Dictionary<string, string> newPrivateConfigurationValues )
        {
            var oldDefinedType = oldPrivateConfigurationValues.GetValueOrNull( "definedtype" ) ?? string.Empty;
            var newDefinedType = newPrivateConfigurationValues.GetValueOrNull( "definedtype" ) ?? string.Empty;
            var oldCustomValues = oldPrivateConfigurationValues.GetValueOrNull( "customvalues" ) ?? string.Empty;
            var newCustomValues = newPrivateConfigurationValues.GetValueOrNull( "customvalues" ) ?? string.Empty;

            if ( oldDefinedType != newDefinedType )
            {
                return true;
            }

            if ( oldCustomValues != newCustomValues )
            {
                return true;
            }

            return false;
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

            var isDefinedType = privateConfigurationValues.GetValueOrDefault( "definedtype", "" ).AsIntegerOrNull().HasValue;

            if ( !isDefinedType )
            {
                return null;
            }

            var entityReferences = new List<ReferencedEntity>();
            var definedValueEntityTypeId = EntityTypeCache.GetId<DefinedValue>().Value;
            var nameValues = privateValue?.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ) ?? new string[0];

            foreach ( var nameValue in nameValues )
            {
                var nameAndValue = nameValue.Split( new char[] { '^' } );

                // Url decode array items just in case they were UrlEncoded (in the KeyValueList controls).
                nameAndValue = nameAndValue.Select( s => HttpUtility.UrlDecode( s ) ).ToArray();

                if ( nameAndValue.Length == 2 )
                {
                    var definedValue = DefinedValueCache.Get( nameAndValue[1].AsInteger() );

                    if ( definedValue != null )
                    {
                        entityReferences.Add( new ReferencedEntity( definedValueEntityTypeId, definedValue.Id ) );
                    }
                }
            }

            return entityReferences;
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            var isDefinedType = privateConfigurationValues.GetValueOrDefault( "definedtype", "" ).AsIntegerOrNull().HasValue;

            if ( !isDefinedType )
            {
                return new List<ReferencedProperty>();
            }

            // This field type references the Value property of a DefinedValue and
            // should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<DefinedValue>().Value, nameof( DefinedValue.Value ) )
            };
        }

        #endregion

        /// <summary>
        /// Represents a single element value (presented as a row when editing)
        /// formatted in a way the public devices will understand.
        /// </summary>
        private class PublicValue
        {
            /// <summary>
            /// Gets or sets the key.
            /// </summary>
            /// <value>
            /// The key.
            /// </value>
            public string Key { get; set; }

            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            public string Value { get; set; }
        }
    }
}