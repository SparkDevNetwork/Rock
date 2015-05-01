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
        /// Example: qry.Select( a => RockUdfHelper.ufnCrm_GetAddress(a.PersonId, "Home", "Full"))
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
        /// calls database TVF function ufnCrm_GetFamilyTitle.
        /// Usage: string familyTitle = RockUdfHelper.ufnCrm_GetFamilyTitle( rockContext, personId, groupId, commaPersonIds, true );
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="PersonId">The PersonId. NULL means use GroupId parameter</param>
        /// <param name="GroupId">The GroupId of the Family. NULL means use PersonId parameter</param>
        /// <param name="GroupPersonIds">If GroupId is specified, set this as a comma-delimited list of PersonIds that you want to limit the family members to. NULL means don't restrict.</param>
        /// <param name="UseNickName">if set to <c>true</c> [use nick name].</param>
        /// <returns></returns>
        public static string ufnCrm_GetFamilyTitle( RockContext rockContext, int? PersonId, int? GroupId, string GroupPersonIds, bool UseNickName )
        {
            var result = rockContext.Database.SqlQuery(
                typeof( string ),
                "SELECT TOP 1 [PersonNames] FROM dbo.ufnCrm_GetFamilyTitle(@PersonId, @GroupId, @GroupPersonIds, @UseNickName)",
                new SqlParameter( "@PersonId", PersonId.HasValue ? (object)PersonId.Value : DBNull.Value ) { SqlDbType = SqlDbType.Int, IsNullable = true },
                new SqlParameter( "@GroupId", GroupId.HasValue ? (object)GroupId.Value : DBNull.Value ) { SqlDbType = SqlDbType.Int, IsNullable = true },
                new SqlParameter( "@GroupPersonIds", string.IsNullOrWhiteSpace( GroupPersonIds ) ? DBNull.Value : (object)GroupPersonIds ) { SqlDbType = SqlDbType.Text, IsNullable = true },
                new SqlParameter( "@UseNickName", UseNickName ));

            // NOTE: ufnCrm_GetFamilyTitle is a Table Valued Function, but it only returns one ROW
            return result.OfType<string>().FirstOrDefault();
        }

        /// <summary>
        /// when used in a Linq Query, calls database function ufnGroup_GetGeofencingGroupNames
        /// Example: qry.Select( a => RockUdfHelper.ufnGroup_GetGeofencingGroupNames(a.PersonId, groupTypeId))
        /// </summary>
        /// <param name="PersonId">The person identifier.</param>
        /// <param name="GroupTypeId">The group type identifier </param>
        /// <returns></returns>
        [DbFunction( "CodeFirstDatabaseSchema", "ufnGroup_GetGeofencingGroupNames" )]
        public static string ufnGroup_GetGeofencingGroupNames( int? PersonId, int groupTypeId )
        {
            // this in-memory implementation will not be invoked when working on LINQ to Entities
            return null;
        }
    }
}
