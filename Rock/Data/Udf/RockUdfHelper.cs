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
using System.Collections.Generic;
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

        /// <summary>
        /// calls database function ufnCrm_GetFamilyTitle.  
        /// </summary>
        /// <param name="PersonId">The PersonId. NULL means use GroupId parameter</param>
        /// <param name="GroupId">The GroupId of the Family. NULL means use PersonId parameter</param>
        /// <param name="GroupPersonIds">If GroupId is specified, set this as a comma-delimited list of PersonIds that you want to limit the family members to. NULL means don't restrict. </param>
        /// <returns></returns>
        public static string ufnCrm_GetFamilyTitle( RockContext rockContext, int? PersonId, int? GroupId, string GroupPersonIds )
        {
            var result = rockContext.Database.SqlQuery(
                typeof( string ),
                "SELECT TOP 1 [PersonNames] FROM dbo.ufnCrm_GetFamilyTitle(null, @GroupId, @GroupPersonIds)",
                //new SqlParameter( "@PersonId", PersonId ) { SqlDbType = SqlDbType.Int, IsNullable = true },
                new SqlParameter( "@GroupId", GroupId ) { SqlDbType = SqlDbType.Int, IsNullable = true },
                new SqlParameter( "@GroupPersonIds", GroupPersonIds )
                ).OfType<string>().FirstOrDefault();
            return result;
        }
    }
}
