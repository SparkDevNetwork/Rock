using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.Models
{
    /// <summary>
    /// Represents a model that supports specific ordering
    /// </summary>
    internal interface IOrdered
    {
        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        int Order { get; set; }
    }
}
