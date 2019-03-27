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
using System.Text;

using Newtonsoft.Json.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Communication Recipient POCO Entity.
    /// </summary>
    [RockDomain( "Communication" )]
    [Table( "CommunicationRecipient" )]
    [DataContract]
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
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the the CommunicationId of the <see cref="Rock.Model.Communication"/>.
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
        [MaxLength(200)]
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
        /// Gets or sets the response code.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the response code.
        /// </value>
        [DataMember]
        public string ResponseCode { get; set; }

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
                objectKeys.ForEach( k => AdditionalMergeValues[k] = ( (JObject)AdditionalMergeValues[k] ).ToDictionary() );

                // Convert any arrays to a list, and also check to see if it contains objects that need to be converted to a dictionary for Lava
                var arrayKeys = AdditionalMergeValues
                    .Where( m => m.Value != null && m.Value.GetType() == typeof( JArray ) )
                    .Select( m => m.Key ).ToList();
                arrayKeys.ForEach( k => AdditionalMergeValues[k] = ( (JArray)AdditionalMergeValues[k] ).ToObjectArray() );
            }
        }

        /// <summary>
        /// Gets or sets the message as it was sent to the recipient (i.e. after lava merge).
        /// </summary>
        /// <value>
        /// The sent message.
        /// </value>
        [DataMember]
        public string SentMessage { get; set; }
        #endregion

        #region Virtual Properties

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
        [LavaInclude]
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
        /// Gets a list of activities.
        /// </summary>
        /// <value>
        /// The activity list.
        /// </value>
        [NotMapped]
        public virtual string ActivityList
        {
            get
            {
                using ( var rockContext = new RockContext() )
                {
                    var interactions = this.GetInteractions( rockContext )
                       .OrderBy( a => a.InteractionDateTime )
                       .ToList();
                    StringBuilder sb = new StringBuilder();
                    foreach ( var interaction in interactions )
                    {
                        sb.AppendFormat( "{0} ({1} {2}): {3}<br/>",
                            interaction.Operation,
                            interaction.InteractionDateTime.ToShortDateString(),
                            interaction.InteractionDateTime.ToShortTimeString(),
                            GetInteractionDetails( interaction ) );
                    }

                    return sb.ToString();
                }
            }
        }

        /// <summary>
        /// Gets a list of activities.
        /// </summary>
        /// <value>
        /// The activity list.
        /// </value>
        [NotMapped]
        public virtual string ActivityListHtml
        {
            get
            {
                using ( var rockContext = new RockContext() )
                {
                    var interactions = this.GetInteractions( rockContext )
                        .OrderBy( a => a.InteractionDateTime )
                        .ToList();

                    StringBuilder sb = new StringBuilder();
                    sb.Append( "<ul>" );
                    foreach ( var interaction in interactions )
                    {
                        sb.AppendFormat( "<li>{0} <small>({1} {2})</small>: {3}</li>",
                            interaction.Operation,
                            interaction.InteractionDateTime.ToShortDateString(),
                            interaction.InteractionDateTime.ToShortTimeString(),
                            GetInteractionDetails( interaction ) );
                    }

                    sb.Append( "</ul>" );

                    return sb.ToString();
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the interactions (Opened and Click activity)
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public virtual IQueryable<Interaction> GetInteractions( RockContext rockContext )
        {
            var interactionService = new InteractionService( rockContext );
            var interactionChannelGuid = Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid();
            var result = interactionService.Queryable()
                .Where( a => a.InteractionComponent.Channel.Guid == interactionChannelGuid && a.InteractionComponentId == this.CommunicationId );
            return result;
        }

        /// <summary>
        /// Helper method to get recipient merge values for sending communication.
        /// </summary>
        /// <param name="globalConfigValues">The global configuration values.</param>
        /// <returns></returns>
        public Dictionary<string, object> CommunicationMergeValues( Dictionary<string, object> globalConfigValues )
        {
            Dictionary<string, object> mergeValues = new Dictionary<string, object>();

            globalConfigValues.ToList().ForEach( v => mergeValues.Add( v.Key, v.Value ) );

            if ( this.Communication != null )
            {
                mergeValues.Add( "Communication", this.Communication );
            }

            if ( this.PersonAlias != null && this.PersonAlias.Person != null )
            {
                mergeValues.Add( "Person", this.PersonAlias.Person );
            }

            // Add any additional merge fields created through a report
            foreach ( var mergeField in this.AdditionalMergeValues )
            {
                if ( !mergeValues.ContainsKey( mergeField.Key ) )
                {
                    mergeValues.Add( mergeField.Key, mergeField.Value );
                }
            }

            return mergeValues;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( this.PersonAlias != null && this.PersonAlias.Person != null )
            {
                return this.PersonAlias.Person.ToStringSafe();
            }
            else
            {
                return base.ToString();
            }
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets the interaction details.
        /// </summary>
        /// <returns></returns>
        public static string GetInteractionDetails( Interaction interaction )
        {
            string interactionDetails = string.Empty;
            string ipAddress = interaction?.InteractionSession?.IpAddress ?? "'unknown'";

            if ( interaction.Operation == "Opened" )
            {
                interactionDetails = $"Opened from {ipAddress}";
            }
            else if ( interaction.Operation == "Click" )
            {
                interactionDetails = $"Clicked the address {interaction?.InteractionData} from {ipAddress}";
            }
            else
            {
                interactionDetails = $"{interaction?.Operation}";
            }

            string deviceTypeDetails = $"{interaction?.InteractionSession?.DeviceType?.OperatingSystem} {interaction?.InteractionSession?.DeviceType?.DeviceTypeData} {interaction?.InteractionSession?.DeviceType?.Application} {interaction?.InteractionSession?.DeviceType?.ClientType}";
            if ( deviceTypeDetails.IsNotNullOrWhiteSpace() )
            {
                interactionDetails += $" using {deviceTypeDetails}";
            }

            return interactionDetails;
        }

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
            this.HasRequired( r => r.PersonAlias).WithMany().HasForeignKey( r => r.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.Communication ).WithMany( c => c.Recipients ).HasForeignKey( r => r.CommunicationId ).WillCascadeOnDelete( true );
            this.HasOptional( c => c.MediumEntityType ).WithMany().HasForeignKey( c => c.MediumEntityTypeId ).WillCascadeOnDelete( false );
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
        /// Communication was successfully delivered to recipient's mail server
        /// </summary>
        Delivered = 1,

        /// <summary>
        /// Communication failed to be sent to recipient
        /// </summary>
        Failed = 2,

        /// <summary>
        /// Communication was cancelled prior to sending to the recipient
        /// </summary>
        Cancelled = 3,

        /// <summary>
        /// Communication was sent and opened (viewed) by the recipient
        /// </summary>
        Opened = 4,

        /// <summary>
        /// Temporary status used while sending ( to prevent transaction and job sending same record )
        /// </summary>
        Sending = 5
    }
}

