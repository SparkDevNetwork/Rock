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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Finance.TransactionDetail;
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;


namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Displays the details of a particular financial transaction.
    /// </summary>

    [DisplayName( "Transaction Detail" )]
    [Category( "Finance" )]
    [Description( "Displays the details of a particular financial transaction." )]
    [IconCssClass( "ti ti-report-money" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Batch Detail Page",
        Description = "Page used to view batch.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.BatchDetailPage )]

    [LinkedPage( "Scheduled Transaction Detail Page",
        Description = "Page used to view scheduled transaction detail.",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.ScheduledTransactionDetailPage )]

    [LinkedPage( "Registration Detail Page",
        Description = "Page used to view an event registration.",
        IsRequired = false,
        Order = 2,
        Key = AttributeKey.RegistrationDetailPage )]

    [TextField( "Refund Batch Name Suffix",
        Description = "The text appended to the batch name when a refund is processed. To use the original batch name without a suffix, disable Append Suffix to Batch Name.",
        IsRequired = false,
        DefaultValue = " - Refund",
        Order = 3,
        Key = AttributeKey.RefundBatchNameSuffix )]

    [BooleanField( "Append Suffix to Batch Name",
        Description = "When enabled, appends a suffix to the batch name for refund transactions. When disabled, uses the original batch name. Note: financial gateways that support settlement batches ignore this setting—all transactions process through the settlement batch regardless.",
        DefaultBooleanValue = true,
        Order = 4,
        Key = AttributeKey.AppendSuffixToBatchName )]

    [DefinedValueField( "Location Types",
        Description = "The type of location type to display for person (if none are selected all addresses will be included ).",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.GROUP_LOCATION_TYPE,
        IsRequired = false,
        AllowMultiple = true,
        Order = 5,
        Key = AttributeKey.LocationTypes )]

    [BooleanField( "Transaction Source Required",
        Description = "Determine if Transaction Source should be required.",
        DefaultBooleanValue = false,
        Order = 6,
        Key = AttributeKey.TransactionSourceRequired )]

    [BooleanField( "Enable Foreign Currency",
        Description = "Shows the transaction's currency code field if enabled.",
        DefaultBooleanValue = false,
        Order = 7,
        Key = AttributeKey.EnableForeignCurrency )]

    [BooleanField( "Carry Over Account",
        Description = "Keep the last used account pre-populated when adding multiple transactions in the same session. Only applies when the saved transaction has exactly one account allocation.",
        DefaultBooleanValue = true,
        Order = 8,
        Key = AttributeKey.CarryOverAccount )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "31989b35-624b-4529-bc55-bb251e7fb1da" )]
    // WAS [Rock.SystemGuid.BlockTypeGuid( "f6f7a34c-c5d7-4cf9-a325-baf5300bce91" )]
    [Rock.SystemGuid.BlockTypeGuid( "1DE16F87-4A49-4A3C-A03E-B8488ECBEEBE" )]
    public class TransactionDetail : RockEntityDetailBlockType<FinancialTransaction, TransactionBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string TransactionId = "TransactionId";
            public const string BatchId = "BatchId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        private static class AttributeKey
        {
            public const string BatchDetailPage = "BatchDetailPage";
            public const string ScheduledTransactionDetailPage = "ScheduledTransactionDetailPage";
            public const string RegistrationDetailPage = "RegistrationDetailPage";
            public const string RefundBatchNameSuffix = "RefundBatchNameSuffix";
            public const string AppendSuffixToBatchName = "AppendSuffixToBatchName";
            public const string LocationTypes = "LocationTypes";
            public const string TransactionSourceRequired = "TransactionSourceRequired";
            public const string EnableForeignCurrency = "EnableForeignCurrency";
            public const string CarryOverAccount = "CarryOverAccount";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<TransactionBag, TransactionDetailOptionsBag>();

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( box.IsEditable );
            SetBoxInitialEntityState( box );
            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private TransactionDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new TransactionDetailOptionsBag();

            options.TransactionTypesGuid = SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE;

            options.TransactionSourceTypesGuid = SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE;

            options.RefundReasonTypesGuid = SystemGuid.DefinedType.FINANCIAL_TRANSACTION_REFUND_REASON;

            options.CurrencyTypesGuid = SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE;

            options.CurrencyCodesGuid = SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE;

            options.CreditCardTypesGuid = SystemGuid.DefinedType.FINANCIAL_CREDIT_CARD_TYPE;

            options.AssetTypes = SystemGuid.DefinedType.FINANCIAL_NONCASH_ASSET_TYPE;

            options.ShowForeignCurrencyFields = GetAttributeValue( AttributeKey.EnableForeignCurrency ).AsBoolean();

            options.TransactionSourceRequired = GetAttributeValue( AttributeKey.TransactionSourceRequired ).AsBoolean();

            options.BatchDetailPageUrl = this.GetLinkedPageUrl( AttributeKey.BatchDetailPage );

            options.CarryOverAccount = GetAttributeValue( AttributeKey.CarryOverAccount ).AsBoolean();

            var currencyInfo = new Rock.Utility.RockCurrencyCodeInfo();
            options.CurrencyInfo = new Rock.ViewModels.Utility.CurrencyInfoBag
            {
                Symbol = currencyInfo.Symbol,
                DecimalPlaces = currencyInfo.DecimalPlaces,
                SymbolLocation = currencyInfo.SymbolLocation
            };

            return options;
        }

        /// <summary>
        /// Validates the Transaction for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="financialTransaction">The Transaction to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the Transaction is valid, <c>false</c> otherwise.</returns>
        private bool ValidateTransaction( FinancialTransaction financialTransaction, out string errorMessage )
        {
            errorMessage = null;

            // Only a new transaction is required to have a batch. An existing transaction may
            // legitimately have no batch (e.g. a gateway transaction not yet downloaded into one),
            // so editing it must not be blocked. This matches the legacy new-transaction-only check.
            if ( financialTransaction.Id == 0
                && ( financialTransaction.BatchId == null || financialTransaction.BatchId == 0 ) )
            {
                errorMessage = "New transactions can only be added to an existing batch.";
                return false;
            }

            if ( financialTransaction.TransactionTypeValueId == 0 )
            {
                errorMessage = "Transaction type is required.";
                return false;
            }

            if ( financialTransaction.FinancialPaymentDetail == null )
            {
                errorMessage = "Payment detail is required.";
                return false;
            }

            var hasNonZeroAmount = financialTransaction.TransactionDetails?
                .Any( d => d.Amount != 0m || ( d.FeeCoverageAmount.HasValue && d.FeeCoverageAmount.Value != 0m ) ) ?? false;

            if ( !hasNonZeroAmount )
            {
                errorMessage = "A transaction must have at least one non-zero amount.";
                return false;
            }

            if ( financialTransaction.RefundDetails != null )
            {
                var totalAmount = financialTransaction.TransactionDetails?.Sum( d => d.Amount ) ?? 0m;

                if ( totalAmount > 0 )
                {
                    errorMessage = "A refund should have a negative amount. Please unselect the refund option, or change amounts to be negative values.";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether a transaction may be edited based on its batch status. A transaction
        /// cannot be edited when it belongs to a batch that is closed or automated. Transactions
        /// with no batch (e.g. a new transaction) are considered editable.
        /// </summary>
        /// <param name="entity">The transaction to evaluate.</param>
        /// <returns><c>true</c> if the transaction's batch permits editing; otherwise <c>false</c>.</returns>
        private static bool IsBatchEditAllowed( FinancialTransaction entity )
        {
            return entity?.Batch == null
                || ( entity.Batch.Status != BatchStatus.Closed && !entity.Batch.IsAutomated );
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<TransactionBag, TransactionDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {FinancialTransaction.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( FinancialTransaction.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( FinancialTransaction.FriendlyTypeName );
                }
            }

            // Check if allowed to edit based on batch status if user has permissions to edit.
            if ( box.IsEditable)
            {
                box.Options.CanEdit = IsBatchEditAllowed( entity );
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Builds the shared base <see cref="TransactionBag"/> from a loaded
        /// <see cref="FinancialTransaction"/> without view-only or edit-only fields.
        /// Both <see cref="GetEntityBagForView"/> and <see cref="GetEntityBagForEdit"/> call
        /// this method and then layer on mode-specific properties.
        /// </summary>
        /// <param name="transaction">The transaction to map; must not be <c>null</c>.</param>
        /// <returns>A populated <see cref="TransactionBag"/> containing common fields.</returns>
        private TransactionBag GetBaseTransactionBag( FinancialTransaction transaction )
        {
            var creditCardGuid = Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid();

            var totalAmount = transaction.TransactionDetails?.Sum( d => d.Amount - ( d.FeeCoverageAmount ?? 0 ) ) ?? 0m;
            var totalFeeAmount = transaction.TransactionDetails?.Sum( d => d.FeeAmount ) ?? 0m;
            var totalFeeCoverageAmount = transaction.TransactionDetails?.Sum( d => d.FeeCoverageAmount ) ?? 0m;

            var transactionDefinedValue = DefinedValueCache.Get( transaction.TransactionTypeValueId );

            var sourceDefinedValue = transaction.SourceTypeValueId.HasValue ? DefinedValueCache.Get( transaction.SourceTypeValueId.Value ) : null;

            var currencyDefinedValue = transaction.ForeignCurrencyCodeValueId.HasValue ? DefinedValueCache.Get( transaction.ForeignCurrencyCodeValueId.Value ) : null;

            var assetTypeDefinedValue = transaction.NonCashAssetTypeValueId.HasValue ? DefinedValueCache.Get( transaction.NonCashAssetTypeValueId.Value ) : null;

            var financialGatewayDefinedValue = transaction.FinancialGatewayId.HasValue ? new FinancialGatewayService( RockContext ).Get( transaction.FinancialGatewayId.Value ) : null;

            return new TransactionBag
            {
                Id = transaction.Id,
                BatchId = transaction.BatchId,
                BatchIdKey = transaction.Batch?.IdKey,
                ScheduledTransactionId = transaction.ScheduledTransactionId,
                AuthorizedPersonAliasId = transaction.AuthorizedPersonAliasId,
                ShowAsAnonymous = transaction.ShowAsAnonymous,
                SourceType = sourceDefinedValue.ToListItemBag(),
                TransactionType = transactionDefinedValue.ToListItemBag(),
                TransactionCode = transaction.TransactionCode,
                Summary = transaction.Summary,
                FinancialGateway = financialGatewayDefinedValue.ToListItemBag(),
                NonCashAssetType = assetTypeDefinedValue.ToListItemBag(),
                CurrencyCode = currencyDefinedValue.ToListItemBag(),
                TotalAmount = totalAmount,
                TotalFeeAmount = totalFeeAmount,
                TotalFeeCoverageAmount = totalFeeCoverageAmount,
                TransactionDateTime = transaction.TransactionDateTime,
                Batch = transaction.Batch == null
                    ? null
                    : new ListItemBag
                    {
                        Value = transaction.Batch.Name
                    },

                ProcessedByPersonAlias = transaction.ProcessedByPersonAlias?.Person == null
                    ? null
                    : new ListItemBag
                    {
                        Value = transaction.ProcessedByPersonAlias.Person.NickName + " " +
                                transaction.ProcessedByPersonAlias.Person.LastName
                    },
                Status = transaction.Status,
                StatusMessage = transaction.StatusMessage,
                ForeignKey = transaction.ForeignKey,
                PaymentDetail = GetPaymentDetailBag( transaction.FinancialPaymentDetail, creditCardGuid ),
                RefundDetails = GetRefundDetailBag( transaction.RefundDetails ),
                AuthorizedPerson = GetAuthorizedPersonBag( transaction.AuthorizedPersonAlias ),
                PersonOrBusiness = transaction.AuthorizedPersonAlias?.Person == null
                    ? null
                    : transaction.AuthorizedPersonAlias.ToListItemBag( transaction.AuthorizedPersonAlias.Person.FullName ),
                ScheduledTransaction = transaction.ScheduledTransaction != null
                    ? GetScheduledTransactionBag( transaction.ScheduledTransaction )
                    : null,
            };
        }

        /// <inheritdoc/>
        protected override TransactionBag GetEntityBagForView( FinancialTransaction entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetBaseTransactionBag( entity );

            if ( bag == null )
            {
                return null;
            }

            bag.IdKey = bag.Id?.AsIdKey();

            bag.TransactionDetails = GetTransactionLineItemBags( entity.Id, entity.TransactionDetails );
            bag.Images = GetImageBags( entity.Id );
            bag.Updates = GetTransactionUpdates( entity );
            bag.RelatedTransactions = GetRelatedTransactions( entity );
            bag.Refunds = GetRefundOverview( entity.Id );

            bag.Registrations = GetRegistrations( entity );

            bag.IsRefund = entity.RefundDetails != null;

            var foreignCurrencySymbol = string.Empty;

            bag.ForeignCurrencyDisplay = bag.CurrencyCode != null ? GetForeignCurrencyDisplay( bag.CurrencyCode.Value, out foreignCurrencySymbol ) : null;
            bag.ForeignCurrencySymbol = foreignCurrencySymbol;

            bag.CanRefund =
                entity != null
                && entity.Id > 0
                && entity.RefundDetails == null
                && IsOrganizationCurrency( entity.ForeignCurrencyCodeValueId )
                && entity.IsAuthorized( Authorization.REFUND, RequestContext.CurrentPerson );

            entity.LoadAttributes( RockContext );
            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: false );

            if ( entity.FinancialPaymentDetail != null && bag.PaymentDetail != null )
            {
                entity.FinancialPaymentDetail.LoadAttributes( RockContext );
                bag.PaymentDetail.Attributes = entity.FinancialPaymentDetail.GetPublicAttributesForView( RequestContext.CurrentPerson, enforceSecurity: true );
                bag.PaymentDetail.AttributeValues = entity.FinancialPaymentDetail.GetPublicAttributeValuesForView( RequestContext.CurrentPerson, enforceSecurity: true );
            }

            return bag;
        }

        /// <inheritdoc/>
        protected override TransactionBag GetEntityBagForEdit( FinancialTransaction entity )
        {
            if ( entity == null )
            {
                return null;
            }

            TransactionBag bag;

            if ( entity.Id == 0 )
            {
                // New transaction: resolve the target batch from the URL and leave all
                // other fields empty — the edit panel will apply its own defaults.
                var batchParameter = PageParameter( PageParameterKey.BatchId );
                var batchId = batchParameter.IsNullOrWhiteSpace() ? 0 : new FinancialBatchService( RockContext )
                    .GetQueryableByKey( batchParameter, !PageCache.Layout.Site.DisablePredictableIds )
                    .Select( b => b.Id )
                    .FirstOrDefault();

                bag = new TransactionBag
                {
                    BatchId = batchId,
                    BatchIdKey = batchId > 0 ? IdHasher.Instance.GetHash( batchId ) : null
                };
            }
            else
            {
                // Existing transaction: return the full bag so the edit panel does not
                // have to borrow data from the view bag.
                bag = GetBaseTransactionBag( entity );
                bag.IdKey = bag.Id?.AsIdKey();
                bag.TransactionDetails = GetTransactionLineItemBags( entity.Id, entity.TransactionDetails );
                bag.Images = GetImageBags( entity.Id );

                if ( entity.FinancialPaymentDetail != null && bag.PaymentDetail != null )
                {
                    entity.FinancialPaymentDetail.LoadAttributes( RockContext );
                    bag.PaymentDetail.Attributes = entity.FinancialPaymentDetail.GetPublicAttributesForEdit( RequestContext.CurrentPerson, enforceSecurity: false );
                    bag.PaymentDetail.AttributeValues = entity.FinancialPaymentDetail.GetPublicAttributeValuesForEdit( RequestContext.CurrentPerson, enforceSecurity: false );
                }
            }

            entity.LoadAttributes( RockContext );
            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: false );

            return bag;
        }

        /// <inheritdoc/>
        protected override FinancialTransaction GetInitialEntity()
        {
            var parameter = PageParameter( PageParameterKey.TransactionId );

            if ( parameter.IsNullOrWhiteSpace() || parameter == "0" )
            {
                return new FinancialTransaction
                {
                    FinancialPaymentDetail = new FinancialPaymentDetail()
                };
            }

            return new FinancialTransactionService( RockContext )
                .GetQueryableByKey( parameter, !PageCache.Layout.Site.DisablePredictableIds )
                .Include( t => t.Batch )
                .Include( t => t.FinancialPaymentDetail )
                .Include( t => t.RefundDetails )
                .Include( t => t.ProcessedByPersonAlias.Person )
                .Include( t => t.AuthorizedPersonAlias.Person )
                .Include( t => t.CreatedByPersonAlias.Person )
                .Include( t => t.ModifiedByPersonAlias.Person )
                .Include( t => t.ScheduledTransaction )
                .Include( t => t.TransactionDetails )
                .Include( t => t.TransactionDetails.Select( d => d.Account ) )
                .Include( t => t.TransactionDetails.Select( d => d.EntityType ) )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var parentPageParams = new Dictionary<string, string>();

            var batchId = PageParameter( PageParameterKey.BatchId );
            if ( batchId.IsNotNullOrWhiteSpace() )
            {
                parentPageParams.Add( PageParameterKey.BatchId, batchId );
            }

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl( parentPageParams )
            };
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out FinancialTransaction entity, out BlockActionResult error )
        {
            var entityService = new FinancialTransactionService( RockContext );
            error = null;

            // Determine if we are editing an existing entity or creating a new one.
            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                // Create a new entity.
                entity = new FinancialTransaction();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{FinancialTransaction.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit {FinancialTransaction.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Maps a <see cref="FinancialTransactionRefund"/> to a <see cref="RefundDetailBag"/>
        /// for display in the view panel.
        /// </summary>
        /// <param name="refundDetails">The refund entity; may be <c>null</c>.</param>
        /// <returns>A populated <see cref="RefundDetailBag"/>, or <c>null</c> if <paramref name="refundDetails"/> is <c>null</c>.</returns>
        private RefundDetailBag GetRefundDetailBag( FinancialTransactionRefund refundDetails )
        {
            if ( refundDetails == null )
            {
                return null;
            }

            var refundReasonDefinedValue = refundDetails.RefundReasonValueId.HasValue
                ? DefinedValueCache.Get( refundDetails.RefundReasonValueId.Value )
                : null;

            return new RefundDetailBag
            {
                OriginalTransactionId = refundDetails.OriginalTransactionId,
                OriginalTransactionIdKey = refundDetails.OriginalTransactionId.HasValue ? IdHasher.Instance.GetHash( refundDetails.OriginalTransactionId.Value ) : null,
                RefundReason = refundReasonDefinedValue.ToListItemBag(),
                RefundReasonSummary = refundDetails.RefundReasonSummary
            };
        }

        /// <summary>
        /// Returns a summary of every refund transaction linked to the given original transaction.
        /// </summary>
        /// <param name="originalTransactionId">The Id of the transaction that was refunded.</param>
        /// <returns>An ordered list of <see cref="RefundTransactionBag"/> items; empty when none exist.</returns>
        private List<RefundTransactionBag> GetRefundOverview( int originalTransactionId )
        {
            var refunds = new FinancialTransactionRefundService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( r => r.OriginalTransactionId == originalTransactionId && r.FinancialTransaction != null )
                .OrderBy( r => r.FinancialTransaction.TransactionDateTime )
                .Select( r => new RefundTransactionBag
                {
                    Id = r.FinancialTransaction.Id,
                    Date = r.FinancialTransaction.TransactionDateTime,
                    TransactionCode = r.FinancialTransaction.TransactionCode,
                    Reason = r.RefundReasonValue.Value,
                    Amount = r.FinancialTransaction.TransactionDetails.Sum( d => d.Amount )
                } )
                .ToList();

            refunds.TranslateIdToIdKey();

            return refunds;
        }

        /// <summary>
        /// Maps a <see cref="FinancialScheduledTransaction"/> to the lightweight
        /// <see cref="ScheduledTransactionBag"/> used for display and linking.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction; may be <c>null</c>.</param>
        /// <returns>A <see cref="ScheduledTransactionBag"/>, or <c>null</c> if <paramref name="scheduledTransaction"/> is <c>null</c>.</returns>
        private ScheduledTransactionBag GetScheduledTransactionBag( FinancialScheduledTransaction scheduledTransaction )
        {
            if ( scheduledTransaction == null )
            {
                return null;
            }

            return new ScheduledTransactionBag
            {
                Id = scheduledTransaction.Id,
                DisplayText = scheduledTransaction.GatewayScheduleId.IsNotNullOrWhiteSpace()
                    ? scheduledTransaction.GatewayScheduleId
                    : scheduledTransaction.Id.ToString(),
                Url = this.GetLinkedPageUrl( AttributeKey.ScheduledTransactionDetailPage, "ScheduledTransactionId", scheduledTransaction.IdKey )
            };
        }

        /// <summary>
        /// Converts a collection of <see cref="FinancialTransactionDetail"/> line items into a
        /// <see cref="TransactionDetailsBag"/> that includes row data and any public attributes
        /// defined on the detail entity type.
        /// </summary>
        /// <param name="transactionId">The Id of the parent transaction; used to load attribute definitions.</param>
        /// <param name="transactionDetails">The line-item collection; may be <c>null</c>.</param>
        /// <returns>A <see cref="TransactionDetailsBag"/> ready to be sent to the client.</returns>
        private TransactionDetailsBag GetTransactionLineItemBags( int transactionId, ICollection<FinancialTransactionDetail> transactionDetails )
        {
            // Load attributes on a temp instance to discover which attributes exist for
            // this entity type without needing a real saved record.
            var tempDetail = new FinancialTransactionDetail
            {
                TransactionId = transactionId
            };
            tempDetail.LoadAttributes( RockContext );

            var attributeCaches = tempDetail.Attributes.Values.Where( a => a.IsGridColumn ).ToList();
            var attributeFields = GetLineItemAttributeFields( attributeCaches );

            var detailList = ( transactionDetails ?? Enumerable.Empty<FinancialTransactionDetail>() ).ToList();
            detailList.LoadAttributes( RockContext );

            var rows = detailList
                .Select( d => new TransactionLineItemBag
                    {
                        Guid = d.Guid,
                        Id = d.Id,
                        Account = d.Account?.ToListItemBag( d.Account.Name ),
                        Amount = d.FeeCoverageAmount.HasValue
                            ? d.Amount - d.FeeCoverageAmount.Value
                            : d.Amount,
                        FeeAmount = d.FeeAmount,
                        FeeCoverageAmount = d.FeeCoverageAmount,
                        ForeignCurrencyAmount = d.ForeignCurrencyAmount,
                        Summary = d.Summary,
                        TransactionId = d.TransactionId,
                        EntityId = d.EntityId,
                        EntityTypeId = d.EntityTypeId,
                        EntityType = d.EntityType == null ? null : new ListItemBag
                        {
                            Text = d.EntityType.FriendlyName,
                            Value = d.EntityType.Name
                        },
                        CanEdit = true,
                        CanDelete = !d.EntityTypeId.HasValue,
                        IsTotalRow = false,
                        AttributeValues = d.GetPublicAttributeValuesForEdit( RequestContext.CurrentPerson, enforceSecurity: false ),
                        AttributeDisplayValues = GetLineItemAttributeDisplayValues( d, attributeCaches )
                    } )
                .ToList();

            return new TransactionDetailsBag
            {
                Rows = rows,
                AttributeFields = attributeFields
            };
        }

        /// <summary>
        /// Builds the <see cref="AttributeFieldDefinitionBag"/> list that describes which attribute
        /// columns the allocations grid should render. This is the column-definition counterpart to
        /// <see cref="GetLineItemAttributeDisplayValues"/>.
        /// </summary>
        /// <param name="attributes">The attributes defined on <see cref="FinancialTransactionDetail"/>.</param>
        /// <returns>
        /// A list of <see cref="AttributeFieldDefinitionBag"/> items, one per attribute, using
        /// the <c>attr_{key}</c> naming convention expected by the grid attribute column renderer.
        /// </returns>
        private static List<AttributeFieldDefinitionBag> GetLineItemAttributeFields( IEnumerable<AttributeCache> attributes )
        {
            var textFieldTypeGuid = SystemGuid.FieldType.TEXT.AsGuid();
            var fields = new List<AttributeFieldDefinitionBag>();

            foreach ( var attribute in attributes )
            {
                fields.Add( new AttributeFieldDefinitionBag
                {
                    Name = $"attr_{attribute.Key}",
                    Title = attribute.Name,
                    FieldTypeGuid = attribute.FieldType?.Guid ?? textFieldTypeGuid
                } );
            }

            return fields;
        }

        /// <summary>
        /// Builds a dictionary of condensed display values for a single line item's attributes,
        /// replicating the per-row data format produced by
        /// <see cref="Rock.Obsidian.UI.GridBuilderExtensions.AddAttributeFieldsFrom"/>.
        /// </summary>
        /// <param name="detail">
        /// The line-item entity with attributes already loaded via <c>LoadAttributes</c>.
        /// </param>
        /// <param name="attributes">The attributes whose values should be included.</param>
        /// <returns>
        /// A dictionary keyed by <c>attr_{attributeKey}</c>. Each value is an object with
        /// <c>Html</c> (condensed HTML, with booleans rendered as a check icon or empty string)
        /// and <c>Text</c> (plain-text equivalent) properties.
        /// </returns>
        private static Dictionary<string, object> GetLineItemAttributeDisplayValues( FinancialTransactionDetail detail, IEnumerable<AttributeCache> attributes )
        {
            var booleanFieldTypeGuid = SystemGuid.FieldType.BOOLEAN.AsGuid();
            var displayValues = new Dictionary<string, object>();

            foreach ( var attribute in attributes )
            {
                var key = attribute.Key;
                var field = attribute.FieldType?.Field;

                // have to look at raw value becasue the build in "Get Condensed HTML" looks at the saved value in the cache. Since this is a nested list, these will not be saved until
                // the entire transaction detail is saved.

                var rawValue = detail.GetAttributeValue( key );
                var textValue = field?.GetCondensedTextValue( rawValue, attribute.ConfigurationValues ) ?? string.Empty;
                var htmlValue = field?.GetCondensedHtmlValue( rawValue, attribute.ConfigurationValues ) ?? string.Empty;

                if ( attribute.FieldType?.Guid == booleanFieldTypeGuid )
                {
                    htmlValue = htmlValue == "Y"
                        ? "<i class=\"ti ti-check\"></i>"
                        : string.Empty;
                }

                displayValues[$"attr_{key}"] = new
                {
                    Html = htmlValue,
                    Text = textValue
                };
            }

            return displayValues;
        }

        /// <summary>
        /// Maps a <see cref="FinancialPaymentDetail"/> entity to a <see cref="PaymentDetailBag"/>
        /// for display in the view panel.
        /// </summary>
        /// <param name="paymentDetail">The payment detail entity; may be <c>null</c>.</param>
        /// <param name="creditCardGuid">The <see cref="Guid"/> of the credit-card currency defined value,
        /// used to determine whether to show credit-card-specific fields.</param>
        /// <returns>A <see cref="PaymentDetailBag"/>, or <c>null</c> if <paramref name="paymentDetail"/> is <c>null</c>.</returns>
        private PaymentDetailBag GetPaymentDetailBag( FinancialPaymentDetail paymentDetail, Guid creditCardGuid )
        {
            if ( paymentDetail == null )
            {
                return null;
            }

            var currencyType = paymentDetail.CurrencyTypeValueId.HasValue
                ? DefinedValueCache.Get( paymentDetail.CurrencyTypeValueId.Value )
                : null;

            var creditCardType = paymentDetail.CreditCardTypeValueId.HasValue
                ? DefinedValueCache.Get( paymentDetail.CreditCardTypeValueId.Value )
                : null;

            return new PaymentDetailBag
            {
                CurrencyType = currencyType.ToListItemBag(),
                CreditCardType = creditCardType.ToListItemBag(),
                NameOnCard = paymentDetail.NameOnCard,
                AccountNumberMasked = paymentDetail.AccountNumberMasked,
                ExpirationDate = paymentDetail.ExpirationDate,
                IsCreditCard = currencyType != null && currencyType.Guid == creditCardGuid
            };
        }

        /// <summary>
        /// Returns a human-readable foreign currency display string (e.g., "Euro €") and its
        /// symbol for the given currency defined-value Guid.
        /// </summary>
        /// <param name="currencyCodeGuid">The Guid string of the currency code defined value.</param>
        /// <param name="symbol">On return, contains the currency symbol (e.g., "€"), or an empty string.</param>
        /// <returns>A combined display string, or an empty string when the feature is disabled or the value is not found.</returns>
        private string GetForeignCurrencyDisplay( string currencyCodeGuid, out string symbol )
        {
            if ( !GetAttributeValue( AttributeKey.EnableForeignCurrency ).AsBoolean() )
            {
                symbol = string.Empty;
                return string.Empty;
            }
            symbol = string.Empty;

            if ( !currencyCodeGuid.IsNotNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            var definedValue = DefinedValueCache.Get( currencyCodeGuid );
            if ( definedValue == null )
            {
                return string.Empty;
            }

            symbol = definedValue.GetAttributeValue( "Symbol" );
            return $"{definedValue.Value} {symbol}".Trim();
        }

        /// <summary>
        /// Maps a <see cref="PersonAlias"/> to an <see cref="AuthorizedPersonBag"/> that provides
        /// display data for the authorized-by section of the view panel.
        /// </summary>
        /// <param name="personAlias">The authorized person alias; may be <c>null</c>.</param>
        /// <returns>An <see cref="AuthorizedPersonBag"/>, or <c>null</c> if <paramref name="personAlias"/> or its person is <c>null</c>.</returns>
        private AuthorizedPersonBag GetAuthorizedPersonBag( PersonAlias personAlias )
        {
            var person = personAlias?.Person;

            if ( person == null )
            {
                return null;
            }

            var addresses = GetAuthorizedPersonAddresses( person.Id );

            var authorizedPersonBag = new AuthorizedPersonBag
            {
                Guid = personAlias.Guid,
                Id = person.Id,
                Name = person.FullName,
                Email = person.Email,
                PhotoUrl = person.PhotoUrl,
                Addresses = addresses,
                Campus = person.PrimaryCampusId.HasValue
                    ? CampusCache.Get( person.PrimaryCampusId.Value )?.Name
                    : null
            };

            authorizedPersonBag.TranslateIdToIdKey();

            return authorizedPersonBag;
        }

        /// <summary>
        /// Queries the family group locations for a person and returns them as
        /// <see cref="AddressBag"/> items, filtered to the location types configured in
        /// block settings.
        /// </summary>
        /// <param name="personId">The Id of the person whose addresses to load.</param>
        /// <returns>A list of <see cref="AddressBag"/> items ordered by mailing, mapped, then newest first.</returns>
        private List<AddressBag> GetAuthorizedPersonAddresses( int personId )
        {
            var allowedLocationTypeIds = GetAttributeValue( AttributeKey.LocationTypes )
                .SplitDelimitedValues()
                .AsGuidList()
                .Select( g => DefinedValueCache.Get( g ) )
                .Where( dv => dv != null )
                .Select( dv => dv.Id )
                .ToList();

            var familyGroupTypeId = GroupTypeCache.GetFamilyGroupType()?.Id;

            var query = new GroupLocationService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( gl =>
                    gl.Location != null &&
                    gl.Location.Street1 != null &&
                    gl.Group.GroupTypeId == familyGroupTypeId &&
                    gl.Group.Members.Any( m => m.PersonId == personId ) );

            if ( allowedLocationTypeIds.Any() )
            {
                query = query.Where( gl =>
                    gl.GroupLocationTypeValueId.HasValue &&
                    allowedLocationTypeIds.Contains( gl.GroupLocationTypeValueId.Value ) );
            }

            var rawLocations = query
                .Include( gl => gl.Location )
                .Include( gl => gl.GroupLocationTypeValue )
                .OrderByDescending( gl => gl.IsMailingLocation )
                .ThenByDescending( gl => gl.IsMappedLocation )
                .ThenByDescending( gl => gl.CreatedDateTime )
                .ToList();

            return rawLocations
                .Select( gl => new AddressBag
                {
                    Type = gl.GroupLocationTypeValue?.Value ?? "",
                    FormattedAddress = gl.Location.FormattedAddress,
                    IsPrimary = gl.IsMailingLocation
                } )
                .ToList();
        }

        /// <summary>
        /// Builds the list of audit-style update strings (created by, processed by, modified by)
        /// shown in the view panel.
        /// </summary>
        /// <param name="transaction">The transaction whose audit persons and dates to format.</param>
        /// <returns>A list of HTML anchor strings ready to be rendered in the UI.</returns>
        private List<string> GetTransactionUpdates( FinancialTransaction transaction )
        {
            var updates = new List<string>();
            var rootUrl = ResolveRockUrlIncludeRoot( "/" );

            if ( transaction.CreatedByPersonAlias?.Person != null && transaction.CreatedDateTime.HasValue )
            {
                updates.Add( $"Created by {transaction.CreatedByPersonAlias.Person.GetAnchorTag( rootUrl )} on {transaction.CreatedDateTime.Value.ToShortDateString()} at {transaction.CreatedDateTime.Value.ToShortTimeString()}" );
            }

            if ( transaction.ProcessedByPersonAlias?.Person != null && transaction.ProcessedDateTime.HasValue )
            {
                updates.Add( $"Processed by {transaction.ProcessedByPersonAlias.Person.GetAnchorTag( rootUrl )} on {transaction.ProcessedDateTime.Value.ToShortDateString()} at {transaction.ProcessedDateTime.Value.ToShortTimeString()}" );
            }

            if ( transaction.ModifiedByPersonAlias?.Person != null && transaction.ModifiedDateTime.HasValue )
            {
                updates.Add( $"Modified by {transaction.ModifiedByPersonAlias.Person.GetAnchorTag( rootUrl )} on {transaction.ModifiedDateTime.Value.ToShortDateString()} at {transaction.ModifiedDateTime.Value.ToShortTimeString()}" );
            }

            return updates;
        }

        /// <summary>
        /// Finds any event registrations linked to this transaction's detail line items and
        /// returns them as <see cref="RegistrationLinkBag"/> items with display text and URLs.
        /// </summary>
        /// <param name="transaction">The transaction whose detail line items to inspect.</param>
        /// <returns>A list of <see cref="RegistrationLinkBag"/> items; empty when none are found.</returns>
        private List<RegistrationLinkBag> GetRegistrations( FinancialTransaction transaction )
        {
            var registrationEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Registration ) )?.Id;

            if ( registrationEntityTypeId == null )
            {
                return new List<RegistrationLinkBag>();
            }

            var registrationIds = transaction.TransactionDetails
                .Where( d => d.EntityTypeId == registrationEntityTypeId )
                .Select( d => d.EntityId )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .Distinct()
                .ToList();

            if ( !registrationIds.Any() )
            {
                return new List<RegistrationLinkBag>();
            }

            return new RegistrationService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( r =>
                    r.RegistrationInstance != null &&
                    r.RegistrationInstance.RegistrationTemplate != null &&
                    registrationIds.Contains( r.Id ) )
                .Select( r => new
                {
                    r.Id,
                    TemplateName = r.RegistrationInstance.RegistrationTemplate.Name,
                    InstanceName = r.RegistrationInstance.Name
                } )
                .ToList()
                .Select( r => new RegistrationLinkBag
                {
                    Id = r.Id,
                    Text = $"{r.TemplateName} - {r.InstanceName}",
                    Url = this.GetLinkedPageUrl( AttributeKey.RegistrationDetailPage, "RegistrationId", IdHasher.Instance.GetHash( r.Id ) )
                } )
                .ToList();
        }

        /// <summary>
        /// Resolves the rock URL and includes the original scheme and domain
        /// from the request.
        /// </summary>
        /// <param name="context">The context of the current request.</param>
        /// <param name="url">The URL to ben resolved.</param>
        /// <returns>A new string resolved to the proper domain.</returns>
        private string ResolveRockUrlIncludeRoot( string url )
        {
            var virtualPath = RequestContext.ResolveRockUrl( url );

            if ( !virtualPath.StartsWith( "/" ) )
            {
                return virtualPath;
            }

            if ( RequestContext.RootUrlPath.IsNotNullOrWhiteSpace() )
            {
                return $"{RequestContext.RootUrlPath}{virtualPath}";
            }

            return GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ) + virtualPath.RemoveLeadingForwardslash();
        }

        /// <summary>
        /// Finds other transactions that share the same gateway, transaction code, and authorized
        /// person alias as the given transaction. These are surfaced in the view panel as related transactions.
        /// </summary>
        /// <param name="transaction">The transaction to find related records for.</param>
        /// <returns>A list of <see cref="RelatedTransactionBag"/> items ordered by date; empty when the
        /// transaction lacks a gateway, transaction code, or authorized person.</returns>
        private List<RelatedTransactionBag> GetRelatedTransactions( FinancialTransaction transaction )
        {
            if ( transaction == null
                || !transaction.FinancialGatewayId.HasValue
                || transaction.TransactionCode.IsNullOrWhiteSpace()
                || !transaction.AuthorizedPersonAliasId.HasValue )
            {
                return new List<RelatedTransactionBag>();
            }

            var bags = new FinancialTransactionService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( t =>
                    t.FinancialGatewayId == transaction.FinancialGatewayId &&
                    t.TransactionCode == transaction.TransactionCode &&
                    t.AuthorizedPersonAliasId == transaction.AuthorizedPersonAliasId &&
                    t.Id != transaction.Id &&
                    t.TransactionDateTime.HasValue )
                .OrderBy( t => t.TransactionDateTime )
                .Select( t => new RelatedTransactionBag
                {
                    Id = t.Id,
                    TransactionDateTime = t.TransactionDateTime.Value,
                    TransactionReference = t.TransactionCode,
                    Amount = t.TransactionDetails.Select( d => ( decimal? ) d.Amount ).DefaultIfEmpty().Sum() ?? 0m
                } )
                .ToList();

            bags.TranslateIdToIdKey();

            return bags;
        }

        /// <summary>
        /// Loads the binary-file images attached to a transaction and resolves each one
        /// to a public image URL.
        /// </summary>
        /// <param name="transactionId">The Id of the transaction whose images to load.</param>
        /// <returns>A list of <see cref="TransactionImageBag"/> items ordered by display order; empty when none exist.</returns>
        private List<TransactionImageBag> GetImageBags( int transactionId )
        {
            var imageBag = new FinancialTransactionImageService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( i => i.TransactionId == transactionId && i.BinaryFile != null)
                .OrderBy( i => i.Order )
                .Select( ( i ) => new TransactionImageBag
                {
                    Guid = i.BinaryFile.Guid,
                    BinaryFileId = i.BinaryFileId,
                } )
                .ToList();

            if(imageBag.Any())
            {
                foreach(var image in imageBag)
                {
                    image.ImageUrl = FileUrlHelper.GetImageUrl( image.BinaryFileId );
                }
            }

            return imageBag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( FinancialTransaction entity, ValidPropertiesBox<TransactionBag> box )
        {
            if ( box?.Bag == null || box.ValidProperties == null )
            {
                return false;
            }

            // Match legacy behavior: always ensure payment detail exists before mapping fields.
            if ( entity.FinancialPaymentDetail == null )
            {
                entity.FinancialPaymentDetail = new FinancialPaymentDetail();
            }

            box.IfValidProperty( nameof( box.Bag.PersonOrBusiness ), () =>
            {
                var personAliasId = box.Bag.PersonOrBusiness != null
                    ? new PersonAliasService( RockContext ).GetId( box.Bag.PersonOrBusiness.Value.AsGuid() )
                    : null;

                if ( personAliasId.HasValue )
                {
                    entity.AuthorizedPersonAliasId = personAliasId;
                }
            } );

            if ( entity.Id == 0 && box.Bag.BatchId != null )
            {
                entity.BatchId = box.Bag.BatchId;
            }

            // Core FinancialTransaction fields.
            box.IfValidProperty( nameof( box.Bag.ShowAsAnonymous ),
                () => entity.ShowAsAnonymous = box.Bag.ShowAsAnonymous );

            box.IfValidProperty( nameof( box.Bag.TransactionDateTime ),
                () => entity.TransactionDateTime = box.Bag.TransactionDateTime );

            box.IfValidProperty( nameof( box.Bag.TransactionCode ),
                () => entity.TransactionCode = box.Bag.TransactionCode );

            box.IfValidProperty( nameof( box.Bag.Summary ),
                () => entity.Summary = box.Bag.Summary );

            box.IfValidProperty( nameof( box.Bag.SourceType ),
                () => entity.SourceTypeValueId = box.Bag.SourceType?.GetEntityId<DefinedValue>( RockContext ) );

            // New transactions can set gateway; existing ones should not change it.
            if ( entity.Id == 0 )
            {
                box.IfValidProperty( nameof( box.Bag.FinancialGateway ),
                    () => entity.FinancialGatewayId = box.Bag.FinancialGateway?.GetEntityId<FinancialGateway>( RockContext ) );
            }

            // Defined values coming from ListItemBag selections.
            box.IfValidProperty( nameof( box.Bag.TransactionType ),
                () => entity.TransactionTypeValueId = box.Bag.TransactionType?.GetEntityId<DefinedValue>( RockContext ) ?? 0 );

            box.IfValidProperty( nameof( box.Bag.NonCashAssetType ),
                () => entity.NonCashAssetTypeValueId = box.Bag.NonCashAssetType?.GetEntityId<DefinedValue>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.CurrencyCode ),
                () =>
                {
                    if ( GetAttributeValue( AttributeKey.EnableForeignCurrency ).AsBoolean() )
                    {
                        entity.ForeignCurrencyCodeValueId = box.Bag.CurrencyCode?.GetEntityId<DefinedValue>( RockContext );
                    }
                } );

            // Payment detail fields.
            box.IfValidProperty( nameof( box.Bag.PaymentDetail ),
                () =>
                {
                    var paymentBag = box.Bag.PaymentDetail;
                    if ( paymentBag == null )
                    {
                        return;
                    }

                    entity.FinancialPaymentDetail.CurrencyTypeValueId =
                        paymentBag.CurrencyType?.GetEntityId<DefinedValue>( RockContext );

                    var isCreditCard = paymentBag.CurrencyType?.Value == Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.ToLower();

                    entity.FinancialPaymentDetail.CreditCardTypeValueId = isCreditCard
                        ? paymentBag.CreditCardType?.GetEntityId<DefinedValue>( RockContext )
                        : null;

                    entity.FinancialPaymentDetail.NameOnCard = paymentBag.NameOnCard;

                    if ( paymentBag.AttributeValues != null )
                    {
                        entity.FinancialPaymentDetail.LoadAttributes( RockContext );
                        entity.FinancialPaymentDetail.SetPublicAttributeValues( paymentBag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: false );
                    }
                } );

            // non-cash asset only applies to Non-Cash transactions.
            var nonCashCurrencyType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_NONCASH );
            if ( nonCashCurrencyType == null
                || entity.FinancialPaymentDetail.CurrencyTypeValueId != nonCashCurrencyType.Id )
            {
                entity.NonCashAssetTypeValueId = null;
            }

            // Transaction attributes.
            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );
                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: false );
                } );

            box.IfValidProperty( nameof( box.Bag.TransactionDetails ),
                () =>
                {
                    SaveTransactionDetails( entity, box.Bag.TransactionDetails );
                } );

            if ( box.IsValidProperty( nameof( box.Bag.IsRefund ) )
                || box.IsValidProperty( nameof( box.Bag.RefundDetails ) ) )
            {
                ApplyRefundState( entity, box.Bag );
            }

            SaveTransactionImages( box.Bag, entity );

            return true;
        }

        /// <summary>
        /// Applies line-item changes from <paramref name="detailsBag"/> to the transaction's
        /// <see cref="FinancialTransactionDetail"/> collection, handling explicit deletes and
        /// add/update of remaining rows.
        /// </summary>
        /// <param name="entity">The transaction whose details are being modified.</param>
        /// <param name="detailsBag">The bag containing current rows and the Guids of rows to delete.</param>
        private void SaveTransactionDetails( FinancialTransaction entity, TransactionDetailsBag detailsBag )
        {
            if ( detailsBag == null )
            {
                return;
            }

            var txnDetailService = new FinancialTransactionDetailService( RockContext );

            var dbDetails = txnDetailService.Queryable()
                .Where( d => d.TransactionId == entity.Id )
                .ToList();

            // Explicit deletes only.
            foreach ( var guid in detailsBag.RowsToDelete.Distinct() )
            {
                var dbDetail = dbDetails.FirstOrDefault( d => d.Guid == guid );

                if ( dbDetail != null && !dbDetail.EntityTypeId.HasValue )
                {
                    txnDetailService.Delete( dbDetail );
                }
            }

            // Add/update remaining rows.
            foreach ( var row in detailsBag.Rows.Where( r => r != null && !r.IsTotalRow ) )
            {
                var detail = dbDetails.FirstOrDefault( d => d.Guid == row.Guid );

                var hasAmount = row.Amount != 0m || ( row.FeeCoverageAmount ?? 0m ) != 0m;
                if ( detail == null && !hasAmount )
                {
                    continue;
                }

                if ( detail == null )
                {
                    detail = new FinancialTransactionDetail
                    {
                        Guid = row.Guid != Guid.Empty ? row.Guid : Guid.NewGuid()
                    };

                    entity.TransactionDetails.Add( detail );
                    dbDetails.Add( detail );
                }

                detail.AccountId = row.Account?.GetEntityId<FinancialAccount>( RockContext ) ?? 0;
                detail.Amount = row.Amount + ( row.FeeCoverageAmount ?? 0 );
                detail.FeeAmount = row.FeeAmount;
                detail.FeeCoverageAmount = row.FeeCoverageAmount;
                detail.ForeignCurrencyAmount = row.ForeignCurrencyAmount;
                detail.Summary = row.Summary;

                if(row?.AttributeValues != null)
                {
                    detail.LoadAttributes( RockContext );
                    detail.SetPublicAttributeValues( row.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: false );
                }
            }
        }


        /// <summary>
        /// Persists image additions and deletions from the bag to the database.
        /// Each image is either maintained/added or removed based on its
        /// <see cref="TransactionImageBag.IsMarkedForDeletion"/> flag.
        /// </summary>
        /// <param name="bag">The transaction bag containing the image list from the client.</param>
        /// <param name="entity">The transaction entity whose images are being updated.</param>
        private void SaveTransactionImages(TransactionBag bag, FinancialTransaction entity )
        {
            var images = bag.Images;
            if ( images?.Any() == true )
            {
                var imageOrder = 0;
                foreach ( var image in images )
                {
                    var binaryFileId = new BinaryFileService( RockContext ).GetId( image.Guid );

                    if ( binaryFileId != null )
                    {
                        if ( image.IsMarkedForDeletion )
                        {
                            RemoveImageFromTransaction( binaryFileId.Value, entity.Id );
                        }
                        else
                        {
                            MaintainOrAddImage( binaryFileId.Value, entity.Id, imageOrder );
                            imageOrder++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Marks a binary file as temporary and removes the corresponding
        /// <see cref="FinancialTransactionImage"/> record so the image is no longer
        /// associated with the transaction.
        /// </summary>
        /// <param name="binaryFileId">The Id of the binary file to remove.</param>
        /// <param name="transactionId">The Id of the transaction the image is being removed from.</param>
        private void RemoveImageFromTransaction( int binaryFileId, int transactionId )
        {

            // First set the binary file to temporary so it is removed
            var binaryFileService = new BinaryFileService( RockContext );

            var imagePreview = binaryFileService.Get( binaryFileId );

            if( imagePreview != null )
            {
                imagePreview.IsTemporary = true;
            }

            // Remove the transaction reference so it does not get pulled in while it is temporarily in db

            var txnImageService = new FinancialTransactionImageService( RockContext );

            var txnImageToRemove = txnImageService.Queryable().Where( i => i.BinaryFileId == binaryFileId && i.TransactionId == transactionId ).FirstOrDefault();

            if( txnImageToRemove != null )
            {
                txnImageService.Delete( txnImageToRemove );
            }

        }

        /// <summary>
        /// Ensures a <see cref="FinancialTransactionImage"/> record exists for the given binary file
        /// and transaction, creating it and marking the file as permanent when it does not already exist.
        /// </summary>
        /// <param name="binaryFileId">The Id of the binary file to link.</param>
        /// <param name="transactionId">The Id of the transaction to link the image to.</param>
        /// <param name="order">The display order of this image among the transaction's images.</param>
        private void MaintainOrAddImage( int binaryFileId, int transactionId, int order )
        {
            var txnImageService = new FinancialTransactionImageService( RockContext );
            var binaryFileService = new BinaryFileService( RockContext );

            var txnImage = txnImageService.Queryable()
                .Where( i => i.BinaryFileId == binaryFileId && i.TransactionId == transactionId )
                .FirstOrDefault();

            if ( txnImage == null )
            {
                txnImage = new FinancialTransactionImage
                {
                    BinaryFileId = binaryFileId,
                    TransactionId = transactionId
                };
                txnImageService.Add( txnImage );

                // Mark the binary file as not temporary so it is persisted.
                var binaryFile = binaryFileService.Get( binaryFileId );
                if ( binaryFile != null )
                {
                    binaryFile.IsTemporary = false;
                }
            }

            txnImage.Order = order;
        }

        /// <summary>
        /// Applies or removes the refund state on <paramref name="entity"/> based on the
        /// <see cref="TransactionBag.IsRefund"/> flag and refund detail values from the bag.
        /// </summary>
        /// <param name="entity">The transaction whose refund details are being updated.</param>
        /// <param name="bag">The bag containing the client-submitted refund state and details.</param>
        private void ApplyRefundState( FinancialTransaction entity, TransactionBag bag )
        {
            var isRefund = bag.IsRefund;

            if ( isRefund )
            {
                if ( entity.RefundDetails == null )
                {
                    entity.RefundDetails = new FinancialTransactionRefund();
                }

                entity.RefundDetails.RefundReasonValueId =
                    bag.RefundDetails?.RefundReason?.GetEntityId<DefinedValue>( RockContext );

                entity.RefundDetails.RefundReasonSummary =
                    bag.RefundDetails?.RefundReasonSummary;
            }
            else if ( entity.RefundDetails != null )
            {
                new FinancialTransactionRefundService( RockContext ).Delete( entity.RefundDetails );
                entity.RefundDetails = null;
            }
        }

        /// <summary>
        /// Determines whether the given currency defined-value Id matches the organization's
        /// configured base currency, returning <c>true</c> when no foreign currency is set.
        /// </summary>
        /// <param name="foreignCurrencyValueId">The defined-value Id of the transaction's currency; <c>null</c> means organization currency.</param>
        /// <returns><c>true</c> if the transaction is in the organization's currency; otherwise <c>false</c>.</returns>
        private bool IsOrganizationCurrency( int? foreignCurrencyValueId )
        {
            // If no foreign currency is set, it's organization currency.
            if ( !foreignCurrencyValueId.HasValue )
            {
                return true;
            }

            // Get the organization's configured currency code (global attribute).
            var organizationCurrencyGuid = GlobalAttributesCache.Get()
                .GetValue( Rock.SystemKey.SystemSetting.ORGANIZATION_CURRENCY_CODE )
                .AsGuidOrNull();

            // If not configured, default to allowing.
            if ( !organizationCurrencyGuid.HasValue )
            {
                return true;
            }

            // Resolve the defined value for the organization currency.
            var organizationCurrencyDefinedValue = DefinedValueCache.Get( organizationCurrencyGuid.Value );

            if ( organizationCurrencyDefinedValue == null )
            {
                return true;
            }

            // Compare IDs
            return organizationCurrencyDefinedValue.Id == foreignCurrencyValueId.Value;
        }

        /// <summary>
        /// Calculates the maximum amount that can still be refunded for a transaction by
        /// summing the transaction's total amount with any existing refund or related
        /// transaction amounts posted against the same gateway and transaction code.
        /// </summary>
        /// <param name="transaction">The transaction for which to compute the refund ceiling.</param>
        /// <returns>The net remaining refundable amount; may be negative when over-refunded.</returns>
        private decimal GetMaxRefundAmountForTransaction( FinancialTransaction transaction )
        {
            var hasTransactionCode = !string.IsNullOrWhiteSpace( transaction.TransactionCode );

            var otherAmounts = new FinancialTransactionDetailService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( d =>
                    d.Transaction != null &&
                    (
                        (
                            transaction.FinancialGatewayId.HasValue &&
                            hasTransactionCode &&
                            d.Transaction.FinancialGatewayId.HasValue &&
                            d.Transaction.FinancialGatewayId.Value == transaction.FinancialGatewayId.Value &&
                            d.Transaction.TransactionCode == transaction.TransactionCode &&
                            d.TransactionId != transaction.Id
                        )
                        ||
                        (
                            d.Transaction.RefundDetails != null &&
                            d.Transaction.RefundDetails.OriginalTransactionId == transaction.Id
                        )
                    ) )
                .Select( d => d.Amount )
                .ToList()
                .Sum();

            return transaction.TotalAmount + otherAmounts;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<TransactionBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<TransactionBag> box )
        {
            var entityService = new FinancialTransactionService( RockContext );


            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Guard against edits to an existing transaction whose batch is closed or automated.
            // The UI hides the Edit button in this case; this enforces the same rule server-side.
            if ( !IsBatchEditAllowed( entity ) )
            {
                return ActionBadRequest( "This transaction cannot be edited because its batch is closed or automated." );
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // For a new transaction the target batch is only known after the bag has been applied.
            // Reject adding into a closed or automated batch (mirrors WebForms hiding the Add button).
            if ( entity.Id == 0 && entity.BatchId.HasValue )
            {
                var targetBatch = new FinancialBatchService( RockContext ).Get( entity.BatchId.Value );

                if ( targetBatch != null && ( targetBatch.Status == BatchStatus.Closed || targetBatch.IsAutomated ) )
                {
                    return ActionBadRequest( "A transaction cannot be added to a closed or automated batch." );
                }
            }

            // Ensure everything is valid before saving.
            if ( !ValidateTransaction( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
                entity.FinancialPaymentDetail?.SaveAttributeValues( RockContext );

                foreach ( var detail in entity.TransactionDetails )
                {
                    detail.SaveAttributeValues( RockContext );
                }
            } );

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.TransactionId] = entity.IdKey
                } ) );
            }

            entity = entityService.Queryable()
                .Include( t => t.TransactionDetails )
                .Include( t => t.TransactionDetails.Select( d => d.Account ) )
                .Include( t => t.TransactionDetails.Select( d => d.EntityType ) )
                .FirstOrDefault( t => t.Id == entity.Id );

            var bag = GetEntityBagForView( entity );

            return ActionOk( new ValidPropertiesBox<TransactionBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Returns the public attributes and default attribute values for a
        /// <see cref="FinancialTransactionDetail"/> line item, used to populate the
        /// attribute fields in the edit modal.
        /// </summary>
        /// <param name="guid">The Guid of an existing detail line item, or an empty string to get defaults.</param>
        /// <returns>A <see cref="TransactionDetailsBag"/> containing the attribute definitions and values.</returns>
        [BlockAction]
        public BlockActionResult GetTransactionDetailAttributes( string guid )
        {
            if ( !BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden();
            }

            FinancialTransactionDetail detailEntity;

            if( guid.IsNotNullOrWhiteSpace())
            {
                detailEntity = new FinancialTransactionDetailService( RockContext ).Get( guid.AsGuid() );
            }
            else
            {
                detailEntity = new FinancialTransactionDetail();
            }

            if ( detailEntity == null )
            {
                return ActionBadRequest( $"{FinancialTransactionDetail.FriendlyTypeName} not found." );
            }

            detailEntity.LoadAttributes( RockContext );

            var bag = new TransactionDetailsBag();

            bag.LoadAttributesAndValuesForPublicEdit( detailEntity, RequestContext.CurrentPerson, enforceSecurity: false );

            return ActionOk( bag );
        }

        /// <summary>
        /// Returns condensed HTML and text display values for the attributes on a single line item,
        /// keyed by <c>attr_{attributeKey}</c>. Called after the client saves a line-item edit so
        /// the allocations grid can show up-to-date attribute values without a full page reload.
        /// </summary>
        /// <param name="attributeValues">
        /// The public attribute values from the edited line item, keyed by attribute key.
        /// </param>
        /// <returns>
        /// A <see cref="Dictionary{TKey,TValue}"/> of <c>attr_{key}</c> to <c>{ Html, Text }</c> objects.
        /// </returns>
        [BlockAction]
        public BlockActionResult GetLineItemAttributeDisplay( Dictionary<string, string> attributeValues )
        {
            if ( !BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden();
            }

            var tempDetail = new FinancialTransactionDetail();
            tempDetail.LoadAttributes( RockContext );

            if ( attributeValues?.Count > 0 )
            {
                tempDetail.SetPublicAttributeValues( attributeValues, RequestContext.CurrentPerson, enforceSecurity: false );
            }

            var attributeCaches = tempDetail.Attributes.Values.Where( a => a.IsGridColumn ).ToList();
            return ActionOk( GetLineItemAttributeDisplayValues( tempDetail, attributeCaches ) );
        }

        /// <summary>
        /// Returns the maximum amount that can be refunded for the specified transaction.
        /// Used to pre-populate the refund amount field in the refund modal.
        /// </summary>
        /// <param name="key">The IdKey of the transaction to evaluate.</param>
        /// <returns>The refundable decimal amount.</returns>
        [BlockAction]
        public BlockActionResult GetMaxRefundAmount( string key )
        {
            if ( !BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden();
            }

            var transaction = new FinancialTransactionService( RockContext )
                .GetQueryableByKey( key, !this.PageCache.Layout.Site.DisablePredictableIds )
                .FirstOrDefault();

            if ( transaction == null )
            {
                return ActionBadRequest( $"{FinancialTransaction.FriendlyTypeName} not found." );
            }

            return ActionOk( GetMaxRefundAmountForTransaction( transaction ) );
        }


        /// <summary>
        /// Processes a refund for the specified transaction using the amount, reason, and
        /// processing options supplied by the client. Optionally runs the refund through
        /// the financial gateway when the transaction has a transaction code and gateway.
        /// </summary>
        /// <param name="key">The IdKey of the transaction to refund.</param>
        /// <param name="bag">The refund request containing amount, reason, and processing options.</param>
        /// <returns>The updated <see cref="TransactionBag"/> for the original transaction after the refund is applied.</returns>
        [BlockAction]
        public BlockActionResult Refund( string key, RefundRequestBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest( "Refund information is required." );
            }

            if ( bag.Amount <= 0 )
            {
                return ActionBadRequest( "Refund amount must be greater than zero." );
            }

            var transactionService = new FinancialTransactionService( RockContext );

            var transactionQry = transactionService.GetQueryableByKey( key, !this.PageCache.Layout.Site.DisablePredictableIds );

            var transaction = transactionQry
                .Include( t => t.Batch )
                .Include( t => t.FinancialGateway )
                .Include( t => t.FinancialPaymentDetail )
                .Include( t => t.RefundDetails )
                .Include( t => t.ProcessedByPersonAlias.Person )
                .Include( t => t.AuthorizedPersonAlias.Person )
                .FirstOrDefault();

            if ( transaction == null )
            {
                return ActionBadRequest( $"{FinancialTransaction.FriendlyTypeName} not found." );
            }

            if ( !transaction.IsAuthorized( Authorization.REFUND, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden();
            }

            var maximumRefundAmount = GetMaxRefundAmountForTransaction( transaction );

            if ( bag.Amount > maximumRefundAmount )
            {
                return ActionBadRequest( $"Refund amount cannot exceed {maximumRefundAmount.FormatAsCurrency()}." );
            }

            if ( transaction.RefundDetails != null )
            {
                return ActionBadRequest( "This transaction cannot be refunded." );
            }

            if ( !IsOrganizationCurrency( transaction.ForeignCurrencyCodeValueId ) )
            {
                return ActionBadRequest( "Refunds are not supported for transactions in foreign currencies." );
            }

            var canProcess = !string.IsNullOrWhiteSpace( transaction.TransactionCode ) && transaction.FinancialGateway != null;
            var process = canProcess && bag.Process;

            var refundReasonValueId = bag.RefundReason?.GetEntityId<DefinedValue>( RockContext );

            var appendSuffix = GetAttributeValue( AttributeKey.AppendSuffixToBatchName ).AsBoolean();
            var batchNameSuffix = appendSuffix ? GetAttributeValue( AttributeKey.RefundBatchNameSuffix ) : string.Empty;

            string errorMessage;
            var refundTransaction = transactionService.ProcessRefund(
                transaction,
                bag.Amount,
                refundReasonValueId,
                bag.RefundReasonSummary,
                process,
                batchNameSuffix,
                out errorMessage );

            if ( refundTransaction == null )
            {
                return ActionBadRequest( errorMessage ?? "Unable to process refund." );
            }

            RockContext.SaveChanges();

            var updatedEntity = new FinancialTransactionService( RockContext )
                .Queryable()
                .Include( t => t.Batch )
                .Include( t => t.FinancialPaymentDetail )
                .Include( t => t.RefundDetails )
                .Include( t => t.ProcessedByPersonAlias.Person )
                .Include( t => t.AuthorizedPersonAlias.Person )
                .Include( t => t.FinancialGateway )
                .Include( t => t.TransactionDetails )
                .Include( t => t.TransactionDetails.Select( d => d.Account ) )
                .Include( t => t.TransactionDetails.Select( d => d.EntityType ) )
                .FirstOrDefault( t => t.Id == transaction.Id );


            return ActionOk( GetEntityBagForView( updatedEntity ) );
        }

        /// <summary>
        /// Returns the integer Ids of the previous and next transactions within the same batch,
        /// enabling the Back/Next navigation buttons in the view panel footer.
        /// </summary>
        /// <param name="key">Not used; navigation is resolved from the current page parameters.</param>
        /// <returns>A <see cref="BatchNavigationBag"/> with the adjacent transaction Ids, or nulls when at the boundary.</returns>
        [BlockAction]
        public BlockActionResult GetBatchNavigation( string key )
        {
            if ( !BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden();
            }

            var transactionParameter = RequestContext.PageParameters.GetValueOrNull( PageParameterKey.TransactionId );
            var transactionId = transactionParameter.IsNullOrWhiteSpace() ? 0 : new FinancialTransactionService( RockContext )
                .GetQueryableByKey( transactionParameter, !PageCache.Layout.Site.DisablePredictableIds )
                .Select( t => t.Id )
                .FirstOrDefault();

            var batchParameter = RequestContext.PageParameters.GetValueOrNull( PageParameterKey.BatchId );
            var batchId = batchParameter.IsNullOrWhiteSpace() ? 0 : new FinancialBatchService( RockContext )
                .GetQueryableByKey( batchParameter, !PageCache.Layout.Site.DisablePredictableIds )
                .Select( b => b.Id )
                .FirstOrDefault();

            if ( batchId == 0 )
            {
                return ActionOk( new BatchNavigationBag() );
            }

            var batchTransactions = new FinancialTransactionService( RockContext )
                .Queryable()
                .Where( t => t.BatchId == batchId );

            var prevId = batchTransactions
                .Where( t => t.Id < transactionId )
                .Select( t => ( int? ) t.Id )
                .Max() ?? 0;

            var nextId = batchTransactions
                .Where( t => t.Id > transactionId )
                .Select( t => ( int? ) t.Id )
                .Min() ?? 0;

            return ActionOk( new BatchNavigationBag
            {
                PreviousTransactionIdKey = prevId == default( int ) ? null : IdHasher.Instance.GetHash( prevId ),
                NextTransactionIdKey = nextId == default( int ) ? null : IdHasher.Instance.GetHash( nextId )
            } );
        }

        #endregion Block Actions
    }
}
