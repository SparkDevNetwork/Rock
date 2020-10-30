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
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a DocumentType that is cached by Rock. 
    /// </summary>
    [Serializable]
    [DataContract]
    public class DocumentTypeCache : ModelCache<DocumentTypeCache, DocumentType>
    {

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the entity type id.
        /// </summary>
        /// <value>
        /// The entity type id.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the entity type qualifier column.
        /// </summary>
        /// <value>
        /// The entity type qualifier column.
        /// </value>
        [DataMember]
        public string EntityTypeQualifierColumn { get; private set; }

        /// <summary>
        /// Gets or sets the entity type qualifier value.
        /// </summary>
        /// <value>
        /// The entity type qualifier value.
        /// </value>
        [DataMember]
        public string EntityTypeQualifierValue { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [user selectable].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [user selectable]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool UserSelectable { get; private set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [DataMember]
        public string IconCssClass { get; private set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; private set; }

        /// <summary>
        /// Gets or sets the default document name template.
        /// </summary>
        /// <value>
        /// The default document name template.
        /// </value>
        [DataMember]
        public string DefaultDocumentNameTemplate
        {
            get; private set;
        }

        /// <summary>
        /// Gets or sets the binary file type identifier.
        /// </summary>
        /// <value>
        /// The binary file type type identifier.
        /// </value>
        [DataMember]
        public int BinaryFileTypeId { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var documentType = entity as DocumentType;
            if ( documentType == null )
                return;

            IsSystem = documentType.IsSystem;
            Name = documentType.Name;
            EntityTypeQualifierColumn = documentType.EntityTypeQualifierColumn;
            EntityTypeQualifierValue = documentType.EntityTypeQualifierValue;
            UserSelectable = documentType.UserSelectable;
            IconCssClass = documentType.IconCssClass;
            Order = documentType.Order;
            BinaryFileTypeId = documentType.BinaryFileTypeId;
            DefaultDocumentNameTemplate = documentType.DefaultDocumentNameTemplate;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Entity Note Types Cache

        /// <summary>
        /// Gets the by entity.
        /// </summary>
        /// <param name="entityTypeid">The entity typeid.</param>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        /// <param name="includeNonSelectable">if set to <c>true</c> [include non selectable].</param>
        /// <returns></returns>
        public static List<DocumentTypeCache> GetByEntity( int? entityTypeid, string entityTypeQualifierColumn, string entityTypeQualifierValue, bool includeNonSelectable = false )
        {
            var allEntityDocumentTypes = EntityDocumentTypesCache.Get();

            if ( allEntityDocumentTypes == null )
                return new List<DocumentTypeCache>();

            var matchingDocumentTypeIds = allEntityDocumentTypes.EntityDocumentTypes
                .Where( a => a.EntityTypeId.Equals( entityTypeid ) )
                .ToList()
                .Where( a =>
                    ( a.EntityTypeQualifierColumn ?? string.Empty ) == ( entityTypeQualifierColumn ?? string.Empty ) &&
                    ( a.EntityTypeQualifierValue ?? string.Empty ) == ( entityTypeQualifierValue ?? string.Empty ) )
                .SelectMany( a => a.DocumentTypesIds )
                .ToList();

            var documentTypes = new List<DocumentTypeCache>();
            foreach ( var documentTypeId in matchingDocumentTypeIds )
            {
                var documentType = Get( documentTypeId );
                if ( documentType != null && ( includeNonSelectable || documentType.UserSelectable ) )
                {
                    documentTypes.Add( documentType );
                }
            }

            return documentTypes;
        }

        /// <summary>
        /// Gets the by entity.
        /// </summary>
        /// <param name="entityTypeid">The entity typeid.</param>
        /// <param name="includeNonSelectable">if set to <c>true</c> [include non selectable].</param>
        /// <returns></returns>
        public static List<DocumentTypeCache> GetByEntity( int? entityTypeid, bool includeNonSelectable = false )
        {
            var allEntityDocumentTypes = EntityDocumentTypesCache.Get();

            if ( allEntityDocumentTypes == null )
                return new List<DocumentTypeCache>();

            var matchingDocumentTypeIds = allEntityDocumentTypes.EntityDocumentTypes
                .Where( a => a.EntityTypeId.Equals( entityTypeid ) )
                .SelectMany( a => a.DocumentTypesIds )
                .ToList();

            var documentTypes = new List<DocumentTypeCache>();
            foreach ( var documentTypeId in matchingDocumentTypeIds )
            {
                var documentType = Get( documentTypeId );
                if ( documentType != null && ( includeNonSelectable || documentType.UserSelectable ) )
                {
                    documentTypes.Add( documentType );
                }
            }

            return documentTypes;
        }

        /// <summary>
        /// Flushes the entity document types.
        /// </summary>
        public static void RemoveEntityDocumentTypes()
        {
            EntityDocumentTypesCache.Remove();
        }

        #endregion
    }


}