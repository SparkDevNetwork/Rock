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
using System.Diagnostics;
using System.Threading;

namespace Rock.Net
{
    /// <summary>
    /// <para>
    /// Provides an internal implementation to get the current request context.
    /// If possible, we should avoid ever making this public so we don't have
    /// to worry about plugins using it if we need to change how this works.
    /// </para>
    /// <para>
    /// This follows the pattern in .NET Core for accessing HttpContext.
    /// </para>
    /// </summary>
    [DebuggerDisplay( "RequestContext = {RequestContext}" )]
    internal static class RockRequestContextAccessor
    {
        private static readonly AsyncLocal<RequestContextHolder> _requestContextCurrent = new AsyncLocal<RequestContextHolder>();

        /// <summary>
        /// Gets or sets the current request context.
        /// </summary>
        /// <value>The current request context.</value>
        public static RockRequestContext RequestContext
        {
            get => _requestContextCurrent.Value?.Context;
            internal set
            {
                var holder = _requestContextCurrent.Value;

                if ( holder != null )
                {
                    // Clear current RockRequestContext trapped in the AsyncLocals, as its done.
                    holder.Context = null;
                }

                if ( value != null )
                {
                    // Use an object indirection to hold the RockRequestContext in the AsyncLocal,
                    // so it can be cleared in all ExecutionContexts when its cleared.
                    _requestContextCurrent.Value = new RequestContextHolder { Context = value };
                }
            }
        }

        private sealed class RequestContextHolder
        {
            public RockRequestContext Context;
        }
    }
}
