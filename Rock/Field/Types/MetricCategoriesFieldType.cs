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
using System.Linq;
using System.Web.UI;

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Stored as a List of Metric.Guid|MetricCategory.Guid (MetricCategory.Guid included so we can preserve which category the metric was selected from)
    /// </summary>
    public class MetricCategoriesFieldType : FieldType
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
                var guidPairs = Rock.Attribute.MetricCategoriesFieldAttribute.GetValueAsGuidPairs( value );

                var metricGuids = guidPairs.Select( a => a.MetricGuid );

                var metrics = new MetricService( new RockContext() ).Queryable().Where( a => metricGuids.Contains( a.Guid ) );
                if ( metrics.Any() )
                {
                    formattedValue = string.Join( ", ", ( from metric in metrics select metric.Title ).ToArray() );
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
            return new MetricCategoryPicker { ID = id, AllowMultiSelect = true };
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as MetricCategoryPicker;
            string result = null;

            if ( picker != null )
            {
                var ids = picker.SelectedValuesAsInt();
                var metricCategories = new MetricCategoryService( new RockContext() ).Queryable().Where( a => ids.Contains( a.Id ) );

                if ( metricCategories.Any() )
                {
                    var guidPairList = metricCategories.Select( a => new { MetricGuid = a.Metric.Guid, CategoryGuid = a.Category.Guid } ).ToList();
                    result = guidPairList.Select( s => string.Format( "{0}|{1}", s.MetricGuid, s.CategoryGuid ) ).ToList().AsDelimited( "," );
                }
            }

            return result;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( value != null )
            {
                var picker = control as MetricCategoryPicker;

                if ( picker != null )
                {
                    List<MetricCategory> metricCategories = new List<MetricCategory>();
                    var guidPairs = Rock.Attribute.MetricCategoriesFieldAttribute.GetValueAsGuidPairs( value );
                    MetricCategoryService metricCategoryService = new MetricCategoryService( new RockContext() );

                    foreach (var guidPair in guidPairs)
                    {
                        // first try to get each metric from the category that it was selected from
                        var metricCategory = metricCategoryService.Queryable().Where( a => a.Metric.Guid == guidPair.MetricGuid && a.Category.Guid == guidPair.CategoryGuid ).FirstOrDefault();
                        if (metricCategory == null)
                        {
                            // if the metric isn't found in the original category, just the first one, ignoring category
                            metricCategory = metricCategoryService.Queryable().Where( a => a.Metric.Guid == guidPair.MetricGuid ).FirstOrDefault();
                        }

                        if (metricCategory != null)
                        {
                            metricCategories.Add( metricCategory );
                        }
                    }
                    
                    picker.SetValues( metricCategories );
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
        /// <returns></returns>
        public override System.Web.UI.Control FilterControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id, bool required )
        {
            // This fieldtype does not support filtering
            return null;
        }

        #endregion

    }
}
