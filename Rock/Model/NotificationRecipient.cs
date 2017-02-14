using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Recipient of a notification
    /// </summary>
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
