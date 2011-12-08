using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.FieldType
{
    /// <summary>
    /// Used to define a <see cref="Field"/> qualifier
    /// </summary>
    public class FieldQualifier
    {
        /// <summary>
        /// Gets the key.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Gets the name of the qualifier
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description of the qualifier
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Instantiates the field type
        /// </summary>
        public Field Field { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldQualifier"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="field">The field.</param>
        public FieldQualifier( string key, string name, string description, Field field )
        {
            Key = key;
            Name = name;
            Description = description;
            Field = field;
        }
    }
}