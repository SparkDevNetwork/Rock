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
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents an event item campus.
    /// </summary>
    [Table( "EventItemCampus" )]
    [DataContract]
    public partial class EventItemCampus : Model<EventItemCampus>
    {
        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.EventItem"/> that this EventItemCampus is associated with. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.EventItem"/> that the EventItemCampus is associated with.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EventItemId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Campus"/> that this EventItemCampus is associated with. when null it is a church-wide event.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Campus"/> that the EventItemCampus is associated with.
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
        /// Gets or sets the Id of the <see cref="Rock.Model.PersonAlias"/> for the EventItemCampus's contact person. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.PersonAlias"/> who is the EventItemCampus's contact person.
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
        [RegularExpression( @"[\w\.\'_%-]+(\+[\w-]*)?@([\w-]+\.)+[\w-]+", ErrorMessage = "The Email address is invalid" )]
        [Index( "IX_Email" )]
        public string ContactEmail { get; set; }

        /// <summary>
        /// Gets or sets the campus note.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the campus note.
        /// </value>
        [DataMember]
        public string CampusNote { get; set; }

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EventItem"/> that this EventItemCampus is a member of.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EventItem"/> that this EventItemCampus is a member of.
        /// </value>
        [DataMember]
        public virtual EventItem EventItem { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Campus"/> that this EventItemCampus is a member of.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Campus"/> that this EventItemCampus is a member of.
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
        /// Gets or sets the linkages.
        /// </summary>
        /// <value>
        /// The linkages.
        /// </value>
        [DataMember]
        public virtual ICollection<EventItemCampusGroupMap> Linkages
        {
            get { return _linkages ?? ( _linkages = new Collection<EventItemCampusGroupMap>() ); }
            set { _linkages = value; }
        }
        private ICollection<EventItemCampusGroupMap> _linkages;

        /// <summary>
        /// Gets or sets a collection of the <see cref="Rock.Model.EventItemSchedule">EventItemSchedules</see> that belong to this EventItem.
        /// </summary>
        /// <value>
        /// A collection containing a collection of the <see cref="Rock.Model.EventItemSchedule">EventItemSchedules</see> that belong to this EventItem.
        /// </value>
        [DataMember]
        public virtual ICollection<EventItemSchedule> EventItemSchedules
        {
            get { return _calendarItemSchedules ?? ( _calendarItemSchedules = new Collection<EventItemSchedule>() ); }
            set { _calendarItemSchedules = value; }
        }
        private ICollection<EventItemSchedule> _calendarItemSchedules;

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
                return EventItemSchedules
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
        public virtual List<DateTime> GetStartTimes( DateTime beginDateTime, DateTime endDateTime )
        {
            var result = new List<DateTime>();

            foreach( var eventItemSchedule in EventItemSchedules )
            {
                result.AddRange( eventItemSchedule.GetStartTimes( beginDateTime, endDateTime ) );
            }

            return result.Distinct().OrderBy( d => d ).ToList();
        }

        /// <summary>
        /// Gets the first start date time.
        /// </summary>
        /// <returns></returns>
        public virtual DateTime? GetFirstStartDateTime()
        {
            var results = new List<DateTime>();

            foreach ( var eventItemSchedule in EventItemSchedules )
            {

                var firstStartDate = eventItemSchedule.GetFirstStartDateTime();

                if ( firstStartDate.HasValue )
                {
                    results.Add( firstStartDate.Value );
                }
            }

            if ( results.Count() > 0 )
            {
                return results.OrderBy( d => d ).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( this.EventItem != null )
            {
                return string.Format( "{0} ({1})", this.EventItem.Name,
                    this.Campus != null ? this.Campus.Name : "All Campuses" );
            }

            return base.ToString();
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// EventItemCampus Configuration class.
    /// </summary>
    public partial class EventItemCampusConfiguration : EntityTypeConfiguration<EventItemCampus>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventItemCampusConfiguration" /> class.
        /// </summary>
        public EventItemCampusConfiguration()
        {
            this.HasRequired( p => p.EventItem ).WithMany( p => p.EventItemCampuses ).HasForeignKey( p => p.EventItemId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.Campus ).WithMany().HasForeignKey( p => p.CampusId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ContactPersonAlias ).WithMany().HasForeignKey( p => p.ContactPersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}