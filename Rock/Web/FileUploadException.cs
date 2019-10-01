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
using System.ServiceModel.Web;

namespace Rock.Web
{
    /// <summary>
    /// Exception for when a file upload fails.  We don't want these to go to the standard error page, this is why we have a special exception class
    /// </summary>
    public class FileUploadException : WebFaultException<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileUploadException"/> class.
        /// </summary>
        /// <param name="detail">The detail.</param>
        /// <param name="statusCode">The status code.</param>
        public FileUploadException( string detail, System.Net.HttpStatusCode statusCode ) : base( detail, statusCode ) 
        {
        }
    }
}
