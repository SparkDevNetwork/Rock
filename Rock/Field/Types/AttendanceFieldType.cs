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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) Attendance
    /// Stored as Attendance.Guid
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    [Rock.SystemGuid.FieldTypeGuid( "45F2BE0A-43C2-40D6-9888-68A2E72ACD06")]
    public class AttendanceFieldType : FieldType, IEntityFieldType, IEntityReferenceFieldType
    {
        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var guid = GetAttendanceGuid( privateValue );

            if ( !guid.HasValue )
            {
                return privateValue;
            }

            using ( var rockContext = new RockContext() )
            {
                var attendance = GetAttendanceForDisplay( guid.Value, rockContext );

                return attendance?.ToString() ?? privateValue;
            }
        }

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

        #region Methods

        /// <summary>
        /// Gets the entity unique identifier for the entity specified by the value.
        /// </summary>
        /// <param name="privateValue">The private value.</param>
        /// <returns>The unique identifier of the entity or <c>null</c> if it could not be determined.</returns>
        private static Guid? GetAttendanceGuid( string privateValue )
        {
            var guid = privateValue.AsGuidOrNull();

            if ( guid.HasValue )
            {
                return guid;
            }

            var attendanceId = privateValue.AsIntegerOrNull();

            if ( attendanceId.HasValue )
            {
                // if an Id was specified instead of a Guid, get the Guid instead
                using ( var rockContext = new RockContext() )
                {
                    return new AttendanceService( rockContext ).GetGuid( attendanceId.Value );
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the attendance record from the database. This applies all the
        /// eager load settings so that the attendance record can be displayed.
        /// </summary>
        /// <param name="attendanceGuid">The attendance unique identifier.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>An <see cref="Attendance"/> entity or <c>null</c> if not found.</returns>
        private static Attendance GetAttendanceForDisplay( Guid attendanceGuid, RockContext rockContext )
        {
            return new AttendanceService( rockContext )
                .Queryable()
                .Include( a => a.Occurrence )
                .Include( a => a.Occurrence.Location )
                .Include( a => a.Occurrence.Group )
                .Include( a => a.PersonAlias )
                .Include( a => a.PersonAlias.Person )
                .FirstOrDefault( a => a.Guid == attendanceGuid );
        }

        #endregion

        #region Persistence

        /// <inheritdoc/>
        public override PersistedValues GetPersistedValues( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var guid = GetAttendanceGuid( privateValue );

            if ( guid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var attendance = GetAttendanceForDisplay( guid.Value, rockContext );

                    if ( attendance != null )
                    {
                        var textValue = attendance.ToString();

                        return new PersistedValues
                        {
                            TextValue = textValue,
                            CondensedTextValue = textValue.Truncate( 100 ),
                            HtmlValue = textValue,
                            CondensedHtmlValue = textValue.Truncate( 100 )
                        };
                    }
                }
            }

            return new PersistedValues
            {
                TextValue = privateValue,
                CondensedTextValue = privateValue,
                HtmlValue = privateValue,
                CondensedHtmlValue = privateValue
            };
        }

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var guid = GetAttendanceGuid( privateValue );

            if ( !guid.HasValue )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var attendance = new AttendanceService( rockContext )
                    .Queryable()
                    .Where( a => a.Guid == guid.Value )
                    .Select( a => new
                    {
                        a.Id,
                        a.PersonAliasId,
                        a.PersonAlias.PersonId,
                        a.Occurrence.GroupId,
                        a.Occurrence.LocationId
                    } )
                    .FirstOrDefault();

                if ( attendance == null )
                {
                    return null;
                }

                var entityReferences = new List<ReferencedEntity>();

                if ( attendance.PersonAliasId.HasValue )
                {
                    entityReferences.Add( new ReferencedEntity( EntityTypeCache.GetId<PersonAlias>().Value, attendance.PersonAliasId.Value ) );
                    entityReferences.Add( new ReferencedEntity( EntityTypeCache.GetId<Person>().Value, attendance.PersonId ) );
                }

                // The Group and Location values on an Attendance record don't
                // change so we don't need to monitor those, just the nested
                // properties.

                if ( attendance.GroupId.HasValue )
                {
                    entityReferences.Add( new ReferencedEntity( EntityTypeCache.GetId<Group>().Value, attendance.GroupId.Value ) );
                }

                if ( attendance.LocationId.HasValue )
                {
                    entityReferences.Add( new ReferencedEntity( EntityTypeCache.GetId<Location>().Value, attendance.LocationId.Value ) );
                }

                entityReferences.Add( new ReferencedEntity( EntityTypeCache.GetId<Rock.Model.Attendance>().Value, attendance.Id ) );

                return entityReferences;
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            var attendanceEntityTypeId = EntityTypeCache.GetId<Attendance>().Value;

            // Technically, more properties on Person are used such as the
            // suffix. But the extra overhead to monitor those is probably
            // not worth it since we will pick up the change eventually
            // on our nightly job runs.
            // Likewise, attendance records should normally only exist on
            // named locations so we don't monitor all the other properties.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( attendanceEntityTypeId, nameof( Attendance.DidAttend ) ),
                new ReferencedProperty( attendanceEntityTypeId, nameof( Attendance.ScheduledToAttend ) ),
                new ReferencedProperty( attendanceEntityTypeId, nameof( Attendance.RequestedToAttend ) ),
                new ReferencedProperty( attendanceEntityTypeId, nameof( Attendance.DeclineReasonValueId ) ),
                new ReferencedProperty( attendanceEntityTypeId, nameof( Attendance.StartDateTime ) ),
                new ReferencedProperty( attendanceEntityTypeId, nameof( Attendance.EndDateTime ) ),
                new ReferencedProperty( EntityTypeCache.GetId<PersonAlias>().Value, nameof( PersonAlias.PersonId ) ),
                new ReferencedProperty( EntityTypeCache.GetId<Person>().Value, nameof( Person.NickName ) ),
                new ReferencedProperty( EntityTypeCache.GetId<Person>().Value, nameof( Person.LastName ) ),
                new ReferencedProperty( EntityTypeCache.GetId<Group>().Value, nameof( Group.Name ) ),
                new ReferencedProperty( EntityTypeCache.GetId<Location>().Value, nameof( Location.Name ) )
            };
        }

        #endregion
    }
}