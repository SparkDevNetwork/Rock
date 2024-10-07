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
using System.Collections.Generic;
#if WEBFORMS
using System.Web.UI;
using System.Web.UI.WebControls;
#endif
using Rock.Attribute;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to edit text in a multi-line text box
    /// </summary>
    [FieldTypeUsage( FieldTypeUsage.Common )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms | Utility.RockPlatform.Obsidian )]
    [IconSvg( @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 16 16""><g><path d=""M1.65,2.46h7.7A.65.65,0,0,0,10,1.81V1.62A.66.66,0,0,0,9.35,1H1.65A.66.66,0,0,0,1,1.62v.19A.65.65,0,0,0,1.65,2.46Zm0,8h7.7A.65.65,0,0,0,10,9.81V9.62A.65.65,0,0,0,9.35,9H1.65A.65.65,0,0,0,1,9.62v.19A.65.65,0,0,0,1.65,10.46ZM14.25,5H1.72a.75.75,0,0,0,0,1.5H14.25A.74.74,0,0,0,15,5.74.8.8,0,0,0,14.25,5Zm0,8H1.72a.75.75,0,0,0,0,1.5H14.25a.74.74,0,0,0,.75-.72A.8.8,0,0,0,14.25,13Z""/></g></svg>" )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.MEMO )]
    public class MemoFieldType : FieldType
    {
        #region Configuration

        private const string NUMBER_OF_ROWS = "numberofrows";
        private const string ALLOW_HTML = "allowhtml";
        private const string MAX_CHARACTERS = "maxcharacters";
        private const string SHOW_COUNT_DOWN = "showcountdown";

        #endregion

        #region Edit Control

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
            configKeys.Add( NUMBER_OF_ROWS );
            configKeys.Add( ALLOW_HTML );
            configKeys.Add( MAX_CHARACTERS );
            configKeys.Add( SHOW_COUNT_DOWN );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // Add number box for selecting the number of rows
            var nb = new NumberBox();
            controls.Add( nb );
            nb.AutoPostBack = true;
            nb.TextChanged += OnQualifierUpdated;
            nb.NumberType = ValidationDataType.Integer;
            nb.Label = "Rows";
            nb.Help = "The number of rows to display (default is 3).";

            // Allow HTML
            var cb = new RockCheckBox();
            controls.Add( cb );
            cb.AutoPostBack = true;
            cb.CheckedChanged += OnQualifierUpdated;
            cb.Label = "Allow HTML";
            cb.Help = "Controls whether server should prevent HTML from being entered in this field or not.";

            // Add number box for selecting the maximum number of characters
            var nbCharacter = new NumberBox();
            controls.Add( nbCharacter );
            nbCharacter.AutoPostBack = true;
            nbCharacter.TextChanged += OnQualifierUpdated;
            nbCharacter.NumberType = ValidationDataType.Integer;
            nbCharacter.Label = "Max Characters";
            nbCharacter.Help = "The maximum number of characters to allow. Leave this field empty to allow for an unlimited amount of text.";

            // Add checkbox indicating whether to show the count down.
            var cbCountDown = new RockCheckBox();
            controls.Add( cbCountDown );
            cbCountDown.AutoPostBack = true;
            cbCountDown.CheckedChanged += OnQualifierUpdated;
            cbCountDown.Label = "Show Character Limit Countdown";
            cbCountDown.Help = "When set, displays a countdown showing how many characters remain (for the Max Characters setting).";

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
            configurationValues.Add( NUMBER_OF_ROWS, new ConfigurationValue( "Rows", "The number of rows to display (default is 3).", "" ) );
            configurationValues.Add( ALLOW_HTML, new ConfigurationValue( "Allow HTML", "Controls whether server should prevent HTML from being entered in this field or not.", "" ) );
            configurationValues.Add( MAX_CHARACTERS, new ConfigurationValue( "Max Characters", "The maximum number of characters to allow. Leave this field empty to allow for an unlimited amount of text.", "" ) );
            configurationValues.Add( SHOW_COUNT_DOWN, new ConfigurationValue( "Show Character Limit Countdown", "When set, displays a countdown showing how many characters remain (for the Max Characters setting).", "" ) );

            if ( controls != null )
            {
                if ( controls.Count > 0 && controls[0] != null && controls[0] is NumberBox )
                {
                    configurationValues[NUMBER_OF_ROWS].Value = ( ( NumberBox ) controls[0] ).Text;
                }

                if ( controls.Count > 1 && controls[1] != null && controls[1] is RockCheckBox )
                {
                    configurationValues[ALLOW_HTML].Value = ( ( RockCheckBox ) controls[1] ).Checked.ToString();
                }

                if ( controls.Count > 2 && controls[2] != null && controls[2] is NumberBox )
                {
                    configurationValues[MAX_CHARACTERS].Value = ( ( NumberBox ) controls[2] ).Text;
                }

                if ( controls.Count > 3 && controls[3] != null && controls[3] is CheckBox )
                {
                    configurationValues[SHOW_COUNT_DOWN].Value = ( ( CheckBox ) controls[3] ).Checked.ToString();
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
                if ( controls.Count > 0 && controls[0] != null && controls[0] is NumberBox && configurationValues.ContainsKey( NUMBER_OF_ROWS ) )
                {
                    ( ( NumberBox ) controls[0] ).Text = configurationValues[NUMBER_OF_ROWS].Value;
                }

                if ( controls.Count > 1 && controls[1] != null && controls[1] is RockCheckBox && configurationValues.ContainsKey( ALLOW_HTML ) )
                {
                    ( ( RockCheckBox ) controls[1] ).Checked = configurationValues[ALLOW_HTML].Value.AsBoolean();
                }

                if ( controls.Count > 2 && controls[2] != null && controls[2] is NumberBox && configurationValues.ContainsKey( MAX_CHARACTERS ) )
                {
                    ( ( NumberBox ) controls[2] ).Text = configurationValues[MAX_CHARACTERS].Value;
                }

                if ( controls[3] != null && controls[3] is CheckBox && configurationValues.ContainsKey( SHOW_COUNT_DOWN ) )
                {
                    ( ( CheckBox ) controls[3] ).Checked = configurationValues[SHOW_COUNT_DOWN].Value.AsBoolean();
                }
            }
        }


        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The id.</param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            RockTextBox tb = new RockTextBox { ID = id, TextMode = TextBoxMode.MultiLine };
            int? rows = 3;
            bool allowHtml = false;
            int? maximumLength = null;
            var showCountDown = false;

            if ( configurationValues != null )
            {
                if ( configurationValues.ContainsKey( NUMBER_OF_ROWS ) )
                {
                    rows = configurationValues[NUMBER_OF_ROWS].Value.AsIntegerOrNull() ?? 3;
                }
                if ( configurationValues.ContainsKey( ALLOW_HTML ) )
                {
                    allowHtml = configurationValues[ALLOW_HTML].Value.AsBoolean();
                }
                if ( configurationValues.ContainsKey( MAX_CHARACTERS ) )
                {
                    maximumLength = configurationValues[MAX_CHARACTERS].Value.AsIntegerOrNull();
                }

                if ( configurationValues.ContainsKey( SHOW_COUNT_DOWN ) )
                {
                    showCountDown = configurationValues[SHOW_COUNT_DOWN].Value.AsBoolean();
                }
            }

            tb.Rows = rows.HasValue ? rows.Value : 3;
            if ( maximumLength.HasValue )
            {
                tb.MaxLength = maximumLength.Value;
            }
            tb.ValidateRequestMode = allowHtml ? ValidateRequestMode.Disabled : ValidateRequestMode.Enabled;
            tb.ShowCountDown = showCountDown;

            return tb;
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