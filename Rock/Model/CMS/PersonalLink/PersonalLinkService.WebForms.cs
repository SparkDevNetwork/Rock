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

using System;
using System.Linq;
using System.Web;
using Rock.Data;

namespace Rock.Model
{
    public partial class PersonalLinkService
    {
        /// <summary>
        /// Class PersonalLinksHelper.
        /// </summary>
        internal static class PersonalLinksHelper
        {
            /// <summary>
            /// Class SessionKey.
            /// </summary>
            private static class SessionKey
            {
                /// <summary>
                /// The personal links last update date time
                /// </summary>
                public const string PersonalLinksLastUpdateDateTime = "PersonalLinksLastUpdateDateTime";
            }

            /// <summary>
            /// Flushes current login's <see cref="System.Web.SessionState">Session</see> LastModifiedDateTime of <see cref="Rock.Model.PersonalLink" /> data.
            /// We store PersonalLinksLastUpdateDateTime in session so that we know if the LocalStorage of PersonalLinks needs to be updated.
            /// </summary>
            /// <remarks>
            /// We store PersonalLinksLastUpdateDateTime in session so that we know if the LocalStorage of PersonalLinks needs to be updated.
            /// </remarks>
            public static void FlushPersonalLinksSessionDataLastModifiedDateTime()
            {
                if ( HttpContext.Current?.Session == null )
                {
                    return;
                }

                HttpContext.Current.Session[SessionKey.PersonalLinksLastUpdateDateTime] = null;
            }

            /// <summary>
            /// Gets current login's LastModifiedDateTime <see cref="Rock.Model.PersonalLink" /> data from  <see cref="System.Web.SessionState">Session</see>.
            /// </summary>
            /// <remarks>
            /// We cache PersonalLinksLastUpdateDateTime in session so that we know if the LocalStorage of PersonalLinks needs to be updated.
            /// </remarks>
            public static DateTime? GetPersonalLinksLastModifiedDateTime( Person currentPerson )
            {
                if ( currentPerson == null )
                {
                    return null;
                }

                DateTime? lastModifiedDateTime = null;

                if ( HttpContext.Current?.Session != null )
                {
                    // If we already got the LastModifiedDateTime, we'll store it in session so that we don't have to keep asking
                    lastModifiedDateTime = HttpContext.Current.Session[SessionKey.PersonalLinksLastUpdateDateTime] as DateTime?;
                }
                else
                {
                    // If we dont' have a Session, see if we are storing it for the current request;
                    lastModifiedDateTime = HttpContext.Current?.Items[SessionKey.PersonalLinksLastUpdateDateTime] as DateTime?;
                }

                if ( lastModifiedDateTime.HasValue )
                {
                    return lastModifiedDateTime;
                }

                // NOTE: Session can be null (probably because it is a REST call). Or maybe it hasn't been set in Session yet So, we'll get it from the Database;
                var rockContext = new RockContext();

                var personAliasQuery = new PersonAliasService( rockContext ).Queryable().Where( a => a.PersonId == currentPerson.Id );

                var sectionLastModifiedDateTimeQuery = new PersonalLinkSectionService( rockContext )
                    .Queryable()
                    .Where( a => !a.IsShared && a.ModifiedDateTime.HasValue && a.PersonAliasId.HasValue && personAliasQuery.Any( x => x.Id == a.PersonAliasId ) )
                    .Select( a => a.ModifiedDateTime );

                var linkLastModifiedDateTimeQuery = new PersonalLinkService( rockContext )
                    .Queryable()
                    .Where( a => a.ModifiedDateTime.HasValue && a.PersonAliasId.HasValue && personAliasQuery.Any( x => x.Id == a.PersonAliasId ) )
                    .Select( a => a.ModifiedDateTime );

                var linkOrderLastModifiedDateTimeQuery = new PersonalLinkSectionOrderService( rockContext )
                    .Queryable()
                    .Where( a => a.ModifiedDateTime.HasValue && personAliasQuery.Any( x => x.Id == a.PersonAliasId ) )
                    .Select( a => a.ModifiedDateTime );

                lastModifiedDateTime = sectionLastModifiedDateTimeQuery.Union( linkLastModifiedDateTimeQuery ).Union( linkOrderLastModifiedDateTimeQuery ).Max( a => a );

                if ( HttpContext.Current?.Session != null )
                {
                    if ( lastModifiedDateTime == null )
                    {
                        // If LastModifiedDateTime is still null, they don't have any personal links,
                        // So we'll set LastModifiedDateTime to Midnight (so it doesn't keep checking the database)
                        lastModifiedDateTime = RockDateTime.Today;
                    }

                    // If we already got the LastModifiedDateTime, we'll store it in session so that we don't have to keep asking
                    HttpContext.Current.Session[SessionKey.PersonalLinksLastUpdateDateTime] = lastModifiedDateTime;
                }
                else
                {
                    // if we don't have Session, store it in the current request (just in case this called multiple times in the same request)
                    if ( HttpContext.Current != null )
                    {
                        HttpContext.Current.Items[SessionKey.PersonalLinksLastUpdateDateTime] = lastModifiedDateTime;
                    }
                }

                return lastModifiedDateTime;
            }
        }
    }
}
