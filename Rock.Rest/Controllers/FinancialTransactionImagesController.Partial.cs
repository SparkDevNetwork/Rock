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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    public partial class FinancialTransactionImagesController
    {
        /// <summary>
        /// POST endpoint. Use this to INSERT a new FinancialTransactionImage.
        /// </summary>
        /// <remarks>
        /// Overrides the base method to require EDIT on the parent transaction. The base
        /// <see cref="ApiController{T}.Post" /> authorizes by reloading the entity by its Id,
        /// but a new record has an Id of 0 so only the entity default check remains.
        /// </remarks>
        /// <param name="value">The FinancialTransactionImage to add.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        public override HttpResponseMessage Post( [FromBody] FinancialTransactionImage value )
        {
            if ( value == null )
            {
                throw new HttpResponseException( HttpStatusCode.BadRequest );
            }

            EnsureCanEditTransaction( value.TransactionId );
            EnsureBinaryFileExists( value.BinaryFileId );

            return base.Post( value );
        }

        /// <summary>
        /// PUT endpoint. Use this to UPDATE a FinancialTransactionImage.
        /// </summary>
        /// <remarks>
        /// Overrides the base method to require EDIT on both the existing and the target
        /// parent transaction.
        /// </remarks>
        /// <param name="id">The identifier.</param>
        /// <param name="value">The value.</param>
        [Authenticate, Secured]
        public override void Put( int id, [FromBody] FinancialTransactionImage value )
        {
            if ( value == null )
            {
                throw new HttpResponseException( HttpStatusCode.BadRequest );
            }

            var existingTransactionId = GetExistingTransactionId( id );

            if ( existingTransactionId.HasValue )
            {
                EnsureCanEditTransaction( existingTransactionId.Value );
            }

            if ( value.TransactionId != existingTransactionId )
            {
                EnsureCanEditTransaction( value.TransactionId );
            }

            EnsureBinaryFileExists( value.BinaryFileId );

            base.Put( id, value );
        }

        /// <summary>
        /// PATCH endpoint. Use this to update a subset of the properties of a FinancialTransactionImage.
        /// </summary>
        /// <remarks>
        /// Overrides the base method to require EDIT on both the existing and any new
        /// parent transaction.
        /// </remarks>
        /// <param name="id">The identifier.</param>
        /// <param name="values">The values.</param>
        [Authenticate, Secured]
        public override void Patch( int id, [FromBody] Dictionary<string, object> values )
        {
            var existingTransactionId = GetExistingTransactionId( id );

            if ( existingTransactionId.HasValue )
            {
                EnsureCanEditTransaction( existingTransactionId.Value );
            }

            if ( values != null )
            {
                var newTransactionId = GetPatchedIntegerValue( values, nameof( FinancialTransactionImage.TransactionId ) );

                if ( newTransactionId.HasValue && newTransactionId != existingTransactionId )
                {
                    EnsureCanEditTransaction( newTransactionId.Value );
                }

                var newBinaryFileId = GetPatchedIntegerValue( values, nameof( FinancialTransactionImage.BinaryFileId ) );

                if ( newBinaryFileId.HasValue )
                {
                    EnsureBinaryFileExists( newBinaryFileId.Value );
                }
            }

            base.Patch( id, values );
        }

        /// <summary>
        /// DELETE endpoint. Use this to delete a FinancialTransactionImage.
        /// </summary>
        /// <remarks>
        /// Overrides the base method to require EDIT on the parent transaction.
        /// </remarks>
        /// <param name="id">The identifier.</param>
        [Authenticate, Secured]
        public override void Delete( int id )
        {
            var existingTransactionId = GetExistingTransactionId( id );

            if ( existingTransactionId.HasValue )
            {
                EnsureCanEditTransaction( existingTransactionId.Value );
            }

            base.Delete( id );
        }

        /// <summary>
        /// Gets the transaction identifier of an existing image, or null if the image does not exist.
        /// </summary>
        /// <param name="imageId">The image identifier.</param>
        /// <returns>The transaction identifier, or null.</returns>
        private int? GetExistingTransactionId( int imageId )
        {
            using ( var rockContext = RockApp.Current.CreateRockContext() )
            {
                return new FinancialTransactionImageService( rockContext ).GetSelect( imageId, i => ( int? ) i.TransactionId );
            }
        }

        /// <summary>
        /// Gets an integer value from a PATCH body, converted the same way the base Patch converts it.
        /// </summary>
        /// <param name="values">The PATCH values.</param>
        /// <param name="key">The property name.</param>
        /// <returns>The integer value, or null if the key is not present or is null.</returns>
        private int? GetPatchedIntegerValue( Dictionary<string, object> values, string key )
        {
            if ( !values.TryGetValue( key, out var value ) || value == null )
            {
                return null;
            }

            try
            {
                return Convert.ToInt32( value );
            }
            catch ( Exception ex ) when ( ex is FormatException || ex is InvalidCastException || ex is OverflowException )
            {
                var response = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, $"Cannot cast {key} to int32" );
                throw new HttpResponseException( response );
            }
        }

        /// <summary>
        /// Throws an Unauthorized exception if the current person cannot edit the transaction.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        private void EnsureCanEditTransaction( int transactionId )
        {
            using ( var rockContext = RockApp.Current.CreateRockContext() )
            {
                var transaction = new FinancialTransactionService( rockContext ).Get( transactionId );

                if ( transaction == null || !transaction.IsAuthorized( Rock.Security.Authorization.EDIT, GetPerson( rockContext ) ) )
                {
                    throw new HttpResponseException( HttpStatusCode.Unauthorized );
                }
            }
        }

        /// <summary>
        /// Throws a BadRequest exception if the binary file does not exist.
        /// </summary>
        /// <param name="binaryFileId">The binary file identifier.</param>
        private void EnsureBinaryFileExists( int binaryFileId )
        {
            using ( var rockContext = RockApp.Current.CreateRockContext() )
            {
                if ( !new BinaryFileService( rockContext ).Queryable().Any( f => f.Id == binaryFileId ) )
                {
                    var response = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, "The specified binary file does not exist." );
                    throw new HttpResponseException( response );
                }
            }
        }
    }
}
