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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// ReminderType entity class.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "ReminderType" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.REMINDER_TYPE )]
    public partial class ReminderType : Model<ReminderType>, IOrdered
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this reminder type is active
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the notification type.
        /// </summary>
        /// <value>
        /// The notification type.
        /// </value>
        [DataMember]
        public ReminderNotificationType NotificationType { get; set; }

        /// <summary>
        /// Gets of sets the notification workflow type id.
        /// </summary>
        /// <value>
        /// The notification workflow type id.
        /// </value>
        [DataMember]
        public int? NotificationWorkflowTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this reminder type should show the note.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this reminder type should show the note.; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShouldShowNote { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the entity type identifier.
        /// </summary>
        /// <value>
        /// The entity type identifier.
        /// </value>
        [DataMember]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this reminder type should auto-complete when notified.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this reminder type should auto-complete when notified.; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShouldAutoCompleteWhenNotified { get; set; }

        /// <summary>
        /// Gets or sets the highlight color.
        /// </summary>
        /// <value>
        /// The highlight color.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string HighlightColor { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public virtual EntityType EntityType { get; set; }

        #endregion Navigation Properties

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance. Returns the name of the ReminderType
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion Public Methods
    }

    #region Entity Configuration

    /// <summary>
    /// Block Type Configuration class.
    /// </summary>
    public partial class ReminderTypeConfiguration : EntityTypeConfiguration<ReminderType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReminderTypeConfiguration"/> class.
        /// </summary>
        public ReminderTypeConfiguration()
        {
            this.HasRequired( b => b.EntityType ).WithMany().HasForeignKey( b => b.EntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}