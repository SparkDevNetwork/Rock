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
    /// Represents a communication in RockChMS (i.e. email, SMS message, etc.).
    /// </summary>
    [Table( "Communication" )]
    [DataContract]
    public partial class Communication : Model<Communication>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the PersonId of the <see cref="Rock.Model.Person"/> who is the sender of the Communication
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PersonId of the <see cref="Rock.Model.Person/> who is the sender of the Communcation.
        /// </value>
        [DataMember]
        public int? SenderPersonId { get; set; }

        /// <summary>
        /// Gets or sets the Subject of the Communication
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the Subject of the communication.
        /// </value>
        [MaxLength( 100 )]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the future send date for the communication. This allows a user to schedule when a communication is sent 
        /// and the communication will not be sent until that date and time.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> value that represents the FutureSendDate for the communication.  If no future send date is provided, this value will be null.
        /// </value>
        [DataMember]
        public DateTime? FutureSendDateTime { get; set; }

        /// <summary>
        /// Gets or sets the status of the Communication.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.CommunicationStatus"/> enum value that represents the status of the Communication.
        /// </value>
        [DataMember]
        public CommunicationStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the PersonId of the <see cref="Rock.Model.Person"/> who is the reviewer of the Communication.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32" /> representing the PersonId of the <see cref="Rock.Model.Person"/> who is the reviewer of the Communication. If there is not reviewer
        /// on this communication, this property will be null.
        /// </value>
        [DataMember]
        public int? ReviewerPersonId { get; set; }

        /// <summary>
        /// Gets or sets the date and time stamp of when the Communication was reviewed.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that the Communication was reviewed.
        /// </value>
        [DataMember]
        public DateTime? ReviewedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the note that was entered by the reviewer.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing a note that was entered by the reviewer.
        /// </value>
        [DataMember]
        public string ReviewerNote { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId of the <see cref="Rock.Model.EntityType"/> for the Communication Channel that is being used for this Communication.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> for the Communication Channel that is being used for this Communication. 
        /// </value>
        [DataMember]
        public int? ChannelEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets a Json formatted string containing the Channel specific data.
        /// </summary>
        /// <value>
        /// A Json formatted <see cref="System.String"/> that contains any Channel specific data.
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
        /// Gets or sets a Json string containing any additional merge fields for the Communication.
        /// </summary>
        /// <value>
        /// A Json formatted <see cref="System.String"/> that contains any additional merge fields for the Communication.
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
        /// Gets or sets the <see cref="Rock.Model.Person"/> of the Communication's sender.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.Person"/> that represents the Communication's sender.
        /// </value>
        [DataMember]
        public virtual Person Sender { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Person"/> of the Communication's reviewer.
        /// </summary>
        /// <value>
        /// The reviewer.
        /// </value>
        [DataMember]
        public virtual Person Reviewer { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.CommunicationRecipient">CommunicationRecipients</see> for the Communication.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.CommunicationRecipient">CommunicationRecipients</see> of the Communication.
        /// </value>
        [DataMember]
        public virtual ICollection<CommunicationRecipient> Recipients
        {
            get { return _recipients ?? ( _recipients = new Collection<CommunicationRecipient>() ); }
            set { _recipients = value; }
        }
        private ICollection<CommunicationRecipient> _recipients;

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> of the communications Channel that is being used by this Communication.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> of the communications Channel that is being used by this Communication.
        /// </value>
        [DataMember]
        public virtual EntityType ChannelEntityType { get; set; }

        /// <summary>
        /// Gets the <see cref="Rock.Communication.ChannelComponent"/> for the communication channel that is being used.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.ChannelComponent"/> for the communication channel that is being used.
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
        /// A <see cref="System.Collections.Generic.Dictionary(String,String)"/> of <see cref="Rock.Model.String"/> key value pairs that contain channel specific data.
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
        /// A <see cref="System.Collections.Generic.List(String)"/> of <see cref="Rock.Model.String"/> containing the additional merge field list.
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
        /// Returns a channel data value.
        /// </summary>
        /// <param name="key">A <see cref="System.String"/> containing the key associated with the value to retrieve. </param>
        /// <returns>A <see cref="System.String"/> representing the value that is linked with the specified key.</returns>
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
        /// Sets a channel data value. If the key exists, the value will be replaced with the new value, otherwise a new key value pair will be added to dictionary.
        /// </summary>
        /// <param name="key">A <see cref="System.String"/> representing the key.</param>
        /// <param name="value">A <see cref="System.String"/> representing the value.</param>
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

