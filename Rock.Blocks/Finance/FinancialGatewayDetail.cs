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
    public class FinancialGatewayDetail : RockDetailBlockType
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
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<FinancialGatewayBag, FinancialGatewayDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<FinancialGateway>();

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
        private FinancialGatewayDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
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
        private bool ValidateFinancialGateway( FinancialGateway financialGateway, RockContext rockContext, out string errorMessage )
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
        private void SetBoxInitialEntityState( DetailBlockBox<FinancialGatewayBag, FinancialGatewayDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {FinancialGateway.FriendlyTypeName} was not found.";
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( FinancialGateway.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( FinancialGateway.FriendlyTypeName );
                }
            }
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
                Description = entity.Description.IsNullOrWhiteSpace() ? entity.GetGatewayComponent()?.Description : entity.Description,
                EntityType = ToGatewayTypeListItemBag( entity.EntityType ),
                IsActive = entity.IsActive,
                Name = entity.Name,
                BatchSchedule = entity.BatchDayOfWeek.HasValue ? BatchWeekly : BatchDaily,
                BatchStartDay = entity.BatchDayOfWeek?.ToString( "D" ),
                InactiveGatewayNotificationMessage = !entity.IsActive ? GetInactiveNotificationMessage( entity ) : null,
            };
        }

        /// <summary>
        /// Converts the EntityType to ListItemBag with the corresponding ComponentName.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        private ListItemBag ToGatewayTypeListItemBag( EntityType entityType )
        {
            if ( entityType == null )
            {
                return null;
            }

            var componentEntityType = EntityTypeCache.Get( entityType.Guid );
            var componentName = Rock.Reflection.GetDisplayName( componentEntityType.GetEntityType() );

            // If it has a DisplayName use it as is, otherwise use the original logic
            if ( string.IsNullOrWhiteSpace( componentName ) )
            {
                componentName = entityType.FriendlyName;
                // If the component name already has a space then trust
                // that they are using the exact name formatting they want.
                if ( !componentName.Contains( ' ' ) )
                {
                    componentName = componentName.SplitCase();
                }
            }

            return new ListItemBag()
            {
                Text = componentName,
                Value = entityType.Guid.ToString(),
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="FinancialGatewayBag"/> that represents the entity.</returns>
        private FinancialGatewayBag GetEntityBagForView( FinancialGateway entity )
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
        /// <returns>A <see cref="FinancialGatewayBag"/> that represents the entity.</returns>
        private FinancialGatewayBag GetEntityBagForEdit( FinancialGateway entity )
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

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( FinancialGateway entity, DetailBlockBox<FinancialGatewayBag, FinancialGatewayDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.BatchTimeOffsetTicks ),
                () => entity.BatchTimeOffsetTicks = GetTimespan( box.Entity.BatchTimeOffsetTicks ) );

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.EntityType ),
                () => entity.EntityTypeId = box.Entity.EntityType.GetEntityId<EntityType>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => entity.IsActive = box.Entity.IsActive );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( rockContext );

                    entity.SetPublicAttributeValues( box.Entity.AttributeValues, RequestContext.CurrentPerson );
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

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="FinancialGateway"/> to be viewed or edited on the page.</returns>
        private FinancialGateway GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<FinancialGateway, FinancialGatewayService>( rockContext, PageParameterKey.FinancialGatewayId );
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
        private string GetSecurityGrantToken( FinancialGateway entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out FinancialGateway entity, out BlockActionResult error )
        {
            var entityService = new FinancialGatewayService( rockContext );
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

        /// <summary>
        /// Gets the notification message for inactive gateways.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        private string GetInactiveNotificationMessage( FinancialGateway entity )
        {
            var message = "<strong>Important!</strong> An 'Inactive' status will prevent the gateway from being shown in the gateway picker for Registration templates if it is not already selected. An 'Inactive' status DOES NOT prevent charges from being processed for a registration where the gateway is already assigned.";

            if ( entity == null || entity.Id == 0 )
            {
                // This is a new gateway so show the message but don't bother looking for registrations using it.
                return message;
            }

            var activeRegistrations = new FinancialGatewayService( new RockContext() ).GetRegistrationTemplatesForGateway( entity.Id, false ).ToList();
            if ( !activeRegistrations.Any() )
            {
                // This gateway isn't used by any registrations so show the message but don't bother looking for registrations using it.
                return message;
            }

            var registrationNames = " To prevent this choose a different payment gateway for these registrations: <b>'" + string.Join( "', '", activeRegistrations.Select( r => r.Name ) ).Trim().TrimEnd( ',' ) + "'</b>";
            message += registrationNames;

            return message;
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

                var box = new DetailBlockBox<FinancialGatewayBag, FinancialGatewayDetailOptionsBag>
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
        public BlockActionResult Save( DetailBlockBox<FinancialGatewayBag, FinancialGatewayDetailOptionsBag> box )
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

                var isWeekly = box.Entity.BatchSchedule == BatchWeekly;
                if ( isWeekly && int.TryParse( box.Entity.BatchStartDay, out int intValue ) && Enum.IsDefined( typeof( DayOfWeek ), intValue ) )
                {
                    entity.BatchDayOfWeek = ( DayOfWeek ) intValue;
                }
                else
                {
                    entity.BatchDayOfWeek = null;
                }

                // Ensure everything is valid before saving.
                if ( !ValidateFinancialGateway( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );
                } );

                return ActionOk( this.GetParentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.FinancialGatewayId] = entity.IdKey
                } ) );
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
                var entityService = new FinancialGatewayService( rockContext );

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
        /// Gets the message to show the user if they choose to deactivate a gateway. The message is generated here so we can include the registrations that will be affected.
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetInactiveGatewayMessage()
        {
            using ( var rockContext = new RockContext() )
            {
                var entity = GetInitialEntity( rockContext );

                var inactiveGatewayMessage = GetInactiveNotificationMessage( entity );

                return ActionOk( new { inactiveGatewayMessage = inactiveGatewayMessage } );
            }
        }

        /// <summary>
        /// Gets the gateway component description.
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetGatewayComponentDescription( Guid? entityTypeGuid )
        {
            var description = string.Empty;
            if ( entityTypeGuid.HasValue )
            {
                var entityType = EntityTypeCache.Get( entityTypeGuid.Value );
                if ( entityType != null )
                {
                    var component = Rock.Financial.GatewayContainer.GetComponent( entityType.Name );
                    description = component?.Description;
                }
            }

            return ActionOk( new { description = description });
        }

        /// <summary>
        /// Refreshes the list of attributes that can be displayed for editing
        /// purposes based on any modified values on the entity.
        /// </summary>
        /// <param name="box">The box that contains all the information about the entity being edited.</param>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public BlockActionResult RefreshAttributes( DetailBlockBox<FinancialGatewayBag, FinancialGatewayDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<FinancialGatewayBag, FinancialGatewayDetailOptionsBag>
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

        #endregion
    }
}
