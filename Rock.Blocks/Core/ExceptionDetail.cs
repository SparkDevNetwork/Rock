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
                    .AsNoTracking()
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

            // Navigate up to the outermost (root) exception.
            var baseException = GetOutermostException( exceptionService, exceptionId );

            if ( baseException == null )
            {
                return null;
            }

            var summaryException = exceptionService.Queryable()
                .AsNoTracking()
                .Include( e => e.Site )
                .Include( e => e.Page )
                .Include( e => e.CreatedByPersonAlias.Person )
                .FirstOrDefault( e => e.Id == baseException.Id );

            if ( summaryException == null )
            {
                return null;
            }

            // Build the query string items as structured key-value pairs.
            var queryStringItems = ParseQueryString( summaryException.QueryString );

            // Get the full exception hierarchy.
            var hierarchyLogs = GetExceptionHierarchy( exceptionService, summaryException );

            var exceptionItems = hierarchyLogs
                .OrderBy( e => e.Id )
                .Select( e => new ExceptionLogItemBag
                {
                    Id = e.Id,
                    ExceptionType = e.ExceptionType,
                    Source = e.Source,
                    Description = e.Description,
                    StackTrace = e.StackTrace
                } )
                .ToList();

            return new ExceptionDetailBag
            {
                ExceptionDate = summaryException.CreatedDateTime.HasValue
                    ? string.Format( "{0:g}", summaryException.CreatedDateTime.Value )
                    : string.Empty,
                Description = summaryException.Description.Truncate( 255 ),
                SiteName = summaryException.Site?.Name,
                PageName = summaryException.Page != null
                    ? summaryException.Page.InternalName
                    : summaryException.PageUrl,
                PageUrl = summaryException.PageUrl,
                QueryStringItems = queryStringItems,
                PersonFullName = summaryException.CreatedByPersonAlias?.Person?.FullName,
                PersonIdKey = summaryException.CreatedByPersonAlias?.Person?.IdKey,
                Cookies = summaryException.Cookies,
                ServerVariables = summaryException.ServerVariables,
                ExceptionItems = exceptionItems
            };
        }

        /// <summary>
        /// Navigates up the parent chain to find the outermost (root-level) exception.
        /// </summary>
        /// <param name="exceptionService">The exception log service.</param>
        /// <param name="exceptionId">The starting exception identifier.</param>
        /// <returns>The outermost exception, or null if not found.</returns>
        private ExceptionLog GetOutermostException( ExceptionLogService exceptionService, int exceptionId )
        {
            var exception = exceptionService.Queryable()
                .AsNoTracking()
                .FirstOrDefault( e => e.Id == exceptionId );

            if ( exception == null )
            {
                return null;
            }

            // Walk up the parent chain until we reach the root.
            while ( exception.ParentId.HasValue )
            {
                var parent = exceptionService.Queryable()
                    .AsNoTracking()
                    .FirstOrDefault( e => e.Id == exception.ParentId.Value );

                // If the parent cannot be found, the current exception is the effective root.
                if ( parent == null )
                {
                    break;
                }

                exception = parent;
            }

            return exception;
        }

        /// <summary>
        /// Gets the full exception hierarchy starting from the root exception,
        /// including parent chain and all inner (child) exceptions recursively.
        /// </summary>
        /// <param name="exceptionService">The exception log service.</param>
        /// <param name="rootException">The root-level exception.</param>
        /// <returns>A distinct list of all exception logs in the hierarchy.</returns>
        private List<ExceptionLog> GetExceptionHierarchy( ExceptionLogService exceptionService, ExceptionLog rootException )
        {
            var exceptionList = new List<ExceptionLog>();
            var visitedIds = new HashSet<int>();

            // Add the root exception.
            exceptionList.Add( rootException );
            visitedIds.Add( rootException.Id );

            // Recursively collect all child (inner) exceptions.
            CollectChildExceptions( exceptionService, rootException.Id, exceptionList, visitedIds );

            return exceptionList;
        }

        /// <summary>
        /// Recursively collects child exceptions for the specified parent.
        /// </summary>
        /// <param name="exceptionService">The exception log service.</param>
        /// <param name="parentId">The parent exception identifier.</param>
        /// <param name="exceptionList">The list to collect exceptions into.</param>
        /// <param name="visitedIds">Set of already-visited IDs to prevent cycles.</param>
        private void CollectChildExceptions( ExceptionLogService exceptionService, int parentId, List<ExceptionLog> exceptionList, HashSet<int> visitedIds )
        {
            var children = exceptionService
                .GetByParentId( parentId )
                .AsNoTracking()
                .ToList();

            foreach ( var child in children )
            {
                if ( visitedIds.Contains( child.Id ) )
                {
                    continue;
                }

                exceptionList.Add( child );
                visitedIds.Add( child.Id );

                if ( child.HasInnerException == true )
                {
                    CollectChildExceptions( exceptionService, child.Id, exceptionList, visitedIds );
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
