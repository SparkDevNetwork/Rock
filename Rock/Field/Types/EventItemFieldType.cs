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
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a dropdown list of event items
    /// </summary>
    [Serializable]
    public class EventItemFieldType : FieldType
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

            Guid? eventItemGuid = value.AsGuidOrNull();
            if ( eventItemGuid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var eventItem = new EventItemService( rockContext ).Get( eventItemGuid.Value );
                    if ( eventItem != null )
                    {
                        formattedValue = eventItem.Name;
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
            return new EventItemPicker { ID = id }; 
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control != null && control is EventItemPicker )
            {
                int id = int.MinValue;
                if ( Int32.TryParse( ( (EventItemPicker)control ).SelectedValue, out id ) )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var eventItem = new EventItemService( rockContext ).Get( id );
                        if ( eventItem != null )
                        {
                            return eventItem.Guid.ToString();
                        }
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
            Guid eventItemGuid = Guid.Empty;
            if ( Guid.TryParse( value, out eventItemGuid ) )
            {
                if ( control != null && control is EventItemPicker )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var eventItem = new EventItemService( rockContext ).Get( eventItemGuid );
                        if ( eventItem != null )
                        {
                            ( (EventItemPicker)control ).SetValue( eventItem.Id.ToString() );
                        }
                    }
                }
            }
        }

        #endregion

    }
}