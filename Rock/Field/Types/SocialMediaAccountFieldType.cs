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
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to configure and display the social Network accounts
    /// </summary>
    [Serializable]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.SOCIAL_MEDIA_ACCOUNT )]
    public class SocialMediaAccountFieldType : FieldType
    {
        #region Configuration

        private const string NAME_KEY = "name";
        private const string ICONCSSCLASS_KEY = "iconcssclass";
        private const string COLOR_KEY = "color";
        private const string TEXT_TEMPLATE = "texttemplate";
        private const string BASEURL = "baseurl";
        private const string BASEURL_ALIASES = "baseurlaliases";

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( string.IsNullOrWhiteSpace( privateValue ) )
            {
                return string.Empty;
            }
            else
            {
                if ( privateConfigurationValues != null )
                {
                    Dictionary<string, object> mergeFields = Lava.LavaHelper.GetCommonMergeFields( null, null, new Lava.CommonMergeFieldsOptions() );
                    string template = string.Empty;

                    if ( privateConfigurationValues.ContainsKey( TEXT_TEMPLATE ) )
                    {
                        template = privateConfigurationValues[TEXT_TEMPLATE];
                    }
                    if ( string.IsNullOrWhiteSpace( template ) )
                    {
                        // If an output template is not specified, use a default.
                        template = "<a href='{{value}}' target='_blank' rel='noopener noreferrer'>{{ value | Url:'segments' | Last }}</a>";
                    }
                    if ( privateConfigurationValues.ContainsKey( ICONCSSCLASS_KEY ) )
                    {

                        string iconCssClass = privateConfigurationValues[ICONCSSCLASS_KEY];
                        if ( !iconCssClass.Contains( "fa-fw" ) )
                        {
                            iconCssClass = iconCssClass + " fa-fw";
                        }
                        mergeFields.Add( ICONCSSCLASS_KEY, iconCssClass );
                    }

                    if ( privateConfigurationValues.ContainsKey( COLOR_KEY ) && !string.IsNullOrEmpty( privateConfigurationValues[COLOR_KEY] ) )
                    {
                        mergeFields.Add( COLOR_KEY, privateConfigurationValues[COLOR_KEY] );
                    }

                    if ( privateConfigurationValues.ContainsKey( BASEURL ) && !string.IsNullOrEmpty( privateConfigurationValues[BASEURL] ) )
                    {
                        mergeFields.Add( BASEURL, privateConfigurationValues[BASEURL] );
                    }

                    if ( privateConfigurationValues.ContainsKey( NAME_KEY ) && !string.IsNullOrEmpty( privateConfigurationValues[NAME_KEY] ) )
                    {
                        mergeFields.Add( NAME_KEY, privateConfigurationValues[NAME_KEY] );
                    }

                    mergeFields.Add( "value", privateValue );

                    return template.ResolveMergeFields( mergeFields );
                }

                return privateValue;
            }
        }

        #endregion

        #region Persistence

        /// <inheritdoc/>
        public override bool IsPersistedValueSupported( Dictionary<string, string> privateConfigurationValues )
        {
            // Lava could cause a different result with each render
            return false;
        }

        #endregion

        #region Edit Control

        /// <inheritdoc />
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetHtmlValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc />
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            var baseUrl = string.Empty;
            var baseUrlAliases = new List<string>();

            if ( privateConfigurationValues != null )
            {
                if ( privateConfigurationValues.ContainsKey( BASEURL ) )
                {
                    baseUrl = privateConfigurationValues[BASEURL];
                    if ( !string.IsNullOrWhiteSpace( baseUrl ) )
                    {
                        baseUrlAliases.Add( baseUrl );
                    }
                }

                if ( privateConfigurationValues.ContainsKey( BASEURL_ALIASES ) )
                {
                    baseUrlAliases.AddRange( privateConfigurationValues[BASEURL_ALIASES].SplitDelimitedValues( "," ).ToList() );
                }
            }

            return ReplaceBaseUrlAliases( publicValue, baseUrlAliases, baseUrl );
        }

        /// <inheritdoc />
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var baseUrlAliases = new List<string>();

            if ( privateConfigurationValues != null )
            {
                if ( privateConfigurationValues.ContainsKey( BASEURL ) )
                {
                    var baseUrl = privateConfigurationValues[BASEURL];
                    if ( !string.IsNullOrWhiteSpace( baseUrl ) )
                    {
                        baseUrlAliases.Add( baseUrl );
                    }
                }

                if ( privateConfigurationValues.ContainsKey( BASEURL_ALIASES ) )
                {
                    baseUrlAliases.AddRange( privateConfigurationValues[BASEURL_ALIASES].SplitDelimitedValues( "," ).ToList() );
                }
            }

            return ReplaceBaseUrlAliases( privateValue, baseUrlAliases, string.Empty );
        }

        private string ReplaceBaseUrlAliases( string input, List<string> baseUrls, string newBaseUrl )
        {
                        var output = input;
            if ( string.IsNullOrWhiteSpace( output ) )
            {
                return output;
            }

            // Recursively remove the base URL and any aliases from the input.
            bool doReplace;
            do
            {
                output = output.Trim();

                // Strip recognized Base Urls from the input text.
                doReplace = false;
                foreach ( var baseUrl in baseUrls )
                {
                    if ( !string.IsNullOrWhiteSpace( baseUrl ) && output.StartsWith( baseUrl, StringComparison.OrdinalIgnoreCase ) )
                    {
                        output = output.Substring( baseUrl.Length );
                        doReplace = true;
                    }
                }
            } while ( doReplace );

            // If the remaining text represents a relative URL, add the new base URL.
            if ( !string.IsNullOrWhiteSpace( newBaseUrl )
                 && Uri.IsWellFormedUriString( output, UriKind.Relative ) )
            {
                output = newBaseUrl.TrimEnd( '/' ) + "/" + output.TrimStart( '/' );
            }

            return output;
        }

        #endregion

        #region FilterControl

        /// <summary>
        /// Determines whether [has filter control].
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return true;
        }

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
                return ComparisonHelper.EqualOrBlankFilterComparisonTypes;
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
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( NAME_KEY );
            configKeys.Add( ICONCSSCLASS_KEY );
            configKeys.Add( COLOR_KEY );
            configKeys.Add( TEXT_TEMPLATE );
            configKeys.Add( BASEURL );
            configKeys.Add( BASEURL_ALIASES );
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
            tbName.Label = "Name";
            tbName.Help = "The name of the social media network.";

            var tbIconCssClass = new RockTextBox();
            controls.Add( tbIconCssClass );
            tbIconCssClass.Label = "Icon CSS Class";
            tbIconCssClass.Help = "The icon that represents the social media network.";

            var cpColor = new ColorPicker();
            controls.Add( cpColor );
            cpColor.Label = "Color";
            cpColor.Help = "The color to use for making buttons for the social media network.";

            var textTemplate = new CodeEditor();
            controls.Add( textTemplate );
            textTemplate.Label = "Text Template";
            textTemplate.EditorMode = CodeEditorMode.Lava;
            textTemplate.Help = "Lava template to use to create a formatted version for the link. Primarily used for making the link text.";

            var ulBaseUrl = new UrlLinkBox();
            controls.Add( ulBaseUrl );
            ulBaseUrl.Label = "Base URL";
            ulBaseUrl.Help = "The base URL for the social media network. If the entry does not have a URL in it this base URL will be prepended to the entered string.";

            var tbBaseUrlAliases = new RockTextBox();
            controls.Add( tbBaseUrlAliases );
            tbBaseUrlAliases.Label = "Base URL Aliases";
            tbBaseUrlAliases.Help = "A comma-delimited list of URL prefixes that are considered valid aliases for the Base URL. If any of these values are detected in the input, they will be replaced by the Base URL.";

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
            configurationValues.Add( NAME_KEY, new ConfigurationValue( "Name", "The name of the social media network.", "" ) );
            configurationValues.Add( ICONCSSCLASS_KEY, new ConfigurationValue( "Icon CSS Class", "The icon that represents the social media network.", "" ) );
            configurationValues.Add( COLOR_KEY, new ConfigurationValue( "Color", "The color to use for making buttons for the social media network.", "" ) );
            configurationValues.Add( TEXT_TEMPLATE, new ConfigurationValue( "Text Template", "Lava template to use to create a formatted version for the link. Primarily used for making the link text.", "" ) );
            configurationValues.Add( BASEURL, new ConfigurationValue( "Base URL", "The base URL for the social media network. If the entry does not have a URL in it this base URL will be prepended to the entered string.", "" ) );
            configurationValues.Add( BASEURL_ALIASES, new ConfigurationValue( "Base URL Aliases", "A comma-delimited list of URL prefixes that are considered valid aliases for the Base URL. If any of these values are detected in the input, they will be replaced by the Base URL.", "" ) );

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
                if ( controls.Count > 5 && controls[5] != null && controls[5] is RockTextBox )
                {
                    configurationValues[BASEURL_ALIASES].Value = ( ( RockTextBox ) controls[5] ).Text;
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
                if ( controls.Count > 5 && controls[5] != null && controls[5] is RockTextBox && configurationValues.ContainsKey( BASEURL_ALIASES ) )
                {
                    ( ( RockTextBox ) controls[5] ).Text = configurationValues[BASEURL_ALIASES].Value;
                }
            }
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
            return GetHtmlValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

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
            var linkBox = new UrlLinkBox
            {
                ID = id,
                ValidationDisplay = ValidatorDisplay.None
            };

            if ( configurationValues != null )
            {
                if ( configurationValues.ContainsKey( NAME_KEY ) )
                {
                    linkBox.Label = configurationValues[NAME_KEY].Value;
                }
                if ( configurationValues.ContainsKey( BASEURL ) )
                {
                    linkBox.BaseUrl = configurationValues[BASEURL].Value;
                }
                if ( configurationValues.ContainsKey( BASEURL_ALIASES ) )
                {
                    linkBox.BaseUrlAliases = configurationValues[BASEURL_ALIASES].Value;
                }
            }
            return linkBox;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var editControl = control as UrlLinkBox;
            return editControl?.Url;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var editControl = control as UrlLinkBox;
            if ( editControl != null )
            {
                editControl.Url = value;
            }
        }

        /// <summary>
        /// Gets the filter compare control.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override Control FilterCompareControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            if ( filterMode == FilterMode.SimpleFilter )
            {
                // hide the compare control for SimpleFilter mode
                RockDropDownList ddlCompare = ComparisonHelper.ComparisonControl( FilterComparisonType, required );
                ddlCompare.ID = string.Format( "{0}_ddlCompare", id );
                ddlCompare.AddCssClass( "js-filter-compare" );
                ddlCompare.Visible = false;
                return ddlCompare;
            }
            else
            {
                return base.FilterCompareControl( configurationValues, id, required, filterMode );
            }
        }

        /// <summary>
        /// Gets the filter values.
        /// </summary>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override List<string> GetFilterValues( Control filterControl, Dictionary<string, ConfigurationValue> configurationValues, FilterMode filterMode )
        {
            // If this is a simple filter, only return values if something was actually entered into the filter's text field
            var values = base.GetFilterValues( filterControl, configurationValues, filterMode );
            if ( filterMode == FilterMode.SimpleFilter &&
                values.Count == 2 &&
                values[0].ConvertToEnum<ComparisonType>() == ComparisonType.Contains &&
                values[1] == "" )
            {
                return new List<string>();
            }

            return values;
        }

        /// <summary>
        /// Gets the filter compare value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override string GetFilterCompareValue( Control control, FilterMode filterMode )
        {
            bool filterValueControlVisible = true;
            var filterField = control.FirstParentControlOfType<FilterField>();
            if ( filterField != null && filterField.HideFilterCriteria )
            {
                filterValueControlVisible = false;
            }

            if ( filterMode == FilterMode.SimpleFilter && filterValueControlVisible )
            {
                // hard code to Contains when in SimpleFilter mode and the FilterValue control is visible
                return ComparisonType.Contains.ConvertToInt().ToString();
            }
            else
            {
                return base.GetFilterCompareValue( control, filterMode );
            }
        }

#endif
        #endregion
    }
}
