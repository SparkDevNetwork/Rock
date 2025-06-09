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

using Microsoft.Extensions.DependencyInjection;

using Rock.Attribute;
using Rock.Constants;
using Rock.Core.Automation;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.AutomationTriggerDetail;
using Rock.ViewModels.Core.Automation;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays the details of a particular automation trigger.
    /// </summary>

    [DisplayName( "Automation Trigger Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of a particular automation trigger." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "d23d35db-2eee-4301-bb3b-21ae0aa7987f" )]
    [Rock.SystemGuid.BlockTypeGuid( "a4a91333-9ff7-4e93-b9ae-15daaf7ae185" )]
    public class AutomationTriggerDetail : RockEntityDetailBlockType<AutomationTrigger, AutomationTriggerBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string AutomationTriggerId = "AutomationTriggerId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// The container for the automation trigger components.
        /// </summary>
        private readonly AutomationTriggerContainer _triggerContainer;

        /// <summary>
        /// The container for the automation event components.
        /// </summary>
        private readonly AutomationEventContainer _eventContainer;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationTriggerDetail"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to get dependency injection services from.</param>
        public AutomationTriggerDetail( IServiceProvider serviceProvider )
        {
            _triggerContainer = serviceProvider.GetRequiredService<AutomationTriggerContainer>();
            _eventContainer = serviceProvider.GetRequiredService<AutomationEventContainer>();
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<AutomationTriggerBag, AutomationTriggerDetailOptionsBag>();

            _triggerContainer.RegisterComponents( RockContext );
            _eventContainer.RegisterComponents( RockContext );

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
        private AutomationTriggerDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new AutomationTriggerDetailOptionsBag();

            options.TriggerTypeItems = _triggerContainer.GetComponentTypes( RockContext )
                .ToListItemBagList();
            options.EventTypeItems = _eventContainer.GetComponentTypes( RockContext )
                .ToListItemBagList();

            return options;
        }

        /// <summary>
        /// Validates the AutomationTrigger for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="automationTrigger">The AutomationTrigger to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the AutomationTrigger is valid, <c>false</c> otherwise.</returns>
        private bool ValidateAutomationTrigger( AutomationTrigger automationTrigger, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<AutomationTriggerBag, AutomationTriggerDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {AutomationTrigger.FriendlyTypeName} was not found.";
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( AutomationTrigger.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( AutomationTrigger.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="AutomationTriggerBag"/> that represents the entity.</returns>
        private AutomationTriggerBag GetCommonEntityBag( AutomationTrigger entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new AutomationTriggerBag
            {
                IdKey = entity.IdKey,
                Description = entity.Description,
                IsActive = entity.IsActive,
                Name = entity.Name
            };

            if ( entity.ComponentEntityTypeId != 0 )
            {
                var componentEntityType = EntityTypeCache.Get( entity.ComponentEntityTypeId, RockContext );

                bag.TriggerType = new ListItemBag
                {
                    Value = componentEntityType?.Guid.ToString(),
                    Text = componentEntityType.FriendlyName
                };
            }

            return bag;
        }

        /// <inheritdoc/>
        protected override AutomationTriggerBag GetEntityBagForView( AutomationTrigger entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var component = _triggerContainer.CreateInstance( entity, RockContext );
            var configurationValues = entity.ComponentConfigurationJson
                ?.FromJsonOrNull<Dictionary<string, string>>()
                ?? new Dictionary<string, string>();

            var bag = GetCommonEntityBag( entity );

            bag.Events = entity.AutomationEvents
                .OrderBy( e => e.Order )
                .ThenBy( e => e.Id )
                .Select( GetEventBagForView )
                .ToList();

            bag.ValueDefinitions = component
                ?.GetValueTypes( configurationValues, RockContext )
                .Select( ToValueDefinitionBag )
                .ToList()
                ?? new List<AutomationValueDefinitionBag>();

            bag.ConfigurationDetails = component
                ?.GetConfigurationDetails( configurationValues, RockContext )
                .ToList()
                ?? new List<ListItemBag>();

            return bag;
        }

        //// <inheritdoc/>
        protected override AutomationTriggerBag GetEntityBagForEdit( AutomationTrigger entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var component = _triggerContainer.CreateInstance( entity, RockContext );
            var configurationValues = entity.ComponentConfigurationJson
                ?.FromJsonOrNull<Dictionary<string, string>>()
                ?? new Dictionary<string, string>();

            var bag = GetCommonEntityBag( entity );

            bag.ComponentDefinition = component
                ?.GetComponentDefinition( configurationValues, RockContext, RequestContext );
            bag.ComponentConfiguration = component
                ?.GetPublicConfiguration( configurationValues, RockContext, RequestContext );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( AutomationTrigger entity, ValidPropertiesBox<AutomationTriggerBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.TriggerType ),
                () => entity.ComponentEntityTypeId = EntityTypeCache.Get( box.Bag.TriggerType.Value.AsGuid(), RockContext )?.Id ?? 0 );

            box.IfValidProperty( nameof( box.Bag.ComponentConfiguration ),
                () =>
                {
                    var component = _triggerContainer.CreateInstance( entity, RockContext );

                    entity.ComponentConfigurationJson = component
                        ?.GetPrivateConfiguration( box.Bag.ComponentConfiguration, RockContext, RequestContext )
                        ?.ToJson();
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override AutomationTrigger GetInitialEntity()
        {
            return GetInitialEntity<AutomationTrigger, AutomationTriggerService>( RockContext, PageParameterKey.AutomationTriggerId );
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
        protected override bool TryGetEntityForEditAction( string idKey, out AutomationTrigger entity, out BlockActionResult error )
        {
            var entityService = new AutomationTriggerService( RockContext );
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
                entity = new AutomationTrigger();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{AutomationTrigger.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${AutomationTrigger.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Converts a value definition to a bag that can be sent down to the
        /// client for display and filtering.
        /// </summary>
        /// <param name="valueDefinition">The definition of the value to be returned.</param>
        /// <returns>An instance of <see cref="AutomationValueDefinitionBag"/> that represents <paramref name="valueDefinition"/>.</returns>
        private AutomationValueDefinitionBag ToValueDefinitionBag( AutomationValueDefinition valueDefinition )
        {
            return new AutomationValueDefinitionBag
            {
                Key = valueDefinition.Key,
                Description = valueDefinition.Description,
                TypeName = valueDefinition.Type.FullName
            };
        }

        #endregion

        #region Automation Event Methods

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="AutomationEventBag"/> that represents the entity.</returns>
        private AutomationEventBag GetCommonEventBag( AutomationEvent entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new AutomationEventBag
            {
                IdKey = entity.IdKey,
                IsActive = entity.IsActive
            };

            if ( entity.ComponentEntityTypeId != 0 )
            {
                var componentEntityType = EntityTypeCache.Get( entity.ComponentEntityTypeId, RockContext );

                bag.EventType = new ListItemBag
                {
                    Value = componentEntityType?.Guid.ToString(),
                    Text = componentEntityType.FriendlyName
                };
            }

            return bag;
        }

        /// <inheritdoc cref="GetEntityBagForView(AutomationTrigger)"/>
        private AutomationEventBag GetEventBagForView( AutomationEvent entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEventBag( entity );

            if ( entity.ComponentEntityTypeId != 0 )
            {
                var component = _eventContainer.CreateInstance( entity, RockContext );
                var componentEntityType = EntityTypeCache.Get( entity.ComponentEntityTypeId, RockContext );
                var configurationValues = entity.ComponentConfigurationJson
                    ?.FromJsonOrNull<Dictionary<string, string>>()
                    ?? new Dictionary<string, string>();

                bag.Name = component?.GetEventName( configurationValues, RockContext ) ?? componentEntityType.Name;
                bag.Description = component?.GetEventDescription( configurationValues, RockContext ) ?? string.Empty;
                bag.IconCssClass = component?.IconCssClass ?? "ti ti-question-mark";
            }

            return bag;
        }

        /// <inheritdoc cref="GetEntityBagForEdit(AutomationTrigger)"/>
        private AutomationEventBag GetEventBagForEdit( AutomationEvent entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var component = _eventContainer.CreateInstance( entity, RockContext );
            var configurationValues = entity.ComponentConfigurationJson
                ?.FromJsonOrNull<Dictionary<string, string>>()
                ?? new Dictionary<string, string>();

            var bag = GetCommonEventBag( entity );

            bag.ComponentDefinition = component
                ?.GetComponentDefinition( configurationValues, RockContext, RequestContext );
            bag.ComponentConfiguration = component
                ?.GetPublicConfiguration( configurationValues, RockContext, RequestContext );

            return bag;
        }

        /// <inheritdoc cref="UpdateEntityFromBox(AutomationTrigger, ValidPropertiesBox{AutomationTriggerBag})"/>
        private bool UpdateEventFromBox( AutomationEvent entity, ValidPropertiesBox<AutomationEventBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.EventType ),
                () => entity.ComponentEntityTypeId = EntityTypeCache.Get( box.Bag.EventType.Value.AsGuid(), RockContext )?.Id ?? 0 );

            box.IfValidProperty( nameof( box.Bag.ComponentConfiguration ),
                () =>
                {
                    var component = _eventContainer.CreateInstance( entity, RockContext );

                    entity.ComponentConfigurationJson = component
                        ?.GetPrivateConfiguration( box.Bag.ComponentConfiguration, RockContext, RequestContext )
                        ?.ToJson();
                } );

            return true;
        }

        /// Validates the AutomationEvent for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="automationEvent">The AutomationEvent to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the AutomationEvent is valid, <c>false</c> otherwise.</returns>
        private bool ValidateAutomationEvent( AutomationEvent automationEvent, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <inheritdoc cref="TryGetEntityForEditAction(string, out AutomationTrigger, out BlockActionResult)"/>
        private bool TryGetEventForEditAction( string triggerIdKey, string idKey, out AutomationEvent entity, out BlockActionResult error )
        {
            var entityService = new AutomationEventService( RockContext );
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
                entity = new AutomationEvent
                {
                    AutomationTriggerId = AutomationTriggerCache.GetByIdKey( triggerIdKey, RockContext )?.Id ?? 0,
                    IsActive = true
                };
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{AutomationEvent.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${AutomationEvent.FriendlyTypeName}." );
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

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<AutomationTriggerBag>
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
        public BlockActionResult Save( ValidPropertiesBox<AutomationTriggerBag> box )
        {
            var entityService = new AutomationTriggerService( RockContext );

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
            if ( !ValidateAutomationTrigger( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
            } );

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.AutomationTriggerId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );

            var bag = GetEntityBagForView( entity );

            return ActionOk( new ValidPropertiesBox<AutomationTriggerBag>
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
            var entityService = new AutomationTriggerService( RockContext );

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
        /// Gets the <see cref="DynamicComponentDefinitionBag"/> that describes
        /// the UI component that will handle the configuration of the trigger.
        /// </summary>
        /// <param name="componentGuid">The unique identifier of the trigger component.</param>
        /// <returns>A bag that contains the component definition.</returns>
        [BlockAction]
        public BlockActionResult GetComponentDefinition( Guid componentGuid )
        {
            var component = _triggerContainer.CreateInstance( componentGuid );
            var definition = component?.GetComponentDefinition( new Dictionary<string, string>(), RockContext, RequestContext );

            var bag = new GetComponentDefinitionResponseBag
            {
                ComponentDefinition = definition,
                ComponentConfiguration = component?.GetPublicConfiguration( new Dictionary<string, string>(), RockContext, RequestContext )
            };

            return ActionOk( bag );
        }

        /// <summary>
        /// Executes a request from the UI component to be processed by the
        /// server component. This is used to load additional information after
        /// the component has been initialized.
        /// </summary>
        /// <param name="componentGuid">The unique identifier of the component that will handle the request.</param>
        /// <param name="request">The object that describes the parameters of the request.</param>
        /// <param name="securityGrantToken">The security grant token that was created when the component was initialized.</param>
        /// <returns>The response from the server component.</returns>
        [BlockAction]
        public BlockActionResult ExecuteComponentRequest( Guid componentGuid, Dictionary<string, string> request, string securityGrantToken )
        {
            var securityGrant = SecurityGrant.FromToken( securityGrantToken ) ?? new SecurityGrant();
            var component = _triggerContainer.CreateInstance( componentGuid );

            var result = component?.ExecuteComponentRequest( request, securityGrant, RockContext, RequestContext );

            return ActionOk( result );
        }

        #endregion

        #region Event Block Actions

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the add new operation.
        /// </summary>
        /// <param name="triggerKey">The identifier of the trigger the event will be attached to.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult AddEvent( string triggerKey )
        {
            if ( !TryGetEventForEditAction( triggerKey, null, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var bag = GetEventBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<AutomationEventBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult EditEvent( string key )
        {
            if ( !TryGetEventForEditAction( null, key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var bag = GetEventBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<AutomationEventBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="triggerKey">The identifier of the trigger the event will be attached to.</param>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult SaveEvent( string triggerKey, ValidPropertiesBox<AutomationEventBag> box )
        {
            var entityService = new AutomationEventService( RockContext );

            if ( !TryGetEventForEditAction( triggerKey, box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEventFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidateAutomationEvent( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            // If this is a new event, then set the default order to position it
            // at the end of the event list.
            if ( entity.Id == 0 )
            {
                var lastOrder = entityService.Queryable()
                    .Where( e => e.AutomationTriggerId == entity.AutomationTriggerId )
                    .Select( e => ( int? ) e.Order )
                    .Max() ?? 0;

                entity.Order = lastOrder + 1;
            }

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
            } );

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );

            var bag = GetEventBagForView( entity );

            return ActionOk( new ValidPropertiesBox<AutomationEventBag>
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
        public BlockActionResult DeleteEvent( string key )
        {
            var entityService = new AutomationEventService( RockContext );

            if ( !TryGetEventForEditAction( null, key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Changes the ordered position of a single item.
        /// </summary>
        /// <param name="key">The identifier of the item that will be moved.</param>
        /// <param name="beforeKey">The identifier of the item it will be placed before.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult ReorderEvent( string key, string beforeKey )
        {
            var eventCache = AutomationEventCache.GetByIdKey( key, RockContext );

            if ( eventCache == null )
            {
                return ActionBadRequest( "That event was not found." );
            }

            // Get the queryable and make sure it is ordered correctly.
            var items = new AutomationEventService( RockContext )
                .Queryable()
                .Where( e => e.AutomationTriggerId == eventCache.AutomationTriggerId )
                .OrderBy( e => e.Order )
                .ThenBy( e => e.Id )
                .ToList();

            if ( !items.ReorderEntity( key, beforeKey ) )
            {
                return ActionBadRequest( "Invalid reorder attempt." );
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Gets the <see cref="DynamicComponentDefinitionBag"/> that describes
        /// the UI component that will handle the configuration of the event.
        /// </summary>
        /// <param name="componentGuid">The unique identifier of the event component.</param>
        /// <returns>A bag that contains the component definition.</returns>
        [BlockAction]
        public BlockActionResult GetEventComponentDefinition( Guid componentGuid )
        {
            var component = _eventContainer.CreateInstance( componentGuid );
            var definition = component?.GetComponentDefinition( new Dictionary<string, string>(), RockContext, RequestContext );

            var bag = new GetComponentDefinitionResponseBag
            {
                ComponentDefinition = definition,
                ComponentConfiguration = component?.GetPublicConfiguration( new Dictionary<string, string>(), RockContext, RequestContext )
            };

            return ActionOk( bag );
        }

        /// <summary>
        /// Executes a request from the UI component to be processed by the
        /// server component. This is used to load additional information after
        /// the component has been initialized.
        /// </summary>
        /// <param name="componentGuid">The unique identifier of the component that will handle the request.</param>
        /// <param name="request">The object that describes the parameters of the request.</param>
        /// <param name="securityGrantToken">The security grant token that was created when the component was initialized.</param>
        /// <returns>The response from the server component.</returns>
        [BlockAction]
        public BlockActionResult ExecuteEventComponentRequest( Guid componentGuid, Dictionary<string, string> request, string securityGrantToken )
        {
            var securityGrant = SecurityGrant.FromToken( securityGrantToken ) ?? new SecurityGrant();
            var component = _eventContainer.CreateInstance( componentGuid );

            var result = component?.ExecuteComponentRequest( request, securityGrant, RockContext, RequestContext );

            return ActionOk( result );
        }

        #endregion
    }
}
