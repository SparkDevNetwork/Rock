//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using Rock.Extension;

namespace Rock.Address
{
    /// <summary>
    /// The base class for all address geocoding components
    /// </summary>
    public abstract class GeocodeComponent : Component
    {
        /// <summary>
        /// Abstract method for geocoding the specified address.  Derived classes should implement
        /// this method to geocode the address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="result">The result code unique to the service.</param>
        /// <returns>
        /// True/False value of whether the address was standardized succesfully
        /// </returns>
        public abstract bool Geocode( Rock.Crm.Address address, out string result );
    }

}
