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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

namespace Rock.Rest.Handler
{
    /// <summary>
    /// Special implementation of PushStreamContent to ensure that the service
    /// scope is disposed of after the stream is finished, rather than when the
    /// <see cref="ServiceScopeHandler"/> has finished processing.
    /// </summary>
    sealed class ScopedPushStreamContent : PushStreamContent
    {
        private readonly IServiceScope _scope;
        private readonly PushStreamContent _innerContent;

        public ScopedPushStreamContent( IServiceScope scope, PushStreamContent innerContent )
            : base( ( _, __, ___ ) => { } )
        {
            _scope = scope;
            _innerContent = innerContent;

            // Copy headers from original content
            foreach ( var header in innerContent.Headers )
            {
                Headers.TryAddWithoutValidation( header.Key, header.Value );
            }
        }

        protected override async Task SerializeToStreamAsync( Stream stream, TransportContext context )
        {
            try
            {
                await _innerContent.CopyToAsync( stream, context );
            }
            finally
            {
                _scope.Dispose();
            }
        }

        protected override bool TryComputeLength( out long length )
        {
            length = -1L;

            return false;
        }
    }
}
