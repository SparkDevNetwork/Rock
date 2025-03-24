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

namespace Rock.Communication.Chat.Sync
{
    /// <summary>
    /// Represents the results of validating a webhook request from the external chat provider.
    /// </summary>
    internal class WebhookValidationResult
    {
        /// <summary>
        /// Gets the request body.
        /// </summary>
        public string RequestBody { get; }

        /// <summary>
        /// Gets the exception - if any - that occurred while validating the webhook request.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Gets a valid indicating whether this webhook is valid.
        /// </summary>
        public bool IsValid => Exception == null && RequestBody.IsNotNullOrWhiteSpace();

        /// <summary>
        /// Initializes a new instance of the <see cref="WebhookValidationResult"/> class.
        /// </summary>
        /// <param name="requestBody">The request body.</param>
        /// <param name="exception">The exception that occurred while validating the webhook request, if any.</param>
        public WebhookValidationResult( string requestBody, Exception exception = null )
        {
            RequestBody = requestBody ?? string.Empty;
            Exception = exception;
        }
    }
}
