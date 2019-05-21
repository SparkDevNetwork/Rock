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
using System.Net.Http.Headers;
using System.Web;

using Rock.Model;

namespace Rock.Financial
{
    /// <summary>
    /// Describes a gateway component that is setup to handle webhook events from a gateway.
    /// </summary>
    public interface IWebhookGatewayComponent
    {
        /// <summary>
        /// Handle a webhook event from a gateway. If used, it is recommended to limit network
        /// traffic to the webhook endpoint on the FinancialGateway controller specifically to
        /// the gateway's URL
        /// </summary>
        /// <param name="financialGateway">The financial gateway</param>
        /// <param name="requestHeaders">The headers from the webhook request.</param>
        /// <param name="request">The request from the gateway. Can be used to obtain the body.</param>
        /// <returns>True if the webhook was handled successfully, false otherwise</returns>
        bool HandleWebhook( FinancialGateway financialGateway, HttpRequestHeaders requestHeaders, HttpRequest request );
    }
}
