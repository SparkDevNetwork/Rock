//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
    /// Communication POCO Entity.
    /// </summary>
    [Table( "Communication" )]
    [DataContract]
    public partial class Communication : Model<Communication>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the sender person id.
        /// </summary>
        /// <value>
        /// The sender person id.
        /// </value>
        [DataMember]
        public int? SenderPersonId { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        [DataMember]
        public string Content { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the sender.
        /// </summary>
        /// <value>
        /// The sender.
        /// </value>
        [DataMember]
        public virtual Person Sender { get; set; }

        /// <summary>
        /// Gets or sets the recipients.
        /// </summary>
        /// <value>
        /// The recipients.
        /// </value>
        [DataMember]
        public virtual ICollection<CommunicationRecipient> Recipients 
        {
            get { return _recipients ?? ( _recipients = new Collection<CommunicationRecipient>() ); }
            set { _recipients = value; }
        }
        private ICollection<CommunicationRecipient> _recipients;

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Subject;
        }

        #endregion

        #region Private Methods

        #endregion

        #region Static Methods

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Communication Configuration class.
    /// </summary>
    public partial class CommunicationConfiguration : EntityTypeConfiguration<Communication>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationConfiguration"/> class.
        /// </summary>
        public CommunicationConfiguration()
        {
            this.HasOptional( c => c.Sender ).WithMany().HasForeignKey( c => c.SenderPersonId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}

