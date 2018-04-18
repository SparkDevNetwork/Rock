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

using Rock.Data;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to configure and display the social Network accounts
    /// </summary>
    [Serializable]
    public class SocialMediaAccountFieldType : FieldType
    {
        #region Configuration

        private const string NAME_KEY = "name";
        private const string ICONCSSCLASS_KEY = "iconcssclass";
        private const string COLOR_KEY = "color";
        private const string TEXT_TEMPLATE = "texttemplate";
        private const string BASEURL = "baseurl";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( NAME_KEY );
            configKeys.Add( ICONCSSCLASS_KEY );
            configKeys.Add( COLOR_KEY );
            configKeys.Add( TEXT_TEMPLATE );
            configKeys.Add( BASEURL );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            var tbName = new RockTextBox();
            controls.Add( tbName );
            tbName.AutoPostBack = true;
            tbName.TextChanged += OnQualifierUpdated;
            tbName.Label = "Name";
            tbName.Help = "The name of the socal media network.";

            var tbIconCssClass = new RockTextBox();
            controls.Add( tbIconCssClass );
            tbIconCssClass.AutoPostBack = true;
            tbIconCssClass.TextChanged += OnQualifierUpdated;
            tbIconCssClass.Label = "IconCssClass";
            tbIconCssClass.Help = "The icon that represents the social media network.";

            var cpColor = new ColorPicker();
            controls.Add( cpColor );
            cpColor.AutoPostBack = true;
            cpColor.TextChanged += OnQualifierUpdated;
            cpColor.Label = "Color";
            cpColor.Help = "The color to use for making buttons for the social media network.";

            var textTemplate = new CodeEditor();
            controls.Add( textTemplate );
            textTemplate.Label = "Text Template";
            textTemplate.AutoPostBack = true;
            textTemplate.TextChanged += OnQualifierUpdated;
            textTemplate.EditorMode = CodeEditorMode.Lava;
            textTemplate.Help = "Lava template to use to create a formatted version for the link. Primarily used for making the link text.";

            var ulBaseUrl = new UrlLinkBox();
            controls.Add( ulBaseUrl );
            ulBaseUrl.Label = "BaseUrl";
            textTemplate.AutoPostBack = true;
            textTemplate.TextChanged += OnQualifierUpdated;
            textTemplate.Help = "The base URL for the social media network. If the entry does not have a URL in it this base URL will be prepended to the entered string.";

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
            configurationValues.Add( NAME_KEY, new ConfigurationValue( "Name", "The name of the socal media network.", "" ) );
            configurationValues.Add( ICONCSSCLASS_KEY, new ConfigurationValue( "IconCssClass", "The icon that represents the social media network.", "" ) );
            configurationValues.Add( COLOR_KEY, new ConfigurationValue( "Color", "The color to use for making buttons for the social media network.", "" ) );
            configurationValues.Add( TEXT_TEMPLATE, new ConfigurationValue( "Text Template", "Lava template to use to create a formatted version for the link. Primarily used for making the link text.", "" ) );
            configurationValues.Add( BASEURL, new ConfigurationValue( "BaseUrl", "The base URL for the social media network. If the entry does not have a URL in it this base URL will be prepended to the entered string.", "" ) );

            if ( controls != null )
            {
                if ( controls.Count > 0 && controls[0] != null && controls[0] is RockTextBox )
                {
                    configurationValues[NAME_KEY].Value = ( ( RockTextBox ) controls[0] ).Text;
                }
                if ( controls.Count > 1 && controls[1] != null && controls[1] is RockTextBox )
                {
                    configurationValues[ICONCSSCLASS_KEY].Value = ( ( RockTextBox ) controls[1] ).Text;
                }
                if ( controls.Count > 2 && controls[2] != null && controls[2] is ColorPicker )
                {
                    configurationValues[COLOR_KEY].Value = ( ( ColorPicker ) controls[2] ).Text;
                }
                if ( controls.Count > 3 && controls[3] != null && controls[3] is CodeEditor )
                {
                    configurationValues[TEXT_TEMPLATE].Value = ( ( CodeEditor ) controls[3] ).Text;
                }
                if ( controls.Count > 4 && controls[4] != null && controls[4] is UrlLinkBox )
                {
                    if ( string.IsNullOrEmpty( ( ( UrlLinkBox ) controls[4] ).Text ) )
                    {
                        configurationValues[BASEURL].Value = string.Empty;
                    }
                    else
                    {
                        Uri baseUri = new Uri( ( ( UrlLinkBox ) controls[4] ).Text );
                        configurationValues[BASEURL].Value = baseUri.AbsoluteUri;
                    }
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
                if ( controls.Count > 0 && controls[0] != null && controls[0] is RockTextBox && configurationValues.ContainsKey( NAME_KEY ) )
                {
                    ( ( RockTextBox ) controls[0] ).Text = configurationValues[NAME_KEY].Value;
                }
                if ( controls.Count > 1 && controls[1] != null && controls[1] is RockTextBox && configurationValues.ContainsKey( ICONCSSCLASS_KEY ) )
                {
                    ( ( RockTextBox ) controls[1] ).Text = configurationValues[ICONCSSCLASS_KEY].Value;
                }
                if ( controls.Count > 2 && controls[2] != null && controls[2] is ColorPicker && configurationValues.ContainsKey( COLOR_KEY ) )
                {
                    ( ( ColorPicker ) controls[2] ).Text = configurationValues[COLOR_KEY].Value;
                }
                if ( controls.Count > 3 && controls[3] != null && controls[3] is CodeEditor && configurationValues.ContainsKey( TEXT_TEMPLATE ) )
                {
                    ( ( CodeEditor ) controls[3] ).Text = configurationValues[TEXT_TEMPLATE].Value;
                }
                if ( controls.Count > 4 && controls[4] != null && controls[4] is UrlLinkBox && configurationValues.ContainsKey( BASEURL ) )
                {
                    ( ( UrlLinkBox ) controls[4] ).Text = configurationValues[BASEURL].Value;
                }
            }
        }

        #endregion

        #region Formatting

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

            if ( string.IsNullOrWhiteSpace( value ) )
            {
                return string.Empty;
            }
            else
            {
                if ( configurationValues != null )
                {
                    Dictionary<string, object> mergeFields = Lava.LavaHelper.GetCommonMergeFields( parentControl?.RockBlock()?.RockPage, null, new Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
                    string template = string.Empty;

                    if ( configurationValues.ContainsKey( TEXT_TEMPLATE ) )
                    {
                        template = configurationValues[TEXT_TEMPLATE].Value;
                    }
                    if ( configurationValues.ContainsKey( ICONCSSCLASS_KEY ) )
                    {

                        string iconCssClass = configurationValues[ICONCSSCLASS_KEY].Value;
                        if ( !iconCssClass.Contains( "fa-fw" ) )
                        {
                            iconCssClass = iconCssClass + " fa-fw";
                        }
                        mergeFields.Add( ICONCSSCLASS_KEY, iconCssClass );
                    }

                    if ( configurationValues.ContainsKey( COLOR_KEY ) && !string.IsNullOrEmpty( configurationValues[COLOR_KEY].Value ) )
                    {
                        mergeFields.Add( COLOR_KEY, configurationValues[COLOR_KEY].Value );
                    }

                    if ( configurationValues.ContainsKey( BASEURL ) && !string.IsNullOrEmpty( configurationValues[BASEURL].Value ) )
                    {
                        mergeFields.Add( BASEURL, configurationValues[BASEURL].Value );
                    }

                    if ( configurationValues.ContainsKey( NAME_KEY ) && !string.IsNullOrEmpty( configurationValues[NAME_KEY].Value ) )
                    {
                        mergeFields.Add( NAME_KEY, configurationValues[NAME_KEY].Value );
                    }

                    mergeFields.Add( "value", value );

                    return template.ResolveMergeFields( mergeFields );
                }
                return value;
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
        public override System.Web.UI.Control EditControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            return new RockTextBox { ID = id };
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control != null && control is TextBox )
            {
                if ( configurationValues != null && configurationValues.ContainsKey( "baseurl" ) )
                {
                    string value = ( ( TextBox ) control ).Text;
                    if ( !value.StartsWith( configurationValues[BASEURL].Value ) && !string.IsNullOrEmpty( value ) )
                    {
                        return string.Format( "{0}{1}", configurationValues[BASEURL].Value, value );
                    }
                    else
                    {
                        return value;
                    }
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
            if ( value != null && control != null && control is TextBox && !string.IsNullOrEmpty( value ) )
            {
                try
                {
                    ( (TextBox)control ).Text = new Uri( value ).Segments.Last();
                }
                catch
                {
                    ( (TextBox)control ).Text = value;
                }
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

    }
}
