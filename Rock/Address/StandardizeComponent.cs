//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using Rock.Extension;

namespace Rock.Address
{
    /// <summary>
    /// The base class for all address standardization components
    /// </summary>
    public abstract class StandardizeComponent : ComponentManaged
    {
        /// <summary>
        /// Abstract method for standardizing the specified address.  Derived classes should implement
        /// this method to standardize the address.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="result">The result code unique to the service.</param>
        /// <returns>
        /// True/False value of whether the address was standardized succesfully
        /// </returns>
        public abstract bool Standardize( Rock.Model.Location location, out string result );
    }
}
