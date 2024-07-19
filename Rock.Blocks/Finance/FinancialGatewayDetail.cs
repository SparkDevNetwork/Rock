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
using Rock.ViewModels.Blocks.Finance.FinancialGatewayDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Displays the details of a particular financial gateway.
    /// </summary>

    [DisplayName( "Gateway Detail" )]
    [Category( "Finance" )]
    [Description( "Displays the details of the given financial gateway." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "68cc9376-8123-4749-aca0-1e7ed8459704" )]
    [Rock.SystemGuid.BlockTypeGuid( "c12c615c-384d-478e-892d-0f353e2ef180" )]
    public class FinancialGatewayDetail : RockEntityDetailBlockType<FinancialGateway, FinancialGatewayBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string FinancialGatewayId = "GatewayId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        private static string BatchDaily = "Daily";
        private static string BatchWeekly = "Weekly";

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<FinancialGatewayBag, FinancialGatewayDetailOptionsBag>();

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
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private FinancialGatewayDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new FinancialGatewayDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the FinancialGateway for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="financialGateway">The FinancialGateway to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the FinancialGateway is valid, <c>false</c> otherwise.</returns>
        private bool ValidateFinancialGateway( FinancialGateway financialGateway, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<FinancialGatewayBag, FinancialGatewayDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {FinancialGateway.FriendlyTypeName} was not found.";
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( FinancialGateway.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( FinancialGateway.FriendlyTypeName );
                }
            }
            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="FinancialGatewayBag"/> that represents the entity.</returns>
        private FinancialGatewayBag GetCommonEntityBag( FinancialGateway entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new FinancialGatewayBag
            {
                IdKey = entity.IdKey,
                BatchTimeOffsetTicks = entity.GetBatchTimeOffset().ToString(),
                Description = entity.Description,
                EntityType = entity.EntityType.ToListItemBag(),
                IsActive = entity.IsActive,
                Name = entity.Name,
                BatchSchedule = entity.BatchDayOfWeek.HasValue ? BatchWeekly : BatchDaily,
                BatchStartDay = entity.BatchDayOfWeek?.ToString( "D" )
            };
        }

        /// <inheritdoc/>
        protected override FinancialGatewayBag GetEntityBagForView( FinancialGateway entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <inheritdoc/>
        protected override FinancialGatewayBag GetEntityBagForEdit( FinancialGateway entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, attributeFilter: IsAttributeIncluded );

            return bag;
        }

        /// <summary>
        /// Ensures the order and active attributes are not included in the attributes for edit.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <returns>
        ///   <c>true</c> if [is attribute included] [the specified attribute]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsAttributeIncluded( AttributeCache attribute )
        {
            return attribute.Key != "Order" && attribute.Key != "Active";
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( FinancialGateway entity, ValidPropertiesBox<FinancialGatewayBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.BatchTimeOffsetTicks ),
                () => entity.BatchTimeOffsetTicks = GetTimespan( box.Bag.BatchTimeOffsetTicks ) );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.EntityType ),
                () => entity.EntityTypeId = box.Bag.EntityType.GetEntityId<EntityType>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

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

        private long GetTimespan( string batchTimeOffsetTicks )
        {
            if ( TimeSpan.TryParse( batchTimeOffsetTicks, out TimeSpan batchTimeOffset ) )
            {
                return batchTimeOffset.Ticks;
            }
            else
            {
                return 0;
            }
        }

        /// <inheritdoc/>
        protected override FinancialGateway GetInitialEntity()
        {
            return GetInitialEntity<FinancialGateway, FinancialGatewayService>( RockContext, PageParameterKey.FinancialGatewayId );
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
        protected override bool TryGetEntityForEditAction( string idKey, out FinancialGateway entity, out BlockActionResult error )
        {
            var entityService = new FinancialGatewayService( RockContext );
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
                entity = new FinancialGateway();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{FinancialGateway.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${FinancialGateway.FriendlyTypeName}." );
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

            return ActionOk( new ValidPropertiesBox<FinancialGatewayBag>
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
        public BlockActionResult Save( ValidPropertiesBox<FinancialGatewayBag> box )
        {
            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            var isWeekly = box.Bag.BatchSchedule == BatchWeekly;
            if ( isWeekly && int.TryParse( box.Bag.BatchStartDay, out int intValue ) && Enum.IsDefined( typeof( DayOfWeek ), intValue ) )
            {
                entity.BatchDayOfWeek = ( DayOfWeek ) intValue;
            }
            else
            {
                entity.BatchDayOfWeek = null;
            }

            // Ensure everything is valid before saving.
            if ( !ValidateFinancialGateway( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
            } );

            return ActionOk( this.GetParentPageUrl( new Dictionary<string, string>
            {
                [PageParameterKey.FinancialGatewayId] = entity.IdKey
            } ) );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new FinancialGatewayService( RockContext );

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

        /// <summary>
        /// Gets the message to show the user if they choose to deactivate a gateway. The message is generated here so we can include the registrations that will be affected.
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetInactiveGatewayMessage()
        {
            var entity = GetInitialEntity();

            var message = "An 'Inactive' status will prevent the gateway from being shown in the gateway picker for Registration templates if it is not already selected. An 'Inactive' status DOES NOT prevent charges from being processed for a registration where the gateway is already assigned.";

            if ( entity == null || entity.Id == 0 )
            {
                // This is a new gateway so show the message but don't bother looking for registrations using it.
                return ActionOk( new { inactiveGatewayMessage = message } );
            }

            var activeRegistrations = new FinancialGatewayService( new RockContext() ).GetRegistrationTemplatesForGateway( entity.Id, false ).ToList();
            if ( !activeRegistrations.Any() )
            {
                // This gateway isn't used by any registrations so show the message but don't bother looking for registrations using it.
                return ActionOk( new { inactiveGatewayMessage = message } );
            }

            var registrationNames = " To prevent this choose a different payment gateway for these registrations: <b>'" + string.Join( "', '", activeRegistrations.Select( r => r.Name ) ).Trim().TrimEnd( ',' ) + "'</b>";
            message += registrationNames;

            return ActionOk( new { inactiveGatewayMessage = message } );
        }

        #endregion
    }
}
