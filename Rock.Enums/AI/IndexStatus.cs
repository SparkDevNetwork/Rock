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

namespace Rock.Model
{
    /// <summary>
    /// The lifecycle status of a <see cref="KnowledgeBaseDocument"/> in the indexing
    /// service (Ragie). Anything richer that the indexing service reports is normalized
    /// to one of these three values.
    /// </summary>
    [Enums.EnumDomain( "AI" )]
    public enum IndexStatus
    {
        /// <summary>
        /// The document is queued for indexing or is currently being indexed.
        /// Anything the indexing service reports that is not Ready or Failed is treated
        /// as Pending. New rows default to this value.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// The document has been successfully indexed and is available for retrieval.
        /// </summary>
        Ready = 1,

        /// <summary>
        /// The document failed to index. Inspect logs or re-queue to retry.
        /// </summary>
        Failed = 2
    }
}
