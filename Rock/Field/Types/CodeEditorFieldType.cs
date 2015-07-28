// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Collections.Generic;
using System.Web.UI;

using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// 
    /// </summary>
    public class CodeEditorFieldType : FieldType
    {

        #region Configuration

        private const string EDITOR_MODE = "editorMode";
        private const string EDITOR_THEME = "editorTheme";
        private const string EDITOR_HEIGHT = "editorHeight";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            List<string> configKeys = new List<string>();
            configKeys.Add( EDITOR_MODE );
            configKeys.Add( EDITOR_THEME );
            configKeys.Add( EDITOR_HEIGHT );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = new List<Control>();

            var ddlMode = new RockDropDownList();
            controls.Add( ddlMode );
            ddlMode.BindToEnum<CodeEditorMode>();
            ddlMode.AutoPostBack = true;
            ddlMode.SelectedIndexChanged += OnQualifierUpdated;
            ddlMode.Label = "Editor Mode";
            ddlMode.Help = "The type of code that will be entered.";

            var ddlTheme = new RockDropDownList();
            controls.Add( ddlTheme );
            ddlTheme.BindToEnum<CodeEditorTheme>();
            ddlTheme.AutoPostBack = true;
            ddlTheme.SelectedIndexChanged += OnQualifierUpdated;
            ddlTheme.Label = "Editor Theme";
            ddlTheme.Help = "The styling them to use for the code editor.";

            var nbHeight = new NumberBox();
            controls.Add( nbHeight );
            nbHeight.NumberType = System.Web.UI.WebControls.ValidationDataType.Integer;
            nbHeight.AutoPostBack = true;
            nbHeight.TextChanged += OnQualifierUpdated;
            nbHeight.Label = "Editor Height";
            nbHeight.Help = "The height of the control in pixels.";

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
            configurationValues.Add( EDITOR_MODE, new ConfigurationValue( "Editor Mode", "The type of code that will be entered.", "" ) );
            configurationValues.Add( EDITOR_THEME, new ConfigurationValue( "Editor Theme", "The styling them to use for the code editor.", "0" ) );
            configurationValues.Add( EDITOR_HEIGHT, new ConfigurationValue( "Editor Height", "The height of the control in pixels.", "0" ) );

            if ( controls != null && controls.Count == 3 )
            {
                if ( controls[0] != null && controls[0] is RockDropDownList )
                {
                    configurationValues[EDITOR_MODE].Value = ( (RockDropDownList)controls[0] ).SelectedValue;
                }
                if ( controls[1] != null && controls[1] is RockDropDownList )
                {
                    configurationValues[EDITOR_THEME].Value = ( (RockDropDownList)controls[1] ).SelectedValue;
                }
                if ( controls[2] != null && controls[2] is NumberBox )
                {
                    configurationValues[EDITOR_HEIGHT].Value = ( (NumberBox)controls[2] ).Text;
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
            if ( controls != null && controls.Count == 3 && configurationValues != null )
            {
                if ( controls[0] != null && controls[0] is RockDropDownList && configurationValues.ContainsKey( EDITOR_MODE ) )
                {
                    ( (RockDropDownList)controls[0] ).SelectedValue = configurationValues[EDITOR_MODE].Value;
                }
                if ( controls[1] != null && controls[1] is RockDropDownList && configurationValues.ContainsKey( EDITOR_THEME ) )
                {
                    ( (RockDropDownList)controls[1] ).SelectedValue = configurationValues[EDITOR_THEME].Value;
                }
                if ( controls[2] != null && controls[2] is NumberBox && configurationValues.ContainsKey( EDITOR_HEIGHT ) )
                {
                    ( (NumberBox)controls[2] ).Text = configurationValues[EDITOR_HEIGHT].Value;
                }
            }
        }

        #endregion

        #region Edit Control

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
            var editor = new CodeEditor { ID = id };

            if ( configurationValues != null )
            {
                if ( configurationValues.ContainsKey( EDITOR_MODE ) )
                {
                    editor.EditorMode = configurationValues[EDITOR_MODE].Value.ConvertToEnum<CodeEditorMode>( CodeEditorMode.Text );
                }

                if ( configurationValues.ContainsKey( EDITOR_THEME ) )
                {
                    editor.EditorTheme = configurationValues[EDITOR_THEME].Value.ConvertToEnum<CodeEditorTheme>( CodeEditorTheme.Rock );
                }

                if ( configurationValues.ContainsKey( EDITOR_HEIGHT ) )
                {
                    editor.EditorHeight = configurationValues[EDITOR_HEIGHT].Value.ToString();
                }
            }

            return editor;
        }

        #endregion

        #region FilterControl

        /// <summary>
        /// Gets the type of the filter comparison.
        /// </summary>
        /// <value>
        /// The type of the filter comparison.
        /// </value>
        public override Model.ComparisonType FilterComparisonType
        {
            get
            {
                return ComparisonHelper.StringFilterComparisonTypes;
            }
        }

        /// <summary>
        /// Gets the filter value control.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <returns></returns>
        public override Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            var tbValue = new RockTextBox();
            tbValue.ID = string.Format( "{0}_ctlCompareValue", id );
            tbValue.AddCssClass( "js-filter-control" );
            return tbValue;
        }

        #endregion

    }
}