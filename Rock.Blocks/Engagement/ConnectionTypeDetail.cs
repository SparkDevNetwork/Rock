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
using Rock.Enums.Connection;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Engagement.ConnectionTypeDetail;
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.Cache.Entities;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays the details of a particular connection type.
    /// </summary>

    [DisplayName( "Connection Type Detail" )]
    [Category( "Engagement" )]
    [Description( "Displays the details of a particular connection type." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "decf6d5a-fe4d-4996-bf24-c3a2e8dadd4f" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "d877d384-74aa-442d-a297-25e6dc423989" )]
    [Rock.SystemGuid.BlockTypeGuid( "6CB76282-DD57-4AC1-85EF-05A5E65CF6D6" )]

    public class ConnectionTypeDetail : RockEntityDetailBlockType<ConnectionType, ConnectionTypeBag>, IBreadCrumbBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string ConnectionTypeId = "ConnectionTypeId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        private static class EntityKey
        {
            public const string ActivityType = "ActivityType";
            public const string Status = "Status";
            public const string ConnectionStatusAutomation = "ConnectionStatusAutomation";
            public const string ConnectionWorkflow = "ConnectionWorkflow";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<ConnectionTypeBag, ConnectionTypeDetailOptionsBag>();

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
        private ConnectionTypeDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var currentPerson = RequestContext.CurrentPerson;
            var currentConnectionTypeId = GetInitialEntity()?.Id ?? 0;
            var communicationTemplates = new CommunicationTemplateService( RockContext ).Queryable()
                .AsNoTracking()
                .Where( t => t.IsActive && t.UsageType == null )
                .ToList()
                .Where( t => t.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .Where( t => !t.SupportsEmailWizard() )
                .OrderBy( t => t.Name )
                .Select( t => t.ToListItemBag() )
                .ToList();

            var connectionTypes = new ConnectionTypeService( RockContext ).Queryable()
                .AsNoTracking()
                .Where( ct => ct.Id != currentConnectionTypeId )
                .OrderBy( ct => ct.Order )
                .ThenBy( ct => ct.Name )
                .ToListItemBagList();

            var options = new ConnectionTypeDetailOptionsBag
            {
                CommunicationTemplateOptions = communicationTemplates,
                ConnectionTypeOptions = connectionTypes,
                HasActiveAIProvider = AIProviderCache.All( RockContext ).Any( a => a.IsActive )
            };

            return options;
        }

        /// <summary>
        /// Validates the ConnectionType for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="connectionType">The ConnectionType to be validated.</param>
        /// <param name="bag">The bag containing the data from the client.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the ConnectionType is valid, <c>false</c> otherwise.</returns>
        private bool ValidateConnectionType( ConnectionType connectionType, ConnectionTypeBag bag, out string errorMessage )
        {
            errorMessage = null;

            if ( bag != null )
            {
                var statuses = bag.Statuses ?? new List<ConnectionStatusBag>();

                var isMissingDefaultStatus = !statuses.Any( s => s.IsDefault );
                var isMissingActivityType = bag.ActivityTypes == null || !bag.ActivityTypes.Any();

                if ( isMissingDefaultStatus && isMissingActivityType )
                {
                    errorMessage = "A default connection status and at least one activity type are required.";
                    return false;
                }

                if ( isMissingDefaultStatus )
                {
                    errorMessage = "A default status is required.";
                    return false;
                }

                if ( isMissingActivityType )
                {
                    errorMessage = "At least one activity type is required.";
                    return false;
                }

                var statusGuids = statuses
                    .Select( s => s.Guid )
                    .Where( g => g != Guid.Empty )
                    .ToHashSet();

                var activityTypeGuids = ( bag.ActivityTypes ?? new List<ConnectionActivityTypeBag>() )
                    .Select( a => a.Guid )
                    .Where( g => g != Guid.Empty )
                    .ToHashSet();

                /*
                     2/9/2026 - MSE

                     This section of validation enforces required per-status values when
                     "DueDateCalculationMode" is set to DurationPerStatus. In this mode, each
                     Connection Status must define its own due duration and due-soon offsets.
                     However, the UI does not clearly indicate that these fields are required
                     unless the edit modal is opened for a status.

                     Without this validation, statuses could be saved without valid due date
                     or due-soon offsets, resulting in broken or inconsistent due date behavior.

                     Similarly, this logic also validates per-status configuration when the
                     "Future Follow-Up" setting is enabled, which requires setting status-level
                     FutureFollowUpDuration values.

                     Reason: Prevent saving Connection Statuses with missing or invalid per-status
                     configuration when "DueDateCalculationMode" changes or "Future Follow-Up" is enabled.
                */
                if ( bag.DueDateCalculationMode == DueDateCalculationMode.DurationPerStatus )
                {
                    var invalidDueDurationStatusNames = statuses
                        .Where( s =>
                            !s.RequestStatusDueDateOffsetInDays.HasValue ||
                            s.RequestStatusDueDateOffsetInDays.Value <= 0 )
                        .Select( s => s.Name.Trim() )
                        .Distinct()
                        .ToList();

                    if ( invalidDueDurationStatusNames.Any() )
                    {
                        var label = invalidDueDurationStatusNames.Count == 1 ? "status" : "statuses";
                        errorMessage = $"A Status Due Duration is required for the following {label}: {string.Join( ", ", invalidDueDurationStatusNames )}.";
                        return false;
                    }

                    var invalidDueSoonStatusNames = statuses
                        .Where( s =>
                            !s.RequestStatusDueSoonOffsetInDays.HasValue ||
                            s.RequestStatusDueSoonOffsetInDays.Value <= 0 ||
                            ( s.RequestStatusDueDateOffsetInDays.HasValue &&
                             s.RequestStatusDueSoonOffsetInDays.Value > s.RequestStatusDueDateOffsetInDays.Value ) )
                        .Select( s => s.Name.Trim() )
                        .Distinct()
                        .ToList();

                    if ( invalidDueSoonStatusNames.Any() )
                    {
                        var label = invalidDueSoonStatusNames.Count == 1 ? "status" : "statuses";
                        errorMessage = $"A Due Soon Window is required and must not exceed Status Due Duration for the following {label}: {string.Join( ", ", invalidDueSoonStatusNames )}.";
                        return false;
                    }
                }

                if ( bag.EnableFutureFollowup )
                {
                    var invalidFutureFollowupStatusNames = statuses
                        .Where( s =>
                            !s.AutoFutureFollowUpPauseInDays.HasValue ||
                            s.AutoFutureFollowUpPauseInDays.Value <= 0 )
                        .Select( s => s.Name.Trim() )
                        .Distinct()
                        .ToList();

                    if ( invalidFutureFollowupStatusNames.Any() )
                    {
                        var label = invalidFutureFollowupStatusNames.Count == 1 ? "status" : "statuses";
                        errorMessage = $"Future Follow-Up Duration is required for the following {label}: {string.Join( ", ", invalidFutureFollowupStatusNames )}.";
                        return false;
                    }
                }

                /*
                     2/9/2026 - MSE

                     Ensure every ConnectionStatusAutomation has a valid Destination Status.
                     While client-side validation already makes this a required field and also
                     prevents deleting statuses that are being used as destinations, this check
                     acts as a final safeguard to protect data integrity.

                     Reason: Prevent invalid automations that could cause unexpected behavior or runtime errors.
                */
                var invalidAutomations = statuses
                    .SelectMany( s => ( s.Automations ?? new List<ConnectionStatusAutomationBag>() )
                        .Select( a => new
                        {
                            StatusName = s.Name,
                            Automation = a
                        } ) )
                    .Where( x => x.Automation == null
                        || !x.Automation.DestinationStatusGuid.HasValue
                        || !statusGuids.Contains( x.Automation.DestinationStatusGuid.Value ) )
                    .ToList();

                if ( invalidAutomations.Any() )
                {
                    var errorMessages = invalidAutomations.Select( x =>
                    {
                        var automationName = x.Automation?.AutomationName ?? "Unnamed Automation";

                        return $"• Automation '{automationName}' on status '{x.StatusName}' must have a valid destination status selected.";
                    } );

                    errorMessage = "One or more status automations are invalid:<br>" + string.Join( "<br>", errorMessages );
                    return false;
                }

                /*
                     2/9/2026 - MSE

                     ConnectionWorkflow validation must account for multiple scenarios:

                     1) Connection Statuses or Activity Types can be used as a trigger/dependency, and
                        we must account for those that are being created/edited/deleted in the
                        same save operation. We must verify those references exist in the current
                        incoming bag to prevent invalid workflow configuration.

                     2) Future Follow-Up as a ConnectionState can be used as a trigger/dependency by
                        workflows set on either ConnectionType or ConnectionOpportunity. Whether this dependency
                        is valid is controlled by this Connection Type's EnableFutureFollowup setting:
                        if Future Follow-Up is disabled, we disallow workflow dependencies that still
                        reference that ConnectionState. We need to verify that no ConnectionWorkflow
                        still depends on Future Follow-Up state before allowing EnableFutureFollowup to be disabled.

                     Reason: Protect ConnectionWorkflow integrity by preventing missing or incorrect
                     triggers dependencies that could lead to runtime errors or broken workflow behavior.
                */
                if ( !TryValidateConnectionWorkflowDependencies( bag, connectionType.Id, statusGuids, activityTypeGuids, out errorMessage ) )
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<ConnectionTypeBag, ConnectionTypeDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {ConnectionType.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( ConnectionType.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( ConnectionType.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="ConnectionTypeBag"/> that represents the entity.</returns>
        private ConnectionTypeBag GetCommonEntityBag( ConnectionType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new ConnectionTypeBag
            {
                IdKey = entity.IdKey,
                ConnectionRequestDetailPage = new PageRouteValueBag
                {
                    Page = entity.ConnectionRequestDetailPage.ToListItemBag(),
                    Route = entity.ConnectionRequestDetailPageRoute.ToListItemBag()
                },
                Description = entity.Description,
                DueDateCalculationMode = entity.DueDateCalculationMode,
                EnabledFeatures = entity.EnabledFeatures,
                EnabledViews = entity.EnabledViews,
                EnableFullActivityList = entity.EnableFullActivityList,
                EnableFutureFollowup = entity.EnableFutureFollowup,
                EnableRequestSecurity = entity.EnableRequestSecurity,
                IconCssClass = entity.IconCssClass,
                IsActive = entity.IsActive,
                IsSequentialStatusEnforced = entity.IsSequentialStatusEnforced,
                Name = entity.Name,
                Order = entity.Order,
                RequestDueDateOffsetInDays = entity.RequestDueDateOffsetInDays,
                RequestDueSoonOffsetInDays = entity.RequestDueSoonOffsetInDays,
                RequiresPlacementGroupToConnect = entity.RequiresPlacementGroupToConnect
            };
        }

        /// <inheritdoc/>
        protected override ConnectionTypeBag GetEntityBagForView( ConnectionType entity )
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

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
        }

        //// <inheritdoc/>
        protected override ConnectionTypeBag GetEntityBagForEdit( ConnectionType entity )
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

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            // Get the ConnectionType, ConnectionOpportunity, and ConnectionRequest attribute definitions for edit.
            LoadAttributesForLocalEntities( entity.Id, bag );

            bag.ActivityTypes = GetConnectionActivityTypeBags( entity.Id, out var activityTypeIdToGuidMap );
            bag.Statuses = GetConnectionStatusBags( entity.Id, out var statusIdToGuidMap );
            bag.Workflows = GetConnectionWorkflowBags( entity.Id, statusIdToGuidMap, activityTypeIdToGuidMap );
            bag.AdditionalSettings = GetAdditionalSettingsBag( entity );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( ConnectionType entity, ValidPropertiesBox<ConnectionTypeBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.ConnectionRequestDetailPage ),
                () =>
                {
                    entity.ConnectionRequestDetailPageId = box.Bag.ConnectionRequestDetailPage.Page.GetEntityId<Page>( RockContext );
                    entity.ConnectionRequestDetailPageRouteId = box.Bag.ConnectionRequestDetailPage.Route.GetEntityId<PageRoute>( RockContext );
                } );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.DueDateCalculationMode ),
                () => entity.DueDateCalculationMode = box.Bag.DueDateCalculationMode );

            box.IfValidProperty( nameof( box.Bag.EnabledFeatures ),
                () => entity.EnabledFeatures = box.Bag.EnabledFeatures );

            box.IfValidProperty( nameof( box.Bag.EnabledViews ),
                () => entity.EnabledViews = box.Bag.EnabledViews );

            box.IfValidProperty( nameof( box.Bag.EnableFullActivityList ),
                () => entity.EnableFullActivityList = box.Bag.EnableFullActivityList );

            box.IfValidProperty( nameof( box.Bag.EnableFutureFollowup ),
                () => entity.EnableFutureFollowup = box.Bag.EnableFutureFollowup );

            box.IfValidProperty( nameof( box.Bag.EnableRequestSecurity ),
                () => entity.EnableRequestSecurity = box.Bag.EnableRequestSecurity );

            box.IfValidProperty( nameof( box.Bag.IconCssClass ),
                () => entity.IconCssClass = box.Bag.IconCssClass );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.IsSequentialStatusEnforced ),
                () => entity.IsSequentialStatusEnforced = box.Bag.IsSequentialStatusEnforced );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.Order ),
                () => entity.Order = box.Bag.Order );

            box.IfValidProperty( nameof( box.Bag.RequestDueDateOffsetInDays ),
                () => entity.RequestDueDateOffsetInDays = entity.DueDateCalculationMode == DueDateCalculationMode.FixedDaysFromStartTypeLevel
                    ? box.Bag.RequestDueDateOffsetInDays
                    : null );

            box.IfValidProperty( nameof( box.Bag.RequestDueSoonOffsetInDays ),
                () => entity.RequestDueSoonOffsetInDays = entity.DueDateCalculationMode == DueDateCalculationMode.FixedDaysFromStartTypeLevel
                    ? box.Bag.RequestDueSoonOffsetInDays
                    : null );

            box.IfValidProperty( nameof( box.Bag.RequiresPlacementGroupToConnect ),
                () => entity.RequiresPlacementGroupToConnect = box.Bag.RequiresPlacementGroupToConnect );

            box.IfValidProperty( nameof( box.Bag.AdditionalSettings ), () =>
            {
                var settings = box.Bag.AdditionalSettings ?? new ConnectionTypeAdditionalSettingsBag();
                var communicationSettings = settings.CommunicationSettings ?? new ConnectionTypeCommunicationSettingsBag();

                entity.SetConnectionTypeAdditionalSettings( new ConnectionType.ConnectionTypeAdditionalSettings
                {
                    AdditionalRequestsToShow = ( settings.AdditionalRequestsToShow ?? new List<ConnectionTypeAdditionalRequestToShowBag>() )
                        .Where( a => a != null && a.ConnectionType?.Value.AsGuidOrNull().HasValue == true )
                        .Select( a => new ConnectionType.ConnectionTypeAdditionalSettings.AdditionalRequestToShowSettings
                        {
                            Key = a.Key,
                            ConnectionTypeGuid = a.ConnectionType.Value.AsGuidOrNull().Value,
                            StatesToShow = a.StatesToShow ?? new List<ConnectionState>(),
                            LimitToRecentRequestsDays = a.LimitToRecentRequestsDays,
                            IncludeFamilyMemberRequests = a.IncludeFamilyMemberRequests
                        } )
                        .ToList(),
                    CommunicationSettings = new ConnectionType.ConnectionTypeAdditionalSettings.CommunicationSettingsInfo
                    {
                        CommunicationTemplateCategoryGuid = communicationSettings.CommunicationTemplateCategoryGuid,
                        SmsSnippetCategoryGuid = communicationSettings.SmsSnippetCategoryGuid
                    },
                    AIInsightsPrompt = settings.AIInsightsPrompt
                } );
            } );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override ConnectionType GetInitialEntity()
        {
            var entity = GetInitialEntity<ConnectionType, ConnectionTypeService>( RockContext, PageParameterKey.ConnectionTypeId );

            ApplyNewConnectionTypeDefaultValues( entity );

            return entity;
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
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var key = pageReference.GetPageParameter( PageParameterKey.ConnectionTypeId );
            var pageParameters = new Dictionary<string, string>();

            var name = new ConnectionTypeService( RockContext )
               .GetSelect( key, ct => ct.Name );

            if ( name != null )
            {
                pageParameters.Add( PageParameterKey.ConnectionTypeId, key );
            }

            var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageParameters );
            var breadCrumb = new BreadCrumbLink( name ?? "New Connection Type", breadCrumbPageRef );

            return new BreadCrumbResult
            {
                BreadCrumbs = new List<IBreadCrumb> { breadCrumb }
            };
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out ConnectionType entity, out BlockActionResult error )
        {
            var entityService = new ConnectionTypeService( RockContext );
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
                entity = new ConnectionType();
                entityService.Add( entity );

                ApplyNewConnectionTypeDefaultValues( entity, entityService );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{ConnectionType.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit {ConnectionType.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Synchronizes related entities by comparing existing entities with incoming data, deleting removed items,
        /// and adding or updating entities as needed.
        /// </summary>
        private void SyncRelatedEntities<TEntity, TBag, TKey>(
            Service<TEntity> service,
            IQueryable<TEntity> existingEntitiesQuery,
            IEnumerable<TBag> incomingBags,
            Func<TEntity, TKey> existingKeySelector,
            Func<TBag, TKey> incomingKeySelector,
            Func<TBag, TEntity> createNew,
            Action<TEntity, TBag> updateEntity )
            where TEntity : Entity<TEntity>, new()
        {
            // Load existing entities from database
            var existingEntities = existingEntitiesQuery.ToList();
            var existingByKey = existingEntities.ToDictionary( existingKeySelector );

            var incomingList = ( incomingBags ?? Enumerable.Empty<TBag>() ).ToList();
            var incomingKeys = incomingList.Select( incomingKeySelector ).ToHashSet();

            // Delete entities that are no longer in the incoming set
            foreach ( var entity in existingEntities.Where( e => !incomingKeys.Contains( existingKeySelector( e ) ) ).ToList() )
            {
                service.Delete( entity );
            }

            // Add or update entities based on incoming data
            foreach ( var bag in incomingList )
            {
                var key = incomingKeySelector( bag );

                if ( !existingByKey.TryGetValue( key, out var entity ) )
                {
                    entity = createNew( bag );
                    service.Add( entity );
                }

                updateEntity( entity, bag );
            }
        }

        /// <summary>
        /// Loads the attributes for the local entities.
        /// </summary>
        /// <param name="entityId">The connection type entity identifier.</param>
        /// <param name="bag">The bag to populate.</param>
        private void LoadAttributesForLocalEntities( int entityId, ConnectionTypeBag bag )
        {
            if ( entityId == 0 )
            {
                bag.ConnectionTypeAttributes = new List<PublicEditableAttributeBag>();
                bag.ConnectionOpportunityAttributes = new List<PublicEditableAttributeBag>();
                bag.ConnectionRequestAttributes = new List<PublicEditableAttributeBag>();
                return;
            }

            var attributeService = new AttributeService( RockContext );
            var qualifierValue = entityId.ToString();

            // ConnectionType attributes
            bag.ConnectionTypeAttributes = attributeService.GetByEntityTypeId( new ConnectionType().TypeId, true )
                .AsNoTracking()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "Id", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList()
                .ConvertAll( a => PublicAttributeHelper.GetPublicEditableAttribute( a ) );

            // ConnectionRequest attributes
            bag.ConnectionRequestAttributes = attributeService.GetByEntityTypeId( new ConnectionRequest().TypeId, true )
                .AsNoTracking()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "ConnectionTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList()
                .ConvertAll( a => PublicAttributeHelper.GetPublicEditableAttribute( a ) );

            // ConnectionOpportunity attributes
            bag.ConnectionOpportunityAttributes = attributeService.GetByEntityTypeId( new ConnectionOpportunity().TypeId, true )
                .AsNoTracking()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "ConnectionTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList()
                .ConvertAll( a => PublicAttributeHelper.GetPublicEditableAttribute( a ) );
        }

        /// <summary>
        /// Saves the attributes for the specified entity type and qualifier.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier whose attributes are being edited.</param>
        /// <param name="qualifierColumn">The attribute qualifier column.</param>
        /// <param name="qualifierValue">The qualifier value.</param>
        /// <param name="attributes">The attributes as edited in the UI.</param>
        private void SaveEntityAttributes( int entityTypeId, string qualifierColumn, string qualifierValue, List<PublicEditableAttributeBag> attributes )
        {
            if ( attributes == null )
            {
                return;
            }

            // Get the existing attributes for this entity type and qualifier value
            var attributeService = new AttributeService( RockContext );
            var existingAttributes = attributeService.GetByEntityTypeQualifier( entityTypeId, qualifierColumn, qualifierValue, true ).ToList();

            // Delete any of those attributes that were removed in the UI
            var remainingAttributeGuids = attributes.Select( a => a.Guid );
            foreach ( var attr in existingAttributes.Where( a => !remainingAttributeGuids.Contains( a.Guid ) ) )
            {
                attributeService.Delete( attr );
                RockContext.SaveChanges();
            }

            // The attributes are coming from the frontend already sorted in the correct order.
            int attributeOrder = 0;
            foreach ( var attrBag in attributes )
            {
                var attr = Helper.SaveAttributeEdits( attrBag, entityTypeId, qualifierColumn, qualifierValue, RockContext );
                if ( attr != null )
                {
                    attr.Order = attributeOrder++;
                }
            }
        }


        /// <summary>
        /// Gets the connection activity type bags for the specified connection type.
        /// </summary>
        /// <param name="connectionTypeId">The connection type identifier.</param>
        /// <param name="activityTypeIdToGuidMap">Output map of activity type ID to GUID for related workflow qualifier conversion.</param>
        /// <returns>A list of ConnectionActivityTypeBag.</returns>
        private List<ConnectionActivityTypeBag> GetConnectionActivityTypeBags( int connectionTypeId, out Dictionary<int, Guid> activityTypeIdToGuidMap )
        {
            var activityTypes = new List<ConnectionActivityType>();

            if ( connectionTypeId > 0 )
            {
                activityTypes = new ConnectionActivityTypeService( RockContext ).Queryable()
                    .AsNoTracking()
                    .Where( a => a.ConnectionTypeId == connectionTypeId )
                    .OrderBy( a => a.Name )
                    .ToList();
            }

            activityTypeIdToGuidMap = activityTypes.ToDictionary( a => a.Id, a => a.Guid );

            var bags = new List<ConnectionActivityTypeBag>();

            foreach ( var activityType in activityTypes )
            {
                activityType.LoadAttributes( RockContext );

                var bag = new ConnectionActivityTypeBag
                {
                    Guid = activityType.Guid,
                    Name = activityType.Name,
                    IsActive = activityType.IsActive,
                    PersonNoteCreationBehavior = activityType.PersonNoteCreationBehavior
                };

                bag.LoadAttributesAndValuesForPublicEdit( activityType, RequestContext.CurrentPerson, enforceSecurity: true );

                bags.Add( bag );
            }

            return bags;
        }

        /// <summary>
        /// Gets the connection status bags for the specified connection type.
        /// </summary>
        /// <param name="connectionTypeId">The connection type identifier.</param>
        /// <param name="statusIdToGuidMap">Output map of status ID to GUID for related workflow qualifier conversion.</param>
        /// <returns>A list of ConnectionStatusBag.</returns>
        private List<ConnectionStatusBag> GetConnectionStatusBags( int connectionTypeId, out Dictionary<int, Guid> statusIdToGuidMap )
        {
            var statuses = new List<ConnectionStatus>();

            if ( connectionTypeId > 0 )
            {
                statuses = new ConnectionStatusService( RockContext ).Queryable()
                    .AsNoTracking()
                    .Include( s => s.ConnectionStatusAutomations )
                    .Where( s => s.ConnectionTypeId == connectionTypeId )
                    .OrderBy( s => s.Order )
                    .ThenBy( s => s.Name )
                    .ToList();
            }

            var localStatusIdToGuidMap = statuses.ToDictionary( s => s.Id, s => s.Guid );
            statusIdToGuidMap = localStatusIdToGuidMap;

            var bags = new List<ConnectionStatusBag>();

            foreach ( var status in statuses )
            {
                var bag = new ConnectionStatusBag
                {
                    Guid = status.Guid,
                    Order = status.Order,
                    Name = status.Name,
                    Description = status.Description,
                    IsActive = status.IsActive,
                    HighlightColor = status.HighlightColor,
                    IsDefault = status.IsDefault,
                    IsNoteRequiredOnCompletion = status.IsNoteRequiredOnCompletion,
                    AutoInactivateState = status.AutoInactivateState,
                    RequestStatusDueDateOffsetInDays = status.RequestStatusDueDateOffsetInDays,
                    RequestStatusDueSoonOffsetInDays = status.RequestStatusDueSoonOffsetInDays,
                    AutoFutureFollowUpPauseInDays = status.AutoFutureFollowUpPauseInDays,
                    Automations = status.ConnectionStatusAutomations.Select( a => new ConnectionStatusAutomationBag
                    {
                        Guid = a.Guid,
                        Order = a.Order,
                        AutomationName = a.AutomationName,
                        DataView = a.DataView?.ToListItemBag(),
                        GroupRequirementsFilter = a.GroupRequirementsFilter,
                        DestinationStatusGuid = localStatusIdToGuidMap[a.DestinationStatusId]
                    } ).ToList()
                };

                bags.Add( bag );
            }

            return bags;
        }

        /// <summary>
        /// Gets the connection workflow bags for the specified connection type.
        /// </summary>
        /// <param name="connectionTypeId">The connection type identifier.</param>
        /// <param name="statusIdToGuidMap">Map of status IDs to GUIDs.</param>
        /// <param name="activityTypeIdToGuidMap">Map of activity type IDs to GUIDs.</param>
        /// <returns>A list of ConnectionWorkflowBag.</returns>
        private List<ConnectionWorkflowBag> GetConnectionWorkflowBags( int connectionTypeId, Dictionary<int, Guid> statusIdToGuidMap, Dictionary<int, Guid> activityTypeIdToGuidMap )
        {
            if ( connectionTypeId == 0 )
            {
                return new List<ConnectionWorkflowBag>();
            }

            var workflows = new ConnectionWorkflowService( RockContext ).Queryable()
                .AsNoTracking()
                .Include( wf => wf.WorkflowType )
                .Include( wf => wf.ManualTriggerFilterConnectionStatus )
                .Include( wf => wf.IncludeDataView )
                .Include( wf => wf.ExcludeDataView )
                .Where( wf => wf.ConnectionTypeId == connectionTypeId )
                .ToList();

            return workflows
                .Select( wf => new ConnectionWorkflowBag
                {
                    Guid = wf.Guid,
                    WorkflowType = wf.WorkflowType != null
                        ? new ListItemBag { Value = wf.WorkflowType.Guid.ToString(), Text = wf.WorkflowType.Name }
                        : null,
                    TriggerType = wf.TriggerType,
                    QualifierValue = ConvertPrimaryQualifierIdToGuid( wf.TriggerType, wf.QualifierValue, statusIdToGuidMap, activityTypeIdToGuidMap ),
                    ManualTriggerFilterConnectionStatusGuid = wf.ManualTriggerFilterConnectionStatus != null
                        ? wf.ManualTriggerFilterConnectionStatus.Guid
                        : ( Guid? ) null,
                    AppliesToAgeClassification = wf.AppliesToAgeClassification,
                    IncludeDataViewId = wf.IncludeDataView != null
                        ? new ListItemBag { Value = wf.IncludeDataView.Guid.ToString(), Text = wf.IncludeDataView.Name }
                        : null,
                    ExcludeDataViewId = wf.ExcludeDataView != null
                        ? new ListItemBag { Value = wf.ExcludeDataView.Guid.ToString(), Text = wf.ExcludeDataView.Name }
                        : null
                } )
                .OrderBy( wf => wf.WorkflowType?.Text ?? string.Empty )
                .ThenBy( wf => wf.TriggerType.ConvertToString() )
                .ToList();
        }

        /// <summary>
        /// Gets the additional settings bag for edit mode.
        /// </summary>
        /// <param name="entity">The connection type entity.</param>
        /// <returns>The additional settings bag.</returns>
        private ConnectionTypeAdditionalSettingsBag GetAdditionalSettingsBag( ConnectionType entity )
        {
            var additionalSettings = entity.GetConnectionTypeAdditionalSettings()
                ?? new ConnectionType.ConnectionTypeAdditionalSettings();

            var additionalRequestFilters = ( additionalSettings.AdditionalRequestsToShow ?? new List<ConnectionType.ConnectionTypeAdditionalSettings.AdditionalRequestToShowSettings>() )
                .Where( a => a != null )
                .ToList();

            var guids = additionalRequestFilters
                .Select( a => a.ConnectionTypeGuid )
                .Where( g => g != Guid.Empty )
                .Distinct()
                .ToList();

            var additionalRequestsConnectionTypes = guids
                .Select( g => new
                {
                    Guid = g,
                    ConnectionType = ConnectionTypeCache.Get( g )
                } )
                .Where( x => x.ConnectionType != null )
                .ToDictionary(
                    x => x.Guid,
                    x => new ListItemBag
                    {
                        Value = x.Guid.ToString(),
                        Text = x.ConnectionType.Name
                    } );

            var communicationSettings = additionalSettings.CommunicationSettings
                ?? new ConnectionType.ConnectionTypeAdditionalSettings.CommunicationSettingsInfo();

            return new ConnectionTypeAdditionalSettingsBag
            {
                AdditionalRequestsToShow = additionalRequestFilters
                    .Select( a => new ConnectionTypeAdditionalRequestToShowBag
                    {
                        Key = a.Key,
                        ConnectionType = additionalRequestsConnectionTypes.TryGetValue( a.ConnectionTypeGuid, out var connectionTypeBag )
                            ? connectionTypeBag
                            : null,
                        StatesToShow = a.StatesToShow ?? new List<ConnectionState>(),
                        LimitToRecentRequestsDays = a.LimitToRecentRequestsDays,
                        IncludeFamilyMemberRequests = a.IncludeFamilyMemberRequests
                    } )
                    .ToList(),
                CommunicationSettings = new ConnectionTypeCommunicationSettingsBag
                {
                    CommunicationTemplateCategoryGuid = communicationSettings.CommunicationTemplateCategoryGuid,
                    SmsSnippetCategoryGuid = communicationSettings.SmsSnippetCategoryGuid
                },
                AIInsightsPrompt = additionalSettings.AIInsightsPrompt
            };
        }

        #endregion Methods

        #region Helper Methods

        /// <summary>
        /// Applies default values to a new <see cref="ConnectionType"/>.
        /// </summary>
        /// <param name="entity">The connection type entity.</param>
        /// <param name="connectionTypeService">An optional service instance to use for queries.</param>
        private void ApplyNewConnectionTypeDefaultValues( ConnectionType entity, ConnectionTypeService connectionTypeService = null )
        {
            if ( entity == null || entity.Id != 0 )
            {
                return;
            }

            connectionTypeService = connectionTypeService ?? new ConnectionTypeService( RockContext );

            entity.EnabledViews =
                EnabledViewFlags.List |
                EnabledViewFlags.Board |
                EnabledViewFlags.Grid |
                EnabledViewFlags.Snapshot;

            entity.EnabledFeatures = EnabledFeatureFlags.GroupPlacement;

            var maxOrder = connectionTypeService.Queryable()
                .Select( t => ( int? ) t.Order )
                .Max();

            entity.Order = maxOrder.HasValue ? maxOrder.Value + 1 : 0;
        }

        /// <summary>
        /// Converts workflow qualifier ID values (pipe-delimited string) to their corresponding entity GUIDs for display.
        /// </summary>
        /// <param name="triggerType">The workflow trigger type.</param>
        /// <param name="qualifierValue">The pipe-delimited qualifier string containing primary and secondary IDs.</param>
        /// <param name="statusIdToGuidMap">Map of status IDs to their GUIDs.</param>
        /// <param name="activityTypeIdToGuidMap">Map of activity type IDs to their GUIDs.</param>
        /// <returns>The qualifier value bag with GUIDs, or <c>null</c> if the input is empty.</returns>
        private ConnectionWorkflowQualifierValueBag ConvertPrimaryQualifierIdToGuid( ConnectionWorkflowTriggerType triggerType, string qualifierValue, Dictionary<int, Guid> statusIdToGuidMap, Dictionary<int, Guid> activityTypeIdToGuidMap )
        {
            statusIdToGuidMap = statusIdToGuidMap ?? new Dictionary<int, Guid>();
            activityTypeIdToGuidMap = activityTypeIdToGuidMap ?? new Dictionary<int, Guid>();

            if ( !TryParseWorkflowQualifierValue( qualifierValue, out var primary, out var secondary ) )
            {
                return null;
            }

            if ( triggerType == ConnectionWorkflowTriggerType.StatusChanged
                || triggerType == ConnectionWorkflowTriggerType.StatusBecomesDue
                || triggerType == ConnectionWorkflowTriggerType.StatusBecomesDueSoon
                || triggerType == ConnectionWorkflowTriggerType.StatusBecomesOverdue )
            {
                if ( int.TryParse( primary, out var statusId ) && statusIdToGuidMap.TryGetValue( statusId, out var statusGuid ) )
                {
                    primary = statusGuid.ToString();
                }

                if ( triggerType == ConnectionWorkflowTriggerType.StatusChanged
                    && int.TryParse( secondary, out var secondaryStatusId )
                    && statusIdToGuidMap.TryGetValue( secondaryStatusId, out var secondaryStatusGuid ) )
                {
                    secondary = secondaryStatusGuid.ToString();
                }
            }
            else if ( triggerType == ConnectionWorkflowTriggerType.ActivityAdded )
            {
                if ( int.TryParse( primary, out var activityTypeId ) && activityTypeIdToGuidMap.TryGetValue( activityTypeId, out var activityGuid ) )
                {
                    primary = activityGuid.ToString();
                }
            }

            return new ConnectionWorkflowQualifierValueBag
            {
                PrimaryQualifier = primary,
                SecondaryQualifier = secondary
            };
        }

        /// <summary>
        /// Converts workflow qualifier GUID values to their corresponding entity IDs for storage.
        /// </summary>
        /// <param name="triggerType">The workflow trigger type.</param>
        /// <param name="qualifierValue">The qualifier value containing primary and secondary qualifiers.</param>
        /// <param name="statusGuidToIdMap">Map of status GUIDs to their IDs.</param>
        /// <param name="activityTypeGuidToIdMap">Map of activity type GUIDs to their IDs.</param>
        /// <returns>A pipe-delimited string of the converted IDs.</returns>
        private string ConvertPrimaryQualifierGuidToId( ConnectionWorkflowTriggerType triggerType, ConnectionWorkflowQualifierValueBag qualifierValue, Dictionary<Guid, int> statusGuidToIdMap, Dictionary<Guid, int> activityTypeGuidToIdMap )
        {
            if ( qualifierValue == null )
            {
                return string.Empty;
            }

            var primary = qualifierValue.PrimaryQualifier ?? string.Empty;
            var secondary = qualifierValue.SecondaryQualifier ?? string.Empty;

            if ( triggerType == ConnectionWorkflowTriggerType.StatusChanged
                || triggerType == ConnectionWorkflowTriggerType.StatusBecomesDue
                || triggerType == ConnectionWorkflowTriggerType.StatusBecomesDueSoon
                || triggerType == ConnectionWorkflowTriggerType.StatusBecomesOverdue )
            {
                if ( Guid.TryParse( primary, out var statusGuid ) && statusGuidToIdMap.TryGetValue( statusGuid, out var statusId ) )
                {
                    primary = statusId.ToString();
                }

                if ( triggerType == ConnectionWorkflowTriggerType.StatusChanged
                    && Guid.TryParse( secondary, out var secondaryStatusGuid )
                    && statusGuidToIdMap.TryGetValue( secondaryStatusGuid, out var secondaryStatusId ) )
                {
                    secondary = secondaryStatusId.ToString();
                }
            }
            else if ( triggerType == ConnectionWorkflowTriggerType.ActivityAdded )
            {
                if ( Guid.TryParse( primary, out var activityGuid ) && activityTypeGuidToIdMap.TryGetValue( activityGuid, out var activityTypeId ) )
                {
                    primary = activityTypeId.ToString();
                }
            }

            return $"|{primary}|{secondary}|";
        }

        /// <summary>
        /// Parses a persisted workflow qualifier value string into primary and secondary values.
        /// </summary>
        /// <param name="qualifierValue">The persisted pipe-delimited qualifier string.</param>
        /// <param name="primaryQualifier">The parsed primary qualifier value.</param>
        /// <param name="secondaryQualifier">The parsed secondary qualifier value.</param>
        /// <returns><c>true</c> if parsing succeeded; otherwise <c>false</c>.</returns>
        private bool TryParseWorkflowQualifierValue( string qualifierValue, out string primaryQualifier, out string secondaryQualifier )
        {
            primaryQualifier = string.Empty;
            secondaryQualifier = string.Empty;

            if ( string.IsNullOrWhiteSpace( qualifierValue ) )
            {
                return false;
            }

            var parts = qualifierValue.Split( '|' );
            primaryQualifier = parts.Length > 1 ? parts[1] ?? string.Empty : string.Empty;
            secondaryQualifier = parts.Length > 2 ? parts[2] ?? string.Empty : string.Empty;

            return true;
        }

        /// <summary>
        /// Validates workflow dependencies, including Future Follow-Up usage and qualifier references.
        /// </summary>
        private bool TryValidateConnectionWorkflowDependencies( ConnectionTypeBag bag, int connectionTypeId, HashSet<Guid> statusGuids, HashSet<Guid> activityTypeGuids, out string errorMessage )
        {
            statusGuids = statusGuids ?? new HashSet<Guid>();
            activityTypeGuids = activityTypeGuids ?? new HashSet<Guid>();

            var workflows = ( bag.Workflows ?? new List<ConnectionWorkflowBag>() )
                .Where( w => w != null )
                .ToList();

            // If Future Follow-Up is being disabled, validate that no workflows are still depending on it either by trigger type or by state qualifier value.
            if ( !bag.EnableFutureFollowup )
            {
                var futureFollowUpErrors = new List<string>();
                var futureFollowUpStateString = ( ( int ) ConnectionState.FutureFollowUp ).ToString();

                // Validate in-memory Connection Type workflows being saved right now.
                var invalidTypeWorkflows = workflows
                    .Where( wf => WorkflowReferencesFutureFollowUp( wf.TriggerType, wf.QualifierValue?.PrimaryQualifier, wf.QualifierValue?.SecondaryQualifier, futureFollowUpStateString ) )
                    .Select( wf => wf.WorkflowType?.Text ?? "Unnamed Workflow" )
                    .Distinct()
                    .ToList();

                if ( invalidTypeWorkflows.Any() )
                {
                    var label = invalidTypeWorkflows.Count == 1 ? "Workflow" : "Workflows";
                    var verb = invalidTypeWorkflows.Count == 1 ? "references" : "reference";
                    var formattedWorkflowNames = invalidTypeWorkflows.Select( w => $"<strong>{w}</strong>" );
                    futureFollowUpErrors.Add( $"The following Connection Type {label} {verb} Future Follow-Up: {string.Join( ", ", formattedWorkflowNames )}." );
                }

                if ( connectionTypeId > 0 )
                {
                    // Validate persisted Connection Opportunity workflows for this Connection Type.
                    var connectionWorkflowService = new ConnectionWorkflowService( RockContext );

                    var opportunityWorkflows = connectionWorkflowService.Queryable()
                        .Where( wf => wf.ConnectionOpportunityId.HasValue && wf.ConnectionOpportunity.ConnectionTypeId == connectionTypeId )
                        .Where( wf => wf.TriggerType == ConnectionWorkflowTriggerType.FutureFollowupDateReached
                            || wf.TriggerType == ConnectionWorkflowTriggerType.StateChanged )
                        .Select( wf => new
                        {
                            OpportunityId = wf.ConnectionOpportunityId.Value,
                            OpportunityName = wf.ConnectionOpportunity != null ? wf.ConnectionOpportunity.Name : "Unnamed Opportunity",
                            WorkflowName = wf.WorkflowType != null ? wf.WorkflowType.Name : "Unnamed Workflow",
                            wf.TriggerType,
                            wf.QualifierValue
                        } )
                        .ToList();

                    var invalidOpportunityWorkflows = opportunityWorkflows
                        .Where( wf =>
                        {
                            TryParseWorkflowQualifierValue( wf.QualifierValue, out var primaryQualifier, out var secondaryQualifier );
                            return WorkflowReferencesFutureFollowUp( wf.TriggerType, primaryQualifier, secondaryQualifier, futureFollowUpStateString );
                        } )
                        .ToList();

                    if ( invalidOpportunityWorkflows.Any() )
                    {
                        var formattedErrors = invalidOpportunityWorkflows
                            .GroupBy( x => new { x.OpportunityId, x.OpportunityName } )
                            .Select( g =>
                            {
                                var formattedOpportunityName = $"<strong>{g.Key.OpportunityName}</strong>";
                                var formattedWorkflowNames = g.Select( wf => wf.WorkflowName )
                                    .Distinct()
                                    .Select( wf => $"<strong>{wf}</strong>" );

                                return $"{formattedOpportunityName} ({string.Join( ", ", formattedWorkflowNames )})";
                            } )
                            .ToList();

                        var label = invalidOpportunityWorkflows.Count == 1 ? "Workflow" : "Workflows";
                        var verb = invalidOpportunityWorkflows.Count == 1 ? "references" : "reference";
                        futureFollowUpErrors.Add( $"The following Connection Opportunity {label} {verb} Future Follow-Up: {string.Join( "; ", formattedErrors )}." );
                    }
                }

                if ( futureFollowUpErrors.Any() )
                {
                    errorMessage = "Future Follow-Up cannot be disabled because one or more workflows still depend on it. Update those workflows first, then try again.<br>" + string.Join( "<br>", futureFollowUpErrors );
                    return false;
                }
            }

            // Validate that any status or activity type referenced in the workflow qualifiers actually exist
            var statusOrActivityErrors = new List<string>();
            foreach ( var workflow in workflows )
            {
                var workflowName = workflow.WorkflowType?.Text ?? "Workflow";
                var primaryQualifier = workflow.QualifierValue?.PrimaryQualifier ?? string.Empty;
                var secondaryQualifier = workflow.QualifierValue?.SecondaryQualifier ?? string.Empty;

                if ( workflow.TriggerType == ConnectionWorkflowTriggerType.StatusChanged )
                {
                    if ( IsInvalidStatusGuid( primaryQualifier, statusGuids ) || IsInvalidStatusGuid( secondaryQualifier, statusGuids ) )
                    {
                        statusOrActivityErrors.Add( $"• Workflow <strong>{workflowName}</strong> references a status that does not exist." );
                    }
                }
                else if ( workflow.TriggerType == ConnectionWorkflowTriggerType.StatusBecomesDue
                    || workflow.TriggerType == ConnectionWorkflowTriggerType.StatusBecomesDueSoon
                    || workflow.TriggerType == ConnectionWorkflowTriggerType.StatusBecomesOverdue )
                {
                    if ( IsInvalidStatusGuid( primaryQualifier, statusGuids ) )
                    {
                        statusOrActivityErrors.Add( $"• Workflow <strong>{workflowName}</strong> references a status that does not exist." );
                    }
                }
                else if ( workflow.TriggerType == ConnectionWorkflowTriggerType.ActivityAdded )
                {
                    if ( IsInvalidActivityGuid( primaryQualifier, activityTypeGuids ) )
                    {
                        statusOrActivityErrors.Add( $"• Workflow <strong>{workflowName}</strong> references an activity type that does not exist." );
                    }
                }

                else if ( workflow.TriggerType == ConnectionWorkflowTriggerType.Manual )
                {
                    if ( workflow.ManualTriggerFilterConnectionStatusGuid.HasValue
                    && !statusGuids.Contains( workflow.ManualTriggerFilterConnectionStatusGuid.Value ) )
                    {
                        statusOrActivityErrors.Add( $"• Workflow <strong>{workflowName}</strong> references a status that does not exist." );
                    }
                }
            }

            if ( statusOrActivityErrors.Any() )
            {
                errorMessage = "One or more workflows are invalid:<br>" + string.Join( "<br>", statusOrActivityErrors );
                return false;
            }

            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Determines if a workflow references Future Follow-Up by trigger type or state qualifier.
        /// </summary>
        /// <param name="triggerType">The workflow trigger type.</param>
        /// <param name="primaryQualifier">The workflow primary qualifier value.</param>
        /// <param name="secondaryQualifier">The workflow secondary qualifier value.</param>
        /// <param name="futureFollowUpStateString">The Future Follow-Up state value as a string.</param>
        /// <returns><c>true</c> if the workflow references Future Follow-Up; otherwise <c>false</c>.</returns>
        private bool WorkflowReferencesFutureFollowUp( ConnectionWorkflowTriggerType triggerType, string primaryQualifier, string secondaryQualifier, string futureFollowUpStateString )
        {
            if ( triggerType == ConnectionWorkflowTriggerType.FutureFollowupDateReached )
            {
                return true;
            }

            if ( triggerType != ConnectionWorkflowTriggerType.StateChanged )
            {
                return false;
            }

            return primaryQualifier == futureFollowUpStateString
                || secondaryQualifier == futureFollowUpStateString;
        }

        /// <summary>
        /// Determines whether the given value is an invalid or non-existent status GUID.
        /// </summary>
        /// <param name="value">The string value to check (expected to be a GUID).</param>
        /// <param name="statusGuids">The set of valid status GUIDs.</param>
        /// <returns><c>true</c> if the value is non-empty and either not a valid GUID or not in the status set; otherwise <c>false</c>.</returns>
        private bool IsInvalidStatusGuid( string value, HashSet<Guid> statusGuids )
        {
            if ( string.IsNullOrWhiteSpace( value ) )
            {
                return false;
            }

            return !Guid.TryParse( value, out var guid ) || !statusGuids.Contains( guid );
        }

        /// <summary>
        /// Determines whether the given value is an invalid or non-existent activity type GUID.
        /// </summary>
        /// <param name="value">The string value to check (expected to be a GUID).</param>
        /// <param name="activityTypeGuids">The set of valid activity type GUIDs.</param>
        /// <returns><c>true</c> if the value is non-empty and either not a valid GUID or not in the activity type set; otherwise <c>false</c>.</returns>
        private bool IsInvalidActivityGuid( string value, HashSet<Guid> activityTypeGuids )
        {
            if ( string.IsNullOrWhiteSpace( value ) )
            {
                return false;
            }

            return !Guid.TryParse( value, out var guid ) || !activityTypeGuids.Contains( guid );
        }

        #endregion Helper Methods

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

            return ActionOk( new ValidPropertiesBox<ConnectionTypeBag>
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
        public BlockActionResult Save( ValidPropertiesBox<ConnectionTypeBag> box )
        {
            var entityService = new ConnectionTypeService( RockContext );

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
            if ( !ValidateConnectionType( entity, box.Bag, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.WrapTransaction( () =>
            {
                // Save the connection type first to ensure it has an Id ( if it's a new connection type )
                // before saving the related entities.
                RockContext.SaveChanges();

                // Activity Types
                box.IfValidProperty( nameof( box.Bag.ActivityTypes ), () =>
                {
                    var activityTypeService = new ConnectionActivityTypeService( RockContext );
                    var activityTypeBags = ( box.Bag.ActivityTypes ?? new List<ConnectionActivityTypeBag>() ).Where( b => b != null ).ToList();

                    foreach ( var b in activityTypeBags.Where( b => b.Guid == Guid.Empty ) )
                    {
                        b.Guid = Guid.NewGuid();
                    }

                    SyncRelatedEntities(
                        activityTypeService,
                        activityTypeService.Queryable().Where( a => a.ConnectionTypeId == entity.Id ),
                        activityTypeBags,
                        existingKeySelector: a => a.Guid,
                        incomingKeySelector: b => b.Guid,
                        createNew: b => new ConnectionActivityType { Guid = b.Guid },
                        updateEntity: ( activityType, bag ) =>
                        {
                            activityType.ConnectionType = entity;
                            activityType.Name = bag.Name;
                            activityType.IsActive = bag.IsActive;
                            activityType.PersonNoteCreationBehavior = bag.PersonNoteCreationBehavior;
                        } );
                } );

                // Statuses
                box.IfValidProperty( nameof( box.Bag.Statuses ), () =>
                {
                    var statusService = new ConnectionStatusService( RockContext );
                    var statusBags = ( box.Bag.Statuses ?? new List<ConnectionStatusBag>() ).Where( b => b != null ).ToList();

                    // The statuses are coming from the frontend already sorted in the correct order.
                    // Since we're potentially creating a new connection type or status, we cannot implement the ReorderItem() block action pattern.
                    // We set the order properly here. We'll do the same thing for ConnectionStatusAutomations further below.
                    for ( var i = 0; i < statusBags.Count; i++ )
                    {
                        var bag = statusBags[i];

                        if ( bag.Guid == Guid.Empty )
                        {
                            bag.Guid = Guid.NewGuid();
                        }

                        bag.Order = i;
                    }

                    SyncRelatedEntities(
                        statusService,
                        statusService.Queryable().Where( s => s.ConnectionTypeId == entity.Id ),
                        statusBags,
                        existingKeySelector: s => s.Guid,
                        incomingKeySelector: b => b.Guid,
                        createNew: b => new ConnectionStatus { Guid = b.Guid },
                        updateEntity: ( status, bag ) =>
                        {
                            status.ConnectionType = entity;
                            status.Name = bag.Name;
                            status.Order = bag.Order;
                            status.Description = bag.Description;
                            status.IsActive = bag.IsActive;
                            status.HighlightColor = bag.HighlightColor;
                            status.IsDefault = bag.IsDefault;
                            status.IsNoteRequiredOnCompletion = bag.IsNoteRequiredOnCompletion;
                            status.AutoInactivateState = bag.AutoInactivateState;
                            status.RequestStatusDueDateOffsetInDays = bag.RequestStatusDueDateOffsetInDays;
                            status.RequestStatusDueSoonOffsetInDays = bag.RequestStatusDueSoonOffsetInDays;
                            status.AutoFutureFollowUpPauseInDays = bag.AutoFutureFollowUpPauseInDays;
                        } );

                    /*
                         2/9/2026 - MSE

                         SaveChanges() is called here to generate database Id values for newly
                         created Connection Statuses and Activity Types before creating related
                         Automations and Workflows that depend on those Ids.

                         Automations and Workflows configured on the client-side can reference newly
                         created entities within the same transaction, without requiring a
                         separate database save. This is required for that functionality.
                    */
                    RockContext.SaveChanges();

                    // Automations ( tied to individual Statuses )
                    var automationService = new ConnectionStatusAutomationService( RockContext );

                    var statusGuidToIdMap = statusService.Queryable()
                        .Where( s => s.ConnectionTypeId == entity.Id )
                        .Select( s => new { s.Id, s.Guid } )
                        .ToList()
                        .ToDictionary( s => s.Guid, s => s.Id );

                    foreach ( var statusBag in statusBags )
                    {
                        if ( statusGuidToIdMap.TryGetValue( statusBag.Guid, out var statusId ) )
                        {
                            var automationBags = ( statusBag.Automations ?? new List<ConnectionStatusAutomationBag>() )
                                .Where( b => b != null
                                    && b.DestinationStatusGuid.HasValue
                                    && statusGuidToIdMap.ContainsKey( b.DestinationStatusGuid.Value ) )
                                .ToList();

                            for ( var i = 0; i < automationBags.Count; i++ )
                            {
                                var bag = automationBags[i];

                                if ( bag.Guid == Guid.Empty )
                                {
                                    bag.Guid = Guid.NewGuid();
                                }

                                bag.Order = i;
                            }

                            SyncRelatedEntities(
                                automationService,
                                automationService.Queryable().Where( a => a.SourceStatusId == statusId ),
                                automationBags,
                                existingKeySelector: a => a.Guid,
                                incomingKeySelector: b => b.Guid,
                                createNew: b => new ConnectionStatusAutomation { Guid = b.Guid },
                                updateEntity: ( automation, bag ) =>
                                {
                                    automation.AutomationName = bag.AutomationName;
                                    automation.Order = bag.Order;
                                    automation.SourceStatusId = statusId;
                                    automation.DestinationStatusId = statusGuidToIdMap[bag.DestinationStatusGuid.Value];
                                    automation.DataViewId = bag.DataView?.GetEntityId<DataView>( RockContext );
                                    automation.GroupRequirementsFilter = ( GroupRequirementsFilter ) bag.GroupRequirementsFilter;
                                } );
                        }
                    }
                } );

                // Workflows
                box.IfValidProperty( nameof( box.Bag.Workflows ), () =>
                {
                    var workflowService = new ConnectionWorkflowService( RockContext );
                    var workflowBags = ( box.Bag.Workflows ?? new List<ConnectionWorkflowBag>() ).Where( b => b != null ).ToList();

                    foreach ( var b in workflowBags.Where( b => b.Guid == Guid.Empty ) )
                    {
                        b.Guid = Guid.NewGuid();
                    }

                    var statusGuidToIdMap = new ConnectionStatusService( RockContext ).Queryable()
                        .Where( s => s.ConnectionTypeId == entity.Id )
                        .Select( s => new { s.Id, s.Guid } )
                        .ToList()
                        .ToDictionary( s => s.Guid, s => s.Id );

                    var activityTypeGuidToIdMap = new ConnectionActivityTypeService( RockContext ).Queryable()
                        .Where( a => a.ConnectionTypeId == entity.Id )
                        .Select( a => new { a.Id, a.Guid } )
                        .ToList()
                        .ToDictionary( a => a.Guid, a => a.Id );

                    SyncRelatedEntities(
                        workflowService,
                        workflowService.Queryable().Where( wf => wf.ConnectionTypeId == entity.Id ),
                        workflowBags,
                        existingKeySelector: wf => wf.Guid,
                        incomingKeySelector: b => b.Guid,
                        createNew: b => new ConnectionWorkflow { Guid = b.Guid },
                        updateEntity: ( wf, bag ) =>
                        {
                            wf.ConnectionType = entity;
                            wf.WorkflowTypeId = bag.WorkflowType?.GetEntityId<WorkflowType>( RockContext ) ?? 0;
                            wf.TriggerType = bag.TriggerType;
                            wf.QualifierValue = ConvertPrimaryQualifierGuidToId( bag.TriggerType, bag.QualifierValue, statusGuidToIdMap, activityTypeGuidToIdMap );

                            if ( bag.TriggerType != ConnectionWorkflowTriggerType.Manual )
                            {
                                // Clear out any filter values associated with the Manual trigger type
                                // if the trigger type is not Manual.
                                wf.ManualTriggerFilterConnectionStatusId = null;
                                wf.AppliesToAgeClassification = AppliesToAgeClassification.All;
                                wf.IncludeDataViewId = null;
                                wf.ExcludeDataViewId = null;
                            }
                            else
                            {
                                wf.ManualTriggerFilterConnectionStatusId = bag.ManualTriggerFilterConnectionStatusGuid.HasValue
                                    && statusGuidToIdMap.TryGetValue( bag.ManualTriggerFilterConnectionStatusGuid.Value, out var statusId )
                                        ? statusId
                                        : ( int? ) null;
                                wf.AppliesToAgeClassification = bag.AppliesToAgeClassification;
                                wf.IncludeDataViewId = bag.IncludeDataViewId?.GetEntityId<DataView>( RockContext );
                                wf.ExcludeDataViewId = bag.ExcludeDataViewId?.GetEntityId<DataView>( RockContext );
                            }
                        } );
                } );

                if ( box.Bag.ActivityTypes != null )
                {
                    var activityTypesByGuid = new ConnectionActivityTypeService( RockContext ).Queryable()
                        .Where( a => a.ConnectionTypeId == entity.Id )
                        .ToList()
                        .ToDictionary( a => a.Guid );

                    foreach ( var bag in box.Bag.ActivityTypes )
                    {
                        if ( bag.AttributeValues != null && activityTypesByGuid.TryGetValue( bag.Guid, out var activity ) )
                        {
                            activity.LoadAttributes( RockContext );
                            activity.SetPublicAttributeValues( bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
                            activity.SaveAttributeValues( RockContext );
                        }
                    }
                }

                var qualifierValue = entity.Id.ToString();
                SaveEntityAttributes( new ConnectionType().TypeId, "Id", qualifierValue, box.Bag.ConnectionTypeAttributes );
                SaveEntityAttributes( new ConnectionOpportunity().TypeId, "ConnectionTypeId", qualifierValue, box.Bag.ConnectionOpportunityAttributes );
                SaveEntityAttributes( new ConnectionRequest().TypeId, "ConnectionTypeId", qualifierValue, box.Bag.ConnectionRequestAttributes );

                // Save ConnectionType attribute values ( the attribute values for this entity itself )
                entity.SaveAttributeValues( RockContext );

                // Ensure the current person has the necessary authorizations.
                if ( !entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                {
                    entity.AllowPerson( Authorization.VIEW, RequestContext.CurrentPerson, RockContext );
                }

                if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    entity.AllowPerson( Authorization.EDIT, RequestContext.CurrentPerson, RockContext );
                }

                if ( !entity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                {
                    entity.AllowPerson( Authorization.ADMINISTRATE, RequestContext.CurrentPerson, RockContext );
                }

                RockContext.SaveChanges();
            } );

            // Clear cached triggers since they may have changed.
            ConnectionWorkflowService.RemoveCachedTriggers();

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.ConnectionTypeId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<ConnectionTypeBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Copies the specified entity and returns the URL of the copied entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be copied.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Copy( string key )
        {
            var entityService = new ConnectionTypeService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var copiedConnectionTypeId = entityService.Copy( entity.Id );
            var copiedConnectionType = entityService.Get( copiedConnectionTypeId );

            if ( copiedConnectionType == null )
            {
                return ActionBadRequest( "Connection Type failed to copy." );
            }

            ConnectionWorkflowService.RemoveCachedTriggers();

            return ActionOk( this.GetCurrentPageUrl( new Dictionary<string, string>
            {
                [PageParameterKey.ConnectionTypeId] = copiedConnectionType.IdKey
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
            var entityService = new ConnectionTypeService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            var connectionOpportunities = entity.ConnectionOpportunities.ToList();
            var connectionOpportunityService = new ConnectionOpportunityService( RockContext );
            var connectionRequestActivityService = new ConnectionRequestActivityService( RockContext );

            foreach ( var connectionOpportunity in connectionOpportunities )
            {
                var connectionRequestActivities = new ConnectionRequestActivityService( RockContext ).Queryable()
                    .Where( a => a.ConnectionOpportunityId == connectionOpportunity.Id )
                    .ToList();

                foreach ( var connectionRequestActivity in connectionRequestActivities )
                {
                    connectionRequestActivityService.Delete( connectionRequestActivity );
                }

                if ( !connectionOpportunityService.CanDelete( connectionOpportunity, out var opportunityErrorMessage ) )
                {
                    return ActionBadRequest( opportunityErrorMessage );
                }

                connectionOpportunityService.Delete( connectionOpportunity );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            ConnectionWorkflowService.RemoveCachedTriggers();

            return ActionOk( this.GetParentPageUrl() );
        }

        /// <summary>
        /// Checks if the specified entity can be deleted.
        /// </summary>
        /// <param name="request">The request that identifies the entity to check.</param>
        /// <returns>A response indicating if the entity can be deleted.</returns>
        [BlockAction]
        public BlockActionResult CanDeleteEntity( CanDeleteRequestBag request )
        {
            if ( request == null || request.EntityGuid == Guid.Empty || request.EntityKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Invalid entity." );
            }

            var entityKey = request.EntityKey;
            string errorMessage;
            bool canDelete;

            if ( entityKey == EntityKey.ActivityType )
            {
                var service = new ConnectionActivityTypeService( RockContext );
                var entity = service.Get( request.EntityGuid );

                if ( entity == null )
                {
                    return ActionOk( new CanDeleteResponseBag { CanDelete = true } );
                }

                canDelete = service.CanDelete( entity, out errorMessage );
            }
            else if ( entityKey == EntityKey.Status )
            {
                var service = new ConnectionStatusService( RockContext );
                var entity = service.Get( request.EntityGuid );

                if ( entity == null )
                {
                    return ActionOk( new CanDeleteResponseBag { CanDelete = true } );
                }

                canDelete = service.CanDelete( entity, out errorMessage );
            }
            else if ( entityKey == EntityKey.ConnectionStatusAutomation )
            {
                var service = new ConnectionStatusAutomationService( RockContext );
                var entity = service.Get( request.EntityGuid );

                if ( entity == null )
                {
                    return ActionOk( new CanDeleteResponseBag { CanDelete = true } );
                }

                canDelete = service.CanDelete( entity, out errorMessage );
            }
            else if ( entityKey == EntityKey.ConnectionWorkflow )
            {
                var service = new ConnectionWorkflowService( RockContext );
                var entity = service.Get( request.EntityGuid );

                if ( entity == null )
                {
                    return ActionOk( new CanDeleteResponseBag { CanDelete = true } );
                }

                canDelete = service.CanDelete( entity, out errorMessage );
            }
            else
            {
                return ActionBadRequest( $"Unknown entity: {entityKey}" );
            }

            return ActionOk( new CanDeleteResponseBag { CanDelete = canDelete, ErrorMessage = errorMessage } );
        }

        #endregion Block Actions
    }
}
