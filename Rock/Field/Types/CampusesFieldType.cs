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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// 
    /// </summary>
    public class CampusesFieldType : SelectFromListFieldType
    {
        // internal configuration values needed since it is not passed to ListSource
        private Dictionary<string, ConfigurationValue> _configurationValues = null;

        #region Configuration

        private const string INCLUDE_INACTIVE_KEY = "includeInactive";
        private const string REPEAT_COLUMNS = "repeatColumns";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( INCLUDE_INACTIVE_KEY );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // Add checkbox for deciding if the list should include inactive items
            var cb = new RockCheckBox();
            controls.Add( cb );
            cb.AutoPostBack = true;
            cb.CheckedChanged += OnQualifierUpdated;
            cb.Label = "Include Inactive";
            cb.Text = "Yes";
            cb.Help = "When set, inactive campuses will be included in the list.";

            var tbRepeatColumns = new NumberBox();
            tbRepeatColumns.Label = "Columns";
            tbRepeatColumns.Help = "Select how many columns the list should use before going to the next row. If blank or 0 then 4 columns will be displayed. There is no upper limit enforced here however the block this is used in might add contraints due to available space.";
            tbRepeatColumns.MinimumValue = "0";
            tbRepeatColumns.AutoPostBack = true;
            tbRepeatColumns.TextChanged += OnQualifierUpdated;
            controls.Add( tbRepeatColumns );

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

            string description = "When set, inactive campuses will be included in the list.";
            configurationValues.Add( INCLUDE_INACTIVE_KEY, new ConfigurationValue( "Include Inactive", description, string.Empty ) );

            description = "Select how many columns the list should use before going to the next row. If blank 4 is used.";
            configurationValues.Add( REPEAT_COLUMNS, new ConfigurationValue( "Repeat Columns", description, string.Empty ) );

            if ( controls != null )
            {
                if ( controls.Count > 0 && controls[0] != null && controls[0] is CheckBox )
                {
                    configurationValues[INCLUDE_INACTIVE_KEY].Value = ( (CheckBox)controls[0] ).Checked.ToString();
                }

                if ( controls.Count > 1 && controls[1] != null && controls[1] is NumberBox )
                {
                    configurationValues[REPEAT_COLUMNS].Value = ( ( NumberBox ) controls[1] ).Text;
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
                if ( controls.Count > 0 && controls[0] != null && controls[0] is CheckBox && configurationValues.ContainsKey( INCLUDE_INACTIVE_KEY ) )
                {
                    ( (CheckBox)controls[0] ).Checked = configurationValues[INCLUDE_INACTIVE_KEY].Value.AsBoolean();
                }

                if ( controls.Count > 1 && controls[1] != null && controls[1] is NumberBox && configurationValues.ContainsKey( REPEAT_COLUMNS ) )
                {
                    ( ( NumberBox ) controls[1] ).Text = configurationValues[REPEAT_COLUMNS].Value;
                }
            }
        }

        #endregion

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            _configurationValues = configurationValues;
            return base.EditControl( configurationValues, id );
        }

        /// <summary>
        /// Gets the list source.
        /// </summary>
        /// <value>
        /// The list source.
        /// </value>
        internal override Dictionary<string, string> ListSource
        {
            get
            {
                var allCampuses = CampusCache.All();

                bool includeInactive = ( _configurationValues != null && _configurationValues.ContainsKey( INCLUDE_INACTIVE_KEY ) && _configurationValues[INCLUDE_INACTIVE_KEY].Value.AsBoolean() );

                var campusList = allCampuses
                    .Where( c => !c.IsActive.HasValue || c.IsActive.Value || includeInactive )
                    .ToList();

                return campusList.ToDictionary( c => c.Guid.ToString(), c => c.Name );
            }
        }
    }
}
