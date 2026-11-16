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

namespace Rock.Lava
{
    /// <summary>
    /// Internal extension methods for Lava merge field dictionaries.
    /// </summary>
    internal static class LavaMergeFieldExtensions
    {
        /// <summary>
        /// Adds the internal fields of the render context to a merge field
        /// dictionary, using keys that start with
        /// <see cref="LavaRenderContextBase.InternalMergeFieldPrefix"/>. When the
        /// dictionary is used to create a new render context, these entries
        /// become internal fields again. This lets a nested render keep the
        /// request context, the database transaction and the culture of the
        /// outer render. The enabled commands are copied too, but any commands
        /// passed when creating the new render context replace them.
        /// </summary>
        /// <param name="mergeFields">The merge field dictionary to add the internal fields to.</param>
        /// <param name="context">The render context to copy internal fields from.</param>
        internal static void AddInternalFieldsFrom( this IDictionary<string, object> mergeFields, ILavaRenderContext context )
        {
            var internalFields = context?.GetInternalFields();

            if ( internalFields == null || mergeFields == null )
            {
                return;
            }

            foreach ( var kvp in internalFields )
            {
                var key = kvp.Key.StartsWith( LavaRenderContextBase.InternalMergeFieldPrefix, StringComparison.Ordinal )
                    ? kvp.Key
                    : LavaRenderContextBase.InternalMergeFieldPrefix + kvp.Key;

                mergeFields[key] = kvp.Value;
            }
        }
    }
}
