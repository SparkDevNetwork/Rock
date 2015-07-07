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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Security;

namespace Rock.Model
{
    /// <summary>
    /// Represents an event calendar item.
    /// </summary>
    [Table( "EventCalendarItem" )]
    [DataContract]
    public partial class EventCalendarItem : Model<EventCalendarItem>, ISecured
    {

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.EventCalendar"/> that this EventCalendarItem belongs to. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.EventCalendar"/> that this EventCalendarItem is a member of.
        /// </value>
        [Required]
        [HideFromReporting]
        [DataMember( IsRequired = true )]
        public int EventCalendarId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.EventItem"/> that this EventCalendarItem belongs to. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.EventItem"/> that this EventCalendarItem is a member of.
        /// </value>
        [Required]
        [HideFromReporting]
        [DataMember( IsRequired = true )]
        public int EventItemId { get; set; }

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EventCalendar"/> that this EventCalendarItem is a member of.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EventCalendar"/> that this EventCalendarItem is a member of.
        /// </value>
        [DataMember]
        public virtual EventCalendar EventCalendar { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EventItem"/> that this EventCalendarItem is a member of.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EventItem"/> that this EventCalendarItem is a member of.
        /// </value>
        [DataMember]
        public virtual EventItem EventItem { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override ISecured ParentAuthority
        {
            get
            {
                return this.EventCalendar != null ? this.EventCalendar : base.ParentAuthority;
            }
        }
        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// EventCalendarItem Configuration class.
    /// </summary>
    public partial class EventCalendarItemConfiguration : EntityTypeConfiguration<EventCalendarItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventCalendarItemConfiguration" /> class.
        /// </summary>
        public EventCalendarItemConfiguration()
        {
            this.HasRequired( p => p.EventCalendar ).WithMany( p => p.EventCalendarItems ).HasForeignKey( p => p.EventCalendarId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.EventItem ).WithMany( p => p.EventCalendarItems ).HasForeignKey( p => p.EventItemId ).WillCascadeOnDelete( true );
        }
    }

    #endregion
}