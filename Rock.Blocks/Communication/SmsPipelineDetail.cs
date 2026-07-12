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
using System.Data.Entity.Infrastructure;
using System.Linq;

using Rock.Attribute;
using Rock.Communication;
using Rock.Communication.Medium;
using Rock.Communication.SmsActions;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.Communication.SmsPipelineDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Configures the pipeline that processes an incoming SMS message.
    /// </summary>

    [DisplayName( "SMS Pipeline Detail" )]
    [Category( "Communication" )]
    [Description( "Configures the pipeline that processes an incoming SMS message." )]
    [IconCssClass( "ti ti-device-mobile-message" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "B4ECD5B6-6F61-4B49-8FF2-C4F03C5A9F4F" )]
    [Rock.SystemGuid.BlockTypeGuid( "44C32EB7-4DA3-4577-AC41-E3517442E269" )]
    public class SmsPipelineDetail : RockBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string SmsPipelineId = "SmsPipelineId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        private static readonly string[] SystemAttributeKeys = { "Order", "Active" };

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new SmsPipelineDetailInitializationBox
            {
                FilterCategoryName = SmsActionComponent.BaseAttributeCategories.Filters,
                SystemAttributeKeys = SystemAttributeKeys.ToList(),
                NavigationUrls = GetBoxNavigationUrls(),
                AvailableComponents = GetAvailableComponents()
            };

            var currentPerson = GetCurrentPerson();
            var canEdit = BlockCache.IsAuthorized( Authorization.EDIT, currentPerson );
            var canAdministrate = BlockCache.IsAuthorized( Authorization.ADMINISTRATE, currentPerson );

            box.IsEditable = canEdit;
            box.IsTestingEnabled = canAdministrate;

            var pipeline = GetPipeline( RockContext, trackChanges: false );

            if ( pipeline == null )
            {
                box.ErrorMessage = $"The {SmsPipeline.FriendlyTypeName} was not found.";
                return box;
            }

            box.Pipeline = GetPipelineBag( pipeline );

            return box;
        }

        /// <summary>
        /// Resolves the pipeline from the SmsPipelineId page parameter.
        /// </summary>
        /// <remarks>
        /// Returns a transient new pipeline (with IsActive = true) when no id is
        /// present, so the block can render in create mode. Returns null only when
        /// an id is present but does not resolve to an existing row.
        /// </remarks>
        private SmsPipeline GetPipeline( RockContext rockContext, bool trackChanges )
        {
            var idKey = PageParameter( PageParameterKey.SmsPipelineId );

            if ( idKey.IsNullOrWhiteSpace() || idKey == "0" )
            {
                return new SmsPipeline { IsActive = true };
            }

            var service = new SmsPipelineService( rockContext );
            var query = service.Queryable().Include( p => p.SmsActions );

            if ( !trackChanges )
            {
                query = query.AsNoTracking();
            }

            var arePredictableIdsEnabled = !PageCache.Layout.Site.DisablePredictableIds;
            var id = service.GetSelect( idKey, p => ( int? ) p.Id, arePredictableIdsEnabled );

            if ( id == null )
            {
                return null;
            }

            return query.FirstOrDefault( p => p.Id == id.Value );
        }

        /// <summary>
        /// Builds the bag for a pipeline, including its ordered actions and
        /// per-action attribute values.
        /// </summary>
        /// <remarks>New pipelines (Id == 0) ship with an empty actions list.</remarks>
        private SmsPipelineBag GetPipelineBag( SmsPipeline pipeline )
        {
            var bag = new SmsPipelineBag
            {
                IdKey = pipeline.IdKey,
                Name = pipeline.Name,
                Description = pipeline.Description,
                IsActive = pipeline.IsActive,
                WebhookUrl = GetWebhookUrl( pipeline ),
                Actions = new List<SmsActionBag>()
            };

            if ( pipeline.Id == 0 )
            {
                return bag;
            }

            var orderedActions = pipeline.SmsActions
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Id )
                .ToList();

            /*
                5/28/26 - JMH

                Load attributes up front so it is obvious this happens in a
                loop. Each call resolves from AttributeCache, so the cost on a
                warm cache is small.

                Reason: Keep the per-action attribute load visible at the call
                site rather than buried inside GetActionBag.
            */
            foreach ( var action in orderedActions )
            {
                action.LoadAttributes( RockContext );
            }

            foreach ( var action in orderedActions )
            {
                bag.Actions.Add( GetActionBag( action ) );
            }

            return bag;
        }

        /// <summary>
        /// Builds a bag view of a single configured SMS action.
        /// </summary>
        /// <remarks>
        /// Callers iterating multiple actions should invoke <c>LoadAttributes</c>
        /// on each action up front so the per-action calls remain visible in
        /// the caller's loop.
        /// </remarks>
        private SmsActionBag GetActionBag( SmsAction action )
        {
            if ( action.Attributes == null )
            {
                action.LoadAttributes( RockContext );
            }

            var componentEntityType = EntityTypeCache.Get( action.SmsActionComponentEntityTypeId );

            return new SmsActionBag
            {
                IdKey = action.IdKey,
                Guid = action.Guid,
                Name = action.Name,
                IsActive = action.IsActive,
                ContinueAfterProcessing = action.ContinueAfterProcessing,
                ExpireDateTime = action.ExpireDate,
                IsInteractionLoggedAfterProcessing = action.IsInteractionLoggedAfterProcessing,
                Order = action.Order,
                ComponentEntityTypeGuid = componentEntityType?.Guid ?? Guid.Empty,
                AttributeValues = action.GetPublicAttributeValuesForEdit( null, enforceSecurity: false, attributeFilter: IsRenderedAttribute )
            };
        }

        /// <summary>
        /// Builds the editable bag for a single action, adding the per-instance
        /// attribute schema to the view bag.
        /// </summary>
        /// <param name="action">The action to represent.</param>
        /// <returns>A bag carrying the action's scalars, attribute schema, and current values.</returns>
        private SmsActionBag GetActionBagForEdit( SmsAction action )
        {
            var bag = GetActionBag( action );

            bag.Attributes = action.GetPublicAttributesForEdit( null, enforceSecurity: false, attributeFilter: IsRenderedAttribute );

            return bag;
        }

        /// <summary>
        /// Builds the list of SMS action component types offered in the action editor.
        /// </summary>
        /// <remarks>
        /// Each entry carries its per-instance attribute schema so the editor can
        /// render the form for a selected component without a per-selection server
        /// round-trip.
        /// </remarks>
        private static List<SmsActionComponentBag> GetAvailableComponents()
        {
            var components = new List<SmsActionComponentBag>();

            foreach ( var componentEntry in SmsActionContainer.Instance.Components )
            {
                var component = componentEntry.Value.Value;
                var componentEntityType = EntityTypeCache.Get( component.GetType() );

                if ( componentEntityType == null )
                {
                    continue;
                }

                components.Add( GetComponentBag( componentEntityType, component ) );
            }

            return components
                .OrderBy( c => c.Title )
                .ToList();
        }

        /// <summary>
        /// Builds the bag for a single SMS action component type, including its
        /// per-instance attribute schema and default values.
        /// </summary>
        /// <param name="componentEntityType">The component's entity type.</param>
        /// <param name="component">The component instance.</param>
        /// <returns>A bag describing the component type and the attribute schema a new action of this type renders.</returns>
        private static SmsActionComponentBag GetComponentBag( EntityTypeCache componentEntityType, SmsActionComponent component )
        {
            var probeAction = new SmsAction
            {
                SmsActionComponentEntityTypeId = componentEntityType.Id
            };

            probeAction.LoadAttributes( null );

            return new SmsActionComponentBag
            {
                EntityTypeGuid = componentEntityType.Guid,
                Title = component.Title,
                Description = component.Description,
                IconCssClass = component.IconCssClass,
                Attributes = probeAction.GetPublicAttributesForEdit( null, enforceSecurity: false, attributeFilter: IsRenderedAttribute ),
                DefaultAttributeValues = probeAction.GetPublicAttributeValuesForEdit( null, enforceSecurity: false, attributeFilter: IsRenderedAttribute )
            };
        }

        /// <summary>
        /// Filters attributes to the set rendered by the editor.
        /// </summary>
        /// <remarks>
        /// The SmsAction entity exposes Order and Active as both scalar properties and
        /// EAV attributes; the editor renders the scalar versions, so the EAV counterparts
        /// must be excluded.
        /// </remarks>
        private static bool IsRenderedAttribute( Rock.Web.Cache.AttributeCache attribute )
        {
            return !SystemAttributeKeys.Contains( attribute.Key );
        }

        /// <summary>
        /// Resolves the public webhook URL for the pipeline.
        /// </summary>
        /// <remarks>
        /// Composes the PublicApplicationRoot with the active SMS transport's webhook
        /// path. Returns null when the configured transport does not implement
        /// <see cref="ISmsPipelineWebhook"/> or when the pipeline is new.
        /// </remarks>
        private static string GetWebhookUrl( SmsPipeline pipeline )
        {
            if ( pipeline == null || pipeline.Id == 0 )
            {
                return null;
            }

            var smsMedium = new Sms();
            var smsTransport = smsMedium.Transport as ISmsPipelineWebhook;

            if ( smsTransport == null )
            {
                return null;
            }

            var publicAppRoot = GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ).TrimEnd( '/' );

            return $"{publicAppRoot}/{smsTransport.SmsPipelineWebhookPath}?{PageParameterKey.SmsPipelineId}={pipeline.Id}";
        }

        /// <summary>
        /// Gets the navigation URLs for the box.
        /// </summary>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        /// <summary>
        /// Resolves the action targeted by an add, edit, or save operation and
        /// verifies edit authorization.
        /// </summary>
        /// <param name="pipelineKey">The IdKey of the parent pipeline. Used only when creating a new action.</param>
        /// <param name="idKey">The IdKey of the action to load, or null to create a transient new action.</param>
        /// <param name="entity">On success, the resolved existing or transient action.</param>
        /// <param name="error">On failure, the result describing why the action could not be resolved or edited.</param>
        /// <returns><c>true</c> when the action was resolved and the current person may edit it.</returns>
        private bool TryGetActionForEditAction( string pipelineKey, string idKey, out SmsAction entity, out BlockActionResult error )
        {
            var actionService = new SmsActionService( RockContext );
            var arePredictableIdsEnabled = !PageCache.Layout.Site.DisablePredictableIds;
            error = null;

            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                entity = actionService.Get( idKey, arePredictableIdsEnabled );
            }
            else
            {
                var pipelineId = new SmsPipelineService( RockContext )
                    .GetSelect( pipelineKey, p => ( int? ) p.Id, arePredictableIdsEnabled );

                entity = new SmsAction
                {
                    SmsPipelineId = pipelineId ?? 0,
                    IsActive = true
                };

                actionService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( "Action not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) )
            {
                error = ActionBadRequest( $"Not authorized to edit {SmsPipeline.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Wraps an action bag in a valid-properties box for the editor.
        /// </summary>
        /// <param name="bag">The action bag to wrap.</param>
        /// <returns>A box marking every bag property as valid.</returns>
        private static ValidPropertiesBox<SmsActionBag> ToEditBox( SmsActionBag bag )
        {
            return new ValidPropertiesBox<SmsActionBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            };
        }

        /// <summary>
        /// Applies the editable scalar properties from a box onto an action.
        /// </summary>
        /// <param name="action">The action to update.</param>
        /// <param name="box">The box carrying the new values and the set of valid properties.</param>
        /// <returns><c>true</c> when the box carried valid properties to apply.</returns>
        /// <remarks>
        /// The component type is set only for a new action; an existing action's type is fixed.
        /// Attribute values are applied by the caller after the type drives <c>LoadAttributes</c>.
        /// </remarks>
        private bool UpdateActionFromBox( SmsAction action, ValidPropertiesBox<SmsActionBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => action.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => action.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.ContinueAfterProcessing ),
                () => action.ContinueAfterProcessing = box.Bag.ContinueAfterProcessing );

            box.IfValidProperty( nameof( box.Bag.ExpireDateTime ),
                () => action.ExpireDate = box.Bag.ExpireDateTime );

            box.IfValidProperty( nameof( box.Bag.IsInteractionLoggedAfterProcessing ),
                () => action.IsInteractionLoggedAfterProcessing = box.Bag.IsInteractionLoggedAfterProcessing );

            if ( action.Id == 0 )
            {
                box.IfValidProperty( nameof( box.Bag.ComponentEntityTypeGuid ),
                    () => action.SmsActionComponentEntityTypeId = EntityTypeCache.Get( box.Bag.ComponentEntityTypeGuid )?.Id ?? 0 );
            }

            return true;
        }

        /// <summary>
        /// Defaults a new action's name to its component title when no name was supplied.
        /// </summary>
        /// <param name="action">The action whose name should be ensured.</param>
        /// <remarks>
        /// The editor has no name field, so a new action takes the component's title;
        /// an action with an existing name is left untouched.
        /// </remarks>
        private static void EnsureActionName( SmsAction action )
        {
            if ( action.Name.IsNotNullOrWhiteSpace() )
            {
                return;
            }

            var componentEntityType = EntityTypeCache.Get( action.SmsActionComponentEntityTypeId );
            var component = componentEntityType != null
                ? SmsActionContainer.GetComponent( componentEntityType.Name )
                : null;

            if ( component != null )
            {
                action.Name = component.Title;
            }
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Persists the pipeline header scalars (Name, Description, IsActive).
        /// </summary>
        /// <param name="bag">The pipeline header state to commit. Any <c>Actions</c> on the bag are ignored; actions are managed by their own block actions.</param>
        /// <returns>
        /// 201 with the saved pipeline's URL when creating a new pipeline; 200 with the refreshed
        /// pipeline bag when updating an existing one.
        /// </returns>
        [BlockAction]
        public BlockActionResult Save( SmsPipelineBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest( "Pipeline data is required." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) )
            {
                return ActionBadRequest( $"Not authorized to edit {SmsPipeline.FriendlyTypeName}." );
            }

            var pipelineService = new SmsPipelineService( RockContext );
            var isNew = bag.IdKey.IsNullOrWhiteSpace();

            SmsPipeline pipeline;

            if ( isNew )
            {
                pipeline = new SmsPipeline();
                pipelineService.Add( pipeline );
            }
            else
            {
                var arePredictableIdsEnabled = !PageCache.Layout.Site.DisablePredictableIds;
                pipeline = pipelineService.Get( bag.IdKey, arePredictableIdsEnabled );

                if ( pipeline == null )
                {
                    return ActionBadRequest( $"{SmsPipeline.FriendlyTypeName} no longer exists." );
                }
            }

            pipeline.Name = bag.Name;
            pipeline.Description = bag.Description;
            pipeline.IsActive = bag.IsActive;

            RockContext.SaveChanges();

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.SmsPipelineId] = pipeline.IdKey
                } ) );
            }

            var refreshed = pipelineService.Queryable()
                .Include( p => p.SmsActions )
                .FirstOrDefault( p => p.Id == pipeline.Id );

            return ActionOk( GetPipelineBag( refreshed ) );
        }

        /// <summary>
        /// Begins adding a new action by returning a transient, unsaved action for the editor.
        /// </summary>
        /// <param name="pipelineKey">The IdKey of the parent pipeline the new action will belong to.</param>
        /// <returns>A box containing a transient action bag with no component type selected yet.</returns>
        [BlockAction]
        public BlockActionResult AddAction( string pipelineKey )
        {
            if ( !TryGetActionForEditAction( pipelineKey, null, out var action, out var actionError ) )
            {
                return actionError;
            }

            return ActionOk( ToEditBox( GetActionBagForEdit( action ) ) );
        }

        /// <summary>
        /// Gets the editable state for an existing action.
        /// </summary>
        /// <param name="key">The IdKey of the action to edit.</param>
        /// <returns>A box containing the action's scalars, attribute schema, and current values.</returns>
        [BlockAction]
        public BlockActionResult EditAction( string key )
        {
            if ( !TryGetActionForEditAction( null, key, out var action, out var actionError ) )
            {
                return actionError;
            }

            return ActionOk( ToEditBox( GetActionBagForEdit( action ) ) );
        }

        /// <summary>
        /// Gets the attribute schema and default values for a freshly chosen action component type.
        /// </summary>
        /// <param name="componentEntityTypeGuid">The entity type GUID of the selected SmsActionComponent.</param>
        /// <returns>The component's per-instance attribute schema and default values.</returns>
        [BlockAction]
        public BlockActionResult GetActionComponentEditDefinition( Guid componentEntityTypeGuid )
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) )
            {
                return ActionBadRequest( $"Not authorized to edit {SmsPipeline.FriendlyTypeName}." );
            }

            var componentEntityType = EntityTypeCache.Get( componentEntityTypeGuid );
            var component = componentEntityType != null
                ? SmsActionContainer.GetComponent( componentEntityType.Name )
                : null;

            if ( component == null )
            {
                return ActionBadRequest( $"Unknown SMS action component '{componentEntityTypeGuid}'." );
            }

            return ActionOk( GetComponentBag( componentEntityType, component ) );
        }

        /// <summary>
        /// Persists scalar and attribute-value edits for a new or existing action.
        /// </summary>
        /// <param name="pipelineKey">The IdKey of the parent pipeline. Used only when saving a new action.</param>
        /// <param name="box">The action state to commit. An empty <c>IdKey</c> inserts a new action at the end of the pipeline.</param>
        /// <returns>The refreshed action bag.</returns>
        [BlockAction]
        public BlockActionResult SaveAction( string pipelineKey, ValidPropertiesBox<SmsActionBag> box )
        {
            if ( box?.Bag == null )
            {
                return ActionBadRequest( "Action data is required." );
            }

            if ( !TryGetActionForEditAction( pipelineKey, box.Bag.IdKey, out var action, out var actionError ) )
            {
                return actionError;
            }

            var isNew = action.Id == 0;

            if ( !UpdateActionFromBox( action, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            if ( isNew )
            {
                var lastOrder = new SmsActionService( RockContext ).Queryable()
                    .Where( a => a.SmsPipelineId == action.SmsPipelineId )
                    .Select( a => ( int? ) a.Order )
                    .Max() ?? -1;

                action.Order = lastOrder + 1;

                EnsureActionName( action );
            }

            action.LoadAttributes( RockContext );

            if ( box.Bag.AttributeValues != null )
            {
                box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                    () => action.SetPublicAttributeValues( box.Bag.AttributeValues, GetCurrentPerson(), enforceSecurity: false ) );
            }

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                action.SaveAttributeValues( RockContext );
            } );

            return ActionOk( ToEditBox( GetActionBagForEdit( action ) ) );
        }

        /// <summary>
        /// Deletes a single action from its pipeline.
        /// </summary>
        /// <param name="actionIdKey">The IdKey of the action to delete.</param>
        [BlockAction]
        public BlockActionResult DeleteAction( string actionIdKey )
        {
            if ( actionIdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Action identifier is required." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) )
            {
                return ActionBadRequest( $"Not authorized to edit {SmsPipeline.FriendlyTypeName}." );
            }

            var actionService = new SmsActionService( RockContext );
            var arePredictableIdsEnabled = !PageCache.Layout.Site.DisablePredictableIds;
            var action = actionService.Get( actionIdKey, arePredictableIdsEnabled );

            /*
                6/1/26 - JMH

                Treat the delete as idempotent. The list editor can fire a second
                delete before the first round-trip settles, so an action that is
                already gone is a success rather than an error: it resolves to null
                here, or its row is removed between this load and SaveChanges (which
                EF surfaces as a DbUpdateConcurrencyException reporting 0 rows). The
                desired end state in both cases is "the action no longer exists."

                Reason: Quick successive deletes must not surface a concurrency error.
            */
            if ( action == null )
            {
                return ActionOk();
            }

            actionService.Delete( action );

            // Renumber the remaining actions so their order stays contiguous.
            var siblings = actionService.Queryable()
                .Where( a => a.SmsPipelineId == action.SmsPipelineId && a.Id != action.Id )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Id )
                .ToList();

            for ( var i = 0; i < siblings.Count; i++ )
            {
                siblings[i].Order = i;
            }

            try
            {
                RockContext.SaveChanges();
            }
            catch ( DbUpdateConcurrencyException )
            {
                // A concurrent delete already removed a row; the end state is met.
            }

            return ActionOk();
        }

        /// <summary>
        /// Moves an action to immediately before another action within its pipeline.
        /// </summary>
        /// <param name="key">The IdKey of the action to move.</param>
        /// <param name="beforeKey">The IdKey of the action it should be placed before, or null to move it to the end.</param>
        [BlockAction]
        public BlockActionResult ReorderAction( string key, string beforeKey )
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) )
            {
                return ActionBadRequest( $"Not authorized to edit {SmsPipeline.FriendlyTypeName}." );
            }

            var actionService = new SmsActionService( RockContext );
            var arePredictableIdsEnabled = !PageCache.Layout.Site.DisablePredictableIds;
            var movedAction = actionService.Get( key, arePredictableIdsEnabled );

            if ( movedAction == null )
            {
                return ActionBadRequest( "That action was not found." );
            }

            var siblings = actionService.Queryable()
                .Where( a => a.SmsPipelineId == movedAction.SmsPipelineId )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Id )
                .ToList();

            if ( !siblings.ReorderEntity( key, beforeKey ) )
            {
                return ActionBadRequest( "Invalid reorder attempt." );
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Deletes the pipeline and returns the parent navigation URL.
        /// </summary>
        [BlockAction]
        public BlockActionResult Delete()
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) )
            {
                return ActionBadRequest( $"Not authorized to delete {SmsPipeline.FriendlyTypeName}." );
            }

            var idKey = PageParameter( PageParameterKey.SmsPipelineId );

            if ( idKey.IsNullOrWhiteSpace() || idKey == "0" )
            {
                return ActionBadRequest( $"{SmsPipeline.FriendlyTypeName} not found." );
            }

            var pipelineService = new SmsPipelineService( RockContext );
            var arePredictableIdsEnabled = !PageCache.Layout.Site.DisablePredictableIds;
            var pipeline = pipelineService.Get( idKey, arePredictableIdsEnabled );

            if ( pipeline == null )
            {
                return ActionBadRequest( $"{SmsPipeline.FriendlyTypeName} not found." );
            }

            if ( !pipelineService.CanDelete( pipeline, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            pipelineService.Delete( pipeline );
            RockContext.SaveChanges();

            return ActionOk( this.GetParentPageUrl() );
        }

        /// <summary>
        /// Runs a synthetic inbound SMS message through the pipeline and returns the per-action outcomes.
        /// </summary>
        /// <param name="request">The synthetic message to dispatch through the pipeline.</param>
        /// <returns>The per-action outcomes and the resolved outbound response.</returns>
        [BlockAction]
        public BlockActionResult SendTestMessage( SmsActionTestRequestBag request )
        {
            if ( !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, GetCurrentPerson() ) )
            {
                return ActionBadRequest( $"Not authorized to test {SmsPipeline.FriendlyTypeName}." );
            }

            if ( request == null || request.Message.IsNullOrWhiteSpace() )
            {
                return ActionOk( new SmsActionTestResponseBag
                {
                    ErrorMessage = "Empty Message"
                } );
            }

            var pipeline = GetPipeline( RockContext, trackChanges: false );

            if ( pipeline == null || pipeline.Id == 0 )
            {
                return ActionOk( new SmsActionTestResponseBag
                {
                    ErrorMessage = "Pipeline not found"
                } );
            }

            var message = new SmsMessage
            {
                FromNumber = request.FromNumber ?? string.Empty,
                ToNumber = request.ToNumber ?? string.Empty,
                Message = request.Message
            };

            message.FromPerson = new PersonService( RockContext )
                .GetPersonFromMobilePhoneNumber( message.FromNumber, true );

            var outcomes = SmsActionService.ProcessIncomingMessage( message, pipeline.Id );
            var response = SmsActionService.GetResponseFromOutcomes( outcomes );

            var outcomeBags = new List<SmsActionTestOutcomeBag>();

            if ( outcomes != null )
            {
                foreach ( var outcome in outcomes )
                {
                    if ( outcome == null )
                    {
                        continue;
                    }

                    outcomeBags.Add( new SmsActionTestOutcomeBag
                    {
                        ActionName = outcome.ActionName,
                        ShouldProcess = outcome.ShouldProcess,
                        ResponseMessage = outcome.Response?.Message,
                        IsInteractionLogged = outcome.IsInteractionLogged,
                        ErrorMessage = outcome.ErrorMessage,
                        ExceptionMessage = outcome.Exception?.Message
                    } );
                }
            }

            return ActionOk( new SmsActionTestResponseBag
            {
                ResponseMessage = response?.Message,
                Outcomes = outcomeBags
            } );
        }

        #endregion Block Actions
    }
}
