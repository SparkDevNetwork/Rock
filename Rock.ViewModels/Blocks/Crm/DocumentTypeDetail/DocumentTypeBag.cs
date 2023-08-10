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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Crm.DocumentTypeDetail
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class DocumentTypeBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the Rock.Model.BinaryFileType of the document type.
        /// </summary>
        public ListItemBag BinaryFileType { get; set; }

        /// <summary>
        /// Gets or sets the default document name template.
        /// </summary>
        public string DefaultDocumentNameTemplate { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.EntityType of the entities that Notes of this DocumentType 
        /// </summary>
        public ListItemBag EntityType { get; set; }

        /// <summary>
        /// Gets or sets the name of the qualifier column/property on the Rock.Model.EntityType that this Docuement Type applies to. If this is not 
        /// provided, the document type can be used on all entities of the provided Rock.Model.EntityType.
        /// </summary>
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the qualifier value in the qualifier column that this document type applies to.  For instance this note type and related notes will only be applicable to entity 
        /// if the value in the EntityTypeQualiferColumn matches this value. This property should not be populated without also populating the EntityTypeQualifierColumn property.
        /// </summary>
        public string EntityTypeQualifierValue { get; set; }

        /// <summary>
        /// Gets or sets the CSS class that is used for a vector/CSS icon.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the IsImage flag for the Rock.Model.DocumentType.
        /// </summary>
        public bool IsImage { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this DocumentType is part of the Rock core system/framework. This property is required.
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the maximum documents per entity.  This would limit the documents of that type per entity. A blank value means no limit.
        /// </summary>
        public int? MaxDocumentsPerEntity { get; set; }

        /// <summary>
        /// Gets or sets the given Name of the DocumentType.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the type is user selectable.
        /// </summary>
        public bool UserSelectable { get; set; }
    }
}
