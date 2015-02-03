// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
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
