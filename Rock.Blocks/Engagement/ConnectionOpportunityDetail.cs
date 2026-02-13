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

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Engagement.ConnectionOpportunityDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays the details of a particular connection opportunity.
    /// </summary>

    [DisplayName( "Connection Opportunity Detail" )]
    [Category( "Engagement" )]
    [Description( "Displays the details of a particular connection opportunity." )]
    [IconCssClass( "ti ti-question-mark" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "B51A4229-1C36-4D62-8EC6-97BB300BCDB2" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "81567935-EFF8-4B19-B140-6F14FE0B896F" )]
    [Rock.SystemGuid.BlockTypeGuid( "216E2EE6-4E2D-4D0F-AA36-AB808F565C48" )]
    public class ConnectionOpportunityDetail : RockEntityDetailBlockType<ConnectionOpportunity, ConnectionOpportunityBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string ConnectionOpportunityId = "ConnectionOpportunityId";
            public const string ConnectionTypeId = "ConnectionTypeId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Fields

        private bool _hasAnyDefaultConnectors;

        #endregion Fields

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string ShowEdit = "ShowEdit";
        }

        #endregion Attribute Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<ConnectionOpportunityBag, ConnectionOpportunityDetailOptionsBag>();

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
        private ConnectionOpportunityDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new ConnectionOpportunityDetailOptionsBag();
            options.IsReOrderColumnVisible = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            return options;
        }

		/// <summary>
		/// Gets the connection request attributes that are scoped to the specified connection opportunity.
		/// </summary>
		/// <param name="connectionOpportunityId">The identifier of the connection opportunity.</param>
		/// <returns>A list of <see cref="Model.Attribute"/> entities ordered for display.</returns>
        private List<Model.Attribute> GetConnectionRequestAttributes( string connectionOpportunityId )
        {
            return new AttributeService( RockContext ).GetByEntityTypeId( new ConnectionRequest().TypeId, true ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "ConnectionOpportunityId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( connectionOpportunityId ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
        }

		/// <summary>
		/// Gets the connection request attributes that are inherited from the specified connection type.
		/// </summary>
		/// <param name="connectionTypeId">The identifier of the connection type.</param>
		/// <returns>A list of inherited attribute bags describing the attributes.</returns>
        private List<InheritedAttributeBag> GetInheritedConnectionRequestAttributes( string connectionTypeId )
        {
            var entityTypeId = new ConnectionRequest().TypeId;

            var inheritedAttributes = new AttributeService( RockContext ).GetByEntityTypeId( new ConnectionRequest().TypeId, true ).AsQueryable()
                .AsNoTracking()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "ConnectionTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( connectionTypeId ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .Select( a => new InheritedAttributeBag
                {
                    Name = a.Name,
                    Description = a.Description,
                    Key = a.Key,
                    Guid = a.Guid
                } )
                .ToList();

            return inheritedAttributes;
        }

		/// <summary>
		/// Gets the campuses configured for the specified connection opportunity and notes if any have a default connector assigned.
		/// </summary>
		/// <param name="connectionOpportunityId">The identifier of the connection opportunity.</param>
		/// <returns>A list of campus list items.</returns>
        private List<ListItemBag> GetConnectionOpportunityCampuses( int connectionOpportunityId )
        {
            var campusData = new ConnectionOpportunityCampusService( RockContext ).Queryable()
                .AsNoTracking()
                .Include( coc => coc.Campus )
                .Where( coc => coc.ConnectionOpportunityId == connectionOpportunityId )
                .Select( coc => new
                {
                    Campus = coc.Campus != null
                        ? new ListItemBag { Value = coc.Campus.Guid.ToString(), Text = coc.Campus.Name }
                        : null,
                    DefaultConnectorPersonAliasId = coc.DefaultConnectorPersonAliasId
                } )
                .ToList();

            _hasAnyDefaultConnectors = campusData.Any( c => c.DefaultConnectorPersonAliasId.HasValue );

            return campusData.Select( c => c.Campus ).Where( c => c != null ).ToList();
        }
		/// <summary>
		/// Gets the existing default connectors for campuses, filtered to ensure they are active members
		/// of a connector group associated to this connection opportunity.
		/// </summary>
		/// <param name="connectionOpportunityId">The identifier of the connection opportunity.</param>
		/// <returns>A dictionary mapping campus Guid string to the default connector person alias as a <see cref="ListItemBag"/>.</returns>
        private Dictionary<Guid, ListItemBag> GetDefaultConnectors( int connectionOpportunityId )
        {
            var connectorGroupIds = new ConnectionOpportunityConnectorGroupService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( cg => cg.ConnectionOpportunityId == connectionOpportunityId )
                .Select( cg => cg.ConnectorGroupId )
                .ToList();

            var query = new ConnectionOpportunityCampusService( RockContext ).Queryable()
                .AsNoTracking()
                .Include( coc => coc.Campus )
                .Include( coc => coc.DefaultConnectorPersonAlias.Person )
                .Where( coc =>
                    coc.ConnectionOpportunityId == connectionOpportunityId &&
                    coc.DefaultConnectorPersonAliasId.HasValue &&
                    coc.DefaultConnectorPersonAlias.Person != null )
                .Join(
                    new GroupMemberService( RockContext ).Queryable().AsNoTracking(),
                    coc => coc.DefaultConnectorPersonAlias.PersonId,
                    gm => gm.PersonId,
                    ( coc, gm ) => new { coc, gm } )
                .Where( j =>
                    connectorGroupIds.Contains( j.gm.GroupId ) &&
                    j.gm.GroupMemberStatus == GroupMemberStatus.Active )
                .Select( j => new
                {
                    CampusGuid = j.coc.Campus.Guid,
                    AliasId = j.coc.DefaultConnectorPersonAlias.Id,
                    j.coc.DefaultConnectorPersonAlias.Person.NickName,
                    j.coc.DefaultConnectorPersonAlias.Person.FirstName,
                    j.coc.DefaultConnectorPersonAlias.Person.LastName
                } )
                .Distinct()
                .ToList()
                .ToDictionary(
                    k => k.CampusGuid,
                    v => new ListItemBag
                    {
                        Value = v.AliasId.ToString(),
                        Text = $"{( v.NickName ?? v.FirstName ?? "" )} {v.LastName ?? ""}"
                    } );

            return query;
        }

        /// <summary>
        /// Saves the connection request attributes for this connection opportunity
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier whose attributes are being edited.</param>
        /// <param name="qualifierColumn">The attribute qualifier column.</param>
        /// <param name="qualifierValue">The qualifier value.</param>
        /// <param name="connectionRequestAttributes">The attributes as edited in the UI.</param>
        private void SaveConnectionRequestAttributes( int entityTypeId, string qualifierColumn, string qualifierValue, List<PublicEditableAttributeBag> connectionRequestAttributes )
        {
            // Get the existing attributes for this entity type and qualifier value
            var attributeService = new AttributeService( RockContext );
            var attributes = attributeService.GetByEntityTypeQualifier( entityTypeId, qualifierColumn, qualifierValue, true ).ToList();

            // Delete any of those attributes that were removed in the UI
            var remainingAttributeGuids = connectionRequestAttributes.Select( a => a.Guid );
            foreach ( var attr in attributes.Where( a => !remainingAttributeGuids.Contains( a.Guid ) ) )
            {
                attributeService.Delete( attr );
                RockContext.SaveChanges();
            }

            // The attributes are coming from the frontend already sorted in the correct order. 
            int attributeOrder = 0;
            foreach ( var attrBag in connectionRequestAttributes )
            {
                var attr = Helper.SaveAttributeEdits( attrBag, entityTypeId, qualifierColumn, qualifierValue, RockContext );
                if ( attr != null )
                {
                    attr.Order = attributeOrder++;
                }
            }

            RockContext.SaveChanges();
        }

        /// <summary>
        /// Validates the ConnectionOpportunity for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="connectionOpportunity">The ConnectionOpportunity to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the ConnectionOpportunity is valid, <c>false</c> otherwise.</returns>
        private bool ValidateConnectionOpportunity( ConnectionOpportunity connectionOpportunity, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Synchronizes related entities for a connection opportunity by comparing existing entities
        /// with incoming data, deleting removed items, and adding or updating entities as needed.
        /// </summary>
        /// <typeparam name="TEntity">The entity type being synchronized.</typeparam>
        /// <typeparam name="TBag">The bag type containing the incoming data.</typeparam>
        /// <typeparam name="TKey">The key type used to identify entities.</typeparam>
        /// <param name="service">The Rock service for the entity type.</param>
        /// <param name="existingEntitiesQuery">Query to retrieve existing entities (already filtered by ConnectionOpportunityId).</param>
        /// <param name="incomingBags">The collection of incoming bag objects from the UI.</param>
        /// <param name="existingKeySelector">Function to extract the key from existing entities.</param>
        /// <param name="incomingKeySelector">Function to extract the key from incoming bags.</param>
        /// <param name="createNew">Function to create a new entity instance from a bag.</param>
        /// <param name="updateEntity">Action to update an entity's properties from a bag.</param>
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
                TEntity entity;

                if ( !existingByKey.TryGetValue( key, out entity ) )
                {
                    entity = createNew( bag );
                    service.Add( entity );
                }

                updateEntity( entity, bag );
            }
        }

        /// <summary>
        /// Updates the default connector person alias for campuses based on the incoming data.
        /// Validates that the default connector is an active member of a connector group.
        /// </summary>
        /// <param name="entity">The connection opportunity entity.</param>
        /// <param name="defaultConnectors">Dictionary mapping campus Guid strings to person alias ListItemBags.</param>
        /// <param name="existingCampusesByCampusGuid">Dictionary of existing ConnectionOpportunityCampus entities keyed by Campus Guid.</param>
        private void UpdateDefaultConnectors(
            ConnectionOpportunity entity,
            Dictionary<Guid, ListItemBag> defaultConnectors,
            Dictionary<Guid, ConnectionOpportunityCampus> existingCampusesByCampusGuid )
        {
            if ( defaultConnectors == null || !defaultConnectors.Any() )
            {
                return;
            }

            // Get connector group IDs for validation
            var connectorGroupIds = new ConnectionOpportunityConnectorGroupService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( cg => cg.ConnectionOpportunityId == entity.Id )
                .Select( cg => cg.ConnectorGroupId )
                .ToList();

            foreach ( var kvp in defaultConnectors )
            {
                var campusGuid = kvp.Key;

                if ( !existingCampusesByCampusGuid.TryGetValue( campusGuid, out var opportunityCampus ) )
                {
                    continue;
                }

                var personAliasId = kvp.Value?.Value.AsIntegerOrNull();
                if ( personAliasId.HasValue )
                {
                    // Validate that the default connector is an active member of a connector group
                    // ( Should always be true from the initial dropdown options population )
                    var isValidConnector = new GroupMemberService( RockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Include( gm => gm.Person )
                        .Any( gm => gm.Person.PrimaryAliasId == personAliasId
                                && connectorGroupIds.Contains( gm.GroupId )
                                && gm.GroupMemberStatus == GroupMemberStatus.Active );

                    if ( !isValidConnector )
                    {
                        continue;
                    }
                }

                opportunityCampus.DefaultConnectorPersonAliasId = personAliasId;
            }
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<ConnectionOpportunityBag, ConnectionOpportunityDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {ConnectionOpportunity.FriendlyTypeName} was not found.";
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( ConnectionOpportunity.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( ConnectionOpportunity.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="ConnectionOpportunityBag"/> that represents the entity.</returns>
        private ConnectionOpportunityBag GetCommonEntityBag( ConnectionOpportunity entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var connectionType = ConnectionTypeCache.Get( PageParameter( PageParameterKey.ConnectionTypeId ), !PageCache.Layout.Site.DisablePredictableIds );
            string connectionTypeName = null;
            string connectionTypeUrl = null;
            if ( connectionType != null )
            {
                connectionTypeName = connectionType.Name;
                var urlTemplate = EntityTypeCache.Get( typeof( ConnectionType ) ).LinkUrlLavaTemplate;
                if ( !string.IsNullOrWhiteSpace( urlTemplate ) )
                {
                    connectionTypeUrl = urlTemplate.ResolveMergeFields( new Dictionary<string, object>
                    {
                        { "Entity", connectionType }
                    } );

                    connectionTypeUrl = this.RequestContext.ResolveRockUrl( connectionTypeUrl );
                }
            }

            return new ConnectionOpportunityBag
            {
                IdKey = entity.IdKey,
                ConnectionTypeName = connectionTypeName,
                ConnectionTypeUrl = connectionTypeUrl,
                Description = entity.Description,
                IconCssClass = entity.IconCssClass,
                IsActive = entity.IsActive,
                Name = entity.Name,
                Photo = entity.Photo.ToListItemBag(),
                PublicName = entity.PublicName,
                ShowCampusOnTransfer = entity.ShowCampusOnTransfer,
                ShowConnectButton = entity.ShowConnectButton,
                ShowStatusOnTransfer = entity.ShowStatusOnTransfer,
                Summary = entity.Summary
            };
        }

        /// <inheritdoc/>
        protected override ConnectionOpportunityBag GetEntityBagForView( ConnectionOpportunity entity )
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

		/// <inheritdoc/>
        protected override ConnectionOpportunityBag GetEntityBagForEdit( ConnectionOpportunity entity )
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

            bag.ConnectionRequestAttributes = GetConnectionRequestAttributes( entity.Id.ToString() ).ConvertAll( attr => PublicAttributeHelper.GetPublicEditableAttribute( attr ) );

            bag.InheritedConnectionRequestAttributes = GetInheritedConnectionRequestAttributes( entity.ConnectionTypeId.ToString() );

            bag.Campuses = GetConnectionOpportunityCampuses( entity.Id );

            bag.PlacementGroupConfigs = new ConnectionOpportunityGroupConfigService( RockContext ).Queryable()
                .AsNoTracking()
                .Where( cfg => cfg.ConnectionOpportunityId == entity.Id )
                .OrderBy( cfg => cfg.GroupType.Name )
                .Select( cfg => new PlacementGroupConfigBag
                {
                    Guid = cfg.Guid,
                    GroupType = cfg.GroupType != null ? new ListItemBag { Value = cfg.GroupType.Guid.ToString(), Text = cfg.GroupType.Name } : null,
                    GroupMemberRole = cfg.GroupMemberRole != null ? new ListItemBag { Value = cfg.GroupMemberRole.Guid.ToString(), Text = cfg.GroupMemberRole.Name } : null,
                    GroupMemberStatus = ( int ) cfg.GroupMemberStatus,
                    UseAllGroupsOfType = cfg.UseAllGroupsOfType
                } )
                .ToList();

            bag.PlacementGroups = new ConnectionOpportunityGroupService( RockContext ).Queryable()
                .AsNoTracking()
                .Where( pg => pg.ConnectionOpportunityId == entity.Id )
                .OrderBy( pg => pg.Group.Name )
                .Select( pg => new PlacementGroupBag
                {
                    Guid = pg.Guid,
                    GroupType = pg.Group.GroupType != null ? new ListItemBag { Value = pg.Group.GroupType.Guid.ToString(), Text = pg.Group.GroupType.Name } : null,
                    Group = pg.Group != null ? new ListItemBag { Value = pg.Group.Guid.ToString(), Text = pg.Group.Name } : null,
                    Campus = pg.Group.Campus != null ? new ListItemBag { Value = pg.Group.Campus.Guid.ToString(), Text = pg.Group.Campus.Name } : null,
                    IsArchived = pg.Group.IsArchived
                } )
                .ToList();

            bag.ConnectorGroups = new ConnectionOpportunityConnectorGroupService( RockContext ).Queryable()
                .AsNoTracking()
                .Where( cg => cg.ConnectionOpportunityId == entity.Id )
                .OrderBy( cg => cg.Campus.Name )
                .ThenBy( cg => cg.ConnectorGroup.Name )
                .Select( cg => new ConnectorGroupBag
                {
                    Guid = cg.Guid,
                    ConnectorGroup = cg.ConnectorGroup != null ? new ListItemBag { Value = cg.ConnectorGroup.Guid.ToString(), Text = cg.ConnectorGroup.Name } : null,
                    Campus = cg.Campus != null ? new ListItemBag { Value = cg.Campus.Guid.ToString(), Text = cg.Campus.Name } : null
                } )
                .ToList();

            bag.DefaultConnectors = _hasAnyDefaultConnectors
                ? ( GetDefaultConnectors( entity.Id ) ?? new Dictionary<Guid, ListItemBag>() )
                : new Dictionary<Guid, ListItemBag>();

            var inheritedWorkflows = new ConnectionWorkflowService( RockContext ).Queryable()
                .AsNoTracking()
                .Where( wf => wf.ConnectionTypeId == entity.ConnectionTypeId && wf.WorkflowTypeId.HasValue )
                .OrderBy( wf => wf.WorkflowType.Name )
                .Select( wf => new InheritedConnectionWorkflowBag
                {
                    Guid = wf.Guid,
                    WorkflowType = wf.WorkflowType != null ? new ListItemBag { Value = wf.WorkflowType.Guid.ToString(), Text = wf.WorkflowType.Name } : null,
                    TriggerType = ( int ) wf.TriggerType,
                } )
                .ToList();

            bag.InheritedConnectionWorkflows = inheritedWorkflows;

            var workflows = new ConnectionWorkflowService( RockContext ).Queryable()
                .AsNoTracking()
                .Where( wf => wf.ConnectionOpportunityId == entity.Id && wf.WorkflowTypeId.HasValue )
                .Select( wf => new
                {
                    wf.WorkflowTypeId,
                    Bag = new ConnectionWorkflowBag
                    {
                        Guid = wf.Guid,
                        WorkflowType = wf.WorkflowType != null ? new ListItemBag { Value = wf.WorkflowType.Guid.ToString(), Text = wf.WorkflowType.Name } : null,
                        TriggerType = ( int ) wf.TriggerType,
                        QualifierValue = wf.QualifierValue,
                        ManualTriggerFilterConnectionStatusId = wf.ManualTriggerFilterConnectionStatusId,
                        AppliesToAgeClassification = ( int ) wf.AppliesToAgeClassification,
                        IncludeDataViewId = wf.IncludeDataView != null ? new ListItemBag { Value = wf.IncludeDataView.Guid.ToString(), Text = wf.IncludeDataView.Name } : null,
                        ExcludeDataViewId = wf.ExcludeDataView != null ? new ListItemBag { Value = wf.ExcludeDataView.Guid.ToString(), Text = wf.ExcludeDataView.Name } : null
                    }
                } )
                .ToList();

            var workflowTypeOrder = entity.GetAdditionalSettingsOrNull<List<int>>( "WorkflowTypeOrder" );
            if ( workflowTypeOrder != null && workflowTypeOrder.Any() )
            {
                workflows = workflows
                    .OrderBy( wf =>
                    {
                        var id = wf.WorkflowTypeId ?? -1;
                        var idx = workflowTypeOrder.IndexOf( id );
                        return idx == -1 ? int.MaxValue : idx;
                    } )
                    .ToList();
            }
            else
            {
                workflows = workflows.OrderBy( wf => wf.Bag.WorkflowType.Text ).ToList();
            }

            bag.ConnectionWorkflows = workflows
                .Select( wf => wf.Bag )
                .ToList();

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( ConnectionOpportunity entity, ValidPropertiesBox<ConnectionOpportunityBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.IconCssClass ),
                () => entity.IconCssClass = box.Bag.IconCssClass );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.Photo ),
                () => entity.PhotoId = box.Bag.Photo.GetEntityId<BinaryFile>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.PublicName ),
                () => entity.PublicName = box.Bag.PublicName );

            box.IfValidProperty( nameof( box.Bag.ShowCampusOnTransfer ),
                () => entity.ShowCampusOnTransfer = box.Bag.ShowCampusOnTransfer );

            box.IfValidProperty( nameof( box.Bag.ShowConnectButton ),
                () => entity.ShowConnectButton = box.Bag.ShowConnectButton );

            box.IfValidProperty( nameof( box.Bag.ShowStatusOnTransfer ),
                () => entity.ShowStatusOnTransfer = box.Bag.ShowStatusOnTransfer );

            box.IfValidProperty( nameof( box.Bag.Summary ),
                () => entity.Summary = box.Bag.Summary );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override ConnectionOpportunity GetInitialEntity()
        {
            return GetInitialEntity<ConnectionOpportunity, ConnectionOpportunityService>( RockContext, PageParameterKey.ConnectionOpportunityId );
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
        protected override bool TryGetEntityForEditAction( string idKey, out ConnectionOpportunity entity, out BlockActionResult error )
        {
            var entityService = new ConnectionOpportunityService( RockContext );
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
                entity = new ConnectionOpportunity();
                entityService.Add( entity );

                var maxOrder = entityService.Queryable()
                    .Select( t => ( int? ) t.Order )
                    .Max();

                entity.Order = maxOrder.HasValue ? maxOrder.Value + 1 : 0;

                var connectionTypeId = ConnectionTypeCache.Get( PageParameter( PageParameterKey.ConnectionTypeId ), !PageCache.Layout.Site.DisablePredictableIds )?.Id ?? 0;
                if ( connectionTypeId > 0 )
                {
                    entity.ConnectionTypeId = connectionTypeId;
                }
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{ConnectionOpportunity.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${ConnectionOpportunity.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        #endregion Methods

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

            return ActionOk( new ValidPropertiesBox<ConnectionOpportunityBag>
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
        public BlockActionResult Save( ValidPropertiesBox<ConnectionOpportunityBag> box )
        {
            var entityService = new ConnectionOpportunityService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            int? originalPhotoId = entity.PhotoId;

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidateConnectionOpportunity( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.WrapTransaction( () =>
            {
                // Placement Group Configs
                var configService = new ConnectionOpportunityGroupConfigService( RockContext );
                var placementGroupConfigs = ( box.Bag.PlacementGroupConfigs ?? new List<PlacementGroupConfigBag>() ).ToList();
                foreach ( var bag in placementGroupConfigs.Where( b => b.Guid == Guid.Empty ) )
                {
                    bag.Guid = Guid.NewGuid();
                }

                SyncRelatedEntities(
                    configService,
                    configService.Queryable().Where( c => c.ConnectionOpportunityId == entity.Id ),
                    placementGroupConfigs,
                    existingKeySelector: cfg => cfg.Guid,
                    incomingKeySelector: bag => bag.Guid,
                    createNew: bag => new ConnectionOpportunityGroupConfig { Guid = bag.Guid },
                    updateEntity: ( cfg, bag ) =>
                    {
                        cfg.ConnectionOpportunityId = entity.Id;
                        cfg.GroupTypeId = bag.GroupType.GetEntityId<GroupType>( RockContext ) ?? cfg.GroupTypeId;
                        cfg.GroupMemberRoleId = bag.GroupMemberRole?.GetEntityId<GroupTypeRole>( RockContext );
                        cfg.GroupMemberStatus = ( GroupMemberStatus ) bag.GroupMemberStatus;
                        cfg.UseAllGroupsOfType = bag.UseAllGroupsOfType;
                    } );

                // Placement Groups
                var groupService = new ConnectionOpportunityGroupService( RockContext );
                var placementGroups = ( box.Bag.PlacementGroups ?? new List<PlacementGroupBag>() ).ToList();
                foreach ( var bag in placementGroups.Where( b => b.Guid == Guid.Empty ) )
                {
                    bag.Guid = Guid.NewGuid();
                }

                SyncRelatedEntities(
                    groupService,
                    groupService.Queryable().Where( g => g.ConnectionOpportunityId == entity.Id ),
                    placementGroups,
                    existingKeySelector: pg => pg.Guid,
                    incomingKeySelector: bag => bag.Guid,
                    createNew: bag => new ConnectionOpportunityGroup { Guid = bag.Guid },
                    updateEntity: ( pg, bag ) =>
                    {
                        pg.ConnectionOpportunityId = entity.Id;
                        pg.GroupId = bag.Group.GetEntityId<Rock.Model.Group>( RockContext ) ?? pg.GroupId;
                    } );

                // Connector Groups
                var connGroupService = new ConnectionOpportunityConnectorGroupService( RockContext );
                var connectorGroups = ( box.Bag.ConnectorGroups ?? new List<ConnectorGroupBag>() ).ToList();
                foreach ( var bag in connectorGroups.Where( b => b.Guid == Guid.Empty ) )
                {
                    bag.Guid = Guid.NewGuid();
                }

                SyncRelatedEntities(
                    connGroupService,
                    connGroupService.Queryable().Where( g => g.ConnectionOpportunityId == entity.Id ),
                    connectorGroups,
                    existingKeySelector: cg => cg.Guid,
                    incomingKeySelector: bag => bag.Guid,
                    createNew: bag => new ConnectionOpportunityConnectorGroup { Guid = bag.Guid },
                    updateEntity: ( cg, bag ) =>
                    {
                        cg.ConnectionOpportunityId = entity.Id;
                        cg.ConnectorGroupId = bag.ConnectorGroup.GetEntityId<Rock.Model.Group>( RockContext ) ?? cg.ConnectorGroupId;
                        cg.CampusId = bag.Campus?.GetEntityId<Campus>( RockContext );
                    } );

                // Campuses
                var campusService = new ConnectionOpportunityCampusService( RockContext );
                var incomingCampusIds = ( box.Bag.Campuses ?? new List<ListItemBag>() )
                    .Select( c => c.GetEntityId<Campus>( RockContext ) )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToList();

                SyncRelatedEntities(
                    campusService,
                    campusService.Queryable().Where( c => c.ConnectionOpportunityId == entity.Id ),
                    incomingCampusIds,
                    existingKeySelector: ec => ec.CampusId,
                    incomingKeySelector: id => id,
                    createNew: id => new ConnectionOpportunityCampus { CampusId = id, ConnectionOpportunityId = entity.Id },
                    updateEntity: ( ec, id ) =>
                    {
                        // There is nothing to update on a ConnectionOpportunityCampus record if it already exists.
                    } );

                // Default Connectors
                var existingCampusesByCampusGuid = campusService.Queryable()
                    .Include( c => c.Campus )
                    .Where( c => c.ConnectionOpportunityId == entity.Id && c.Campus != null )
                    .ToList()
                    .ToDictionary( c => c.Campus.Guid );

                UpdateDefaultConnectors( entity, box.Bag.DefaultConnectors, existingCampusesByCampusGuid );

                // Workflows
                var workflowService = new ConnectionWorkflowService( RockContext );
                var incomingWorkflows = box.Bag.ConnectionWorkflows ?? new List<ConnectionWorkflowBag>();
                foreach ( var bag in incomingWorkflows.Where( b => b.Guid == Guid.Empty ) )
                {
                    bag.Guid = Guid.NewGuid();
                }

                SyncRelatedEntities(
                    workflowService,
                    workflowService.Queryable().Where( wf => wf.ConnectionOpportunityId == entity.Id ),
                    incomingWorkflows,
                    existingKeySelector: wf => wf.Guid,
                    incomingKeySelector: bag => bag.Guid,
                    createNew: bag => new ConnectionWorkflow { Guid = bag.Guid },
                    updateEntity: ( wf, bag ) =>
                    {
                        wf.ConnectionOpportunityId = entity.Id;
                        wf.WorkflowTypeId = bag.WorkflowType.GetEntityId<WorkflowType>( RockContext );
                        wf.TriggerType = ( ConnectionWorkflowTriggerType ) bag.TriggerType;
                        wf.QualifierValue = bag.QualifierValue;
                        
                        // These properties are assigned directly as they are gatekept and cleared in the UI if the trigger type is not Manual.
                        wf.ManualTriggerFilterConnectionStatusId = bag.ManualTriggerFilterConnectionStatusId;
                        wf.AppliesToAgeClassification = ( AppliesToAgeClassification ) bag.AppliesToAgeClassification;
                        wf.IncludeDataViewId = bag.IncludeDataViewId.GetEntityId<DataView>( RockContext );
                        wf.ExcludeDataViewId = bag.ExcludeDataViewId.GetEntityId<DataView>( RockContext );
                    } );

                /*
                     12/19/2025 - MSE

                     The order of the Connection Workflows is stored as a list of wf.WorkflowTypeId
                     in the AdditionalSettingsJson column on the Connection Opportunity.

                     The frontend already applies the correct sort order when sending the bag, so this logic
                     preserves that order when saving the values rather than re-sorting them on the server.
                */
                if ( incomingWorkflows.Any() )
                {
                    entity.SetAdditionalSettings( "WorkflowTypeOrder", incomingWorkflows.Select( wf => wf.WorkflowType.GetEntityId<WorkflowType>( RockContext ) ).ToList() );
                }

                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );

                // Delete orphaned previous photo if it changed.
                if ( originalPhotoId.HasValue && originalPhotoId != entity.PhotoId )
                {
                    var binaryFileService = new BinaryFileService( RockContext );
                    var oldPhoto = binaryFileService.Get( originalPhotoId.Value );
                    if ( oldPhoto != null )
                    {
                        string errorMessage;
                        if ( binaryFileService.CanDelete( oldPhoto, out errorMessage ) )
                        {
                            binaryFileService.Delete( oldPhoto );
                            RockContext.SaveChanges();
                        }
                    }
                }
            } );

            ConnectionWorkflowService.RemoveCachedTriggers();

            // Save Connection Request Attributes
            var connectionRequestAttributes = box.Bag.ConnectionRequestAttributes;
            if ( connectionRequestAttributes != null )
            {
                SaveConnectionRequestAttributes( new ConnectionRequest().TypeId, "ConnectionOpportunityId", entity.Id.ToString(), connectionRequestAttributes );
            }

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.ConnectionOpportunityId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<ConnectionOpportunityBag>
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
            var entityService = new ConnectionOpportunityService( RockContext );

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
        /// Gets the supplemental data for a new placement group, which we need in order to apply proper filtering logic.
        /// </summary>
        /// <param name="groupGuid">The group unique identifier.</param>
        /// <returns>A PlacementGroupBag containing the group type, campus, and archived status.</returns>
        [BlockAction]
        public BlockActionResult GetSupplementalDataForNewPlacementGroup( Guid groupGuid )
        {
            var group = new GroupService( RockContext ).Get( groupGuid );

            if ( group == null )
            {
                return ActionNotFound( "Group not found." );
            }

            var bag = new PlacementGroupBag
            {
                GroupType = group.GroupType != null ? new ListItemBag { Value = group.GroupType.Guid.ToString(), Text = group.GroupType.Name } : null,
                Campus = group.Campus != null ? new ListItemBag { Value = group.Campus.Guid.ToString(), Text = group.Campus.Name } : null,
                IsArchived = group.IsArchived
            };

            return ActionOk( bag );
        }

        [BlockAction]
		/// <summary>
		/// Gets the qualifier options for the specified workflow trigger type, scoped to the current connection type.
		/// </summary>
		/// <param name="triggerType">The trigger type as an integer value of <see cref="ConnectionWorkflowTriggerType"/>.</param>
		/// <returns>A result containing a list of qualifier options as <see cref="ListItemBag"/> values.</returns>
        public BlockActionResult GetConnectionWorkflowQualifierOptions( int triggerType )
        {
            var connectionType = ConnectionTypeCache.Get( PageParameter( PageParameterKey.ConnectionTypeId ), !PageCache.Layout.Site.DisablePredictableIds );
            var qualifierOptions = new List<ListItemBag>();

            var theTriggerType = ( ConnectionWorkflowTriggerType ) triggerType;

            switch ( theTriggerType )
            {
                case ConnectionWorkflowTriggerType.StatusChanged:
                // we don't use the QualifierValue DB column for manual trigger type, but we need the connection status options
                // to populate the dropdown for ManualTriggerFilterConnectionStatusId
                case ConnectionWorkflowTriggerType.Manual:
                    qualifierOptions = new ConnectionStatusService( RockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Where( stat => stat.ConnectionTypeId == connectionType.Id || stat.ConnectionTypeId == null )
                        .OrderBy( stat => stat.Name )
                        .Select( stat => new ListItemBag { Text = stat.Name, Value = stat.Id.ToString() } )
                        .ToList();

                    break;

                case ConnectionWorkflowTriggerType.StateChanged:
                    qualifierOptions = typeof( ConnectionState ).ToEnumListItemBag().ToList();
                    if ( connectionType != null && !connectionType.EnableFutureFollowup )
                    {
                        var futureFollowUp = ( ( int ) ConnectionState.FutureFollowUp ).ToString();
                        qualifierOptions = qualifierOptions.Where( o => o.Value != futureFollowUp ).ToList();
                    }

                    break;

                case ConnectionWorkflowTriggerType.ActivityAdded:
                    qualifierOptions = new ConnectionActivityTypeService( RockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Where( a => a.ConnectionTypeId == connectionType.Id )
                        .OrderBy( a => a.Name )
                        .Select( a => new ListItemBag { Text = a.Name, Value = a.Guid.ToString() } )
                        .ToList();

                    break;
            }

            var response = new ConnectionWorkflowQualifierOptionsResponseBag
            {
                QualifierOptions = qualifierOptions
            };

            return ActionOk( response );
        }

        [BlockAction]
		/// <summary>
		/// Gets the list of valid default connector options (people) for the supplied connector group GUIDs.
		/// </summary>
		/// <param name="bag">The request bag that contains connector group Guid values.</param>
		/// <returns>A list of distinct person options eligible to be default connectors.</returns>
        public BlockActionResult GetDefaultConnectorOptions( DefaultConnectorOptionsRequestBag bag )
        {
            if ( bag.GroupGuids == null || !bag.GroupGuids.Any() )
            {
                return ActionOk( new List<ListItemBag>() );
            }

            var validGroupGuids = bag.GroupGuids
                .Where( g => Guid.TryParse( g, out _ ) )
                .Select( Guid.Parse )
                .ToList();

            if ( !validGroupGuids.Any() )
            {
                return ActionOk( new List<ListItemBag>() );
            }

            var groupService = new GroupService( RockContext );
            var groupIds = groupService
                .Queryable()
                .AsNoTracking()
                .Where( g => validGroupGuids.Contains( g.Guid ) && g.IsActive )
                .Select( g => g.Id )
                .ToList();

            if ( !groupIds.Any() )
            {
                return ActionOk( new List<ListItemBag>() );
            }

            var connectionOpportunityId = PageParameter( PageParameterKey.ConnectionOpportunityId ).AsInteger();

            var defaultConnectorOptions = new GroupMemberService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Include( gm => gm.Person )
                .Where( gm =>
                    groupIds.Contains( gm.GroupId ) &&
                    gm.GroupMemberStatus == GroupMemberStatus.Active &&
                    gm.Person != null )
                .Select( gm => new ListItemBag
                {
                    Value = gm.Person.PrimaryAliasId.ToString(),
                    Text = ( gm.Person.NickName ?? gm.Person.FirstName ?? "" ) + " " + ( gm.Person.LastName ?? "" )
                } )
                .DistinctBy( p => p.Value )
                .ToList();

            return ActionOk( defaultConnectorOptions );
        }

        #endregion Block Actions
    }
}
