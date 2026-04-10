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

using System;

namespace Rock.Web
{
    /// <summary>
    /// Represents a processed CSS file that. This can be cached so none of
    /// the properties can be modified.
    /// </summary>
    internal class ProcessedCssFile
    {
        /// <summary>
        /// The raw content of the processed CSS file.
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// The hash value to use for ETag and cache invalidation.
        /// </summary>
        public string ETag { get; }

        /// <summary>
        /// The date and time the file on disk was last modified. This should
        /// only be used for cache invalidation on browsers since the actual
        /// contents might have "changed" more recently due to the processing.
        /// </summary>
        public DateTime LastModified { get; }

        /// <summary>
        /// Creates a new instance of <see cref="ProcessedCssFile"/> with the
        /// given content and last modified time. The ETag will be generated
        /// based on the content hash.
        /// </summary>
        /// <param name="content">The content of the processed file.</param>
        /// <param name="lastModified">The date and time the file was last modified on disk.</param>
        internal ProcessedCssFile( string content, DateTime lastModified )
        {
            Content = content;
            LastModified = lastModified;
            ETag = content.XxHash();
        }
    }
}
