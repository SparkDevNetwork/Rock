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

using Rock.Data;
using Rock.Model;
using Rock.Net;

namespace Rock.Cms.StructuredContent
{
    /// <summary>
    /// Provides the standard functionality for a plugin to detect changes to
    /// structured content and make any database changes required.
    /// </summary>
    /// <remarks>
    /// Subclass this class rather than trying to implement <see cref="IStructuredContentBlockChangeHandler"/> directly.
    /// </remarks>
    /// <typeparam name="TData">The type of the block data.</typeparam>
    /// <seealso cref="IStructuredContentBlockChangeHandler" />
    public abstract class StructuredContentBlockChangeHandler<TData> : IStructuredContentBlockChangeHandler
        where TData : class
    {
        /// <inheritdoc/>
        void IStructuredContentBlockChangeHandler.DetectChanges( dynamic newData, dynamic oldData, StructuredContentChanges changes )
        {
            TData oldBlockData, newBlockData;

            if ( newData == null )
            {
                newBlockData = null;
            }
            else if ( newData is Newtonsoft.Json.Linq.JToken newToken )
            {
                newBlockData = newToken.ToObject<TData>();
            }
            else
            {
                throw new ArgumentException( "Expected a JToken object.", nameof( newData ) );
            }

            if ( oldData == null )
            {
                oldBlockData = null;
            }
            else if ( oldData is Newtonsoft.Json.Linq.JToken oldToken )
            {
                oldBlockData = oldToken.ToObject<TData>();
            }
            else
            {
                throw new ArgumentException( "Expected a JToken object.", nameof( oldData ) );
            }

            DetectBlockChanges( newBlockData, oldBlockData, changes );
        }

        /// <inheritdoc cref="IStructuredContentBlockChangeHandler.DetectChanges(dynamic, dynamic, StructuredContentChanges)"/>
        protected virtual void DetectBlockChanges( TData newData, TData oldData, StructuredContentChanges changes )
        {
        }

        /// <inheritdoc/>
        public virtual bool ApplyDatabaseChanges( StructuredContentHelper helper, StructuredContentChanges changes, RockContext rockContext )
        {
            return false;
        }

        /// <summary>
        /// Determines whether a file newly referenced by the content may be
        /// marked as permanent. A file that is already permanent, a save that
        /// does not come from a web request, or the current person's own
        /// temporary upload is allowed.
        /// </summary>
        /// <param name="binaryFile">The binary file referenced by the content.</param>
        /// <param name="binaryFileService">The binary file service to check the file with.</param>
        /// <returns><c>true</c> if the file may be marked as permanent; otherwise <c>false</c>.</returns>
        internal static bool CanMarkFileAsPermanent( BinaryFile binaryFile, BinaryFileService binaryFileService )
        {
            /*
                11/25/2026 - MSE

                The file references come from the content sent by the client, so a
                temporary file is only kept when the person saving uploaded it. Saves
                outside a web request (jobs) keep the previous behavior.

                Reason: Any temporary file could be kept by referencing it in the content.
            */
            if ( !binaryFile.IsTemporary )
            {
                return true;
            }

            var requestContext = RockRequestContextAccessor.Current;
            if ( requestContext == null )
            {
                return true;
            }

            return binaryFileService.IsUploadedBinaryFileAllowedForPerson( binaryFile.Id, null, requestContext.CurrentPerson );
        }
    }
}
