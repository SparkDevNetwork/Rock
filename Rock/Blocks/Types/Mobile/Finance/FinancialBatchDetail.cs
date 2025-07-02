using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using AngleSharp.Dom;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Finance.FinancialBatchDetail;
using Rock.Common.Mobile.Blocks.Finance.FinancialBatchList;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks.Finance.FinancialBatchDetail;
using Rock.ViewModels.Blocks;
using Rock.Web.Cache;

using WebGrease.Css.Extensions;
using BatchStatus = Rock.Model.BatchStatus;
using Rock.Lava;
using OpenXmlPowerTools;
using CSScriptLibrary;

namespace Rock.Blocks.Types.Mobile.Finance
{
    /// <summary>
    /// The Rock Mobile Financial Batch Detail block, used to display
    /// </summary>
    [DisplayName( "Financial Batch Detail" )]
    [Category( "Mobile > Finance" )]
    [Description( "The Financial Batch Detail block." )]
    [IconCssClass( "fa fa-money-bills" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_FINANCE_FINANCIAL_BATCH_DETAIL_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_FINANCE_FINANCIAL_BATCH_DETAIL )]
    public class FinancialBatchDetail : RockBlockType
    {

        private const string AuthorizationReopenBatch = "ReopenBatch";

        #region Methods

        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out FinancialBatch entity, out BlockActionResult error )
        {
            var entityService = new FinancialBatchService( rockContext );
            error = null;

            // Determine if we are editing an existing entity or creating a new one.
            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityService.Get( idKey );
            }
            else
            {
                // Create a new entity.
                entity = new FinancialBatch();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{FinancialBatch.FriendlyTypeName} not found." );
                return false;
            }

