using System.Collections.Generic;

namespace Rock.Extension
{
    /// <summary>
    /// Interface for MEF Container
    /// </summary>
    public interface IContainer
    {
        /// <summary>
        /// Gets the component name and attributes.
        /// </summary>
        Dictionary<int, KeyValuePair<string, Component>> Dictionary { get; }

        /// <summary>
        /// Refreshes the components.
        /// </summary>
        void Refresh();
    }
}
