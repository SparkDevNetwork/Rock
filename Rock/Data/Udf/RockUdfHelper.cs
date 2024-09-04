// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;

namespace Rock.Data
{
    // NOTE: Namespace for these need to literally be 'CodeFirstDatabaseSchema', not the namespace of the function or schema

    /// <summary>
    /// pattern from https://github.com/divega/UdfCodeFirstSample
    /// </summary>
    public static class RockUdfHelper
    {
        /// <summary>
        /// when used in a Linq Query, calls database function ufnCrm_GetAddress 
        /// Example: qry.Select( a =&gt; RockUdfHelper.ufnCrm_GetAddress(a.PersonId, "Home", "Full"))
        /// </summary>
        /// <param name="PersonId">The person identifier.</param>
        /// <param name="AddressType">Type of the address.  Can either be text "Home", "Work", "Previous", or the Id of the GroupLocationTypeValue </param>
        /// <param name="AddressComponent">The address component  Can be 'Full','Street1', 'Street2', 'City', 'State', 'PostalCode', 'Country'.</param>
        /// <returns></returns>
        [DbFunction( "CodeFirstDatabaseSchema", "ufnCrm_GetAddress" )]
        public static string ufnCrm_GetAddress( int? PersonId, string AddressType, string AddressComponent )
        {
            // this in-memory implementation will not be invoked when working on LINQ to Entities
            return null;
        }

        /// <summary>
        /// The Enum equivs of what ufnCrm_GetAddress accepts for AddressComponent
        /// </summary>
        public enum AddressNamePart
        {
            /// <summary>
            /// Full Address
            /// </summary>
            Full = 0,

            /// <summary>
            /// Street1
            /// </summary>
            Street1 = 1,

            /// <summary>
            /// Street2
            /// </summary>
            Street2 = 2,

            /// <summary>
            /// City
            /// </summary>
            City = 3,

            /// <summary>
            /// State
            /// </summary>
            State = 4,

            /// <summary>
            /// Postal code
            /// </summary>
            PostalCode = 5,

            /// <summary>
            /// Country
            /// </summary>
            Country = 6,

            /// <summary>
            /// The GeoPoint Latitude
            /// </summary>
            Latitude = 7,

            /// <summary>
            /// The GeoPoint Longitude
            /// </summary>
            Longitude = 8
        }

        /// <summary>
        /// when used in a Linq Query, calls database function ufnGroup_GetGeofencingGroupNames
        /// Example: qry.Select( a =&gt; RockUdfHelper.ufnGroup_GetGeofencingGroupNames(a.PersonId, groupTypeId))
        /// </summary>
        /// <param name="PersonId">The person identifier.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        [DbFunction( "CodeFirstDatabaseSchema", "ufnGroup_GetGeofencingGroupNames" )]
        public static string ufnGroup_GetGeofencingGroupNames( int? PersonId, int groupTypeId )
        {
            // this in-memory implementation will not be invoked when working on LINQ to Entities
            return null;
        }

        /// <summary>
        /// when used in a Linq Query, calls database function ufnCrm_GetSpousePersonIdFromPersonId 
        /// Example: qry.Select( a =&gt; RockUdfHelper.ufnCrm_GetSpousePersonIdFromPersonId(a.PersonId))
        /// </summary>
        /// <param name="PersonId">The person identifier.</param>
        /// <returns></returns>
        [DbFunction( "CodeFirstDatabaseSchema", "ufnCrm_GetSpousePersonIdFromPersonId" )]
        public static int? ufnCrm_GetSpousePersonIdFromPersonId( int? PersonId )
        {
            // this in-memory implementation will not be invoked when working on LINQ to Entities
            return null;
        }
    }
}
