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
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.OData;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// BinaryFileDatas REST API
    /// </summary>
    public partial class BinaryFileDatasController
    {
        /// <summary>
        /// Gets all binary file data records when the current person can read every file.
        /// </summary>
        /// <returns>A queryable of binary file data records.</returns>
        /// <exception cref="HttpResponseException">Thrown with a 401 status when the current person is not authorized to read every file.</exception>
        [Authenticate, Secured]
        [EnableQuery]
        public override IQueryable<BinaryFileData> Get()
        {
            EnsureCanReadAllFiles();

            return base.Get();
        }

        /// <summary>
        /// Gets the binary file data with the specified identifier when the current person is authorized to view the file.
        /// </summary>
        /// <param name="id">The identifier of the binary file.</param>
        /// <returns>The binary file data.</returns>
        /// <exception cref="HttpResponseException">Thrown with a 404 status when the file does not exist, or a 401 status when the current person is not authorized to view it.</exception>
        [Authenticate, Secured]
        [ActionName( "GetById" )]
        public override BinaryFileData GetById( int id )
        {
            EnsureCanViewFile( id );

            return base.GetById( id );
        }

        /// <summary>
        /// Gets the binary file data with the specified key when the current person is authorized to view the file.
        /// </summary>
        /// <param name="key">The identifier of the binary file.</param>
        /// <returns>The binary file data.</returns>
        /// <exception cref="HttpResponseException">Thrown with a 404 status when the file does not exist, or a 401 status when the current person is not authorized to view it.</exception>
        [Authenticate, Secured]
        [EnableQuery]
        public override BinaryFileData Get( [FromODataUri] int key )
        {
            EnsureCanViewFile( key );

            return base.Get( key );
        }

        /// <summary>
        /// Gets the binary file data records that have a particular attribute value when the current person can read every file.
        /// </summary>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="value">The attribute value to match.</param>
        /// <param name="caseSensitive">If set to <c>true</c>, the value is matched case sensitively.</param>
        /// <returns>A queryable of matching binary file data records.</returns>
        /// <exception cref="HttpResponseException">Thrown with a 401 status when the current person is not authorized to read every file.</exception>
        [Authenticate, Secured]
        [ActionName( "GetByAttributeValue" )]
        [EnableQuery]
        public override IQueryable<BinaryFileData> GetByAttributeValue( [FromUri] int? attributeId = null, [FromUri] string attributeKey = null, [FromUri] string value = null, [FromUri] bool caseSensitive = false )
        {
            EnsureCanReadAllFiles();

            return base.GetByAttributeValue( attributeId, attributeKey, value, caseSensitive );
        }

        /// <summary>
        /// Gets the binary file data records in the selected data view when the current person can read every file.
        /// </summary>
        /// <param name="id">The data view identifier.</param>
        /// <returns>A queryable of binary file data records.</returns>
        /// <exception cref="HttpResponseException">Thrown with a 401 status when the current person is not authorized to read every file.</exception>
        [Authenticate, Secured]
        [ActionName( "DataView" )]
        [EnableQuery]
        public override IQueryable<BinaryFileData> GetDataView( int id )
        {
            EnsureCanReadAllFiles();

            return base.GetDataView( id );
        }

        /// <summary>
        /// Gets the binary file data records followed by the specified person when the current person can read every file.
        /// </summary>
        /// <param name="personId">The identifier of the person whose followed records are returned.</param>
        /// <param name="personAliasId">The alias identifier of the person whose followed records are returned, used when <paramref name="personId"/> is not provided.</param>
        /// <returns>A queryable of followed binary file data records.</returns>
        /// <exception cref="HttpResponseException">Thrown with a 401 status when the current person is not authorized to read every file.</exception>
        [Authenticate, Secured]
        [ActionName( "FollowedItems" )]
        [EnableQuery]
        public override IQueryable<BinaryFileData> GetFollowedItems( int? personId = null, int? personAliasId = null )
        {
            EnsureCanReadAllFiles();

            return base.GetFollowedItems( personId, personAliasId );
        }

        /// <summary>
        /// Determines whether the person can read every file through this controller.
        /// </summary>
        /// <param name="person">The person to check.</param>
        /// <returns><c>true</c> if the person has ADMINISTRATE on the BinaryFileDatas REST controller; otherwise <c>false</c>.</returns>
        private static bool CanReadAllFiles( Person person )
        {
            var restController = RestControllerCache.Get( typeof( BinaryFileDatasController ).FullName );

            return restController != null && restController.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, person );
        }

        /// <summary>
        /// Ensures the current person can read every file through this controller.
        /// </summary>
        /// <exception cref="HttpResponseException">Thrown with a 401 status when the current person is not authorized to read every file.</exception>
        private void EnsureCanReadAllFiles()
        {
            /*
                10/16/2026 - MSE

                Bulk reads require ADMINISTRATE on this REST controller and are not filtered per file.
                Per-file security (ParentEntityAllowsView plus a per-file IsAuthorized VIEW check, the
                same checks GetFile.ashx makes) costs about 40ms or more per file (see the note in
                GetFile.ashx.cs) and cannot be expressed in SQL, so running it over a bulk query would
                materialize every row, including the raw BinaryFileData.Content bytes, and break OData paging.

                Reason: Keep file bytes from being listed by people who could not open each file through GetFile.ashx.
            */
            if ( !CanReadAllFiles( GetPerson() ) )
            {
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
            }
        }

        /// <summary>
        /// Ensures the current person can view the file with the specified identifier, using the same checks as GetFile.ashx.
        /// </summary>
        /// <param name="id">The identifier of the binary file.</param>
        /// <exception cref="HttpResponseException">Thrown with a 404 status when the file does not exist, or a 401 status when the current person is not authorized to view it.</exception>
        private void EnsureCanViewFile( int id )
        {
            /*
                10/16/2026 - MSE

                BinaryFileData has no security of its own, so a single read must apply the checks
                GetFile.ashx makes on the file before returning its bytes: the parent entity VIEW
                check first, then the per-file VIEW check when the file type requires view security.
                The GetFile security grant token does not apply to REST. A person who can read every
                file through the bulk routes skips the per-file check so the two stay consistent.

                Reason: Keep file bytes from being returned to people GetFile.ashx would refuse.
            */
            var person = GetPerson();

            if ( CanReadAllFiles( person ) )
            {
                return;
            }

            var binaryFile = new BinaryFileService( ( RockContext ) Service.Context ).Queryable()
                .Include( f => f.BinaryFileType )
                .AsNoTracking()
                .FirstOrDefault( f => f.Id == id );

            if ( binaryFile == null )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            var parentEntityAllowsView = binaryFile.ParentEntityAllowsView( person );
            var requiresViewSecurity = binaryFile.BinaryFileType?.RequiresViewSecurity ?? false;

            if ( !IsFileViewAllowed( parentEntityAllowsView, requiresViewSecurity, () => binaryFile.IsAuthorized( Rock.Security.Authorization.VIEW, person ) ) )
            {
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
            }
        }

        /// <summary>
        /// Determines whether a file can be viewed, in the same order GetFile.ashx checks it.
        /// </summary>
        /// <param name="parentEntityAllowsView">The result of <see cref="BinaryFile.ParentEntityAllowsView(Person)"/>, <c>null</c> when the file has no parent entity.</param>
        /// <param name="requiresViewSecurity">If set to <c>true</c>, the file type requires view security.</param>
        /// <param name="isAuthorizedToView">Checks whether the person is authorized to view the file. Only called when the file has no parent entity and its type requires view security, because the check is slow.</param>
        /// <returns><c>true</c> if the file can be viewed; otherwise <c>false</c>.</returns>
        private static bool IsFileViewAllowed( bool? parentEntityAllowsView, bool requiresViewSecurity, Func<bool> isAuthorizedToView )
        {
            if ( parentEntityAllowsView.HasValue )
            {
                return parentEntityAllowsView.Value;
            }

            return !requiresViewSecurity || isAuthorizedToView();
        }
    }
}
