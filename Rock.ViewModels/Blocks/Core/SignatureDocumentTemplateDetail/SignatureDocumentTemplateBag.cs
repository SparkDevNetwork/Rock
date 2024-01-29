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
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Core.SignatureDocumentTemplateDetail
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class SignatureDocumentTemplateBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the type of the Rock.Model.BinaryFile.
        /// </summary>
        public ListItemBag BinaryFileType { get; set; }

        /// <summary>
        /// The System Communication that will be used when sending the signature document completion email.
        /// </summary>
        public ListItemBag CompletionSystemCommunication { get; set; }

        /// <summary>
        /// Gets or sets a user defined description or summary about the SignatureDocumentTemplate.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The term used to simply describe the document (wavier, release form, etc.).
        /// </summary>
        public string DocumentTerm { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this item is active or not.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// The Lava template that will be used to build the signature document.
        /// </summary>
        public string LavaTemplate { get; set; }

        /// <summary>
        /// Gets or sets the friendly Name of the SignatureDocumentTemplate. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        public ListItemBag ProviderEntityType { get; set; }

        /// <summary>
        /// Gets or sets the provider template key.
        /// </summary>
        public string ProviderTemplateKey { get; set; }

        /// <summary>
        /// Gets or sets the type of the signature.
        /// </summary>
        /// <value>
        /// The type of the signature.
        /// </value>
        public string SignatureType { get; set; }

        /// <summary>
        /// Gets or sets the signature input types.
        /// </summary>
        /// <value>
        /// The signature input types.
        /// </value>
        public List<ListItemBag> SignatureInputTypes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the signature document made using this template
        /// may be kept valid for future use.
        /// </summary>
        public bool IsValidInFuture { get; set; }

        /// <summary>
        /// Gets or sets a number of days the signature document made form this template be deemed valid.
        /// This property is honored only if the IsValidInFuture property is set.
        /// </summary>
        public int? ValidityDurationInDays { get; set; }
    }
}
