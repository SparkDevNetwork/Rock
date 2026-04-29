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
