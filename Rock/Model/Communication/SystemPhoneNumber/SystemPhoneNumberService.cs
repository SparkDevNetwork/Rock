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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Rock.Attribute;
using Rock.Data;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class SystemPhoneNumberService
    {
        /// <summary>
        /// The lock object used when updating the legacy phone numbers in
        /// the Defined Value table.
        /// </summary>
        private static readonly object _legacyUpdateLock = new object();

        /// <summary>
        /// Gets the authorized phone numbers that match the query expression.
        /// </summary>
        /// <param name="currentPerson">The current person to use when checking authorization.</param>
        /// <returns>An enumeration of <see cref="SystemPhoneNumber"/> objects that can be viewed by <paramref name="currentPerson"/>.</returns>
        internal IEnumerable<SystemPhoneNumber> GetAuthorizedPhoneNumbers( Person currentPerson )
        {
            return GetAuthorizedPhoneNumbers( currentPerson, null );
        }

        /// <summary>
        /// Gets the authorized phone numbers that match the query expression.
        /// </summary>
        /// <param name="currentPerson">The current person to use when checking authorization.</param>
        /// <param name="queryExpression">The query expression to use when filtering phone numbers.</param>
        /// <returns>An enumeration of <see cref="SystemPhoneNumber"/> objects that match the query and can be viewed by <paramref name="currentPerson"/>.</returns>
        internal IEnumerable<SystemPhoneNumber> GetAuthorizedPhoneNumbers( Person currentPerson, Expression<Func<SystemPhoneNumber, bool>> queryExpression )
        {
            var qry = Queryable();

            if ( queryExpression != null )
            {
                qry = qry.Where( queryExpression );
            }

            return qry
                .ToList()
                .Where( pn => pn.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                .ToList();
        }
    }
}
