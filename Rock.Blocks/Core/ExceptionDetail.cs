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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks.Core.ExceptionDetail;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays the details of the given exception, including its full hierarchy of inner exceptions.
    /// </summary>

    [DisplayName( "Exception Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of the given exception." )]
    [IconCssClass( "ti ti-bug" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "4467CF3A-9944-48AF-8D2E-7E270DEBD852" )]
    [Rock.SystemGuid.BlockTypeGuid( "6CFEAD0B-D36E-4CB9-B691-B7E6B06C9993" )]
    public class ExceptionDetail : RockBlockType
    {
        #region Keys

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
            using ( var rockContext = new RockContext() )
            {
                return GetExceptionDetailBag( rockContext );
            }
        }

        /// <summary>
        /// Loads and builds the <see cref="ExceptionDetailBag"/> from the current page parameters.
        /// </summary>
        /// <param name="rockContext">The database context to use for loading data.</param>
        /// <returns>
        /// An <see cref="ExceptionDetailBag"/> populated with exception data, or
        /// <c>null</c> if no matching exception could be found.
        /// </returns>
        private ExceptionDetailBag GetExceptionDetailBag( RockContext rockContext )
        {
            var exceptionService = new ExceptionLogService( rockContext );

            // Resolve the exception by Id first, then fall back to Guid.
            var exceptionId = PageParameter( PageParameterKey.ExceptionId ).AsIntegerOrNull();

            if ( !exceptionId.HasValue )
            {
                var exceptionGuid = PageParameter( PageParameterKey.ExceptionGuid ).AsGuidOrNull();

                if ( exceptionGuid.HasValue )
                {
                    exceptionId = exceptionService.Queryable()
                        .Where( e => e.Guid == exceptionGuid.Value )
                        .Select( e => e.Id )
                        .FirstOrDefault();
                }
            }

            if ( !exceptionId.HasValue || exceptionId.Value == 0 )
            {
                return null;
            }

            // Get the root-level (outermost) exception, then build the full hierarchy.
            var baseException = GetOutermostException( exceptionId.Value, exceptionService );

            if ( baseException == null )
            {
                return null;
            }

            var rootException = GetOutermostException( baseException, exceptionService );
            var exceptionLogs = GetExceptionHierarchy( rootException, exceptionService );

            // Reload the base exception with its navigation properties for the summary section.
            var baseExceptionWithDetails = exceptionService.Queryable()
                .Include( e => e.Site )
                .Include( e => e.Page )
                .Include( e => e.CreatedByPersonAlias.Person )
                .FirstOrDefault( e => e.Id == baseException.Id );

            if ( baseExceptionWithDetails == null )
            {
                return null;
            }

            var bag = new ExceptionDetailBag
            {
                ExceptionDate = baseExceptionWithDetails.CreatedDateTime.HasValue
                    ? baseExceptionWithDetails.CreatedDateTime.Value.ToString( "s" )
                    : null,
                Description = baseExceptionWithDetails.Description,
                SiteName = baseExceptionWithDetails.Site?.Name,
                PageName = baseExceptionWithDetails.Page?.InternalName,
                PageUrl = baseExceptionWithDetails.PageUrl,
                QueryString = baseExceptionWithDetails.QueryString,
                UserName = baseExceptionWithDetails.CreatedByPersonAlias?.Person?.FullName,
                Cookies = baseExceptionWithDetails.Cookies,
                ServerVariables = baseExceptionWithDetails.ServerVariables,
                HasCookies = !string.IsNullOrWhiteSpace( baseExceptionWithDetails.Cookies ),
                HasServerVariables = !string.IsNullOrWhiteSpace( baseExceptionWithDetails.ServerVariables ),
                ExceptionItems = exceptionLogs
                    .OrderBy( e => e.Id )
                    .Select( e => new ExceptionDetailItemBag
                    {
                        Id = e.Id,
                        ExceptionType = e.ExceptionType,
                        Source = e.Source,
                        Description = e.Description,
                        StackTrace = e.StackTrace,
                        HasStackTrace = e.StackTrace.IsNotNullOrWhiteSpace()
                    } )
                    .ToList()
            };

            return bag;
        }

        /// <summary>
        /// Traverses up the parent chain to find the outermost (root) exception
        /// in the hierarchy. Returns the same exception if it has no parent.
        /// </summary>
        /// <param name="exceptionId">The identifier of the starting exception.</param>
        /// <param name="exceptionService">The service used to load exceptions.</param>
        /// <returns>The root-level <see cref="ExceptionLog"/> in the hierarchy.</returns>
        private ExceptionLog GetOutermostException( int exceptionId, ExceptionLogService exceptionService )
        {
            var exception = exceptionService.GetNoTracking( exceptionId );

            return GetOutermostException( exception, exceptionService );
        }

        /// <summary>
        /// Traverses up the parent chain to find the outermost (root) exception
        /// in the hierarchy. Returns the same exception if it has no parent.
        /// </summary>
        /// <param name="exception">The starting exception log entry.</param>
        /// <param name="exceptionService">The service used to load parent exceptions.</param>
        /// <returns>The root-level <see cref="ExceptionLog"/> in the hierarchy.</returns>
        private ExceptionLog GetOutermostException( ExceptionLog exception, ExceptionLogService exceptionService )
        {
            if ( exception == null )
            {
                return null;
            }

            if ( exception.ParentId == null )
            {
                return exception;
            }

            while ( exception != null )
            {
                var parentException = exceptionService.Get( exception.ParentId.Value );

                // If the parent cannot be found, treat this exception as the root.
                if ( parentException == null )
                {
                    return exception;
                }

                if ( parentException.ParentId == null )
                {
                    return parentException;
                }

                exception = parentException;
            }

            return null;
        }

        /// <summary>
        /// Builds the complete exception hierarchy starting from the root exception,
        /// including any ancestor and descendant inner exceptions.
        /// </summary>
        /// <param name="rootException">The root-level exception to start from.</param>
        /// <param name="exceptionService">The service used to load related exceptions.</param>
        /// <returns>A distinct list of all <see cref="ExceptionLog"/> entries in the hierarchy.</returns>
        private List<ExceptionLog> GetExceptionHierarchy( ExceptionLog rootException, ExceptionLogService exceptionService )
        {
            var exceptionList = new List<ExceptionLog>();

            if ( rootException == null )
            {
                return exceptionList;
            }

            // Load the root and any ancestors (parent, grandparent, etc.).
            exceptionList.Add( rootException );

            var parentId = rootException.ParentId;

            while ( parentId != null && parentId > 0 )
            {
                var parentException = exceptionService.Get( parentId.Value );

                if ( parentException != null )
                {
                    exceptionList.Add( parentException );
                }

                parentId = parentException?.ParentId;
            }

            // Load all descendant inner exceptions recursively.
            var childLogs = new List<ExceptionLog>();
            GetChildExceptionsRecursive( exceptionService, rootException, ref childLogs );
            exceptionList.AddRange( childLogs );

            return exceptionList.Distinct().ToList();
        }

        /// <summary>
        /// Recursively collects all inner exceptions descending from the given exception.
        /// A duplicate check prevents stack overflows from potential circular references.
        /// </summary>
        /// <param name="exceptionService">The service used to load child exceptions.</param>
        /// <param name="exceptionLog">The exception whose children should be loaded.</param>
        /// <param name="exceptionLogs">The accumulating list of loaded child exceptions.</param>
        private void GetChildExceptionsRecursive( ExceptionLogService exceptionService, ExceptionLog exceptionLog, ref List<ExceptionLog> exceptionLogs )
        {
            if ( exceptionLogs.Any( a => a.Id == exceptionLog.Id ) )
            {
                // Prevent a potential stack overflow from circular references.
                return;
            }

            exceptionLogs.Add( exceptionLog );

            if ( exceptionLog.HasInnerException == true )
            {
                var innerExceptions = exceptionService.GetByParentId( exceptionLog.Id ).ToList();

                foreach ( var innerException in innerExceptions )
                {
                    GetChildExceptionsRecursive( exceptionService, innerException, ref exceptionLogs );
                }
            }
        }

        #endregion Methods
    }
}
