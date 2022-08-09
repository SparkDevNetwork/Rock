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
using System.Runtime.Serialization;
using Rock.Data;
using Rock.UniversalSearch;
using Rock.Lava;
using Rock.Cms.ContentCollection.Attributes;

namespace Rock.Model
{
    /// <summary>
    /// Represents an event item for one or more event calendars.
    /// </summary>
    [RockDomain( "Event" )]
    [Table( "EventItem" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "6A58AD11-3491-84AE-4896-8F39906EA65E")]
    [ContentCollectionIndexable( typeof( Rock.Cms.ContentCollection.Indexers.EventItemIndexer ), typeof( Rock.Cms.ContentCollection.IndexDocuments.EventItemDocument ) )]
    public partial class EventItem : Model<EventItem>, IHasActiveFlag, IRockIndexable
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
        /// Gets or sets the URL for an external event.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the URL for an external event.
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
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets a flag indicating if the event has been approved.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this event has been approved; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsApproved { get; set; }

        /// <summary>
        /// Gets or sets the PersonId of the <see cref="Rock.Model.Person"/> who approved this event.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PersonId of the <see cref="Rock.Model.Person"/> who approved this event.
        /// </value>
        [DataMember]
        public int? ApprovedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the date this event was approved.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date that this event was approved.
        /// </value>
        [DataMember]
        public DateTime? ApprovedOnDateTime { get; set; }

        #endregion

        #region Navigation Properties

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
        [LavaVisible]
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
        [LavaVisible]
        public virtual ICollection<EventItemAudience> EventItemAudiences
        {
            get { return _calendarItemAudiences ?? ( _calendarItemAudiences = new Collection<EventItemAudience>() ); }
            set { _calendarItemAudiences = value; }
        }

        private ICollection<EventItemAudience> _calendarItemAudiences;

        /// <summary>
        /// Gets or sets the approved by <see cref="Rock.Model.PersonAlias"/>.
        /// </summary>
        /// <value>
        /// The approved by person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias ApprovedByPersonAlias { get; set; }

        /// <summary>
        /// Gets a value indicating whether [allows interactive bulk indexing].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allows interactive bulk indexing]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowsInteractiveBulkIndexing => true;

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