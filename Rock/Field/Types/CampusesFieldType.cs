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
    public class CampusesFieldType : SelectFromListFieldType, ICachedEntitiesFieldType
    {
        #region Configuration

        private const string INCLUDE_INACTIVE_KEY = "includeInactive";
        private const string REPEAT_COLUMNS = "repeatColumns";
        private const string FILTER_CAMPUS_TYPES_KEY = "filterCampusTypes";
        private const string FILTER_CAMPUS_STATUS_KEY = "filterCampusStatus";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            //configKeys.Add( REPEAT_COLUMNS );
            configKeys.Add( INCLUDE_INACTIVE_KEY );
            configKeys.Add( FILTER_CAMPUS_TYPES_KEY );
            configKeys.Add( FILTER_CAMPUS_STATUS_KEY );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            // Add checkbox for deciding if the list should include inactive items
            var cbIncludeInactive = new RockCheckBox();
            cbIncludeInactive.AutoPostBack = true;
            cbIncludeInactive.CheckedChanged += OnQualifierUpdated;
            cbIncludeInactive.Label = "Include Inactive";
            cbIncludeInactive.Text = "Yes";
            cbIncludeInactive.Help = "When set, inactive campuses will be included in the list.";

            // Checkbox list to select Filter Campus Types
            var campusTypeDefinedValues = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.CAMPUS_TYPE.AsGuid() ).DefinedValues.Select( v => new { Text = v.Value, Value = v.Id } );
            var cblCampusTypes = new RockCheckBoxList();
            cblCampusTypes.AutoPostBack = true;
            cblCampusTypes.SelectedIndexChanged += OnQualifierUpdated;
            cblCampusTypes.RepeatDirection = RepeatDirection.Horizontal;
            cblCampusTypes.Label = "Filter Campus Types";
            cblCampusTypes.Help = "When set this will filter the campuses displayed in the list to the selected Types. Setting a filter will cause the campus picker to display even if 0 campuses are in the list.";
            cblCampusTypes.DataTextField = "Text";
            cblCampusTypes.DataValueField = "Value";
            cblCampusTypes.DataSource = campusTypeDefinedValues;
            cblCampusTypes.DataBind();

            // Checkbox list to select Filter Campus Status
            var campusStatusDefinedValues = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.CAMPUS_STATUS.AsGuid() ).DefinedValues.Select( v => new { Text = v.Value, Value = v.Id } );
            var cblCampusStatuses = new RockCheckBoxList();
            cblCampusStatuses.AutoPostBack = true;
            cblCampusStatuses.SelectedIndexChanged += OnQualifierUpdated;
            cblCampusStatuses.RepeatDirection = RepeatDirection.Horizontal;
            cblCampusStatuses.Label = "Filter Campus Status";
            cblCampusStatuses.Help = "When set this will filter the campuses displayed in the list to the selected Statuses. Setting a filter will cause the campus picker to display even if 0 campuses are in the list.";
            cblCampusStatuses.DataTextField = "Text";
            cblCampusStatuses.DataValueField = "Value";
            cblCampusStatuses.DataSource = campusStatusDefinedValues;
            cblCampusStatuses.DataBind();

            var controls = base.ConfigurationControls();
            controls.Add( cbIncludeInactive );
            controls.Add( cblCampusTypes );
            controls.Add( cblCampusStatuses );

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

            string description = "Select how many columns the list should use before going to the next row. If blank 4 is used.";
            configurationValues.Add( REPEAT_COLUMNS, new ConfigurationValue( "Repeat Columns", description, string.Empty ) );

            description = "When set, inactive campuses will be included in the list.";
            configurationValues.Add( INCLUDE_INACTIVE_KEY, new ConfigurationValue( "Include Inactive", description, string.Empty ) );
            
            configurationValues.Add( FILTER_CAMPUS_TYPES_KEY, new ConfigurationValue( "Filter Campus Types", string.Empty, string.Empty ) );
            configurationValues.Add( FILTER_CAMPUS_STATUS_KEY, new ConfigurationValue( "Filter Campus Status", string.Empty, string.Empty ) );

            if ( controls != null )
            {
                if ( controls.Count > 0 && controls[0] != null && controls[0] is NumberBox )
                {
                    configurationValues[REPEAT_COLUMNS].Value = ( ( NumberBox ) controls[0] ).Text;
                }

                if ( controls.Count > 1 && controls[1] != null && controls[1] is CheckBox )
                {
                    configurationValues[INCLUDE_INACTIVE_KEY].Value = ( ( CheckBox ) controls[1] ).Checked.ToString();
                }
                
                if ( controls.Count > 2 && controls[2] != null && controls[2] is RockCheckBoxList )
                {
                    configurationValues[FILTER_CAMPUS_TYPES_KEY].Value = string.Join( ",", ( ( RockCheckBoxList ) controls[2] ).SelectedValues );
                }

                if ( controls.Count > 3 && controls[3] != null && controls[3] is RockCheckBoxList )
                {
                    configurationValues[FILTER_CAMPUS_STATUS_KEY].Value = string.Join( ",", ( ( RockCheckBoxList ) controls[3] ).SelectedValues );
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
                if ( controls.Count > 0 && controls[0] != null && controls[0] is NumberBox && configurationValues.ContainsKey( REPEAT_COLUMNS ) )
                {
                    ( ( NumberBox ) controls[0] ).Text = configurationValues[REPEAT_COLUMNS].Value;
                }

                if ( controls.Count > 1 && controls[1] != null && controls[1] is CheckBox && configurationValues.ContainsKey( INCLUDE_INACTIVE_KEY ) )
                {
                    ( ( CheckBox ) controls[1] ).Checked = configurationValues[INCLUDE_INACTIVE_KEY].Value.AsBoolean();
                }
                
                if ( controls.Count > 2 && controls[2] != null && controls[2] is RockCheckBoxList && configurationValues.ContainsKey( FILTER_CAMPUS_TYPES_KEY ) )
                {
                    var selectedCampusTypes = configurationValues[FILTER_CAMPUS_TYPES_KEY].Value.SplitDelimitedValues( false );
                    foreach( ListItem listItem in ( ( RockCheckBoxList)controls[2] ).Items )
                    {
                        listItem.Selected = selectedCampusTypes.Contains( listItem.Value );
                    }
                }
                
                if ( controls.Count > 3 && controls[3] != null && controls[3] is RockCheckBoxList && configurationValues.ContainsKey( FILTER_CAMPUS_STATUS_KEY ) )
                {
                    var selectedCampusTypes = configurationValues[FILTER_CAMPUS_STATUS_KEY].Value.SplitDelimitedValues( false );
                    foreach( ListItem listItem in ( ( RockCheckBoxList)controls[3] ).Items )
                    {
                        listItem.Selected = selectedCampusTypes.Contains( listItem.Value );
                    }
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
            return base.EditControl( configurationValues, id );
        }

        /// <summary>
        /// Gets the list source.
        /// </summary>
        /// <value>
        /// The list source.
        /// </value>
        internal override Dictionary<string, string> GetListSource( Dictionary<string, ConfigurationValue> configurationValues )
        {
            var allCampuses = CampusCache.All();

            bool includeInactive = ( configurationValues != null && configurationValues.ContainsKey( INCLUDE_INACTIVE_KEY ) && configurationValues[INCLUDE_INACTIVE_KEY].Value.AsBoolean() );
            List<int> campusTypesFilter = ( configurationValues != null && configurationValues.ContainsKey( FILTER_CAMPUS_TYPES_KEY ) ) ? configurationValues[FILTER_CAMPUS_TYPES_KEY].Value.SplitDelimitedValues( false ).AsIntegerList() : null;
            List<int> campusStatusFilter = ( configurationValues != null && configurationValues.ContainsKey( FILTER_CAMPUS_STATUS_KEY ) ) ? configurationValues[FILTER_CAMPUS_STATUS_KEY].Value.SplitDelimitedValues( false ).AsIntegerList() : null;

            var campusList = allCampuses
                .Where( c => ( !c.IsActive.HasValue || c.IsActive.Value || includeInactive )
                    && campusTypesFilter.ContainsOrEmpty( c.CampusTypeValueId ?? -1 )
                    && campusStatusFilter.ContainsOrEmpty( c.CampusStatusValueId ?? -1 ) )
                .ToList();

            return campusList.ToDictionary( c => c.Guid.ToString(), c => c.Name );
        }

        /// <summary>
        /// Gets the cached entities as a list.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public List<IEntityCache> GetCachedEntities( string value )
        {
            var guids = value.SplitDelimitedValues().AsGuidList();
            var result = new List<IEntityCache>();

            result.AddRange( guids.Select( g => CampusCache.Get( g ) ) );

            return result;
        }
    }
}
