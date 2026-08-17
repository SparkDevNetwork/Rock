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

namespace Rock.ViewModels.Blocks.Core.EntityTypes
{
    /// <summary>
    /// The bag of data that contains entity type data for the Entity Types block.
    /// </summary>
    public class EntityTypesBag
    {
        /// <summary>
        /// Id of Entity Type
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of entity type
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// friendly name of entity type
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Represents whether the entity is commonly used or not
        /// </summary>
        public bool IsCommon { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsSecured { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsRelatedToInteractionTrackedOnCreate { get; set; }

        /// <summary>
        /// Gets or sets the index result template.
        /// </summary>
        /// <value>
        /// The index result template.
        /// </value>
        public string IndexResultTemplate { get; set; }

        /// <summary>
        /// Gets or sets the index document URL.
        /// </summary>
        /// <value>
        /// The index document URL.
        /// </value>
        public string IndexDocumentUrl { get; set; }

        /// <summary>
        /// Gets or sets a lava template that can be used for generating a link to view details for this entity (i.e. "~/person/{{ Entity.Id }}").
        /// </summary>
        /// <value>
        /// The link URL.
        /// </value>
        public string LinkUrlLavaTemplate { get; set; }
    }
}
