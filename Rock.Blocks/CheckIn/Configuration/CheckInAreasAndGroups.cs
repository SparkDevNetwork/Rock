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
using Rock.CheckIn;
using Rock.Enums.CheckIn;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInAreasAndGroups;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.CheckIn.Configuration
{
    /// <summary>
    /// Helps to build the areas and groups used for check-in.
    /// </summary>
    [DisplayName( "Check-in Areas and Groups" )]
    [Category( "Check-in > Configuration" )]
    [Description( "Helps to build the areas and groups used for check-in." )]
    [IconCssClass( "ti ti-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ContextAware( typeof( Campus ) )]

    #region Block Attributes

    [BooleanField(
        "Enable Classic Check-in Labels",
        Key = AttributeKey.EnableClassicCheckInLabels,
        Description = "Enabling this will allow you to add Classic Check-in Labels to this area.",
        DefaultBooleanValue = true,
        Order = 0,
        IsRequired = false )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "B648BB88-E6C2-4BFF-A3A6-FB601C602776" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "DBB6146E-31E6-4475-9C04-0A1F1561D29B" )]
    [Rock.SystemGuid.BlockTypeGuid( "B7CD296F-3AAB-4BA3-902C-44DB96C79798" )]
    public class CheckInAreasAndGroups : RockBlockType
    {
        #region Keys
        private static class AttributeKey
        {
            public const string EnableClassicCheckInLabels = "EnableClassicCheckInLabels";
        }

        private static class PageParameterKey
        {
            public const string CheckInConfiguration = "CheckInConfiguration";
        }

        private static class PersonPreferenceKey
        {
            /// <summary>
            /// Scoped to the check-in configuration GroupType entity (not the block) and shared with other check-in
            /// configuration blocks, so the area slicer selection persists across all blocks for the same configuration.
            /// Value is the area's Guid; empty means "All Areas".
            /// </summary>
            public const string SelectedArea = "checkin-config-selected-area";
            public const string ShowInactive = "show-inactive";
        }

        private static class NavigationUrlKey
        {
            public const string CreateCheckInLabel = "CreateCheckInLabel";
            public const string CreateClassicCheckInLabel = "CreateClassicCheckInLabel";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// The backing field for the <see cref="GroupTypeIdFromPageParameter"/> property.
        /// </summary>
        private int? _groupTypeIdFromPageParameter;

        /// <summary>
        /// The backing field for the <see cref="SelectedAreaId"/> property.
        /// </summary>
        private int? _selectedAreaId;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the check-in configuration <see cref="GroupType"/> entity key passed to the
        /// <see cref="PageParameterKey.CheckInConfiguration"/> page parameter.
        /// </summary>
        private string GroupTypeKeyFromPageParameter => PageParameter( PageParameterKey.CheckInConfiguration );

        /// <summary>
        /// Gets the check-in configuration <see cref="GroupType"/> Id resolved from the
        /// <see cref="PageParameterKey.CheckInConfiguration"/> page parameter.
        /// </summary>
        private int? GroupTypeIdFromPageParameter
        {
            get
            {
                if ( !_groupTypeIdFromPageParameter.HasValue )
                {
                    if ( GroupTypeKeyFromPageParameter.IsNullOrWhiteSpace() )
                    {
                        return null;
                    }

                    var groupType = GroupTypeCache.Get( GroupTypeKeyFromPageParameter, !PageCache.Layout.Site.DisablePredictableIds );
                    _groupTypeIdFromPageParameter = groupType?.Id;
                }

                return _groupTypeIdFromPageParameter;
            }
        }

        /// <summary>
        /// Gets the block person preferences.
        /// </summary>
        private PersonPreferenceCollection BlockPersonPreferences => this.GetBlockPersonPreferences();

        /// <summary>
        /// Gets the person preferences scoped to the current check-in configuration GroupType, or <c>null</c> when
        /// no configuration is resolved. Scoping to the configuration entity (rather than the block) is what allows
        /// preferences to be shared with other check-in configuration blocks.
        /// </summary>
        private PersonPreferenceCollection ConfigurationPersonPreferences
        {
            get
            {
                if ( !GroupTypeIdFromPageParameter.HasValue )
                {
                    return null;
                }

                var configuration = GroupTypeCache.Get( GroupTypeIdFromPageParameter.Value );
                return configuration != null ? GetScopedPersonPreferences( configuration ) : null;
            }
        }

        /// <summary>
        /// Gets the unique identifier of the currently-selected area from person preferences, or null if none is
        /// selected (i.e. the user has "All Areas" selected in the slicer).
        /// </summary>
        protected Guid? SelectedAreaGuid => ConfigurationPersonPreferences
            ?.GetValue( PersonPreferenceKey.SelectedArea )
            .AsGuidOrNull();

        /// <summary>
        /// Gets the identifier of the currently-selected area from person preferences, or null if none is selected
        /// (i.e. the user has "All Areas" selected in the slicer).
        /// </summary>
        private int? SelectedAreaId
        {
            get
            {
                if ( !_selectedAreaId.HasValue )
                {
                    if ( SelectedAreaGuid.HasValue )
                    {
                        _selectedAreaId = GroupTypeCache.GetId( SelectedAreaGuid.Value );
                    }
                }

                return _selectedAreaId;
            }
        }

        /// <summary>
        /// Gets a value indicating whether inactive groups should be included in the tree. Sourced from the block
        /// person preference; defaults to <c>false</c> so the tree hides inactive groups out of the box.
        /// </summary>
        private bool ShowInactive => BlockPersonPreferences
            .GetValue( PersonPreferenceKey.ShowInactive )
            .AsBoolean();

        /// <summary>
        /// Gets the campus identifier from the request context, if defined.
        /// </summary>
        private int? ContextCampusId => RequestContext.GetContextEntity<Campus>()?.Id;

        #endregion Properties

        #region RockBlockType Implementation

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CheckInAreasAndGroupsInitializationBox();

            var configuration = GroupTypeIdFromPageParameter.HasValue
                ? GroupTypeCache.Get( GroupTypeIdFromPageParameter.Value )
                : null;

            if ( configuration == null )
            {
                box.ErrorMessage = "Check-in Configuration not found.";
                return box;
            }

            box.CheckInTypeName = configuration.Name;
            box.AreaItems = GetAreaOptions( configuration.Id );
            box.ConfigurationIdKey = configuration.IdKey;
            box.SelectedAreaGuid = SelectedAreaGuid;
            box.Tree = BuildTree( configuration, SelectedAreaId );
            box.InheritedGroupTypeOptions = GetInheritedGroupTypeOptions();
            box.InheritedAttributesByGuid = GetInheritedAttributesByGuid();
            box.IsClassicCheckInLabelsEnabled = GetAttributeValue( AttributeKey.EnableClassicCheckInLabels ).AsBoolean();
            box.CampusRootLocations = GetCampusRootLocations();
            box.NavigationUrls = GetBoxNavigationUrls();

            return box;
        }

        #endregion RockBlockType Implementation

        #region Block Actions

        /// <summary>
        /// Rebuilds the areas-and-groups tree for the current configuration, respecting the active
        /// "Selected Area" person preference and the current "Show Inactive Groups" toggle. The client uses
        /// this on filter-change events (slicer campus change, Areas dropdown change, list-settings save) to
        /// avoid a full block reload.
        /// </summary>
        /// <returns><see cref="ActionOk"/> with the new tree, or <see cref="ActionBadRequest"/> when the
        /// configuration can't be resolved.</returns>
        [BlockAction]
        public BlockActionResult GetTree()
        {
            if ( !GroupTypeIdFromPageParameter.HasValue )
            {
                return ActionBadRequest( "Check-in Configuration not found." );
            }

            var configuration = GroupTypeCache.Get( GroupTypeIdFromPageParameter.Value );
            if ( configuration == null )
            {
                return ActionBadRequest( "Check-in Configuration not found." );
            }

            return ActionOk( BuildTree( configuration, SelectedAreaId ) );
        }

        /// <summary>
        /// Reorders an area among its siblings, applying the new positions to the siblings'
        /// <see cref="GroupType.Order"/> values and clearing the kiosk device cache so connected kiosks pick up
        /// the new presentation order on their next configuration refresh.
        /// </summary>
        /// <param name="bag">The reorder request payload.</param>
        /// <returns><see cref="ActionOk"/> on success, <see cref="ActionBadRequest"/> when the request is invalid.</returns>
        [BlockAction]
        public BlockActionResult ReorderArea( ReorderAreaRequestBag bag )
        {
            if ( bag == null || bag.IdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Invalid reorder request." );
            }

            if ( !GroupTypeIdFromPageParameter.HasValue )
            {
                return ActionBadRequest( "Check-in Configuration not found." );
            }

            var usePredictableIds = !PageCache.Layout.Site.DisablePredictableIds;
            var groupTypeService = new GroupTypeService( RockContext );

            var movedArea = GroupTypeCache.Get( bag.IdKey, usePredictableIds );
            if ( movedArea == null || !IsAreaInCurrentConfiguration( movedArea.Id ) )
            {
                return ActionBadRequest( "Area not found." );
            }

            // The sibling list always sits underneath one specific parent: the configuration when ParentAreaIdKey
            // is null, or another area when it is set. Resolve that parent's id first so the rendered-tree walk
            // below can be scoped to it.
            int parentId;
            if ( bag.ParentAreaIdKey.IsNullOrWhiteSpace() )
            {
                parentId = GroupTypeIdFromPageParameter.Value;
            }
            else
            {
                var parentArea = GroupTypeCache.Get( bag.ParentAreaIdKey, usePredictableIds );
                if ( parentArea == null || !IsAreaInCurrentConfiguration( parentArea.Id ) )
                {
                    return ActionBadRequest( "Parent area not found." );
                }
                parentId = parentArea.Id;
            }

            // Compute the rendered sibling list using the same visited-once walk the tree-builder uses, so a
            // multi-parent area never has its globally-shared GroupType.Order rewritten from a parent the UI
            // didn't render it under. Mirrors legacy behavior, which sourced siblings from rendered rows.
            var renderedSiblingIds = GetRenderedAreaSiblingIds( parentId );
            if ( renderedSiblingIds.Count == 0 || !renderedSiblingIds.Contains( movedArea.Id ) )
            {
                return ActionBadRequest( "Invalid reorder attempt." );
            }

            // Materialize the rendered siblings as tracked entities in rendered order so ReorderEntity can
            // splice the moved entry into its new slot and rewrite Order = 0..N-1 in one pass.
            var siblingsById = groupTypeService
                .Queryable()
                .Where( gt => renderedSiblingIds.Contains( gt.Id ) )
                .ToDictionary( gt => gt.Id );

            var siblings = renderedSiblingIds
                .Where( id => siblingsById.ContainsKey( id ) )
                .Select( id => siblingsById[id] )
                .ToList();

            if ( !siblings.ReorderEntity( bag.IdKey, bag.BeforeIdKey ) )
            {
                return ActionBadRequest( "Invalid reorder attempt." );
            }

            RockContext.SaveChanges();

            RefreshConnectedKiosks();

            return ActionOk();
        }

        /// <summary>
        /// Reorders a group among its siblings, applying the new positions to the siblings'
        /// <see cref="Group.Order"/> values and clearing the kiosk device cache so connected kiosks pick up
        /// the new presentation order on their next configuration refresh.
        /// </summary>
        /// <param name="bag">The reorder request payload.</param>
        /// <returns><see cref="ActionOk"/> on success, <see cref="ActionBadRequest"/> when the request is invalid.</returns>
        [BlockAction]
        public BlockActionResult ReorderGroup( ReorderGroupRequestBag bag )
        {
            if ( bag == null || bag.IdKey.IsNullOrWhiteSpace() || bag.ParentIdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Invalid reorder request." );
            }

            var usePredictableIds = !PageCache.Layout.Site.DisablePredictableIds;
            var groupService = new GroupService( RockContext );

            List<Model.Group> siblings;

            // Always pull every sibling (active + inactive), regardless of the "Show Inactive" preference.
            // ReorderEntity rewrites Order = 0..N-1 across the returned set, so excluding hidden inactive
            // groups would leave their original Order values behind to collide with the rewritten active
            // ones. Including them keeps the persisted Order internally consistent at the cost of letting
            // a visible drag implicitly shift the position of an adjacent inactive sibling.
            if ( bag.ParentNodeType == CheckInTreeNodeType.Area )
            {
                var parentArea = GroupTypeCache.Get( bag.ParentIdKey, usePredictableIds );
                if ( parentArea == null || !IsAreaInCurrentConfiguration( parentArea.Id ) )
                {
                    return ActionBadRequest( "Parent area not found." );
                }

                var parentAreaId = parentArea.Id;
                siblings = groupService
                    .Queryable()
                    .Where( g =>
                        g.GroupTypeId == parentAreaId
                        && (
                            g.ParentGroupId == null
                            || g.ParentGroup.GroupTypeId != parentAreaId
                        )
                    )
                    .OrderBy( g => g.Order )
                    .ThenBy( g => g.Name )
                    .ToList();
            }
            else if ( bag.ParentNodeType == CheckInTreeNodeType.Group )
            {
                var parentGroup = GroupCache.Get( bag.ParentIdKey, usePredictableIds );
                if ( parentGroup == null || !IsGroupTypeIdInCurrentConfiguration( parentGroup.GroupTypeId ) )
                {
                    return ActionBadRequest( "Parent group not found." );
                }

                var parentGroupId = parentGroup.Id;
                var parentGroupTypeId = parentGroup.GroupTypeId;
                siblings = groupService
                    .Queryable()
                    .Where( g =>
                        g.ParentGroupId == parentGroupId
                        && g.GroupTypeId == parentGroupTypeId
                    )
                    .OrderBy( g => g.Order )
                    .ThenBy( g => g.Name )
                    .ToList();
            }
            else
            {
                return ActionBadRequest( "Unsupported parent node type." );
            }

            if ( !siblings.ReorderEntity( bag.IdKey, bag.BeforeIdKey ) )
            {
                return ActionBadRequest( "Invalid reorder attempt." );
            }

            RockContext.SaveChanges();

            RefreshConnectedKiosks();

            return ActionOk();
        }

        /// <summary>
        /// Loads the editable detail for a single area.
        /// </summary>
        /// <param name="idKey">The hashed identifier of the area to load.</param>
        /// <returns><see cref="ActionOk"/> with the populated <see cref="AreaDetailBag"/>, or
        /// <see cref="ActionNotFound"/> when the area can't be located.</returns>
        [BlockAction]
        public BlockActionResult GetAreaDetail( string idKey )
        {
            var usePredictableIds = !PageCache.Layout.Site.DisablePredictableIds;
            var area = GroupTypeCache.Get( idKey, usePredictableIds );
            if ( area == null || !IsAreaInCurrentConfiguration( area.Id ) )
            {
                return ActionNotFound( "Area not found." );
            }

            return ActionOk( ToAreaDetailBag( area ) );
        }

        /// <summary>
        /// Saves a check-in area. Empty <see cref="AreaDetailBag.IdKey"/> creates; otherwise updates.
        /// </summary>
        /// <param name="bag">The save request payload.</param>
        /// <returns><see cref="ActionOk"/> with the saved <see cref="AreaDetailBag"/>, or
        /// <see cref="ActionBadRequest"/>/<see cref="ActionNotFound"/> when the request is invalid.</returns>
        [BlockAction]
        public BlockActionResult SaveArea( SaveAreaRequestBag bag )
        {
            if ( bag?.Area == null )
            {
                return ActionBadRequest( "Invalid area request." );
            }

            if ( bag.Area.Name.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Area name is required." );
            }

            if ( !GroupTypeIdFromPageParameter.HasValue )
            {
                return ActionBadRequest( "Check-in Configuration not found." );
            }

            var usePredictableIds = !PageCache.Layout.Site.DisablePredictableIds;
            var groupTypeService = new GroupTypeService( RockContext );

            GroupType area;
            if ( bag.Area.IdKey.IsNullOrWhiteSpace() )
            {
                // Resolve the parent the new area is attached to: the supplied parent area when the client is
                // creating "under" an existing area, or the check-in configuration itself for a top-level add.
                GroupType parent;
                if ( bag.ParentAreaIdKey.IsNullOrWhiteSpace() )
                {
                    parent = groupTypeService.Get( GroupTypeIdFromPageParameter.Value );
                }
                else
                {
                    parent = groupTypeService.Get( bag.ParentAreaIdKey, usePredictableIds );
                    if ( parent == null || !IsAreaInCurrentConfiguration( parent.Id ) )
                    {
                        return ActionBadRequest( "Parent area not found." );
                    }
                }

                // Defaults mirror the legacy "Add Area" logic.
                area = new GroupType
                {
                    IsSystem = false,
                    ShowInNavigation = false,
                    TakesAttendance = true,
                    AllowMultipleLocations = true,
                    EnableLocationSchedules = true,
                    Order = parent.ChildGroupTypes.Any()
                        ? parent.ChildGroupTypes.Max( c => c.Order ) + 1
                        : 0
                };
                area.Roles.Add( new GroupTypeRole { Name = "Member" } );

                parent.ChildGroupTypes.Add( area );
                groupTypeService.Add( area );
            }
            else
            {
                area = groupTypeService.Get( bag.Area.IdKey, usePredictableIds );
                if ( area == null || !IsAreaInCurrentConfiguration( area.Id ) )
                {
                    return ActionNotFound( "Area not found." );
                }
            }

            area.Name = bag.Area.Name;
            area.IsConcurrentCheckInPrevented = bag.Area.IsConcurrentCheckInPrevented;
            area.AttendanceRule = bag.Area.AttendanceRule;
            area.AlreadyEnrolledMatchingLogic = bag.Area.AlreadyEnrolledMatchingLogic;
            area.AttendancePrintTo = bag.Area.AttendancePrintTo;

            var priorInheritedGroupTypeId = area.InheritedGroupTypeId;
            area.InheritedGroupTypeId = ResolveInheritedGroupTypeId( priorInheritedGroupTypeId, bag.Area.InheritedGroupTypeGuid );

            // Surface entity-level validation failures.
            if ( !area.IsValid )
            {
                var validationMessages = area.ValidationResults
                    .Select( r => r.ErrorMessage )
                    .ToList()
                    .AsDelimited( "</li><li>" );

                return ActionBadRequest( $"Please correct the following:<ul><li>{validationMessages}</li></ul>" );
            }

            // Load the attribute schema (resolves through the inherit-from chain) before staging values
            // so SetPublicAttributeValues has the schema to translate against and SaveAttributeValues has
            // something to write after SaveChanges assigns an Id.
            area.LoadAttributes( RockContext );

            if ( bag.Area.AttributeValues != null )
            {
                area.SetPublicAttributeValues( bag.Area.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: false );
            }

            RockContext.WrapTransaction( () =>
            {
                // First SaveChanges persists the area entity (assigning Id for create), any newly-added role on a
                // new area, and the GroupTypeAssociation linking parent to area. Defer the default-role linkage
                // until after this save so the role has an Id; setting DefaultGroupRoleId via the nav property
                // up-front creates a circular FK dependency between area.DefaultGroupRoleId and role.GroupTypeId
                // that EF can't resolve.
                RockContext.SaveChanges();
                area.SaveAttributeValues( RockContext );

                // Match legacy: link the default role by value now that the role has an Id. Skipped when
                // the area already has a default or has no roles at all.
                if ( !area.DefaultGroupRoleId.HasValue && area.Roles.Any() )
                {
                    area.DefaultGroupRoleId = area.Roles.First().Id;
                }

                // Persist label attachments. The classic rebuild deletes-then-recreates Attribute rows,
                // which is split across two SaveChanges so unique-key collisions on attribute Key don't
                // fire when a new label happens to share a file name with one being deleted.
                SaveClassicCheckInLabels( area, bag.Area.ClassicCheckInLabels );
                RockContext.SaveChanges();

                SaveCheckInLabels( area.Id, bag.Area.CheckInLabels );
                RockContext.SaveChanges();
            } );

            // When the area's InheritedGroupTypeId changed, every group whose GroupTypeId is this area now has
            // a stale resolved attribute schema in GroupCache. The framework only invalidates the directly-touched
            // GroupType entry, not descendant groups, so flush them by hand. The editor's read path re-resolves on
            // the entity (see GetGroupDetail), but public-facing check-in consumes GroupCache.Attributes at runtime
            // and would otherwise apply the prior chain's filters until each entry organically refreshed.
            if ( priorInheritedGroupTypeId != area.InheritedGroupTypeId )
            {
                FlushGroupCachesForAreaInheritedAttributes( area.Id );
            }

            RefreshConnectedKiosks();

            return ActionOk( ToAreaDetailBag( GroupTypeCache.Get( area.Id ) ) );
        }

        /// <summary>
        /// Deletes a check-in area.
        /// </summary>
        /// <param name="idKey">The hashed identifier of the area to delete.</param>
        /// <returns><see cref="ActionOk"/> on success, otherwise <see cref="ActionBadRequest"/> or
        /// <see cref="ActionNotFound"/>.</returns>
        [BlockAction]
        public BlockActionResult DeleteArea( string idKey )
        {
            var usePredictableIds = !PageCache.Layout.Site.DisablePredictableIds;
            var groupTypeService = new GroupTypeService( RockContext );

            var area = groupTypeService.Get( idKey, usePredictableIds );
            if ( area == null || !IsAreaInCurrentConfiguration( area.Id ) )
            {
                return ActionNotFound( "Area not found." );
            }

            // Refuse delete when this area or any descendant is referenced as an InheritedGroupTypeId.
            if ( IsAreaOrDescendantInherited( area.Id ) )
            {
                return ActionBadRequest( "Cannot delete. This group type or one of its child group types is assigned as an inherited group type." );
            }

            if ( !groupTypeService.CanDelete( area, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            // Disconnect M:M associations to avoid GroupTypeAssociation FK violations on delete.
            area.ParentGroupTypes.Clear();
            area.ChildGroupTypes.Clear();

            groupTypeService.Delete( area );
            RockContext.SaveChanges();

            RefreshConnectedKiosks();

            return ActionOk();
        }

        /// <summary>
        /// Returns the check-in labels that are eligible to attach to the supplied area: every <see cref="CheckInLabel"/>
        /// in the system, minus the ones already attached to this area.
        /// </summary>
        /// <param name="areaIdKey">
        /// The hashed identifier of the area being edited. <c>null</c> or empty for an unsaved area, in which case
        /// every label is eligible.</param>
        /// <returns><see cref="ActionOk"/> with the list of eligible labels.</returns>
        [BlockAction]
        public BlockActionResult GetAvailableCheckInLabels( string areaIdKey )
        {
            var usePredictableIds = !PageCache.Layout.Site.DisablePredictableIds;
            var areaId = areaIdKey.IsNotNullOrWhiteSpace()
                ? GroupTypeCache.Get( areaIdKey, usePredictableIds )?.Id
                : null;

            if ( areaId.HasValue && !IsAreaInCurrentConfiguration( areaId.Value ) )
            {
                return ActionNotFound( "Area not found." );
            }

            var alreadyAttachedIds = areaId.HasValue
                ? GetAreaCheckInLabelRelatedEntityQuery( areaId.Value )
                    .Select( re => re.TargetEntityId )
                    .ToHashSet()
                : new HashSet<int>();

            var available = new CheckInLabelService( RockContext )
                .Queryable()
                .Where( cl =>
                    cl.IsActive
                    && !alreadyAttachedIds.Contains( cl.Id )
                )
                .OrderBy( cl => cl.Name )
                .Select( cl => new { cl.Id, cl.Name } )
                .ToList()
                .Select( cl => new ListItemBag
                {
                    Value = cl.Id.AsIdKey(),
                    Text = cl.Name
                } )
                .ToList();

            return ActionOk( available );
        }

        /// <summary>
        /// Returns the classic check-in labels that are eligible to attach to the supplied area: every
        /// non-temporary binary file of type <see cref="SystemGuid.BinaryFiletype.CHECKIN_LABEL"/>, minus
        /// the ones already attached to this area as label attributes.
        /// </summary>
        /// <param name="areaIdKey">
        /// The hashed identifier of the area being edited. <c>null</c> or empty for an unsaved area, in which case
        /// every check-in-label binary file is eligible.
        /// </param>
        /// <returns><see cref="ActionOk"/> with the list of eligible labels.</returns>
        [BlockAction]
        public BlockActionResult GetAvailableClassicCheckInLabels( string areaIdKey )
        {
            var usePredictableIds = !PageCache.Layout.Site.DisablePredictableIds;
            var area = areaIdKey.IsNotNullOrWhiteSpace()
                ? GroupTypeCache.Get( areaIdKey, usePredictableIds )
                : null;

            if ( area != null && !IsAreaInCurrentConfiguration( area.Id ) )
            {
                return ActionNotFound( "Area not found." );
            }

            var alreadyAttachedBinaryFileGuids = area != null
                ? GetAttachedClassicCheckInLabels( area.Attributes )
                    .Select( l => l.BinaryFileGuid )
                    .ToHashSet()
                : new HashSet<Guid>();

            var checkInLabelBinaryFileTypeGuid = SystemGuid.BinaryFiletype.CHECKIN_LABEL.AsGuid();

            var available = new BinaryFileService( RockContext )
                .Queryable()
                .Where( bf =>
                    bf.BinaryFileType.Guid == checkInLabelBinaryFileTypeGuid
                    && !bf.IsTemporary
                    && !alreadyAttachedBinaryFileGuids.Contains( bf.Guid )
                )
                .OrderBy( bf => bf.FileName )
                .Select( bf => new { bf.Guid, bf.FileName } )
                .ToList()
                .Select( bf => new ListItemBag
                {
                    Value = bf.Guid.ToString(),
                    Text = bf.FileName
                } )
                .ToList();

            return ActionOk( available );
        }

        /// <summary>
        /// Loads the editable detail for a single group.
        /// </summary>
        /// <param name="idKey">The hashed identifier of the group to load.</param>
        /// <returns><see cref="ActionOk"/> with the populated <see cref="GroupDetailBag"/>, or
        /// <see cref="ActionNotFound"/> when the group can't be located.</returns>
        [BlockAction]
        public BlockActionResult GetGroupDetail( string idKey )
        {
            var usePredictableIds = !PageCache.Layout.Site.DisablePredictableIds;
            var group = new GroupService( RockContext ).Get( idKey, usePredictableIds );
            if ( group == null || !IsGroupTypeIdInCurrentConfiguration( group.GroupTypeId ) )
            {
                return ActionNotFound( $"{Model.Group.FriendlyTypeName} not found." );
            }

            // Re-resolve the inherit chain on the entity rather than reading the cached projection.
            // GroupCache.Attributes never invalidates when an ancestor area's InheritedGroupTypeId
            // changes, so the chain walk has to happen against a fresh entity to surface the right
            // filter schema.
            group.LoadAttributes( RockContext );

            return ActionOk( ToGroupDetailBag( group ) );
        }

        /// <summary>
        /// Resolves the group-level attribute schema that a new group placed under the supplied parent would inherit,
        /// so the placeholder editor can render the Check-in Filters section immediately without waiting for a first
        /// save. Walks the inherit chain via a phantom <see cref="Model.Group"/> + <c>LoadAttributes</c>, which is
        /// the same resolution path <see cref="GetGroupDetail"/> uses for saved groups, so the schema returned here
        /// matches what the server would persist against on save. Accepts the same parent-identification shape as
        /// <see cref="SaveGroup"/> so the caller doesn't need to walk the tree to find the owning area.
        /// </summary>
        /// <param name="parentIdKey"> The hashed identifier of the placeholder's immediate parent (an area or a group).</param>
        /// <param name="parentNodeType">Whether <paramref name="parentIdKey"/> identifies an area or a group.</param>
        /// <returns>
        /// <see cref="ActionOk"/> with the projected schema (may be empty when the area has no inherited filter
        /// attributes), or <see cref="ActionNotFound"/> / <see cref="ActionBadRequest"/> when the parent can't be
        /// located in this configuration.
        /// </returns>
        [BlockAction]
        public BlockActionResult GetPlaceholderGroupSchema( string parentIdKey, CheckInTreeNodeType parentNodeType )
        {
            var usePredictableIds = !PageCache.Layout.Site.DisablePredictableIds;

            int areaId;
            if ( parentNodeType == CheckInTreeNodeType.Area )
            {
                var parentArea = GroupTypeCache.Get( parentIdKey, usePredictableIds );
                if ( parentArea == null || !IsAreaInCurrentConfiguration( parentArea.Id ) )
                {
                    return ActionNotFound( "Parent area not found." );
                }

                areaId = parentArea.Id;
            }
            else if ( parentNodeType == CheckInTreeNodeType.Group )
            {
                var parentGroup = GroupCache.Get( parentIdKey, usePredictableIds );
                if ( parentGroup == null || !IsGroupTypeIdInCurrentConfiguration( parentGroup.GroupTypeId ) )
                {
                    return ActionNotFound( $"Parent {Model.Group.FriendlyTypeName.ToLower()} not found." );
                }

                areaId = parentGroup.GroupTypeId;
            }
            else
            {
                return ActionBadRequest( "Unsupported parent node type." );
            }

            // The phantom Group is never saved; it just gives LoadAttributes a real entity to walk the inherit chain
            // against, identical to what a freshly-created group under this area would produce on first save.
            var phantom = new Model.Group { GroupTypeId = areaId };
            phantom.LoadAttributes( RockContext );

            var (filters, ownAttributes) = phantom.Attributes != null
                ? SplitGroupAttributeSchema( phantom.Attributes, areaId )
                : (new Dictionary<string, PublicAttributeBag>(), new Dictionary<string, PublicAttributeBag>());

            return ActionOk( new PlaceholderGroupSchemaBag
            {
                Attributes = filters,
                GroupAttributes = ownAttributes,
                InheritedGroupTypeName = GetInheritedSetupTypeName( areaId )
            } );
        }

        /// <summary>
        /// Saves a check-in group. Empty <see cref="GroupDetailBag.IdKey"/> creates; otherwise updates.
        /// </summary>
        /// <param name="bag">The save request payload.</param>
        /// <returns><see cref="ActionOk"/> with the saved <see cref="GroupDetailBag"/>, or
        /// <see cref="ActionBadRequest"/>/<see cref="ActionNotFound"/> when the request is invalid.</returns>
        [BlockAction]
        public BlockActionResult SaveGroup( SaveGroupRequestBag bag )
        {
            if ( bag?.Group == null )
            {
                return ActionBadRequest( "Invalid group request." );
            }

            if ( bag.Group.Name.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( $"{Model.Group.FriendlyTypeName} name is required." );
            }

            var usePredictableIds = !PageCache.Layout.Site.DisablePredictableIds;
            var groupService = new GroupService( RockContext );

            Model.Group group;
            if ( bag.Group.IdKey.IsNullOrWhiteSpace() )
            {
                if ( bag.ParentIdKey.IsNullOrWhiteSpace() )
                {
                    return ActionBadRequest( $"Parent identifier is required to create a new {Model.Group.FriendlyTypeName.ToLower()}." );
                }

                int groupTypeId;
                int? parentGroupId;
                int nextOrder;

                if ( bag.ParentNodeType == CheckInTreeNodeType.Area )
                {
                    var parentArea = GroupTypeCache.Get( bag.ParentIdKey, usePredictableIds );
                    if ( parentArea == null || !IsAreaInCurrentConfiguration( parentArea.Id ) )
                    {
                        return ActionBadRequest( "Parent area not found." );
                    }

                    groupTypeId = parentArea.Id;
                    parentGroupId = null;
                    nextOrder = groupService
                        .Queryable()
                        .Where( g =>
                            g.GroupTypeId == groupTypeId
                            && (
                                g.ParentGroupId == null
                                || g.ParentGroup.GroupTypeId != groupTypeId
                            )
                        )
                        .Select( g => ( int? ) g.Order )
                        .Max() + 1 ?? 0;
                }
                else if ( bag.ParentNodeType == CheckInTreeNodeType.Group )
                {
                    var parentGroup = GroupCache.Get( bag.ParentIdKey, usePredictableIds );
                    if ( parentGroup == null || !IsGroupTypeIdInCurrentConfiguration( parentGroup.GroupTypeId ) )
                    {
                        return ActionBadRequest( $"Parent {Model.Group.FriendlyTypeName.ToLower()} not found." );
                    }

                    groupTypeId = parentGroup.GroupTypeId;
                    parentGroupId = parentGroup.Id;
                    nextOrder = groupService
                        .Queryable()
                        .Where( g =>
                            g.ParentGroupId == parentGroupId
                            && g.GroupTypeId == groupTypeId
                        )
                        .Select( g => ( int? ) g.Order )
                        .Max() + 1 ?? 0;
                }
                else
                {
                    return ActionBadRequest( "Unsupported parent node type." );
                }

                group = new Model.Group
                {
                    GroupTypeId = groupTypeId,
                    ParentGroupId = parentGroupId,
                    Order = nextOrder,
                    IsActive = true,
                    IsPublic = true
                };
                groupService.Add( group );
            }
            else
            {
                group = groupService.Get( bag.Group.IdKey, usePredictableIds );
                if ( group == null || !IsGroupTypeIdInCurrentConfiguration( group.GroupTypeId ) )
                {
                    return ActionNotFound( $"{Model.Group.FriendlyTypeName} not found." );
                }

                // Eager-load the attached locations and each row's schedule configs so the reconcile below
                // doesn't lazy-load a removed row's GroupLocationScheduleConfigs one query at a time.
                RockContext.Entry( group )
                    .Collection( g => g.GroupLocations )
                    .Query()
                    .Include( gl => gl.GroupLocationScheduleConfigs )
                    .Load();
            }

            group.Name = bag.Group.Name;
            group.IsActive = bag.Group.IsActive;
            group.IsSpecialNeeds = bag.Group.IsSpecialNeeds;

            if ( !group.IsValid )
            {
                var validationMessages = group.ValidationResults
                    .Select( r => r.ErrorMessage )
                    .ToList()
                    .AsDelimited( "</li><li>" );

                return ActionBadRequest( $"Please correct the following:<ul><li>{validationMessages}</li></ul>" );
            }

            // Update the group's attached locations before saving so location changes save together with the rest of
            // the group. The order locations appear in the bag is the order they save in. The bag carries every
            // campus's locations, so this is a full reconcile over the group's complete location set.
            if ( !TryReconcileGroupLocations( group, bag.Group, out var locationError ) )
            {
                return ActionBadRequest( locationError );
            }

            // Load this group's attributes (including any inherited from a parent group type) and apply the new
            // values. The group's basic fields and its attribute values save together. Values for attributes that no
            // longer apply (e.g. after switching the inherited type) get silently ignored rather than written to the
            // wrong attribute.
            group.LoadAttributes( RockContext );
            if ( bag.Group.AttributeValues != null )
            {
                group.SetPublicAttributeValues( bag.Group.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: false );
            }

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                group.SaveAttributeValues( RockContext );
            } );

            RefreshConnectedKiosks();

            return ActionOk( ToGroupDetailBag( group ) );
        }

        /// <summary>
        /// Deletes a check-in group.
        /// </summary>
        /// <param name="idKey">The hashed identifier of the group to delete.</param>
        /// <returns><see cref="ActionOk"/> on success, otherwise <see cref="ActionBadRequest"/> or
        /// <see cref="ActionNotFound"/>.</returns>
        [BlockAction]
        public BlockActionResult DeleteGroup( string idKey )
        {
            var usePredictableIds = !PageCache.Layout.Site.DisablePredictableIds;
            var groupService = new GroupService( RockContext );

            var group = groupService.Get( idKey, usePredictableIds );
            if ( group == null || !IsGroupTypeIdInCurrentConfiguration( group.GroupTypeId ) )
            {
                return ActionNotFound( $"{Model.Group.FriendlyTypeName} not found." );
            }

            if ( !groupService.CanDelete( group, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            // Mirror the legacy soft-delete cascade: hard-delete when the group is already inactive or
            // has no attendance history, otherwise just flip it to inactive so historical attendance
            // records aren't orphaned. The attendance lookup only runs when the group is still active
            // to avoid a needless round trip on the already-inactive case.
            if ( !group.IsActive )
            {
                groupService.Delete( group );
            }
            else
            {
                var hasAttendance = new AttendanceService( RockContext )
                    .Queryable()
                    .Any( a => a.Occurrence.GroupId == group.Id );

                if ( hasAttendance )
                {
                    group.IsActive = false;
                }
                else
                {
                    groupService.Delete( group );
                }
            }

            RockContext.SaveChanges();

            RefreshConnectedKiosks();

            return ActionOk();
        }

        #endregion Block Actions

        #region Private Methods

        /// <summary>
        /// Returns true when <paramref name="areaId"/> is reachable from the current check-in configuration's area
        /// subtree. Used as an authorization guard so block actions can refuse to read or mutate an area/group whose
        /// <see cref="GroupType"/> doesn't actually belong to the configuration in the URL (preventing a client from
        /// poking at a different configuration's entities by id).
        /// </summary>
        /// <param name="areaId">The candidate area's <see cref="GroupType.Id"/>.</param>
        /// <returns><c>true</c> when the area is reachable from the configuration; <c>false</c> otherwise.</returns>
        private bool IsAreaInCurrentConfiguration( int areaId )
        {
            if ( !GroupTypeIdFromPageParameter.HasValue )
            {
                return false;
            }

            if ( areaId == GroupTypeIdFromPageParameter.Value )
            {
                return false;
            }

            var configuration = GroupTypeCache.Get( GroupTypeIdFromPageParameter.Value );
            if ( configuration == null )
            {
                return false;
            }

            var checkInFilterPurposeGuid = SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_FILTER.AsGuid();
            var areaIds = new HashSet<int>();
            var visited = new HashSet<int> { configuration.Id };
            CollectDescendantAreaIds( configuration, areaIds, visited, checkInFilterPurposeGuid );
            return areaIds.Contains( areaId );
        }

        /// <summary>
        /// Returns true when the supplied group lives under an area belonging to the current check-in configuration.
        /// A group is in-scope when its owning area (the <see cref="Model.Group.GroupTypeId"/>) is in-scope.
        /// </summary>
        /// <param name="groupTypeId">The candidate group's owning <see cref="GroupType.Id"/>.</param>
        /// <returns><c>true</c> when the group's area is reachable from the configuration; <c>false</c> otherwise.</returns>
        private bool IsGroupTypeIdInCurrentConfiguration( int groupTypeId )
        {
            return IsAreaInCurrentConfiguration( groupTypeId );
        }

        /// <summary>
        /// Returns true when the area or any descendant is referenced as an <c>InheritedGroupTypeId</c>
        /// elsewhere.
        /// </summary>
        /// <param name="areaId">The id of the area being considered for delete.</param>
        /// <returns><c>true</c> if any inheritance reference would be left dangling.</returns>
        private bool IsAreaOrDescendantInherited( int areaId )
        {
            var idsAtRisk = new HashSet<int>();
            var queue = new Queue<int>();

            if ( idsAtRisk.Add( areaId ) )
            {
                queue.Enqueue( areaId );
            }

            while ( queue.Count > 0 )
            {
                var node = GroupTypeCache.Get( queue.Dequeue() );
                if ( node == null )
                {
                    continue;
                }

                foreach ( var child in node.ChildGroupTypes )
                {
                    if ( idsAtRisk.Add( child.Id ) )
                    {
                        queue.Enqueue( child.Id );
                    }
                }
            }

            return GroupTypeCache.All( RockContext )
                .Any( gt => gt.InheritedGroupTypeId.HasValue
                            && idsAtRisk.Contains( gt.InheritedGroupTypeId.Value ) );
        }

        /// <summary>
        /// Builds the editable bag representation of a single area. The matching schema is shipped once on the box's
        /// <c>InheritedAttributesByGuid</c>; the bag only carries the values, keyed by attribute key.
        /// </summary>
        /// <param name="area">The cached area to project.</param>
        /// <returns>The bag carrying the area's editable fields and its current attribute values.</returns>
        private AreaDetailBag ToAreaDetailBag( GroupTypeCache area )
        {
            return new AreaDetailBag
            {
                IdKey = area.IdKey,
                Name = area.Name,
                IsConcurrentCheckInPrevented = area.IsConcurrentCheckInPrevented,
                InheritedGroupTypeGuid = area.InheritedGroupTypeId.HasValue
                    ? GroupTypeCache.Get( area.InheritedGroupTypeId.Value )?.Guid
                    : null,
                AttendanceRule = area.AttendanceRule,
                AlreadyEnrolledMatchingLogic = area.AlreadyEnrolledMatchingLogic,
                AttendancePrintTo = area.AttendancePrintTo,
                AttributeValues = area.GetPublicAttributeValuesForEdit( RequestContext.CurrentPerson, enforceSecurity: false ),
                CheckInLabels = GetAttachedCheckInLabels( area.Id ),
                ClassicCheckInLabels = GetAttachedClassicCheckInLabels( area.Attributes )
            };
        }

        /// <summary>
        /// Builds the editable bag representation of a single group. Callers must call
        /// <see cref="Rock.Attribute.IHasAttributes.LoadAttributes()"/> on the group before projection so the
        /// inherited filter schema is freshly resolved from the chain rather than read from a potentially stale
        /// <see cref="GroupCache.Attributes"/>.
        /// </summary>
        /// <param name="group">The group entity to project, with attributes already loaded.</param>
        /// <returns>The bag carrying the group's editable fields plus the parent area's display name for the
        /// editor breadcrumb.</returns>
        private GroupDetailBag ToGroupDetailBag( Model.Group group )
        {
            var attachedLocations = GetAttachedNamedLocations( group.Id );

            // Split attached locations into Main vs Overflow in a single pass.
            List<NamedLocationBag> mainLocations = null;
            List<NamedLocationBag> overflowLocations = null;
            if ( attachedLocations != null )
            {
                foreach ( var item in attachedLocations )
                {
                    if ( item.IsOverflow )
                    {
                        overflowLocations ??= new List<NamedLocationBag>();
                        overflowLocations.Add( item.Bag );
                    }
                    else
                    {
                        mainLocations ??= new List<NamedLocationBag>();
                        mainLocations.Add( item.Bag );
                    }
                }
            }

            Dictionary<string, PublicAttributeBag> filterAttributes = null;
            Dictionary<string, PublicAttributeBag> groupAttributes = null;
            if ( group.Attributes != null && group.Attributes.Count > 0 )
            {
                var (filters, ownAttributes) = SplitGroupAttributeSchema( group.Attributes, group.GroupTypeId );
                filterAttributes = filters.Count > 0 ? filters : null;
                groupAttributes = ownAttributes.Count > 0 ? ownAttributes : null;
            }

            return new GroupDetailBag
            {
                IdKey = group.IdKey,
                Name = group.Name,
                IsActive = group.IsActive,
                IsSpecialNeeds = group.IsSpecialNeeds,
                ParentAreaName = GroupTypeCache.Get( group.GroupTypeId )?.Name,
                InheritedGroupTypeName = GetInheritedSetupTypeName( group.GroupTypeId ),
                Locations = mainLocations,
                OverflowLocations = overflowLocations,
                Attributes = filterAttributes,
                GroupAttributes = groupAttributes,
                AttributeValues = group.GetPublicAttributeValuesForEdit( RequestContext.CurrentPerson, enforceSecurity: false )
            };
        }

        /// <summary>
        /// Resolves the name of a group's inherited check-in setup type. This is the ancestor area's "Inherit Check-in
        /// Setup Type From" selection, read from the area group type's <see cref="GroupType.InheritedGroupTypeId"/>.
        /// Returns <c>null</c> when the group type inherits from nothing.
        /// </summary>
        /// <param name="ownGroupTypeId">The group's own group type identifier (a check-in group's group type is its area).</param>
        /// <returns>The directly-inherited setup type's name, or <c>null</c>.</returns>
        private static string GetInheritedSetupTypeName( int ownGroupTypeId )
        {
            var inheritedGroupTypeId = GroupTypeCache.Get( ownGroupTypeId )?.InheritedGroupTypeId;
            return inheritedGroupTypeId.HasValue
                ? GroupTypeCache.Get( inheritedGroupTypeId.Value )?.Name
                : null;
        }

        /// <summary>
        /// Loads every named location attached to a group, in display order, paired with the
        /// <see cref="GroupLocation.IsOverflowLocation"/> flag so the caller can split them into the Main and
        /// Overflow grids. Each bag carries its location's campus so the client can filter the displayed grids by the
        /// active campus slicer without a server round-trip.
        /// </summary>
        /// <param name="groupId">The id of the group whose attached locations to load.</param>
        /// <returns>The attached locations in <see cref="GroupLocation.Order"/> order, each carrying its
        /// overflow flag.</returns>
        private List<AttachedNamedLocation> GetAttachedNamedLocations( int groupId )
        {
            // GroupLocationCache isn't indexed by group, so we still need one DB hit to discover which
            // GroupLocation ids belong to this group. From there, GetMany hits cache for the field
            // values (LocationId, IsOverflowLocation, Order) instead of pulling those columns again.
            var groupLocationIds = new GroupLocationService( RockContext )
                .Queryable()
                .Where( gl => gl.GroupId == groupId )
                .Select( gl => gl.Id )
                .ToList();

            if ( groupLocationIds.Count == 0 )
            {
                return null;
            }

            return GroupLocationCache.GetMany( groupLocationIds, RockContext )
                .OrderBy( gl => gl.Order )
                .Select( gl => new AttachedNamedLocation
                {
                    IsOverflow = gl.IsOverflowLocation,
                    Bag = BuildNamedLocationBag( gl.LocationId )
                } )
                .Where( a => a.Bag != null )
                .ToList();
        }

        /// <summary>
        /// Builds a <see cref="NamedLocationBag"/> for the supplied location id, resolving the
        /// <see cref="NamedLocationBag.Name"/> as the location's full hierarchical name path
        /// ("Grandparent &gt; Parent &gt; Child") and tagging it with its campus so the client can filter the
        /// displayed grids by the active campus. Returns <c>null</c> if the location can't be resolved.
        /// </summary>
        /// <param name="locationId">The id of the named location to project.</param>
        /// <returns>The bag, or <c>null</c> when no named location matches.</returns>
        private NamedLocationBag BuildNamedLocationBag( int locationId )
        {
            var location = NamedLocationCache.Get( locationId );
            if ( location == null )
            {
                return null;
            }

            var path = location.Name;
            var parent = location.ParentLocation;
            while ( parent != null )
            {
                path = $"{parent.Name} > {path}";
                parent = parent.ParentLocation;
            }

            return new NamedLocationBag
            {
                IdKey = locationId.AsIdKey(),
                Name = path,
                CampusGuid = location.CampusId.HasValue
                    ? CampusCache.Get( location.CampusId.Value )?.Guid
                    : null
            };
        }

        /// <summary>
        /// Returns <c>true</c> when the supplied named location belongs to <paramref name="campusId"/> (or when
        /// <paramref name="campusId"/> is <c>null</c>, which means "All Campuses" and accepts any location). The
        /// location's campus is resolved through the location tree (<see cref="NamedLocationCache.CampusId"/>).
        /// </summary>
        /// <param name="location">The named location to test.</param>
        /// <param name="campusId">The campus to test against, or <c>null</c> for "All Campuses".</param>
        /// <returns><c>true</c> when the location is in-scope for the supplied campus.</returns>
        private bool IsLocationInCampus( NamedLocationCache location, int? campusId )
        {
            if ( location == null )
            {
                return false;
            }

            if ( !campusId.HasValue )
            {
                return true;
            }

            return location.CampusId == campusId.Value;
        }

        /// <summary>
        /// Reconciles the group's attached <see cref="GroupLocation"/> rows against the supplied bag: removes
        /// rows whose location is no longer attached, adds rows for newly-attached locations, and refreshes
        /// <see cref="GroupLocation.Order"/> and <see cref="GroupLocation.IsOverflowLocation"/> on survivors so
        /// the persisted order matches the bag's order on both lists. The bag is the complete authoritative set
        /// across every campus, so this is a full reconcile over all of the group's locations.
        /// </summary>
        /// <param name="group">The tracked group whose <see cref="Model.Group.GroupLocations"/> collection is being mutated.</param>
        /// <param name="bag">The desired state, as the union of <see cref="GroupDetailBag.Locations"/> and
        /// <see cref="GroupDetailBag.OverflowLocations"/>.</param>
        /// <param name="errorMessage">When the method returns <c>false</c>, an error message describing why
        /// the reconciliation failed.</param>
        /// <returns><c>true</c> when reconciliation completes; <c>false</c> when validation fails.</returns>
        private bool TryReconcileGroupLocations( Model.Group group, GroupDetailBag bag, out string errorMessage )
        {
            errorMessage = null;

            var usePredictableIds = !PageCache.Layout.Site.DisablePredictableIds;
            var groupLocationService = new GroupLocationService( RockContext );

            // Resolve each bag to a LocationId. Skip anything that can't be resolved (e.g. a stale bag pointing at a
            // deleted location) rather than failing the save outright. The null guards on the bag lists avoid
            // allocating an empty fallback when the client sends null.
            var desiredLocations = new List<(int LocationId, int Order, bool IsOverflow)>();

            if ( bag.Locations != null )
            {
                for ( var i = 0; i < bag.Locations.Count; i++ )
                {
                    if ( !TryResolveAttachedLocationId( bag.Locations[i], usePredictableIds, out var locationId, out errorMessage ) )
                    {
                        return false;
                    }
                    if ( locationId.HasValue )
                    {
                        desiredLocations.Add( (locationId.Value, i, false) );
                    }
                }
            }

            if ( bag.OverflowLocations != null )
            {
                for ( var i = 0; i < bag.OverflowLocations.Count; i++ )
                {
                    if ( !TryResolveAttachedLocationId( bag.OverflowLocations[i], usePredictableIds, out var locationId, out errorMessage ) )
                    {
                        return false;
                    }
                    if ( locationId.HasValue )
                    {
                        desiredLocations.Add( (locationId.Value, i, true) );
                    }
                }
            }

            var desiredLocationIds = desiredLocations.Select( d => d.LocationId ).ToHashSet();

            // Remove rows whose location dropped out of the desired set. Clear the schedule configs first so the
            // GroupLocation row can drop without leaving orphaned config rows behind.
            foreach ( var groupLocation in group.GroupLocations.Where( gl => !desiredLocationIds.Contains( gl.LocationId ) ).ToList() )
            {
                groupLocation.GroupLocationScheduleConfigs.Clear();
                groupLocationService.Delete( groupLocation );
                group.GroupLocations.Remove( groupLocation );
            }

            // Apply the desired list: update survivors, add new rows.
            foreach ( var (locationId, order, isOverflow) in desiredLocations )
            {
                var groupLocation = group.GroupLocations.FirstOrDefault( gl => gl.LocationId == locationId );
                if ( groupLocation == null )
                {
                    groupLocation = new GroupLocation
                    {
                        LocationId = locationId,
                        GroupId = group.Id
                    };
                    group.GroupLocations.Add( groupLocation );
                }

                groupLocation.Order = order;
                groupLocation.IsOverflowLocation = isOverflow;
            }

            return true;
        }

        /// <summary>
        /// Resolves a single <see cref="NamedLocationBag"/> to its underlying <see cref="Model.Location.Id"/>.
        /// Returns <c>true</c> with a <c>null</c> id when the bag's <see cref="NamedLocationBag.IdKey"/> doesn't
        /// resolve to a named location (e.g. the location was deleted out from under the editor), so a stale entry is
        /// skipped rather than failing the whole save.
        /// </summary>
        /// <param name="bag">The attached-location bag to resolve.</param>
        /// <param name="usePredictableIds">Whether plain integer ids are accepted in addition to IdKey/Guid.</param>
        /// <param name="locationId">The resolved location id, or <c>null</c> when the bag doesn't match a named location.</param>
        /// <param name="errorMessage">When the method returns <c>false</c>, an error message describing the failure.</param>
        /// <returns><c>true</c> when the bag resolved successfully (with or without a match), <c>false</c> on a hard error.</returns>
        private bool TryResolveAttachedLocationId( NamedLocationBag bag, bool usePredictableIds, out int? locationId, out string errorMessage )
        {
            locationId = null;
            errorMessage = null;

            if ( bag == null || bag.IdKey.IsNullOrWhiteSpace() )
            {
                return true;
            }

            var resolvedId = IdHasher.Instance.GetId( bag.IdKey );
            if ( !resolvedId.HasValue && usePredictableIds && int.TryParse( bag.IdKey, out var parsedId ) )
            {
                resolvedId = parsedId;
            }

            if ( !resolvedId.HasValue )
            {
                return true;
            }

            var location = NamedLocationCache.Get( resolvedId.Value );
            if ( location == null )
            {
                return true;
            }

            locationId = location.Id;
            return true;
        }

        /// <summary>
        /// Resolves a single named location into a <see cref="NamedLocationBag"/> for the client's locations
        /// grid. Enforces the active campus filter, so attempts to add a location outside the active campus are
        /// rejected before the user's "Add" click commits to the working copy.
        /// </summary>
        /// <param name="locationGuid">The Guid of the location to resolve.</param>
        /// <returns><see cref="ActionOk"/> with the populated <see cref="NamedLocationBag"/>, or
        /// <see cref="ActionBadRequest"/>/<see cref="ActionNotFound"/>.</returns>
        [BlockAction]
        public BlockActionResult ResolveNamedLocation( Guid? locationGuid )
        {
            var location = locationGuid.HasValue
                ? NamedLocationCache.Get( locationGuid.Value )
                : null;
            if ( location == null )
            {
                return ActionNotFound( "Location not found." );
            }

            if ( !IsLocationInCampus( location, ContextCampusId ) )
            {
                return ActionBadRequest( $"\"{location.Name}\" is not part of the active campus." );
            }

            return ActionOk( BuildNamedLocationBag( location.Id ) );
        }

        /// <summary>
        /// Gets the check-in labels currently attached to an area, in display order.
        /// </summary>
        /// <param name="areaId">The id of the area whose attached labels to load.</param>
        /// <returns>The attached labels in <see cref="RelatedEntity.Order"/> order.</returns>
        private List<CheckInLabelBag> GetAttachedCheckInLabels( int areaId )
        {
            var relatedEntityQry = GetAreaCheckInLabelRelatedEntityQuery( areaId );

            return new CheckInLabelService( RockContext )
                .Queryable()
                .Join(
                    relatedEntityQry,
                    cl => cl.Id,
                    re => re.TargetEntityId,
                    ( cl, re ) => new { cl.Id, cl.Name, re.Order }
                )
                .OrderBy( x => x.Order )
                .ToList()
                .Select( x => new CheckInLabelBag
                {
                    IdKey = x.Id.AsIdKey(),
                    Name = x.Name
                } )
                .ToList();
        }

        /// <summary>
        /// Builds the base query for <see cref="RelatedEntity"/> rows that represent an area's attached check-in labels.
        /// </summary>
        /// <param name="areaId">The id of the area whose label relations to query.</param>
        /// <returns>The unmaterialized query.</returns>
        /// <remarks>
        /// Each attachment is one <see cref="RelatedEntity"/> row whose <c>SourceEntity</c> is the area's
        /// <see cref="GroupType"/>, <c>TargetEntity</c> is a <see cref="Model.CheckInLabel"/>, and <c>PurposeKey</c>
        /// is <see cref="RelatedEntityPurposeKey.AreaCheckInLabel"/>. <see cref="RelatedEntity.Order"/> drives the
        /// position in the grid.
        /// </remarks>
        private IQueryable<RelatedEntity> GetAreaCheckInLabelRelatedEntityQuery( int areaId )
        {
            var groupTypeEntityTypeId = EntityTypeCache.GetId<GroupType>().Value;
            var checkInLabelEntityTypeId = EntityTypeCache.GetId<Model.CheckInLabel>().Value;

            return new RelatedEntityService( RockContext )
                .Queryable()
                .Where( re =>
                    re.SourceEntityTypeId == groupTypeEntityTypeId
                    && re.TargetEntityTypeId == checkInLabelEntityTypeId
                    && re.SourceEntityId == areaId
                    && re.PurposeKey == RelatedEntityPurposeKey.AreaCheckInLabel
                );
        }

        /// <summary>
        /// Gets the classic check-in labels currently attached to an area, in display order.
        /// </summary>
        /// <param name="attributes">The area's attribute dictionary keyed by attribute key.</param>
        /// <returns>The attached classic labels in attribute-<c>Order</c> order.</returns>
        private static List<ClassicCheckInLabelBag> GetAttachedClassicCheckInLabels( Dictionary<string, AttributeCache> attributes )
        {
            if ( attributes == null )
            {
                return new List<ClassicCheckInLabelBag>();
            }

            var labelFieldTypeGuid = SystemGuid.FieldType.LABEL.AsGuid();

            return attributes
                .Where( kvp => kvp.Value.FieldType.Guid.Equals( labelFieldTypeGuid ) )
                .OrderBy( kvp => kvp.Value.Order )
                .Select( kvp => new ClassicCheckInLabelBag
                {
                    BinaryFileGuid = kvp.Value.DefaultValue.AsGuid(),
                    FileName = kvp.Value.Name
                } )
                .ToList();
        }

        /// <summary>
        /// Builds the base query for <see cref="Model.Attribute"/> rows that represent an area's attached
        /// classic check-in labels.
        /// </summary>
        /// <param name="areaId">The id of the area whose label attributes to query.</param>
        /// <returns>The unmaterialized query.</returns>
        /// <remarks>
        /// Each attached label is one <see cref="Model.Attribute"/> on the area's own group type, qualified with
        /// <see cref="Model.Attribute.EntityTypeQualifierColumn"/> <c>"Id"</c> and
        /// <see cref="Model.Attribute.EntityTypeQualifierValue"/> equal to the area's <see cref="GroupType.Id"/>,
        /// and a <see cref="Model.Attribute.FieldTypeId"/> pointing at <see cref="SystemGuid.FieldType.LABEL"/>.
        /// <see cref="Model.Attribute.DefaultValue"/> carries the BinaryFile Guid of the label design,
        /// <see cref="Model.Attribute.Name"/> is the display name, and <see cref="Model.Attribute.Order"/>
        /// drives the position in the grid.
        /// </remarks>
        private IQueryable<Model.Attribute> GetAreaClassicCheckInLabelAttributeQuery( int areaId )
        {
            var groupTypeEntityTypeId = EntityTypeCache.GetId<GroupType>().Value;
            var labelFieldTypeGuid = SystemGuid.FieldType.LABEL.AsGuid();
            var areaIdQualifierValue = areaId.ToString();

            return new AttributeService( RockContext )
                .Queryable()
                .Where( a =>
                    a.EntityTypeId == groupTypeEntityTypeId
                    && a.EntityTypeQualifierColumn == "Id"
                    && a.EntityTypeQualifierValue == areaIdQualifierValue
                    && a.FieldType.Guid == labelFieldTypeGuid
                );
        }

        /// <summary>
        /// Gets the check-in setup types selectable in the Area / Group editor's "Inherit Check-in Setup
        /// Type From" dropdown. The first entry is a "None" item with an empty value, so the user can
        /// clear the selection (matches the legacy block, which prepended <c>Rock.Constants.None.ListItem</c>).
        /// </summary>
        /// <returns>An ordered list of <see cref="ListItemBag"/> items keyed by Guid.</returns>
        private List<ListItemBag> GetInheritedGroupTypeOptions()
        {
            var checkInFilterPurposeGuid = SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_FILTER.AsGuid();

            var options = new List<ListItemBag>
            {
                new ListItemBag { Value = string.Empty, Text = "None" }
            };

            options.AddRange( GroupTypeCache.All( RockContext )
                .Where( gt => gt.GroupTypePurposeValue?.Guid == checkInFilterPurposeGuid )
                .OrderBy( gt => gt.Order )
                .ThenBy( gt => gt.Name )
                .ToListItemBagList() );

            return options;
        }

        /// <summary>
        /// Decides the persisted <see cref="GroupType.InheritedGroupTypeId"/> for a save, mirroring the legacy block's
        /// three-way logic: take the new selection when one is provided; clear when the admin explicitly deselected a
        /// value that's still a valid filter type in the dropdown; preserve the existing value otherwise. The
        /// preservation branch protects an area whose inherit-from is pointing at a group type that has since lost
        /// its check-in-filter purpose (so the dropdown couldn't represent it) from being silently cleared on the
        /// next unrelated save.
        /// </summary>
        /// <param name="currentInheritedGroupTypeId">
        /// The area's <see cref="GroupType.InheritedGroupTypeId"/> as it sits in the database before the save.
        /// </param>
        /// <param name="bagInheritedGroupTypeGuid">
        /// The Guid sent up from the editor (empty / null means "no selection").
        /// </param>
        /// <returns>The value to write to <see cref="GroupType.InheritedGroupTypeId"/>.</returns>
        private int? ResolveInheritedGroupTypeId( int? currentInheritedGroupTypeId, Guid? bagInheritedGroupTypeGuid )
        {
            if ( bagInheritedGroupTypeGuid.HasValue )
            {
                return GroupTypeCache.GetId( bagInheritedGroupTypeGuid.Value );
            }

            if ( !currentInheritedGroupTypeId.HasValue )
            {
                return null;
            }

            var checkInFilterPurposeGuid = SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_FILTER.AsGuid();
            var currentInherited = GroupTypeCache.Get( currentInheritedGroupTypeId.Value );
            var isCurrentInDropdown = currentInherited != null
                && currentInherited.GroupTypePurposeValue?.Guid == checkInFilterPurposeGuid;

            return isCurrentInDropdown
                // The area's current inherited group type IS in the dropdown list, but the admin chose to clear it.
                ? ( int? ) null
                // The area's current inherited group type is NOT in the dropdown list, so it's an orphaned reference
                // that should be preserved.
                : currentInheritedGroupTypeId;
        }

        /// <summary>
        /// Evicts <see cref="GroupCache"/> entries for every group whose <c>GroupTypeId</c> matches the supplied
        /// area, forcing the next read to re-resolve the inherit chain from a fresh entity. Call this after an area's
        /// <see cref="GroupType.InheritedGroupTypeId"/> has actually changed; the framework's standard cache
        /// invalidation only touches the directly-edited GroupType, not the descendant Group entries whose resolved
        /// attribute schemas just went stale.
        /// </summary>
        /// <param name="areaId">The id of the area whose groups should have their cache entries dropped.</param>
        private void FlushGroupCachesForAreaInheritedAttributes( int areaId )
        {
            var groupIds = new GroupService( RockContext )
                .Queryable()
                .Where( g => g.GroupTypeId == areaId )
                .Select( g => g.Id )
                .ToList();

            foreach ( var groupId in groupIds )
            {
                GroupCache.Remove( groupId );
            }
        }

        /// <summary>
        /// Builds a map of campus <see cref="System.Guid"/> to that campus's root named-location
        /// <see cref="System.Guid"/>, so the client can scope the Named Locations picker to the active campus's
        /// location tree.
        /// </summary>
        /// <returns>A campus-Guid-keyed map of root location Guids.</returns>
        private Dictionary<string, string> GetCampusRootLocations()
        {
            return CampusCache.All()
                .Where( c => c.LocationId.HasValue )
                .Select( c => new
                {
                    CampusGuid = c.Guid.ToString(),
                    LocationGuid = NamedLocationCache.Get( c.LocationId.Value, RockContext )?.Guid.ToString()
                } )
                .Where( c => c.LocationGuid != null )
                .ToDictionary( c => c.CampusGuid, c => c.LocationGuid );
        }

        /// <summary>
        /// Builds a lookup of the public attribute schema applicable to an area for each potential
        /// "Inherit Check-in Setup Type From" selection. The client uses this to swap the conditional
        /// well's AttributeValuesContainer schema in response to dropdown changes without a server
        /// round-trip.
        /// </summary>
        /// <returns>Outer key: setup type's <see cref="System.Guid"/>; inner key: attribute key.</returns>
        private Dictionary<Guid, Dictionary<string, PublicAttributeBag>> GetInheritedAttributesByGuid()
        {
            var checkInFilterPurposeGuid = SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_FILTER.AsGuid();
            var schemas = new Dictionary<Guid, Dictionary<string, PublicAttributeBag>>();

            var filterGroupTypes = GroupTypeCache.All( RockContext )
                .Where( gt => gt.GroupTypePurposeValue?.Guid == checkInFilterPurposeGuid );

            foreach ( var filterGroupType in filterGroupTypes )
            {
                if ( filterGroupType.Attributes == null )
                {
                    continue;
                }

                schemas[filterGroupType.Guid] = BuildOrderedAttributeSchema( filterGroupType.Attributes );
            }

            return schemas;
        }

        /// <summary>
        /// Projects a filtered and sorted source attribute cache dictionary into the edit-bag form expected by the
        /// attribute values container.
        /// </summary>
        /// <param name="attributes">The source attribute cache map.</param>
        /// <returns>The filtered and sorted edit-bag projection.</returns>
        private static Dictionary<string, PublicAttributeBag> BuildOrderedAttributeSchema( IDictionary<string, AttributeCache> attributes )
        {
            return attributes
                .Where( kvp => kvp.Value.IsActive )
                .OrderBy( kvp => kvp.Value.EntityTypeQualifierValue )
                .ThenBy( kvp => kvp.Value.Order )
                .ThenBy( kvp => kvp.Value.Name )
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => PublicAttributeHelper.GetPublicAttributeForEdit( kvp.Value )
                );
        }

        /// <summary>
        /// Splits a group's resolved attribute schema into its inherited check-in filter attributes and its own
        /// attributes. Both are <see cref="Model.Group"/>-entity attributes qualified by <c>GroupTypeId</c>; an
        /// attribute qualified to an ancestor group type (the check-in setup type the area inherits from) is a filter,
        /// while one qualified to the group's own group type is an own ("Group Attributes") attribute.
        /// </summary>
        /// <param name="attributes">The group's fully-resolved attribute cache (own plus inherited).</param>
        /// <param name="groupTypeId">The group's own group type identifier, used to tell own from inherited.</param>
        /// <returns>A tuple of the ordered filter schema and the ordered own-attribute schema.</returns>
        private static (
            Dictionary<string, PublicAttributeBag> filters,
            Dictionary<string, PublicAttributeBag> groupAttributes
        ) SplitGroupAttributeSchema( IDictionary<string, AttributeCache> attributes, int groupTypeId )
        {
            var ownQualifierValue = groupTypeId.ToString();

            bool isInheritedFilter( AttributeCache attribute )
            {
                return attribute.EntityTypeQualifierColumn == "GroupTypeId"
                    && attribute.EntityTypeQualifierValue.IsNotNullOrWhiteSpace()
                    && attribute.EntityTypeQualifierValue != ownQualifierValue;
            }

            var filters = attributes
                .Where( kvp => isInheritedFilter( kvp.Value ) )
                .ToDictionary( kvp => kvp.Key, kvp => kvp.Value );

            var groupAttributes = attributes
                .Where( kvp => !isInheritedFilter( kvp.Value ) )
                .ToDictionary( kvp => kvp.Key, kvp => kvp.Value );

            return (BuildOrderedAttributeSchema( filters ), BuildOrderedAttributeSchema( groupAttributes ));
        }

        /// <summary>
        /// Gets all check-in area descendants under the supplied configuration as <see cref="ListItemBag"/> items,
        /// suitable for binding to the area slicer dropdown. Includes nested sub-areas and excludes any group types
        /// whose purpose is "check-in filter".
        /// </summary>
        /// <param name="configurationId">The parent check-in configuration <see cref="GroupType"/> identifier.</param>
        /// <returns>A flat list of area items in hierarchical order.</returns>
        private List<ListItemBag> GetAreaOptions( int configurationId )
        {
            var checkInFilterPurposeGuid = SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_FILTER.AsGuid();

            return new GroupTypeService( RockContext )
                .GetCheckinAreaDescendants( configurationId )
                .Where( a =>
                    a.GroupTypePurposeValue == null
                    || !a.GroupTypePurposeValue.Guid.Equals( checkInFilterPurposeGuid )
                )
                .ToListItemBagList();
        }

        /// <summary>
        /// Builds the recursive areas-and-groups tree for the supplied configuration. The tree blends two
        /// hierarchies into one: nested areas (group types) and the groups that belong to those areas, where a
        /// group's children are limited to other groups of the same group type.
        /// </summary>
        /// <param name="configuration">The check-in configuration <see cref="GroupTypeCache"/> that roots the tree.</param>
        /// <param name="scopedAreaId">An optional area identifier that, when supplied, narrows the tree to a single
        /// branch instead of starting from the configuration's direct children.</param>
        /// <returns>The ordered top-level nodes, each with its descendants populated.</returns>
        private List<CheckInTreeNodeBag> BuildTree( GroupTypeCache configuration, int? scopedAreaId )
        {
            if ( configuration == null )
            {
                return new List<CheckInTreeNodeBag>();
            }

            var checkInFilterPurposeGuid = SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_FILTER.AsGuid();

            // Collect every area reachable from the configuration via ChildGroupTypes, skipping self-references and
            // any group types whose purpose is "check-in filter" (those participate in attendee filtering, not the
            // areas-and-groups hierarchy).
            var areaIds = new HashSet<int>();
            var visitedDuringCollection = new HashSet<int> { configuration.Id };
            CollectDescendantAreaIds( configuration, areaIds, visitedDuringCollection, checkInFilterPurposeGuid );

            // Pull every group that belongs to one of the collected areas in a single round-trip so the recursion
            // below stays in-memory. Groups outside the descendant set are not addressable from this configuration.
            var groupRowsById = new Dictionary<int, GroupRow>();
            var groupRowsByParentId = new Dictionary<int, List<GroupRow>>();
            var groupRowsByGroupTypeId = new Dictionary<int, List<GroupRow>>();

            if ( areaIds.Count > 0 )
            {
                var loadedRows = new GroupService( RockContext )
                    .Queryable()
                    .Where( g => areaIds.Contains( g.GroupTypeId ) )
                    .Select( g => new
                    {
                        g.Id,
                        g.Guid,
                        g.Name,
                        g.Order,
                        g.IsActive,
                        g.IsSystem,
                        g.GroupTypeId,
                        g.ParentGroupId
                    } )
                    .ToList();

                foreach ( var row in loadedRows )
                {
                    var groupRow = new GroupRow
                    {
                        Id = row.Id,
                        Guid = row.Guid,
                        Name = row.Name,
                        Order = row.Order,
                        IsActive = row.IsActive,
                        IsSystem = row.IsSystem,
                        GroupTypeId = row.GroupTypeId,
                        ParentGroupId = row.ParentGroupId
                    };

                    groupRowsById[groupRow.Id] = groupRow;

                    if ( !groupRowsByGroupTypeId.TryGetValue( groupRow.GroupTypeId, out var byType ) )
                    {
                        byType = new List<GroupRow>();
                        groupRowsByGroupTypeId[groupRow.GroupTypeId] = byType;
                    }
                    byType.Add( groupRow );

                    if ( groupRow.ParentGroupId.HasValue )
                    {
                        if ( !groupRowsByParentId.TryGetValue( groupRow.ParentGroupId.Value, out var byParent ) )
                        {
                            byParent = new List<GroupRow>();
                            groupRowsByParentId[groupRow.ParentGroupId.Value] = byParent;
                        }
                        byParent.Add( groupRow );
                    }
                }
            }

            // Decide which areas anchor the walk. With a scoped area id, only that branch is returned so the slicer
            // can narrow what the user is editing; otherwise every direct child area of the configuration is walked.
            IEnumerable<GroupTypeCache> rootAreas;
            if ( scopedAreaId.HasValue && areaIds.Contains( scopedAreaId.Value ) )
            {
                var scopedArea = GroupTypeCache.Get( scopedAreaId.Value );
                rootAreas = scopedArea != null
                    ? new[] { scopedArea }
                    : Enumerable.Empty<GroupTypeCache>();
            }
            else
            {
                rootAreas = configuration.ChildGroupTypes
                    .Where( a => a != null && a.Id != configuration.Id && areaIds.Contains( a.Id ) );
            }

            // Tree-wide visit trackers ensure an area or group reachable from multiple branches renders only once
            // and that any malformed parent cycle terminates instead of running forever.
            var visitedAreaIds = new HashSet<int>();
            var visitedGroupIds = new HashSet<int>();
            var hideInactiveGroups = !ShowInactive;

            return rootAreas
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .Where( a => !visitedAreaIds.Contains( a.Id ) )
                .Select( a => BuildAreaNode( a, areaIds, groupRowsById, groupRowsByParentId, groupRowsByGroupTypeId, visitedAreaIds, visitedGroupIds, hideInactiveGroups ) )
                .ToList();
        }

        /// <summary>
        /// Computes the rendered child-area ids of <paramref name="targetParentId"/> by replaying the tree-builder's
        /// visited-once walk: areas reachable from more than one parent inside this configuration only render under
        /// the first parent the walk encounters, so this method returns exactly the siblings the user sees under the
        /// requested parent. Honors the same scope (selected area in the slicer) the initial tree was built with.
        /// </summary>
        /// <param name="targetParentId">The id of the parent whose rendered area children to return. May be the
        /// configuration's id (top-level reorder under "All Areas") or any area id within the configuration.</param>
        /// <returns>The rendered child-area ids in tree order, or an empty list when the target parent isn't reached
        /// by the walk (e.g., outside the active scope).</returns>
        private List<int> GetRenderedAreaSiblingIds( int targetParentId )
        {
            var configuration = GroupTypeCache.Get( GroupTypeIdFromPageParameter.Value );
            if ( configuration == null )
            {
                return new List<int>();
            }

            var checkInFilterPurposeGuid = SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_FILTER.AsGuid();
            var validAreaIds = new HashSet<int>();
            var visitedForReachability = new HashSet<int> { configuration.Id };
            CollectDescendantAreaIds( configuration, validAreaIds, visitedForReachability, checkInFilterPurposeGuid );

            // Pick the walk root the tree-builder would use: the scoped area when the slicer narrows the view,
            // or the configuration in "All Areas" mode. Top-level reorder under the configuration is handled
            // up front because the configuration itself doesn't enter the visited set.
            IEnumerable<GroupTypeCache> rootAreas;
            if ( SelectedAreaId.HasValue && validAreaIds.Contains( SelectedAreaId.Value ) )
            {
                var scopedArea = GroupTypeCache.Get( SelectedAreaId.Value );
                rootAreas = scopedArea != null
                    ? new[] { scopedArea }
                    : Enumerable.Empty<GroupTypeCache>();
            }
            else
            {
                if ( targetParentId == configuration.Id )
                {
                    return configuration.ChildGroupTypes
                        .Where( c => c != null && c.Id != configuration.Id && validAreaIds.Contains( c.Id ) )
                        .OrderBy( c => c.Order )
                        .ThenBy( c => c.Name )
                        .Select( c => c.Id )
                        .ToList();
                }

                rootAreas = configuration.ChildGroupTypes
                    .Where( a => a != null && a.Id != configuration.Id && validAreaIds.Contains( a.Id ) );
            }

            var visited = new HashSet<int>();
            foreach ( var root in rootAreas.OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
            {
                if ( visited.Contains( root.Id ) )
                {
                    continue;
                }

                var captured = WalkAreaCapturingChildren( root, targetParentId, validAreaIds, visited );
                if ( captured != null )
                {
                    return captured;
                }
            }

            return new List<int>();
        }

        /// <summary>
        /// Recursive partner to <see cref="GetRenderedAreaSiblingIds"/>. Walks one branch of the tree depth-first,
        /// adding visited area ids to <paramref name="visited"/> in the same order <c>BuildAreaNode</c> would, and
        /// returns the rendered child-area ids when the walk reaches <paramref name="targetParentId"/>.
        /// </summary>
        /// <param name="area">The area whose children to consider next.</param>
        /// <param name="targetParentId">The parent id whose rendered child-area ids to capture.</param>
        /// <param name="validAreaIds">The set of areas reachable from the configuration; areas outside this set
        /// are skipped just as the tree-builder skips them.</param>
        /// <param name="visited">The shared visited-once tracker, mutated as the walk progresses.</param>
        /// <returns>The rendered child-area ids when <paramref name="area"/> matches the target, or <c>null</c>
        /// when the walk should keep searching.</returns>
        private static List<int> WalkAreaCapturingChildren( GroupTypeCache area, int targetParentId, HashSet<int> validAreaIds, HashSet<int> visited )
        {
            visited.Add( area.Id );

            var renderedChildren = area.ChildGroupTypes
                .Where( c =>
                    c != null
                    && c.Id != area.Id
                    && validAreaIds.Contains( c.Id )
                    && !visited.Contains( c.Id ) )
                .OrderBy( c => c.Order )
                .ThenBy( c => c.Name )
                .ToList();

            if ( area.Id == targetParentId )
            {
                return renderedChildren.Select( c => c.Id ).ToList();
            }

            foreach ( var child in renderedChildren )
            {
                if ( visited.Contains( child.Id ) )
                {
                    continue;
                }

                var captured = WalkAreaCapturingChildren( child, targetParentId, validAreaIds, visited );
                if ( captured != null )
                {
                    return captured;
                }
            }

            return null;
        }

        /// <summary>
        /// Recursively collects descendant area identifiers reachable from a parent area, defending against circular
        /// references and pruning group types whose purpose is "check-in filter".
        /// </summary>
        /// <param name="parent">The parent area to walk.</param>
        /// <param name="areaIds">The accumulator for collected area identifiers.</param>
        /// <param name="visited">The set of already-visited identifiers, used to break cycles in malformed
        /// configurations.</param>
        /// <param name="checkInFilterPurposeGuid">The unique identifier of the "check-in filter" group type purpose,
        /// resolved once by the caller so it does not need to be re-fetched per node.</param>
        private static void CollectDescendantAreaIds( GroupTypeCache parent, HashSet<int> areaIds, HashSet<int> visited, Guid checkInFilterPurposeGuid )
        {
            foreach ( var child in parent.ChildGroupTypes )
            {
                if ( child == null || child.Id == parent.Id || !visited.Add( child.Id ) )
                {
                    continue;
                }

                if ( child.GroupTypePurposeValue?.Guid.Equals( checkInFilterPurposeGuid ) == true )
                {
                    continue;
                }

                areaIds.Add( child.Id );
                CollectDescendantAreaIds( child, areaIds, visited, checkInFilterPurposeGuid );
            }
        }

        /// <summary>
        /// Builds a single area node along with its nested area children and the area's "root" groups (groups that
        /// are not parented to another group of the same area). Visited area ids are tracked across recursion to
        /// prevent cycles.
        /// </summary>
        /// <param name="area">The area whose node to produce.</param>
        /// <param name="validAreaIds">The set of areas reachable from the configuration; child areas outside this
        /// set are skipped.</param>
        /// <param name="groupRowsById">All groups indexed by id, used to resolve the group type of a candidate
        /// parent group.</param>
        /// <param name="groupRowsByParentId">All groups indexed by their parent id, used to enumerate a group's
        /// direct children.</param>
        /// <param name="groupRowsByGroupTypeId">All groups indexed by their group type id, used to find every group
        /// that belongs to the supplied area.</param>
        /// <param name="visitedAreaIds">Area ids already placed in the tree, shared across the whole tree so an
        /// area reachable from multiple branches renders only once and circular hierarchies terminate.</param>
        /// <param name="visitedGroupIds">Group ids already placed in the tree, shared across the whole tree so a
        /// group reachable from multiple branches renders only once and parent-id cycles terminate.</param>
        /// <param name="hideInactiveGroups">When true, groups whose <see cref="GroupRow.IsActive"/> is false are
        /// excluded from the tree, matching legacy behavior when the show-inactive preference is off.</param>
        private static CheckInTreeNodeBag BuildAreaNode(
            GroupTypeCache area,
            HashSet<int> validAreaIds,
            Dictionary<int, GroupRow> groupRowsById,
            Dictionary<int, List<GroupRow>> groupRowsByParentId,
            Dictionary<int, List<GroupRow>> groupRowsByGroupTypeId,
            HashSet<int> visitedAreaIds,
            HashSet<int> visitedGroupIds,
            bool hideInactiveGroups )
        {
            visitedAreaIds.Add( area.Id );

            var node = new CheckInTreeNodeBag
            {
                NodeType = CheckInTreeNodeType.Area,
                IdKey = area.IdKey,
                Name = area.Name,
                IsActive = true,
                IsSystem = area.IsSystem,
                Order = area.Order,
                Children = new List<CheckInTreeNodeBag>()
            };

            var childAreas = area.ChildGroupTypes
                .Where( c =>
                    c != null
                    && c.Id != area.Id
                    && validAreaIds.Contains( c.Id )
                    && !visitedAreaIds.Contains( c.Id )
                )
                .OrderBy( c => c.Order )
                .ThenBy( c => c.Name );

            foreach ( var childArea in childAreas )
            {
                node.Children.Add( BuildAreaNode( childArea, validAreaIds, groupRowsById, groupRowsByParentId, groupRowsByGroupTypeId, visitedAreaIds, visitedGroupIds, hideInactiveGroups ) );
            }

            // A group counts as a "root" for this area when it has no parent group, or its parent group belongs to
            // a different area; nested groups of the same area are attached underneath their parent group instead.
            if ( groupRowsByGroupTypeId.TryGetValue( area.Id, out var areaGroups ) )
            {
                var rootGroups = areaGroups
                    .Where( g => IsAreaRootGroup( g, area.Id, groupRowsById ) )
                    .Where( g => !hideInactiveGroups || g.IsActive )
                    .Where( g => !visitedGroupIds.Contains( g.Id ) )
                    .OrderBy( g => g.Order )
                    .ThenBy( g => g.Name );

                foreach ( var rootGroup in rootGroups )
                {
                    node.Children.Add( BuildGroupNode( rootGroup, groupRowsByParentId, visitedGroupIds, hideInactiveGroups ) );
                }
            }

            return node;
        }

        /// <summary>
        /// Determines whether a group should anchor an area's group tree (no parent, or parent in a different
        /// area). Nested same-area groups attach beneath their parent instead.
        /// </summary>
        /// <param name="group">The candidate group.</param>
        /// <param name="areaId">The id of the area whose tree the group is being considered for.</param>
        /// <param name="groupRowsById">All loaded groups indexed by id, used to look up the parent's group type.</param>
        private static bool IsAreaRootGroup( GroupRow group, int areaId, Dictionary<int, GroupRow> groupRowsById )
        {
            if ( !group.ParentGroupId.HasValue )
            {
                return true;
            }

            return !groupRowsById.TryGetValue( group.ParentGroupId.Value, out var parent )
                || parent.GroupTypeId != areaId;
        }

        /// <summary>
        /// Builds a single group node, recursively attaching any same-group-type child groups beneath it.
        /// </summary>
        /// <param name="group">The group whose node to produce.</param>
        /// <param name="groupRowsByParentId">All groups indexed by their parent id, used to enumerate this group's
        /// direct children.</param>
        /// <param name="visitedGroupIds">Group ids already placed in the tree, shared across the whole tree so a
        /// group reachable from multiple branches renders only once and parent-id cycles terminate.</param>
        /// <param name="hideInactiveGroups">When true, child groups whose <see cref="GroupRow.IsActive"/> is false
        /// are excluded from the tree.</param>
        private static CheckInTreeNodeBag BuildGroupNode(
            GroupRow group,
            Dictionary<int, List<GroupRow>> groupRowsByParentId,
            HashSet<int> visitedGroupIds,
            bool hideInactiveGroups )
        {
            visitedGroupIds.Add( group.Id );

            var node = new CheckInTreeNodeBag
            {
                NodeType = CheckInTreeNodeType.Group,
                IdKey = group.Id.AsIdKey(),
                Name = group.Name,
                IsActive = group.IsActive,
                IsSystem = group.IsSystem,
                Order = group.Order,
                Children = new List<CheckInTreeNodeBag>()
            };

            if ( groupRowsByParentId.TryGetValue( group.Id, out var children ) )
            {
                var sameTypeChildren = children
                    .Where( c => c.GroupTypeId == group.GroupTypeId )
                    .Where( c => !hideInactiveGroups || c.IsActive )
                    .Where( c => !visitedGroupIds.Contains( c.Id ) )
                    .OrderBy( c => c.Order )
                    .ThenBy( c => c.Name );

                foreach ( var child in sameTypeChildren )
                {
                    node.Children.Add( BuildGroupNode( child, groupRowsByParentId, visitedGroupIds, hideInactiveGroups ) );
                }
            }

            return node;
        }

        /// <summary>
        /// Brute-force rebuilds the area's classic check-in label attributes from the supplied bag.
        /// </summary>
        /// <param name="area">The area entity whose label attributes to rebuild.</param>
        /// <param name="labels">The desired set of attached labels, in display order. <c>null</c> is treated
        /// as an empty list (clears all classic labels).</param>
        private void SaveClassicCheckInLabels( GroupType area, List<ClassicCheckInLabelBag> labels )
        {
            var attributeService = new AttributeService( RockContext );
            var labelFieldTypeId = FieldTypeCache.GetId( SystemGuid.FieldType.LABEL.AsGuid() ).Value;
            var groupTypeEntityTypeId = EntityTypeCache.GetId<GroupType>().Value;

            // Remove every existing label attribute on this area; the desired set is rebuilt from scratch
            // below. Matches legacy's "delete then add" save sequence.
            var existingLabelAttributes = GetAreaClassicCheckInLabelAttributeQuery( area.Id ).ToList();

            foreach ( var existing in existingLabelAttributes )
            {
                attributeService.Delete( existing );
            }

            if ( labels == null || labels.Count == 0 )
            {
                return;
            }

            // Disambiguate attribute keys when two attached labels share a file name. Matches the legacy
            // modal's duplicate-suffix behavior, just consolidated into a single pass since the bag is the
            // authoritative source.
            var assignedKeys = new HashSet<string>();
            for ( var labelIndex = 0; labelIndex < labels.Count; labelIndex++ )
            {
                var labelBag = labels[labelIndex];
                if ( labelBag == null || labelBag.BinaryFileGuid == Guid.Empty )
                {
                    continue;
                }

                var baseKey = ( labelBag.FileName ?? string.Empty ).Replace( " ", string.Empty );
                if ( baseKey.IsNullOrWhiteSpace() )
                {
                    baseKey = "Label";
                }

                var key = baseKey;
                var disambiguator = 1;
                while ( !assignedKeys.Add( key ) )
                {
                    key = $"{baseKey}{disambiguator}";
                    disambiguator++;
                }

                var attribute = new Model.Attribute
                {
                    Guid = Guid.NewGuid(),
                    FieldTypeId = labelFieldTypeId,
                    EntityTypeId = groupTypeEntityTypeId,
                    EntityTypeQualifierColumn = "Id",
                    EntityTypeQualifierValue = area.Id.ToString(),
                    DefaultValue = labelBag.BinaryFileGuid.ToString(),
                    Key = key,
                    Name = labelBag.FileName,
                    Order = labelIndex
                };
                attribute.AttributeQualifiers.Add( new AttributeQualifier
                {
                    Key = "binaryFileType",
                    Value = SystemGuid.BinaryFiletype.CHECKIN_LABEL
                } );

                attributeService.Add( attribute );
            }
        }

        /// <summary>
        /// Reconciles the check-in label attachments for an area against the supplied bag: drops relations the bag no
        /// longer references, refreshes <c>Order</c> on survivors, and adds rows for labels the bag introduces.
        /// </summary>
        /// <param name="areaId">The id of the area whose label attachments to reconcile.</param>
        /// <param name="labels">
        /// The desired set of attached labels, in display order. <c>null</c> is treated as an empty list (detaches
        /// all labels).
        /// </param>
        private void SaveCheckInLabels( int areaId, List<CheckInLabelBag> labels )
        {
            var relatedEntityService = new RelatedEntityService( RockContext );
            var groupTypeEntityTypeId = EntityTypeCache.GetId<GroupType>().Value;
            var checkInLabelEntityTypeId = EntityTypeCache.GetId<Model.CheckInLabel>().Value;

            var existingRelatedEntities = GetAreaCheckInLabelRelatedEntityQuery( areaId ).ToList();

            var desiredLabelIds = ( labels ?? new List<CheckInLabelBag>() )
                .Select( l => IdHasher.Instance.GetId( l?.IdKey ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();

            // Drop relations the bag no longer references.
            foreach ( var orphan in existingRelatedEntities.Where( re => !desiredLabelIds.Contains( re.TargetEntityId ) ).ToList() )
            {
                relatedEntityService.Delete( orphan );
            }

            // Stage adds for newly-introduced labels and refresh Order on the survivors. The bag's index is
            // authoritative for display order.
            for ( var labelIndex = 0; labelIndex < desiredLabelIds.Count; labelIndex++ )
            {
                var labelId = desiredLabelIds[labelIndex];
                var existing = existingRelatedEntities.FirstOrDefault( re => re.TargetEntityId == labelId );
                if ( existing != null )
                {
                    existing.Order = labelIndex;
                    continue;
                }

                relatedEntityService.Add( new RelatedEntity
                {
                    SourceEntityTypeId = groupTypeEntityTypeId,
                    SourceEntityId = areaId,
                    TargetEntityTypeId = checkInLabelEntityTypeId,
                    TargetEntityId = labelId,
                    PurposeKey = RelatedEntityPurposeKey.AreaCheckInLabel,
                    Order = labelIndex
                } );
            }
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var classicLabelBinaryFileTypeId = BinaryFileTypeCache.GetId( SystemGuid.BinaryFiletype.CHECKIN_LABEL.AsGuid() );
            var classicLabelDetailUrl = classicLabelBinaryFileTypeId.HasValue
                ? $"/admin/checkin/labels-classic/0?BinaryFileTypeId={classicLabelBinaryFileTypeId.Value}"
                : string.Empty;

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.CreateCheckInLabel] = "/admin/checkin/labels/0",
                [NavigationUrlKey.CreateClassicCheckInLabel] = classicLabelDetailUrl
            };
        }

        /// <summary>
        /// Clears the kiosk device cache and pushes a refresh notification to all connected kiosks so configuration
        /// changes propagate without waiting for an app recycle.
        /// </summary>
        private void RefreshConnectedKiosks()
        {
#if NET472_OR_GREATER
            // Temporary until legacy check-in is removed.
            KioskDevice.Clear();
#endif

            // I know, this is a terrible hack. But we need to force the
            // kiosks to refresh and we don't want to make this public yet. -dsh
            typeof( GroupType ).Assembly.GetType( "Rock.CheckIn.v2.CheckInDirector" )
                ?.GetMethod( "SendRefreshKioskConfiguration" )
                ?.Invoke( null, new object[0] );
        }

        #endregion Private Methods

        #region Supporting Classes

        /// <summary>
        /// A single attached named location paired with its overflow discriminator. Used internally by
        /// <see cref="GetAttachedNamedLocations(int)"/> to split the group's <see cref="GroupLocation"/>
        /// rows into the Main and Overflow lists projected onto <see cref="GroupDetailBag"/>.
        /// </summary>
        private sealed class AttachedNamedLocation
        {
            /// <summary>
            /// Whether the underlying <see cref="GroupLocation"/> row is flagged as overflow. Drives which
            /// list the bag lands in.
            /// </summary>
            public bool IsOverflow { get; set; }

            /// <summary>
            /// The projected location bag (hashed identifier plus full hierarchical name path).
            /// </summary>
            public NamedLocationBag Bag { get; set; }
        }

        /// <summary>
        /// A lightweight projection of <see cref="Model.Group"/> rows used while assembling the tree. Materializing
        /// into this shape keeps the recursive walk free of EF tracking and avoids round-tripping per node.
        /// </summary>
        private sealed class GroupRow
        {
            /// <summary>
            /// The group's primary key.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// The group's unique identifier.
            /// </summary>
            public Guid Guid { get; set; }

            /// <summary>
            /// The group's display name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The group's sort order among its siblings.
            /// </summary>
            public int Order { get; set; }

            /// <summary>
            /// Whether the group is active.
            /// </summary>
            public bool IsActive { get; set; }

            /// <summary>
            /// Whether the group is a system entity that should not be deleted.
            /// </summary>
            public bool IsSystem { get; set; }

            /// <summary>
            /// The identifier of the group type (area) the group belongs to.
            /// </summary>
            public int GroupTypeId { get; set; }

            /// <summary>
            /// The identifier of the group's parent group, or null when the group has no parent.
            /// </summary>
            public int? ParentGroupId { get; set; }
        }

        #endregion Supporting Classes
    }
}
