using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Services;
using System.Linq;
using System.Web;

namespace Rock.Models
{
    /// <summary>
    /// Represents a model with attributes. 
    /// </summary>
    [IgnoreProperties(new[] { "AttributeValues" })]
    public class ModelWithAttributes<T> : Model<T>, Rock.Attribute.IHasAttributes
    {
        // Note: For complex/non-entity types, we'll need to decorate some classes with the IgnoreProperties attribute
        // to tell WCF Data Services not to worry about the associated properties.

        /// <summary>
        /// List of attributes associated with the object.  This property will not include the attribute values.
        /// The <see cref="AttributeValues"/> property should be used to get attribute values
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        [NotMapped]
        public List<Rock.Cms.Cached.Attribute> Attributes { get; set; }

        /// <summary>
        /// Dictionary of all attributes and their value.
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        [NotMapped]
        public Dictionary<string, KeyValuePair<string, string>> AttributeValues { get; set; }
    }
}