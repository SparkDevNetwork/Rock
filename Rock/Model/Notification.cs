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

namespace Rock.Model
{
    /// <summary>
    /// Represents a notification
    /// </summary>
    [RockDomain( "Communication" )]
    [Table( "Notification" )]
    [DataContract]
    public partial class Notification : Model<Notification>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the title of the notification. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> that represents the notification title.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the sent date time.
        /// </summary>
        /// <value>
        /// The sent date time.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public DateTime SentDateTime { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the classification.
        /// </summary>
        /// <value>
        /// The classification.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public NotificationClassification Classification { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Notification"/> class.
        /// </summary>
        public Notification() : base()
        {
            _recipients = new Collection<NotificationRecipient>();
        }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the recipients.
        /// </summary>
        /// <value>
        /// The recipients.
        /// </value>
        public virtual ICollection<NotificationRecipient> Recipients
        {
            get { return _recipients; }
            set { _recipients = value; }
        }
        private ICollection<NotificationRecipient> _recipients;

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> containing the Notifications's Title that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the Notifications's Title that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Title;
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Communication Configuration class.
    /// </summary>
    public partial class NotificationConfiguration : EntityTypeConfiguration<Notification>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationConfiguration" /> class.
        /// </summary>
        public NotificationConfiguration()
        {
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// The classification of the notification (borrowed from Bootstrap)
    /// </summary>
    public enum NotificationClassification
    {
        /// <summary>
        /// The success
        /// </summary>
        Success,

        /// <summary>
        /// The information
        /// </summary>
        Info,

        /// <summary>
        /// The warning
        /// </summary>
        Warning,

        /// <summary>
        /// The danger
        /// </summary>
        Danger
    }

    #endregion
}
