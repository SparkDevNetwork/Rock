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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents an event item for one or more event calendars.
    /// </summary>
    [Table( "EventItem" )]
    [DataContract]
    public partial class EventItem : Model<EventItem>
    {
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
        public virtual ICollection<EventCalendarItem> EventCalendarItems
        {
            get { return _eventCalenderItems ?? ( _eventCalenderItems = new Collection<EventCalendarItem>() ); }
            set { _eventCalenderItems = value; }
        }

        private ICollection<EventCalendarItem> _eventCalenderItems;

        /// <summary>
        /// Gets or sets a collection of the <see cref="Rock.Model.EventItemCampus">EventItemCampuses</see> that belong to this EventItem.
        /// </summary>
        /// <value>
        /// A collection containing a collection of the <see cref="Rock.Model.EventItemCampus">EventItemCampuses</see> that belong to this EventItem.
        /// </value>
        [DataMember]
        public virtual ICollection<EventItemCampus> EventItemCampuses
        {
            get { return _eventItemCampuses ?? ( _eventItemCampuses = new Collection<EventItemCampus>() ); }
            set { _eventItemCampuses = value; }
        }

        private ICollection<EventItemCampus> _eventItemCampuses;

        /// <summary>
        /// Gets or sets a collection of the <see cref="Rock.Model.EventItemAudience">EventItemAudiences</see> that belong to this EventItem.
        /// </summary>
        /// <value>
        /// A collection containing a collection of the <see cref="Rock.Model.EventItemAudience">EventItemAudiences</see> that belong to this EventItem.
        /// </value>
        public virtual ICollection<EventItemAudience> EventItemAudiences
        {
            get { return _calendarItemAudiences ?? ( _calendarItemAudiences = new Collection<EventItemAudience>() ); }
            set { _calendarItemAudiences = value; }
        }

        private ICollection<EventItemAudience> _calendarItemAudiences;

        #endregion

        #region Virtual Properties

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
                return EventItemCampuses
                    .SelectMany( c => c.EventItemSchedules)
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

            foreach ( var eventItemCampus in EventItemCampuses )
            {
                result.AddRange( eventItemCampus.GetStartTimes( beginDateTime, endDateTime ) );
            }

            return result.Distinct().OrderBy( d => d ).ToList();
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