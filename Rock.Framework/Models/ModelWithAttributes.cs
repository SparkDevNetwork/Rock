using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Services;
using System.Linq;
using System.Web;

namespace Rock.Models
{
    /// <summary>
    /// If model needs to support attributes it should inherit from this base class instead of the Model class
    /// </summary>
    [IgnoreProperties(new[] { "AttributeValues" })]
    public class ModelWithAttributes : Model
    {
        // Note: For complex/non-entity types, we'll need to decorate some classes with the IgnoreProperties attribute
        // to tell WCF Data Services not to worry about the associated properties.

        /// <summary>
        /// Collection of attributes for an instance of a model
        /// </summary>
        [NotMapped]
        public List<Rock.Models.Core.Attribute> Attributes { get; set; }

        /// <summary>
        /// Lightweight collection of attribute name and values
        /// </summary>
        [NotMapped]
        public Dictionary<string, string> AttributeValues { get; set; }
    }
}