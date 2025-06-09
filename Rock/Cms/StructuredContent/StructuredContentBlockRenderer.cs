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
using System.IO;

namespace Rock.Cms.StructuredContent
{
    /// <summary>
    /// Provides the standard functionality for a plugin to add a new structured
    /// content block type to the system so it can be rendered out by Rock.
    /// </summary>
    /// <remarks>
    /// Subclass this class rather than trying to implement <see cref="IStructuredContentBlockRenderer"/> directly.
    /// </remarks>
    /// <typeparam name="TData">The type of the block data.</typeparam>
    /// <seealso cref="IStructuredContentBlockRenderer" />
    public abstract class StructuredContentBlockRenderer<TData> : IStructuredContentBlockRenderer, IStructuredContentBlockLavaRenderer
        where TData : class
    {
        /// <inheritdoc/>
        void IStructuredContentBlockRenderer.Render( TextWriter writer, dynamic data )
        {
            if ( data == null )
            {
                Render( writer, null );
            }
            else if ( data is Newtonsoft.Json.Linq.JToken token )
            {
                Render( writer, token.ToObject<TData>() );
            }
            else
            {
                throw new ArgumentException( "Expected a JToken object.", nameof( data ) );
            }
        }

        /// <summary>
        /// Renders the block data to the writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The block data.</param>
        protected abstract void Render( TextWriter writer, TData data );

        /// <inheritdoc/>
        public dynamic ResolveMergeFields( Dictionary<string, object> mergeFields, dynamic data )
        {
            if ( data == null )
            {
                return ResolveMergeFields( mergeFields, null );
            }
            else if ( data is Newtonsoft.Json.Linq.JToken token )
            {
                return ResolveMergeFields( mergeFields, token.ToObject<TData>() );
            }
            else
            {
                throw new ArgumentException( "Expected a JToken object.", nameof( data ) );
            }
        }

        /// <summary>
        /// Renders the block data to the writer.
        /// </summary>
        /// <param name="mergeFields">The merge fields to use when on the block data.</param>
        /// <param name="data">The block data.</param>
        protected virtual TData ResolveMergeFields( Dictionary<string, object> mergeFields, TData data )
        {
            return data;
        }
    }
}
