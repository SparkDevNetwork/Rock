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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Util;

namespace Rock.Web
{
    /// <summary>
    /// Validates all http request values
    /// </summary>
    public class RequestValidator : System.Web.Util.RequestValidator
    {
        /// <summary>
        /// Validates a string that contains HTTP request data.
        /// </summary>
        /// <param name="context">The context of the current request.</param>
        /// <param name="value">The HTTP request data to validate.</param>
        /// <param name="requestValidationSource">An enumeration that represents the source of request data that is being validated. The following are possible values for the enumeration:QueryStringForm CookiesFilesRawUrlPathPathInfoHeaders</param>
        /// <param name="collectionKey">The key in the request collection of the item to validate. This parameter is optional. This parameter is used if the data to validate is obtained from a collection. If the data to validate is not from a collection, <paramref name="collectionKey" /> can be null.</param>
        /// <param name="validationFailureIndex">When this method returns, indicates the zero-based starting point of the problematic or invalid text in the request collection. This parameter is passed uninitialized.</param>
        /// <returns>
        /// true if the string to be validated is valid; otherwise, false.
        /// </returns>
        protected override bool IsValidRequestString( HttpContext context, string value, RequestValidationSource requestValidationSource, string collectionKey, out int validationFailureIndex )
        {
            bool valid = base.IsValidRequestString( context, value, requestValidationSource, collectionKey, out validationFailureIndex );
            //if (!valid && requestValidationSource == RequestValidationSource.Form )
            //{
            //    // TODO: For now do not validate form values.  Eventually should provide way for just specific controls to be ignored
            //    validationFailureIndex = -1;
            //    return true;
            //}
            return valid;
        }
    }
}
