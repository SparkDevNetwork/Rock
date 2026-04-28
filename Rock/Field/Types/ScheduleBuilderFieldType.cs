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
#endif
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field type that stores a schedule as Schedule.Guid.
    /// </summary>
    [FieldTypeUsage( FieldTypeUsage.Administrative )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.SCHEDULE_BUILDER )]
    public class ScheduleBuilderFieldType : FieldType, IEntityFieldType, IEntityReferenceFieldType
    {
        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string value, Dictionary<string, string> privateConfigurationValues )
        {
            var schedule = GetSchedule( value, null );

            return schedule != null && schedule.HasSchedule()
                ? schedule.ToFriendlyScheduleText( true )
                : string.Empty;
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
            var editValue = publicValue.FromJsonOrNull<ScheduleBuilderEditValueBag>();

            return SaveScheduleValue( editValue?.ScheduleGuid.AsGuidOrNull(), editValue?.ICalendarContent );
        }

        /// <inheritdoc/>
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var schedule = GetSchedule( privateValue, null );
            if ( schedule == null || !schedule.HasSchedule() )
            {
                return string.Empty;
            }

            return new ScheduleBuilderEditValueBag
            {
                ScheduleGuid = schedule.Guid.ToString(),
                ICalendarContent = schedule.iCalendarContent
            }.ToCamelCaseJson( false, true );
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Determines whether this filter has a filter control.
        /// </summary>
        /// <returns><c>false</c> because filtering is not currently supported.</returns>
        public override bool HasFilterControl()
        {
            return false;
        }

        #endregion

        #region Entity Methods

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The referenced schedule entity; otherwise <c>null</c>.</returns>
        public IEntity GetEntity( string value )
        {
            return GetEntity( value, null );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The referenced schedule entity; otherwise <c>null</c>.</returns>
        public IEntity GetEntity( string value, RockContext rockContext )
        {
            return GetSchedule( value, rockContext );
        }

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var guid = privateValue.AsGuidOrNull();
            if ( !guid.HasValue )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var scheduleId = new ScheduleService( rockContext ).GetId( guid.Value );
                if ( !scheduleId.HasValue )
                {
                    return null;
                }

                return new List<ReferencedEntity>
                {
                    new ReferencedEntity( EntityTypeCache.GetId<Schedule>().Value, scheduleId.Value )
                };
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type renders a friendly summary from the schedule definition,
            // so persisted values should update when the iCalendar content changes.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<Schedule>().Value, nameof( Schedule.iCalendarContent ) )
            };
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Gets the referenced schedule from the stored value.
        /// </summary>
        /// <param name="value">The stored schedule GUID.</param>
        /// <param name="rockContext">The optional rock context.</param>
        /// <returns>The matching schedule; otherwise <c>null</c>.</returns>
        private static Schedule GetSchedule( string value, RockContext rockContext )
        {
            var guid = value.AsGuidOrNull();
            if ( !guid.HasValue )
            {
                return null;
            }

            rockContext = rockContext ?? new RockContext();
            return new ScheduleService( rockContext ).Get( guid.Value );
        }

        /// <summary>
        /// Determines if the specified iCalendar content represents a real schedule that should be saved.
        /// </summary>
        /// <param name="iCalendarContent">The iCalendar content.</param>
        /// <returns><c>true</c> if the schedule is valid and complete; otherwise <c>false</c>.</returns>
        private static bool HasPersistableSchedule( string iCalendarContent )
        {
            if ( iCalendarContent.IsNullOrWhiteSpace() )
            {
                return false;
            }

            try
            {
                var schedule = new Schedule
                {
                    iCalendarContent = iCalendarContent
                };

                return schedule.HasSchedule() && !schedule.HasScheduleWarning();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Saves a schedule entity for the specified iCalendar content and returns its GUID.
        /// </summary>
        /// <param name="scheduleGuid">The schedule GUID to update, if one already exists.</param>
        /// <param name="iCalendarContent">The iCalendar content.</param>
        /// <returns>The schedule GUID if one was saved; otherwise an empty string.</returns>
        private static string SaveScheduleValue( Guid? scheduleGuid, string iCalendarContent )
        {
            if ( !HasPersistableSchedule( iCalendarContent ) )
            {
                return string.Empty;
            }

            using ( var rockContext = new RockContext() )
            {
                var scheduleService = new ScheduleService( rockContext );
                Schedule schedule = null;

                if ( scheduleGuid.HasValue )
                {
                    schedule = scheduleService.Get( scheduleGuid.Value );
                }

                if ( schedule == null )
                {
                    schedule = new Schedule();
                    scheduleService.Add( schedule );
                }

                schedule.iCalendarContent = iCalendarContent;
                rockContext.SaveChanges();

                return schedule.Guid.ToString();
            }
        }

        /// <summary>
        /// Gets the schedule GUID from the stored value.
        /// </summary>
        /// <param name="value">The stored value.</param>
        /// <returns>The schedule GUID if one exists; otherwise <c>null</c>.</returns>
        private static Guid? GetScheduleGuid( string value )
        {
            return value.AsGuidOrNull();
        }

        /// <summary>
        /// Payload used by Obsidian when editing the field.
        /// </summary>
        private sealed class ScheduleBuilderEditValueBag
        {
            /// <summary>
            /// Gets or sets the schedule GUID currently associated with the field value.
            /// </summary>
            public string ScheduleGuid { get; set; }

            /// <summary>
            /// Gets or sets the iCalendar content being edited.
            /// </summary>
            public string ICalendarContent { get; set; }
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <inheritdoc/>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return !condensed
                ? GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

        /// <inheritdoc/>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            return new ScheduleBuilderFieldEditControl
            {
                ID = id,
                DisplayScheduleFriendlyTextAfterLabel = true,
                ShowScheduleFriendlyTextAsToolTip = true,
                ShowClearButton = true
            };
        }

        /// <inheritdoc/>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control is ScheduleBuilderFieldEditControl editor )
            {
                var scheduleGuid = SaveScheduleValue( editor.ScheduleGuid, editor.iCalendarContent ).AsGuidOrNull();
                editor.ScheduleGuid = scheduleGuid;
                return scheduleGuid?.ToString() ?? string.Empty;
            }

            if ( control is ScheduleBuilder scheduleBuilder )
            {
                return SaveScheduleValue( null, scheduleBuilder.iCalendarContent );
            }

            return null;
        }

        /// <inheritdoc/>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var scheduleGuid = GetScheduleGuid( value );
            var schedule = GetSchedule( value, null );

            if ( control is ScheduleBuilderFieldEditControl editor )
            {
                editor.ScheduleGuid = scheduleGuid;
                editor.iCalendarContent = schedule?.iCalendarContent ?? string.Empty;
            }
            else if ( control is ScheduleBuilder scheduleBuilder )
            {
                scheduleBuilder.iCalendarContent = schedule?.iCalendarContent ?? string.Empty;
            }
        }

        /// <summary>
        /// WebForms editor that tracks the persisted schedule GUID while reusing the standard Schedule Builder control.
        /// </summary>
        internal sealed class ScheduleBuilderFieldEditControl : ScheduleBuilder
        {
            /// <summary>
            /// Gets or sets the current schedule GUID.
            /// </summary>
            internal Guid? ScheduleGuid
            {
                get => ( ViewState["ScheduleGuid"] as string ).AsGuidOrNull();
                set => ViewState["ScheduleGuid"] = value?.ToString();
            }
        }

        /// <summary>
        /// Gets the edit value as the IEntity.Id.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns>The schedule entity identifier if one is selected; otherwise <c>null</c>.</returns>
        public int? GetEditValueAsEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var guid = GetEditValue( control, configurationValues ).AsGuidOrNull();
            if ( !guid.HasValue )
            {
                return null;
            }

            var schedule = new ScheduleService( new RockContext() ).Get( guid.Value );
            return schedule?.Id;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            var schedule = new ScheduleService( new RockContext() ).Get( id ?? 0 );
            SetEditValue( control, configurationValues, schedule?.Guid.ToString() ?? string.Empty );
        }
#endif

        #endregion
    }
}