            var isReopenDisabled = entity.Status == BatchStatus.Closed && !entity.IsAuthorized( AuthorizationReopenBatch, RequestContext.CurrentPerson );
            if ( entity.IsAutomated || !entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson ) || isReopenDisabled )
            {
                error = ActionBadRequest( $"Not authorized to edit {FinancialBatch.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Saves the financial batch.
        /// </summary>
        /// <param name="financialBatchBag"></param>
        /// <returns></returns>
        [BlockAction( "Save" )]
        public BlockActionResult Save( AddFinancialBatchBag financialBatchBag )
        {
            if ( financialBatchBag == null )
            {
                return ActionBadRequest( "Financial batch data is required." );
            }

            BlockActionResult error;

            var financialBatchService = new FinancialBatchService( RockContext );
            var idKey = financialBatchBag.IdKey;

            // Determine if we are editing or creating new batch.
            if ( !TryGetEntityForEditAction( idKey, RockContext, out var batch, out var actionError ) )
            {
                return actionError;
            }

            if ( batch.Status == BatchStatus.Closed && financialBatchBag.Status.ConvertToEnum<BatchStatus>() != BatchStatus.Closed )
            {
                if ( !batch.IsAuthorized( AuthorizationReopenBatch, RequestContext.CurrentPerson ) )
                {
                    return ActionUnauthorized( "User is not authorized to reopen a closed batch" );
                }
            }

            var isNew = batch.Id == 0;
            var isStatusChanged = financialBatchBag.Status.ConvertToEnum<BatchStatus>() != batch.Status;

            var changes = new History.HistoryChangeList();
            if ( isNew )
            {
                changes.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Batch" );
            }


            var currentCampusName = CampusCache.Get( batch.CampusId ?? 0 )?.Name ?? "None";
            var newCampus = CampusCache.Get( financialBatchBag.Campus );

            History.EvaluateChange( changes, "Batch Name", batch.Name, financialBatchBag.Name );
            History.EvaluateChange( changes, "Campus", currentCampusName, newCampus?.Name ?? "None" );
            History.EvaluateChange( changes, "Status", batch?.Status, financialBatchBag.Status.ConvertToEnum<BatchStatus>() );
            History.EvaluateChange( changes, "Start Date/Time", batch.BatchStartDateTime, financialBatchBag.BatchStartDate );
            History.EvaluateChange( changes, "End Date/Time", batch.BatchEndDateTime, financialBatchBag.BatchEndDate );
            History.EvaluateChange( changes, "Control Amount", batch?.ControlAmount.FormatAsCurrency(), ( financialBatchBag.ControlAmount ?? 0.0m ).FormatAsCurrency() );
            History.EvaluateChange( changes, "Control Item Count", batch.ControlItemCount, financialBatchBag.ControlItemCount );
            //History.EvaluateChange( changes, "Accounting System Code", batch.AccountingSystemCode, financialBatchBag.AccountingSystemCode );
            History.EvaluateChange( changes, "Notes", batch.Note, financialBatchBag.Note );

            // Replicating the behavior in the Webforms block where the batch end date is set
            // to the next day after the start date if not provided.
            batch.BatchEndDateTime = financialBatchBag.BatchEndDate;
            batch.BatchStartDateTime = financialBatchBag.BatchStartDate;
            batch.CampusId = newCampus?.Id;
            batch.ControlAmount = financialBatchBag.ControlAmount ?? 0.0m;
            batch.ControlItemCount = financialBatchBag.ControlItemCount ?? 0;
            batch.Name = financialBatchBag.Name;
            batch.Note = financialBatchBag.Note;
            batch.Status = financialBatchBag.Status.ConvertToEnum<BatchStatus>();
            if ( batch.BatchEndDateTime == null )
            {
                batch.BatchEndDateTime = batch.BatchStartDateTime?.AddDays( 1 );
            }


            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                if ( changes.Any() )
                {
                    HistoryService.SaveChanges(
                        RockContext,
                        typeof( FinancialBatch ),
                        Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                        batch.Id,
                        changes );
                }

                // NOT SUPPORT ATTRIBUTE IN MOBILE IT YET.
                //batch.SaveAttributeValues( RockContext );

            } );

            return ActionOk( batch.IdKey );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction( "Delete" )]
        public BlockActionResult Delete( string key )
        {
            var entityService = new FinancialBatchService( RockContext );

            if ( !TryGetEntityForEditAction( key, RockContext, out var batch, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( batch, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( batch );
            RockContext.SaveChanges();

            return ActionOk( this.GetParentPageUrl() );
        }

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction( "Edit" )]
        public BlockActionResult Edit( string key )
        {
            if ( !TryGetEntityForEditAction( key, RockContext, out var batch, out var actionError ) )
            {
                return actionError;
            }

            var bag = new FinancialBatchDetailBag
            {
                IdKey = batch.IdKey,
                Id = batch.Id,
                Name = batch.Name,
                Status = batch.Status.ToString(),
                BatchStartDate = batch.BatchStartDateTime,
                BatchEndDate = batch.BatchEndDateTime,
                TransactionAmount = batch.GetTotalTransactionAmount( RockContext ),
                ControlAmount = batch.ControlAmount,
                ControlItemCount = batch.ControlItemCount,
                Campus = batch.Campus.Name,
                CampusGuid = batch.Campus.Guid.ToString(),
                Note = batch.Note,
            };

            // NOT SUPPORT YET IN MOBILE.
            //bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return ActionOk( bag );
        }

        /// <summary>
        /// Gets the financial batch detail.
        /// </summary>
        /// <returns></returns>
        [BlockAction( "GetBatchDetails" )]
        public BlockActionResult GetBatchDetails( string key )
        {
            var batch = new FinancialBatchService( RockContext ).Get( key );

            // Grab the financial transaction that belong to the batch.
            var batchTransactionsQuery = new FinancialTransactionService( RockContext )
                .Queryable()
                .Where( ft => ft.BatchId.HasValue && ft.BatchId.Value == batch.Id );

            // Get the transaction count that belong to the batch.
            var transactionItemCount = batchTransactionsQuery.Count();

            // Get the Currency Totals
            var currencyTotals = batchTransactionsQuery
                .Where( t => t.FinancialPaymentDetailId.HasValue )
                .GroupBy( ft => ft.FinancialPaymentDetail.CurrencyTypeValueId )
                .ToList()
                .Select( g => new CurrencyTotalsBag
                {
                    CurrencyTypeValueId = g.Key ?? 0,
                    CurrencyName = DefinedValueCache.GetName( g.Key ),
                    Amount = g.Sum( pd => pd.TotalAmount )
                } )
                .ToList();

            // Grab the financial transaction detail that belong to the batch.
            var batchFinancialTransactionDetails = new FinancialTransactionDetailService( RockContext )
                .Queryable()
                .Where( ftd => ftd.Transaction.BatchId.HasValue && ftd.Transaction.BatchId.Value == batch.Id );

            // Get the Account Totals.
            var accountTotals = batchFinancialTransactionDetails
                .GroupBy( ftd => ftd.Account )
                .Select( g => new AccountTotalsBag
                {
                    AccountId = g.Key.Id,
                    AccountName = g.Key.Name,
                    TotalAmount = g.Sum( ftd => ftd.Amount )
                } )
                .ToList();

            return ActionOk( new FinancialBatchDetailBag
            {
                Id = batch?.Id,
                IdKey = batch?.IdKey,
                Name = batch?.Name,
                Status = batch?.Status.ConvertToString(),
                BatchStartDate = batch?.BatchStartDateTime,
                BatchEndDate = batch?.BatchEndDateTime,
                TransactionAmount = batch?.GetTotalTransactionAmount( RockContext ),
                TransactionCount = transactionItemCount,
                CurrencyTotals = currencyTotals,
                AccountTotals = accountTotals,
                ControlAmount = batch?.ControlAmount,
                ControlItemCount = batch?.ControlItemCount,
                Campus = batch?.Campus?.Name,
                CampusGuid = batch?.Campus?.Guid.ToString(),
                Note = batch?.Note
            } );
        }

        #endregion
    }
}
