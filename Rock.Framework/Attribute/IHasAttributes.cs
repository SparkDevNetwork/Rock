using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.Attribute
{
    /// <summary>
    /// Represents any class that supports having attributes
    /// </summary>
    public interface IHasAttributes
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// List of attributes associated with the object.  This property will not include the attribute values.
        /// The <see cref="AttributeValues"/> property should be used to get attribute values
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        List<Rock.Cms.Cached.Attribute> Attributes { get; set; }

        /// <summary>
        /// Dictionary of all attributes and their value.
        /// </summary>
        /// <remarks>
        /// The dictionary key stores tha attribute's key, and the dictionary value is a KeyValuePair object that
        /// stores the attribute name as the key and the attribute's value as it's value. 
        /// <example>
        /// Example
        /// </example>
        /// <code>
        /// <![CDATA[Dictionary<"AttributeKey", KeyValuePair<"Attribute Name", "Attribute Value">>]]>
        /// </code>
        /// </remarks>
        /// <value>
        /// The attribute values.
        /// </value>
        Dictionary<string, KeyValuePair<string, string>> AttributeValues { get; set; }
    }
}
