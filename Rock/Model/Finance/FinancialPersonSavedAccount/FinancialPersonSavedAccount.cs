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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Represents a bank or debit/credit card that a <see cref="Rock.Model.Person"/> ( or group ) has saved to Rock for 
    /// future reuse. Please note that account number is not actually stored here. The reference/profile number is stored 
    /// here as well as a masked version of the account number.  This saved account will either be associated to a person
    /// alias or a group. 
    /// </summary>
    [RockDomain( "Finance" )]
    [Table( "FinancialPersonSavedAccount" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "F5244E64-53DB-4707-A398-D248616A776D")]
    public partial class FinancialPersonSavedAccount : Model<FinancialPersonSavedAccount>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/> identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        [DataMember]
        public int? GroupId { get; set; }

        /// <summary>
        /// Gets or sets a reference identifier needed by the payment provider to use as a payment token.
        /// For gateways that have a concept of a customer vault (NMI and MyWell), this would be the customer vault id <see cref="GatewayPersonIdentifier"/>
        /// For gateways that use a source transaction for payment info (PayFlowPro), this would be the <see cref="TransactionCode" />
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> reference identifier needed by the payment provider to use as a payment token (customer vault id).
        /// </value>
        [DataMember]
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the name of the saved account. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the account.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the transaction code that was used as the "source transaction", and is used by some gateways (PayFlowPro) to lookup the payment info.
        /// For gateways that have the concept of a Customer Vault (NMI and MyWell), <see cref="GatewayPersonIdentifier" /> is what would be used.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the transaction code of the transaction.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string TransactionCode { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialGateway"/> identifier.
        /// </summary>
        /// <value>
        /// The gateway identifier.
        /// </value>
        [DataMember]
        public int? FinancialGatewayId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialPaymentDetail"/> identifier.
        /// </summary>
        /// <value>
        /// The financial payment detail identifier.
        /// </value>
        [DataMember]
        public int? FinancialPaymentDetailId { get; set; }

        /// <summary>
        /// Gets or sets the Gateway Person Identifier.
        /// This would indicate id the customer vault information on the gateway (for gateways that have customer vaults (NMI and MyWell) )
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Gateway Person Identifier of the account.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string GatewayPersonIdentifier { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this saved account was created by and is a part of the Rock core system/framework.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this saved account is part of the Rock core system/framework, otherwise is <c>false</c>.
        /// </value>
        /// <example>
        /// True
        /// </example>
        [DataMember]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this saved account is the default payment option for the given person.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this saved account is the default payment option for the given person, otherwise is <c>false</c>.
        /// </value>
        /// <example>
        /// True
        /// </example>
        [DataMember]
        public bool IsDefault { get; set; }

        /// <summary>
        /// Gets or sets the foreign currency code value identifier.
        /// </summary>
        /// <value>
        /// The foreign currency code value identifier.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE )]
        public int? PreferredForeignCurrencyCodeValueId { get; set; }

        /// <summary>
        /// Gets or sets the last error code received from the gateway when attempting to charge this account.
        /// </summary>
        /// <value>
        /// The last error code received from the gateway when attempting to charge this account.
        /// </value>
        [DataMember]
        [MaxLength( 200 )]
        public string LastErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the date/time the last error code was received.
        /// </summary>
        /// <value>
        /// The date/time the last error code was received.
        /// </value>
        [DataMember]
        public DateTime? LastErrorCodeDateTime { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/>.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [LavaVisible]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group"/>.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [LavaVisible]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialGateway"/>.
        /// </summary>
        /// <value>
        /// The gateway.
        /// </value>
        [DataMember]
        public virtual FinancialGateway FinancialGateway { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialPaymentDetail"/>.
        /// </summary>
        /// <value>
        /// The financial payment detail.
        /// </value>
        [DataMember]
        public virtual FinancialPaymentDetail FinancialPaymentDetail { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// FinancialPersonSavedAccount Configuration class.
    /// </summary>
    public partial class FinancialPersonSavedAccountConfiguration : EntityTypeConfiguration<FinancialPersonSavedAccount>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialPersonSavedAccountConfiguration"/> class.
        /// </summary>
        public FinancialPersonSavedAccountConfiguration()
        {
            this.HasOptional( t => t.PersonAlias ).WithMany().HasForeignKey( t => t.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.Group ).WithMany().HasForeignKey( t => t.GroupId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.FinancialGateway ).WithMany().HasForeignKey( t => t.FinancialGatewayId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.FinancialPaymentDetail ).WithMany().HasForeignKey( t => t.FinancialPaymentDetailId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}