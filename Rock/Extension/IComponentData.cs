using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Extension
{
    /// <summary>
    /// Component Metadata
    /// </summary>
    public interface IComponentData
    {
        /// <summary>
        /// Gets the name of the component.
        /// </summary>
        /// <value>
        /// The name of the component.
        /// </value>
        string ComponentName { get; }
    }
}