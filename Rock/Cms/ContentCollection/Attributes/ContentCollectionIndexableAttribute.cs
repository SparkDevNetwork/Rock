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

namespace Rock.Cms.ContentCollection.Attributes
{
    /// <summary>
    /// Identifiers the class as supporting the content collection index system
    /// and specifies the class that will handle the indexing of items.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
    internal class ContentCollectionIndexableAttribute : System.Attribute
    {
        /// <summary>
        /// Gets the type of the indexer class that will handle indexing items.
        /// </summary>
        /// <value>The type of the indexer class that will handle indexing items.</value>
        public Type IndexerType { get; }

        /// <summary>
        /// Gets the type of the document that will be stored in the index.
        /// </summary>
        /// <value>The type of the document that will be stored in the index.</value>
        public Type DocumentType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentCollectionIndexableAttribute"/> class.
        /// </summary>
        /// <param name="indexerType">Type of the indexer that will handle indexing items.</param>
        /// <param name="documentType">Type of the document that will be stored in the index.</param>
        public ContentCollectionIndexableAttribute( Type indexerType, Type documentType )
        {
            IndexerType = indexerType;
            DocumentType = documentType;
        }
    }
}
