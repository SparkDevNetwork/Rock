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
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Search.Person
{
    /// <summary>
    /// 
    /// </summary>
    public static class PersonQueryExtensions
    {
        /// <summary>
        /// Queries the name of the by.
        /// </summary>
        /// <param name="qry">The qry.</param>
        /// <param name="searchTerm">The search term.</param>
        /// <param name="reversed">if set to <c>true</c> [last first].</param>
        /// <returns></returns>
        public static IOrderedQueryable<Rock.Model.Person> QueryByName( this IQueryable<Rock.Model.Person> qry, string searchTerm, out bool reversed )
        {
            var names = searchTerm.SplitDelimitedValues();

            string firstName = string.Empty;
            string lastName = string.Empty;

            if ( searchTerm.Contains( ',' ) )
            {
                reversed = true;
                lastName = names.Length >= 1 ? names[0].Trim() : string.Empty;
                firstName = names.Length >= 2 ? names[1].Trim() : string.Empty;
            }
            else if ( searchTerm.Contains( ' ' ) )
            {
                reversed = false;
                firstName = names.Length >= 1 ? names[0].Trim() : string.Empty;
                lastName = names.Length >= 2 ? names[1].Trim() : string.Empty;
            }
            else
            {
                reversed = true;
                lastName = searchTerm.Trim();
            }

            if ( !string.IsNullOrWhiteSpace( lastName ) )
            {
                qry = qry.Where( p => p.LastName.StartsWith( lastName ) );
            }
            if ( !string.IsNullOrWhiteSpace( firstName ) )
            {
                qry = qry.Where( p => p.FirstName.StartsWith( firstName ) );
            }
      
            IOrderedQueryable<Rock.Model.Person> result;

            if ( reversed )
            {
                result = qry.OrderBy( p => p.LastName ).ThenBy( p => p.FirstName );
            }
            else
            {
                result = qry.OrderBy( p => p.FirstName ).ThenBy( p => p.LastName );
            }

            return result;
        }

        /// <summary>
        /// Selects the full names.
        /// </summary>
        /// <param name="qry">The qry.</param>
        /// <param name="searchTerm">The search term.</param>
        /// <param name="distinct">if set to <c>true</c> [distinct].</param>
        /// <returns></returns>
        public static IQueryable<string> SelectFullNames( this IQueryable<Rock.Model.Person> qry, string searchTerm, bool distinct = true )
        {
            bool reversed;
            var peopleQry = qry.QueryByName( searchTerm, out reversed );

            var selectQry = peopleQry.Select( p => ( reversed ?
                p.LastName + ", " + p.NickName + ( p.SuffixValueId.HasValue ? " " + p.SuffixValue.Name : "" ) :
                p.NickName + " " + p.LastName + ( p.SuffixValueId.HasValue ? " " + p.SuffixValue.Name : "" ) ) );

            if (distinct)
            {
                selectQry = selectQry.Distinct();
            }

            return selectQry;
        }
    }
}