using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rock.Data;

namespace Rock.Model
{
    [RockDomain( "Communication" )]
    [Table( "CommunicationResponse" )]
    [DataContract]
    public partial class CommunicationResponse : Model<CommunicationResponse>
    {
        #region Entity Properties
        /// <summary>
        /// This is the address of the sender communication medium. e.g. A phone number or email address.
        /// It is used when an incoming message cannot be identified with a person, this can be used to
        /// link it up later.
        /// </summary>
        /// <value>
        /// The message key.
        /// </value>
        [Required]
        [MaxLength( 1000 )]
        [DataMember( IsRequired = true )]
        public string MessageKey { get; set; }

        [DataMember]
        public int? FromPersonAliasId { get; set; }

        [DataMember]
        public int? ToPersonAliasId { get; set; }

        [DataMember]
        public bool IsRead { get; set; }

        [DataMember]
        public int? RelatedSmsFromDefinedValueId { get; set; }

        [DataMember]
        public int? RelatedCommunicationId { get; set; }

        [DataMember]
        public int RelatedTransportEntityTypeId { get; set; }

        [DataMember]
        public int RelatedMediumEntityTypeId { get; set; }

        [DataMember]
        public string Response { get; set; }

        #endregion Entity Properties

        #region Virtual Properties

        public virtual PersonAlias FromPersonAlias { get; set; }

        public virtual PersonAlias ToPersonAlias { get; set; }

        public virtual Communication RelatedCommunication { get; set; }

        public virtual EntityType ReleatedMedium { get; set; }

        public virtual EntityType RelatedTransport { get; set; }

        #endregion Virtual Properties

        #region Public Methods

        #endregion Public Methods

        #region Static Methods

        #endregion Static Methods

    }

    #region Entity Configuration
    public partial class CommunicationResponseConfiguration : EntityTypeConfiguration<CommunicationResponse>
    {
        public CommunicationResponseConfiguration()
        {
            this.HasOptional( r => r.FromPersonAlias ).WithMany().HasForeignKey( r => r.FromPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.ToPersonAlias ).WithMany().HasForeignKey( r => r.ToPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.RelatedCommunication ).WithMany().HasForeignKey( c => c.RelatedCommunicationId ).WillCascadeOnDelete( false );
            this.HasRequired( c => c.ReleatedMedium ).WithMany().HasForeignKey( c => c.RelatedMediumEntityTypeId ).WillCascadeOnDelete( false );
            this.HasRequired( c => c.RelatedTransport ).WithMany().HasForeignKey( c => c.RelatedTransportEntityTypeId ).WillCascadeOnDelete( false );

        }

    }
    #endregion Entity Configuration

}
