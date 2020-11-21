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
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
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
        /// Method that will be called on an entity immediately before the item is saved by context
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The database entity entry.</param>
        public override void PreSaveChanges( Data.DbContext dbContext, DbEntityEntry entry )
        {
            var rockContext = ( RockContext ) dbContext;

            switch ( entry.State )
            {
                case EntityState.Deleted:
                    {
                        // If a FinancialPaymentDetail was linked to this FinancialPersonSavedAccount and is now orphaned, delete it.
                        var financialPaymentDetailService = new FinancialPaymentDetailService( rockContext );
                        financialPaymentDetailService.DeleteOrphanedFinancialPaymentDetail( entry );

                        break;
                    }
            }

            base.PreSaveChanges( dbContext, entry );
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
            if ( this.GatewayPersonIdentifier.IsNotNullOrWhiteSpace() )
            {
                reference.GatewayPersonIdentifier = this.GatewayPersonIdentifier;
            }
            else
            {
                // if GatewayPersonIdentifier is unknown, this is probably from an older NMI gateway transaction that only saved the GatewayPersonIdentifier to ReferenceNumber
                reference.GatewayPersonIdentifier = this.ReferenceNumber;
            }

            if ( this.Id > 0 )
            {
                reference.FinancialPersonSavedAccountId = this.Id;
            }

            if ( this.FinancialPaymentDetail != null )
            {
                reference.MaskedAccountNumber = this.FinancialPaymentDetail.AccountNumberMasked;

                // if the ExpirationMonth and ExpirationYear are valid, set the reference.PaymentExpirationDate from that 
                if ( this.FinancialPaymentDetail.ExpirationMonth.HasValue && this.FinancialPaymentDetail.ExpirationYear.HasValue )
                {
                    if ( this.FinancialPaymentDetail.ExpirationMonth.Value >= 1 && this.FinancialPaymentDetail.ExpirationMonth.Value <= 12 )
                    {
                        reference.PaymentExpirationDate = new DateTime( this.FinancialPaymentDetail.ExpirationYear.Value, this.FinancialPaymentDetail.ExpirationMonth.Value, 1 );
                    }
                }

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

        #endregion Public Methods

        #region History

        /// <summary>
        /// This method is called in the
        /// <see cref="M:Rock.Data.Model`1.PreSaveChanges(Rock.Data.DbContext,System.Data.Entity.Infrastructure.DbEntityEntry,System.Data.Entity.EntityState)" />
        /// method. Use it to populate <see cref="P:Rock.Data.Model`1.HistoryItems" /> if needed.
        /// These history items are queued to be written into the database post save (so that they
        /// are only written if the save actually occurs).
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        /// <param name="state">The state.</param>
        protected override void BuildHistoryItems( Data.DbContext dbContext, DbEntityEntry entry, EntityState state )
        {
            // Sometimes, especially if the model is being deleted, some properties might not be
            // populated, but we can query to try to get their original value. We need to use a new
            // rock context to get the actual value from the DB
            var rockContext = new RockContext();
            var service = new FinancialPersonSavedAccountService( rockContext );
            var originalModel = service.Queryable( "PersonAlias, FinancialPaymentDetail" )
                .FirstOrDefault( fpsa => fpsa.Id == Id );

            // Use the original value for the person alias or the new value if that is not set
            var personId = ( originalModel?.PersonAlias ?? PersonAlias )?.PersonId;

            if ( !personId.HasValue )
            {
                // If this model is new, it won't have any virtual properties hydrated or an original
                // record in the database
                if ( PersonAliasId.HasValue )
                {
                    var personAliasService = new PersonAliasService( rockContext );
                    var personAlias = personAliasService.Get( PersonAliasId.Value );
                    personId = personAlias?.PersonId;
                }

                // We can't log history if we don't know who the saved account belongs to
                if ( !personId.HasValue )
                {
                    return;
                }
            }

            History.HistoryVerb verb;

            switch ( state )
            {
                case EntityState.Added:
                    verb = History.HistoryVerb.Add;
                    break;
                case EntityState.Deleted:
                    verb = History.HistoryVerb.Delete;
                    break;
                default:
                    // As of now, there is no requirement to log other events
                    return;
            }

            var historyChangeList = new History.HistoryChangeList();
            historyChangeList.AddChange( verb, History.HistoryChangeType.Record, "Financial Person Saved Account" );

            HistoryItems = HistoryService.GetChanges(
                typeof( Person ),
                Rock.SystemGuid.Category.HISTORY_PERSON.AsGuid(),
                personId.Value,
                historyChangeList,
                GetNameForHistory( originalModel?.FinancialPaymentDetail ?? FinancialPaymentDetail ),
                typeof( FinancialPersonSavedAccount ),
                Id,
                dbContext.GetCurrentPersonAlias()?.Id,
                dbContext.SourceOfChange );
        }

        /// <summary>
        /// Get the name of the saved account
        /// </summary>
        /// <param name="financialPaymentDetail">The financial payment detail.</param>
        /// <returns></returns>
        private string GetNameForHistory( FinancialPaymentDetail financialPaymentDetail )
        {
            var fpsaName = Name.IsNullOrWhiteSpace() ? "<Unnamed>" : Name.Trim();

            if ( financialPaymentDetail != null && !financialPaymentDetail.AccountNumberMasked.IsNullOrWhiteSpace() )
            {
                return string.Format( "{0} ({1})", fpsaName, financialPaymentDetail.AccountNumberMasked.Trim() );
            }

            return fpsaName;
        }

        #endregion History

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