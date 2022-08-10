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

namespace Rock.Cms.ContentCollection
{
    /// <summary>
    /// Provides a set of options that describe additional options that will be
    /// used when indexing documents.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal]
    public class IndexDocumentOptions
    {
        /// <summary>
        /// Gets or sets the maximum number of concurrent operations to perform.
        /// </summary>
        /// <value>The maximum number of concurrent operations to perform.</value>
        public int MaxConcurrency { get; set; } = 10;

        /// <summary>
        /// Gets or sets a value indicating whether this index operation should
        /// calculate trending values. Trending calculation can be an expensive
        /// operation so it must be explicitely enabled. Trending is only used
        /// when indexing an entire source, not individual documents.
        /// </summary>
        /// <value><c>true</c> if this index operation should calculate trending values; otherwise, <c>false</c>.</value>
        public bool IsTrendingEnabled { get; set; } = false;
    }
}
