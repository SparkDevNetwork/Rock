using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Models
{
    /// <summary>
    /// Represents a POCO class model
    /// </summary>
    public interface IModel : Rock.Cms.Security.ISecured
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