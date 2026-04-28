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

using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.ExceptionDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    [DisplayName( "Exception Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of the given exception." )]

    [Rock.SystemGuid.EntityTypeGuid( "C14DCEF9-7C83-4D26-B203-74F03ECDAA33" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "EEBEF78B-1252-41E1-A5A0-0600E5C8DDCD" )]
    [Rock.SystemGuid.BlockTypeGuid( "B9E704E8-2097-491D-A216-8011012AA84E" )]
    public class ExceptionDetail : RockBlockType
    {
        #region Keys

        /// <summary>
        /// Keys for page parameters.
        /// </summary>
        private static class PageParameterKey
        {
            public const string ExceptionId = "ExceptionId";
            public const string ExceptionGuid = "ExceptionGuid";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<ExceptionDetailBag, ExceptionDetailOptionsBag>();

            var exceptionId = ResolveExceptionId();

            if ( exceptionId == 0 )
            {
                box.ErrorMessage = "The specified exception could not be found.";
                return box;
            }

            box.Bag = GetExceptionDetailBag( exceptionId );

            if ( box.Bag == null )
            {
                box.ErrorMessage = "The specified exception could not be found.";
            }

            return box;
        }

        /// <summary>
        /// Resolves the exception identifier from page parameters.
        /// Supports ExceptionId as an IdKey, int, or Guid, and also the ExceptionGuid parameter.
        /// </summary>
        /// <returns>The resolved exception ID, or 0 if not found.</returns>
        private int ResolveExceptionId()
        {
            var exceptionService = new ExceptionLogService( RockContext );
            var exceptionIdKey = PageParameter( PageParameterKey.ExceptionId );

            if ( exceptionIdKey.IsNotNullOrWhiteSpace() )
            {
                // Try to resolve as IdKey or int.
                var exception = exceptionService.Get( exceptionIdKey, !PageCache.Layout.Site.DisablePredictableIds );

                if ( exception != null )
                {
                    return exception.Id;
                }
            }

            var exceptionGuid = PageParameter( PageParameterKey.ExceptionGuid ).AsGuidOrNull();

            if ( exceptionGuid.HasValue )
            {
                return exceptionService.Queryable()
                    .Where( e => e.Guid == exceptionGuid.Value )
                    .Select( e => e.Id )
                    .FirstOrDefault();
            }

            return 0;
        }

        /// <summary>
        /// Builds the exception detail bag for the specified exception ID.
        /// </summary>
        /// <param name="exceptionId">The exception identifier.</param>
        /// <returns>The populated detail bag, or null if the exception is not found.</returns>
        private ExceptionDetailBag GetExceptionDetailBag( int exceptionId )
        {
            var exceptionService = new ExceptionLogService( RockContext );

            // Walk up to the outermost (root) exception.
            var rootId = GetOutermostExceptionId( exceptionService, exceptionId );

            if ( rootId == 0 )
            {
                return null;
            }

            // Collect all exception IDs in the hierarchy using lightweight projections.
            var hierarchyIds = CollectHierarchyIds( exceptionService, rootId ).ToList();

            // Load all exceptions with navigation properties in a single query.
            var exceptions = exceptionService.Queryable()
                .AsNoTracking()
                .Include( e => e.Site )
                .Include( e => e.Page )
                .Include( e => e.CreatedByPersonAlias.Person )
                .Where( e => hierarchyIds.Contains( e.Id ) )
                .OrderBy( e => e.Id )
                .ToList();

            var rootException = exceptions.FirstOrDefault( e => e.Id == rootId );

            if ( rootException == null )
            {
                return null;
            }

            return new ExceptionDetailBag
            {
                RootException = BuildExceptionLogItemBag( rootException ),
                Cookies = rootException.Cookies.SanitizeHtml( strict: false ),
                ServerVariables = rootException.ServerVariables.SanitizeHtml( strict: false ),
                InnerExceptions = exceptions
                    .Where( e => e.Id != rootId )
                    .Select( e => BuildExceptionLogItemBag( e ) )
                    .ToList()
            };
        }

        /// <summary>
        /// Builds an <see cref="ExceptionLogItemBag"/> from an <see cref="ExceptionLog"/>
        /// entity with all display fields populated.
        /// </summary>
        /// <param name="exception">The exception log entity (with navigation properties loaded).</param>
        /// <returns>The populated item bag.</returns>
        private ExceptionLogItemBag BuildExceptionLogItemBag( ExceptionLog exception )
        {
            return new ExceptionLogItemBag
            {
                Id = exception.Id,
                ExceptionDate = exception.CreatedDateTime.HasValue
                    ? string.Format( "{0:g}", exception.CreatedDateTime.Value )
                    : string.Empty,
                ExceptionType = exception.ExceptionType,
                Source = exception.Source,
                Description = exception.Description,
                StackTrace = exception.StackTrace,
                SiteName = exception.Site?.Name,
                PageName = exception.Page != null
                    ? exception.Page.InternalName
                    : exception.PageUrl,
                PageUrl = exception.PageUrl,
                QueryStringItems = ParseQueryString( exception.QueryString ),
                PersonFullName = exception.CreatedByPersonAlias?.Person?.FullName,
                PersonIdKey = exception.CreatedByPersonAlias?.Person?.IdKey
            };
        }

        /// <summary>
        /// Navigates up the parent chain to find the outermost (root-level) exception ID
        /// using lightweight projections that only select Id and ParentId.
        /// </summary>
        /// <param name="exceptionService">The exception log service.</param>
        /// <param name="exceptionId">The starting exception identifier.</param>
        /// <returns>The outermost exception ID, or 0 if not found.</returns>
        private int GetOutermostExceptionId( ExceptionLogService exceptionService, int exceptionId )
        {
            var current = exceptionService.Queryable()
                .Where( e => e.Id == exceptionId )
                .Select( e => new { e.Id, e.ParentId } )
                .FirstOrDefault();

            if ( current == null )
            {
                return 0;
            }

            // Walk up the parent chain until we reach the root.
            while ( current.ParentId.HasValue )
            {
                var parent = exceptionService.Queryable()
                    .Where( e => e.Id == current.ParentId.Value )
                    .Select( e => new { e.Id, e.ParentId } )
                    .FirstOrDefault();

                // If the parent cannot be found, the current exception is the effective root.
                if ( parent == null )
                {
                    break;
                }

                current = parent;
            }

            return current.Id;
        }

        /// <summary>
        /// Collects all exception IDs in the hierarchy starting from the root,
        /// using lightweight projections that only select Id and HasInnerException.
        /// </summary>
        /// <param name="exceptionService">The exception log service.</param>
        /// <param name="rootId">The root exception identifier.</param>
        /// <returns>A set of all exception IDs in the hierarchy.</returns>
        private HashSet<int> CollectHierarchyIds( ExceptionLogService exceptionService, int rootId )
        {
            var visitedIds = new HashSet<int> { rootId };
            CollectChildIds( exceptionService, rootId, visitedIds );

            return visitedIds;
        }

        /// <summary>
        /// Recursively collects child exception IDs for the specified parent
        /// using lightweight projections.
        /// </summary>
        /// <param name="exceptionService">The exception log service.</param>
        /// <param name="parentId">The parent exception identifier.</param>
        /// <param name="visitedIds">Set of already-visited IDs to prevent cycles.</param>
        private void CollectChildIds( ExceptionLogService exceptionService, int parentId, HashSet<int> visitedIds )
        {
            var children = exceptionService
                .GetByParentId( parentId )
                .Select( e => new { e.Id, e.HasInnerException } )
                .ToList();

            foreach ( var child in children )
            {
                if ( !visitedIds.Add( child.Id ) )
                {
                    continue;
                }

                if ( child.HasInnerException == true )
                {
                    CollectChildIds( exceptionService, child.Id, visitedIds );
                }
            }
        }

        /// <summary>
        /// Parses a query string into a list of key-value pairs.
        /// </summary>
        /// <param name="queryString">The raw query string.</param>
        /// <returns>A list of ListItemBag where Text is the key and Value is the value.</returns>
        private List<ListItemBag> ParseQueryString( string queryString )
        {
            if ( queryString.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var items = new List<ListItemBag>();
            var variables = queryString.TrimStart( '?' ).Split( '&' );

            foreach ( var variable in variables )
            {
                var parts = variable.Split( new[] { '=' }, 2 );

                items.Add( new ListItemBag
                {
                    Text = parts[0],
                    Value = parts.Length > 1 ? parts[1] : string.Empty
                } );
            }

            return items;
        }

        #endregion Methods
    }
}
