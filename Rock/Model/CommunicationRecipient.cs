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
    /// Communication Recipient POCO Entity.
    /// </summary>
    [Table( "CommunicationRecipient" )]
    [DataContract]
    public partial class CommunicationRecipient : Model<CommunicationRecipient>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the person id.
        /// </summary>
        /// <value>
        /// The person id.
        /// </value>
        [DataMember]
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the communication id.
        /// </summary>
        /// <value>
        /// The communication id.
        /// </value>
        [DataMember]
        public int CommunicationId { get; set; }

        /// <summary>
        /// Gets or sets the merge data.
        /// </summary>
        /// <value>
        /// The merge data.
        /// </value>
        [DataMember]
        public string MergeData { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        [DataMember]
        public virtual Person Person { get; set; }

        /// <summary>
        /// Gets or sets the communication.
        /// </summary>
        /// <value>
        /// The communication.
        /// </value>
        [DataMember]
        public virtual Communication Communication { get; set; }

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
            return this.Person.FullName;
        }

        #endregion

        #region Private Methods

        #endregion

        #region Static Methods

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Communication Recipient Configuration class.
    /// </summary>
    public partial class CommunicationRecipientConfiguration : EntityTypeConfiguration<CommunicationRecipient>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationRecipientConfiguration"/> class.
        /// </summary>
        public CommunicationRecipientConfiguration()
        {
            this.HasRequired( r => r.Person).WithMany().HasForeignKey( r => r.PersonId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.Communication ).WithMany( c => c.Recipients ).HasForeignKey( r => r.CommunicationId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

}

