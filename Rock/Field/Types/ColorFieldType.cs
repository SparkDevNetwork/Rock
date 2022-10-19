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
using System.Reflection;
#if WEBFORMS
using System.Web.UI;
using System.Web.UI.WebControls;
#endif
using Rock.Attribute;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a dropdown list of System.Drawing.Color options
    /// </summary>
    [Serializable]
    [RockPlatformSupport( Utility.RockPlatform.WebForms | Utility.RockPlatform.Obsidian )]
    [IconSvg( @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 16 16""><path d=""M4.48,7.12A.87.87,0,0,0,3.6,8a.88.88,0,1,0,.88-.88Zm8-4.53A7.05,7.05,0,0,0,6.59,1.14,6.94,6.94,0,0,0,1.14,6.56,7,7,0,0,0,3,12.87,6.58,6.58,0,0,0,7.58,15l.72,0a2,2,0,0,0,1.53-1.13,2.3,2.3,0,0,0,0-2.09,1,1,0,0,1,.05-1,1,1,0,0,1,.87-.5h2A2.26,2.26,0,0,0,15,8,7,7,0,0,0,12.45,2.59Zm.29,6.28h-2a2.34,2.34,0,0,0-2,1.13,2.37,2.37,0,0,0-.09,2.32,1,1,0,0,1,0,.9.79.79,0,0,1-.56.43A5.05,5.05,0,0,1,3.94,12,5.71,5.71,0,0,1,2.43,6.85,5.57,5.57,0,0,1,6.84,2.47a5.73,5.73,0,0,1,4.77,1.18A5.65,5.65,0,0,1,13.69,8,.94.94,0,0,1,12.74,8.87ZM5.35,4.48a.87.87,0,1,0,.88.87A.85.85,0,0,0,5.35,4.48ZM8,3.62a.88.88,0,1,0,.87.87A.86.86,0,0,0,8,3.62Zm2.62.86a.87.87,0,1,0,.88.87A.85.85,0,0,0,10.6,4.48Z""/></svg>" )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.COLOR )]
    public class ColorFieldType : FieldType
    {
        #region Configuration

        private const string SELECTION_TYPE = "selectiontype";
        private const string COLOR_PICKER = "Color Picker";
        private const string NAMED_COLOR = "Named Color";

        #endregion

        #region Edit Control

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
            configKeys.Add( SELECTION_TYPE );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            var ddl = new RockDropDownList();
            controls.Add( ddl );
            ddl.AutoPostBack = true;
            ddl.SelectedIndexChanged += OnQualifierUpdated;
            ddl.Label = "Selection Type";
            ddl.Help = "The type of control to select color.";
            ddl.Items.Add( COLOR_PICKER );
            ddl.Items.Add( NAMED_COLOR );

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
            configurationValues.Add( SELECTION_TYPE, new ConfigurationValue( "Selection Type", "The type of control to select color.", "" ) );

            if ( controls.Count > 0 && controls[0] is RockDropDownList )
            {
                configurationValues[SELECTION_TYPE].Value = ( ( RockDropDownList ) controls[0] ).SelectedValue;
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
            if ( configurationValues != null )
            {
                if ( controls.Count > 0 && controls[0] is RockDropDownList && configurationValues.ContainsKey( SELECTION_TYPE ) )
                {
                    ( ( RockDropDownList ) controls[0] ).SetValue( configurationValues[SELECTION_TYPE].Value );
                }
            }
        }

        /// <summary>
        /// Renders the controls necessary for prompting user for a new value and adds them to the parentControl
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            if ( configurationValues != null &&
              configurationValues.ContainsKey( SELECTION_TYPE ) &&
              configurationValues[SELECTION_TYPE].Value == NAMED_COLOR )
            {
                var ddl = new RockDropDownList { ID = id };
                ddl.Items.Add( new ListItem() );

                Type colors = typeof( System.Drawing.Color );
                PropertyInfo[] colorInfo = colors.GetProperties( BindingFlags.Public | BindingFlags.Static );
                foreach ( PropertyInfo info in colorInfo )
                {
                    ddl.Items.Add( new ListItem( info.Name, info.Name ) );
                }

                return ddl;
            }
            else
            {
                var colorPicker = new ColorPicker() { ID = id };
                return colorPicker;
            }

        }

        /// <summary>
        /// Determines whether this FieldType supports doing PostBack for the editControl
        /// </summary>
        /// <param name="editControl">The edit control.</param>
        /// <returns>
        ///   <c>true</c> if [has change handler] [the specified control]; otherwise, <c>false</c>.
        /// </returns>
        public override bool HasChangeHandler( Control editControl )
        {
            if ( editControl is ColorPicker )
            {
                // The ColorPicker can get stuck in a postback loop if we enable AutoPostback, so disable ChangeHandler support when using the ColorPicker control.
                return false;
            }
            else
            {
                return base.HasChangeHandler( editControl );
            }
        }

        /// <summary>
        /// Specifies an action to perform when the EditControl's Value is changed. See also <seealso cref="HasChangeHandler(Control)" />
        /// </summary>
        /// <param name="editControl">The edit control.</param>
        /// <param name="action">The action.</param>
        public override void AddChangeHandler( Control editControl, Action action )
        {
            if ( editControl is ColorPicker )
            {
                // The ColorPicker can get stuck in a postback loop if we enable AutoPostback, so disable ChangeHandler support when using the ColorPicker control.
                return;
            }
            else
            {
                base.AddChangeHandler( editControl, action );
            };
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( configurationValues != null &&
              configurationValues.ContainsKey( SELECTION_TYPE ) &&
              configurationValues[SELECTION_TYPE].Value == NAMED_COLOR )
            {
                var editControl = control as ListControl;
                if ( editControl != null )
                {
                    return editControl.SelectedValue;
                }
            }
            else
            {
                var editControl = control as ColorPicker;
                if ( editControl != null )
                {
                    return editControl.Text;
                }

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
            if ( configurationValues != null &&
              configurationValues.ContainsKey( SELECTION_TYPE ) &&
              configurationValues[SELECTION_TYPE].Value == NAMED_COLOR )
            {
                var editControl = control as ListControl;
                if ( editControl != null )
                {
                    editControl.SetValue( value );
                }
            }
            else
            {
                var editControl = control as ColorPicker;
                if ( editControl != null )
                {
                    editControl.Text = value;
                }
            }
        }

#endif
        #endregion

    }
}