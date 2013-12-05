//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;

namespace Rock.Data
{
    /// <summary>
    /// Custom attribute used to decorate model properties that are defined values from a system defined type
    /// </summary>
    [AttributeUsage(AttributeTargets.Property )]
    public class DefinedValueAttribute : System.Attribute
    {
        /// <summary>
        /// Gets or sets the defined type GUID.
        /// </summary>
        /// <value>
        /// The defined type GUID.
        /// </value>
        public Guid DefinedTypeGuid { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedValueAttribute" /> class.
        /// </summary>
        /// <param name="definedTypeGuid">The defined type GUID.</param>
        public DefinedValueAttribute( string definedTypeGuid )
        {
            DefinedTypeGuid = new Guid( definedTypeGuid );
        }

    }
}