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
    public static class RestApiConstants
    {
        /// <summary>
        /// A list of Error Codes that can be returned to a REST client.
        /// </summary>
        public static class ErrorCodes
        {
            public const string Unspecified = "Rock.RestApi.Error";
            
            public const string EntityValidationError = "Rock.RestApi.EntityValidationError";
            public const string DataAccessError = "Rock.RestApi.DataAccessError";
        }
    }
}