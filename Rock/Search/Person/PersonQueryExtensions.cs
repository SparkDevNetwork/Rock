//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
        /// <param name="lastFirst">if set to <c>true</c> [last first].</param>
        /// <returns></returns>
        public static IOrderedQueryable<Rock.Model.Person> QueryByName( this IQueryable<Rock.Model.Person> qry, string searchTerm, out bool lastFirst )
        {
            var names = searchTerm.SplitDelimitedValues();

            string firstName = string.Empty;
            string lastName = string.Empty;

            if ( searchTerm.Contains( ',' ) )
            {
                lastFirst = true;
                lastName = names.Length >= 1 ? names[0].Trim() : string.Empty;
                firstName = names.Length >= 2 ? names[1].Trim() : string.Empty;
            }
            else if ( searchTerm.Contains( ' ' ) )
            {
                lastFirst = false;
                firstName = names.Length >= 1 ? names[0].Trim() : string.Empty;
                lastName = names.Length >= 2 ? names[1].Trim() : string.Empty;
            }
            else
            {
                lastFirst = true;
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

            if ( lastFirst )
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
            bool lastFirst;
            var peopleQry = qry.QueryByName( searchTerm, out lastFirst );

            var selectQry = peopleQry.Select( p => ( lastFirst ? p.LastName + ", " + ( p.NickName ?? p.GivenName ) : ( p.NickName ?? p.GivenName ) + " " + p.LastName ) );

            if (distinct)
            {
                selectQry = selectQry.Distinct();
            }

            return selectQry;
        }
    }
}