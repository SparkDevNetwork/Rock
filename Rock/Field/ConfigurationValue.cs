//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Rock.Field
{
    /// <summary>
    /// The Name, Description and Value of an field type's configuration items
    /// </summary>
    [Serializable]
    [DataContract( IsReference = true )]
    public class ConfigurationValue
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [DataMember]
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
        public ConfigurationValue( string name, string description, string value )
        {
            Name = name;
            Description = description;
            Value = value;
        }
    }
}