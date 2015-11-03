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
using System;
using System.Collections.Generic;
using System.Web.UI;

using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) MergeTemplate
    /// Stored as MergeTemplate's Guid
    /// </summary>
    public class MergeTemplateFieldType : FieldType
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
        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            string formattedValue = value;

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                var mergeTemplate = new MergeTemplateService( new RockContext() ).Get( value.AsGuid() );
                if ( mergeTemplate != null )
                {
                    formattedValue = mergeTemplate.Name;
                }
            }

            return base.FormatValue( parentControl, formattedValue, configurationValues, condensed );
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
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var mergeTemplatePicker = new MergeTemplatePicker { ID = id };
            return mergeTemplatePicker;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// returns MergeTemplate.Guid as string
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            MergeTemplatePicker mergeTemplatePicker = control as MergeTemplatePicker;

            if ( mergeTemplatePicker != null )
            {
                int? mergeTemplateId = mergeTemplatePicker.SelectedValue.AsIntegerOrNull();
                if ( mergeTemplateId.HasValue )
                {
                    var mergeTemplate = new MergeTemplateService( new RockContext() ).Get( mergeTemplateId.Value );
                    if ( mergeTemplate != null )
                    {
                        return mergeTemplate.Guid.ToString();
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the value.
        /// Expects value as a MergeTemplate.Guid as string
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            MergeTemplatePicker mergeTemplatePicker = control as MergeTemplatePicker;

            if ( mergeTemplatePicker != null )
            {
                Guid guid = value.AsGuid();

                // get the item (or null) and set it
                var mergeTemplate = new MergeTemplateService( new RockContext() ).Get( guid );
                mergeTemplatePicker.SetValue( mergeTemplate );
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
        public override Control FilterControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
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