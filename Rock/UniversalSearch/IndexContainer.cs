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
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Rock.Extension;
using Rock.UniversalSearch.IndexModels;

namespace Rock.UniversalSearch
{
    /// <summary>
    /// MEF Container class for Binary File Search Components
    /// </summary>
    public class IndexContainer : Container<IndexComponent, IComponentData>
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly Lazy<IndexContainer> instance =
            new Lazy<IndexContainer>( () => new IndexContainer() );

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static IndexContainer Instance
        {
            get {
                if ( instance != null )
                {
                    return instance.Value;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether [indexing enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [indexing enabled]; otherwise, <c>false</c>.
        /// </value>
        public static bool IndexingEnabled
        {
            get
            {
                return GetActiveComponent() != null;
            }
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static IndexComponent GetComponent( string entityType )
        {
            return Instance.GetComponentByEntity( entityType );
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static string GetComponentName( string entityType )
        {
            return Instance.GetComponentNameByEntity( entityType );
        }

        /// <summary>
        /// Gets the active component.
        /// </summary>
        /// <returns></returns>
        public static IndexComponent GetActiveComponent()
        {
            foreach ( var indexType in IndexContainer.Instance.Components )
            {
                var component = indexType.Value.Value;
                if ( component.IsActive )
                {
                    return component;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets or sets the MEF components.
        /// </summary>
        /// <value>
        /// The MEF components.
        /// </value>
        [ImportMany( typeof( IndexComponent ) )]
        protected override IEnumerable<Lazy<IndexComponent, IComponentData>> MEFComponents { get; set; }

        /// <summary>
        /// Indexes the documents.
        /// </summary>
        /// <param name="document">The document.</param>
        public static void IndexDocument( IndexModelBase document )
        {
            foreach ( var indexType in IndexContainer.Instance.Components )
            {
                var component = indexType.Value.Value;
                if ( component.IsActive && component.IsConnected )
                {
                   component.IndexDocument( document );
                }
            }
        }

        /// <summary>
        /// Indexes the documents.
        /// </summary>
        /// <param name="documents">The documents.</param>
        public static void IndexDocuments( IEnumerable<IndexModelBase> documents )
        {
            foreach ( var indexType in IndexContainer.Instance.Components )
            {
                var component = indexType.Value.Value;
                if ( component.IsActive && component.IsConnected )
                {
                    foreach(var document in documents )
                    {
                        component.IndexDocument( document );
                    }
                }
            }
        }

        /// <summary>
        /// Deletes the type of the documents by.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void DeleteDocumentsByType<T>() where T : class, new()
        {
            foreach ( var indexType in IndexContainer.Instance.Components )
            {
                var component = indexType.Value.Value;
                if ( component.IsActive && component.IsConnected )
                {
                    component.DeleteDocumentsByType<T>();
                }
            }
        }

        /// <summary>
        /// Deletes the document.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document">The document.</param>
        /// <param name="indexName">Name of the index.</param>
        public static void DeleteDocument<T>(T document, string indexName = null ) where T : class, new()
        {
            foreach ( var indexType in IndexContainer.Instance.Components )
            {
                var component = indexType.Value.Value;
                if ( component.IsActive && component.IsConnected )
                {
                    component.DeleteDocument<T>( document, indexName );
                }
            }
        }

        /// <summary>
        /// Deletes the document by property.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">The property value.</param>
        public static void DeleteDocumentByProperty(Type documentType, string propertyName, object propertyValue )
        {
            foreach ( var indexType in IndexContainer.Instance.Components )
            {
                var component = indexType.Value.Value;
                if ( component.IsActive && component.IsConnected )
                {
                    component.DeleteDocumentByProperty( documentType, propertyName, propertyValue );
                }
            }
        }

        /// <summary>
        /// Deletes the document by identifier.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="id">The identifier.</param>
        public static void DeleteDocumentById( Type documentType, int id )
        {
            foreach ( var indexType in IndexContainer.Instance.Components )
            {
                var component = indexType.Value.Value;
                if ( component.IsActive && component.IsConnected )
                {
                    component.DeleteDocumentById( documentType, id );
                }
            }
        }

        /// <summary>
        /// Creates the index.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="deleteIfExists">if set to <c>true</c> [delete if exists].</param>
        public static void CreateIndex(Type documentType, bool deleteIfExists = true)
        {
            foreach ( var indexType in IndexContainer.Instance.Components )
            {
                var component = indexType.Value.Value;
                if ( component.IsActive && component.IsConnected )
                {
                    component.CreateIndex( documentType, deleteIfExists );
                }
            }
        }

        /// <summary>
        /// Deletes the index.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        public static void DeleteIndex(Type documentType )
        {
            foreach ( var indexType in IndexContainer.Instance.Components )
            {
                var component = indexType.Value.Value;
                if ( component.IsActive && component.IsConnected )
                {
                    component.DeleteIndex( documentType );
                }
            }
        }
    }
}
