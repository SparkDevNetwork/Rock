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
using System.Linq;
#if WEBFORMS
using System.Web.UI;
using OpenXmlPowerTools;
#endif
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Stored as a List of Schedule Guids
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.SCHEDULES )]
    public class SchedulesFieldType : FieldType, IEntityReferenceFieldType, ISplitMultiValueFieldType
    {
        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string value, Dictionary<string, string> privateConfigurationValues )
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
                    var schedules = guids.Select( a => NamedScheduleCache.Get( a ) ).ToList();
                    if ( schedules.Any() )
                    {
                        formattedValue = string.Join( ", ", ( from schedule in schedules select schedule?.Name ) );
                    }
                }
            }

            return formattedValue;
        }

        #endregion

        #region Edit Control

        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            var scheduleValues = publicValue.FromJsonOrNull<List<ListItemBag>>();

            if ( scheduleValues?.Any() == true )
            {
                return string.Join( ",", scheduleValues.Select( s => s.Value ) );
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( !string.IsNullOrWhiteSpace( privateValue ) )
            {
                var scheduleValues = new List<ListItemBag>();

                foreach ( string guidValue in privateValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                {
                    Guid? guid = guidValue.AsGuidOrNull();
                    if ( guid.HasValue )
                    {
                        var schedule = NamedScheduleCache.Get( guid.Value );
                        if ( schedule != null )
                        {
                            var scheduleValue = new ListItemBag()
                            {
                                Text = schedule.Name,
                                Value = schedule.Guid.ToString(),
                            };

                            scheduleValues.Add( scheduleValue );
                        }
                    }
                }

                if ( scheduleValues.Any() )
                {
                    return scheduleValues.ToCamelCaseJson( false, true );
                }
            }

            return string.Empty;
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return true;
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

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( string.IsNullOrWhiteSpace( privateValue ) )
            {
                return null;
            }

            var guids = new List<Guid>();

            foreach ( string guidValue in privateValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
            {
                Guid? guid = guidValue.AsGuidOrNull();
                if ( guid.HasValue )
                {
                    guids.Add( guid.Value );
                }
            }

            using ( var rockContext = new RockContext() )
            {
                var referencedEntities = guids.Select( a => new ScheduleService( rockContext ).Get( a ) )
                .Select( s => s.Id )
                .ToList()
                .Select( s => new ReferencedEntity( EntityTypeCache.GetId<Schedule>().Value, s ) )
                .ToList();

                if ( !referencedEntities.Any() )
                {
                    return null;
                }

                return referencedEntities;
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the Name property of a Group and
            // should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<Schedule>().Value, nameof( Schedule.Name ) )
            };
        }

        #endregion

        #region ISplitMultiValueFieldType

        /// <inheritdoc/>
        public ICollection<string> SplitMultipleValues( string privateValue )
        {
            return privateValue.Split( ',' );
        }

        #endregion

        #region WebForms
#if WEBFORMS

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
            return !condensed
                ? GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

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
            return new SchedulePicker { ID = id, AllowMultiSelect = true };
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as SchedulePicker;
            string result = string.Empty;

            if ( picker != null )
            {
                var ids = picker.SelectedValuesAsInt().ToList();

                var schedules = ids.Select( a => NamedScheduleCache.Get( a ) ).Where( a => a != null ).ToList();

                if ( schedules.Any() )
                {
                    result = schedules.Select( s => s.Guid.ToString() ).ToList().AsDelimited( "," );
                }

                return result;
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
            var picker = control as SchedulePicker;

            if ( picker != null )
            {
                var guids = value?.SplitDelimitedValues().AsGuidList() ?? new List<Guid>();

                if ( guids.Any() )
                {
                    var scheduleIds = guids.Select( g => NamedScheduleCache.GetId( g ) ).Where( a => a.HasValue ).Select( a => a.Value ).ToList();
                    picker.SetValues( scheduleIds );
                }
                else
                {
                    // make sure that no schedules are selected
                    picker.SetValues( new List<int>() );
                }
            }
        }

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
            return base.FilterControl( configurationValues, id, required, filterMode );
        }

#endif
        #endregion
    }
}
