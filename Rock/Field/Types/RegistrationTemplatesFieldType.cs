﻿// <copyright>
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
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Stored as a List of RegistrationTemplate Guids
    /// </summary>
    public class RegistrationTemplatesFieldType : FieldType
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

                if ( !guids.Any() )
                {
                    return base.FormatValue( parentControl, formattedValue, null, condensed );
                }

                using ( var rockContext = new RockContext() )
                {
                    var registrationTemplates = new RegistrationTemplateService( rockContext ).Queryable().AsNoTracking().Where( a => guids.Contains( a.Guid ) );
                    if ( registrationTemplates.Any() )
                    {
                        formattedValue = string.Join( ", ", ( from registrationTemplate in registrationTemplates select registrationTemplate.Name ).ToArray() );
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
            return new RegistrationTemplatePicker { ID = id, AllowMultiSelect = true };
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as RegistrationTemplatePicker;
            if ( picker == null )
            {
                return null;
            }

            string result = null;

            var ids = picker.SelectedValuesAsInt().ToList();
            using ( var rockContext = new RockContext() )
            {
                var registrationTemplates = new RegistrationTemplateService( rockContext ).GetByIds( ids ).ToList();

                if ( registrationTemplates.Any() )
                {
                    result = registrationTemplates.Select( s => s.Guid.ToString() ).ToList().AsDelimited( "," );
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
            var picker = control as RegistrationTemplatePicker;

            if ( picker != null )
            {
                var guids = value?.SplitDelimitedValues().AsGuidList() ?? new List<Guid>();

                if ( guids.Any() )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var registrationTemplates = new RegistrationTemplateService( rockContext ).GetByGuids( guids ).ToList();
                        picker.SetValues( registrationTemplates );
                    }
                }
                else
                {
                    // make sure that no registration templates are selected
                    picker.SetValues( new List<RegistrationTemplate>() );
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
