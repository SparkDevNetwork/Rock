// <copyright>
// Copyright by BEMA Information Technologies
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
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Field;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace com.bemaservices.WorkflowExtensions.Field.Types
{
    public class ScheduleBuilderFieldType : Rock.Field.FieldType, IEntityFieldType
    {
        #region Configuration

        public const string SCHEDULE = "schedule";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( SCHEDULE );
            return configKeys;
        }

        ///// <summary>
        ///// Creates the HTML controls required to configure this type of field
        ///// </summary>
        ///// <returns></returns>
        //public override List<Control> ConfigurationControls()
        //{
        //    var controls = base.ConfigurationControls();

        //    var sbSchedule = new ScheduleBuilder();
        //    controls.Add( sbSchedule );
        //    sbSchedule.Label = "Default Schedule";

        //    return controls;
        //}

        ///// <summary>
        ///// Gets the configuration value.
        ///// </summary>
        ///// <param name="controls">The controls.</param>
        ///// <returns></returns>
        //public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        //{
        //    Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();
        //    configurationValues.Add( SCHEDULE, new ConfigurationValue( "Default Schedule", "", string.Empty ) );

        //    if ( controls != null )
        //    {
        //        if ( controls.Count > 0 )
        //        {
        //            var sbSchedule = controls[0] as ScheduleBuilder;
        //            configurationValues[SCHEDULE].Value = sbSchedule?.iCalendarContent;
        //        }
        //    }

        //    return configurationValues;
        //}

        ///// <summary>
        ///// Sets the configuration value.
        ///// </summary>
        ///// <param name="controls"></param>
        ///// <param name="configurationValues"></param>
        //public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        //{
        //    if ( controls != null && configurationValues != null )
        //    {
        //        if ( controls.Count > 0 )
        //        {
        //            var sbSchedule = controls[0] as ScheduleBuilder;
        //            if ( sbSchedule != null && configurationValues.ContainsKey( SCHEDULE ) )
        //            {
        //                sbSchedule.iCalendarContent = configurationValues[SCHEDULE]?.Value;
        //            }
        //        }
        //    }
        //}

        #endregion

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
            using ( var rockContext = new RockContext() )
            {
                var scheduleService = new ScheduleService( rockContext );
                Schedule schedule = null;
                Guid? scheduleGuid = value.AsGuidOrNull();
                if ( scheduleGuid.HasValue )
                {
                    schedule = scheduleService.GetNoTracking( scheduleGuid.Value );
                }

                if ( schedule != null )
                {
                    return schedule.FriendlyScheduleText;
                }
            }

            return base.FormatValue( parentControl, value, configurationValues, condensed );
        }

        #endregion

        /// <summary>
        /// Gets a value indicating whether this field has a control to configure the default value
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has default control; otherwise, <c>false</c>.
        /// </value>
        public override bool HasDefaultControl => false;

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The id.</param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            ScheduleBuilder sbSchedule = new ScheduleBuilder { ID = id };

            return sbSchedule;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            ScheduleBuilder sbSchedule = control as ScheduleBuilder;
            sbSchedule.ShowScheduleFriendlyTextAsToolTip = true;

            if ( sbSchedule != null )
            {
                if ( sbSchedule.iCalendarContent.IsNotNullOrWhiteSpace() )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var schedule = BuildScheduleFromICalContent( sbSchedule.iCalendarContent );
                        var scheduleService = new ScheduleService( rockContext );
                        scheduleService.Add( schedule );
                        rockContext.SaveChanges();

                        return schedule.Guid.ToString();
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            ScheduleBuilder sbSchedule = control as ScheduleBuilder;
            if ( sbSchedule != null )
            {
                var rockContext = new RockContext();
                var scheduleService = new ScheduleService( rockContext );
                Schedule schedule = null;
                Guid? scheduleGuid = value.AsGuidOrNull();
                if ( scheduleGuid.HasValue )
                {
                    schedule = scheduleService.Get( scheduleGuid.Value );
                    if ( schedule != null )
                    {
                        sbSchedule.iCalendarContent = schedule.iCalendarContent;
                    }
                }

                sbSchedule.ShowScheduleFriendlyTextAsToolTip = true;
            }
        }

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
            var item = new ScheduleService( new RockContext() ).Get( guid );
            return item != null ? item.Id : ( int? ) null;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            var item = new ScheduleService( new RockContext() ).Get( id ?? 0 );
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
                return new ScheduleService( rockContext ).Get( guid.Value );
            }

            return null;
        }

        #endregion

        public static Schedule BuildScheduleFromICalContent( string iCalContent )
        {
            var schedule = new Schedule();
            schedule.iCalendarContent = iCalContent;
            var calEvent = ScheduleICalHelper.GetCalendarEvent( iCalContent );
            if ( calEvent != null )
            {
                schedule.EffectiveStartDate = calEvent.DTStart != null ? calEvent.DTStart.Value.Date : ( DateTime? ) null;
                schedule.EffectiveEndDate = calEvent.DTEnd != null ? calEvent.DTEnd.Value.Date : ( DateTime? ) null;
            }

            return schedule;
        }
    }
}
