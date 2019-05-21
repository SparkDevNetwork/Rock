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

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a workflow type picker. Stored as WorkflowType guid. 
    /// </summary>
    [Serializable]
    public class WorkflowTypeFieldType : FieldType, IEntityFieldType
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
                Guid? guid = value.AsGuidOrNull();
                if ( guid.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var workflowTypeName = new WorkflowTypeService( rockContext ).GetSelect( guid.Value, a => a.Name );
                        if ( workflowTypeName != null )
                        {
                            formattedValue = workflowTypeName;
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
            return new WorkflowTypePicker { ID = id, AllowMultiSelect = false }; 
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
            if ( picker != null )
            {
                int? itemId = picker.SelectedValue.AsIntegerOrNull();
                Guid? itemGuid = null;
                if ( itemId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        itemGuid = new WorkflowTypeService( rockContext ).GetGuid( itemId.Value );
                    }
                }

                return itemGuid?.ToString() ?? string.Empty;
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
                WorkflowType item = null;
                Guid? guid = value.AsGuidOrNull();

                // get the item (or null) and set it
                if ( guid.HasValue )
                {
                    var rockContext = new RockContext();
                    item = new WorkflowTypeService( rockContext ).GetNoTracking( guid.Value );
                }

                picker.SetValue( item );
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
            Guid guid = GetEditValue( control, configurationValues ).AsGuid();
            return new WorkflowTypeService( new RockContext() ).GetId( guid );
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            var item = new WorkflowTypeService( new RockContext() ).Get( id ?? 0 );
            string guidValue = item != null ? item.Guid.ToString() : string.Empty;
            SetEditValue( control, configurationValues, guidValue );
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
            Guid? guid = value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                rockContext = rockContext ?? new RockContext();
                return new WorkflowTypeService( rockContext ).Get( guid.Value );
            }

            return null;
        }

        #endregion

    }
}