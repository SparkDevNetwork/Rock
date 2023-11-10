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
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

using Rock.Data;
using Rock.Financial;
using Rock.Web.Cache;

#if REVIEW_NET5_0_OR_GREATER
using DbEntityEntry = Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry;
#endif

namespace Rock.Model
{
    /// <summary>
    /// FinancialPersonSavedAccount Logic
    /// </summary>
    public partial class FinancialPersonSavedAccount : Model<FinancialPersonSavedAccount>
    {
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

            reference.AmountCurrencyCodeValueId = this.PreferredForeignCurrencyCodeValueId;

            return reference;
        }

        #endregion Public Methods

        #region History Methods

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

        #endregion History Methods
    }
}