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

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( "name" );
            configKeys.Add( "iconcssclass" );
            configKeys.Add( "color" );
            configKeys.Add( "texttemplate" );
            configKeys.Add( "baseurl" );
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
            configurationValues.Add( "name", new ConfigurationValue( "Name", "The name of the socal media network.", "" ) );
            configurationValues.Add( "iconcssclass", new ConfigurationValue( "IconCssClass", "The icon that represents the social media network.", "" ) );
            configurationValues.Add( "color", new ConfigurationValue( "Color", "The color to use for making buttons for the social media network.", "" ) );
            configurationValues.Add( "texttemplate", new ConfigurationValue( "Text Template", "Lava template to use to create a formatted version for the link. Primarily used for making the link text.", "" ) );
            configurationValues.Add( "baseurl", new ConfigurationValue( "BaseUrl", "The base URL for the social media network. If the entry does not have a URL in it this base URL will be prepended to the entered string.", "" ) );

            if ( controls != null )
            {
                if ( controls.Count > 0 && controls[0] != null && controls[0] is RockTextBox )
                {
                    configurationValues["name"].Value = ( ( RockTextBox ) controls[0] ).Text;
                }
                if ( controls.Count > 1 && controls[1] != null && controls[1] is RockTextBox )
                {
                    configurationValues["iconcssclass"].Value = ( ( RockTextBox ) controls[1] ).Text;
                }
                if ( controls.Count > 2 && controls[2] != null && controls[2] is ColorPicker )
                {
                    configurationValues["color"].Value = ( ( ColorPicker ) controls[2] ).Text;
                }
                if ( controls.Count > 3 && controls[3] != null && controls[3] is CodeEditor )
                {
                    configurationValues["texttemplate"].Value = ( ( CodeEditor ) controls[3] ).Text;
                }
                if ( controls.Count > 4 && controls[4] != null && controls[4] is UrlLinkBox )
                {
                    Uri baseUri = new Uri( ( ( UrlLinkBox ) controls[4] ).Text );
                    configurationValues["baseurl"].Value = baseUri.AbsoluteUri;
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
                if ( controls.Count > 0 && controls[0] != null && controls[0] is RockTextBox && configurationValues.ContainsKey( "name" ) )
                {
                    ( ( RockTextBox ) controls[0] ).Text = configurationValues["name"].Value;
                }
                if ( controls.Count > 1 && controls[1] != null && controls[1] is RockTextBox && configurationValues.ContainsKey( "iconcssclass" ) )
                {
                    ( ( RockTextBox ) controls[1] ).Text = configurationValues["iconcssclass"].Value;
                }
                if ( controls.Count > 2 && controls[2] != null && controls[2] is ColorPicker && configurationValues.ContainsKey( "color" ) )
                {
                    ( ( ColorPicker ) controls[2] ).Text = configurationValues["color"].Value;
                }
                if ( controls.Count > 3 && controls[3] != null && controls[3] is CodeEditor && configurationValues.ContainsKey( "texttemplate" ) )
                {
                    ( ( CodeEditor ) controls[3] ).Text = configurationValues["texttemplate"].Value;
                }
                if ( controls.Count > 4 && controls[4] != null && controls[4] is UrlLinkBox && configurationValues.ContainsKey( "baseurl" ) )
                {
                    ( ( UrlLinkBox ) controls[4] ).Text = configurationValues["baseurl"].Value;
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
                    
                    if ( configurationValues.ContainsKey( "texttemplate" ) )
                    {
                        template = configurationValues["texttemplate"].Value;
                    }
                    if ( configurationValues.ContainsKey( "iconcssclass" ) )
                    {
                        
                        string iconCssClass = configurationValues["iconcssclass"].Value;
                        if ( !iconCssClass.Contains( "fa-fw" ) )
                        {
                            iconCssClass = iconCssClass + " fa-fw";
                        }
                        mergeFields.Add( "iconcssclass", iconCssClass );
                    }

                    if ( configurationValues.ContainsKey( "color" ) && !string.IsNullOrEmpty( configurationValues["color"].Value ) )
                    {
                        mergeFields.Add( "color", configurationValues["color"].Value );
                    }

                    if ( configurationValues.ContainsKey( "baseurl" ) && !string.IsNullOrEmpty( configurationValues["baseurl"].Value ) )
                    {
                        mergeFields.Add( "baseurl", configurationValues["baseurl"].Value );
                    }

                    if ( configurationValues.ContainsKey( "name" ) && !string.IsNullOrEmpty( configurationValues["name"].Value ) )
                    {
                        mergeFields.Add( "name", configurationValues["name"].Value );
                    }

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
        /// Tests the value to ensure that it is a valid value.  If not, message will indicate why
        /// </summary>
        /// <param name="value"></param>
        /// <param name="required"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool IsValid( string value, bool required, out string message )
        {
            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                Uri validatedUri;
                if ( !Uri.TryCreate( value, UriKind.Absolute, out validatedUri ) )
                {
                    message = "Invalid Social Handler name provided";
                    return true;
                }
            }
            return base.IsValid( value, required, out message );
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
                    Uri validatedUri;
                    if ( Uri.TryCreate( ( ( TextBox ) control ).Text, UriKind.Absolute, out validatedUri ) )
                    {
                        return string.Format( "{0}{1}", configurationValues["baseurl"].Value, validatedUri.Segments.Last() );
                    }
                    else
                    {
                        return string.Format( "{0}{1}", configurationValues["baseurl"].Value, ( ( TextBox ) control ).Text );
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
                ( ( TextBox ) control ).Text = new Uri( value ).Segments.Last();
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
