// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;

namespace Rock.Client.ErrorHandling
{
    /// <summary>
    ///     The default data contract for providing details of a processing error to a Rest API consumer.
    /// </summary>
    public class RestApiErrorDetail
    {
        /// <summary>
        ///     An application-specific identifier for the error.
        ///     The error code is a hierarchical string in the form of "{Application}.{Component}.{ErrorName}".
        ///     For errors returned from the Rock REST API, the error code is of the form "Rock.RestApi.{ErrorSpecificName}".
        /// </summary>
        public string ErrorCode = RestApiConstants.ErrorCodes.Unspecified;

        /// <summary>
        /// A user-friendly message that provides additional details about the error and any possible resolution.
        /// </summary>
        public string Details = "Please contact your Rock RMS system administrator for further assistance.";

        /// <summary>
        /// A user-friendly message that describes the general nature of the error.
        /// </summary>
        public string Message = "The Rock RMS Server encountered a problem while trying to process your request.";

        /// <summary>
        ///     An optional error code providing an internal identifier for the error.
        /// </summary>
        public string InternalCode = null;

        /// <summary>
        /// The server-side stack trace at the time the error occurred.
        /// This information is useful for diagnostic purposes, and is only available to system administrators.
        /// </summary>
        public string StackTrace = null;

        public RestApiErrorDetail()
        {
        }

        public RestApiErrorDetail(string code, string message)
            : this(code, null, message, null)
        {
        }

        public RestApiErrorDetail(string code, string message, string details)
            : this(code, null, message, details)
        {
        }

        public RestApiErrorDetail(string code, string subCode, string message, string details)
        {
            ErrorCode = code;
            InternalCode = subCode;
            Message = message;
            Details = details;
        }

        public override string ToString()
        {
            string message = "[ErrorCode=" + this.ErrorCode;

            if (!string.IsNullOrEmpty(this.InternalCode))
                message += ",SubCode=" + this.InternalCode;

            message += "]\n";

            message += this.Message;

            if (!string.IsNullOrEmpty(this.Details))
                message += "\n" + this.Details;

            return message;
        }
    }
}