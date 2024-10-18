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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Communication Recipient POCO Entity.
    /// </summary>
    [RockDomain( "Communication" )]
    [Table( "CommunicationRecipient" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "3EC89B90-6692-451E-A48F-0D2ADEBA05BC")]
    public partial class CommunicationRecipient : Model<CommunicationRecipient>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the PersonId of the <see cref="Rock.Model.Person"/> who is being sent the <see cref="Rock.Model.Communication"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PersonId of the <see cref="Rock.Model.Person"/> who is being sent the <see cref="Rock.Model.Communication"/>.
        /// </value>
        [DataMember]
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the CommunicationId of the <see cref="Rock.Model.Communication"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the CommunicationId of the <see cref="Rock.Model.Communication"/>.
        /// </value>
        [DataMember]
        public int CommunicationId { get; set; }


        /// <summary>
        /// Gets or sets the medium entity type identifier.
        /// </summary>
        /// <value>
        /// The medium entity type identifier.
        /// </value>
        [DataMember]
        public int? MediumEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the status of the Communication submission to the recipient.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.CommunicationRecipientStatus"/> Enum representing the status of <see cref="Rock.Model.Communication"/> submission to the recipient.
        /// This property will be  <c>CommunicationRecipientStatus.Pending</c> when Rock is waiting to send the <see cref="Rock.Model.Communication"/> to the recipient;
        /// <c>CommunicationRecipientStatus.Success</c> when Rock has successfully sent the <see cref="Rock.Model.Communication"/> to the recipient;
        /// <c>CommunicationRecipientStatus.Failed</c> when the attempt to send the <see cref="Rock.Model.Communication"/> failed.
        /// <c>CommunicationRecipientStatus.Cancelled</c> when the attempt to send the <see cref="Rock.Model.Communication"/> was canceled.
        /// </value>
        [DataMember]
        public CommunicationRecipientStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the status note.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the status note.
        /// </value>
        [DataMember]
        public string StatusNote { get; set; }

        /// <summary>
        /// Gets or sets the datetime that communication was sent.
        /// </summary>
        /// <value>
        /// The send date time.
        /// </value>
        [DataMember]
        public DateTime? SendDateTime { get; set; }

        /// <summary>
        /// Gets or sets the datetime that communication was opened by recipient.
        /// </summary>
        /// <value>
        /// The opened date time.
        /// </value>
        [DataMember]
        public DateTime? OpenedDateTime { get; set; }

        /// <summary>
        /// Gets or sets type of client that the recipient used to open the communication.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        [DataMember]
        [MaxLength( 200 )]
        public string OpenedClient { get; set; }

        /// <summary>
        /// Gets or sets the transport entity type identifier.
        /// </summary>
        /// <value>
        /// The transport identifier.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string TransportEntityTypeName { get; set; }

        /// <summary>
        /// Gets or sets the unique message identifier.
        /// </summary>
        /// <value>
        /// The unique message identifier.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string UniqueMessageId { get; set; }

        /// <summary>
        /// The response code from 100-99999 (excluding 666 and 911)
        /// with a prefix of '@'. For example, '@126345'
        /// Note: this numeric portion must be between 3 and 5 digits due
        /// to a regex that parses the message to find response codes
        /// </summary>
        /// <value>
        /// The response code.
        /// </value>
        [DataMember]
        [MaxLength( 6 )]
        public string ResponseCode { get; set; }

        /// <summary>
        /// Gets or sets the message as it was sent to the recipient (i.e. after lava merge).
        /// </summary>
        /// <value>
        /// The sent message.
        /// </value>
        [DataMember]
        public string SentMessage { get; set; }

        /// <summary>
        /// Gets or sets the personal device identifier.
        /// </summary>
        /// <value>
        /// The personal device identifier.
        /// </value>
        public int? PersonalDeviceId { get; set; }

        /// <summary>
        /// Gets or sets the AdditionalMergeValues as a Json string.
        /// </summary>
        /// <value>
        /// A Json formatted <see cref="System.String"/> containing the AdditionalMergeValues for the communication recipient. 
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
                AdditionalMergeValues = value.FromJsonOrNull<Dictionary<string, object>>() ?? new Dictionary<string, object>();

                // Convert any objects to a dictionary so that they can be used by Lava
                var objectKeys = AdditionalMergeValues
                    .Where( m => m.Value != null && m.Value.GetType() == typeof( JObject ) )
                    .Select( m => m.Key ).ToList();
                objectKeys.ForEach( k => AdditionalMergeValues[k] = ( ( JObject ) AdditionalMergeValues[k] ).ToDictionary() );

                // Convert any arrays to a list, and also check to see if it contains objects that need to be converted to a dictionary for Lava
                var arrayKeys = AdditionalMergeValues
                    .Where( m => m.Value != null && m.Value.GetType() == typeof( JArray ) )
                    .Select( m => m.Key ).ToList();
                arrayKeys.ForEach( k => AdditionalMergeValues[k] = ( ( JArray ) AdditionalMergeValues[k] ).ToObjectArray() );
            }
        }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Person"/> who is receiving the <see cref="Rock.Model.Communication"/>.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Person"/> who is receiving the <see cref="Rock.Model.Communication"/>.
        /// </value>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Communication"/>.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Communication"/>
        /// </value>
        [LavaVisible]
        public virtual Communication Communication { get; set; }

        /// <summary>
        /// Gets or sets the type of the medium entity.
        /// </summary>
        /// <value>
        /// The type of the medium entity.
        /// </value>
        [DataMember]
        public virtual EntityType MediumEntityType { get; set; }

        /// <summary>
        /// Gets or sets a dictionary containing the Additional Merge values for this communication
        /// </summary>
        /// <value>
        ///  A <see cref="System.Collections.Generic.Dictionary{String,String}"/> of <see cref="System.String"/> objects containing additional merge values for the <see cref="Rock.Model.Communication"/>
        /// </value>
        [DataMember]
        public virtual Dictionary<string, object> AdditionalMergeValues
        {
            get { return _additionalMergeValues; }
            set { _additionalMergeValues = value; }
        }
        private Dictionary<string, object> _additionalMergeValues = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the personal device.
        /// </summary>
        /// <value>
        /// The personal device.
        /// </value>
        public virtual PersonalDevice PersonalDevice { get; set; }

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
            this.HasOptional( r => r.PersonAlias ).WithMany().HasForeignKey( r => r.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.Communication ).WithMany( c => c.Recipients ).HasForeignKey( r => r.CommunicationId ).WillCascadeOnDelete( true );
            this.HasOptional( r => r.MediumEntityType ).WithMany().HasForeignKey( r => r.MediumEntityTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.PersonalDevice ).WithMany().HasForeignKey( r => r.PersonalDeviceId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}

