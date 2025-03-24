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
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Finance.FinancialPersonSavedAccountDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Displays the details of a particular financial person saved account.
    /// </summary>

    [DisplayName( "Saved Account Detail" )]
    [Category( "Finance" )]
    [Description( "Displays the details of a particular financial person saved account." )]
    [IconCssClass( "fa fa-credit-card" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "8e672306-427f-46d1-bf1b-08dd74ca2af6" )]
    [Rock.SystemGuid.BlockTypeGuid( "141278a4-eb96-4f4a-b936-ab1bacef7ae4" )]
    public class FinancialPersonSavedAccountDetail : RockEntityDetailBlockType<FinancialPersonSavedAccount, FinancialPersonSavedAccountBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string FinancialPersonSavedAccount = "FinancialPersonSavedAccount";
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
            var box = new DetailBlockBox<FinancialPersonSavedAccountBag, FinancialPersonSavedAccountDetailOptionsBag>();

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
        private FinancialPersonSavedAccountDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new FinancialPersonSavedAccountDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the FinancialPersonSavedAccount for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="financialPersonSavedAccount">The FinancialPersonSavedAccount to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the FinancialPersonSavedAccount is valid, <c>false</c> otherwise.</returns>
        private bool ValidateFinancialPersonSavedAccount( FinancialPersonSavedAccount financialPersonSavedAccount, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<FinancialPersonSavedAccountBag, FinancialPersonSavedAccountDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {FinancialPersonSavedAccount.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( FinancialPersonSavedAccount.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( FinancialPersonSavedAccount.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="FinancialPersonSavedAccountBag"/> that represents the entity.</returns>
        private FinancialPersonSavedAccountBag GetCommonEntityBag( FinancialPersonSavedAccount entity )
        {
            if ( entity == null )
            {
                return null;
            }

            Guid? currencyTypeValueGuid = null;

            if( entity.FinancialPaymentDetail?.CurrencyTypeValue == null && entity.FinancialPaymentDetail?.CurrencyTypeValueId > 0 )
            {
                currencyTypeValueGuid = DefinedValueCache.Get( entity.FinancialPaymentDetail.CurrencyTypeValueId.Value )?.Guid;
            }
            else if( entity.FinancialPaymentDetail?.CurrencyTypeValue != null )
            {
                currencyTypeValueGuid = entity.FinancialPaymentDetail.CurrencyTypeValue.Guid;
            }

            return new FinancialPersonSavedAccountBag
            {
                IdKey = entity.IdKey,
                Name = entity.Name,
                AccountNumberMasked = entity.FinancialPaymentDetail?.AccountNumberMasked,
                Description = entity.FinancialPaymentDetail?.GetDescription(),
                ExpirationDate = entity.FinancialPaymentDetail?.ExpirationDate,
                ImageSource = entity.FinancialPaymentDetail?.GetCreditCardImageSource(),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsUsedInScheduledTransaction = IsSavedAccountInUse( entity ),
                Guid = entity.Guid
            };
        }

        /// <summary>
        /// Checks if the specified saved account is currently in use by any scheduled transactions.
        /// </summary>
        /// <param name="entity">The financial person saved account to check.</param>
        /// <returns>
        /// <c>true</c> if the saved account is in use by one or more scheduled transactions; otherwise, <c>false</c>.
        /// </returns>
        private bool IsSavedAccountInUse( FinancialPersonSavedAccount entity )
        {
            if ( entity == null )
            {
                return false;
            }

            // Check if there are any scheduled transactions that are using this saved account.
            var scheduledTransactionService = new FinancialScheduledTransactionService( RockContext );
            var scheduledTransactionQuery = scheduledTransactionService
                .Queryable()
                .Where( st => st.FinancialPaymentDetail.FinancialPersonSavedAccountId == entity.Id );

            return scheduledTransactionQuery.Any();
        }

        /// <inheritdoc/>
        protected override FinancialPersonSavedAccountBag GetEntityBagForView( FinancialPersonSavedAccount entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            if ( entity.Attributes == null )
            {
                entity.LoadAttributes( RockContext );
            }

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        //// <inheritdoc/>
        protected override FinancialPersonSavedAccountBag GetEntityBagForEdit( FinancialPersonSavedAccount entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            if ( entity.Attributes == null )
            {
                entity.LoadAttributes( RockContext );
            }

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( FinancialPersonSavedAccount entity, ValidPropertiesBox<FinancialPersonSavedAccountBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override FinancialPersonSavedAccount GetInitialEntity()
        {
            return GetInitialEntity<FinancialPersonSavedAccount, FinancialPersonSavedAccountService>( RockContext, PageParameterKey.FinancialPersonSavedAccount );
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
        protected override bool TryGetEntityForEditAction( string idKey, out FinancialPersonSavedAccount entity, out BlockActionResult error )
        {
            var entityService = new FinancialPersonSavedAccountService( RockContext );
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
                entity = new FinancialPersonSavedAccount();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{FinancialPersonSavedAccount.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${FinancialPersonSavedAccount.FriendlyTypeName}." );
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
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<FinancialPersonSavedAccountBag>
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
        public BlockActionResult Save( ValidPropertiesBox<FinancialPersonSavedAccountBag> box )
        {
            var entityService = new FinancialPersonSavedAccountService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidateFinancialPersonSavedAccount( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
            } );

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.FinancialPersonSavedAccount] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<FinancialPersonSavedAccountBag>
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
            var entityService = new FinancialPersonSavedAccountService( RockContext );

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
