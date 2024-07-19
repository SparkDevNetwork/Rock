﻿// <copyright>
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
using System.Linq;
using System.Text;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Finance.FinancialStatementTemplateDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Displays the details of a particular financial statement template.
    /// </summary>

    [DisplayName( "Financial Statement Template Detail" )]
    [Category( "Finance" )]
    [Description( "Displays the details of the statement template." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "feea3b29-3fce-4216-ab28-e1f69c67a574" )]
    [Rock.SystemGuid.BlockTypeGuid( "3d13455f-7e5c-46f7-975a-4a5ce12bd330" )]
    public class FinancialStatementTemplateDetail : RockDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string FinancialStatementTemplateId = "StatementTemplateId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<FinancialStatementTemplateBag, FinancialStatementTemplateDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<FinancialStatementTemplate>();

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private FinancialStatementTemplateDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new FinancialStatementTemplateDetailOptionsBag();

            var currencyType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE.AsGuid() );
            options.CurrencyTypeOptions = currencyType.DefinedValues.ConvertAll( x => x.ToListItemBag() );

            var transactionType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE.AsGuid() );
            options.TransactionTypeOptions = transactionType.DefinedValues.ConvertAll( x => x.ToListItemBag() );

            options.PaperSizeOptions = ToEnumListItemBag( typeof( Rock.Financial.FinancialStatementTemplatePDFSettingsPaperSize ) );

            return options;
        }

        /// <summary>
        /// Converts the enum to a List of ListItemBag options
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        private List<ListItemBag> ToEnumListItemBag( Type enumType )
        {
            var listItemBag = new List<ListItemBag>();
            foreach ( Enum enumValue in Enum.GetValues( enumType ) )
            {
                var text = enumValue.GetDescription() ?? enumValue.ToString().SplitCase();
                var value = enumValue.ToString();
                listItemBag.Add( new ListItemBag { Text = text, Value = value } );
            }

            return listItemBag.ToList();
        }

        /// <summary>
        /// Validates the FinancialStatementTemplate for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="financialStatementTemplate">The FinancialStatementTemplate to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the FinancialStatementTemplate is valid, <c>false</c> otherwise.</returns>
        private bool ValidateFinancialStatementTemplate( FinancialStatementTemplate financialStatementTemplate, out string errorMessage )
        {
            errorMessage = null;

            if ( !financialStatementTemplate.IsValid )
            {
                errorMessage = financialStatementTemplate.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
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
        private void SetBoxInitialEntityState( DetailBlockBox<FinancialStatementTemplateBag, FinancialStatementTemplateDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {FinancialStatementTemplate.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( FinancialStatementTemplate.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity, rockContext );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( FinancialStatementTemplate.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="FinancialStatementTemplateBag"/> that represents the entity.</returns>
        private FinancialStatementTemplateBag GetCommonEntityBag( FinancialStatementTemplate entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new FinancialStatementTemplateBag
            {
                IdKey = entity.IdKey,
                Description = entity.Description,
                IsActive = entity.IsActive,
                LogoBinaryFile = entity.LogoBinaryFile.ToListItemBag(),
                Name = entity.Name,
                ReportTemplate = entity.ReportTemplate
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="FinancialStatementTemplateBag"/> that represents the entity.</returns>
        private FinancialStatementTemplateBag GetEntityBagForView( FinancialStatementTemplate entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            var transactionSettings = entity.ReportSettings.TransactionSettings;
            if ( transactionSettings.AccountSelectionOption == Rock.Financial.FinancialStatementTemplateTransactionSettingAccountSelectionOption.AllTaxDeductibleAccounts )
            {
                var accountList = new FinancialAccountService( new RockContext() ).Queryable()
                        .Where( a => a.IsActive && a.IsTaxDeductible )
                        .ToList();

                bag.AccountsForTransactions = accountList.ConvertAll( a => a.Name ).AsDelimited( "<br/>" );
            }
            else
            {
                if ( transactionSettings.SelectedAccountIds.Any() )
                {
                    var accountList = new FinancialAccountService( new RockContext() )
                        .GetByIds( transactionSettings.SelectedAccountIds )
                        .Where( a => a.IsActive )
                        .ToList();
                    bag.AccountsForTransactions = accountList.ConvertAll( a => a.Name ).AsDelimited( "<br/>" );
                }
            }

            if ( transactionSettings.TransactionTypeGuids.Any() )
            {
                var transactionTypes = transactionSettings.TransactionTypeGuids.ConvertAll( a => DefinedValueCache.Get( a )?.Value ?? string.Empty );
                bag.SelectedTransactionTypes = transactionTypes.AsDelimited( "<br/>" );
            }

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A <see cref="FinancialStatementTemplateBag"/> that represents the entity.</returns>
        private FinancialStatementTemplateBag GetEntityBagForEdit( FinancialStatementTemplate entity, RockContext rockContext )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.FooterTemplateHtmlFragment = entity.FooterSettings.HtmlFragment;

            bag.MarginTopMillimeters = entity.ReportSettings.PDFSettings.MarginTopMillimeters;
            bag.MarginBottomMillimeters = entity.ReportSettings.PDFSettings.MarginBottomMillimeters;
            bag.MarginLeftMillimeters = entity.ReportSettings.PDFSettings.MarginLeftMillimeters;
            bag.MarginRightMillimeters = entity.ReportSettings.PDFSettings.MarginRightMillimeters;
            bag.PaperSize = entity.ReportSettings.PDFSettings.PaperSize.ToString();

            bag.HideRefundedTransactions = entity.ReportSettings.TransactionSettings.HideRefundedTransactions;
            bag.HideCorrectedTransactionOnSameData = entity.ReportSettings.TransactionSettings.HideCorrectedTransactionOnSameData;
            bag.CurrencyTypesForCashGifts = entity.ReportSettings.TransactionSettings.CurrencyTypesForCashGiftGuids;
            bag.CurrencyTypesForNonCashGifts = entity.ReportSettings.TransactionSettings.CurrencyTypesForNonCashGuids;
            bag.TransactionTypes = entity.ReportSettings.TransactionSettings.TransactionTypeGuids;
            bag.AccountSelectionOption = entity.ReportSettings.TransactionSettings.AccountSelectionOption == Financial.FinancialStatementTemplateTransactionSettingAccountSelectionOption.AllTaxDeductibleAccounts ? "0" : "1";
            bag.UseCustomAccountIds = entity.ReportSettings.TransactionSettings.AccountSelectionOption != Rock.Financial.FinancialStatementTemplateTransactionSettingAccountSelectionOption.AllTaxDeductibleAccounts;

            if ( entity.ReportSettings.TransactionSettings.SelectedAccountIds.Any() )
            {
                var accountList = new FinancialAccountService( rockContext ).GetByIds( entity.ReportSettings.TransactionSettings.SelectedAccountIds )
                    .Where( a => a.IsActive )
                    .ToList();
                bag.SelectedAccounts = accountList.ToListItemBagList();
            }

            bag.IncludeChildAccountsCustom = entity.ReportSettings.TransactionSettings.AccountSelectionOption == Rock.Financial.FinancialStatementTemplateTransactionSettingAccountSelectionOption.SelectedAccountsIncludeChildren;

            bag.IncludeChildAccountsPledges = entity.ReportSettings.PledgeSettings.IncludeGiftsToChildAccounts;
            bag.IncludeNonCashGiftsPledge = entity.ReportSettings.PledgeSettings.IncludeNonCashGifts;

            if ( entity.ReportSettings.PledgeSettings.AccountIds.Any() )
            {
                var accountList = new FinancialAccountService( rockContext ).GetByIds( entity.ReportSettings.PledgeSettings.AccountIds )
                    .Where( a => a.IsActive )
                    .ToList();
                bag.PledgeAccounts = accountList.ToListItemBagList();
            }

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( FinancialStatementTemplate entity, DetailBlockBox<FinancialStatementTemplateBag, FinancialStatementTemplateDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => entity.IsActive = box.Entity.IsActive );

            box.IfValidProperty( nameof( box.Entity.ReportTemplate ),
                () => entity.ReportTemplate = box.Entity.ReportTemplate );

            box.IfValidProperty( nameof( box.Entity.FooterTemplateHtmlFragment ),
                () => entity.FooterSettings.HtmlFragment = box.Entity.FooterTemplateHtmlFragment );

            // PDF Settings
            box.IfValidProperty( nameof( box.Entity.MarginTopMillimeters ),
                () => entity.ReportSettings.PDFSettings.MarginTopMillimeters = box.Entity.MarginTopMillimeters );

            box.IfValidProperty( nameof( box.Entity.MarginLeftMillimeters ),
                () => entity.ReportSettings.PDFSettings.MarginLeftMillimeters = box.Entity.MarginLeftMillimeters );

            box.IfValidProperty( nameof( box.Entity.MarginRightMillimeters ),
                () => entity.ReportSettings.PDFSettings.MarginRightMillimeters = box.Entity.MarginRightMillimeters );

            box.IfValidProperty( nameof( box.Entity.MarginBottomMillimeters ),
                () => entity.ReportSettings.PDFSettings.MarginBottomMillimeters = box.Entity.MarginBottomMillimeters );

            box.IfValidProperty( nameof( box.Entity.PaperSize ),
                () => entity.ReportSettings.PDFSettings.PaperSize = box.Entity.PaperSize.ConvertToEnum<Rock.Financial.FinancialStatementTemplatePDFSettingsPaperSize>() );

            // Transaction Settings
            box.IfValidProperty( nameof( box.Entity.CurrencyTypesForCashGifts ),
                () => entity.ReportSettings.TransactionSettings.CurrencyTypesForCashGiftGuids = box.Entity.CurrencyTypesForCashGifts );

            box.IfValidProperty( nameof( box.Entity.CurrencyTypesForNonCashGifts ),
                () => entity.ReportSettings.TransactionSettings.CurrencyTypesForNonCashGuids = box.Entity.CurrencyTypesForNonCashGifts );

            box.IfValidProperty( nameof( box.Entity.TransactionTypes ),
                () => entity.ReportSettings.TransactionSettings.TransactionTypeGuids = box.Entity.TransactionTypes );

            box.IfValidProperty( nameof( box.Entity.HideRefundedTransactions ),
                () => entity.ReportSettings.TransactionSettings.HideRefundedTransactions = box.Entity.HideRefundedTransactions );

            box.IfValidProperty( nameof( box.Entity.HideCorrectedTransactionOnSameData ),
                () => entity.ReportSettings.TransactionSettings.HideCorrectedTransactionOnSameData = box.Entity.HideCorrectedTransactionOnSameData );

            // Pledge Settings
            box.IfValidProperty( nameof( box.Entity.IncludeChildAccountsPledges ),
                () => entity.ReportSettings.PledgeSettings.IncludeGiftsToChildAccounts = box.Entity.IncludeChildAccountsPledges );

            box.IfValidProperty( nameof( box.Entity.IncludeNonCashGiftsPledge ),
                () => entity.ReportSettings.PledgeSettings.IncludeNonCashGifts = box.Entity.IncludeNonCashGiftsPledge );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="FinancialStatementTemplate"/> to be viewed or edited on the page.</returns>
        private FinancialStatementTemplate GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<FinancialStatementTemplate, FinancialStatementTemplateService>( rockContext, PageParameterKey.FinancialStatementTemplateId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
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
        private string GetSecurityGrantToken( FinancialStatementTemplate entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out FinancialStatementTemplate entity, out BlockActionResult error )
        {
            var entityService = new FinancialStatementTemplateService( rockContext );
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
                entity = new FinancialStatementTemplate();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{FinancialStatementTemplate.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${FinancialStatementTemplate.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Marks the old image as temporary.
        /// </summary>
        /// <param name="oldbinaryFileId">The binary file identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        private void MarkOldImageAsTemporary( int? oldbinaryFileId, int? newBinaryFileId, RockContext rockContext )
        {
            var binaryFileService = new BinaryFileService( rockContext );

            if ( oldbinaryFileId != newBinaryFileId )
            {
                var oldImageTemplatePreview = binaryFileService.Get( oldbinaryFileId ?? 0 );
                if ( oldImageTemplatePreview != null )
                {
                    // the old image won't be needed anymore, so make it IsTemporary and have it get cleaned up later
                    oldImageTemplatePreview.IsTemporary = true;
                }
            }
        }

        /// <summary>
        /// Ensures the current image is not marked as temporary.
        /// </summary>
        /// <param name="binaryFileId">The binary file identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void EnsureCurrentImageIsNotMarkedAsTemporary( int? binaryFileId, RockContext rockContext )
        {
            var binaryFileService = new BinaryFileService( rockContext );

            if ( binaryFileId.HasValue )
            {
                var imageTemplatePreview = binaryFileService.Get( binaryFileId.Value );
                if ( imageTemplatePreview != null && imageTemplatePreview.IsTemporary )
                {
                    imageTemplatePreview.IsTemporary = false;
                }
            }
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

                var box = new DetailBlockBox<FinancialStatementTemplateBag, FinancialStatementTemplateDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity, rockContext )
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
        public BlockActionResult Save( DetailBlockBox<FinancialStatementTemplateBag, FinancialStatementTemplateDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new FinancialStatementTemplateService( rockContext );
                var financialAccountService = new FinancialAccountService( rockContext );

                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                if ( box.Entity.AccountSelectionOption == "0" )
                {
                    entity.ReportSettings.TransactionSettings.AccountSelectionOption = Rock.Financial.FinancialStatementTemplateTransactionSettingAccountSelectionOption.AllTaxDeductibleAccounts;
                }
                else if ( box.Entity.AccountSelectionOption == "1" )
                {
                    entity.ReportSettings.TransactionSettings.AccountSelectionOption = Rock.Financial.FinancialStatementTemplateTransactionSettingAccountSelectionOption.SelectedAccountsIncludeChildren;
                }
                else
                {
                    entity.ReportSettings.TransactionSettings.AccountSelectionOption = Rock.Financial.FinancialStatementTemplateTransactionSettingAccountSelectionOption.SelectedAccounts;
                }

                var selectedAccountGuids = box.Entity.SelectedAccounts.ConvertAll( lb => lb.Value.AsGuid() );
                entity.ReportSettings.TransactionSettings.SelectedAccountIds = financialAccountService.Queryable().Where( fa => selectedAccountGuids.Contains( fa.Guid ) ).Select( fa => fa.Id ).ToList();

                var pledgeAccountGuids = box.Entity.PledgeAccounts.ConvertAll( lb => lb.Value.AsGuid() );
                entity.ReportSettings.PledgeSettings.AccountIds = financialAccountService.Queryable().Where( fa => pledgeAccountGuids.Contains( fa.Guid ) ).Select( fa => fa.Id ).ToList();

                if ( box.Entity.LogoBinaryFile != null )
                {
                    var binaryFileId = box.Entity.LogoBinaryFile.GetEntityId<BinaryFile>( rockContext );
                    if ( entity.LogoBinaryFileId != binaryFileId )
                    {
                        MarkOldImageAsTemporary( entity.LogoBinaryFileId, binaryFileId, rockContext );
                        entity.LogoBinaryFileId = binaryFileId;
                        // Ensure that the Image is not set as IsTemporary=True
                        EnsureCurrentImageIsNotMarkedAsTemporary( entity.LogoBinaryFileId, rockContext );
                    }
                }

                // Ensure everything is valid before saving.
                if ( !ValidateFinancialStatementTemplate( entity, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var isNew = entity.Id == 0;

                rockContext.SaveChanges();

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.FinancialStatementTemplateId] = entity.IdKey
                    } ) );
                }

                // Ensure navigation properties will work now.
                entity = entityService.Get( entity.Id );

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
                var entityService = new FinancialStatementTemplateService( rockContext );

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
        public BlockActionResult RefreshAttributes( DetailBlockBox<FinancialStatementTemplateBag, FinancialStatementTemplateDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<FinancialStatementTemplateBag, FinancialStatementTemplateDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity, rockContext )
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

        #endregion
    }
}
