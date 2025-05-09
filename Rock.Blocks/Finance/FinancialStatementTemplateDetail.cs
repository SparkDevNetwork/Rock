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
using System.Linq;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Financial;
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
    public class FinancialStatementTemplateDetail : RockEntityDetailBlockType<FinancialStatementTemplate, FinancialStatementTemplateBag>
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
            var box = new DetailBlockBox<FinancialStatementTemplateBag, FinancialStatementTemplateDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( box.IsEditable );

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private FinancialStatementTemplateDetailOptionsBag GetBoxOptions( bool isEditable )
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
        private void SetBoxInitialEntityState( DetailBlockBox<FinancialStatementTemplateBag, FinancialStatementTemplateDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {FinancialStatementTemplate.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( RockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
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
                    box.Entity = GetEntityBagForEdit( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( FinancialStatementTemplate.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
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

        /// <inheritdoc/>
        protected override FinancialStatementTemplateBag GetEntityBagForView( FinancialStatementTemplate entity )
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

        /// <inheritdoc/>
        protected override FinancialStatementTemplateBag GetEntityBagForEdit( FinancialStatementTemplate entity )
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
                var accountList = new FinancialAccountService( RockContext ).GetByIds( entity.ReportSettings.TransactionSettings.SelectedAccountIds )
                    .Where( a => a.IsActive )
                    .ToList();
                bag.SelectedAccounts = accountList.ToListItemBagList();
            }

            bag.IncludeChildAccountsCustom = entity.ReportSettings.TransactionSettings.AccountSelectionOption == Rock.Financial.FinancialStatementTemplateTransactionSettingAccountSelectionOption.SelectedAccountsIncludeChildren;

            bag.IncludeChildAccountsPledges = entity.ReportSettings.PledgeSettings.IncludeGiftsToChildAccounts;
            bag.IncludeNonCashGiftsPledge = entity.ReportSettings.PledgeSettings.IncludeNonCashGifts;

            if ( entity.ReportSettings.PledgeSettings.AccountIds.Any() )
            {
                var accountList = new FinancialAccountService( RockContext ).GetByIds( entity.ReportSettings.PledgeSettings.AccountIds )
                    .Where( a => a.IsActive )
                    .ToList();
                bag.PledgeAccounts = accountList.ToListItemBagList();
            }

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( FinancialStatementTemplate entity, ValidPropertiesBox<FinancialStatementTemplateBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.ReportTemplate ),
                () => entity.ReportTemplate = box.Bag.ReportTemplate );

            box.IfValidProperty( nameof( box.Bag.FooterTemplateHtmlFragment ),
                () => entity.FooterSettings.HtmlFragment = box.Bag.FooterTemplateHtmlFragment );

            // PDF Settings
            box.IfValidProperty( nameof( box.Bag.MarginTopMillimeters ),
                () => entity.ReportSettings.PDFSettings.MarginTopMillimeters = box.Bag.MarginTopMillimeters );

            box.IfValidProperty( nameof( box.Bag.MarginLeftMillimeters ),
                () => entity.ReportSettings.PDFSettings.MarginLeftMillimeters = box.Bag.MarginLeftMillimeters );

            box.IfValidProperty( nameof( box.Bag.MarginRightMillimeters ),
                () => entity.ReportSettings.PDFSettings.MarginRightMillimeters = box.Bag.MarginRightMillimeters );

            box.IfValidProperty( nameof( box.Bag.MarginBottomMillimeters ),
                () => entity.ReportSettings.PDFSettings.MarginBottomMillimeters = box.Bag.MarginBottomMillimeters );

            box.IfValidProperty( nameof( box.Bag.PaperSize ),
                () => entity.ReportSettings.PDFSettings.PaperSize = box.Bag.PaperSize.ConvertToEnum<Rock.Financial.FinancialStatementTemplatePDFSettingsPaperSize>() );

            // Transaction Settings
            box.IfValidProperty( nameof( box.Bag.CurrencyTypesForCashGifts ),
                () => entity.ReportSettings.TransactionSettings.CurrencyTypesForCashGiftGuids = box.Bag.CurrencyTypesForCashGifts );

            box.IfValidProperty( nameof( box.Bag.CurrencyTypesForNonCashGifts ),
                () => entity.ReportSettings.TransactionSettings.CurrencyTypesForNonCashGuids = box.Bag.CurrencyTypesForNonCashGifts );

            box.IfValidProperty( nameof( box.Bag.TransactionTypes ),
                () => entity.ReportSettings.TransactionSettings.TransactionTypeGuids = box.Bag.TransactionTypes );

            box.IfValidProperty( nameof( box.Bag.HideRefundedTransactions ),
                () => entity.ReportSettings.TransactionSettings.HideRefundedTransactions = box.Bag.HideRefundedTransactions );

            box.IfValidProperty( nameof( box.Bag.HideCorrectedTransactionOnSameData ),
                () => entity.ReportSettings.TransactionSettings.HideCorrectedTransactionOnSameData = box.Bag.HideCorrectedTransactionOnSameData );

            box.IfValidProperty( nameof( box.Bag.IncludeChildAccountsCustom ),
                () => entity.ReportSettings.TransactionSettings.AccountSelectionOption = GetAccountSelectionOption( box.Bag ) );

            // Pledge Settings
            box.IfValidProperty( nameof( box.Bag.IncludeChildAccountsPledges ),
                () => entity.ReportSettings.PledgeSettings.IncludeGiftsToChildAccounts = box.Bag.IncludeChildAccountsPledges );

            box.IfValidProperty( nameof( box.Bag.IncludeNonCashGiftsPledge ),
                () => entity.ReportSettings.PledgeSettings.IncludeNonCashGifts = box.Bag.IncludeNonCashGiftsPledge );

            return true;
        }

        /// <summary>
        /// Gets the account selection option.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <returns></returns>
        private FinancialStatementTemplateTransactionSettingAccountSelectionOption GetAccountSelectionOption( FinancialStatementTemplateBag bag )
        {
            if ( bag.AccountSelectionOption == "0" )
            {
                return FinancialStatementTemplateTransactionSettingAccountSelectionOption.AllTaxDeductibleAccounts;
            }
            else if ( bag.IncludeChildAccountsCustom )
            {
                return FinancialStatementTemplateTransactionSettingAccountSelectionOption.SelectedAccountsIncludeChildren;
            }
            else
            {
                return FinancialStatementTemplateTransactionSettingAccountSelectionOption.SelectedAccounts;
            }
        }

        /// <inheritdoc/>
        protected override FinancialStatementTemplate GetInitialEntity()
        {
            return GetInitialEntity<FinancialStatementTemplate, FinancialStatementTemplateService>( RockContext, PageParameterKey.FinancialStatementTemplateId );
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
        protected override bool TryGetEntityForEditAction( string idKey, out FinancialStatementTemplate entity, out BlockActionResult error )
        {
            var entityService = new FinancialStatementTemplateService( RockContext );
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
        private void MarkOldImageAsTemporary( int? oldbinaryFileId, int? newBinaryFileId )
        {
            var binaryFileService = new BinaryFileService( RockContext );

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
        private void EnsureCurrentImageIsNotMarkedAsTemporary( int? binaryFileId )
        {
            var binaryFileService = new BinaryFileService( RockContext );

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
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<FinancialStatementTemplateBag>
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
        public BlockActionResult Save( ValidPropertiesBox<FinancialStatementTemplateBag> box )
        {
            var entityService = new FinancialStatementTemplateService( RockContext );
            var financialAccountService = new FinancialAccountService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            if ( box.Bag.SelectedAccounts != null )
            {
                var selectedAccountGuids = box.Bag.SelectedAccounts.ConvertAll( lb => lb.Value.AsGuid() );
                entity.ReportSettings.TransactionSettings.SelectedAccountIds = financialAccountService.Queryable().Where( fa => selectedAccountGuids.Contains( fa.Guid ) ).Select( fa => fa.Id ).ToList();
            }

            if ( box.Bag.PledgeAccounts != null )
            {
                var pledgeAccountGuids = box.Bag.PledgeAccounts.ConvertAll( lb => lb.Value.AsGuid() );
                entity.ReportSettings.PledgeSettings.AccountIds = financialAccountService.Queryable().Where( fa => pledgeAccountGuids.Contains( fa.Guid ) ).Select( fa => fa.Id ).ToList();
            }

            if ( box.Bag.LogoBinaryFile != null )
            {
                var binaryFileId = box.Bag.LogoBinaryFile.GetEntityId<BinaryFile>( RockContext );
                if ( entity.LogoBinaryFileId != binaryFileId )
                {
                    MarkOldImageAsTemporary( entity.LogoBinaryFileId, binaryFileId );
                    entity.LogoBinaryFileId = binaryFileId;
                    // Ensure that the Image is not set as IsTemporary=True
                    EnsureCurrentImageIsNotMarkedAsTemporary( entity.LogoBinaryFileId );
                }
            }

            // Ensure everything is valid before saving.
            if ( !ValidateFinancialStatementTemplate( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.SaveChanges();

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.FinancialStatementTemplateId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForView( entity );

            return ActionOk( new ValidPropertiesBox<FinancialStatementTemplateBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new FinancialStatementTemplateService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk( this.GetParentPageUrl() );
        }

        #endregion
    }
}
