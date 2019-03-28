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

namespace Rock.UniversalSearch
{
    /// <summary>
    /// Rock Indexable
    /// </summary>
    public interface IRockIndexable
    {

        /// <summary>
        /// Gets a value indicating whether [allows interactive bulk indexing].
        /// </summary>
        /// <value>
        /// <c>true</c> if [allows interactive bulk indexing]; otherwise, <c>false</c>.
        /// </value>
        bool AllowsInteractiveBulkIndexing { get; }

        /// <summary>
        /// Bulks the index documents.
        /// </summary>
        void BulkIndexDocuments();

        /// <summary>
        /// Indexes the document.
        /// </summary>
        /// <param name="id">The identifier.</param>
        void IndexDocument( int id );

        /// <summary>
        /// Deletes the indexed document.
        /// </summary>
        /// <param name="id">The identifier.</param>
        void DeleteIndexedDocument( int id );

        /// <summary>
        /// Deletes the indexed documents.
        /// </summary>
        void DeleteIndexedDocuments();

        /// <summary>
        /// Indexes the name of the model.
        /// </summary>
        /// <returns></returns>
        Type IndexModelType();

        /// <summary>
        /// Gets the index filter values.
        /// </summary>
        /// <returns></returns>
        ModelFieldFilterConfig GetIndexFilterConfig();

        /// <summary>
        /// Gets the index filter field.
        /// </summary>
        /// <returns></returns>
        bool SupportsIndexFieldFiltering();
    }
}
