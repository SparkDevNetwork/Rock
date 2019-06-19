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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents an event item for one or more event calendars.
    /// </summary>
    [RockDomain( "Event" )]
    [Table( "EventItem" )]
    [DataContract]
    public partial class EventItem : Model<EventItem>, IHasActiveFlag
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Name of the EventItem. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the EventItem.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Summary of the EventItem.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the summary of the EventItem.
        /// </value>
        [DataMember]
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the Description of the EventItem.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the EventItem.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.BinaryFile"/> that contains the photo of the EventItem.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.BinaryFile"/> containing the photo of the EventItem.
        /// </value>
        [DataMember]
        public int? PhotoId { get; set; }

        /// <summary>
        /// Gets or sets the url for an external event.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the url for an external event.
        /// </value>
        [DataMember]
        [MaxLength(200)]
        public string DetailsUrl { get; set; }

        /// <summary>
        /// Gets or sets the is active.
        /// </summary>
        /// <value>
        /// The is active.
        /// </value>
        [DataMember]
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }
        private bool _isActive = true;

        /// <summary>
        /// Gets or sets a flag indicating if the prayer request has been approved. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this prayer request has been approved; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsApproved { get; set; }

        /// <summary>
        /// Gets or sets the PersonId of the <see cref="Rock.Model.Person"/> who approved this prayer request.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PersonId of the <see cref="Rock.Model.Person"/> who approved this prayer request.
        /// </value>
        [DataMember]
        public int? ApprovedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the date this prayer request was approved.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date that this prayer request was approved.
        /// </value>
        [DataMember]
        public DateTime? ApprovedOnDateTime { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.BinaryFile"/> that contains the EventItem's photo.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.BinaryFile"/> that contains the EventItem's photo.
        /// </value>
        [DataMember]
        public virtual BinaryFile Photo { get; set; }

        /// <summary>
        /// Gets or sets a collection of the <see cref="Rock.Model.EventCalendarItem">EventCalendarItems</see> that belong to this EventItem.
        /// </summary>
        /// <value>
        /// A collection containing a collection of the <see cref="Rock.Model.EventCalendarItem">EventCalendarItems</see> that belong to this EventItem.
        /// </value>
        [LavaInclude]
        public virtual ICollection<EventCalendarItem> EventCalendarItems
        {
            get { return _eventCalenderItems ?? ( _eventCalenderItems = new Collection<EventCalendarItem>() ); }
            set { _eventCalenderItems = value; }
        }

        private ICollection<EventCalendarItem> _eventCalenderItems;

        /// <summary>
        /// Gets or sets a collection of the <see cref="Rock.Model.EventItemOccurrence">EventItemOccurrence</see> that belong to this EventItem.
        /// </summary>
        /// <value>
        /// A collection containing a collection of the <see cref="Rock.Model.EventItemOccurrence">EventItemOccurrence</see> that belong to this EventItem.
        /// </value>
        [DataMember]
        public virtual ICollection<EventItemOccurrence> EventItemOccurrences
        {
            get { return _eventItemOccurrences ?? ( _eventItemOccurrences = new Collection<EventItemOccurrence>() ); }
            set { _eventItemOccurrences = value; }
        }

        private ICollection<EventItemOccurrence> _eventItemOccurrences;

        /// <summary>
        /// Gets or sets a collection of the <see cref="Rock.Model.EventItemAudience">EventItemAudiences</see> that belong to this EventItem.
        /// </summary>
        /// <value>
        /// A collection containing a collection of the <see cref="Rock.Model.EventItemAudience">EventItemAudiences</see> that belong to this EventItem.
        /// </value>
        [LavaInclude]
        public virtual ICollection<EventItemAudience> EventItemAudiences
        {
            get { return _calendarItemAudiences ?? ( _calendarItemAudiences = new Collection<EventItemAudience>() ); }
            set { _calendarItemAudiences = value; }
        }

        private ICollection<EventItemAudience> _calendarItemAudiences;

        /// <summary>
        /// Gets or sets the approved by person alias.
        /// </summary>
        /// <value>
        /// The approved by person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias ApprovedByPersonAlias { get; set; }

        /// <summary>
        /// Gets the next start date time.
        /// </summary>
        /// <value>
        /// The next start date time.
        /// </value>
        [NotMapped]
        public virtual DateTime? NextStartDateTime
        {
            get
            {
                return EventItemOccurrences
                    .Select( s => s.NextStartDateTime )
                    .DefaultIfEmpty()
                    .Min();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the start times.
        /// </summary>
        /// <param name="beginDateTime">The begin date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <returns></returns>
        public virtual List<DateTime> GetStartTimes ( DateTime beginDateTime, DateTime endDateTime )
        {
            var result = new List<DateTime>();

            foreach ( var eventItemOccurrence in EventItemOccurrences )
            {
                result.AddRange( eventItemOccurrence.GetStartTimes( beginDateTime, endDateTime ) );
            }

            return result.Distinct().OrderBy( d => d ).ToList();
        }

        /// <summary>
        /// Get a list of all inherited Attributes that should be applied to this entity.
        /// </summary>
        /// <returns>A list of all inherited AttributeCache objects.</returns>
        public override List<AttributeCache> GetInheritedAttributes( Rock.Data.RockContext rockContext )
        {
            var calendarIds = this.EventCalendarItems.Select( c => c.EventCalendarId ).ToList();
            if ( !calendarIds.Any() )
            {
                return null;
            }

            var inheritedAttributes = new Dictionary<int, List<AttributeCache>>();
            calendarIds.ForEach( c => inheritedAttributes.Add( c, new List<AttributeCache>() ) );

            //
            // Check for any calendar item attributes that the event item inherits.
            //
            var calendarItemEntityType = EntityTypeCache.Get( typeof( EventCalendarItem ) );
            if ( calendarItemEntityType != null )
            {
                foreach ( var calendarItemEntityAttributes in AttributeCache
                    .GetByEntity( calendarItemEntityType.Id )
                    .Where( a =>
                        a.EntityTypeQualifierColumn == "EventCalendarId" &&
                        calendarIds.Contains( a.EntityTypeQualifierValue.AsInteger() ) ) )
                {
                    foreach ( var attributeId in calendarItemEntityAttributes.AttributeIds )
                    {
                        inheritedAttributes[calendarItemEntityAttributes.EntityTypeQualifierValue.AsInteger()].Add(
                            AttributeCache.Get( attributeId ) );
                    }
                }
            }

            //
            // Walk the generated list of attribute groups and put them, ordered, into a list
            // of inherited attributes.
            //
            var attributes = new List<AttributeCache>();
            foreach ( var attributeGroup in inheritedAttributes )
            {
                foreach ( var attribute in attributeGroup.Value.OrderBy( a => a.Order ) )
                {
                    attributes.Add( attribute );
                }
            }

            return attributes;
        }

        /// <summary>
        /// Get any alternate Ids that should be used when loading attribute value for this entity.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <returns>
        /// A list of any alternate entity Ids that should be used when loading attribute values.
        /// </returns>
        public override List<int> GetAlternateEntityIds( RockContext rockContext )
        {
            //
            // Find all the calendar Ids this event item is present on.
            //
            return this.EventCalendarItems.Select( c => c.Id ).ToList();
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// EventItem Configuration class.
    /// </summary>
    public partial class EventItemConfiguration : EntityTypeConfiguration<EventItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventItemConfiguration" /> class.
        /// </summary>
        public EventItemConfiguration()
        {
            this.HasOptional( i => i.ApprovedByPersonAlias ).WithMany().HasForeignKey( i => i.ApprovedByPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.Photo ).WithMany().HasForeignKey( p => p.PhotoId ).WillCascadeOnDelete( false );
        }
    }

    #endregion 
}