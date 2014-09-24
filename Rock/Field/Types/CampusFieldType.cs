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
using System.Linq;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) CampusFieldType
    /// Stored as Campus's Guid
    /// </summary>
    public class CampusFieldType : FieldType, IEntityFieldType
    {
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
                var campus = CampusCache.Read( value.AsGuid() );
                if ( campus != null )
                {
                    formattedValue = campus.Name;
                }
            }

            return base.FormatValue( parentControl, formattedValue, configurationValues, condensed );
        }

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
            var campusPicker = new CampusPicker { ID = id };

            var campusList = CampusCache.All();

            if ( campusList.Any() )
            {
                campusPicker.Campuses = campusList;
                return campusPicker;
            }

            return null;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// returns Campus.Guid as string
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            CampusPicker campusPicker = control as CampusPicker;

            if ( campusPicker != null )
            {
                int? campusId = campusPicker.SelectedCampusId;
                if (campusId.HasValue)
                {
                    var campus = CampusCache.Read( campusId.Value );
                    if (campus != null )
                    {
                        return campus.Guid.ToString();
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the value.
        /// Expects value as a Campus.Guid as string
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            CampusPicker campusPicker = control as CampusPicker;

            if ( campusPicker != null )
            {
                Guid guid = value.AsGuid();

                // get the item (or null) and set it
                var campus = CampusCache.Read( guid );
                campusPicker.SetValue( campus == null ? "0" : campus.Id.ToString() );
            }
        }

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            Guid guid = GetEditValue( control, configurationValues ).AsGuid();
            var item = CampusCache.Read( guid );
            return item != null ? item.Id : (int?)null;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            CampusCache item = null;
            if ( id.HasValue )
            {
                item = CampusCache.Read( id.Value );
            }
            string guidValue = item != null ? item.Guid.ToString() : string.Empty;
            SetEditValue( control, configurationValues, guidValue );
        }
    }
}