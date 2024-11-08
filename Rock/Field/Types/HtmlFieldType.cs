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
#endif
using Rock.Attribute;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// 
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.HTML )]
    public class HtmlFieldType : FieldType
    {
        #region Configuration

        private const string TOOLBAR = "toolbar";
        private const string DOCUMENT_FOLDER_ROOT = "documentfolderroot";
        private const string IMAGE_FOLDER_ROOT = "imagefolderroot";
        private const string USER_SPECIFIC_ROOT = "userspecificroot";

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
        public override Model.ComparisonType FilterComparisonType
        {
            get
            {
                return ComparisonHelper.StringFilterComparisonTypes;
            }
        }

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return privateValue.StripHtml();
        }

        /// <inheritdoc/>
        public override string GetHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            // The base method will encode for HTML which we don't want. The
            // privateValue is the raw HTML we want to return.
            return privateValue;
        }

        /// <inheritdoc/>
        public override string GetCondensedHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            // We need to clean up the HTML right around the truncate length.
            return privateValue.TruncateHtml( CondensedTruncateLength );
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
            configKeys.Add( TOOLBAR );
            configKeys.Add( DOCUMENT_FOLDER_ROOT );
            configKeys.Add( IMAGE_FOLDER_ROOT );
            configKeys.Add( USER_SPECIFIC_ROOT );
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
            ddl.Label = "Toolbar Type";
            ddl.Help = "The type of toolbar to display on editor.";
            ddl.Items.Add( "Light" );
            ddl.Items.Add( "Full" );

            var tbDocumentRoot = new RockTextBox();
            controls.Add( tbDocumentRoot );
            tbDocumentRoot.AutoPostBack = true;
            tbDocumentRoot.TextChanged += OnQualifierUpdated;
            tbDocumentRoot.Label = "Document Root Folder";
            tbDocumentRoot.Help = "The folder to use as the root when browsing or uploading documents ( e.g. ~/Content ).";

            var tbImageRoot = new RockTextBox();
            controls.Add( tbImageRoot );
            tbImageRoot.AutoPostBack = true;
            tbImageRoot.TextChanged += OnQualifierUpdated;
            tbImageRoot.Label = "Image Root Folder";
            tbImageRoot.Help = "The folder to use as the root when browsing or uploading images ( e.g. ~/Content ).";

            var cbUserSpecificFolder = new RockCheckBox();
            controls.Add( cbUserSpecificFolder );
            cbUserSpecificFolder.AutoPostBack = true;
            cbUserSpecificFolder.CheckedChanged += OnQualifierUpdated;
            cbUserSpecificFolder.Label = "User Specific Folders";
            cbUserSpecificFolder.Help = "Should the root folders be specific to current user?";
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
            configurationValues.Add( TOOLBAR, new ConfigurationValue( "Toolbar Type", "The type of toolbar to display on editor.", "" ) );
            configurationValues.Add( DOCUMENT_FOLDER_ROOT, new ConfigurationValue( "Document Root Folder", "The folder to use as the root when browsing or uploading documents ( e.g. ~/Content ).", "" ) );
            configurationValues.Add( IMAGE_FOLDER_ROOT, new ConfigurationValue( "Image Root Folder", "The folder to use as the root when browsing or uploading images ( e.g. ~/Content ).", "" ) );
            configurationValues.Add( USER_SPECIFIC_ROOT, new ConfigurationValue( "User Specific Folders", "Should the root folders be specific to current user?", "" ) );

            if ( controls.Count > 0 && controls[0] is RockDropDownList )
            {
                configurationValues[TOOLBAR].Value = ( ( RockDropDownList ) controls[0] ).SelectedValue;
            }

            if ( controls.Count > 1 && controls[1] is RockTextBox )
            {
                configurationValues[DOCUMENT_FOLDER_ROOT].Value = ( ( RockTextBox ) controls[1] ).Text;
            }

            if ( controls.Count > 2 && controls[2] is RockTextBox )
            {
                configurationValues[IMAGE_FOLDER_ROOT].Value = ( ( RockTextBox ) controls[2] ).Text;
            }

            if ( controls.Count > 3 && controls[3] is RockCheckBox )
            {
                configurationValues[USER_SPECIFIC_ROOT].Value = ( ( RockCheckBox ) controls[3] ).Checked.ToString();
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
                if ( controls.Count > 0 && controls[0] is RockDropDownList && configurationValues.ContainsKey( TOOLBAR ) )
                {
                    ( ( RockDropDownList ) controls[0] ).SetValue( configurationValues[TOOLBAR].Value );
                }

                if ( controls.Count > 1 && controls[1] is RockTextBox && configurationValues.ContainsKey( DOCUMENT_FOLDER_ROOT ) )
                {
                    ( ( RockTextBox ) controls[1] ).Text = configurationValues[DOCUMENT_FOLDER_ROOT].Value;
                }

                if ( controls.Count > 2 && controls[2] is RockTextBox && configurationValues.ContainsKey( IMAGE_FOLDER_ROOT ) )
                {
                    ( ( RockTextBox ) controls[2] ).Text = configurationValues[IMAGE_FOLDER_ROOT].Value;
                }

                if ( controls.Count > 3 && controls[3] is RockCheckBox && configurationValues.ContainsKey( USER_SPECIFIC_ROOT ) )
                {
                    ( ( RockCheckBox ) controls[3] ).Checked = configurationValues[USER_SPECIFIC_ROOT].Value.AsBoolean();
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
            var editor = new HtmlEditor { ID = id };

            if ( configurationValues != null &&
                configurationValues.ContainsKey( TOOLBAR ) &&
                configurationValues[TOOLBAR].Value == "Full" )
            {
                editor.Toolbar = HtmlEditor.ToolbarConfig.Full;
            }
            else
            {
                editor.Toolbar = HtmlEditor.ToolbarConfig.Light;
            }

            if ( configurationValues != null && configurationValues.ContainsKey( DOCUMENT_FOLDER_ROOT ) )
            {
                editor.DocumentFolderRoot = configurationValues[DOCUMENT_FOLDER_ROOT].Value;
            }

            if ( configurationValues != null && configurationValues.ContainsKey( IMAGE_FOLDER_ROOT ) )
            {
                editor.ImageFolderRoot = configurationValues[IMAGE_FOLDER_ROOT].Value;
            }

            if ( configurationValues != null && configurationValues.ContainsKey( USER_SPECIFIC_ROOT ) )
            {
                editor.UserSpecificRoot = configurationValues[USER_SPECIFIC_ROOT].Value.AsBoolean( false );
            }

            return editor;
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

        /// <summary>
        /// Determines whether this FieldType supports doing PostBack for the editControl
        /// </summary>
        /// <param name="editControl">The edit control.</param>
        /// <returns>
        ///   <c>true</c> if [has change handler] [the specified control]; otherwise, <c>false</c>.
        /// </returns>
        public override bool HasChangeHandler( Control editControl )
        {
            // the HTML Editor can cause a postback loop if OnChange and AutoPostback is enabled, so disable the HasChangeHandler
            return false;
        }

        /// <summary>
        /// Specifies an action to perform when the EditControl's Value is changed. See also <seealso cref="HasChangeHandler(Control)" />
        /// </summary>
        /// <param name="editControl">The edit control.</param>
        /// <param name="action">The action.</param>
        public override void AddChangeHandler( Control editControl, Action action )
        {
            // the HTML Editor can cause a postback loop if OnChange and AutoPostback is enabled, so disable the HasChangeHandler
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
                ? GetHtmlValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetCondensedHtmlValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

#endif
        #endregion
    }
}