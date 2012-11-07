//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;

namespace Rock.Extension
{
    /// <summary>
    /// Interface for MEF Container
    /// </summary>
    public interface IContainerManaged
    {
        /// <summary>
        /// Gets the component name and attributes.
        /// </summary>
        Dictionary<int, KeyValuePair<string, ComponentManaged>> Dictionary { get; }

        /// <summary>
        /// Refreshes the components.
        /// </summary>
        void Refresh();
    }
}
