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
using Newtonsoft.Json;
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
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [DataMember]
        public CommunicationRecipientStatus Status { get; set; }

        [DataMember]
        public string StatusNote { get; set; }

        /// <summary>
        /// Gets or sets the additional merge values json.
        /// </summary>
        /// <value>
        /// The additional merge values json.
        /// </value>
        [DataMember]
        public string AdditionalMergeValuesJson
        {
            get
            {
                return AdditionalMergeValues.ToJson();
            }

            set
            {
                if ( string.IsNullOrWhiteSpace( value ) )
                {
                    AdditionalMergeValues = new Dictionary<string, string>();
                }
                else
                {
                    AdditionalMergeValues = JsonConvert.DeserializeObject<Dictionary<string, string>>( value );
                }
            }
        }

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

        /// <summary>
        /// Gets or sets the additional merge values.
        /// </summary>
        /// <value>
        /// The additional merge values.
        /// </value>
        [DataMember]
        public virtual Dictionary<string, string> AdditionalMergeValues
        {
            get { return _additionalMergeValues; }
            set { _additionalMergeValues = value; }
        }
        private Dictionary<string, string> _additionalMergeValues = new Dictionary<string, string>();


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

    /// <summary>
    /// The status of communication being sent to recipient
    /// </summary>
    public enum CommunicationRecipientStatus
    {
        /// <summary>
        /// Communication has not yet been sent to recipient
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Communication was successfully sent to recipient
        /// </summary>
        Success = 1,

        /// <summary>
        /// Communication failed to be sent to recipient
        /// </summary>
        Failed = 2,

        /// <summary>
        /// Communication was cancelled prior to sending to the recipient
        /// </summary>
        Cancelled = 3
    }
}

