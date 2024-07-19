﻿// <copyright>
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
using System.ComponentModel;
using System.Reflection;
#if WEBFORMS
using System.Web.UI;
#endif
using Rock.Attribute;
using Rock.Reporting;
using Rock.ViewModels.Utility;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    ///
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.CODE_EDITOR )]
    public class CodeEditorFieldType : FieldType
    {
        #region Configuration

        private const string EDITOR_MODE = "editorMode";
        private const string EDITOR_MODE_OPTIONS = "editorModeOptions";
        private const string EDITOR_THEME = "editorTheme";
        private const string EDITOR_THEME_OPTIONS = "editorThemeOptions";
        private const string EDITOR_HEIGHT = "editorHeight";

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicEditConfigurationProperties( Dictionary<string, string> privateConfigurationValues )
        {
            var configurationProperties = base.GetPublicEditConfigurationProperties( privateConfigurationValues );

            // Get the Code editor options that are available
            var codeEditorModeOptions = ToListItemBagList<CodeEditorMode>();
            var codeEditorThemeOptions = ToListItemBagList<CodeEditorTheme>();

            configurationProperties[EDITOR_MODE_OPTIONS] = codeEditorModeOptions.ToCamelCaseJson( false, true );
            configurationProperties[EDITOR_THEME_OPTIONS] = codeEditorThemeOptions.ToCamelCaseJson( false, true );

            return configurationProperties;
        }

        /// <summary>
        /// Converts the passed enum into a Listitembaglist.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private List<ListItemBag> ToListItemBagList<T>()
            where T : Enum
        {
            var enumType = typeof( T );
            var listItems = new List<ListItemBag>();
            var names = Enum.GetNames( enumType );
            foreach ( var name in names )
            {
                // ignore Obsolete Enum values
                object value = Enum.Parse( typeof( T ), name );
                var fieldInfo = value.GetType().GetField( name );
                if ( fieldInfo?.GetCustomAttribute<ObsoleteAttribute>() != null )
                {
                    continue;
                }

                // if the Enum has a [Description] attribute, use the description text
                var description = fieldInfo.GetCustomAttribute<DescriptionAttribute>()?.Description ?? name.SplitCase();
                listItems.Add( new ListItemBag() { Text = description, Value = Convert.ToInt32( value ).ToString() } );
            }

            return listItems;
        }

        #endregion

        #region Edit Controls

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            // Encode because if the user typed <span>hello</span> then we want
            // it to display on screen as "<span>hello</span>" rather than "hello".
            return privateValue.EncodeHtml();
        }

        /// <inheritdoc/>
        public override string GetCondensedHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            // Encode because if the user typed <span>hello</span> then we want
            // it to display on screen as "<span>hello</span>" rather than "hello".
            return privateValue.Truncate( CondensedTruncateLength ).EncodeHtml();
        }

        #endregion

        #region FilterControl

        /// <summary>
        /// /*Get*/s the type of the filter comparison.
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

        #endregion

        #region WebForms
#if WEBFORMS

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
            ddlTheme.Help = "The styling theme to use for the code editor.";

            var nbHeight = new NumberBox();
            controls.Add( nbHeight );
            nbHeight.NumberType = System.Web.UI.WebControls.ValidationDataType.Integer;
            nbHeight.AutoPostBack = true;
            nbHeight.TextChanged += OnQualifierUpdated;
            nbHeight.Label = "Editor Height";
            nbHeight.Help = "The height of the control in pixels (minimum of 200)";

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
            configurationValues.Add( EDITOR_THEME, new ConfigurationValue( "Editor Theme", "The styling theme to use for the code editor.", CodeEditorTheme.Rock.ConvertToInt().ToString() ) );
            configurationValues.Add( EDITOR_HEIGHT, new ConfigurationValue( "Editor Height", "The height of the control in pixels.", "200" ) );

            if ( controls != null && controls.Count == 3 )
            {
                if ( controls[0] != null && controls[0] is RockDropDownList )
                {
                    configurationValues[EDITOR_MODE].Value = ( ( RockDropDownList ) controls[0] ).SelectedValue;
                }
                if ( controls[1] != null && controls[1] is RockDropDownList )
                {
                    configurationValues[EDITOR_THEME].Value = ( ( RockDropDownList ) controls[1] ).SelectedValue;
                }
                if ( controls[2] != null && controls[2] is NumberBox )
                {
                    configurationValues[EDITOR_HEIGHT].Value = ( ( NumberBox ) controls[2] ).Text;
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
                    ( ( RockDropDownList ) controls[0] ).SelectedValue = configurationValues[EDITOR_MODE].Value;
                }
                if ( controls[1] != null && controls[1] is RockDropDownList && configurationValues.ContainsKey( EDITOR_THEME ) )
                {
                    ( ( RockDropDownList ) controls[1] ).SelectedValue = configurationValues[EDITOR_THEME].Value;
                }
                if ( controls[2] != null && controls[2] is NumberBox && configurationValues.ContainsKey( EDITOR_HEIGHT ) )
                {
                    ( ( NumberBox ) controls[2] ).Text = configurationValues[EDITOR_HEIGHT].Value;
                }
            }
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
                    editor.EditorHeight = configurationValues[EDITOR_HEIGHT].Value;
                }
            }

            return editor;
        }

        /// <summary>
        /// Formats the value as HTML.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condesed].</param>
        /// <returns></returns>
        public override string FormatValueAsHtml( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false )
        {
            // NOTE: this really should not be encoding the value. FormatValueAsHtml method is really designed to wrap a value with appropriate html (i.e. convert an email into a mailto anchor tag)
            // but keeping it here for backward compatibility.
            return System.Web.HttpUtility.HtmlEncode( FormatValue( parentControl, value, configurationValues, condensed ) );
        }

        /// <summary>
        /// Formats the value as HTML.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        public override string FormatValueAsHtml( Control parentControl, int? entityTypeId, int? entityId, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false )
        {
            // NOTE: this really should not be encoding the value. FormatValueAsHtml method is really designed to wrap a value with appropriate html (i.e. convert an email into a mailto anchor tag)
            // but keeping it here for backward compatibility.
            return System.Web.HttpUtility.HtmlEncode( FormatValue( parentControl, entityTypeId, entityId, value, configurationValues, condensed ) );
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
            var tbValue = new RockTextBox();
            tbValue.ID = string.Format( "{0}_ctlCompareValue", id );
            tbValue.AddCssClass( "js-filter-control" );
            return tbValue;
        }

#endif
        #endregion
    }
}