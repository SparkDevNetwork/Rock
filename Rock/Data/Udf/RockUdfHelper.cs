using System.Data.Entity;

namespace Rock.Data
{
    // NOTE: Namespace for these need to literally be 'CodeFirstDatabaseSchema', not the namespace of the function or schema
    
    /// <summary>
    /// pattern from https://github.com/divega/UdfCodeFirstSample
    /// </summary>
    public static class RockUdfHelper
    {
        /// <summary>
        /// calls database function ufnCrm_GetAddress 
        /// </summary>
        /// <param name="PersonId">The person identifier.</param>
        /// <param name="AddressType">Type of the address.  Can either be text "Home", "Work", "Previous", or the Id of the GroupLocationTypeValue </param>
        /// <param name="AddressComponent">The address component  Can be 'Street1', 'Street2', 'City', 'State', 'PostalCode', 'Country'.</param>
        /// <returns></returns>
        [DbFunction( "CodeFirstDatabaseSchema", "ufnCrm_GetAddress" )]
        public static string ufnCrm_GetAddress( int? PersonId, string AddressType, string AddressComponent )
        {
            // this in-memory implementation will not be invoked when working on LINQ to Entities
            return null;
        }
    }
}
