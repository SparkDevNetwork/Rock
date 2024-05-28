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
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Represents an event item occurrence.
    /// </summary>
    [RockDomain( "Event" )]
    [Table( "EventItemOccurrence" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "71632E1A-1E7F-42B9-A630-EC99F375303A")]
    public partial class EventItemOccurrence : Model<EventItemOccurrence>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EventItem"/> identifier.
        /// </summary>
        /// <value>
        /// The event item identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EventItemId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Campus"/> identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.CAMPUS )]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the Description of the Location.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the Location.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Schedule"/> identifier.
        /// </summary>
        /// <value>
        /// The schedule identifier.
        /// </value>
        [DataMember]
        public int? ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.PersonAlias"/> for the EventItemOccurrence's contact person. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.PersonAlias"/> who is the EventItemOccurrence's contact person.
        /// </value>
        [DataMember( IsRequired = true )]
        public int? ContactPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the Contact Person's phone number.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the Contact Person's phone number.
        /// </value>
        [MaxLength( 20 )]
        [DataMember]
        public String ContactPhone { get; set; }

        /// <summary>
        /// Gets or sets the Contact Person's email address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the Contact Person's email address.
        /// </value>
        [MaxLength( 75 )]
        [DataMember]
        [Previewable]
        [EmailAddressValidation]
        [Index( "IX_Email" )]
        public string ContactEmail { get; set; }

        /// <summary>
        /// Gets or sets the campus note.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the campus note.
        /// </value>
        [DataMember]
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the datetime for the next scheduled occurrence of this event. 
        /// </summary>
        /// <value>
        /// The datetime of the next occurrence.
        /// </value>
        [DataMember]
        public DateTime? NextStartDateTime { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EventItem"/> that this EventItemOccurrence is a member of.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EventItem"/> that this EventItemOccurrence is a member of.
        /// </value>
        [LavaVisible]
        public virtual EventItem EventItem { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Schedule"/>.
        /// </summary>
        /// <value>
        /// The schedule.
        /// </value>
        [DataMember]
        public virtual Schedule Schedule { get; set; }
        
        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Campus"/> that this EventItemOccurrence is a member of.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Campus"/> that this EventItemOccurrence is a member of.
        /// </value>
        [DataMember]
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/> representing the personalias who is the contact person.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.PersonAlias"/> representing the personalias who is the contact person.
        /// </value>
        [DataMember]
        public virtual PersonAlias ContactPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EventItemOccurrenceGroupMap">linkages</see>.
        /// </summary>
        /// <value>
        /// The linkages.
        /// </value>
        [DataMember]
        public virtual ICollection<EventItemOccurrenceGroupMap> Linkages
        {
            get { return _linkages ?? ( _linkages = new Collection<EventItemOccurrenceGroupMap>() ); }
            set { _linkages = value; }
        }
        private ICollection<EventItemOccurrenceGroupMap> _linkages;

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EventItemOccurrenceChannelItem">content channel items</see>.
        /// </summary>
        /// <value>
        /// The content channel items.
        /// </value>
        [DataMember]
        public virtual ICollection<EventItemOccurrenceChannelItem> ContentChannelItems
        {
            get { return _contentChannelItems ?? ( _contentChannelItems = new Collection<EventItemOccurrenceChannelItem>() ); }
            set { _contentChannelItems = value; }
        }
        private ICollection<EventItemOccurrenceChannelItem> _contentChannelItems;

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// EventItemOccurrence Configuration class.
    /// </summary>
    public partial class EventItemOccurrenceConfiguration : EntityTypeConfiguration<EventItemOccurrence>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventItemOccurrenceConfiguration" /> class.
        /// </summary>
        public EventItemOccurrenceConfiguration()
        {
            this.HasRequired( p => p.EventItem ).WithMany( p => p.EventItemOccurrences ).HasForeignKey( p => p.EventItemId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.Schedule ).WithMany().HasForeignKey( p => p.ScheduleId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.Campus ).WithMany().HasForeignKey( p => p.CampusId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ContactPersonAlias ).WithMany().HasForeignKey( p => p.ContactPersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}