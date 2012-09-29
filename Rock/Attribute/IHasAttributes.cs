//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Collections.Generic;

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
        /// List of attributes associated with the object grouped by category.  This property will not include 
        /// the attribute values. The <see cref="AttributeValues"/> property should be used to get attribute values
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        SortedDictionary<string, List<Rock.Web.Cache.AttributeCache>> Attributes { get; set; }

        /// <summary>
        /// Dictionary of all attributes and their value.
        /// </summary>
        /// <remarks>
        /// The dictionary key stores tha attribute's key, and the dictionary value is a KeyValuePair object that
        /// stores the attribute name as the key and a list of the attribute's values as it's value. 
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
        Dictionary<string, KeyValuePair<string, List<Rock.Core.AttributeValueDto>>> AttributeValues { get; set; }
    }
}
