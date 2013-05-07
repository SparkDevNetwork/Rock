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
using Rock.Communication;

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
        [MaxLength( 100 )]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the future send date time.
        /// </summary>
        /// <value>
        /// The future send date time.
        /// </value>
        [DataMember]
        public DateTime? FutureSendDateTime { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [DataMember]
        public CommunicationStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the reviewer person id.
        /// </summary>
        /// <value>
        /// The reviewer person id.
        /// </value>
        [DataMember]
        public int? ReviewerPersonId { get; set; }

        /// <summary>
        /// Gets or sets the reviewed date time.
        /// </summary>
        /// <value>
        /// The reviewed date time.
        /// </value>
        [DataMember]
        public DateTime? ReviewedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the reviewer note.
        /// </summary>
        /// <value>
        /// The reviewer note.
        /// </value>
        [DataMember]
        public string ReviewerNote { get; set; }

        /// <summary>
        /// Gets or sets the channel entity type id.
        /// </summary>
        /// <value>
        /// The channel entity type id.
        /// </value>
        [DataMember]
        public int? ChannelEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the channel data json.
        /// </summary>
        /// <value>
        /// The channel data json.
        /// </value>
        public string ChannelDataJson
        {
            get
            {
                return ChannelData.ToJson();
            }

            set
            {
                if ( string.IsNullOrWhiteSpace( value ) )
                {
                    ChannelData = new Dictionary<string, string>();
                }
                else
                {
                    ChannelData = JsonConvert.DeserializeObject<Dictionary<string, string>>( value );
                }
            }
        }

        /// <summary>
        /// Gets or sets any additional merge fields.  
        /// </summary>
        /// <value>
        /// The additional merge fields.
        /// </value>
        public string AdditionalMergeFieldsJson 
        {
            get
            {
                return AdditionalMergeFields.ToJson();
            }

            set
            {
                if ( string.IsNullOrWhiteSpace( value ) )
                {
                    AdditionalMergeFields = new List<string>();
                }
                else
                {
                    AdditionalMergeFields = JsonConvert.DeserializeObject<List<string>>( value );
                }
            }
        }

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
        /// Gets or sets the reviewer.
        /// </summary>
        /// <value>
        /// The reviewer.
        /// </value>
        [DataMember]
        public virtual Person Reviewer { get; set; }

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

        /// <summary>
        /// Gets or sets the communication channel.
        /// </summary>
        /// <value>
        /// The communication channel.
        /// </value>
        [DataMember]
        public virtual EntityType ChannelEntityType { get; set; }

        /// <summary>
        /// Gets the channel component.
        /// </summary>
        /// <value>
        /// The channel component.
        /// </value>
        public virtual ChannelComponent Channel
        {
            get
            {
                if ( this.ChannelEntityType != null || this.ChannelEntityTypeId.HasValue )
                {
                    foreach ( var serviceEntry in ChannelContainer.Instance.Components )
                    {
                        var component = serviceEntry.Value.Value;

                        if ( this.ChannelEntityTypeId.HasValue &&
                            this.ChannelEntityTypeId == component.EntityType.Id )
                        {
                            return component;
                        }

                        string componentName = component.GetType().FullName;
                        if ( this.ChannelEntityType != null &&
                            this.ChannelEntityType.Name == componentName)
                        {
                            return component;
                        }
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Gets or sets the data used by the selected communication channel.
        /// </summary>
        /// <value>
        /// The channel data.
        /// </value>
        [DataMember]
        public virtual Dictionary<string, string> ChannelData
        {
            get { return _channelData; }
            set { _channelData = value; }
        }
        private Dictionary<string, string> _channelData = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the additional merge field list. When a communication is created
        /// from a grid, the grid may add additional merge fields that will be available
        /// for the communication.
        /// </summary>
        /// <value>
        /// The additional merge field list.
        /// </value>
        [DataMember]
        public virtual List<string> AdditionalMergeFields
        {
            get { return _additionalMergeFields; }
            set { _additionalMergeFields = value; }
        }
        private List<string> _additionalMergeFields = new List<string>();

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets a channel data value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetChannelDataValue( string key )
        {
            if ( ChannelData.ContainsKey( key ) )
            {
                return ChannelData[key];
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Sets a channel data value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void SetChannelDataValue( string key, string value )
        {
            if ( ChannelData.ContainsKey( key ) )
            {
                ChannelData[key] = value;
            }
            else
            {
                ChannelData.Add( key, value );
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
            this.HasOptional( c => c.ChannelEntityType ).WithMany().HasForeignKey( c => c.ChannelEntityTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.Sender ).WithMany().HasForeignKey( c => c.SenderPersonId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.Reviewer ).WithMany().HasForeignKey( c => c.ReviewerPersonId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    /// <summary>
    /// The status of a communication
    /// </summary>
    public enum CommunicationStatus
    {
        /// <summary>
        /// Communication was created, but not yet edited by a user. (i.e. from data grid or report)
        /// Transient communications more than a few hours old may be deleted by clean-up job.
        /// </summary>
        Transient = 0,

        /// <summary>
        /// Communication is currently being drafted
        /// </summary>
        Draft = 1,

        /// <summary>
        /// Communication has been submitted but not yet approved or denied
        /// </summary>
        Submitted = 2,

        /// <summary>
        /// Communication has been approved for sending
        /// </summary>
        Approved = 3,

        /// <summary>
        /// Communication has been denied
        /// </summary>
        Denied = 4,

    }

}

