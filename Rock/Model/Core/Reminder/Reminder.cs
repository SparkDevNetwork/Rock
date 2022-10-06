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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Reminder entity class.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "Reminder" )]
    [DataContract]
    [SystemGuid.EntityTypeGuid( SystemGuid.EntityType.REMINDER )]
    public partial class Reminder : Model<Reminder>
    {
        #region Properties

        /// <summary>
        /// Gets or sets the reminder type identifier.
        /// </summary>
        /// <value>
        /// The reminder type identifier.
        /// </value>
        [DataMember]
        public int ReminderTypeId { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        [DataMember]
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>
        /// The note.
        /// </value>
        [DataMember]
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the reminder date.
        /// </summary>
        /// <value>
        /// The reminder date.
        /// </value>
        [DataMember]
        public DateTime ReminderDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this reminder is complete.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is complete; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsComplete { get; set; }

        /// <summary>
        /// Gets or sets the renew period days.
        /// </summary>
        /// <value>
        /// The renew period days.
        /// </value>
        [DataMember]
        public int? RenewPeriodDays { get; set; }

        /// <summary>
        /// Gets or sets the renew max count.
        /// </summary>
        /// <value>
        /// The renew max count.
        /// </value>
        [DataMember]
        public int? RenewMaxCount { get; set; }

        /// <summary>
        /// Gets or sets renew current count.
        /// </summary>
        /// <value>
        /// The renew current count.
        /// </value>
        [DataMember]
        public int? RenewCurrentCount { get; set; }

        #endregion Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the type of the reminder.
        /// </summary>
        /// <value>
        /// The type of the reminder.
        /// </value>
        [DataMember]
        public virtual ReminderType ReminderType { get; set; }

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// Reminder Configuration class.
    /// </summary>
    public partial class ReminderConfiguration : EntityTypeConfiguration<Reminder>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReminderConfiguration"/> class.
        /// </summary>
        public ReminderConfiguration()
        {
            this.HasRequired( f => f.ReminderType ).WithMany().HasForeignKey( f => f.ReminderTypeId ).WillCascadeOnDelete( true );
            this.HasRequired( f => f.PersonAlias ).WithMany().HasForeignKey( f => f.PersonAliasId ).WillCascadeOnDelete( true );
        }
    }

    #endregion Entity Configuration
}