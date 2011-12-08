using System;

namespace Rock.Data
{
    /// <summary>
    /// Represents a POCO class model
    /// </summary>
    public interface IModel : Security.ISecured
    {
        /// <summary>
        /// Gets or sets the GUID.
        /// </summary>
        /// <value>
        /// The GUID.
        /// </value>
        Guid Guid { get; set; }
    }
}