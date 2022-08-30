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
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http;
using Rock.Badge;
using Rock.ClientService.Core.Category;
using Rock.ClientService.Core.Category.Options;
using Rock.Communication;
using Rock.Data;
using Rock.Enums.Controls;
using Rock.Extension;
using Rock.Financial;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Crm;
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Utility;

namespace Rock.Rest.v2
{
    /// <summary>
    /// Provides API endpoints for the Controls controller.
    /// </summary>
    [RoutePrefix( "api/v2/Controls" )]
    [Rock.SystemGuid.RestControllerGuid( "815B51F0-B552-47FD-8915-C653EEDD5B67")]
    public class ControlsController : ApiControllerBase
    {

        #region Achievement Type Picker

        /// <summary>
        /// Gets the achievement types that can be displayed in the achievement type picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the achievement types.</returns>
        [HttpPost]
        [System.Web.Http.Route( "AchievementTypePickerGetAchievementTypes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "F98E3033-C652-4031-94B3-E7C44ECA51AA" )]
        public IHttpActionResult AchievementTypePickerGetAchievementTypes( [FromBody] AchievementTypePickerGetAchievementTypesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var items = AchievementTypeCache.All( rockContext )
                    .Select( t => new ListItemBag
                    {
                        Value = t.Guid.ToString(),
                        Text = t.Name,
                        Category = t.Category?.Name
                    } )
                    .ToList();

                return Ok( items );
            }
        }

        #endregion

        #region Assessment Type Picker

        /// <summary>
        /// Gets the assessment types that can be displayed in the assessment type picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the assessment types.</returns>
        [HttpPost]
        [System.Web.Http.Route( "AssessmentTypePickerGetAssessmentTypes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "B47DCE1B-89D7-4DD5-88A7-B3C393D49A7C" )]
        public IHttpActionResult AssessmentTypePickerGetEntityTypes( [FromBody] AssessmentTypePickerGetAssessmentTypesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var items = AssessmentTypeCache.All( rockContext )
                    .Where( at => options.isInactiveIncluded == true || at.IsActive )
                    .OrderBy( at => at.Title )
                    .ThenBy( at => at.Id )
                    .Select( at => new ListItemBag
                    {
                        Value = at.Guid.ToString(),
                        Text = at.Title
                    } )
                    .ToList();

                return Ok( items );
            }
        }

        #endregion

        #region Asset Storage Provider Picker

        /// <summary>
        /// Gets the asset storage providers that can be displayed in the asset storage provider picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the asset storage providers.</returns>
        [HttpPost]
        [System.Web.Http.Route( "AssetStorageProviderPickerGetAssetStorageProviders" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "665EDE0C-1FEA-4421-B355-4D4F72B7E26E" )]
        public IHttpActionResult AssetStorageProviderPickerGetAssetStorageProviders( [FromBody] AssetStorageProviderPickerGetAssetStorageProvidersOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var items = new AssetStorageProviderService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( g => g.EntityTypeId.HasValue && g.IsActive )
                    .OrderBy( g => g.Name )
                    .Select( t => new ListItemBag
                    {
                        Value = t.Guid.ToString(),
                        Text = t.Name
                    } )
                    .ToList();

                return Ok( items );
            }
        }

        #endregion

        #region Audit Detail

        /// <summary>
        /// Gets the audit details about the entity.
        /// </summary>
        /// <param name="options">The options that describe which entity to be audited.</param>
        /// <returns>A <see cref="EntityAuditBag"/> that contains the requested information.</returns>
        [HttpPost]
        [Authenticate]
        [System.Web.Http.Route( "AuditDetailGetAuditDetails" )]
        [Rock.SystemGuid.RestActionGuid( "714D83C9-96E4-49D7-81AF-2EED7D5CCD56" )]
        public IHttpActionResult AuditDetailGetAuditDetails( [FromBody] AuditDetailGetAuditDetailsOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                // Get the entity type identifier to use to lookup the entity.
                var entityType = EntityTypeCache.Get( options.EntityTypeGuid )?.GetEntityType();

                if ( entityType == null )
                {
                    return NotFound();
                }

                var entity = Reflection.GetIEntityForEntityType( entityType, options.EntityKey, rockContext ) as IModel;

                if ( entity == null )
                {
                    return NotFound();
                }

                // If the entity can be secured, ensure the person has access to it.
                if ( entity is ISecured securedEntity )
                {
                    var isAuthorized = securedEntity.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson )
                        || grant?.IsAccessGranted( entity, Authorization.VIEW ) == true;

                    if ( !isAuthorized )
                    {
                        return Unauthorized();
                    }
                }

                return Ok( entity.GetEntityAuditBag() );
            }
        }

        #endregion

        #region Badge Component Picker

        /// <summary>
        /// Gets the badge components that can be displayed in the badge component picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the badge components.</returns>
        [HttpPost]
        [System.Web.Http.Route( "BadgeComponentPickerGetBadgeComponents" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "ABDFC10F-BCCC-4AF1-8DB3-88A26862485D" )]
        public IHttpActionResult BadgeComponentPickerGetEntityTypes( [FromBody] BadgeComponentPickerGetBadgeComponentsOptionsBag options )
        {
            var componentsList = GetComponentListItems( "Rock.Badge.BadgeContainer, Rock", ( Component component ) =>
            {
                var badgeComponent = component as BadgeComponent;
                var entityType = EntityTypeCache.Get( options.EntityTypeGuid.GetValueOrDefault() )?.Name;

                return badgeComponent != null && badgeComponent.DoesApplyToEntityType( entityType );
            } );

            return Ok( componentsList );
        }

        #endregion

        #region Badge List

        /// <summary>
        /// Get the rendered badge information for a specific entity.
        /// </summary>
        /// <param name="options">The options that describe which badges to render.</param>
        /// <returns>A collection of <see cref="RenderedBadgeBag"/> objects.</returns>
        [HttpPost]
        [System.Web.Http.Route( "BadgeListGetBadges" )]
        [Rock.SystemGuid.RestActionGuid( "34387B98-BF7E-4000-A28A-24EA08605285" )]
        public IHttpActionResult BadgeListGetBadges( [FromBody] BadgeListGetBadgesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityTypeCache = EntityTypeCache.Get( options.EntityTypeGuid, rockContext );
                var entityType = entityTypeCache?.GetEntityType();
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                // Verify that we found the entity type.
                if ( entityType == null )
                {
                    return BadRequest( "Unknown entity type." );
                }

                // Load the entity and verify we got one.
                var entity = Rock.Reflection.GetIEntityForEntityType( entityType, options.EntityKey );

                if ( entity == null )
                {
                    return NotFound();
                }

                // If the entity can be secured, ensure the person has access to it.
                if ( entity is ISecured securedEntity )
                {
                    var isAuthorized = securedEntity.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson )
                        || grant?.IsAccessGranted( entity, Authorization.VIEW ) == true;

                    if ( !isAuthorized )
                    {
                        return Unauthorized();
                    }
                }

                List<BadgeCache> badges;

                // Load the list of badges that were requested or all badges
                // if no specific badges were requested.
                if ( options.BadgeTypeGuids != null && options.BadgeTypeGuids.Any() )
                {
                    badges = options.BadgeTypeGuids
                        .Select( g => BadgeCache.Get( g ) )
                        .Where( b => b != null )
                        .ToList();
                }
                else
                {
                    badges = BadgeCache.All()
                        .Where( b => !b.EntityTypeId.HasValue || b.EntityTypeId.Value == entityTypeCache.Id )
                        .ToList();
                }

