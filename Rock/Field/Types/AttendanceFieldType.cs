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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) Attendance
    /// Stored as Attendance.Guid
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    public class AttendanceFieldType : FieldType, IEntityFieldType
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
            string formattedValue = value;

            Guid? guid = value.AsGuidOrNull();
            if ( !guid.HasValue )
            {
                var attendanceId = value.AsIntegerOrNull();
                if ( attendanceId.HasValue )
                {
                    // if an Id was specified instead of a Guid, get the Guid instead
                    using ( var rockContext = new RockContext() )
                    {
                        guid = new AttendanceService( rockContext ).GetGuid( attendanceId.Value );
                    }
                }
            }

            if ( guid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var attendance = new AttendanceService( rockContext ).GetNoTracking( guid.Value );
                    if ( attendance != null )
                    {
                        formattedValue = attendance.ToString();
                    }
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
            RockTextBox tbAttendanceGuid = new RockTextBox { ID = id };
            return tbAttendanceGuid;
        }

        /// <summary>
        /// Reads new values entered by the user for the field (as guid)
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            RockTextBox tbAttendanceGuidOrId = control as RockTextBox;
            if ( tbAttendanceGuidOrId != null )
            {
                var attendanceGuid = tbAttendanceGuidOrId.Text.AsGuidOrNull();
                if ( attendanceGuid.HasValue )
                {
                    return attendanceGuid.ToString();
                }

                var attendanceId = tbAttendanceGuidOrId.Text.AsIntegerOrNull();
                if ( attendanceId.HasValue )
                {
                    // if an Id was specified instead of a Guid, get the Guid instead
                    using ( var rockContext = new RockContext() )
                    {
                        attendanceGuid = new AttendanceService( rockContext ).GetGuid( attendanceId.Value );
                    }

                    return attendanceGuid.ToString();
                }

                return tbAttendanceGuidOrId.Text ?? string.Empty;
            }

            return null;
        }

        /// <summary>
        /// Sets the value (as guid)
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var tbAttendanceGuid = control as RockTextBox;

            if ( tbAttendanceGuid != null )
            {
                tbAttendanceGuid.Text = value;
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
        public int? GetEditValueAsEntityId( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            Guid guid = GetEditValue( control, configurationValues ).AsGuid();
            return new AttendanceService( new RockContext() ).GetId( guid );
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            var itemGuid = new AttendanceService( new RockContext() ).GetGuid( id ?? 0 );
            SetEditValue( control, configurationValues, itemGuid.ToString() );
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
                return new AttendanceService( rockContext ).Get( guid.Value );
            }

            return null;
        }

        #endregion
    }
}