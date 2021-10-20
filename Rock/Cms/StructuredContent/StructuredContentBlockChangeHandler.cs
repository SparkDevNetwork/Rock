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
    }
}
