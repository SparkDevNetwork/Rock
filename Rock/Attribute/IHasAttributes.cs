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
        /// List of attributes associated with the object.  This property will not include the attribute values.
        /// The <see cref="AttributeValues"/> property should be used to get attribute values.  Dictionary key
        /// is the attribute key, and value is the cached attribute
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        Dictionary<string, Rock.Web.Cache.AttributeCache> Attributes { get; set; }

        /// <summary>
        /// Dictionary of all attributes and their value.  Key is the attribute key, and value is the values
        /// associated with the attribute and object instance
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        Dictionary<string, List<Rock.Model.AttributeValue>> AttributeValues { get; set; }

        /// <summary>
        /// Gets the attribute value defaults.  This property can be used by a subclass to override the parent class's default
        /// value for an attribute
        /// </summary>
        /// <value>
        /// The attribute value defaults.
        /// </value>
        Dictionary<string, string> AttributeValueDefaults { get; }        
        
        /// <summary>
        /// Gets the first value of an attribute key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        string GetAttributeValue( string key );

        /// <summary>
        /// Gets the first value of an attribute key - splitting that delimited value into a list of strings.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>A list of string values or an empty list if none exist.</returns>
        List<string> GetAttributeValues( string key );

        /// <summary>
        /// Sets the first value of an attribute key in memory.  Note, this will not persist value to database
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        void SetAttributeValue( string key, string value );
    }
}
