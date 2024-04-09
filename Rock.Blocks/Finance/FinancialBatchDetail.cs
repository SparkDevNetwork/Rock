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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Finance.FinancialBatchDetail;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Displays the details of a particular financial batch.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockDetailBlockType" />

    [DisplayName( "Financial Batch Detail" )]
    [Category( "Finance" )]
    [Description( "Displays the details of a particular financial batch." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes
    [LinkedPage( "Transaction Matching Page",
        Description = "Page used to match transactions for a batch.",
        Order = 1 )]

    [LinkedPage( "Audit Page",
        Description = "Page used to display the history of changes to a batch.",
        Order = 2 )]

    [DefinedTypeField( "Batch Names",
        Description = "The Defined Type that contains a predefined list of batch names to choose from instead of entering it in manually when adding a new batch. Leave this blank to hide this option and let them edit the batch name manually.",
        IsRequired = false,
        DefaultValue = "",
        Category = "",
        Key = AttributeKey.BatchNames,
        Order = 3 )]
    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "b5976e12-a3e4-4faf-95b5-3d54f25405da" )]
    [Rock.SystemGuid.BlockTypeGuid( "6be58680-8795-46a0-8bfa-434a01feb4c8" )]
    public class FinancialBatchDetail : RockDetailBlockType, IBreadCrumbBlock
    {
        private const string AuthorizationReopenBatch = "ReopenBatch";

        #region Keys

        /// <summary>
        /// Keys to use for Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string BatchNames = "BatchNames";
        }

        private static class PageParameterKey
        {
            public const string BatchId = "BatchId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
            public const string MatchTransactions = "MatchTransactions";
            public const string AuditLogs = "AuditLogs";
        }

        #endregion Keys

        #region Block State
        /// <summary>
        /// The flag is set to true if the user has the AuthorizationReopenBatch
        /// This value is computed from the entity and is set in the options
        /// </summary>
        private bool IsReopenAuthorized;

        /// <summary>
        /// The flag which is set to true if the user is forbidden from reopening a closed batch
        /// </summary>
        private bool IsReopenDisabled;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<FinancialBatchBag, FinancialBatchDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                // this computation is redundant for this block. But, keeping it as it was generated as part of the code generator.
                int id = Rock.Utility.IdHasher.Instance.GetId( box.Entity?.IdKey ) ?? 0;

                box.NavigationUrls = GetBoxNavigationUrls( id );
                box.Options = GetBoxOptions( box.Entity, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<FinancialBatch>();

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="entity"><c>true</c> the entity on which the boxes are created <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private FinancialBatchDetailOptionsBag GetBoxOptions( FinancialBatchBag entity, RockContext rockContext )
        {
            var options = new FinancialBatchDetailOptionsBag();
            options.IsReopenAuthorized = IsReopenAuthorized;

            if ( entity == null )
            {
                return options;
            }
            var financialTransactionService = new FinancialTransactionService( rockContext );
            var batchTransactionsQuery = financialTransactionService.Queryable()
                .Where( a => a.BatchId.HasValue && a.BatchId.Value == entity.Id );

            options.TransactionItemCount = batchTransactionsQuery.Count();

            options.CurrencyTypes = batchTransactionsQuery
                    .GroupBy( c => new
                    {
                        CurrencyTypeValueId = c.FinancialPaymentDetailId.HasValue ? c.FinancialPaymentDetail.CurrencyTypeValueId : 0,
                    } )
                    .Select( s => new
                    {
                        CurrencyTypeValueId = s.Key.CurrencyTypeValueId,
                        Amount = s.Sum( a => ( decimal? ) a.TransactionDetails.Sum( t => t.Amount ) ) ?? 0.0M
                    } )
                    .ToList()
                    .Select( s => new FinancialBatchCurrencyTotalsBag
                    {
                        Name = DefinedValueCache.GetName( s.CurrencyTypeValueId ),
                        Currency = s.Amount
                    } ).OrderBy( a => a.Name )
                    .ToList();

            var financialTransactionDetailService = new FinancialTransactionDetailService( rockContext );
            var qryTransactionDetails = financialTransactionDetailService.Queryable()
                .Where( a => a.Transaction.BatchId == entity.Id );
            options.Accounts = qryTransactionDetails
                    .GroupBy( d => new
                    {
                        AccountId = d.AccountId,
                        AccountName = d.Account.Name
                    } )
                    .Select( s => new FinancialBatchAccountTotalsBag
                    {
                        Name = s.Key.AccountName,
                        Currency = s.Sum( a => ( decimal? ) a.Amount ) ?? 0.0M
                    } )
                    .OrderBy( s => s.Name )
                    .ToList();

            options.TransactionAmount = options.Accounts
                .Select( a => a.Currency )
                .Sum();

            // copying the logic from the web forms
            options.IsStatusChangeDisabled = entity.IsAutomated && entity.Status == BatchStatus.Pending || IsReopenDisabled;

            // The Attribute Value for Batch Name needs to be sent only if a new Financial Batch is being created.
            if ( entity.Id == 0 )
            {
                options.BatchNameDefinedTypeGuid = GetAttributeValue( AttributeKey.BatchNames );
            }

            var currencyInfo = new RockCurrencyCodeInfo();
            options.CurrencyInfo = new ViewModels.Utility.CurrencyInfoBag
            {
                Symbol = currencyInfo.Symbol,
                DecimalPlaces = currencyInfo.DecimalPlaces,
                SymbolLocation = currencyInfo.SymbolLocation
            };

            return options;
        }

        /// <summary>
        /// Validates the FinancialBatch for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="financialBatch">The FinancialBatch to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the FinancialBatch is valid, <c>false</c> otherwise.</returns>
        private bool ValidateFinancialBatch( FinancialBatch financialBatch, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;
            if ( !financialBatch.IsValid )
            {
                errorMessage = string.Join( "</br>", financialBatch.ValidationResults.Select( v => v.ErrorMessage ) );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<FinancialBatchBag, FinancialBatchDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {FinancialBatch.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson );
            IsReopenAuthorized = entity.IsAuthorized( AuthorizationReopenBatch, RequestContext.CurrentPerson );
            IsReopenDisabled = entity.Status == BatchStatus.Closed && !IsReopenAuthorized;
            box.IsEditable = entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson ) && !IsReopenDisabled;

            // do the authorization check.
            entity.LoadAttributes( rockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( FinancialBatch.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                    box.Entity.Status = null; // set the status to null for new batch so that the default status is displayed by the remote device
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( FinancialBatch.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="FinancialBatchBag"/> that represents the entity.</returns>
        private FinancialBatchBag GetCommonEntityBag( FinancialBatch entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new FinancialBatchBag
            {
                IdKey = entity.IdKey,
                Id = entity.Id,
                AccountingSystemCode = entity.AccountingSystemCode,
                BatchEndDateTime = entity.BatchEndDateTime,
                BatchStartDateTime = entity.BatchStartDateTime,
                Campus = entity.Campus.ToListItemBag(),
                ControlAmount = entity.ControlAmount,
                ControlItemCount = entity.ControlItemCount,
                Name = entity.Name,
                Note = entity.Note,
                IsAutomated = entity.IsAutomated,
                Status = entity.Status
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="FinancialBatchBag"/> that represents the entity.</returns>
        private FinancialBatchBag GetEntityBagForView( FinancialBatch entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="FinancialBatchBag"/> that represents the entity.</returns>
        private FinancialBatchBag GetEntityBagForEdit( FinancialBatch entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( FinancialBatch entity, DetailBlockBox<FinancialBatchBag, FinancialBatchDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.AccountingSystemCode ),
                () => entity.AccountingSystemCode = box.Entity.AccountingSystemCode );

            box.IfValidProperty( nameof( box.Entity.BatchEndDateTime ),
                () => entity.BatchEndDateTime = box.Entity.BatchEndDateTime );

            box.IfValidProperty( nameof( box.Entity.BatchStartDateTime ),
                () =>
                {
                    entity.BatchStartDateTime = box.Entity.BatchStartDateTime;

                    // Replicating the behavior in the Webforms block where the batch end date is set
                    // to the next day after the start date if not provided.
                    if ( entity.BatchEndDateTime == null )
                    {
                        entity.BatchEndDateTime = entity.BatchStartDateTime?.AddDays( 1 );
                    }
                } );

            box.IfValidProperty( nameof( box.Entity.Campus ),
                () => entity.CampusId = box.Entity.Campus.GetEntityId<Campus>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.ControlAmount ),
                () => entity.ControlAmount = box.Entity.ControlAmount );

            box.IfValidProperty( nameof( box.Entity.ControlItemCount ),
                () => entity.ControlItemCount = box.Entity.ControlItemCount );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.Note ),
                () => entity.Note = box.Entity.Note );

            box.IfValidProperty( nameof( box.Entity.Status ),
                () => entity.Status = box.Entity.Status ?? BatchStatus.Open );

            box.IfValidProperty( nameof( box.Entity.Id ),
                () => entity.Id = box.Entity.Id );

            box.IfValidProperty( nameof( box.Entity.IsAutomated ),
                () => entity.IsAutomated = box.Entity.IsAutomated );

            box.IfValidProperty( nameof( box.Entity.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( rockContext );

                    entity.SetPublicAttributeValues( box.Entity.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="FinancialBatch"/> to be viewed or edited on the page.</returns>
        private FinancialBatch GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<FinancialBatch, FinancialBatchService>( rockContext, PageParameterKey.BatchId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls( int id )
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl(),
                [NavigationUrlKey.MatchTransactions] = this.GetLinkedPageUrl( "TransactionMatchingPage",
                    new Dictionary<string, string>() { { "BatchId", $"{id}" } } ),
                [NavigationUrlKey.AuditLogs] = this.GetLinkedPageUrl( "AuditPage",
                    new Dictionary<string, string>() { { "BatchId", $"{id}" } } )
            };
        }

        /// <inheritdoc/>
        protected override string RenewSecurityGrantToken()
        {
            using ( var rockContext = new RockContext() )
            {
                var entity = GetInitialEntity( rockContext );

                if ( entity != null )
                {
                    entity.LoadAttributes( rockContext );
                }

                return GetSecurityGrantToken( entity );
            }
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken( FinancialBatch entity )
        {
            var securityGrant = new Rock.Security.SecurityGrant();

            securityGrant.AddRulesForAttributes( entity, RequestContext.CurrentPerson );

            return securityGrant.ToToken();
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="rockContext">The database context to load the entity from.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out FinancialBatch entity, out BlockActionResult error )
        {
            var entityService = new FinancialBatchService( rockContext );
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
                entity = new FinancialBatch();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{FinancialBatch.FriendlyTypeName} not found." );
                return false;
            }

            var isReopenDisabled = entity.Status == BatchStatus.Closed && !entity.IsAuthorized( AuthorizationReopenBatch, RequestContext.CurrentPerson );
            if ( !entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson ) || isReopenDisabled )
            {
                error = ActionBadRequest( $"Not authorized to edit ${FinancialBatch.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        #endregion

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
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                entity.LoadAttributes( rockContext );

                var box = new DetailBlockBox<FinancialBatchBag, FinancialBatchDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity )
                };

                return ActionOk( box );
            }
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( DetailBlockBox<FinancialBatchBag, FinancialBatchDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new FinancialBatchService( rockContext );

                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                string errorMessage;
                if ( !entity.IsValidBatchStatusChange( entity.Status, box.Entity.Status.GetValueOrDefault(), RequestContext.CurrentPerson, out errorMessage ) )
                {
                    return ActionUnauthorized( errorMessage );
                }

                var isNew = entity.Id == 0;
                var isStatusChanged = box.Entity.Status != entity.Status;

                var changes = new History.HistoryChangeList();
                if ( isNew )
                {
                    changes.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Batch" );
                }

                History.EvaluateChange( changes, "Batch Name", entity.Name, box.Entity?.Name );
                var currentCampusName = CampusCache.Get( entity.CampusId ?? 0 )?.Name ?? "None";
                History.EvaluateChange( changes, "Campus", currentCampusName, box.Entity?.Campus?.Text ?? "None" );
                History.EvaluateChange( changes, "Status", entity?.Status, box.Entity?.Status );
                History.EvaluateChange( changes, "Start Date/Time", entity.BatchStartDateTime, box.Entity?.BatchStartDateTime );
                History.EvaluateChange( changes, "End Date/Time", entity.BatchEndDateTime, box.Entity?.BatchEndDateTime );
                History.EvaluateChange( changes, "Control Amount", entity?.ControlAmount.FormatAsCurrency(), ( box.Entity?.ControlAmount ?? 0.0m ).FormatAsCurrency() );
                History.EvaluateChange( changes, "Control Item Count", entity.ControlItemCount, box.Entity?.ControlItemCount );
                History.EvaluateChange( changes, "Accounting System Code", entity.AccountingSystemCode, box.Entity?.AccountingSystemCode );
                History.EvaluateChange( changes, "Notes", entity.Note, box.Entity?.Note );

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Ensure everything is valid before saving.
                if ( !ValidateFinancialBatch( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                rockContext.WrapTransaction( () =>
                {
                    if ( rockContext.SaveChanges() > 0 )
                    {
                        if ( changes.Any() )
                        {
                            HistoryService.SaveChanges(
                                rockContext,
                                typeof( FinancialBatch ),
                                Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                                entity.Id,
                                changes );
                        }
                    }
                    entity.SaveAttributeValues( rockContext );
                } );

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.BatchId] = entity.IdKey
                    } ) );
                }

                /**
                 * 11/18/2023 - KA
                 * If the status has been updated return current page url to trigger
                 * a page refresh on the client. The Batch Detail block is typically
                 * used with the Transaction List block and an update may be required
                 * to reflect the change in the batch's status. This will required some
                 * refactoring once Obsidian blocks can signal each other.
                 */
                if ( isStatusChanged )
                {
                    return ActionOk( this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.BatchId] = entity.IdKey
                    } ) );
                }

                // Ensure navigation properties will work now.
                entity = entityService.Get( entity.Id );

                entity.LoadAttributes( rockContext );

                var contextEntity = RequestContext.GetContextEntity<FinancialBatch>();
                contextEntity.CopyPropertiesFrom( entity );

                return ActionOk( GetEntityBagForView( entity ) );
            }
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new FinancialBatchService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk( this.GetParentPageUrl() );
            }
        }

        /// <summary>
        /// Refreshes the list of attributes that can be displayed for editing
        /// purposes based on any modified values on the entity.
        /// </summary>
        /// <param name="box">The box that contains all the information about the entity being edited.</param>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public BlockActionResult RefreshAttributes( DetailBlockBox<FinancialBatchBag, FinancialBatchDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Reload attributes based on the new property values.
                entity.LoadAttributes( rockContext );

                var refreshedBox = new DetailBlockBox<FinancialBatchBag, FinancialBatchDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity )
                };

                var oldAttributeGuids = box.Entity.Attributes.Values.Select( a => a.AttributeGuid ).ToList();
                var newAttributeGuids = refreshedBox.Entity.Attributes.Values.Select( a => a.AttributeGuid );

                // If the attributes haven't changed then return a 204 status code.
                if ( oldAttributeGuids.SequenceEqual( newAttributeGuids ) )
                {
                    return ActionStatusCode( System.Net.HttpStatusCode.NoContent );
                }

                // Replace any values for attributes that haven't changed with
                // the value sent by the client. This ensures any unsaved attribute
                // value changes are not lost.
                foreach ( var kvp in refreshedBox.Entity.Attributes )
                {
                    if ( oldAttributeGuids.Contains( kvp.Value.AttributeGuid ) )
                    {
                        refreshedBox.Entity.AttributeValues[kvp.Key] = box.Entity.AttributeValues[kvp.Key];
                    }
                }

                return ActionOk( refreshedBox );
            }
        }

        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            using ( var rockContext = new RockContext() )
            {
                var batchId = pageReference.GetPageParameter( PageParameterKey.BatchId );
                var batchName = new FinancialBatchService( rockContext )
                    .GetSelect( batchId, b => b.Name );
                var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageReference.Parameters );
                var breadCrumb = new BreadCrumbLink( batchName ?? "New Batch", breadCrumbPageRef );

                return new BreadCrumbResult
                {
                    BreadCrumbs = new List<IBreadCrumb>
                   {
                       breadCrumb
                   }
                };
            }
        }

        #endregion
    }
}