                // Filter out any badges that don't apply to the entity or are not
                // authorized by the person to be viewed.
                badges = badges.Where( b => b.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson )
                        || grant?.IsAccessGranted( b, Authorization.VIEW ) == true )
                    .ToList();

                // Render all the badges and then filter out any that are empty.
                var badgeResults = badges.Select( b => b.RenderBadge( entity ) )
                    .Where( b => b.Html.IsNotNullOrWhiteSpace() || b.JavaScript.IsNotNullOrWhiteSpace() )
                    .ToList();

                return Ok( badgeResults );
            }
        }

        #endregion

        #region Badge Picker

        /// <summary>
        /// Get the list of Badge types for use in a Badge Picker.
        /// </summary>
        /// <returns>A list of badge types.</returns>
        [HttpPost]
        [System.Web.Http.Route( "BadgePickerGetBadges" )]
        [Rock.SystemGuid.RestActionGuid( "34387B98-BF7E-4000-A28A-24EA08605285" )]
        public IHttpActionResult BadgePickerGetBadges( [FromBody] BadgePickerGetBadgesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );
                var badges = BadgeCache.All().ToList();

                // Filter out any badges that don't apply to the entity or are not
                // authorized by the person to be viewed.
                var badgeList = badges.Where( b => b.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson )
                        || grant?.IsAccessGranted( b, Authorization.VIEW ) == true )
                    .Select(b => new ListItemBag { Text = b.Name, Value = b.Guid.ToString() } )
                    .ToList();

                return Ok( badgeList );
            }
        }

        #endregion

        #region Binary File Picker

        /// <summary>
        /// Gets the binary files that can be displayed in the binary file picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the binary files.</returns>
        [HttpPost]
        [System.Web.Http.Route( "BinaryFilePickerGetBinaryFiles" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "9E5F190E-91FD-4E50-9F00-8B4F9DBD874C" )]
        public IHttpActionResult BinaryFilePickerGetBinaryFiles( [FromBody] BinaryFilePickerGetBinaryFilesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var items = new BinaryFileService( new RockContext() )
                    .Queryable()
                    .Where( f => f.BinaryFileType.Guid == options.BinaryFileTypeGuid && !f.IsTemporary )
                    .OrderBy( f => f.FileName )
                    .Select( t => new ListItemBag
                    {
                        Value = t.Guid.ToString(),
                        Text = t.FileName
                    } )
                    .ToList();

                return Ok( items );
            }
        }

        #endregion

        #region Binary File Type Picker

        /// <summary>
        /// Gets the binary file types that can be displayed in the binary file type picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the binary file types.</returns>
        [HttpPost]
        [System.Web.Http.Route( "BinaryFileTypePickerGetBinaryFileTypes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "C93E5A06-82DE-4475-88B8-B173C03BFB50" )]
        public IHttpActionResult BinaryFileTypePickerGetBinaryFileTypes( [FromBody] BinaryFileTypePickerGetBinaryFileTypesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var items = new BinaryFileTypeService( rockContext )
                    .Queryable()
                    .OrderBy( f => f.Name )
                    .Select( t => new ListItemBag
                    {
                        Value = t.Guid.ToString(),
                        Text = t.Name
                    } )
                    .ToList();

                return Ok( items );
            }
        }

        #endregion

        #region Category Picker

        private static readonly Regex QualifierValueLookupRegex = new Regex( "^{EL:((?:[a-f\\d]{8})-(?:[a-f\\d]{4})-(?:[a-f\\d]{4})-(?:[a-f\\d]{4})-(?:[a-f\\d]{12})):((?:[a-f\\d]{8})-(?:[a-f\\d]{4})-(?:[a-f\\d]{4})-(?:[a-f\\d]{4})-(?:[a-f\\d]{12}))}$", RegexOptions.IgnoreCase );

        /// <summary>
        /// Gets the child items that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A collection of view models that represent the tree items.</returns>
        [HttpPost]
        [System.Web.Http.Route( "CategoryPickerChildTreeItems" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "A1D07211-6C50-463B-98ED-1622DC4D73DD" )]
        public IHttpActionResult CategoryPickerChildTreeItems( [FromBody] CategoryPickerChildTreeItemsOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = new CategoryClientService( rockContext, GetPerson( rockContext ) );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                var items = clientService.GetCategorizedTreeItems( new CategoryItemTreeOptions
                {
                    ParentGuid = options.ParentGuid,
                    GetCategorizedItems = options.GetCategorizedItems,
                    EntityTypeGuid = options.EntityTypeGuid,
                    EntityTypeQualifierColumn = options.EntityTypeQualifierColumn,
                    EntityTypeQualifierValue = GetQualifierValueLookupResult( options.EntityTypeQualifierValue, rockContext ),
                    IncludeUnnamedEntityItems = options.IncludeUnnamedEntityItems,
                    IncludeCategoriesWithoutChildren = options.IncludeCategoriesWithoutChildren,
                    DefaultIconCssClass = options.DefaultIconCssClass,
                    IncludeInactiveItems = options.IncludeInactiveItems,
                    ItemFilterPropertyName = options.ItemFilterPropertyName,
                    ItemFilterPropertyValue = options.ItemFilterPropertyValue,
                    LazyLoad = options.LazyLoad,
                    SecurityGrant = grant
                } );

                return Ok( items );
            }
        }

        /// <summary>
        /// Checks if the qualifier value is a lookup and if so translate it to the
        /// identifier from the unique identifier. Otherwise returns the original
        /// value.
        /// </summary>
        /// <remarks>
        /// At some point this needs to be moved into a ClientService layer, but
        /// I'm not sure where since it isn't related to any one service.
        /// </remarks>
        /// <param name="value">The value to be translated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The qualifier value to use.</returns>
        private static string GetQualifierValueLookupResult( string value, RockContext rockContext )
        {
            if ( value == null )
            {
                return null;
            }

            var m = QualifierValueLookupRegex.Match( value );

            if ( m.Success )
            {
                int? id = null;

                if ( Guid.TryParse( m.Groups[1].Value, out var g1 ) && Guid.TryParse( m.Groups[2].Value, out var g2 ) )
                {
                    id = Rock.Reflection.GetEntityIdForEntityType( g1, g2, rockContext );
                }

                return id?.ToString() ?? "0";
            }
            else
            {
                return value;
            }
        }

        #endregion

        #region Component Picker

        /// <summary>
        /// Gets the components that can be displayed in the component picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the components.</returns>
        [HttpPost]
        [System.Web.Http.Route( "ComponentPickerGetComponents" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "75DA0671-38E2-4FF9-B334-CC0C88B559D0" )]
        public IHttpActionResult ComponentPickerGetEntityTypes( [FromBody] ComponentPickerGetComponentsOptionsBag options )
        {
            var componentsList = GetComponentListItems( options.ContainerType );

            return Ok( componentsList );
        }

        #endregion

        #region Connection Request Picker

        /// <summary>
        /// Gets the data views and their categories that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which data views to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent a tree of data views.</returns>
        [HttpPost]
        [System.Web.Http.Route( "ConnectionRequestPickerGetChildren" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "5316914b-cf47-4dac-9e10-71767fdf1eb9" )]
        public IHttpActionResult ConnectionRequestPickerGetChildren( [FromBody] ConnectionRequestPickerGetChildrenOptionsBag options )
        {
            var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

            using ( var rockContext = new RockContext() )
            {
                string service = null;

                /*
                 * Determine what type of resource the GUID we received is so we know what types of 
                 * children to query for.
                 */
                if (options.ParentGuid == null)
                {
                    // Get the root Connection Types
                    service = "type";
                }
                else
                {
                    var conOpp = new ConnectionOpportunityService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( op => op.Guid == options.ParentGuid )
                        .ToList()
                        .Where( op => op.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || grant?.IsAccessGranted( op, Authorization.VIEW ) == true );

                    if (conOpp.Any())
                    {
                        // Get the Connection Requests
                        service = "request";
                    }
                    else
                    {
                        var conType = new ConnectionTypeService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( t => t.Guid == options.ParentGuid )
                            .ToList()
                            .Where( t => t.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || grant?.IsAccessGranted( t, Authorization.VIEW ) == true );

                        if (conType.Any() )
                        {
                            // Get the Connection Opportunities
                            service = "opportunity";
                        }
                    }
                }

                /*
                 * Fetch the children
                 */
                var list = new List<TreeItemBag>();

                if (service == "type")
                {
                    // Get the Connection Types
                    var connectionTypes = new ConnectionTypeService( rockContext )
                        .Queryable().AsNoTracking()
                        .OrderBy( ct => ct.Name )
                        .ToList()
                        .Where( ct => ct.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || grant?.IsAccessGranted( ct, Authorization.VIEW ) == true );

                    foreach ( var connectionType in connectionTypes)
                    {
                        var item = new TreeItemBag();
                        item.Value = connectionType.Guid.ToString();
                        item.Text = connectionType.Name;
                        item.HasChildren = connectionType.ConnectionOpportunities.Any();
                        item.IconCssClass = connectionType.IconCssClass;
                        list.Add( item );
                    }
                }
                else if (service == "opportunity")
                {
                    // Get the Connection Opportunities
                    var opportunities = new ConnectionOpportunityService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( op => op.ConnectionType.Guid == options.ParentGuid )
                        .OrderBy( op => op.Name )
                        .ToList()
                        .Where( op => op.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || grant?.IsAccessGranted( op, Authorization.VIEW ) == true );

                    foreach ( var opportunity in opportunities )
                    {
                        var item = new TreeItemBag();
                        item.Value = opportunity.Guid.ToString();
                        item.Text = opportunity.Name;
                        item.HasChildren = opportunity.ConnectionRequests
                            .Any( r =>
                                r.ConnectionState == ConnectionState.Active ||
                                r.ConnectionState == ConnectionState.FutureFollowUp );
                        item.IconCssClass = opportunity.IconCssClass;
                        list.Add( item );
                    }
                }
                else if ( service == "request" )
                {
                    var requests = new ConnectionRequestService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( r =>
                            r.ConnectionOpportunity.Guid == options.ParentGuid &&
                            r.PersonAlias != null &&
                            r.PersonAlias.Person != null )
                        .OrderBy( r => r.PersonAlias.Person.LastName )
                        .ThenBy( r => r.PersonAlias.Person.NickName )
                        .ToList()
                        .Where( op => op.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || grant?.IsAccessGranted( op, Authorization.VIEW ) == true );

                    foreach ( var request in requests )
                    {
                        var item = new TreeItemBag();
                        item.Value = request.Guid.ToString();
                        item.Text = request.PersonAlias.Person.FullName;
                        item.HasChildren = false;
                        item.IconCssClass = "fa fa-user";
                        list.Add( item );
                    }
                }
                else
                {
                    // service type wasn't set, so we don't know where to look
                    return NotFound();
                }

                return Ok( list );
            }
        }

        #endregion

        #region Data View Picker

        /// <summary>
        /// Gets the data views and their categories that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which data views to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent a tree of data views.</returns>
        [HttpPost]
        [System.Web.Http.Route( "DataViewPickerGetDataViews" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "1E079A57-9B44-4365-9C9C-2383A9A3F45B" )]
        public IHttpActionResult DataViewPickerGetDataViews( [FromBody] DataViewPickerGetDataViewsOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = new CategoryClientService( rockContext, GetPerson( rockContext ) );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                var items = clientService.GetCategorizedTreeItems( new CategoryItemTreeOptions
                {
                    ParentGuid = options.ParentGuid,
                    GetCategorizedItems = options.GetCategorizedItems,
                    EntityTypeGuid = EntityTypeCache.Get<Rock.Model.DataView>().Guid,
                    IncludeUnnamedEntityItems = options.IncludeUnnamedEntityItems,
                    IncludeCategoriesWithoutChildren = options.IncludeCategoriesWithoutChildren,
                    DefaultIconCssClass = options.DefaultIconCssClass,
                    ItemFilterPropertyName = options.EntityTypeGuidFilter.HasValue ? "EntityTypeId" : null,
                    ItemFilterPropertyValue = options.EntityTypeGuidFilter.HasValue ? EntityTypeCache.GetId( options.EntityTypeGuidFilter.Value ).ToString() : "",
                    LazyLoad = options.LazyLoad,
                    SecurityGrant = grant
                } );

                return Ok( items );
            }
        }

        #endregion

        #region Defined Value Picker

        /// <summary>
        /// Gets the defined values and their categories that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which defined values to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent a tree of defined values.</returns>
        [HttpPost]
        [System.Web.Http.Route( "DefinedValuePickerGetDefinedValues" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "1E4A1812-8A2C-4266-8F39-3004C1DEBC9F" )]
        public IHttpActionResult DefinedValuePickerGetDefinedValues( DefinedValuePickerGetDefinedValuesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var definedType = DefinedTypeCache.Get( options.DefinedTypeGuid );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                if ( definedType == null || !definedType.IsAuthorized( Rock.Security.Authorization.VIEW, RockRequestContext.CurrentPerson ) )
                {
                    return NotFound();
                }

                var definedValues = definedType.DefinedValues
                    .Where( v => ( v.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || grant?.IsAccessGranted( v, Authorization.VIEW ) == true )
                        && ( options.IncludeInactive || v.IsActive ) )
                    .OrderBy( v => v.Order )
                    .ThenBy( v => v.Value )
                    .Select( v => new ListItemBag
                    {
                        Value = v.Guid.ToString(),
                        Text = v.Value
                    } )
                    .ToList();

                return Ok( definedValues );
            }
        }

        #endregion

        #region Entity Tag List

        /// <summary>
        /// Gets the tags that are currently specified for the given entity.
        /// </summary>
        /// <param name="options">The options that describe which tags to load.</param>
        /// <returns>A collection of <see cref="EntityTagListTagBag"/> that represent the tags.</returns>
        [HttpPost]
        [System.Web.Http.Route( "EntityTagListGetEntityTags" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "7542D4B3-17DC-4640-ACBD-F02784130401" )]
        public IHttpActionResult EntityTagListGetEntityTags( [FromBody] EntityTagListGetEntityTagsOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityTypeId = EntityTypeCache.GetId( options.EntityTypeGuid );
                var entityGuid = Reflection.GetEntityGuidForEntityType( options.EntityTypeGuid, options.EntityKey, false, rockContext );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                if ( !entityTypeId.HasValue || !entityGuid.HasValue )
                {
                    return NotFound();
                }

                var taggedItemService = new TaggedItemService( rockContext );
                var items = taggedItemService.Get( entityTypeId.Value, string.Empty, string.Empty, RockRequestContext.CurrentPerson?.Id, entityGuid.Value, options.CategoryGuid, false )
                    .Include( ti => ti.Tag.Category )
                    .Select( ti => ti.Tag )
                    .ToList()
                    .Where( t => t.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || grant?.IsAccessGranted( t, Authorization.VIEW ) == true )
                    .Select( t => GetTagBagFromTag( t ) )
                    .ToList();

                return Ok( items );
            }
        }

        /// <summary>
        /// Gets the tags that are available for the given entity.
        /// </summary>
        /// <param name="options">The options that describe which tags to load.</param>
        /// <returns>A collection of list item bags that represent the tags.</returns>
        [HttpPost]
        [System.Web.Http.Route( "EntityTagListGetAvailableTags" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "91890D39-6E3E-4623-AAD7-F32E686C784E" )]
        public IHttpActionResult EntityTagListGetAvailableTags( [FromBody] EntityTagListGetAvailableTagsOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityTypeId = EntityTypeCache.GetId( options.EntityTypeGuid );
                var entityGuid = Reflection.GetEntityGuidForEntityType( options.EntityTypeGuid, options.EntityKey, false, rockContext );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                if ( !entityTypeId.HasValue || !entityGuid.HasValue )
                {
                    return NotFound();
                }

                var tagService = new TagService( rockContext );
                var items = tagService.Get( entityTypeId.Value, string.Empty, string.Empty, RockRequestContext.CurrentPerson?.Id, options.CategoryGuid, false )
                    .Where( t => t.Name.StartsWith( options.Name )
                        && !t.TaggedItems.Any( i => i.EntityGuid == entityGuid ) )
                    .ToList()
                    .Where( t => t.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || grant?.IsAccessGranted( t, Authorization.VIEW ) == true )
                    .Select( t => GetTagBagFromTag( t ) )
                    .ToList();

                return Ok( items );
            }
        }

        /// <summary>
        /// Create a new personal tag for the EntityTagList control.
        /// </summary>
        /// <param name="options">The options that describe the tag to be created.</param>
        /// <returns>An instance of <see cref="EntityTagListTagBag"/> that represents the tag.</returns>
        [HttpPost]
        [System.Web.Http.Route( "EntityTagListCreatePersonalTag" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "8CCB7B8D-5D5C-4AA6-A12C-ED062C7AFA05" )]
        public IHttpActionResult EntityTagListCreatePersonalTag( [FromBody] EntityTagListCreatePersonalTagOptionsBag options )
        {
            if ( RockRequestContext.CurrentPerson == null )
            {
                return Unauthorized();
            }

            using ( var rockContext = new RockContext() )
            {
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );
                var entityTypeId = EntityTypeCache.GetId( options.EntityTypeGuid );

                if ( !entityTypeId.HasValue )
                {
                    return NotFound();
                }

                var tagService = new TagService( rockContext );
                var tag = tagService.Get( entityTypeId.Value, string.Empty, string.Empty, RockRequestContext.CurrentPerson?.Id, options.Name, options.CategoryGuid, true );

                // If the personal tag already exists, use a 409 to indicate
                // it already exists and return the existing tag.
                if ( tag != null && tag.OwnerPersonAliasId.HasValue )
                {
                    // If the personal tag isn't active, make it active.
                    if ( !tag.IsActive )
                    {
                        tag.IsActive = true;
                        System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", RockRequestContext.CurrentPerson );
                        rockContext.SaveChanges();
                    }

                    return Content( System.Net.HttpStatusCode.Conflict, GetTagBagFromTag( tag ) );
                }

                // At this point tag either doesn't exist or was an organization
                // tag so we need to create a new personal tag.
                tag = new Tag
                {
                    EntityTypeId = entityTypeId,
                    OwnerPersonAliasId = new PersonAliasService( rockContext ).GetPrimaryAliasId( RockRequestContext.CurrentPerson.Id ),
                    Name = options.Name
                };

                if ( options.CategoryGuid.HasValue )
                {
                    var category = new CategoryService( rockContext ).Get( options.CategoryGuid.Value );

                    if ( category == null || ( !category.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) && !grant?.IsAccessGranted( category, Authorization.VIEW ) != true ) )
                    {
                        return NotFound();
                    }

                    // Set the category as well so we can properly convert to a bag.
                    tag.Category = category;
                    tag.CategoryId = category.Id;
                }

                tagService.Add( tag );

                System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", RockRequestContext.CurrentPerson );
                rockContext.SaveChanges();

                return Content( System.Net.HttpStatusCode.Created, GetTagBagFromTag( tag ) );
            }
        }

        /// <summary>
        /// Adds a tag on the given entity.
        /// </summary>
        /// <param name="options">The options that describe the tag and the entity to be tagged.</param>
        /// <returns>An instance of <see cref="EntityTagListTagBag"/> that represents the tag applied to the entity.</returns>
        [HttpPost]
        [System.Web.Http.Route( "EntityTagListAddEntityTag" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "C9CACC7F-68DE-4765-8967-B50EE2949062" )]
        public IHttpActionResult EntityTagListAddEntityTag( [FromBody] EntityTagListAddEntityTagOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityTypeId = EntityTypeCache.GetId( options.EntityTypeGuid );
                var entityGuid = Reflection.GetEntityGuidForEntityType( options.EntityTypeGuid, options.EntityKey, false, rockContext );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                if ( !entityTypeId.HasValue || !entityGuid.HasValue )
                {
                    return NotFound();
                }

                var tagService = new TagService( rockContext );
                var tag = tagService.Get( options.TagKey );

                if ( tag == null || ( !tag.IsAuthorized( Authorization.TAG, RockRequestContext.CurrentPerson ) && grant?.IsAccessGranted( tag, Authorization.VIEW ) != true ) )
                {
                    return NotFound();
                }

                // If the entity is not already tagged, then tag it.
                var taggedItem = tag.TaggedItems.FirstOrDefault( i => i.EntityGuid.Equals( entityGuid ) );

                if ( taggedItem == null )
                {
                    taggedItem = new TaggedItem
                    {
                        Tag = tag,
                        EntityTypeId = entityTypeId.Value,
                        EntityGuid = entityGuid.Value
                    };

                    tag.TaggedItems.Add( taggedItem );

                    System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", RockRequestContext.CurrentPerson );
                    rockContext.SaveChanges();
                }

                return Ok( GetTagBagFromTag( tag ) );
            }
        }

        /// <summary>
        /// Removes a tag from the given entity.
        /// </summary>
        /// <param name="options">The options that describe the tag and the entity to be untagged.</param>
        /// <returns>A response code that indicates success or failure.</returns>
        [HttpPost]
        [System.Web.Http.Route( "EntityTagListRemoveEntityTag" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "6A78D538-87DB-43FE-9150-4E9A3F276AFE" )]
        public IHttpActionResult EntityTagListRemoveEntityTag( [FromBody] EntityTagListRemoveEntityTagOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityTypeId = EntityTypeCache.GetId( options.EntityTypeGuid );
                var entityGuid = Reflection.GetEntityGuidForEntityType( options.EntityTypeGuid, options.EntityKey, false, rockContext );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );
                var tagService = new TagService( rockContext );
                var taggedItemService = new TaggedItemService( rockContext );

                if ( !entityTypeId.HasValue || !entityGuid.HasValue )
                {
                    return NotFound();
                }

                var tag = tagService.Get( options.TagKey );

                if ( tag == null || ( !tag.IsAuthorized( Authorization.TAG, RockRequestContext.CurrentPerson ) && grant?.IsAccessGranted( tag, Authorization.VIEW ) != true ) )
                {
                    return NotFound();
                }

                // If the entity is tagged, then untag it.
                var taggedItem = taggedItemService.Queryable()
                    .FirstOrDefault( ti => ti.TagId == tag.Id && ti.EntityGuid == entityGuid.Value );

                if ( taggedItem != null )
                {
                    taggedItemService.Delete( taggedItem );

                    System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", RockRequestContext.CurrentPerson );
                    rockContext.SaveChanges();
                }

                return Ok();
            }
        }

        private static EntityTagListTagBag GetTagBagFromTag( Tag tag )
        {
            return new EntityTagListTagBag
            {
                IdKey = tag.IdKey,
                BackgroundColor = tag.BackgroundColor,
                Category = tag.Category != null
                    ? new ListItemBag
                    {
                        Value = tag.Category.Guid.ToString(),
                        Text = tag.Category.ToString()
                    }
                    : null,
                EntityTypeGuid = tag.EntityTypeId.HasValue ? EntityTypeCache.Get( tag.EntityTypeId.Value ).Guid : Guid.Empty,
                IconCssClass = tag.IconCssClass,
                IsPersonal = tag.OwnerPersonAliasId.HasValue,
                Name = tag.Name
            };
        }

        #endregion

        #region Entity Type Picker

        /// <summary>
        /// Gets the entity types that can be displayed in the entity type picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the entity types.</returns>
        [HttpPost]
        [System.Web.Http.Route( "EntityTypePickerGetEntityTypes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "AFDD3D40-5856-478B-A41A-0539127F0631" )]
        public IHttpActionResult EntityTypePickerGetEntityTypes( [FromBody] EntityTypePickerGetEntityTypesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var items = EntityTypeCache.All( rockContext )
                    .Where( t => t.IsEntity )
                    .OrderByDescending( t => t.IsCommon )
                    .ThenBy( t => t.FriendlyName )
                    .Select( t => new ListItemBag
                    {
                        Value = t.Guid.ToString(),
                        Text = t.FriendlyName,
                        Category = t.IsCommon ? "Common" : "All Entities"
                    } )
                    .ToList();

                return Ok( items );
            }
        }

        #endregion

        #region Event Item Picker

        /// <summary>
        /// Gets the event items that can be displayed in the event item picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the event items.</returns>
        [HttpPost]
        [System.Web.Http.Route( "EventItemPickerGetEventItems" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "1D558F8A-08C9-4B62-A3A9-853C9F66B748" )]
        public IHttpActionResult EventItemPickerGetEventItems( [FromBody] EventItemPickerGetEventItemsOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {

                var eventItems = new EventCalendarItemService( rockContext ).Queryable()
                    .Where( i => options.IncludeInactive ? true : i.EventItem.IsActive )
                    .Select( i => new ListItemBag
                    {
                        Category = i.EventCalendar.Name,
                        Value = i.EventItem.Guid.ToString(),
                        Text = i.EventItem.Name
                    } )
                    .OrderBy( i => i.Category )
                    .ThenBy( i => i.Text )
                    .ToList();

                return Ok( eventItems );
            }
        }

        #endregion

        #region Field Type Editor

        /// <summary>
        /// Gets the available field types for the current person.
        /// </summary>
        /// <param name="options">The options that provide details about the request.</param>
        /// <returns>A collection <see cref="ListItemBag"/> that represents the field types that are available.</returns>
        [HttpPost]
        [System.Web.Http.Route( "FieldTypeEditorGetAvailableFieldTypes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "FEDEF3F7-FCB0-4538-9629-177C7D2AE06F" )]
        public IHttpActionResult FieldTypeEditorGetAvailableFieldTypes( [FromBody] FieldTypeEditorGetAvailableFieldTypesOptionsBag options )
        {
            var fieldTypes = FieldTypeCache.All()
                .Where( f => f.Platform.HasFlag( Rock.Utility.RockPlatform.Obsidian ) )
                .ToList();

            var fieldTypeItems = fieldTypes
                .Select( f => new ListItemBag
                {
                    Text = f.Name,
                    Value = f.Guid.ToString()
                } )
                .ToList();

            return Ok( fieldTypeItems );
        }

        /// <summary>
        /// Gets the attribute configuration information provided and returns a new
        /// set of configuration data. This is used by the attribute editor control
        /// when a field type makes a change that requires new data to be retrieved
        /// in order for it to continue editing the attribute.
        /// </summary>
        /// <param name="options">The view model that contains the update request.</param>
        /// <returns>An instance of <see cref="FieldTypeEditorUpdateAttributeConfigurationResultBag"/> that represents the state of the attribute configuration.</returns>
        [HttpPost]
        [System.Web.Http.Route( "FieldTypeEditorUpdateAttributeConfiguration" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "AFDF0EC4-5D17-4278-9FA6-3F859F38E3B5" )]
        public IHttpActionResult FieldTypeEditorUpdateAttributeConfiguration( [FromBody] FieldTypeEditorUpdateAttributeConfigurationOptionsBag options )
        {
            var fieldType = Rock.Web.Cache.FieldTypeCache.Get( options.FieldTypeGuid )?.Field;

            if ( fieldType == null )
            {
                return BadRequest( "Unknown field type." );
            }

            // Convert the public configuration options into our private
            // configuration options (values).
            var configurationValues = fieldType.GetPrivateConfigurationValues( options.ConfigurationValues );

            // Convert the default value from the public value into our
            // private internal value.
            var privateDefaultValue = fieldType.GetPrivateEditValue( options.DefaultValue, configurationValues );

            // Get the new configuration properties from the currently selected
            // options.
            var configurationProperties = fieldType.GetPublicEditConfigurationProperties( configurationValues );

            // Get the public configuration options from the internal options (values).
            var publicConfigurationValues = fieldType.GetPublicConfigurationValues( configurationValues, Field.ConfigurationValueUsage.Configure, null );

            return Ok( new FieldTypeEditorUpdateAttributeConfigurationResultBag
            {
                ConfigurationProperties = configurationProperties,
                ConfigurationValues = publicConfigurationValues,
                DefaultValue = fieldType.GetPublicEditValue( privateDefaultValue, configurationValues )
            } );
        }

        #endregion

        #region Field Type Picker

        /// <summary>
        /// Gets the field types that can be displayed in the field type picker.
        /// </summary>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the field types.</returns>
        [HttpPost]
        [System.Web.Http.Route( "FieldTypePickerGetFieldTypes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "AB53509A-C8A9-481B-839F-DA53232A698A" )]
        public IHttpActionResult FieldTypePickerGetFieldTypes()
        {
            List<ListItemBag> items = new List<ListItemBag> { };

            foreach ( var item in FieldTypeCache.All() )
            {
                items.Add( new ListItemBag { Text = item.Name, Value = item.Guid.ToString() } );
            }

            return Ok( items );
        }

        #endregion

        #region Financial Gateway Picker

        /// <summary>
        /// Gets the financial gateways that can be displayed in the financial gateway picker.
        /// </summary>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the financial gateways.</returns>
        [HttpPost]
        [System.Web.Http.Route( "FinancialGatewayPickerGetFinancialGateways" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "DBF12D3D-09BF-419F-A315-E3B6C0206344" )]
        public IHttpActionResult FinancialGatewayPickerGetFinancialGateways( [FromBody] FinancialGatewayPickerGetFinancialGatewaysOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                List<ListItemBag> items = new List<ListItemBag> { };

                var gateways = new FinancialGatewayService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( g => g.EntityTypeId.HasValue )
                    .OrderBy( g => g.Name )
                    .ToList();

                foreach ( var gateway in gateways )
                {
                    var entityType = EntityTypeCache.Get( gateway.EntityTypeId.Value );
                    GatewayComponent component = GatewayContainer.GetComponent( entityType.Name );

                    // TODO: Need to see if the gateway is selected e.g. gateway.Guid == options.selectedGuid
                    // Add the gateway if the control is configured to show all of the gateways.
                    if (  options.IncludeInactive && options.ShowAllGatewayComponents )
                    {
                        items.Add( new ListItemBag { Text = gateway.Name, Value = gateway.Guid.ToString() } );
                        continue;
                    }

                    // Do not add if the component or gateway is not active and the controls has ShowInactive set to false.
                    if ( options.IncludeInactive == false && ( gateway.IsActive == false || component == null || component.IsActive == false ) )
                    {
                        continue;
                    }

                    if ( options.ShowAllGatewayComponents == false && ( component == null || component.SupportsRockInitiatedTransactions == false ) )
                    {
                        continue;
                    }

                    items.Add( new ListItemBag { Text = gateway.Name, Value = gateway.Guid.ToString() } );
                }

                return Ok( items );
            }
        }

        #endregion

        #region Financial Statement Template Picker

        /// <summary>
        /// Gets the financial statement templates that can be displayed in the financial statement template picker.
        /// </summary>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the financial statement templates.</returns>
        [HttpPost]
        [System.Web.Http.Route( "FinancialStatementTemplatePickerGetFinancialStatementTemplates" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "4E10F2DC-BD7C-4F75-919C-B3F71868ED24" )]
        public IHttpActionResult FinancialStatementTemplatePickerGetFinancialStatementTemplates()
        {

            using ( var rockContext = new RockContext() )
            {
                List<ListItemBag> items = new FinancialStatementTemplateService( rockContext )
                    .Queryable()
                    .Where( s => s.IsActive == true )
                    .Select( i => new ListItemBag
                    {
                        Value = i.Guid.ToString(),
                        Text = i.Name
                    } )
                    .OrderBy( a => a.Text )
                    .ToList();

                return Ok( items );
            }
        }

        #endregion

        #region Following

        /// <summary>
        /// Determines if the entity is currently being followed by the logged in person.
        /// </summary>
        /// <param name="options">The options that describe which entity to be checked.</param>
        /// <returns>A <see cref="FollowingGetFollowingResponseBag"/> that contains the followed state of the entity.</returns>
        [HttpPost]
        [Authenticate]
        [System.Web.Http.Route( "FollowingGetFollowing" )]
        [Rock.SystemGuid.RestActionGuid( "FA1CC136-A994-4870-9507-818EA7A70F01" )]
        public IHttpActionResult FollowingGetFollowing( [FromBody] FollowingGetFollowingOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( RockRequestContext.CurrentPerson == null )
                {
                    return Unauthorized();
                }

                // Get the entity type identifier to use to lookup the entity.
                int? entityTypeId = EntityTypeCache.GetId( options.EntityTypeGuid );

                if ( !entityTypeId.HasValue )
                {
                    return NotFound();
                }

                int? entityId = null;

                // Special handling for a person record, need to translate it to
                // a person alias record.
                if ( entityTypeId.Value == EntityTypeCache.GetId<Person>() )
                {
                    entityTypeId = EntityTypeCache.GetId<PersonAlias>();
                    entityId = new PersonService( rockContext ).Get( options.EntityKey, true )?.PrimaryAliasId;
                }
                else
                {
                    // Get the entity identifier to use for the following query.
                    entityId = Reflection.GetEntityIdForEntityType( entityTypeId.Value, options.EntityKey, true, rockContext );
                }

                if ( !entityId.HasValue )
                {
                    return NotFound();
                }

                var purposeKey = options.PurposeKey ?? string.Empty;

                // Look for any following objects that match the criteria.
                var followings = new FollowingService( rockContext ).Queryable()
                    .Where( f =>
                        f.EntityTypeId == entityTypeId.Value &&
                        f.EntityId == entityId.Value &&
                        f.PersonAlias.PersonId == RockRequestContext.CurrentPerson.Id &&
                        ( ( f.PurposeKey == null && purposeKey == "" ) || f.PurposeKey == purposeKey ) );

                return Ok( new FollowingGetFollowingResponseBag
                {
                    IsFollowing = followings.Any()
                } );
            }
        }

        /// <summary>
        /// Sets the following state of the entity for the logged in person.
        /// </summary>
        /// <param name="options">The options that describe which entity to be followed or unfollowed.</param>
        /// <returns>An HTTP status code that indicates if the request was successful.</returns>
        [HttpPost]
        [Authenticate]
        [System.Web.Http.Route( "FollowingSetFollowing" )]
        [Rock.SystemGuid.RestActionGuid( "8CA2EAFB-E577-4F65-8D96-F42D8D5AAE7A" )]
        public IHttpActionResult FollowingSetFollowing( [FromBody] FollowingSetFollowingOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var followingService = new FollowingService( rockContext );

                if ( RockRequestContext.CurrentPerson == null )
                {
                    return Unauthorized();
                }

                // Get the entity type identifier to use to lookup the entity.
                int? entityTypeId = EntityTypeCache.GetId( options.EntityTypeGuid );

                if ( !entityTypeId.HasValue )
                {
                    return NotFound();
                }

                int? entityId = null;

                // Special handling for a person record, need to translate it to
                // a person alias record.
                if ( entityTypeId.Value == EntityTypeCache.GetId<Person>() )
                {
                    entityTypeId = EntityTypeCache.GetId<PersonAlias>();
                    entityId = new PersonService( rockContext ).Get( options.EntityKey, true )?.PrimaryAliasId;
                }
                else
                {
                    // Get the entity identifier to use for the following query.
                    entityId = Reflection.GetEntityIdForEntityType( entityTypeId.Value, options.EntityKey, true, rockContext );
                }

                if ( !entityId.HasValue )
                {
                    return NotFound();
                }

                var purposeKey = options.PurposeKey ?? string.Empty;

                // Look for any following objects that match the criteria.
                var followings = followingService.Queryable()
                    .Where( f =>
                        f.EntityTypeId == entityTypeId.Value &&
                        f.EntityId == entityId.Value &&
                        f.PersonAlias.PersonId == RockRequestContext.CurrentPerson.Id &&
                        ( ( f.PurposeKey == null && purposeKey == "" ) || f.PurposeKey == purposeKey ) );

                if ( options.IsFollowing )
                {
                    // Already following, don't need to add a new record.
                    if ( followings.Any() )
                    {
                        return Ok();
                    }

                    var following = new Following
                    {
                        EntityTypeId = entityTypeId.Value,
                        EntityId = entityId.Value,
                        PersonAliasId = RockRequestContext.CurrentPerson.PrimaryAliasId.Value,
                        PurposeKey = purposeKey
                    };

                    followingService.Add( following );

                    if ( !following.IsValid )
                    {
                        return BadRequest( string.Join( ", ", following.ValidationResults.Select( r => r.ErrorMessage ) ) );
                    }
                }
                else
                {
                    foreach ( var following in followings )
                    {
                        // Don't check security here because a person is allowed
                        // to un-follow/delete something they previously followed.
                        followingService.Delete( following );
                    }
                }

                System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", RockRequestContext.CurrentPerson );

                rockContext.SaveChanges();

                return Ok();
            }
        }

        #endregion

        #region Grade Picker

        /// <summary>
        /// Gets the school grades that can be displayed in the grade picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the grades.</returns>
        [HttpPost]
        [System.Web.Http.Route( "GradePickerGetGrades" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "2C8F0B8E-F54D-460D-91DB-97B34A9AA174" )]
        public IHttpActionResult GradePickerGetGrades( GradePickerGetGradesOptionsBag options )
        {
            var schoolGrades = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid() );

            if ( schoolGrades == null)
            {
                return NotFound();
            }

            var list = new List<ListItemBag>();

            foreach ( var schoolGrade in schoolGrades.DefinedValues.OrderByDescending( a => a.Value.AsInteger() ) )
            {
                ListItemBag listItem = new ListItemBag();
                if ( options.UseAbbreviation )
                {
                    string abbreviation = schoolGrade.GetAttributeValue( "Abbreviation" );
                    listItem.Text = string.IsNullOrWhiteSpace( abbreviation ) ? schoolGrade.Description : abbreviation;
                }
                else
                {
                    listItem.Text = schoolGrade.Description;
                }

                listItem.Value = options.UseGuidAsValue ? schoolGrade.Guid.ToString() : schoolGrade.Value;

                list.Add( listItem );
            }

            return Ok( list );
        }

        #endregion

        #region Group Member Picker

        /// <summary>
        /// Gets the group members that can be displayed in the group member picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the group members.</returns>
        [HttpPost]
        [System.Web.Http.Route( "GroupMemberPickerGetGroupMembers" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "E0A893FD-0275-4251-BA6E-F669F110D179" )]
        public IHttpActionResult GroupMemberPickerGetGroupMembers( GroupMemberPickerGetGroupMembersOptionsBag options )
        {
            Rock.Model.Group group;

            if ( !options.GroupGuid.HasValue)
            {
                return NotFound();
            }

            group = new GroupService( new RockContext() ).Get( options.GroupGuid.Value );

            if ( group == null && !group.Members.Any() )
            {
                return NotFound();
            }

            var list = new List<ListItemBag>();

            foreach ( var groupMember in group.Members.OrderBy( m => m.Person.FullName ) )
            {
                var li = new ListItemBag {
                    Text = groupMember.Person.FullName,
                    Value = groupMember.Guid.ToString()
                };

                list.Add( li );
            }

            return Ok( list );
        }

        #endregion

        #region Group Picker

        /// <summary>
        /// Gets the groups that can be displayed in the group picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent the groups.</returns>
        [HttpPost]
        [System.Web.Http.Route( "GroupPickerGetChildren" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "c4f5432a-eb1e-4235-a5cd-bde37cc324f7" )]
        public IHttpActionResult GroupPickerGetChildren( GroupPickerGetChildrenOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var groupService = new GroupService(rockContext);

                List<int> includedGroupTypeIds = options.IncludedGroupTypeGuids
                    .Select( ( guid ) =>
                    {
                        var gt = GroupTypeCache.Get( guid );

                        if (gt != null)
                        {
                            return gt.Id;
                        }

                        return 0;
                    } )
                    .ToList();

                // if specific group types are specified, show the groups regardless of ShowInNavigation
                bool limitToShowInNavigation = !includedGroupTypeIds.Any();

                Rock.Model.Group parentGroup = groupService.GetByGuid( options.Guid ?? Guid.Empty );
                int id = parentGroup == null ? 0 : parentGroup.Id;

                Rock.Model.Group rootGroup = groupService.GetByGuid( options.RootGroupGuid ?? Guid.Empty );
                int rootGroupId = rootGroup == null ? 0 : rootGroup.Id;

                var qry = groupService
                    .GetChildren( id, rootGroupId, false, includedGroupTypeIds, new List<int>(), options.IncludeInactiveGroups, limitToShowInNavigation, 0, false, false )
                    .AsNoTracking();

                List<Rock.Model.Group> groupList = new List<Rock.Model.Group>();
                List<TreeItemBag> groupNameList = new List<TreeItemBag>();

                var person = GetPerson();

                if (parentGroup == null)
                {
                    parentGroup = rootGroup;
                }

                List<int> groupIdsWithSchedulingEnabledWithAncestors = null;
                List<int> groupIdsWithRSVPEnabledWithAncestors = null;

                var listOfChildGroups = qry.ToList().OrderBy( g => g.Order ).ThenBy( g => g.Name ).ToList();
                if ( listOfChildGroups.Any() )
                {
                    if ( options.LimitToSchedulingEnabled )
                    {
                        groupIdsWithSchedulingEnabledWithAncestors = groupService.GetGroupIdsWithSchedulingEnabledWithAncestors();
                    }

                    if ( options.LimitToRSVPEnabled )
                    {
                        groupIdsWithRSVPEnabledWithAncestors = groupService.GetGroupIdsWithRSVPEnabledWithAncestors();
                    }
                }

                foreach ( var group in listOfChildGroups )
                {
                    // we already have the ParentGroup record, so lets set it for each group to avoid a database round-trip during Auth
                    group.ParentGroup = parentGroup;

                    var groupType = GroupTypeCache.Get( group.GroupTypeId );

                    //// Before checking Auth, filter based on the limitToSchedulingEnabled and limitToRSVPEnabled option.
                    //// Auth takes longer to check, so if we can rule the group out sooner, that will save a bunch of time

                    if ( options.LimitToSchedulingEnabled )
                    {
                        var includeGroup = false;
                        if ( groupType?.IsSchedulingEnabled == true )
                        {
                            // if this group's group type has scheduling enabled, we will include this group
                            includeGroup = true;
                        }
                        else
                        {
                            // if this group's group type does not have scheduling enabled, we will need to include it if any of its children
                            // have scheduling enabled

                            if ( groupIdsWithSchedulingEnabledWithAncestors != null )
                            {
                                bool hasChildScheduledEnabledGroups = groupIdsWithSchedulingEnabledWithAncestors.Contains( group.Id );
                                if ( hasChildScheduledEnabledGroups )
                                {
                                    includeGroup = true;
                                }
                            }
                        }

                        if ( !includeGroup )
                        {
                            continue;
                        }
                    }

                    if ( options.LimitToRSVPEnabled )
                    {
                        var includeGroup = false;
                        if ( groupType?.EnableRSVP == true )
                        {
                            // if this group's group type has RSVP enabled, we will include this group
                            includeGroup = true;
                        }
                        else
                        {
                            if ( groupIdsWithRSVPEnabledWithAncestors != null )
                            {
                                bool hasChildRSVPEnabledGroups = groupIdsWithRSVPEnabledWithAncestors.Contains( group.Id );
                                if ( hasChildRSVPEnabledGroups )
                                {
                                    includeGroup = true;
                                }
                            }
                        }

                        if ( !includeGroup )
                        {
                            continue;
                        }
                    }

                    bool groupIsAuthorized = group.IsAuthorized( Rock.Security.Authorization.VIEW, person );
                    if ( !groupIsAuthorized )
                    {
                        continue;
                    }

                    groupList.Add( group );
                    var treeViewItem = new TreeItemBag();
                    treeViewItem.Value = group.Guid.ToString();
                    treeViewItem.Text = group.Name;
                    treeViewItem.IsActive = group.IsActive;

                    // if there a IconCssClass is assigned, use that as the Icon.
                    treeViewItem.IconCssClass = groupType?.IconCssClass;

                    groupNameList.Add( treeViewItem );
                }

                // try to quickly figure out which items have Children
                List<int> resultIds = groupList.Select( a => a.Id ).ToList();
                var qryHasChildren = groupService.Queryable().AsNoTracking()
                    .Where( g =>
                        g.ParentGroupId.HasValue &&
                        resultIds.Contains( g.ParentGroupId.Value ) );

                if ( includedGroupTypeIds.Any() )
                {
                    qryHasChildren = qryHasChildren.Where( a => includedGroupTypeIds.Contains( a.GroupTypeId ) );
                }

                var qryHasChildrenList = qryHasChildren
                    .Select( g => g.ParentGroup.Guid )
                    .Distinct()
                    .ToList();

                foreach ( var g in groupNameList )
                {
                    Guid groupGuid = g.Value.AsGuid();
                    g.HasChildren = qryHasChildrenList.Any( a => a == groupGuid );
                }

                return Ok( groupNameList );
            }
        }

        #endregion

        #region Interaction Channel Picker

        /// <summary>
        /// Gets the interaction channels that can be displayed in the interaction channel picker.
        /// </summary>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the interaction channels.</returns>
        [HttpPost]
        [System.Web.Http.Route( "InteractionChannelPickerGetInteractionChannels" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "2F855DC7-7C20-4C09-9CB1-FFC1E022385B" )]
        public IHttpActionResult InteractionChannelPickerGetInteractionChannels( )
        {
            var items = new List<ListItemBag>();
            var rockContext = new RockContext();
            var interactionChannelService = new InteractionChannelService( rockContext );
            var channels = interactionChannelService.Queryable().AsNoTracking()
                .Include( "ChannelTypeMediumValue" )
                .Where( ic => ic.IsActive )
                .OrderBy( ic => ic.Name )
                .Select( ic => new
                {
                    ic.Name,
                    ic.Guid,
                    Medium = ic.ChannelTypeMediumValue.Value
                } )
                .ToList();

            foreach ( var channel in channels )
            {
                ListItemBag li;

                if ( channel.Medium.IsNullOrWhiteSpace() )
                {
                    li = new ListItemBag { Text = channel.Name, Value = channel.Guid.ToString() };
                }
                else
                {
                    li = new ListItemBag { Text = $"{channel.Name} ({ channel.Medium ?? string.Empty })", Value = channel.Guid.ToString() };
                }

                items.Add( li );
            }

            return Ok( items );
        }

        #endregion

        #region Interaction Component Picker

        /// <summary>
        /// Gets the interection components that can be displayed in the interection component picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the interection components.</returns>
        [HttpPost]
        [System.Web.Http.Route( "InteractionComponentPickerGetInteractionComponents" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "BD61A390-39F9-4FDE-B9AD-02E53B5F2073" )]
        public IHttpActionResult InteractionComponentPickerGetInteractionComponents( [FromBody] InteractionComponentPickerGetInteractionComponentsOptionsBag options)
        {
            if ( !options.InteractionChannelGuid.HasValue )
            {
                return NotFound();
            }

            int interactionChannelId = InteractionChannelCache.GetId( options.InteractionChannelGuid.Value ) ?? 0;
            var rockContext = new RockContext();
            var interactionComponentService = new InteractionComponentService( rockContext );

            var components = interactionComponentService.Queryable().AsNoTracking()
                .Where( ic => ic.InteractionChannelId == interactionChannelId )
                .OrderBy( ic => ic.Name )
                .Select(ic => new ListItemBag
                {
                    Text = ic.Name,
                    Value = ic.Guid.ToString()
                } )
                .ToList();

            return Ok( components );
        }

        #endregion

        #region Lava Command Picker

        /// <summary>
        /// Gets the lava commands that can be displayed in the lava command picker.
        /// </summary>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the lava commands.</returns>
        [HttpPost]
        [System.Web.Http.Route( "LavaCommandPickerGetLavaCommands" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "9FD03EE7-49E8-4C64-AC25-648422579F28" )]
        public IHttpActionResult LavaCommandPickerGetLavaCommands()
        {
            var items = new List<ListItemBag>();

            items.Add( new ListItemBag { Text = "All", Value = "All" } );

            foreach ( var command in Rock.Lava.LavaHelper.GetLavaCommands() )
            {
                items.Add( new ListItemBag { Text = command.SplitCase(), Value = command } );
            }

            return Ok( items );
        }

        #endregion

        #region Location Picker

        /// <summary>
        /// Gets the child locations, excluding inactive items.
        /// </summary>
        /// <param name="options">The options that describe which child locations to retrieve.</param>
        /// <returns>A collection of <see cref="TreeItemBag"/> objects that represent the child locations.</returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "LocationPickerGetActiveChildren" )]
        [Rock.SystemGuid.RestActionGuid( "E57312EC-92A7-464C-AA7E-5320DDFAEF3D" )]
        public IHttpActionResult LocationPickerGetActiveChildren( [FromBody] LocationPickerGetActiveChildrenOptionsBag options )
        {
            IQueryable<Location> qry;

            using ( var rockContext = new RockContext() )
            {
                var locationService = new LocationService( rockContext );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                if ( options.Guid == Guid.Empty )
                {
                    qry = locationService.Queryable().AsNoTracking().Where( a => a.ParentLocationId == null );
                    if ( options.RootLocationGuid != Guid.Empty )
                    {
                        qry = qry.Where( a => a.Guid == options.RootLocationGuid );
                    }
                }
                else
                {
                    qry = locationService.Queryable().AsNoTracking().Where( a => a.ParentLocation.Guid == options.Guid );
                }

                // limit to only active locations.
                qry = qry.Where( a => a.IsActive );

                // limit to only Named Locations (don't show home addresses, etc)
                qry = qry.Where( a => a.Name != null && a.Name != string.Empty );

                List<Location> locationList = new List<Location>();
                List<TreeItemBag> locationNameList = new List<TreeItemBag>();

                var person = GetPerson();

                foreach ( var location in qry.OrderBy( l => l.Name ) )
                {
                    if ( location.IsAuthorized( Authorization.VIEW, person ) || grant?.IsAccessGranted( location, Authorization.VIEW ) == true )
                    {
                        locationList.Add( location );
                        var treeViewItem = new TreeItemBag();
                        treeViewItem.Value = location.Guid.ToString();
                        treeViewItem.Text = location.Name;
                        locationNameList.Add( treeViewItem );
                    }
                }

                // try to quickly figure out which items have Children
                List<int> resultIds = locationList.Select( a => a.Id ).ToList();

                var qryHasChildren = locationService.Queryable().AsNoTracking()
                    .Where( l =>
                        l.ParentLocationId.HasValue &&
                        resultIds.Contains( l.ParentLocationId.Value ) &&
                        l.IsActive
                    )
                    .Select( l => l.ParentLocation.Guid )
                    .Distinct()
                    .ToList();

                var qryHasChildrenList = qryHasChildren.ToList();

                foreach ( var item in locationNameList )
                {
                    var locationGuid = item.Value.AsGuid();
                    item.IsFolder = qryHasChildrenList.Any( a => a == locationGuid );
                    item.HasChildren = item.IsFolder;
                }

                return Ok( locationNameList );
            }
        }

        #endregion

        #region Merge Template Picker

        /// <summary>
        /// Gets the merge templates and their categories that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which merge templates to load.</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent a tree of merge templates.</returns>
        [HttpPost]
        [System.Web.Http.Route( "MergeTemplatePickerGetMergeTemplates" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "2e486da8-927f-4474-8ba8-00a68d261403" )]
        public IHttpActionResult MergeTemplatePickerGetMergeTemplates( [FromBody] MergeTemplatePickerGetMergeTemplatesOptionsBag options )
        {
            List<Guid> include = null;
            List<Guid> exclude = null;

            if (options.MergeTemplateOwnership == Rock.Enums.Controls.MergeTemplateOwnership.Global)
            {
                exclude = new List<Guid>();
                exclude.Add( Rock.SystemGuid.Category.PERSONAL_MERGE_TEMPLATE.AsGuid() );
            }
            else if ( options.MergeTemplateOwnership == Rock.Enums.Controls.MergeTemplateOwnership.Personal )
            {
                include = new List<Guid>();
                include.Add( Rock.SystemGuid.Category.PERSONAL_MERGE_TEMPLATE.AsGuid() );
            }

            using ( var rockContext = new RockContext() )
            {
                var clientService = new CategoryClientService( rockContext, GetPerson( rockContext ) );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );
                var queryOptions = new CategoryItemTreeOptions
                {
                    ParentGuid = options.ParentGuid,
                    GetCategorizedItems = options.ParentGuid.HasValue,
                    EntityTypeGuid = EntityTypeCache.Get<MergeTemplate>().Guid,
                    IncludeUnnamedEntityItems = false,
                    IncludeCategoriesWithoutChildren = false,
                    IncludeCategoryGuids = include,
                    ExcludeCategoryGuids = exclude,
                    DefaultIconCssClass = options.DefaultIconCssClass,
                    ItemFilterPropertyName = null,
                    ItemFilterPropertyValue = "",
                    LazyLoad = true,
                    SecurityGrant = grant
                };

                var items = clientService.GetCategorizedTreeItems( queryOptions );

                return Ok( items );
            }
        }

        #endregion

        #region Metric Category Picker

        /// <summary>
        /// Gets the metric categories and their categories that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which metric categories to load.</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent a tree of metric categories.</returns>
        [HttpPost]
        [System.Web.Http.Route( "MetricCategoryPickerGetChildren" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "92a11376-6bcd-4299-a54d-946cbde7566b" )]
        public IHttpActionResult MetricCategoryPickerGetChildren( [FromBody] MetricCategoryPickerGetChildrenOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = new CategoryClientService( rockContext, GetPerson( rockContext ) );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );
                var queryOptions = new CategoryItemTreeOptions
                {
                    ParentGuid = options.ParentGuid,
                    GetCategorizedItems = options.ParentGuid.HasValue,
                    EntityTypeGuid = EntityTypeCache.Get<MetricCategory>().Guid,
                    IncludeUnnamedEntityItems = true,
                    IncludeCategoriesWithoutChildren = false,
                    DefaultIconCssClass = options.DefaultIconCssClass,
                    ItemFilterPropertyName = null,
                    ItemFilterPropertyValue = "",
                    LazyLoad = true,
                    SecurityGrant = grant
                };

                var items = clientService.GetCategorizedTreeItems( queryOptions );

                return Ok( items );
            }
        }

        #endregion

        #region Metric Item Picker

        /// <summary>
        /// Gets the metric items and their categories that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which metric items to load.</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent a tree of metric items.</returns>
        [HttpPost]
        [System.Web.Http.Route( "MetricItemPickerGetChildren" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "c8e8f26e-a7cd-445a-8d72-6d4484a8ee59" )]
        public IHttpActionResult MetricItemPickerGetChildren( [FromBody] MetricItemPickerGetChildrenOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var items = GetMetricItemPickerChildren( options, rockContext );

                if (items == null || items.Count == 0)
                {
                    return NotFound();
                }

                return Ok( items );
            }
        }

        /// <summary>
        /// Gets the metric items and their categories that match the options given.
        /// </summary>
        /// <param name="options">The options that describe which metric items to load.</param>
        /// <param name="rockContext">Context for performing DB queries.</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent a tree of metric items.</returns>
        private List<TreeItemBag> GetMetricItemPickerChildren( [FromBody] MetricItemPickerGetChildrenOptionsBag options, RockContext rockContext )
        {
            var clientService = new CategoryClientService( rockContext, GetPerson( rockContext ) );
            var grant = SecurityGrant.FromToken( options.SecurityGrantToken );
            var queryOptions = new CategoryItemTreeOptions
            {
                ParentGuid = options.ParentGuid,
                GetCategorizedItems = options.ParentGuid.HasValue,
                EntityTypeGuid = EntityTypeCache.Get<MetricCategory>().Guid,
                IncludeUnnamedEntityItems = true,
                IncludeCategoriesWithoutChildren = false,
                DefaultIconCssClass = options.DefaultIconCssClass,
                LazyLoad = true,
                SecurityGrant = grant,
                IncludeCategoryGuids = options.IncludeCategoryGuids
            };

            var metricCategories = clientService.GetCategorizedTreeItems( queryOptions );
            var metricCategoryService = new MetricCategoryService( new RockContext() );
            var convertedMetrics = new List<TreeItemBag>();

            // Translate from MetricCategory to Metric.
            foreach ( var categoryItem in metricCategories )
            {
                if ( !categoryItem.IsFolder )
                {
                    // Load the MetricCategory.
                    var metricCategory = metricCategoryService.Get( categoryItem.Value.AsGuid() );
                    if ( metricCategory != null )
                    {
                        // Swap the Id to the Metric Guid (instead of MetricCategory.Guid).
                        categoryItem.Value = metricCategory.Guid.ToString();
                    }
                }

                if (categoryItem.HasChildren)
                {
                    categoryItem.Children = new List<TreeItemBag>();
                    categoryItem.Children.AddRange( GetMetricItemPickerChildren( new MetricItemPickerGetChildrenOptionsBag
                    {
                        ParentGuid = categoryItem.Value.AsGuid(),
                        DefaultIconCssClass = options.DefaultIconCssClass,
                        SecurityGrantToken = options.SecurityGrantToken,
                        IncludeCategoryGuids = options.IncludeCategoryGuids
                    }, rockContext ) );
                }


                convertedMetrics.Add( categoryItem );
            }

            return convertedMetrics;
        }

        #endregion

        #region Page Picker

        /// <summary>
        /// Gets the tree list of pages
        /// </summary>
        /// <param name="options">The options that describe which pages to retrieve.</param>
        /// <returns>A collection of <see cref="TreeItemBag"/> objects that represent the pages.</returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "PagePickerGetChildren" )]
        [Rock.SystemGuid.RestActionGuid( "EE9AB2EA-EE01-4D0F-B626-02D1C8D1ABF4" )]
        public IHttpActionResult PagePickerGetChildren( [FromBody] PagePickerGetChildrenOptionsBag options )
        {
            var service = new Service<Page>( new RockContext() ).Queryable().AsNoTracking();
            var grant = SecurityGrant.FromToken( options.SecurityGrantToken );
            IQueryable<Page> qry;

            if ( options.Guid.IsEmpty() )
            {
                qry = service.Where( a => a.ParentPage.Guid == options.RootPageGuid );
            }
            else
            {
                qry = service.Where( a => a.ParentPage.Guid == options.Guid );
            }

            if ( options.SiteType != null )
            {
                qry = qry.Where( p => ( int ) p.Layout.Site.SiteType == options.SiteType.Value );
            }

            var hidePageGuids = options.HidePageGuids ?? new List<Guid>();

            List<Page> pageList = qry
                .Where( p => !hidePageGuids.Contains( p.Guid ))
                .OrderBy( p => p.Order )
                .ThenBy( p => p.InternalName )
                .ToList()
                .Where( p => p.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || grant?.IsAccessGranted( p, Authorization.VIEW ) == true )
                .ToList();
            List<TreeItemBag> pageItemList = new List<TreeItemBag>();
            foreach ( var page in pageList )
            {
                var pageItem = new TreeItemBag();
                pageItem.Value = page.Guid.ToString();
                pageItem.Text = page.InternalName;

                pageItemList.Add( pageItem );
            }

            // try to quickly figure out which items have Children
            List<int> resultIds = pageList.Select( a => a.Id ).ToList();

            var qryHasChildren = service
                .Where( p =>
                    p.ParentPageId.HasValue &&
                    resultIds.Contains( p.ParentPageId.Value ) )
                .Select( p => p.ParentPage.Guid )
                .Distinct()
                .ToList();

            foreach ( var g in pageItemList )
            {
                var hasChildren = qryHasChildren.Any( a => a.ToString() == g.Value );
                g.HasChildren = hasChildren;
                g.IsFolder = hasChildren;
                g.IconCssClass = "fa fa-file-o";
            }

            return Ok(pageItemList.AsQueryable());
        }

        /// <summary>
        /// Gets the list of pages in the hierarchy going from the root to the given page
        /// </summary>
        /// <param name="options">The options that describe which pages to retrieve.</param>
        /// <returns>A collection of <see cref="Guid"/> that represent the pages.</returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "PagePickerGetSelectedPageHierarchy" )]
        [Rock.SystemGuid.RestActionGuid( "e74611a0-1711-4a0b-b3bd-df242d344679" )]
        public IHttpActionResult PagePickerGetSelectedPageHierarchy( [FromBody] PagePickerGetSelectedPageHierarchyOptionsBag options )
        {
            var parentPageGuids = new List<string>();
            var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

            foreach ( Guid pageGuid in options.SelectedPageGuids )
            {
                var page = PageCache.Get( pageGuid );

                if (page == null)
                {
                    continue;
                }

                var parentPage = page.ParentPage;

                while ( parentPage != null )
                {
                    if ( !parentPageGuids.Contains( parentPage.Guid.ToString() ) && ( parentPage.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || ( grant?.IsAccessGranted( parentPage, Authorization.VIEW ) == true )) )
                    {
                        parentPageGuids.Insert( 0, parentPage.Guid.ToString() );
                    }
                    else
                    {
                        // infinite recursion
                        break;
                    }

                    parentPage = parentPage.ParentPage;
                }
            }

            return Ok( parentPageGuids );
        }

        /// <summary>
        /// Gets the internal name of the page with the given Guid
        /// </summary>
        /// <param name="options">The options that contains the Guid of the page</param>
        /// <returns>A string internal name of the page with the given Guid.</returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "PagePickerGetPageName" )]
        [Rock.SystemGuid.RestActionGuid( "20d219bd-3635-4cbc-b79f-250972ae6b97" )]
        public IHttpActionResult PagePickerGetPageName( [FromBody] PagePickerGetPageNameOptionsBag options )
        {
            var grant = SecurityGrant.FromToken( options.SecurityGrantToken );
            var page = PageCache.Get( options.PageGuid );

            if (page == null)
            {
                return NotFound();
            }

            var isAuthorized = page.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || grant?.IsAccessGranted( page, Authorization.VIEW ) == true;

            if ( !isAuthorized )
            {
                return Unauthorized();
            }

            return Ok( page.InternalName );
        }

        /// <summary>
        /// Gets the list of routes to the given page
        /// </summary>
        /// <param name="options">The options that describe which routes to retrieve.</param>
        /// <returns>A collection of <see cref="ListItemBag"/> that represent the routes.</returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "PagePickerGetPageRoutes" )]
        [Rock.SystemGuid.RestActionGuid( "858209a4-7715-43e6-aff5-00b82773f241" )]
        public IHttpActionResult PagePickerGetPageRoutes( [FromBody] PagePickerGetPageRoutesOptionsBag options )
        {
            var page = PageCache.Get( options.PageGuid );
            var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

            var isAuthorized = page.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || grant?.IsAccessGranted( page, Authorization.VIEW ) == true;

            if ( !isAuthorized )
            {
                return Unauthorized();
            }

            var routes = page.PageRoutes
                .Select( r => new ListItemBag
                {
                    Text = r.Route,
                    Value = r.Guid.ToString()
                } )
                .ToList();

            return Ok( routes );
        }

        #endregion

        #region Person Picker

        /// <summary>
        /// Searches for people that match the given search options and returns
        /// those matches.
        /// </summary>
        /// <param name="options">The options that describe how the search should be performed.</param>
        /// <returns>A collection of <see cref="Rock.Rest.Controllers.PersonSearchResult"/> objects.</returns>
        [Authenticate]
        [Secured]
        [HttpPost]
        [System.Web.Http.Route( "PersonPickerSearch" )]
        [Rock.SystemGuid.RestActionGuid( "1947578D-B28F-4956-8666-DCC8C0F2B945" )]
        public IQueryable<Rock.Rest.Controllers.PersonSearchResult> PersonPickerSearch( [FromBody] PersonPickerSearchOptionsBag options )
        {
            var rockContext = new RockContext();

            // Chain to the v1 controller.
            return Rock.Rest.Controllers.PeopleController.SearchForPeople( rockContext, options.Name, options.Address, options.Phone, options.Email, options.IncludeDetails, options.IncludeBusinesses, options.IncludeDeceased, false );
        }

        #endregion

        #region Remote Auths Picker

        /// <summary>
        /// Gets the remote auths that can be displayed in the remote auths picker.
        /// </summary>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the remote auths.</returns>
        [HttpPost]
        [System.Web.Http.Route( "RemoteAuthsPickerGetRemoteAuths" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "844D17E3-45FF-4A63-8BC7-32956A11CC94" )]
        public IHttpActionResult RemoteAuthsPickerGetRemoteAuths()
        {
            var items = new List<ListItemBag>();

            foreach ( var serviceEntry in AuthenticationContainer.Instance.Components )
            {
                var component = serviceEntry.Value.Value;

                if ( component.IsActive && component.RequiresRemoteAuthentication )
                {
                    var entityType = EntityTypeCache.Get( component.GetType() );
                    if ( entityType != null )
                    {
                        items.Add( new ListItemBag { Text = entityType.FriendlyName, Value = entityType.Guid.ToString() } );
                    }
                }
            }

            return Ok( items );
        }

        #endregion

        #region Save Financial Account Form

        /// <summary>
        /// Saves the financial account.
        /// </summary>
        /// <param name="options">The options that describe what account should be saved.</param>
        /// <returns></returns>
        [Authenticate]
        [HttpPost]
        [System.Web.Http.Route( "SaveFinancialAccountFormSaveAccount" )]
        [Rock.SystemGuid.RestActionGuid( "544B6302-A9E0-430E-A1C1-7BCBC4A6230C" )]
        public SaveFinancialAccountFormSaveAccountResultBag SaveFinancialAccountFormSaveAccount( [FromBody] SaveFinancialAccountFormSaveAccountOptionsBag options )
        {
            // Validate the arguments
            if ( options?.TransactionCode.IsNullOrWhiteSpace() != false )
            {
                return new SaveFinancialAccountFormSaveAccountResultBag
                {
                    Title = "Sorry",
                    Detail = "The account information cannot be saved as there's not a valid transaction code to reference",
                    IsSuccess = false
                };
            }

            if ( options.SavedAccountName.IsNullOrWhiteSpace() )
            {
                return new SaveFinancialAccountFormSaveAccountResultBag
                {
                    Title = "Missing Account Name",
                    Detail = "Please enter a name to use for this account",
                    IsSuccess = false
                };
            }

            var currentPerson = GetPerson();
            var isAnonymous = currentPerson == null;

            using ( var rockContext = new RockContext() )
            {
                if ( isAnonymous )
                {
                    if ( options.Username.IsNullOrWhiteSpace() || options.Password.IsNullOrWhiteSpace() )
                    {
                        return new SaveFinancialAccountFormSaveAccountResultBag
                        {
                            Title = "Missing Information",
                            Detail = "A username and password are required when saving an account",
                            IsSuccess = false
                        };
                    }

                    var userLoginService = new UserLoginService( rockContext );

                    if ( userLoginService.GetByUserName( options.Username ) != null )
                    {
                        return new SaveFinancialAccountFormSaveAccountResultBag
                        {
                            Title = "Invalid Username",
                            Detail = "The selected Username is already being used. Please select a different Username",
                            IsSuccess = false
                        };
                    }

                    if ( !UserLoginService.IsPasswordValid( options.Password ) )
                    {
                        return new SaveFinancialAccountFormSaveAccountResultBag
                        {
                            Title = "Invalid Password",
                            Detail = UserLoginService.FriendlyPasswordRules(),
                            IsSuccess = false
                        };
                    }
                }

                // Load the gateway from the database
                var financialGatewayService = new FinancialGatewayService( rockContext );
                var financialGateway = financialGatewayService.Get( options.GatewayGuid );
                var gateway = financialGateway?.GetGatewayComponent();

                if ( gateway is null )
                {
                    return new SaveFinancialAccountFormSaveAccountResultBag
                    {
                        Title = "Invalid Gateway",
                        Detail = "Sorry, the financial gateway information is not valid.",
                        IsSuccess = false
                    };
                }

                // Load the transaction from the database
                var financialTransactionService = new FinancialTransactionService( rockContext );
                var transaction = financialTransactionService.GetByTransactionCode( financialGateway.Id, options.TransactionCode );
                var transactionPersonAlias = transaction?.AuthorizedPersonAlias;
                var transactionPerson = transactionPersonAlias?.Person;
                var paymentDetail = transaction?.FinancialPaymentDetail;

                if ( transactionPerson is null || paymentDetail is null )
                {
                    return new SaveFinancialAccountFormSaveAccountResultBag
                    {
                        Title = "Invalid Transaction",
                        Detail = "Sorry, the account information cannot be saved as there's not a valid transaction to reference",
                        IsSuccess = false
                    };
                }

                // Create the login if needed
                if ( isAnonymous )
                {
                    var user = UserLoginService.Create(
                        rockContext,
                        transactionPerson,
                        AuthenticationServiceType.Internal,
                        EntityTypeCache.Get( SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                        options.Username,
                        options.Password,
                        false );

                    var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null, currentPerson );
                    // TODO mergeFields.Add( "ConfirmAccountUrl", RootPath + "ConfirmAccount" );
                    mergeFields.Add( "Person", transactionPerson );
                    mergeFields.Add( "User", user );

                    var emailMessage = new RockEmailMessage( SystemGuid.SystemCommunication.SECURITY_CONFIRM_ACCOUNT.AsGuid() );
                    emailMessage.AddRecipient( new RockEmailMessageRecipient( transactionPerson, mergeFields ) );
                    // TODO emailMessage.AppRoot = ResolveRockUrl( "~/" );
                    // TODO emailMessage.ThemeRoot = ResolveRockUrl( "~~/" );
                    emailMessage.CreateCommunicationRecord = false;
                    emailMessage.Send();
                }

                var savedAccount = new FinancialPersonSavedAccount
                {
                    PersonAliasId = transactionPersonAlias.Id,
                    ReferenceNumber = options.TransactionCode,
                    GatewayPersonIdentifier = options.GatewayPersonIdentifier,
                    Name = options.SavedAccountName,
                    TransactionCode = options.TransactionCode,
                    FinancialGatewayId = financialGateway.Id,
                    FinancialPaymentDetail = new FinancialPaymentDetail
                    {
                        AccountNumberMasked = paymentDetail.AccountNumberMasked,
                        CurrencyTypeValueId = paymentDetail.CurrencyTypeValueId,
                        CreditCardTypeValueId = paymentDetail.CreditCardTypeValueId,
                        NameOnCard = paymentDetail.NameOnCard,
                        ExpirationMonth = paymentDetail.ExpirationMonth,
                        ExpirationYear = paymentDetail.ExpirationYear,
                        BillingLocationId = paymentDetail.BillingLocationId
                    }
                };

                var financialPersonSavedAccountService = new FinancialPersonSavedAccountService( rockContext );
                financialPersonSavedAccountService.Add( savedAccount );

                System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", RockRequestContext.CurrentPerson );
                rockContext.SaveChanges();

                return new SaveFinancialAccountFormSaveAccountResultBag
                {
                    Title = "Success",
                    Detail = "The account has been saved for future use",
                    IsSuccess = true
                };
            }
        }

        #endregion

        #region Step Program Picker

        /// <summary>
        /// Gets the step programs that can be displayed in the step program picker.
        /// </summary>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the step programs.</returns>
        [HttpPost]
        [System.Web.Http.Route( "StepProgramPickerGetStepPrograms" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "6C7816B0-D41D-4081-B998-0B42B542111F" )]
        public IHttpActionResult StepProgramPickerGetStepPrograms()
        {
            var items = new List<ListItemBag>();

            var stepProgramService = new StepProgramService( new RockContext() );
            var stepPrograms = stepProgramService.Queryable().AsNoTracking()
                .Where( sp => sp.IsActive )
                .OrderBy( sp => sp.Order )
                .ThenBy( sp => sp.Name )
                .ToList();

            foreach ( var stepProgram in stepPrograms )
            {
                var li = new ListItemBag { Text = stepProgram.Name, Value = stepProgram.Guid.ToString() };
                items.Add( li );
            }

            return Ok( items );
        }

        #endregion

        #region Step Status Picker

        /// <summary>
        /// Gets the step statuses that can be displayed in the step status picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the step statuses.</returns>
        [HttpPost]
        [System.Web.Http.Route( "StepStatusPickerGetStepStatuses" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "5B4E7419-266C-4235-93B7-8D0DE0E80D2B" )]
        public IHttpActionResult StepStatusPickerGetStepStatuses( [FromBody] StepStatusPickerGetStepStatusesOptionsBag options )
        {
            if ( !options.StepProgramGuid.HasValue )
            {
                return NotFound();
            }

            var items = new List<ListItemBag>();
            int stepProgramId = StepProgramCache.GetId( options.StepProgramGuid.Value ) ?? 0;

            var stepStatusService = new StepStatusService( new RockContext() );
            var statuses = stepStatusService.Queryable().AsNoTracking()
                .Where( ss =>
                    ss.StepProgramId == stepProgramId &&
                    ss.IsActive )
                .OrderBy( ss => ss.Order )
                .ThenBy( ss => ss.Name )
                .ToList();

            foreach ( var status in statuses )
            {
                var li = new ListItemBag { Text = status.Name, Value = status.Guid.ToString() };
                items.Add( li );
            }

            return Ok( items );
        }

        #endregion

        #region Step Type Picker

        /// <summary>
        /// Gets the step types that can be displayed in the step type picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the step types.</returns>
        [HttpPost]
        [System.Web.Http.Route( "StepTypePickerGetStepTypes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "9BC4C3BA-573E-4FB4-A4FC-938D40BED2BE" )]
        public IHttpActionResult StepTypePickerGetStepTypes( [FromBody] StepTypePickerGetStepTypesOptionsBag options )
        {
            if ( !options.StepProgramGuid.HasValue )
            {
                return NotFound();
            }

            var items = new List<ListItemBag>();
            int stepProgramId = StepProgramCache.GetId( options.StepProgramGuid.Value ) ?? 0;

            var stepTypeService = new StepTypeService( new RockContext() );
            var stepTypes = stepTypeService.Queryable().AsNoTracking()
                .Where( st =>
                    st.StepProgramId == stepProgramId &&
                    st.IsActive )
                .OrderBy( st => st.Order )
                .ThenBy( st => st.Name )
                .ToList();

            foreach ( var stepType in stepTypes )
            {
                var li = new ListItemBag { Text = stepType.Name, Value = stepType.Guid.ToString() };
                items.Add( li );
            }

            return Ok( items );
        }

        #endregion

        #region Streak Type Picker

        /// <summary>
        /// Gets the streak types that can be displayed in the streak type picker.
        /// </summary>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the streak types.</returns>
        [HttpPost]
        [System.Web.Http.Route( "StreakTypePickerGetStreakTypes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "78D0A6D1-317E-4CB7-98BB-AF9194AD3C94" )]
        public IHttpActionResult StreakTypePickerGetStreakTypes()
        {
            var items = new List<ListItemBag>();

            var streakTypes = StreakTypeCache.All()
                .Where( st => st.IsActive )
                .OrderBy( st => st.Name )
                .ThenBy( st => st.Id )
                .ToList();

            foreach ( var streakType in streakTypes )
            {
                var li = new ListItemBag { Text = streakType.Name, Value = streakType.Guid.ToString() };
                items.Add( li );
            }

            return Ok( items );
        }

        #endregion

        #region Workflow Type Picker

        /// <summary>
        /// Gets the workflow types and their categories that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which workflow types to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent a tree of workflow types.</returns>
        [HttpPost]
        [System.Web.Http.Route( "WorkflowTypePickerGetWorkflowTypes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "622EE929-7A18-46BE-9AEA-9E0725293612" )]
        public IHttpActionResult WorkflowTypePickerGetWorkflowTypes( [FromBody] WorkflowTypePickerGetWorkflowTypesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = new CategoryClientService( rockContext, GetPerson( rockContext ) );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                var items = clientService.GetCategorizedTreeItems( new CategoryItemTreeOptions
                {
                    ParentGuid = options.ParentGuid,
                    GetCategorizedItems = true,
                    EntityTypeGuid = Rock.SystemGuid.EntityType.WORKFLOW_TYPE.AsGuid(),
                    IncludeUnnamedEntityItems = true,
                    IncludeCategoriesWithoutChildren = false,
                    IncludeInactiveItems = options.IncludeInactiveItems,
                    LazyLoad = false,
                    SecurityGrant = grant
                } );

                return Ok( items );
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Retrieve a list of ListItems representing components for the given container type
        /// </summary>
        /// <param name="containerType"></param>
        /// <returns>A list of ListItems representing components</returns>
        private List<ListItemBag> GetComponentListItems( string containerType )
        {
            return GetComponentListItems( containerType, (x) => true );
        }

        /// <summary>
        /// Retrieve a list of ListItemBags representing components for the given container type. Filters any components
        /// out that don't pass the given validator
        /// </summary>
        /// <param name="containerType"></param>
        /// <param name="isValidComponentChecker"></param>
        /// <returns>A list of ListItemBags representing components</returns>
        private List<ListItemBag> GetComponentListItems( string containerType, Func<Component, bool> isValidComponentChecker )
        {
            if ( containerType.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var resolvedContainerType = Container.ResolveContainer( containerType );

            if ( resolvedContainerType == null )
            {
                return null;
            }

            var instanceProperty = resolvedContainerType.GetProperty( "Instance" );

            if ( instanceProperty == null )
            {
                return null;
            }

            var container = instanceProperty.GetValue( null, null ) as IContainer;
            var componentDictionary = container?.Dictionary;

            var items = new List<ListItemBag>();

            foreach ( var component in componentDictionary )
            {
                var componentValue = component.Value.Value;
                var entityType = EntityTypeCache.Get( componentValue.GetType() );

                if ( !componentValue.IsActive || entityType == null || !isValidComponentChecker(componentValue) )
                {
                    continue;
                }

                var componentName = component.Value.Key;

                // If the component name already has a space then trust
                // that they are using the exact name formatting they want.
                if ( !componentName.Contains( ' ' ) )
                {
                    componentName = componentName.SplitCase();
                }

                items.Add( new ListItemBag
                {
                    Text = componentName,
                    Value = entityType.Guid.ToString().ToUpper()
                } );
            }


            return items;
        }

        #endregion
    }
}