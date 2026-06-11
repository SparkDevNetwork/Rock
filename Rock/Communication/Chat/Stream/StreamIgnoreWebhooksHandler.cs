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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Rock.Communication.Chat
{
    /// <summary>
    /// A handler that adds a "x-stream-ext" header to outgoing requests, indicating that any resulting webhooks should
    /// be ignored. Stream will then send this value back to Rock within any resulting webhook requests. Finally, Rock
    /// will look for this value in order to determine whether it should ignore or handle a given webhook request.
    /// https://getstream.io/chat/docs/dotnet-csharp/webhooks_overview/#request-info
    /// </summary>
    internal sealed class StreamIgnoreWebhooksHandler : DelegatingHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StreamIgnoreWebhooksHandler"/> class.
        /// </summary>
        /// <param name="inner">The inner <see cref="HttpMessageHandler"/>.</param>
        public StreamIgnoreWebhooksHandler( HttpMessageHandler inner = null ) : base( inner ?? new HttpClientHandler() )
        {
        }

        /// <inheritdoc/>
        protected override Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
        {
            request.Headers.TryAddWithoutValidation(
                StreamChatProvider.HttpHeaderName.XStreamExt,
                StreamChatProvider.HttpHeaderValue.IgnoreWebhooks
            );

            return base.SendAsync( request, cancellationToken );
        }
    }
}
