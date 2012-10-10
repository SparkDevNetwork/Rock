using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Field
{
    /// <summary>
    /// The Name, Description and Value of an field type's configuration items
    /// </summary>
    public class ConfigurationValue
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationValue" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ConfigurationValue( string value )
        {
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationValue"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="value">The value.</param>
        public ConfigurationValue(string name, string description, string value)
        {
            Name = name;
            Description = description;
            Value = value;
        }
    }
}