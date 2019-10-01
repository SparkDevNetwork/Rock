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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;

using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a workflow type picker with option to select multiple. Stored as a comma-delimited list of WorkflowType Guids
    /// </summary>
    [Serializable]
    public class WorkflowTypesFieldType : FieldType
    {

        #region Formatting

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
            string formattedValue = string.Empty;

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                var names = new List<string>();
                var guids = new List<Guid>();

                foreach ( string guidValue in value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                {
                    Guid? guid = guidValue.AsGuidOrNull();
                    if ( guid.HasValue )
                    {
                        guids.Add( guid.Value );
                    }
                }

                if ( guids.Any() )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var workflowTypes = new WorkflowTypeService( rockContext ).Queryable().AsNoTracking().Where( a => guids.Contains( a.Guid ) );
                        if ( workflowTypes.Any() )
                        {
                            formattedValue = string.Join( ", ", ( from workflowType in workflowTypes select workflowType.Name ).ToArray() );
                        }
                    }
                }
            }

            return base.FormatValue( parentControl, formattedValue, null, condensed );

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
            return new WorkflowTypePicker { ID = id, AllowMultiSelect = true };
        }

        /// <summary>
        /// Reads new values entered by the user for the field ( as Guid )
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as WorkflowTypePicker;
            string result = string.Empty;

            if ( picker != null )
            {
                var ids = picker.SelectedValuesAsInt().ToList();
                using ( var rockContext = new RockContext() )
                {
                    var items = new WorkflowTypeService( rockContext ).GetByIds( ids ).ToList();

                    if ( items.Any() )
                    {
                        result = items.Select( s => s.Guid.ToString() ).ToList().AsDelimited( "," );
                    }
                }

                return result;
            }

            return null;
        }

        /// <summary>
        /// Sets the value. ( as Guid )
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var picker = control as WorkflowTypePicker;

            if ( picker != null )
            {
                var guids = value?.SplitDelimitedValues().AsGuidList() ?? new List<Guid>();
                var workflowTypes = new List<WorkflowType>();

                if ( guids.Any() )
                {
                    var rockContext = new RockContext();
                    workflowTypes = new WorkflowTypeService( rockContext ).GetByGuids( guids ).AsNoTracking().ToList();
                }

                picker.SetValues( workflowTypes );
            }
        }

        #endregion

        #region FilterControl

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
            var control = base.FilterValueControl(configurationValues, id, required, filterMode );
            WorkflowTypePicker workflowTypePicker = (WorkflowTypePicker)control;
            workflowTypePicker.Required = required;
            workflowTypePicker.AllowMultiSelect = false;
            return control;
        }

        /// <summary>
        /// Gets the type of the filter comparison.
        /// </summary>
        /// <value>
        /// The type of the filter comparison.
        /// </value>
        public override ComparisonType FilterComparisonType
        {
            get
            {
                return ComparisonHelper.ContainsFilterComparisonTypes;
            }
        }

        #endregion

    }
}