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
using System.Text.RegularExpressions;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.PageMap;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    [DisplayName( "Page Map" )]
    [Category( "CMS" )]
    [Description( "Displays a page map in a tree view." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage(
        "Root Page",
        Description = "Select the root page to use as a starting point for the tree view. Leaving empty will build a tree of all pages.",
        IsRequired = false,
        Key = AttributeKey.RootPage )]
    [EnumsField(
        "Site Type",
        Description = "Select the Site Type of the root-level pages shown in the page map. If no item is selected, all root-level pages will be shown.",
        IsRequired = false,
        EnumSourceType = typeof( SiteType ),
        Key = AttributeKey.SiteType )]

    #endregion Block Attributes

    //was [Rock.SystemGuid.BlockTypeGuid( "362179DE-5E57-46AE-A41D-A1E0F869179F" )]
    [Rock.SystemGuid.BlockTypeGuid( "2700A1B8-BD1A-40F1-A660-476DA86D0432" )]
    public class PageMap : RockBlockType
    {
        #region Properties

        private bool IsPredictableIdDisabled => PageCache.Layout.Site.DisablePredictableIds;

        private PageService PageService => new PageService( RockContext );

        #endregion Properties

        #region Keys

        private static class PageParameterKey
        {
            public const string PageId = "Page";
            public const string ParentPageId = "ParentPageId";
            public const string ExpandedIds = "ExpandedIds";
            public const string IsRedirect = "Redirect";
        }

        private static class AttributeKey
        {
            public const string RootPage = "RootPage";
            public const string SiteType = "SiteType";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            string errorMessage;

            if ( !IsPersonAuthorized( out errorMessage ) )
            {
                return new CustomBlockBox<PageMapBag, PageMapOptionsBag>
                {
                    ErrorMessage = errorMessage
                };
            }

            var box = new CustomBlockBox<PageMapBag, PageMapOptionsBag>
            {
                Options = GetBoxOptions(),
                NavigationUrls = GetBoxNavigationUrls()
            };

            box.Bag = GetBoxBag();

            return box;
        }

        /// <summary>
        /// Creates and returns a new instance of the PageMapOptionsBag containing
        /// the current block's configuration options.
        /// </summary>
        /// <returns>
        /// A PageMapOptionsBag object that encapsulates the block's configuration,
        /// including the root page and site type settings.
        /// </returns>
        private PageMapOptionsBag GetBoxOptions()
        {
            var isValidSiteType = Enum.TryParse( GetAttributeValue( AttributeKey.SiteType ), true, out SiteType siteType );

            var options = new PageMapOptionsBag
            {
                BlockProperties = new PageMapBlockAttributesBag
                {
                    RootPage = GetAttributeValue( AttributeKey.RootPage ).AsGuidOrNull(),
                    SiteType = isValidSiteType ? siteType : SiteType.Web,
                }
            };

            return options;
        }

        /// <summary>
        /// Retrieves a dictionary containing navigation URLs for the box component.
        /// </summary>
        /// <returns>
        /// A dictionary where each key is a navigation target name and
        /// each value is the corresponding URL. The dictionary contains
        /// an entry for the root page.
        /// </returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var dict = new Dictionary<string, string>
            {
                ["RootPage"] = this.GetLinkedPageUrl( AttributeKey.RootPage ),
            };

            return dict;
        }

        /// <summary>
        /// Creates and returns a new instance of a PageMapBag populated with tree list data.
        /// </summary>
        /// <returns>A PageMapBag instance containing the populated tree list data.</returns>
        private PageMapBag GetBoxBag()
        {
            var bag = new PageMapBag();

            PopulateTreeListBag( bag );

            return bag;
        }

        /// <summary>
        /// Populates the specified bag with data required for rendering a tree list view.
        /// </summary>
        /// <param name="bag">The bag to populate with tree list data.</param>
        private void PopulateTreeListBag( PageMapBag bag )
        {
            PopulateSelectedPageParameterGuids( bag );
            PopulateExpandedIds( bag );
        }

        /// <summary>
        /// Populates the selected page parameter GUIDs in the specified page map bag based
        /// on the current request context.
        /// </summary>
        /// <param name="bag">
        /// The page map bag to populate with selected page parameter GUIDs and any error messages.
        /// </param>
        private void PopulateSelectedPageParameterGuids( PageMapBag bag )
        {
            bag.TreeList = new PageMapTreeListBag();

            // Check for a value matching the key selected in the Block Attributes
            var pageParameterKey = PageParameterKey.PageId;
            var pageParameterValue = RequestContext.GetPageParameter( pageParameterKey );
            if ( !string.IsNullOrEmpty( pageParameterKey ) && !string.IsNullOrEmpty( pageParameterValue ) )
            {
                var attemptedSelectedGuids = GetPageGuidsFromIdKeys( new[] { pageParameterValue }.ToList(), out var error );

                if ( error.IsError )
                {
                    bag.ErrorMessage = error.Message;
                    return;
                }

                bag.TreeList.selectedItems = attemptedSelectedGuids;
            }

            return;
        }

        /// <summary>
        /// Populates the expanded item identifiers in the specified page map bag based on the
        /// current request's expanded IDs parameter.
        /// </summary>
        /// <remarks>
        /// If the expanded IDs parameter is missing or empty, no changes are made to the bag. If
        /// an error occurs while parsing the expanded IDs, the error message is set on the bag
        /// and no items are expanded.
        /// </remarks>
        /// <param name="bag">
        /// The page map bag to update with expanded item identifiers and any error message
        /// encountered during processing.
        /// </param>
        private void PopulateExpandedIds( PageMapBag bag )
        {
            var expandedIdsParameter = RequestContext.GetPageParameter( PageParameterKey.ExpandedIds );

            if ( string.IsNullOrEmpty( expandedIdsParameter ) )
            {
                return;
            }

            var expandedIds = expandedIdsParameter.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries )
                .Select( s => s.Trim() )
                .ToList();

            var attemptedSelectedGuids = GetPageGuidsFromIdKeys( expandedIds, out var error );

            if ( error.IsError )
            {
                bag.ErrorMessage = error.Message;
                return;
            }

            bag.TreeList.expandedItems = attemptedSelectedGuids;
            bag.TreeList.initiallyExpandedItems = attemptedSelectedGuids;
        }

        /// <summary>
        /// Retrieves the GUIDs of pages corresponding to the specified identifier keys.
        /// </summary>
        /// <remarks>
        /// If one or more keys do not match any existing page, the error parameter will indicate
        /// which keys were not found. The method does not throw an exception for missing keys; instead, it reports
        /// errors through the error parameter.
        /// </remarks>
        /// <param name="idKeys">
        /// A list of string keys representing the identifiers of the pages to retrieve. Cannot be null.
        /// </param>
        /// <param name="error">
        /// When this method returns, contains an ErrorPouch indicating whether any of the specified keys did not
        /// correspond to a page. If all keys are found, IsError is false.
        /// </param>
        /// <returns>
        /// A list of GUIDs for the pages that were found. The list contains one GUID for each key in idKeys that
        /// matches an existing page. If a key does not correspond to a page, it is omitted from the result.
        /// </returns>
        private List<Guid> GetPageGuidsFromIdKeys( List<string> idKeys, out ErrorPouch error )
        {
            var results = new List<Guid>();
            error = new ErrorPouch();

            foreach ( var key in idKeys )
            {
                var pageGuid = PageService.GetQueryableByKey( key, !IsPredictableIdDisabled )
                    .Select(p => p.Guid)
                    .FirstOrDefault();

                if ( pageGuid == Guid.Empty )
                {
                    error = MergeErrorPouch( error, new ErrorPouch()
                    {
                        IsError = true,
                        Message = $"Page with IdKey '{key}' was not found.",
                    } );
                    continue;
                }

                results.Add( pageGuid );
            }

            return results;
        }

        /// <summary>
        /// Determines whether the current person is authorized to edit or administrate the block.
        /// </summary>
        /// <returns>
        /// <see cref="true"/> if the current person has either edit or administrate authorization for the block;
        /// otherwise, <see cref="false"/>.
        /// </returns>
        private bool IsPersonEditOrAdminAuthorized()
        {
            var currentPerson = RequestContext.CurrentPerson;
            var allowedAuthorizations = new[] { Authorization.EDIT, Authorization.ADMINISTRATE };

            return allowedAuthorizations.Any( a => BlockCache.IsAuthorized( a, currentPerson ) );
        }

        /// <summary>
        /// Determines whether the current person is authorized to view the block.
        /// </summary>
        /// <returns>
        /// <see cref="true"/> if the current person has view authorization for the block;
        /// otherwise, <see cref="false"/>.
        /// </returns>
        private bool IsPersonViewAuthorized()
        {
            return BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Determines whether the current person is authorized to administrate, edit, or view the block.
        /// </summary>
        /// <returns>
        /// <see cref="true"/> if the current person has either edit or view authorization for the block;
        /// otherwise, <see cref="false"/>.
        /// </returns>
        private bool IsPersonAuthorized( out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( !IsPersonEditOrAdminAuthorized() && !IsPersonViewAuthorized() )
            {
                errorMessage = "You are not authorized to view this block.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the navigation URL for a specific entity.
        /// </summary>
        /// <param name="entityGuid">The GUID of the entity.</param>
        /// <param name="parentGuid">The GUID of the parent entity.</param>
        /// <param name="expandedGuids">A list of expanded entity GUIDs.</param>
        /// <param name="compiledError">An error pouch to capture any errors.</param>
        /// <returns>The navigation URL for the entity.</returns>
        private string GetNavigationUrl( Guid entityGuid, Guid parentGuid, List<Guid> expandedGuids, out ErrorPouch compiledError )
        {
            var currentError = new ErrorPouch();
            compiledError = new ErrorPouch();

            // If an empty guid is provided, we're adding an item, aka entityIdKey = "0"
            string entityIdKey = string.Empty;

            var relevantPages = PageService.Queryable().AsNoTracking()
                .Where(
                    p => entityGuid == p.Guid ||
                    parentGuid == p.Guid ||
                    expandedGuids.Contains( p.Guid )
                )
                .ToList();

            if ( entityGuid != Guid.Empty )
            {
                entityIdKey = relevantPages.Where( p => p.Guid == entityGuid ).Select( p => p.IdKey ).FirstOrDefault();
            }
            else
            {
                entityIdKey = "0";
            }

            if ( !string.IsNullOrEmpty( entityIdKey ) )
            {
                var qryParams = new Dictionary<string, string>();

                qryParams.Add( PageParameterKey.PageId, entityIdKey );

                // Parent Category IdKey, if any
                var parentIdKey = relevantPages.Where( p => p.Guid == parentGuid ).Select( p => p.IdKey ).FirstOrDefault();

                if ( !string.IsNullOrEmpty( parentIdKey ) )
                {
                    qryParams.Add( PageParameterKey.ParentPageId, parentIdKey );
                }

                // Expanded Category IdKeys, if any
                var expandedIds = relevantPages
                    .Where( p => expandedGuids.Contains( p.Guid ) )
                    .Select( p => p.IdKey )
                    .ToList();

                if ( expandedIds.Any() )
                {
                    qryParams.Add( PageParameterKey.ExpandedIds, string.Join( ",", expandedIds ) );
                }

                return this.GetCurrentPageUrl( qryParams, skipExistingParameters: true );
            }

            compiledError = MergeErrorPouch( compiledError, new ErrorPouch()
            {
                IsError = true,
                Message = "Invalid entity Guid.",
            } );

            return string.Empty;
        }

        /// <summary>
        /// Merges the error pouch instances, combining their error states and messages and clearing out the newError instance.
        /// </summary>
        /// <param name="baseError">The base error pouch to merge with.</param>
        /// <param name="newError">The new error pouch to merge.</param>
        /// <returns>The merged error pouch.</returns>
        private ErrorPouch MergeErrorPouch( ErrorPouch baseError, ErrorPouch newError )
        {
            if ( !newError.IsError )
            {
                return baseError;
            }

            var mergedError = new ErrorPouch
            {
                IsError = ( baseError?.IsError ?? false ) || ( newError?.IsError ?? false ),
                Message = string.Join( "\n", new[] { baseError?.Message, newError?.Message }.Where( m => !string.IsNullOrEmpty( m ) ) )
            };

            // This allows reusing the same newError instance for multiple merges, such as when multiple methods have out error parameters.
            newError = new ErrorPouch();

            return mergedError;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Retrieves a list of page items corresponding to the specified page identifier keys.
        /// </summary>
        /// <param name="pageIds">
        /// A list of page identifier keys to retrieve. Each entry should be a non-empty string
        /// representing a page's unique key.
        /// </param>
        /// <returns>
        /// A <see cref="BlockActionResult"/> containing a collection of <see cref="ListItemBag"/>
        /// objects for matching pages if found; otherwise, a result indicating a bad request or
        /// not found error.
        /// </returns>
        [BlockAction]
        public BlockActionResult GetPagesListItemBag( List<string> pageIds )
        {
            if ( pageIds.Count <= 0 || pageIds.All( pid => string.IsNullOrEmpty( pid ) ) )
            {
                if ( pageIds.Count > 1 )
                {
                    return ActionBadRequest( "Page IDs were not provided." );
                }
                else
                {
                    return ActionBadRequest( "Page ID was not provided." );
                }
            }

            var pages = PageService.Queryable().AsNoTracking().ToList();
            pages = pages.Where( p => pageIds.Contains( p.IdKey ) ).ToList();

            if ( pages.Count == 0 )
            {
                if ( pageIds.Count > 1 )
                {
                    return ActionNotFound( "Pages with specified IDs do not exist." );
                }
                else
                {
                    return ActionNotFound( "Page with specified ID does not exist." );
                }
            }

            var returnObject = pages.Select( p =>
                new ListItemBag
                {
                    Text = p.InternalName,
                    Value = p.Guid.ToString(),
                } )
                .ToList();

            return ActionOk( returnObject );
        }

        /// <summary>
        /// Retrieves the page IDs corresponding to the specified page GUIDs.
        /// </summary>
        /// <param name="pageGuids">
        /// A list of page GUIDs to retrieve IDs for. Each entry should be a non-empty GUID.
        /// </param>
        /// <returns>
        /// A <see cref="BlockActionResult"/> containing a collection of page IDs if found;
        /// otherwise, a result indicating a bad request or not found error.
        /// </returns>
        [BlockAction]
        public BlockActionResult GetPageIdsKey( List<Guid> pageGuids )
        {
            if ( pageGuids.Count <= 0 || pageGuids.All( pg => pg == Guid.Empty ) )
            {
                if ( pageGuids.Count > 1 )
                {
                    return ActionBadRequest( "Page GUIDs were not provided or empty." );
                }
                else
                {
                    return ActionBadRequest( "Page GUID was not provided or empty." );
                }
            }

            var pages = PageService.GetByGuids( pageGuids ).ToList();

            if ( pages.Count <= 0 )
            {
                if ( pageGuids.Count > 1 )
                {
                    return ActionBadRequest( "Page GUIDs don't exist" );
                }
                else
                {
                    return ActionBadRequest( "Page GUID doesn't exist" );
                }
            }

            return ActionOk( pages.Select( p => p.IdKey ).ToList() );
        }

        /// <summary>
        /// Attempts to retrieve the navigation URL for the specified entity within a
        /// navigation tree structure.
        /// </summary>
        /// <param name="entityGuid">
        /// The unique identifier of the entity for which to obtain the navigation URL.
        /// </param>
        /// <param name="parentGuid">
        /// The unique identifier of the parent entity in the navigation hierarchy.
        /// </param>
        /// <param name="expandedGuids">
        /// A list of entity GUIDs representing the currently expanded nodes in the
        /// navigation tree. Used to determine the navigation context.
        /// </param>
        /// <returns>
        /// A result containing the navigation URL if successful; otherwise, a bad request
        /// result with an error message.
        /// </returns>
        [BlockAction]
        public BlockActionResult GetNavigationUrl( Guid entityGuid, Guid parentGuid, List<Guid> expandedGuids)
        {
            var url = GetNavigationUrl( entityGuid, parentGuid, expandedGuids, out var error );

            if ( string.IsNullOrEmpty( url ) )
            {
                if ( error.IsError )
                {
                    return ActionBadRequest( error.Message );
                }

                return ActionBadRequest( "Could not determine navigation URL for the provided entity." );
            }

            return ActionOk( url );
        }

        #endregion Block Actions

        #region Helper Classes

        private class ErrorPouch
        {
            public bool IsError { get; set; } = false;
            public string Message { get; set; } = string.Empty;
        }

        #endregion Helper Classes
    }
}