//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Services;

namespace Rock.Data
{
    /// <summary>
    /// Represents a model with attributes. 
    /// </summary>
    [IgnoreProperties(new[] { "AttributeValues" })]
    public class ModelWithAttributes<T> : Model<T>, Rock.Attribute.IHasAttributes
    {
        private bool _attributesLoaded = false;

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
        public SortedDictionary<string, List<Rock.Web.Cache.Attribute>> Attributes
        {
            get 
            {
                if ( _attributes == null && !_attributesLoaded )
                {
                    Attribute.Helper.LoadAttributes( this );
                    _attributesLoaded = true;
                }
                return _attributes; 
            }
            set { _attributes = value; }
        }
        private SortedDictionary<string, List<Rock.Web.Cache.Attribute>> _attributes;

        /// <summary>
        /// Dictionary of all attributes and their value.
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        [NotMapped]
        public Dictionary<string, KeyValuePair<string, string>> AttributeValues
        {
            get 
            {
                if ( _attributeValues == null && !_attributesLoaded )
                {
                    Attribute.Helper.LoadAttributes( this );
                    _attributesLoaded = true;
                }
                return _attributeValues; 
            }
            set { _attributeValues = value; }
        }
        private Dictionary<string, KeyValuePair<string, string>> _attributeValues;
    }
}