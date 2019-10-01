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
    /// Recipient of a notification
    /// </summary>
    [RockDomain( "Communication" )]
    [Table( "NotificationRecipient" )]
    [DataContract]
    public partial class NotificationRecipient : Model<NotificationRecipient>
    {
        /// <summary>
        /// Gets or sets the notification identifier.
        /// </summary>
        /// <value>
        /// The notification identifier.
        /// </value>
        [DataMember]
        public int NotificationId { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="NotificationRecipient" /> is read.
        /// </summary>
        /// <value>
        ///   <c>true</c> if read; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool Read { get; set; }

        /// <summary>
        /// Gets or sets the read date time.
        /// </summary>
        /// <value>
        /// The read date time.
        /// </value>
        [DataMember]
        public DateTime? ReadDateTime { get; set; }

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Person" /> who is receiving the <see cref="Rock.Model.Communication" />.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Person" /> who is receiving the <see cref="Rock.Model.Communication" />.
        /// </value>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the notification.
        /// </summary>
        /// <value>
        /// The notification.
        /// </value>
        [DataMember]
        public virtual Notification Notification { get; set; }

        #endregion       
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class NotificationRecipientConfiguration : EntityTypeConfiguration<NotificationRecipient>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationRecipientConfiguration" /> class.
        /// </summary>
        public NotificationRecipientConfiguration()
        {
            this.HasRequired( r => r.PersonAlias ).WithMany().HasForeignKey( r => r.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.Notification ).WithMany( c => c.Recipients ).HasForeignKey( r => r.NotificationId ).WillCascadeOnDelete( true );
        }

    }

    #endregion
}
