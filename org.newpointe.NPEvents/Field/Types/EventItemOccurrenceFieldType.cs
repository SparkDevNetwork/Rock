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
using System.Web.UI;

using Rock;
using Rock.Field;
using Rock.Data;
using Rock.Model;
using org.newpointe.RockU.Web.UI.Controls;

namespace org.newpointe.RockU.Field.Types
{
    /// <summary>
    /// Field Type used to display a dropdown list of event items
    /// </summary>
    [Serializable]
    public class EventItemOccurrenceFieldType : Rock.Field.FieldType, IEntityFieldType
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

            var eventItemOccurrence = GetEntity( value ) as EventItemOccurrence;
            if ( eventItemOccurrence != null )
            {
                formattedValue = eventItemOccurrence.Id + " - " + eventItemOccurrence.EventItem.Name + " (" + eventItemOccurrence.NextStartDateTime + ")";
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
            return new EventItemOccurrencePicker { ID = id };
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public IEntity GetEditValueAsEntity( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control != null && control is EventItemOccurrencePicker )
            {
                return GetEntity( ( (EventItemOccurrencePicker)control ).SelectedValue );
            }
            return null;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            return GetEditValueAsEntity( control, configurationValues )?.Guid.ToString();
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( control != null && control is EventItemOccurrencePicker )
            {
                var entity = GetEntity( value );
                if ( entity != null )
                {
                    ( (EventItemOccurrencePicker)control ).SetValue( entity.Id.ToString() );
                }
            }
        }

        #endregion

        #region Entity Methods

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            return GetEditValueAsEntity( control, configurationValues )?.Id;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            if ( id != null && control != null && control is EventItemOccurrencePicker )
            {
                ( (EventItemOccurrencePicker)control ).SetValue( id.ToString() );
            }
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value )
        {
            return GetEntity( value, null );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value, RockContext rockContext )
        {
            if ( Guid.TryParse( value, out Guid eventItemGuid ) )
            {
                return new EventItemOccurrenceService( new RockContext() ).Get( eventItemGuid );
            }
            if ( Int32.TryParse( value, out int eventItemId ) )
            {
                return new EventItemOccurrenceService( new RockContext() ).Get( eventItemId );
            }
            return null;
        }

        #endregion

    }
}