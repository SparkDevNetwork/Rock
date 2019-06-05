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
using Rock.Financial;
using Rock.Web.Cache;

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
    public partial class FinancialPersonSavedAccount : Model<FinancialPersonSavedAccount>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the person alias identifier.
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
        /// Gets or sets a reference identifier needed by the payment provider to initiate a future transaction
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the reference identifier to initiate a future transaction.
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
        /// Gets or sets the transaction code for the transaction.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the transaction code of the transaction.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string TransactionCode { get; set; }

        /// <summary>
        /// Gets or sets the gateway identifier.
        /// </summary>
        /// <value>
        /// The gateway identifier.
        /// </value>
        [DataMember]
        public int? FinancialGatewayId { get; set; }

        /// <summary>
        /// Gets or sets the financial payment detail identifier.
        /// </summary>
        /// <value>
        /// The financial payment detail identifier.
        /// </value>
        [DataMember]
        public int? FinancialPaymentDetailId { get; set; }

        /// <summary>
        /// Gets or sets the Gateway Person Identifier.
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

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [LavaInclude]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [LavaInclude]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the gateway.
        /// </summary>
        /// <value>
        /// The gateway.
        /// </value>
        [DataMember]
        public virtual FinancialGateway FinancialGateway { get; set; }

        /// <summary>
        /// Gets or sets the financial payment detail.
        /// </summary>
        /// <value>
        /// The financial payment detail.
        /// </value>
        [DataMember]
        public virtual FinancialPaymentDetail FinancialPaymentDetail { get; set; }
        
        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this FinancialPersonSavedAccount.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this FinancialPersonSavedAccount.
        /// </returns>
        public override string ToString()
        {
            if ( this.FinancialPaymentDetail != null )
            {
                return this.FinancialPaymentDetail.AccountNumberMasked.ToStringSafe();
            }

            return TransactionCode;
        }

        /// <summary>
        /// Gets a reference payment info record.
        /// </summary>
        /// <returns></returns>
        public ReferencePaymentInfo GetReferencePayment()
        {
            var reference = new ReferencePaymentInfo();
            reference.TransactionCode = this.TransactionCode;
            reference.ReferenceNumber = this.ReferenceNumber;
            reference.GatewayPersonIdentifier = this.GatewayPersonIdentifier;

            if ( this.FinancialPaymentDetail != null )
            {
                reference.MaskedAccountNumber = this.FinancialPaymentDetail.AccountNumberMasked;
                if ( this.FinancialPaymentDetail.CurrencyTypeValueId.HasValue )
                {
                    reference.InitialCurrencyTypeValue = DefinedValueCache.Get( this.FinancialPaymentDetail.CurrencyTypeValueId.Value );
                    if ( reference.InitialCurrencyTypeValue != null &&
                        reference.InitialCurrencyTypeValue.Guid.Equals( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ) ) &&
                        this.FinancialPaymentDetail.CreditCardTypeValueId.HasValue )
                    {
                        reference.InitialCreditCardTypeValue = DefinedValueCache.Get( this.FinancialPaymentDetail.CreditCardTypeValueId.Value );
                    }
                }
            }

            return reference;
        }

        #endregion

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

    #endregion

}