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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a dropdown list of genders
    /// </summary>
    [Serializable]
    [RockPlatformSupport( Utility.RockPlatform.WebForms | Utility.RockPlatform.Obsidian )]
    [IconSvg( @"<svg id=""Layer_1"" data-name=""Layer 1"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 16 16""><defs><style>.cls-1{fill:#343a40;}</style></defs><path class=""cls-1"" d=""M6.87,3.12A1.13,1.13,0,1,1,8,4.25,1.13,1.13,0,0,1,6.87,3.12ZM8.19,5h.16a2.25,2.25,0,0,1,1.93,1.09l1.36,2.27a.75.75,0,0,1-1.28.78L9.69,8v5.23a.75.75,0,0,1-1.5,0ZM5.61,6.27A2.65,2.65,0,0,1,7.81,5v8.25a.75.75,0,0,1-1.5,0V11H5.9a.37.37,0,0,1-.36-.49l.9-2.7-.8,1.33a.75.75,0,1,1-1.28-.78Z""/></svg>" )]
    public class GenderFieldType : FieldType
    {
        #region Configuration
        private const string HIDE_UNKNOWN_GENDER_KEY = "hideUnknownGender";

        /// <inheritdoc />
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( HIDE_UNKNOWN_GENDER_KEY );
            return configKeys;
        }

        /// <inheritdoc />
        public override List<Control> ConfigurationControls()
        {
            // Add checkbox for deciding if the list should include inactive items
            var cbHideUnknownGender = new RockCheckBox();
            cbHideUnknownGender.AutoPostBack = true;
            cbHideUnknownGender.CheckedChanged += OnQualifierUpdated;
            cbHideUnknownGender.Label = "Hide Unknown Gender";
            cbHideUnknownGender.Text = "Yes";
            cbHideUnknownGender.Help = "When set the 'Unknown' option will not appear in the list of genders.";

            var controls = base.ConfigurationControls();
            controls.Add( cbHideUnknownGender );
            return controls;
        }

        /// <inheritdoc />
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();
            configurationValues.Add( HIDE_UNKNOWN_GENDER_KEY, new ConfigurationValue( "Hide Unknown Gender", "When set the 'Unknown' option will not appear in the list of genders.", string.Empty ) );

            if ( controls == null )
            {
                return configurationValues;
            }

            CheckBox cbHideUnknownGender = controls.Count > 0 ? controls[0] as CheckBox : null;

            if ( cbHideUnknownGender != null )
            {
                configurationValues[HIDE_UNKNOWN_GENDER_KEY].Value = cbHideUnknownGender.Checked.ToString();
            }

            return configurationValues;
        }

        /// <inheritdoc />
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( controls == null && configurationValues == null )
            {
                return;
            }

            CheckBox cbHideUnknownGender = controls.Count > 0 ? controls[0] as CheckBox : null;

            if ( cbHideUnknownGender != null )
            {
                cbHideUnknownGender.Checked = configurationValues.GetValueOrNull( HIDE_UNKNOWN_GENDER_KEY ).AsBooleanOrNull() ?? false;
            }
        }

        #endregion Configuration

        #region Edit Control

        /// <inheritdoc/>
        public override string GetTextValue( string value, Dictionary<string, string> configurationValues )
        {
            if (value.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            if ( configurationValues.GetValueOrNull( HIDE_UNKNOWN_GENDER_KEY ).AsBooleanOrNull() ?? false )
            {
                return value.ConvertToEnum<Gender>().ConvertToString();
            }

            return value.ConvertToEnum<Gender>( Gender.Unknown ).ConvertToString();
        }

        /// <inheritdoc />
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return !condensed
                ? GetTextValue( value, configurationValues.ToDictionary( k => k.Key, k => k.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( k => k.Key, k => k.Value.Value ) );
        }

        /// <inheritdoc />
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var ddl = new RockDropDownList
            {
                ID = id,
                CssClass = "input-width-md"
            };

            ddl.Items.Add( new ListItem() );
            foreach ( Gender gender in Enum.GetValues( typeof( Gender ) ) )
            {
                ddl.Items.Add( new ListItem( gender.ConvertToString(), gender.ConvertToInt().ToString() ) );
            }

            if ( configurationValues != null )
            {
                if ( configurationValues.ContainsKey( HIDE_UNKNOWN_GENDER_KEY ) && configurationValues[HIDE_UNKNOWN_GENDER_KEY].Value.AsBoolean() )
                {
                    ddl.Items.Remove( new ListItem( Gender.Unknown.ConvertToString(), Gender.Unknown.ConvertToInt().ToString() ) );
                }
            }

            return ddl;
        }

        /// <inheritdoc />
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var editControl = control as ListControl;
            return editControl?.SelectedValue ?? string.Empty;
        }

        /// <inheritdoc />
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var editControl = control as ListControl;
            editControl?.SetValue( value );
        }

        #endregion

    }
}