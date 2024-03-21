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
using System.Threading;

namespace Rock.Net
{
    /// <summary>
    /// Internal implementation to provide the current RockRequestContext
    /// through dependency injection.
    /// </summary>
    class RockRequestContextAccessor : IRockRequestContextAccessor
    {
        /// <summary>
        /// The rock request context for the current async execution context.
        /// </summary>
        private static readonly AsyncLocal<RockRequestContextHolder> _rockRequestContext = new AsyncLocal<RockRequestContextHolder>();

        /// <inheritdoc/>
        public RockRequestContext RockRequestContext
        {
            get
            {
                return _rockRequestContext.Value?.Context;
            }
            set
            {
                var holder = _rockRequestContext.Value;

                if ( holder != null )
                {
                    // Clear current RockRequestContext trapped in the AsyncLocals.
                    holder.Context = null;
                }

                if ( value != null )
                {
                    // Use an indirect object to hold the real context. This
                    // allows us to clear it even in other execution contexts
                    // that have inherited the AsyncLocal.
                    _rockRequestContext.Value = new RockRequestContextHolder { Context = value };
                }
            }
        }

        private class RockRequestContextHolder
        {
            public RockRequestContext Context;
        }
    }
}
