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

using Rock.Attribute;

namespace Rock.Utility.ContentLibraryApi
{
    /// <summary>
    /// Represents a Content Library API result.
    /// </summary>
    /// <typeparam name="T">The result data type.</typeparam>
    [RockInternal( "1.16" )]
    public class ContentLibraryApiResult<T>
    {
        /// <summary>
        /// Gets or sets the result data.
        /// </summary>
        /// <value>
        /// The result data.
        /// </value>
        public T Data { get; set; }

        /// <summary>
        /// Gets or sets the raw data.
        /// </summary>
        /// <value>
        /// The raw result data.
        /// </value>
        public string RawData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this result is successful.
        /// <para>The <see cref="Data"/> property may be <c>null</c> even when the result is successful.</para>
        /// </summary>
        /// <value>
        ///   <c>true</c> if this result is successful; otherwise, <c>false</c>.
        /// </value>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        public string Error { get; set; }
    }
}
